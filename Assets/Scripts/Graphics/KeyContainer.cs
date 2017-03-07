using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using Leap.Unity.Interaction;

/** 
 * @class KeyContainer
 * @brief A container that loads each key object and manages them.
*/
public class KeyContainer : MonoBehaviour {

    /*************************************************************************//** 
    * @defgroup KeyContainConst Constants
    * @ingroup DocKeyContain
    * These are constants used for creating the keys.
    * @{
    ****************************************************************************/
    private const string BLACK_KEY_PATH = "Graphics/Prefabs/BlackKeyPrefab"; //!< The path to load the prefab for the black keys.
    private const string LESSON_MARKER_PATH = "Audio/Prefabs/LessonMarkerPrefab"; //!< The path to load a lesson marker.
    private const string VIM_PATH = "Audio/Prefabs/VirtualInstrumentManagerPrefab"; //!< The path to load the @link VIM Virtual Instrument Manager@endlink.
    private const string WHITE_KEY_PATH = "Graphics/Prefabs/WhiteKeyPrefab"; //!< The path to load the prefab for the white keys.

    // TODO: Fill this out.
    //private const string[] TESTING_KEYS = [ "KEY_1", "KEY_2", ... ]

    /*************************************************************************//** 
    * @}
    * @defgroup KeyContainPubVar Variables
    * @ingroup DocKeyContain
    * These are variables other classes/the inspector can use to interact with the key container.
    * @{
    ****************************************************************************/
    public bool EnableAudio = false;
    public Leap.Unity.Interaction.InteractionManager mInteractionManager;
    public float KeyboardCenterX = 0f;
    public float KeyboardCenterY = 10f;
    public float KeyboardCenterZ = 0f;
    public float KeyPadding = 0.05f;
    public int NumberOfKeysToLoad = 25;
	public int keyAngle = -12;
    public Music.PITCH LowestPitchToLoad = Music.PITCH.C3;

    /*************************************************************************//** 
    * @}
    * @defgroup KeyContainPrivVar Private Variables
    * @ingroup DocKeyContain
    * These are variables used to manage the keys.
    * @{
    ****************************************************************************/
    private GameObject[] mKeyObjects = null; //!< The keys held in the container. 
    private GameObject[] mLessonMarkers = null; //!< Lesson markers for showing which key to hit. Just testing this for now to see if I can get the audio timing right.
    private int mNumLoadedKeys = 0; //!< The number of currently loaded keys.
    private int mNumWhiteKeys = 0; //!< The number of white keys being shown. Used for positioning.
    private Music.PITCH[] mRepresentedPitches = null; //!< The pitches that are represented by the keyboard.
    private PianoKey[] mKeys = null; //!< The keys that are loaded.
    private Vector3 mBlackKeySize; //!< The size of a black key.
    private Vector3 mKeyboardPosition_W; //!< The position of the center of the keyboard relative to the world.
    private Vector3 mWhiteKeySize; //!< The size of a white key.
    private VirtualInstrumentManager mVIM = null; //!< The bridge to the audio code.

    /*************************************************************************//** 
    * @}
    * @defgroup KeyContainUnity Unity Functions
    * @ingroup DocKeyContain
    * Built-in Unity functions that are automatically called.
    * @{
    *****************************************************************************/
    
    /**
     * @brief Initializes the KeyContainer and @link KeyContainer::LoadKeys begins loading each key@endlink.
    */
    void Awake()
    {
        // Destroy the placement marker.
        GameObject placementMarker = transform.GetChild( 1 ).gameObject;
        DestroyImmediate( placementMarker, false );

        // Initialize the represented pitches. Default is from C3 to C5.
        mRepresentedPitches = new Music.PITCH[NumberOfKeysToLoad];
        for( int i = 0; i < NumberOfKeysToLoad; i++ )
        {
            mRepresentedPitches[i] = (Music.PITCH)( (int)LowestPitchToLoad + i );
            if( !Music.IsPitchABlackKey( mRepresentedPitches[i] ) )
            {
                mNumWhiteKeys++;
            }
        }

        // Load a black key and a white key to find their dimensions.
        GameObject whiteKey = Resources.Load<GameObject>( WHITE_KEY_PATH );
        Assert.IsNotNull( whiteKey, "Could not load the white key prefab!" );
        GameObject blackKey = Resources.Load<GameObject>( BLACK_KEY_PATH );
        Assert.IsNotNull( blackKey, "Could not load the black key prefab!" );

        // Get the dimensions of each key.
        mWhiteKeySize = whiteKey.GetComponent<Renderer>().bounds.size;
        mBlackKeySize = blackKey.GetComponent<Renderer>().bounds.size;

        // Get the position of the keyboard.
        mKeyboardPosition_W = new Vector3( KeyboardCenterX, KeyboardCenterY, KeyboardCenterZ );
        transform.position = mKeyboardPosition_W;

        // Load the keys.
        LoadKeys();      
	}
	
	// Update is called once per frame
	void Update()
    {
        // See if we need to load the audio.
        if( mVIM == null && EnableAudio )
        {
            // Load the prefab
            GameObject temp = Instantiate( Resources.Load<GameObject>( VIM_PATH ) );
            Assert.IsNotNull( temp, "Failed to load VirtualInstrumentManagerPrefab!" );

            // Get the VIM from the prefab.
            temp.name = "VirtualInstrumentManager";
            mVIM = temp.GetComponent<VirtualInstrumentManager>();
            Assert.IsNotNull( mVIM, "Failed to get the VirtualInstrumentManager component!" );

            // Add the listener for note range changes.
            mVIM.ChangeNoteRange.AddListener( HandleChangeNoteRangeEvent );
            //mVIM.PlaySong.AddListener( HandlePlaySongEvent );

            #if DEBUG_MUSICAL_TYPING
                // Disable Musical Typing
                mVIM.GetMusicalTypingHandler().MusicalTypingEnabled = false;
            #endif
        }
        // See if we need to unload the audio.
        else if( !EnableAudio && mVIM != null )
        {
            DestroyImmediate( mVIM.gameObject, false );
            mVIM = null;
        }
    }

    /**
     * @brief Gets a key in the key container.
     * @param[in] aPitch The pitch of the key to get
     * @return The key associated with the given pitch.
    */
    public PianoKey GetKey( Music.PITCH aPitch )
    {
        int keyIndex = (int)aPitch - (int)mRepresentedPitches[0];
        return mKeys[keyIndex]; 
    }

    /**
     * @brief Gets all of the keys in the key container.
     * @return An array of all of the PianoKey objects managed by the key container.
    */
    public PianoKey[] GetAllKeys()
    {
        return mKeys;
    }

    /*************************************************************************//** 
    * @}
    * @defgroup KeyContainPrivFunc Private Functions
    * @ingroup DocKeyContain
    * Functions used internally by the KeyContainer
    * @{
    *****************************************************************************/

    /**
     * @brief Clears the keyboard
    */
    private void ClearKeyboard()
    {
        Assert.IsNotNull( mKeyObjects, "Tried to clear the keyboard, but it was already clear!" );

        // Destroy the key objects.
        for( int i = 0; i < mNumLoadedKeys; i++ )
        {
            DestroyImmediate( mKeyObjects[i], false );
        }

        // Set the array to null.
        mKeyObjects = null;

        // Clean up.
        GC.Collect();
        Resources.UnloadUnusedAssets();
    }

    /**
     * @brief Loads the keys in the container.
     * 
     * This function loads each key and sets their position and values.
    */
    private void LoadKeys()
    {
        // Destroy the previous keys if needed.
        if( mKeyObjects != null )
        {
           ClearKeyboard();
        }

        // Load the keys.
        mKeyObjects = new GameObject[mRepresentedPitches.Length];
        mLessonMarkers = new GameObject[mRepresentedPitches.Length];
        Vector3 keyPosition_loc = Vector3.zero;
        Vector3 referencePosition_w = mKeyboardPosition_W;
        Vector3 lessonMarkerPosition = Vector3.zero;
        Vector3 pressedPosition_loc = Vector3.zero;

        // Position the keys according to the number of white keys since
        // the black keys are put in the intersection between two white keys.
        int whiteKeyIndex = -1 * ( mNumWhiteKeys / 2 );

        // Loop through all of the needed keys.
        for( int i = 0; i < mNumLoadedKeys; i++ )
        {
            PianoKey key = null;
			BoxCollider keyCollider = null;
            GameObject keyObject = null;

            // Handle if the key is a black key.
            if( Music.IsPitchABlackKey( mRepresentedPitches[i] ) )
            {
                // Create the black key object.
                keyObject = Resources.Load<GameObject>( BLACK_KEY_PATH );
                Assert.IsNotNull( keyObject, "Failed to load prefab from " + BLACK_KEY_PATH + "!" );
                mKeyObjects[i] = Instantiate( keyObject );

                // Get the offset position.
				keyCollider = mKeyObjects[i].GetComponent<BoxCollider>(); 
                keyPosition_loc = Vector3.zero;
                keyPosition_loc.x = ( mBlackKeySize.x - mWhiteKeySize.x );
                keyPosition_loc.y = ( mWhiteKeySize.y / 2f ) + ( mBlackKeySize.y / 2f );
                keyPosition_loc.z = ( ( (float)( whiteKeyIndex - 1 ) * ( mWhiteKeySize.z + KeyPadding ) ) + ( ( mWhiteKeySize.z + KeyPadding ) / 2f ) );

                // Get the position for the hinge (rotation point).
                referencePosition_w = mKeyboardPosition_W;
				referencePosition_w.x -= ( mBlackKeySize.x / 2f );
                referencePosition_w.y += ( ( mWhiteKeySize.y / 2f ) + mBlackKeySize.y );
                referencePosition_w.z += keyPosition_loc.z;

                // Get the position for when the key is pressed.
                pressedPosition_loc = keyPosition_loc;
                pressedPosition_loc.x += ( mBlackKeySize.x * ( Mathf.Cos( keyAngle * Mathf.Deg2Rad ) ) ) - mBlackKeySize.x;
                pressedPosition_loc.y += mBlackKeySize.y * ( Mathf.Sin( keyAngle * Mathf.Deg2Rad ) );

                // Get the position of the lesson marker for this key.
                lessonMarkerPosition = keyPosition_loc;
                lessonMarkerPosition.y += ( ( mBlackKeySize.y / 2f ) + .001f );
                lessonMarkerPosition.x += ( mBlackKeySize.x / 5f );
            }

            // Handle if the key is a white key.
            else
            {
                // Create the white key object.
                keyObject = Resources.Load<GameObject>( WHITE_KEY_PATH );
                Assert.IsNotNull( keyObject, "Failed to load prefab from " + WHITE_KEY_PATH + "!" );
                mKeyObjects[i] = Instantiate( keyObject );

                // Get the offset position.
                keyPosition_loc = Vector3.zero;
                keyPosition_loc.y = ( mWhiteKeySize.y / 2f );
                keyPosition_loc.z = ( (float)whiteKeyIndex * ( mWhiteKeySize.z + KeyPadding ) );

                // Get the position for the hinge (rotation point).
                referencePosition_w = mKeyboardPosition_W;
				referencePosition_w.x -= ( mWhiteKeySize.x / 2f );
                referencePosition_w.y += mWhiteKeySize.y;
                referencePosition_w.z += keyPosition_loc.z;

                // Get the position for when the key is pressed.
                pressedPosition_loc = keyPosition_loc;
                pressedPosition_loc.x += ( mWhiteKeySize.x * ( Mathf.Cos( keyAngle * Mathf.Deg2Rad ) ) ) - mWhiteKeySize.x;
                pressedPosition_loc.y += mWhiteKeySize.y * ( Mathf.Sin( keyAngle * Mathf.Deg2Rad ) );

                // Update the index of which white key we're on.
                whiteKeyIndex++;
            }

            // Get the key's script.
            key = mKeyObjects[i].GetComponent<PianoKey>();

            // Set the key's position.
            key.SetParentContainer( this );
            mKeyObjects[i].transform.SetParent( transform, false );
            mKeyObjects[i].transform.localPosition = keyPosition_loc;

            // Set its pitch.
            key.Pitch = mRepresentedPitches[i];

            // Add listeners to its events.
            key.PianoKeyPressed.AddListener( HandleKeyPressed );
            key.PianoKeyReleased.AddListener( HandlePianoKeyReleased );

            // Set the object's name.
            mKeyObjects[i].name = Music.NoteToString( mRepresentedPitches[i] ) + "Key";

            // Make sure that it knows its hinge position
            key.SetReferencePosition( referencePosition_w );
			key.SetPressedPosition( pressedPosition_loc );

            //Debug.Log( "Position of key representing " + Music.NoteToString( mRepresentedPitches[i] ) + ":" + mKeyObjects[i].transform.position.ToString() );
        }
    }

    /*************************************************************************//** 
    * @}
    * @defgroup KeyContainHandlers Event Handlers
    * @ingroup DocKeyContain
    * Functions used internally by the KeyContainer to handle events.
    * @{
    *****************************************************************************/

    /**
     * @brief Allows for other objects to toggle the audio.
     * @param[in] aAudioEnabled Whether or not the audio should be enabled.
    */
    public void HandleAudioToggle( bool aAudioEnabled )
    {
        EnableAudio = aAudioEnabled;
    }

    /** 
     * @brief Handles @link VirtualInstrumentManager::ChangeNoteRangeEvent a change in the note range@endlink.
     * @param[in] aNewLowestPitch The lowest pitch of the new range.
     * 
     * Sets the @link KeyContainer::mRepresentedPitches represented pitches@endlink and loads a new part of the keyboard for a changed note range.
     * @sa VirtualInstrumentManager::ChangeNoteRangeEvent Music::PITCH
    */
    private void HandleChangeNoteRangeEvent( Music.PITCH aNewLowestPitch )
    {
        // Clear the keyboard.
        ClearKeyboard();
        mNumWhiteKeys = 0;

        // Update the represented pitches.
        for( int i = 0; i < NumberOfKeysToLoad; i++ )
        {
            mRepresentedPitches[i] = (Music.PITCH)( i + (int)aNewLowestPitch );
            if( !Music.IsPitchABlackKey( mRepresentedPitches[i] ) )
            {
                mNumWhiteKeys++;
            }
        }

        // Load the keys.
        LoadKeys();
    }

    /**
    * @brief Handles a key in the container being pressed.
    * @param[in] aPianoKey The white key that was pressed.
    */
    private void HandleKeyPressed( PianoKey aPianoKey, int aNoteVelocity )
    {
        if( EnableAudio && mVIM != null )
        {
            #if DEBUG_AUDIO_DIAGNOSTICS
                mVIM.GetDiagnosticsHandler().SetInputTime.Invoke();
            #endif

            mVIM.PlayNote.Invoke( aPianoKey.Pitch, aNoteVelocity );
        }
    }

    /**
    * @brief Handles a white key in the container being released.
    * @param[in] aWhiteKey The white key that was released.
    */
    private void HandlePianoKeyReleased( PianoKey aPianoKey )
    {
        if( EnableAudio && mVIM != null )
        {
            mVIM.ReleaseNote.Invoke( aPianoKey.Pitch );
        }
    }

    /**
    * @brief Handler for when a song begins playing.
    * @param[in] aSong The song that began playing.
    */
/*    private void HandlePlaySongEvent( Song aSong )
    {
        // Keep track of the current key layout.
        Music.PITCH[] currentLayout = mRepresentedPitches;

        // Get the highest note in the song.
        Music.PITCH highestPitch = aSong.GetHighestPitch();

        // Get the lowest note in the song.
        Music.PITCH lowestPitch = aSong.GetLowestPitch();

        // Get the song data.
        Song.CombinedNoteData[] noteData = aSong.GetNoteData();

        // Clear the keyboard.
        ClearKeyboard();

        // Set the range of keys.
        mRepresentedPitches = null;
        int numPitches = (int)highestPitch - (int)lowestPitch + 1;
        mRepresentedPitches = new Music.PITCH[numPitches];
        mNumWhiteKeys = 0;
        for( int i = 0; i < numPitches; i++ )
        {
            mRepresentedPitches[i] = (Music.PITCH)( (int)lowestPitch + i );
            if( !Music.IsPitchABlackKey( mRepresentedPitches[i] ) )
            {
                mNumWhiteKeys++;
            }
        }

        // Load the proper keys.
        LoadKeys();

        // Get the sample interval (seconds between waveform samples.)
        float sampInt = VirtualInstrument.SAMPLE_INTERVAL;

        // Draw the lesson markers for each pitch.
        foreach( Song.CombinedNoteData note in noteData )
        {
            if( note.MelodyData.Pitches != null )
            {
                int index = 0;
                foreach( Music.PITCH pitch in note.MelodyData.Pitches )
                {
                    StartCoroutine( DrawLessonMarker( (int)pitch - (int)lowestPitch, (float)note.TotalOffset * sampInt, ( note.MelodyData.NumSamples[index] * sampInt ) - .02f ) );
                    index++;
                }
            }
        }

    }*/

    
    /**
     * @brief Handles drawing the lesson markers.
     * @param[in] aIndex The index of the key the draw the marker on.
     * @param[in] aDelay The delay of when to draw the marker.
     * @param[in] aStop How long the marker should be drawn.
    */
    /*
    private IEnumerator DrawLessonMarker( int aIndex, float aDelay, float aStop )
    {
        yield return new WaitForSeconds( aDelay );
        mLessonMarkers[aIndex].SetActive( true );
        yield return new WaitForSeconds( aStop );
        mLessonMarkers[aIndex].SetActive( false );

        yield return null;
    }*/

    /** @} */
}
