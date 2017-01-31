using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

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
    private const int NUM_KEYS = 25; //!< The default number of keys shown.
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

    /*************************************************************************//** 
    * @}
    * @defgroup KeyContainPrivVar Private Variables
    * @ingroup DocKeyContain
    * These are variables used to manage the keys.
    * @{
    ****************************************************************************/
    private float mBlackKeyWidth = 0f; //!< The width of a black key.
    private float mWhiteKeyWidth = 0f; //!< The width of a white key.
    private GameObject[] mKeyObjects = null; //!< The keys held in the container. Need to update this once the classes are combined.
    private GameObject[] mLessonMarkers = null; //!< Lesson markers for showing which key to hit. Just testing this for now to see if I can get the audio timing right.
    private int mNumWhiteKeys = 0; //!< The number of white keys being shown. Used for positioning.
    private Music.PITCH[] mRepresentedPitches = null; //!< The pitches that are represented by the keyboard.
    private VirtualInstrumentManager mVIM = null; //!< The bridge to the audio code.

    /*************************************************************************//** 
    * @}
    * @defgroup KeyContainUnity Unity Functions
    * @ingroup DocKeyContain
    * Built-in Unity functions that are automatically called.
    * @{
    *****************************************************************************/
    
    /**
     * @brief Initializes the KeyContainer and loads each key.
    */
    void Awake()
    {
        // Initialize the represented pitches. Default is from C3 to C5.
        mRepresentedPitches = new Music.PITCH[NUM_KEYS];
        for( int i = 0; i < NUM_KEYS; i++ )
        {
            mRepresentedPitches[i] = (Music.PITCH)( (int)Music.PITCH.C3 + i );
            if( !Music.IsPitchABlackKey( mRepresentedPitches[i] ) )
            {
                mNumWhiteKeys++;
            }
        }

        // Get the width of each key.
        mWhiteKeyWidth = Resources.Load<GameObject>( WHITE_KEY_PATH ).GetComponent<Renderer>().bounds.size.z;
        mWhiteKeyWidth += ( mWhiteKeyWidth / 10f );
        mBlackKeyWidth = Resources.Load<GameObject>( BLACK_KEY_PATH ).GetComponent<Renderer>().bounds.size.z;
        Debug.Log( "White key width: " + mWhiteKeyWidth.ToString() );
        Debug.Log( "Black key width: " + mBlackKeyWidth.ToString() );

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
            mVIM.PlaySong.AddListener( HandlePlaySongEvent );

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
        for( int i = 0; i < mRepresentedPitches.Length; i++ )
        {
            DestroyImmediate( mKeyObjects[i], false );
        }

        // Set the array to null.
        mKeyObjects = null;

        // Destroy the key objects.
        for( int i = 0; i < mRepresentedPitches.Length; i++ )
        {
            DestroyImmediate( mLessonMarkers[i], false );
        }
        mLessonMarkers = null;

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
           // ClearKeyboard();
        }

        // Load the keys.
        mKeyObjects = new GameObject[mRepresentedPitches.Length];
        mLessonMarkers = new GameObject[mRepresentedPitches.Length];
        Vector3 keyPosition = Vector3.zero;
        int whiteKeyIndex = -1 * ( mNumWhiteKeys / 2 );
        for( int i = 0; i < mRepresentedPitches.Length; i++ )
        {

            // Handle if the key is a black key.
            if( Music.IsPitchABlackKey( mRepresentedPitches[i] ) )
            {
                // Make an empty black key to help initialize this one.
                BlackKey key = null;

                // Create the black key object.
                mKeyObjects[i] = Instantiate( Resources.Load<GameObject>( BLACK_KEY_PATH ) );
                Assert.IsNotNull( mKeyObjects[i], "Failed to load prefab from " + BLACK_KEY_PATH + "!" );
                mKeyObjects[i].name = Music.NoteToString( mRepresentedPitches[i] ) + "Key";

                // Get the black key.
                key = mKeyObjects[i].GetComponent<BlackKey>();
                Assert.IsNotNull( key, "Could not get a reference to the BlackKey script!" );

                // Set its pitch.
                key.Pitch = mRepresentedPitches[i];

                // Add listeners to its events.
                key.BlackKeyPressed.AddListener( HandleBlackKeyPressed );
                key.BlackKeyReleased.AddListener( HandleBlackKeyReleased );

                // Set its position, which is determined by its joint. First, get the spring joint.
                SpringJoint spring = mKeyObjects[i].GetComponent<SpringJoint>();
                Assert.IsNotNull( spring, "Could not get spring joint of black key!" );

                // Get the offset position.
                keyPosition = Vector3.zero;
                keyPosition.z += ( ( (float)( whiteKeyIndex - 1 ) * mWhiteKeyWidth ) + ( mWhiteKeyWidth / 2f ) );

                // Update the joint's connected anchor.
                spring.autoConfigureConnectedAnchor = false;
                spring.connectedAnchor += keyPosition;

                // Set the new position.
                keyPosition.y = mKeyObjects[i].transform.position.y;
                keyPosition.x = mKeyObjects[i].transform.position.x;
                mKeyObjects[i].transform.position = keyPosition;

                // Put settings back on the spring joint.
                spring.autoConfigureConnectedAnchor = true;

                // Set its place in the hierarchy.
                mKeyObjects[i].transform.SetParent( transform, true );

                Debug.Log( "Position of key representing " + Music.NoteToString( mRepresentedPitches[i] ) + ":" + keyPosition.ToString() );

                // Set its parent.
                key.SetParentContainer( this );
            }

            // Handle if the key is a white key.
            else
            {
                // Make an empty black key to help initialize this one.
                WhiteKey key = null;

                // Create the black key object.
                mKeyObjects[i] = Instantiate( Resources.Load<GameObject>( WHITE_KEY_PATH ) );
                Assert.IsNotNull( mKeyObjects[i], "Failed to load prefab from " + WHITE_KEY_PATH + "!" );
                mKeyObjects[i].name = Music.NoteToString( mRepresentedPitches[i] ) + "Key";

                // Get the black key.
                key = mKeyObjects[i].GetComponent<WhiteKey>();
                Assert.IsNotNull( key, "Could not get a reference to the BlackKey script!" );

                // Set its pitch.
                key.Pitch = mRepresentedPitches[i];

                // Add listeners to its events.
                key.WhiteKeyPressed.AddListener( HandleWhiteKeyPressed );
                key.WhiteKeyReleased.AddListener( HandleWhiteKeyReleased );

                // Get the hinge joint.
                HingeJoint hinge = mKeyObjects[i].GetComponent<HingeJoint>();
                hinge.autoConfigureConnectedAnchor = false;

                // Get the offset position.
                keyPosition = Vector3.zero;
                keyPosition.z += ( (float)whiteKeyIndex * mWhiteKeyWidth );

                // Set the hinge's position.
                hinge.anchor += keyPosition;
                hinge.connectedAnchor += keyPosition;

                // Set the key's position.
                mKeyObjects[i].transform.position += keyPosition;
                hinge.autoConfigureConnectedAnchor = true;

                // Update the index of which white key we're on.
                whiteKeyIndex++;

                // Set its place in the hierarchy.
                mKeyObjects[i].transform.SetParent( transform, true ); 

                Debug.Log( "Position of key representing " + Music.NoteToString( mRepresentedPitches[i] ) + ":" + mKeyObjects[i].transform.position.ToString() );

                // Set its parent.
                key.SetParentContainer( this );
            }

            // Load the lesson marker.
            mLessonMarkers[i] = Instantiate( Resources.Load<GameObject>( LESSON_MARKER_PATH ) );
            Assert.IsNotNull( mLessonMarkers[i], "Could not load the lesson marker!" );

            // Set the lesson marker position.
            mLessonMarkers[i].transform.SetParent( transform, true );
            keyPosition.y += ( mKeyObjects[i].GetComponent<Renderer>().bounds.size.y / 2f ) + 0.001f;
            keyPosition.x += ( mKeyObjects[i].GetComponent<Renderer>().bounds.size.x / 5f );
            mLessonMarkers[i].transform.position = keyPosition;
            mLessonMarkers[i].SetActive( false );

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
    * @brief Handles a black key in the container being pressed.
    * @param[in] aBlackKey The black key that was pressed.
    */
    private void HandleBlackKeyPressed( BlackKey aBlackKey )
    {
        if( EnableAudio && mVIM != null )
        {
            #if DEBUG_AUDIO_DIAGNOSTICS
                mVIM.GetDiagnosticsHandler().SetInputTime.Invoke();
            #endif

            mVIM.PlayNote.Invoke( aBlackKey.Pitch, 100 );
        }
    }

    /**
    * @brief Handles a black key in the container being released.
    * @param[in] aBlackKey The black key that was released.
    */
    private void HandleBlackKeyReleased( BlackKey aBlackKey )
    {
        if( EnableAudio && mVIM != null )
        {
            mVIM.ReleaseNote.Invoke( aBlackKey.Pitch );
        }
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
        for( int i = 0; i < NUM_KEYS; i++ )
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
    * @brief Handles a white key in the container being pressed.
    * @param[in] aWhiteKey The white key that was pressed.
    */
    private void HandleWhiteKeyPressed( WhiteKey aWhiteKey )
    {
        if( EnableAudio && mVIM != null )
        {
            #if DEBUG_AUDIO_DIAGNOSTICS
                mVIM.GetDiagnosticsHandler().SetInputTime.Invoke();
            #endif

            mVIM.PlayNote.Invoke( aWhiteKey.Pitch, 100 );
        }
    }

    /**
    * @brief Handles a white key in the container being released.
    * @param[in] aWhiteKey The white key that was released.
    */
    private void HandleWhiteKeyReleased( WhiteKey aWhiteKey )
    {
        if( EnableAudio && mVIM != null )
        {
            mVIM.ReleaseNote.Invoke( aWhiteKey.Pitch );
        }
    }

    /**
    * @brief Handler for when a song begins playing.
    * @param[in] aSong The song that began playing.
    */
    private void HandlePlaySongEvent( Song aSong )
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
                foreach( Music.PITCH pitch in note.MelodyData.Pitches )
                {
                    StartCoroutine( DrawLessonMarker( (int)pitch - (int)lowestPitch, (float)note.TotalOffset * sampInt, ( note.MelodyData.NumSamples * sampInt ) - .02f ) );
                }
            }
        }

    }

    /**
     * @brief Handles drawing the lesson markers.
     * @param[in] aIndex The index of the key the draw the marker on.
     * @param[in] aDelay The delay of when to draw the marker.
     * @param[in] aStop How long the marker should be drawn.
    */
    private IEnumerator DrawLessonMarker( int aIndex, float aDelay, float aStop )
    {
        yield return new WaitForSeconds( aDelay );
        mLessonMarkers[aIndex].SetActive( true );
        yield return new WaitForSeconds( aStop );
        mLessonMarkers[aIndex].SetActive( false );

        yield return null;
    }

    /** @} */
}
