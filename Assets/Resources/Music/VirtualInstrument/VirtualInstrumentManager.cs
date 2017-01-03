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
using UnityEngine.Assertions;

public class VirtualInstrumentManager : MonoBehaviour {

    //---------------------------------------------------------------------------- 
    // Types
    //---------------------------------------------------------------------------- 

    // A type of event that is invoked whenever a note should be played. Functions
    // that invoke this type of event will need to provide parameters that detail
    // the note to play and the velocity at which to play it.
    public class NoteStartEvent : UnityEvent<Music.NOTE, int>
    {
    }

    // A type of event that is invoked whenever a note should fade out as though 
    // the key was released. Functions that invoke this type of event will need to 
    // provide which note to fade out as a parameter.
    public class NoteReleaseEvent : UnityEvent<Music.NOTE>
    {
    }

    //---------------------------------------------------------------------------- 
    // Public Variables
    //---------------------------------------------------------------------------- 
    public NoteStartEvent NotePlayEvent; // The event that will be invoked whenever a note should be played.
    public NoteReleaseEvent NoteFadeOutEvent;

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private bool                       mReady; // Whether or not the manager is ready to play notes.
    private int                        mNumActiveNotes; // The number of currently active notes.
    private Music.INSTRUMENT_TYPE      mInstrumentType; // The type of instrument that is currently loaded.
    private Music.NOTE                 mLowestActiveNote; // The lowest currently active note.
    private Music.NOTE                 mHighestActiveNote; // The highest currently active note.
    private Music.NOTE[]               mActiveNotes; // An array that holds all of the currently active notes.
    private NoteOutputObject[]         mOutputs; // An array that holds the NoteOutputObjects that actually handle sound output.
    private VirtualInstrument          mInstrument; // The loaded virtual instrument that this object will manage.

#if DEBUG
    //---------------------------------------------------------------------------- 
    // Debug Variables
    //---------------------------------------------------------------------------- 
    private int                     DEBUG_velocity;
#endif

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Sets initial values when the object is first loaded.
    public void Awake()
    {
        #if DEBUG && DEBUG_MUSICAL_TYPING
            DEBUG_SetDebugVariables();
        #endif

        // Set up the events.
        NotePlayEvent = new NoteStartEvent();
        NoteFadeOutEvent = new NoteReleaseEvent();
        NotePlayEvent.AddListener( OnNotePlay );
        NoteFadeOutEvent.AddListener( OnNoteFadeOut );

        // Set default values for the member variables.
        mReady = false;
        mLowestActiveNote = Music.NOTE.C4;
        mHighestActiveNote = Music.NOTE.B5;
        mNumActiveNotes = (int)mHighestActiveNote - (int)mLowestActiveNote + 1;
        mActiveNotes = new Music.NOTE[mNumActiveNotes];
        for ( int i = 0; i < mNumActiveNotes; i++ )
        {
            mActiveNotes[i] = (Music.NOTE)( i + (int)mLowestActiveNote );
        }
        mInstrumentType = Music.INSTRUMENT_TYPE.PIANO;

        // Begin loading the default virtual instrument which is a piano.
        StartCoroutine( LoadInstrument( mInstrumentType, ( returned ) => { mInstrument = returned; } ) ); 
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
        if ( Event.current.isKey )
        {
            DEBUG_HandleMusicalTyping( Event.current );
        }
    #endif
    }

    // Function to handle the instrument being loaded. Should only be called via an initialized virtual 
    // instrument's LoadEvent being invoked.  
    public void OnInstrumentLoaded()
    {
        Assert.IsNotNull( mInstrument, "OnInstrumentLoaded was called, but the instrument was not loaded." );

        // Get the sample rate 
        int sampleRate = mInstrument.GetSampleRate();

        // Initialize the array of NoteOutputObjects.
        mOutputs = new NoteOutputObject[mNumActiveNotes];

        // In order to have multiple notes play at once, an invisible GameObject is created and cloned multiple 
        // times to serve as containers for each NoteOutputObject. The invisible object will be placed at the position
        // of the virtual instrument manager, so be sure to put the virtual instrument manager on an object that is as
        // close to the AudioListener as possible.  
        GameObject toBeCloned = new GameObject();
        toBeCloned.transform.position = gameObject.transform.position;
        mOutputs[0] = toBeCloned.AddComponent<NoteOutputObject>();
        mOutputs[0].SetAudioData( mInstrument.GetRawAudioDataForNote( mActiveNotes[0] ), sampleRate, mInstrument.GetBuiltInDynamicsThresholds() );



        // For each note, clone the NoteOutputObject component of the invisible object (which will also clone the invisible object).
        // After that, set the audio data of the NoteOutputObject.
        for ( int i = 0; i < mNumActiveNotes; i++ )
        {
            mOutputs[i] = Instantiate( toBeCloned.GetComponent<NoteOutputObject>() );
            Assert.IsNotNull( mOutputs[i] );
            mOutputs[i].SetAudioData( mInstrument.GetRawAudioDataForNote( mActiveNotes[i] ), sampleRate, mInstrument.GetBuiltInDynamicsThresholds() );
        }

        mReady = true;
    }

    // Begins fading out the note as though the key was released. Should only be called via an invoked NoteReleaseEvent.
    // IN: aNoteToFade The note to fade out. 
    public void OnNoteFadeOut( Music.NOTE aNoteToFade )
    {
        Assert.IsTrue( (int)aNoteToFade >= (int)mLowestActiveNote && (int)aNoteToFade <= (int)mHighestActiveNote, "Tried to fade out a note that is not active!" );
        int noteIndex = (int)aNoteToFade - (int)mLowestActiveNote;
        mOutputs[noteIndex].BeginNoteFadeOut();
    }

    // Begin playing a note. Should only be called via an invoked NoteStartEvent.
    // IN: aNoteToPlay The note to play.
    // IN: aVelocity The velocity at which to play the note.
    public void OnNotePlay( Music.NOTE aNoteToPlay, int aVelocity )
    {
        if ( mReady )
        {
            Assert.IsTrue( (int)aNoteToPlay >= (int)mLowestActiveNote && (int)aNoteToPlay <= (int)mHighestActiveNote, "Tried to play a note that is not active!" );
            int noteIndex = (int)aNoteToPlay - (int)mLowestActiveNote;
            mOutputs[noteIndex].BeginNotePlaying( aVelocity );
        }

    }

#if DEBUG

    //---------------------------------------------------------------------------- 
    // Debug Functions & Types
    //---------------------------------------------------------------------------- 

    #if DEBUG_MUSICAL_TYPING

        // Musical Typing allows for using a computer keyboard to debug sound output by 
        // triggering note events for a 19-note range whenever specific keys on the 
        // keyboard are pressed/released. A preset velocity variable is used to simulate the 
        // velocity of the generated note events.
        // The keys that are used are (in this order)
        // a, w, s, e, d, f, t, g, y, h, u, j, k, o, l, p, ;, ', and ] 

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

        // DEBUG_velocity is used to simulate velocity values for musical typing events.
        // Its default value is 100 (max). This function can be used as a callback for 
        // a slider or something similar to allow for changing DEBUG_velocity's value so that 
        // different velocities can be tested.
        public void DEBUG_HandleDebugVelocityChange( float aNewDebugVelocity )
        {
            DEBUG_velocity = (int)aNewDebugVelocity;
        }

        // Handler for musical typing that maps key events to note events. 
        public void DEBUG_HandleMusicalTyping( Event aKeyEvent )
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
                        NotePlayEvent.Invoke( mActiveNotes[i], DEBUG_velocity );
                    }
                    if ( aKeyEvent.type == EventType.KeyUp )
                    {
                        NoteFadeOutEvent.Invoke( mActiveNotes[i] );
                    }
                }
            i++;
            }
        }

        // Sets default values for debug functions for this object.
        public void DEBUG_SetDebugVariables()
        {
            DEBUG_velocity = 100;
        }

    #endif
#endif
}
