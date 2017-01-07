//---------------------------------------------------------------------------- 
// /Resources/Music/VirtualInstrument/VirtualInstrumentManager.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: GameObject that manages a virtual instrument and its output.
//     It's sort of the common bridge between all of the 
//     virtual instrument-related classes and objects.
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;
using UnityEngine.Assertions;

public class VirtualInstrumentManager : MonoBehaviour {

    //---------------------------------------------------------------------------- 
    // Constants
    //---------------------------------------------------------------------------- 
    private static Music.PITCH DEFAULT_LOWEST_PITCH = Music.PITCH.C4;
    private static Music.PITCH DEFAULT_HIGHEST_PITCH = Music.PITCH.B5;
    private static Music.INSTRUMENT_TYPE DEFAULT_INSTRUMENT_TYPE = Music.INSTRUMENT_TYPE.PIANO;

    //---------------------------------------------------------------------------- 
    // Types
    //---------------------------------------------------------------------------- 

    // A type of event that is invoked whenever a note should be played. Functions
    // that invoke this type of event will need to provide parameters that detail
    // the note to play and the velocity at which to play it.
    public class NoteStartEvent : UnityEvent<Music.PITCH, int>
    {
    }

    // A type of event that is invoked whenever a note should fade out as though 
    // the key was released. Functions that invoke this type of event will need to 
    // provide which note to fade out as a parameter.
    public class NoteReleaseEvent : UnityEvent<Music.PITCH>
    {
    }

    // A type of event that is invoked whenever a note should fade out as though 
    // the key was released. Functions that invoke this type of event will need to 
    // provide which note to fade out as a parameter.
    public class ChangeNoteRangeEvent : UnityEvent<Music.PITCH>
    {
    }

    // A type of event that's invoked when the instrument should change. Tha
    // parameter is the type of instrument to change to.
    public class ChangeInstrumentEvent : UnityEvent<Music.INSTRUMENT_TYPE>
    {
    }

    // A type of event that's invoked in order to modify the echo filter.
    // The parameter is a struct of EchoFilterParameters.
    public class ModifyEchoFilterEvent : UnityEvent<EchoFilterParameters>
    {
    }

    // A type of event that's invoked in order to play a song.
    // The parameter is the a queue of notes to play.
    public class PlaySongEvent : UnityEvent<Queue<Music.Note>>
    {
    }

    // A struct that contains the parameters for the echo filter. 
    public struct EchoFilterParameters
    {
        public bool Active;
        public float Decay;
        public float Delay;
        public float DryMix;
        public float WetMix;
    }

    //---------------------------------------------------------------------------- 
    // Public Variables
    //---------------------------------------------------------------------------- 
    public NoteStartEvent              NotePlay; // The event that will be invoked whenever a note should be played.
    public NoteReleaseEvent            NoteRelease; // The event that will be invoked whenever a note should fade out.
    public ChangeNoteRangeEvent        ChangeNoteRange; // The event that will be invoked whenever the note range should change.
    public ChangeInstrumentEvent       ChangeInstrument; // The event that will be invoked whenever the instrument should be changed.
    public ModifyEchoFilterEvent       ModifyEchoFilter; // The event that will be invoked when the echo filter needs to be modified.
    public PlaySongEvent               PlaySong; // The event that will be invoked when a song should be played.

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private AudioMixer                 mMixer; // The audio mixer that all audio output will be routed through.
    private bool                       mReady; // Whether or not the manager is ready to play notes.
    private int                        mNumActiveNotes; // The number of currently active notes.
    private Music.INSTRUMENT_TYPE      mInstrumentType; // The type of instrument that is currently loaded.
    private Music.PITCH                mLowestActiveNote; // The lowest currently active note.
    private Music.PITCH                mHighestActiveNote; // The highest currently active note.
    private Music.PITCH[]              mActiveNotes; // An array that holds all of the currently active notes.
    private NoteOutputObject           mSongOutput; // A NoteOutputObject, but the note is actually a song.
    private NoteOutputObject[]         mOutputs; // An array that holds the NoteOutputObjects that actually handle sound output.
    private VirtualInstrument          mInstrument; // The loaded virtual instrument that this object will manage.


    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Sets initial values when the object is first loaded.
    public void Awake()
    {
        mReady = false;

        #if DEBUG && DEBUG_MUSICAL_TYPING
            DEBUG_SetMusicalTypingVariables();
        #endif

        // Set up the events.
        SetUpEvents();

        // Set default values for the member variables.
        SetDefaultValues();

        // Begin loading the default virtual instrument which is a piano.
        StartCoroutine( LoadInstrument( mInstrumentType, ( returned ) => { mInstrument = returned; } ) ); 
    }

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Checks if the parameters for the echo filter are valid. Delay ranges from 10 to 5000,
    // and Decay|Dry Mix|Wet Mix range from 0 to 1.
    // IN: aParams The parameters for the echo filter.
    // OUT: true if the value is valid. false otherwise.
    public bool CheckEchoFilterParameters( EchoFilterParameters aParams )
    {
        if( ( aParams.Decay < 0f || aParams.Decay > 1f ) ||
            ( aParams.Delay < 10f || aParams.Delay > 5000f ) ||
            ( aParams.DryMix < 0f || aParams.DryMix > 1f ) ||
            ( aParams.WetMix < 0f || aParams.WetMix > 1f ) )
        {
            return false;
        } 
        else
        {
            return true;
        }
    }

    // Gets the virtual instrument that is being managed.
    public VirtualInstrument GetInstrument()
    {
        return mInstrument;
    }

    //---------------------------------------------------------------------------- 
    // Private Functions
    //---------------------------------------------------------------------------- 

    private void LoadNoteOutputObjects()
    {
        Assert.IsNotNull( mInstrument, "Tried to load NoteOutputObjects when the instrument was null!" );

        if( mOutputs == null )
        {
            // Initialize the array of NoteOutputObjects.
            mOutputs = new NoteOutputObject[mNumActiveNotes];

            // In order to have multiple notes play at once, an invisible GameObject is created and cloned multiple 
            // times to serve as containers for each NoteOutputObject. The invisible object will be placed at the position
            // of the virtual instrument manager, so be sure to put the virtual instrument manager on an object that is as
            // close to the AudioListener as possible.  
            GameObject toBeCloned = new GameObject( Music.NoteToString( mLowestActiveNote ) + "NoteOutputObjectContainer" );
            toBeCloned.transform.position = gameObject.transform.position;
            mOutputs[0] = toBeCloned.AddComponent<NoteOutputObject>();
            GameObject clone = null;

            // For each note, clone the invisible object (which will also clone the NoteOutputObject).
            for( int i = 1; i < mNumActiveNotes; i++ )
            {
                clone = Instantiate( toBeCloned );
                clone.name = Music.NoteToString( mActiveNotes[i] ) + "NoteOutputObjectContainer";
                mOutputs[i] = clone.GetComponent<NoteOutputObject>();
            }
        }

        // Set the audio data of the NoteOutputObject.
        for( int i = 0; i < mNumActiveNotes; i++ )
        {
            mOutputs[i].SetAudioData( mInstrument.GetRawAudioDataForNote( mActiveNotes[i] ), mMixer, mInstrument.GetBuiltInDynamicsThresholds() );
        }

        // Set that we're ready for note events.
        mReady = true;

#if DEBUG
        Debug.Log( "NoteOutputObjects are loaded" );
#endif

    }

    // Sets up the events and adds their listeners.
    // Should only be called on Awake().
    private void SetUpEvents()
    {
        NotePlay = new NoteStartEvent();
        NoteRelease = new NoteReleaseEvent();
        ChangeNoteRange = new ChangeNoteRangeEvent();
        ChangeInstrument = new ChangeInstrumentEvent();
        ModifyEchoFilter = new ModifyEchoFilterEvent();
        PlaySong = new PlaySongEvent();
        NotePlay.AddListener( OnNotePlayEvent );
        NoteRelease.AddListener( OnNoteReleaseEvent );
        ChangeNoteRange.AddListener( OnChangeNoteRangeEvent );
        ChangeInstrument.AddListener( OnChangeInstrumentEvent );
        ModifyEchoFilter.AddListener( OnModifyEchoFilterEvent );
        PlaySong.AddListener( OnPlaySongEvent );
    }

    // Sets the default values for the member variables.
    // Should only be called on Awake().
    private void SetDefaultValues()
    {
        mMixer = Resources.Load<AudioMixer>( "Music/VirtualInstrument/VirtualInstrumentAudioMixer" );
        Assert.IsNotNull( mMixer, "Audio mixer was unable to load!" );

        mLowestActiveNote = DEFAULT_LOWEST_PITCH;
        mHighestActiveNote = DEFAULT_HIGHEST_PITCH;
        mNumActiveNotes = (int)mHighestActiveNote - (int)mLowestActiveNote + 1;
        mActiveNotes = new Music.PITCH[mNumActiveNotes];
        for( int i = 0; i < mNumActiveNotes; i++ )
        {
            mActiveNotes[i] = (Music.PITCH)( i + (int)mLowestActiveNote );
        }
        mInstrumentType = DEFAULT_INSTRUMENT_TYPE;

        GameObject songOutputContainer = new GameObject();
        mSongOutput = songOutputContainer.AddComponent<NoteOutputObject>();
    }

    //---------------------------------------------------------------------------- 
    // Coroutines
    //---------------------------------------------------------------------------- 

    // Coroutine to load a virtual instrument. 
    // IN: aINSTRUMENT_TYPE The type of instrument to load.
    // IN: aInstrumentCallback Callback to allow returning the loaded instrument to the calling context. 
    private IEnumerator LoadInstrument( Music.INSTRUMENT_TYPE aINSTRUMENT_TYPE, System.Action<VirtualInstrument> aInstrumentCallback )
    {
        // Load a new instrument
        VirtualInstrument returned = null;
        switch ( aINSTRUMENT_TYPE )
        {
            case Music.INSTRUMENT_TYPE.MARIMBA:
                returned = new Marimba( this );
                break;
            case Music.INSTRUMENT_TYPE.PIANO:
            default:
                returned = new Piano( this );
                break;
        }

        // Return the loaded instrument and invoke the instrument's LoadEvent.
        aInstrumentCallback( returned );
        returned.LoadEvent.Invoke();
        yield return null;
    }

    //---------------------------------------------------------------------------- 
    // Event Handlers
    //---------------------------------------------------------------------------- 

    // Called whenever there is a GUI event.
    void OnGUI()
    {
    #if DEBUG && DEBUG_MUSICAL_TYPING
        // If musical typing is enabled, then watch for key events and send them along to the handler.
        if ( Event.current.isKey &&
            ( Input.GetKeyDown( Event.current.keyCode ) || Input.GetKeyUp( Event.current.keyCode ) ) )
        {
            DEBUG_HandleMusicalTypingEvent( Event.current );
        }
    #endif
    }

    // Handles changing the loaded instrument. Should only be called via a ChangeInstrumentEvent being invoked.
    public void OnChangeInstrumentEvent( Music.INSTRUMENT_TYPE aNewInstrumentType )
    {
        // Mark that we're not ready for note events.
        mReady = false;

        // Set the instrument type.
        mInstrumentType = aNewInstrumentType;

        // Set default values for the active notes.
        mLowestActiveNote = DEFAULT_LOWEST_PITCH;
        mHighestActiveNote = DEFAULT_HIGHEST_PITCH;
        mNumActiveNotes = (int)mHighestActiveNote - (int)mLowestActiveNote + 1;
        mActiveNotes = new Music.PITCH[mNumActiveNotes];
        for( int i = 0; i < mNumActiveNotes; i++ )
        {
            mActiveNotes[i] = (Music.PITCH)( i + (int)mLowestActiveNote );
        }

        // Start a coroutine to load the instrument.
        StartCoroutine( LoadInstrument( mInstrumentType, ( returned ) => { mInstrument = returned; } ) );
    }

    // Handles changing the note range. Should only be called via a ChangeNoteRangeEvent being invoked.
    // IN: aNewLowestNote The lowest note of the new range.
    public void OnChangeNoteRangeEvent( Music.PITCH aNewLowestNote )
    {
        if( mReady )
        {
            mReady = false;

            // Make sure that we can change to the note range given.
            int newHighestIndex = (int)aNewLowestNote + mNumActiveNotes - 1;
            if( !mInstrument.IsNoteSupported( aNewLowestNote ) || !mInstrument.IsNoteSupported( (Music.PITCH)newHighestIndex ) )
            {
                Assert.IsTrue( false, "Tried to change the note range to include notes that aren't supported by the instrument!" );
                return;
            }

            // Set the active note variables accordingly.
            mLowestActiveNote = aNewLowestNote;
            mHighestActiveNote = (Music.PITCH)( (int)aNewLowestNote + mNumActiveNotes - 1 );
            for( int i = 0; i < mNumActiveNotes; i++ )
            {
                mActiveNotes[i] = (Music.PITCH)( (int)aNewLowestNote + i );
            }

            // Load the note output objects.
            LoadNoteOutputObjects();
        }

    }

    // Function to handle the instrument being loaded. Should only be called via an initialized virtual 
    // instrument's LoadEvent being invoked.  
    public void OnInstrumentLoaded()
    {
        Assert.IsNotNull( mInstrument, "OnInstrumentLoaded was called, but the instrument was not loaded." );

        LoadNoteOutputObjects();
    }

    // Function to modify the echo filter. Should only be called via an invoked ModifyEchoFilterEvent. 
    // IN: aParameters The parameters for the echo filter.
    public void OnModifyEchoFilterEvent( EchoFilterParameters aParameters )
    {
        // If the filter is active and the parameters are valid, then use the new parameters.
        if( aParameters.Active )
        {
            if( CheckEchoFilterParameters( aParameters ) )
            {
                mMixer.SetFloat( "echoDelay", aParameters.Delay );
                mMixer.SetFloat( "echoDecay", aParameters.Decay );
                mMixer.SetFloat( "echoDryMix", aParameters.DryMix );
                mMixer.SetFloat( "echoWetMix", aParameters.WetMix );
            }

        }
        // If the filter is not active, then set values that negate the filter.
        else
        {
            mMixer.SetFloat( "echoDelay", 1f );
            mMixer.SetFloat( "echoDecay", 0f );
            mMixer.SetFloat( "echoDryMix", 100f );
            mMixer.SetFloat( "echoWetMix", 0f );
        }

    }

    // Begins fading out the note as though the key was released. Should only be called via an invoked NoteReleaseEvent.
    // IN: aNoteToFade The note to fade out. 
    public void OnNoteReleaseEvent( Music.PITCH aNoteToRelease )
    {
        Assert.IsTrue( (int)aNoteToRelease >= (int)mLowestActiveNote && (int)aNoteToRelease <= (int)mHighestActiveNote, "Tried to fade out a note that is not active!" );
        int noteIndex = (int)aNoteToRelease - (int)mLowestActiveNote;
        mOutputs[noteIndex].BeginNoteFadeOut();
    }

    // Begin playing a note. Should only be called via an invoked NoteStartEvent.
    // IN: aNoteToPlay The note to play.
    // IN: aVelocity The velocity at which to play the note.
    public void OnNotePlayEvent( Music.PITCH aNoteToPlay, int aVelocity )
    {
        if ( mReady )
        {
            Assert.IsTrue( (int)aNoteToPlay >= (int)mLowestActiveNote && (int)aNoteToPlay <= (int)mHighestActiveNote, "Tried to play a note that is not active!" );
            int noteIndex = (int)aNoteToPlay - (int)mLowestActiveNote;
            mOutputs[noteIndex].BeginNotePlaying( aVelocity );
        }

    }

    // Begins playing a song. 
    // IN: aNoteQueue The queue of notes that make up the song.
    public void OnPlaySongEvent( Queue<Music.Note> aNoteQueue )
    {
        Music.Note[] noteArray = aNoteQueue.ToArray();
        int numSamples = noteArray[noteArray.Length - 1].Length + noteArray[noteArray.Length - 1].OffsetSamples;
        float[][] songData = new float[1][];
        songData[0] = new float[numSamples];
        int offset = 0;
        int velocity = 0;
        int velIndex = 0;
        for( int i = 0; i < noteArray.Length; i++ )
        {
            if( noteArray[i].Pitch != Music.PITCH.REST )
            {
                offset = noteArray[i].OffsetSamples;
                velocity = noteArray[i].Velocity;
                velIndex = mInstrument.GetBuiltInDynamicsThresholdIndex( velocity );
                float[][] noteData = mInstrument.GetRawAudioDataForNote( noteArray[i].Pitch );
                int j = 0;
                while( j < noteArray[i].Length && j < noteData[velIndex].Length )
                {
                    songData[0][j + offset] += noteData[velIndex][j];
                    j++;
                }
                float releaseFactor = 1f;
                while( j < noteData[velIndex].Length && j + offset < numSamples )
                {
                    songData[0][j + offset] = releaseFactor * noteData[velIndex][j];
                    releaseFactor *= .999f;
                    j++;
                }
            }
        }
        mSongOutput.SetAudioData( songData, mMixer, null );
        mSongOutput.BeginNotePlaying( 100 );
    }

#if DEBUG && DEBUG_MUSICAL_TYPING

    // Musical Typing allows for using a computer keyboard to debug sound output by 
    // triggering note events for a 19-note range whenever specific keys on the 
    // keyboard are pressed/released. The velocities for each key can be set via
    // an invoked ChangeKeyVelocityEvent.
    // The keys that are used are (in this order)
    // a, w, s, e, d, f, t, g, y, h, u, j, k, o, l, p, ;, ', and ] 

    //---------------------------------------------------------------------------- 
    // Musical Typing Constants
    //---------------------------------------------------------------------------- 

    private static int DEBUG_numMusicalTypingKeys = 19;
    private static KeyCode[] DEBUG_musicalTypingKeys =
        {
            KeyCode.A,
            KeyCode.W,
            KeyCode.S,
            KeyCode.E,
            KeyCode.D,
            KeyCode.F,
            KeyCode.T,
            KeyCode.G,
            KeyCode.Y,
            KeyCode.H,
            KeyCode.U,
            KeyCode.J,
            KeyCode.K,
            KeyCode.O,
            KeyCode.L,
            KeyCode.P,
            KeyCode.Semicolon,
            KeyCode.Quote,
            KeyCode.RightBracket
        };

    //---------------------------------------------------------------------------- 
    // Musical Typing Types
    //---------------------------------------------------------------------------- 

    // A type of event that is invoked whenever a musical typing key will generate
    // a different velocity. 
    public class DEBUG_ChangeKeyVelocityEvent : UnityEvent<int, int>
    {
    }

    //---------------------------------------------------------------------------- 
    // Musical Typing Variables
    //---------------------------------------------------------------------------- 
    private int[]                         DEBUG_keyVelocities; // The velocities that will be used whenever musical typing simulates a note event.
    public DEBUG_ChangeKeyVelocityEvent   DEBUG_ChangeKeyVelocity; // An event that will be invoked whenever the velocity for a musical typing key should change.

    //---------------------------------------------------------------------------- 
    // Musical Typing Private Functions
    //---------------------------------------------------------------------------- 
    
    // Sets default values for Musical Typing.
    private void DEBUG_SetMusicalTypingVariables()
    {
        DEBUG_ChangeKeyVelocity = new DEBUG_ChangeKeyVelocityEvent();
        DEBUG_ChangeKeyVelocity.AddListener( DEBUG_HandleKeyVelocityChange );
        DEBUG_keyVelocities = new int[DEBUG_numMusicalTypingKeys];
        for( int i = 0; i < DEBUG_numMusicalTypingKeys; i++ )
        {
            DEBUG_keyVelocities[i] = 100;
        }
    }

    //---------------------------------------------------------------------------- 
    // Musical Typing Event Handlers
    //---------------------------------------------------------------------------- 

    // Handler for changing the velocity that is used when musical typing simulates a note event.
    // Should only be called via an invoked DEBUG_ChangeKeyVelocity event.
    private void DEBUG_HandleKeyVelocityChange( int aKeyIndex, int aNewVelocity )
    {
        DEBUG_keyVelocities[aKeyIndex] = aNewVelocity;
    }

    // Handler for musical typing that maps key events to note events. 
    // IN: aKeyEvent A GUI keyboard event triggered by a key being pressed or released.
    private void DEBUG_HandleMusicalTypingEvent( Event aKeyEvent )
    {
        // Check if a musical typing key is pressed or released and fire off the 
        // corresponding NotePlay or NoteFadeOut event if so. The debug velocity is 
        // used for the events.
        bool found = false;
        int i = 0;
        while ( !found && i < DEBUG_numMusicalTypingKeys )
        {
            if ( aKeyEvent.keyCode == DEBUG_musicalTypingKeys[i] )
            {
                if ( aKeyEvent.type == EventType.KeyDown )
                {
                    NotePlay.Invoke( mActiveNotes[i], DEBUG_keyVelocities[i] );
                }
                if ( aKeyEvent.type == EventType.KeyUp )
                {
                    NoteRelease.Invoke( mActiveNotes[i] );
                }
            }
        i++;
        }
    }

#endif

}
