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

    // A type of event that's invoked in order to signal that the drum kit is loaded.
    public class DrumKitLoadedEvent : UnityEvent
    {
    }

    // A type of event that's invoked in order to signal that the instrument is loaded.
    public class InstrumentLoadedEvent : UnityEvent
    {
    }

    // A type of event that's invoked in order to modify the echo filter.
    // The parameter is a struct of EchoFilterParameters.
    public class ModifyEchoFilterEvent : UnityEvent<EchoFilterParameters>
    {
    }

    // A type of event that's invoked in order to modify the reverb filter.
    // The parameter is a struct of ReverbFilterParameters
    public class ModifyReverbFilterEvent : UnityEvent<ReverbFilterParameters>
    {
    }

    // A type of event that's invoked in order to play a song.
    // The parameter is the a queue of notes to play.
    public class PlaySongEvent : UnityEvent<Song>
    {
    }

    // A type of event that's invoked in order to play a song.
    // The parameter is the a queue of notes to play.
    public class PlayDrumLoopEvent : UnityEvent<Song>
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

    // A struct that contains the parameters for the reverb filter.
    public struct ReverbFilterParameters
    {
        public bool Active;
        public float DryLevel;
        public float Room;
        public float RoomHF;
        public float DecayTime;
        public float DecayHFRatio;
        public float Reflections;
        public float ReflectDelay;
        public float Reverb;
        public float ReverbDelay;
        public float Diffusion;
        public float Density;
        public float HFReference;
        public float RoomLF;
        public float LFReference;
    }

    //---------------------------------------------------------------------------- 
    // Public Variables
    //---------------------------------------------------------------------------- 
    public NoteStartEvent              NotePlay; // The event that will be invoked whenever a note should be played.
    public NoteReleaseEvent            NoteRelease; // The event that will be invoked whenever a note should fade out.
    public ChangeNoteRangeEvent        ChangeNoteRange; // The event that will be invoked whenever the note range should change.
    public ChangeInstrumentEvent       ChangeInstrument; // The event that will be invoked whenever the instrument should be changed.
    public DrumKitLoadedEvent          DrumKitLoaded; // The event that will be invoked whenever the drum kit has finished loading.
    public InstrumentLoadedEvent       InstrumentLoaded; // The event that will be invoked when the instrument has finished loading.
    public ModifyEchoFilterEvent       ModifyEchoFilter; // The event that will be invoked when the echo filter needs to be modified.
    public ModifyReverbFilterEvent     ModifyReverbFilter; // The event that will be invoked when the reverb filter needs to be modified.
    public PlayDrumLoopEvent           PlayDrumLoop; // The event that will be invoked when a drum loop should be played.
    public PlaySongEvent               PlaySong; // The event that will be invoked when a song should be played.
    public SongManagerClass            SongManager; // The song manager                 

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
    private NoteOutputObject           mDrumLoopOutput; // A NoteOutputObject, but the note is actually a drum loop.
    private NoteOutputObject           mSongOutput; // A NoteOutputObject, but the note is actually a song.
    private NoteOutputObject[]         mOutputs; // An array that holds the NoteOutputObjects that actually handle sound output.
    private VirtualInstrument          mDrumKit; // A drum kit virtual instrument for drum loops.
    private VirtualInstrument          mInstrument; // The loaded virtual instrument that this object will manage.


    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Sets initial values when the object is first loaded.
    public void Awake()
    {
        mReady = false;
        DontDestroyOnLoad( this );

        #if DEBUG && DEBUG_MUSICAL_TYPING
            DEBUG_SetMusicalTypingVariables();
        #endif

        // Set up the events.
        SetUpEvents();

        // Set default values for the member variables.
        SetDefaultValues();

        // Begin loading the default virtual instrument which is a piano.
        StartCoroutine( LoadInstrument( mInstrumentType, ( returned ) => { mInstrument = returned; } ) );
        StartCoroutine( LoadDrumKit( ( returned ) => { mDrumKit = returned; } ) );
    }

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Gets the highest active note.
    // OUT: The highest active note.
    public Music.PITCH GetHighestActiveNote()
    {
        return mHighestActiveNote;
    }

    // Gets the highest note that is supported by the loaded instrument.
    // OUT: The highest note that is supported by the loaded instrument. 
    public Music.PITCH GetHighestSupportedNote()
    {
        return mInstrument.GetHighestSupportedNote();
    }

    // Gets the virtual instrument that is being managed.
    public VirtualInstrument GetInstrument()
    {
        return mInstrument;
    }

    // Gets the lowest active note.
    // OUT: The lowest active note.
    public Music.PITCH GetLowestActiveNote()
    {
        return mLowestActiveNote;
    }

    // Gets the lowest note that is supported by the loaded instrument.
    // OUT: The lowest note that is supported by the loaded instrument.
    public Music.PITCH GetLowestSupportedNote()
    {
        return mInstrument.GetLowestSupportedNote();
    }

    // Gets the number of active notes.
    // OUT: THe number of active notes.
    public int GetNumActiveNotes()
    {
        return mNumActiveNotes;
    }

    //---------------------------------------------------------------------------- 
    // Private Functions
    //---------------------------------------------------------------------------- 

    // Loads the objects that play the audio data.
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
            mOutputs[0].SetLoop( false );
            GameObject clone = null;

            // For each note, clone the invisible object (which will also clone the NoteOutputObject).
            for( int i = 1; i < mNumActiveNotes; i++ )
            {
                clone = Instantiate( toBeCloned );
                clone.name = Music.NoteToString( mActiveNotes[i] ) + "NoteOutputObjectContainer";
                mOutputs[i] = clone.GetComponent<NoteOutputObject>();
            }
        }       

        // Set the audio data of the NoteOutputObject and make sure that they don't loop..
        for( int i = 0; i < mNumActiveNotes; i++ )
        {
            mOutputs[i].SetAudioData( mInstrument.GetRawAudioDataForNote( mActiveNotes[i] ), mMixer, mInstrument.GetBuiltInDynamicsThresholds() );
            mOutputs[i].SetLoop( false );
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
        InstrumentLoaded = new InstrumentLoadedEvent();
        ModifyEchoFilter = new ModifyEchoFilterEvent();
        ModifyReverbFilter = new ModifyReverbFilterEvent();
        PlayDrumLoop = new PlayDrumLoopEvent();
        PlaySong = new PlaySongEvent();
        NotePlay.AddListener( OnNotePlayEvent );
        NoteRelease.AddListener( OnNoteReleaseEvent );
        ChangeNoteRange.AddListener( OnChangeNoteRangeEvent );
        ChangeInstrument.AddListener( OnChangeInstrumentEvent );
        InstrumentLoaded.AddListener( OnInstrumentLoaded );
        ModifyEchoFilter.AddListener( OnModifyEchoFilterEvent );
        ModifyReverbFilter.AddListener( OnModifyReverbFilterEvent );
        PlayDrumLoop.AddListener( OnPlayDrumLoopEvent );
        PlaySong.AddListener( OnPlaySongEvent );

    }

    // Sets the default values for the member variables.
    // Should only be called on Awake().
    private void SetDefaultValues()
    {
        mMixer = Resources.Load<AudioMixer>( "Music/VirtualInstrument/VirtualInstrumentAudioMixer" );
        Assert.IsNotNull( mMixer, "Audio mixer was unable to load!" );

        // Set the active notes.
        mLowestActiveNote = DEFAULT_LOWEST_PITCH;
        mHighestActiveNote = DEFAULT_HIGHEST_PITCH;
        mNumActiveNotes = (int)mHighestActiveNote - (int)mLowestActiveNote + 1;
        mActiveNotes = new Music.PITCH[mNumActiveNotes];
        for( int i = 0; i < mNumActiveNotes; i++ )
        {
            mActiveNotes[i] = (Music.PITCH)( i + (int)mLowestActiveNote );
        }

        // Set the instrument type
        mInstrumentType = DEFAULT_INSTRUMENT_TYPE;

        // Initialize the song and drum loop outputs.
        GameObject songOutputContainer = new GameObject();
        mSongOutput = songOutputContainer.AddComponent<NoteOutputObject>();
        mSongOutput.SetLoop( false );

        GameObject drumOutputContainer = new GameObject();
        mDrumLoopOutput = drumOutputContainer.AddComponent<NoteOutputObject>();
        mDrumLoopOutput.SetLoop( true );

        // Initialize the SongManager.
        SongManager = gameObject.AddComponent<SongManagerClass>();
    }

    //---------------------------------------------------------------------------- 
    // Coroutines
    //---------------------------------------------------------------------------- 

    // Coroutine to load a drum kit. 
    // IN: aINSTRUMENT_TYPE The type of instrument to load.
    // IN: aInstrumentCallback Callback to allow returning the loaded instrument to the calling context. 
    private IEnumerator LoadDrumKit( System.Action<VirtualInstrument> aDrumKitCallback )
    {
        // Load a new instrument
        VirtualInstrument returned = new DrumKit( this );

        // Return the loaded instrument and invoke the instrument's LoadEvent.
        aDrumKitCallback( returned );
        //DrumKitLoaded.Invoke();
        yield return null;
    }

    // Coroutine to load a virtual instrument. 
    // IN: aINSTRUMENT_TYPE The type of instrument to load.
    // IN: aInstrumentCallback Callback to allow returning the loaded instrument to the calling context. 
    private IEnumerator LoadInstrument( Music.INSTRUMENT_TYPE aINSTRUMENT_TYPE, System.Action<VirtualInstrument> aInstrumentCallback )
    {
        // Load a new instrument
        VirtualInstrument returned = null;
        switch( aINSTRUMENT_TYPE )
        {
            case Music.INSTRUMENT_TYPE.DRUM_KIT:
                returned = new DrumKit( this );
                break;
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
        InstrumentLoaded.Invoke();
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

        // Handle the difference in active notes for a drum kit vs. a normal instrument.
        if( mInstrumentType == Music.INSTRUMENT_TYPE.DRUM_KIT )
        {
            mNumActiveNotes = 18;
        }
        else
        {
            mNumActiveNotes = 24;
        }

        // Destroy the note output objects.
        for( int i = 0; i < mOutputs.Length; i++ )
        {
            DestroyImmediate( mOutputs[i], false );
        }

        mOutputs = null;

        // Set default values for the active notes and be sure to account for drums
        if( mInstrumentType == Music.INSTRUMENT_TYPE.DRUM_KIT )
        {
            mLowestActiveNote = Music.PITCH.C0;
        }
        else
        {
            mLowestActiveNote = DEFAULT_LOWEST_PITCH;
        }        
        mHighestActiveNote = (Music.PITCH)( (int)mLowestActiveNote + mNumActiveNotes - 1 );
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
            mMixer.SetFloat( "echoDelay", aParameters.Delay );
            mMixer.SetFloat( "echoDecay", aParameters.Decay );
            mMixer.SetFloat( "echoDryMix", aParameters.DryMix );
            mMixer.SetFloat( "echoWetMix", aParameters.WetMix );
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

    // Function to modify the reverb filter. Should only be called via an invoked ModifyReverbFilterEvent. 
    // IN: aParameters The parameters for the reverb filter.
    public void OnModifyReverbFilterEvent( ReverbFilterParameters aParameters )
    {
        if( aParameters.Active )
        {
            mMixer.SetFloat( "revDryLevel", aParameters.DryLevel );
            mMixer.SetFloat( "revRoom", aParameters.Room );
            mMixer.SetFloat( "revRoomHF", aParameters.RoomHF );
            mMixer.SetFloat( "revDecayTime", aParameters.DecayTime );
            mMixer.SetFloat( "revDecayHFRatio", aParameters.DecayHFRatio );
            mMixer.SetFloat( "revReflections", aParameters.Reflections );
            mMixer.SetFloat( "revReflectDelay", aParameters.ReflectDelay );
            mMixer.SetFloat( "revReverb", aParameters.Reverb );
            mMixer.SetFloat( "revReverbDelay", aParameters.ReverbDelay );
            mMixer.SetFloat( "revDiffusion", aParameters.Diffusion );
            mMixer.SetFloat( "revDensity", aParameters.Density );
            mMixer.SetFloat( "revHFReference", aParameters.HFReference );
            mMixer.SetFloat( "revRoomLF", aParameters.RoomLF );
            mMixer.SetFloat( "revLFReference", aParameters.LFReference );
        }
        else
        {
            mMixer.SetFloat( "revDryLevel", 0f );
            mMixer.SetFloat( "revRoom", -10000f );
            mMixer.SetFloat( "revRoomHF", -10000f );
            mMixer.SetFloat( "revDecayTime", 0.1f );
            mMixer.SetFloat( "revDecayHFRatio", 0.1f );
            mMixer.SetFloat( "revReflections", -10000f );
            mMixer.SetFloat( "revReflectDelay", -10000f );
            mMixer.SetFloat( "revReverb", -10000f );
            mMixer.SetFloat( "revReverbDelay", 0f );
            mMixer.SetFloat( "revDiffusion", 0f );
            mMixer.SetFloat( "revDensity", 0f );
            mMixer.SetFloat( "revHFReference", 20f );
            mMixer.SetFloat( "revRoomLF", -10000f );
            mMixer.SetFloat( "revLFReference", 20f );
        }
    }

    // Begin playing a note. Should only be called via an invoked NoteStartEvent.
    // IN: aNoteToPlay The note to play.
    // IN: aVelocity The velocity at which to play the note.
    public void OnNotePlayEvent( Music.PITCH aNoteToPlay, int aVelocity )
    {
        if ( mReady )
        {
            // Sanity checks.
            Assert.IsTrue( (int)aNoteToPlay >= (int)mLowestActiveNote && (int)aNoteToPlay <= (int)mHighestActiveNote, 
                "Tried to play the note " + Music.NoteToString( aNoteToPlay ) + " which is not active!" );
            Assert.IsTrue( aVelocity <= 100 && aVelocity >= 0,
                "Tried to play a note with a velocity of " + aVelocity.ToString() + " which is not within the range from 0 to 100!" );

            // Get the note index.
            int noteIndex = (int)aNoteToPlay - (int)mLowestActiveNote;

            // Account for drum kits where we would need to make some outputs silent in some situations.
            // Ex: Playing a closed hi-hat should stop an open hi-hat that is still playing.
            if( mInstrumentType == Music.INSTRUMENT_TYPE.DRUM_KIT )
            {
                switch( (Music.DRUM)aNoteToPlay )
                {
                    case Music.DRUM.HIHAT_C:
                        mOutputs[(int)Music.DRUM.HIHAT_O].BeginNotePlaying( 0, 0 );
                        mOutputs[(int)Music.DRUM.HIHAT_P].BeginNotePlaying( 0, 0 );
                        break;
                    case Music.DRUM.HIHAT_O:
                        mOutputs[(int)Music.DRUM.HIHAT_C].BeginNotePlaying( 0, 0 );
                        mOutputs[(int)Music.DRUM.HIHAT_P].BeginNotePlaying( 0, 0 );
                        break;
                    case Music.DRUM.HIHAT_P:
                        mOutputs[(int)Music.DRUM.HIHAT_C].BeginNotePlaying( 0, 0 );
                        mOutputs[(int)Music.DRUM.HIHAT_O].BeginNotePlaying( 0, 0 );
                        break;
                    default:
                        break;
                }
            }

            // Get the built-in dynamics index and the velocity factor.
            int velIndex = mInstrument.GetBuiltInDynamicsThresholdIndex( aVelocity );
            float velFactor = mInstrument.GetAdjustedVelocityFactor( aVelocity );

            // Begin playing the note.
            mOutputs[noteIndex].BeginNotePlaying( velFactor, velIndex );
        }

    }

    // Begins fading out the note as though the key was released. Should only be called via an invoked NoteReleaseEvent.
    // IN: aNoteToFade The note to fade out. 
    public void OnNoteReleaseEvent( Music.PITCH aNoteToRelease )
    {
        // Don't account for note releases for drums.
        if( mInstrumentType != Music.INSTRUMENT_TYPE.DRUM_KIT )
        {
            Assert.IsTrue( (int)aNoteToRelease >= (int)mLowestActiveNote && (int)aNoteToRelease <= (int)mHighestActiveNote, 
                "Tried to fade out the note " + Music.NoteToString( aNoteToRelease ) + " which is not active!" );
            int noteIndex = (int)aNoteToRelease - (int)mLowestActiveNote;
            mOutputs[noteIndex].BeginNoteFadeOut();
        }
    }

    // Begins playing a drum loop
    // IN: aLoop The loop to play.
    public void OnPlayDrumLoopEvent( Song aLoop )
    {
        // Get the song's note data.
        Song.CombinedNoteData[] loopNoteData = aLoop.GetNoteData();

        // Get the number of notes in the loop.
        int numNotes = loopNoteData.Length;

        // Get the number of samples in a measure of the loop.
        Music.TimeSignature loopTS = aLoop.GetTimeSignature();
        int numSamplesInMeasure = loopTS.BeatsPerMeasure * Song.GetNoteLengthInSamples( aLoop.GetBPM(), 44100, loopTS.BaseBeat, loopTS );

        // Get the total number of samples in the loop
        int numSamples = 0;
        if( loopNoteData[numNotes - 1].TotalOffset > numSamplesInMeasure )
        {
            numSamples = loopNoteData[numNotes - 1].TotalOffset + ( loopNoteData[numNotes - 1].TotalOffset % numSamplesInMeasure );
        }
        else
        {
            numSamples = loopNoteData[numNotes - 1].TotalOffset + ( numSamplesInMeasure % loopNoteData[numNotes - 1].TotalOffset );
        }

        
        // Allocate a 2-D array for the song's audio data.
        float[][] loopAudioData = new float[1][];
        loopAudioData[0] = new float[numSamples];

        // Make some variables to keep track of the velocity and offset.
        int offset = 0;
        int velocity = 0;
        int velIndex = 0;
        float velFactor = 0f;

        // Iterate through all of the notes.
        for( int i = 0; i < numNotes; i++ )
        {
            // Get the offset and velocity from the note.
            offset = loopNoteData[i].TotalOffset;
            velocity = loopNoteData[i].PercussionData.Velocity;

            // Get the associated dynamics index and velocity factor.
            velIndex = mDrumKit.GetBuiltInDynamicsThresholdIndex( velocity );
            velFactor = mDrumKit.GetAdjustedVelocityFactor( velocity );

            // Add the audio data for all of the drums in the note.
            for( int j = 0; j < loopNoteData[i].PercussionData.Hits.Length; j++ )
            {
                // Get the audio data for the note.
                float[][] drumAudioData = mDrumKit.GetRawAudioDataForNote( (Music.PITCH)( loopNoteData[i].PercussionData.Hits[j] ) );

                // Put all of the samples from the drum's audio data into the song audio data.
                int audioDataIndex = 0;
                int loopDataIndex = offset;

                // Account for silencing hi-hats if needed.
                int numSamplesUntilNextHiHat = numSamples;
                if( loopNoteData[i].PercussionData.Hits[j] == Music.DRUM.HIHAT_O )
                {
                    int nextHiHatIndex = i+1;
                    bool nextHiHatFound = false;

                    // Get the next hi hat hit by iterating through the notes.
                    while( nextHiHatIndex != i && !nextHiHatFound )
                    {
                        // Make sure we don't have any index out of bounds errors.
                        if( nextHiHatIndex == numNotes )
                        {
                            nextHiHatIndex = 0;
                        }

                        // See if this note has hi-hat hits. Break the loop if so.
                        if( loopNoteData[nextHiHatIndex].PercussionData.HasHiHat )
                        {
                            nextHiHatFound = true;
                            numSamplesUntilNextHiHat = loopNoteData[nextHiHatIndex].TotalOffset - loopNoteData[i].TotalOffset;
                            if( numSamplesUntilNextHiHat < 0 )
                            {
                                numSamplesUntilNextHiHat = ( -1 * numSamplesUntilNextHiHat ) + ( numSamples - loopNoteData[i].TotalOffset );
                            }
                        }
                        else
                        {
                            // Go to the next note if this note doesn't have any hi-hat hits.
                            nextHiHatIndex++;
                        }
                    }
                }

                // Put the audio data for the drum into the loop.
                while( audioDataIndex < drumAudioData[velIndex].Length && numSamplesUntilNextHiHat > 0 )
                {
                    loopAudioData[0][loopDataIndex] += ( drumAudioData[velIndex][audioDataIndex] * velFactor );
                    loopDataIndex++;

                    // Account for bleeding over into the next loop.
                    if( loopDataIndex >= numSamples )
                    {
                        loopDataIndex = 0;
                    }

                    audioDataIndex++;
                    numSamplesUntilNextHiHat--;
                }

            }
        }

        // Set the song output object's audio data and begin playing the song.
        mDrumLoopOutput.SetAudioData( loopAudioData, mMixer, null );
        mDrumLoopOutput.SetLoop( true );
        mDrumLoopOutput.BeginNotePlaying( 1f, 0 );
    }


    // Begins playing a song. 
    // IN: aSong The song to play.
    public void OnPlaySongEvent( Song aSong )
    {
        // Get the song's note data.
        Song.CombinedNoteData[] songNoteData = aSong.GetNoteData();

        // Get the number of notes in the song.
        int numNotes = songNoteData.Length;

        // Get the total number of samples in the song.
        int numSamples = 0;

        if( aSong.GetSongType() != Song.SongType.DrumLoop )
        {
            for( int i = 0; i < numNotes; i++ )
            {
                numSamples = Mathf.Max( ( songNoteData[i].MelodyData.NumSamples + songNoteData[i].TotalOffset ), numSamples );
            }
        }
        else
        {
            numSamples = 0;
            for( int i = 0; i < songNoteData[numNotes - 1].PercussionData.Hits.Length; i++ )
            {
                if( numSamples < ( songNoteData[numNotes - 1].TotalOffset + 
                    mDrumKit.GetRawAudioDataForNote( (Music.PITCH)( songNoteData[numNotes - 1].PercussionData.Hits[i] ) )[0].Length ) )
                {
                    numSamples = songNoteData[numNotes - 1].TotalOffset +
                        mDrumKit.GetRawAudioDataForNote( (Music.PITCH)( songNoteData[numNotes - 1].PercussionData.Hits[i] ) )[0].Length;
                }
            }
        }


        // Allocate a 2-D array for the song's audio data.
        float[][] songAudioData = new float[1][];
        songAudioData[0] = new float[numSamples];

        // Make some variables to keep track of the velocity and offset.
        int offset = 0;
        int velocity = 0;
        int velIndex = 0;
        float velFactor = 0f;

        // Iterate through all of the notes.
        for( int i = 0; i < numNotes; i++ )
        {
            // If the note is a rest, we don't need to worry its pitches. Otherwise, add the note's pitches.
            if( songNoteData[i].MelodyData.Pitches != null && songNoteData[i].MelodyData.Pitches[0] != Music.PITCH.REST )
            {
                // Get the offset and velocity from the note.
                offset = songNoteData[i].TotalOffset;
                velocity = songNoteData[i].MelodyData.Velocity;

                // Get the associated dynamics index and velocity factor.
                velIndex = mInstrument.GetBuiltInDynamicsThresholdIndex( velocity );
                velFactor = mInstrument.GetAdjustedVelocityFactor( velocity );

                // Add the audio data for all of the pitches in the note.
                for( int j = 0; j < songNoteData[i].MelodyData.Pitches.Length; j++ )
                {
                    // Get the audio data for the note.
                    float[][] pitchAudioData = mInstrument.GetRawAudioDataForNote( songNoteData[i].MelodyData.Pitches[j] );

                    // Put all of the samples from the pitch's audio data into the song audio data.
                    int k = 0;
                    while( k < songNoteData[i].MelodyData.NumSamples && k < pitchAudioData[velIndex].Length )
                    {
                        songAudioData[0][k + offset] += ( pitchAudioData[velIndex][k] * velFactor );
                        k++;
                    }

                    // Adjust for when the note is released.
                    float releaseFactor = velFactor;
                    while( k < pitchAudioData[velIndex].Length && k + offset < numSamples && releaseFactor > 0 )
                    {
                        songAudioData[0][k + offset] += pitchAudioData[velIndex][k] * releaseFactor;
                        releaseFactor -= ( 1f / 22500f );
                        k++;
                    }
                }
            }

            // If the note has drums, then add them.
            if( songNoteData[i].PercussionData.Hits != null )
            {
                // Get the offset and velocity from the note.
                offset = songNoteData[i].TotalOffset;
                velocity = songNoteData[i].PercussionData.Velocity;

                // Get the associated dynamics index and velocity factor.
                velIndex = mDrumKit.GetBuiltInDynamicsThresholdIndex( velocity );
                velFactor = mDrumKit.GetAdjustedVelocityFactor( velocity );

                // Add the audio data for all of the drums in the note.
                for( int j = 0; j < songNoteData[i].PercussionData.Hits.Length; j++ )
                {
                    // Get the audio data for the note.
                    float[][] drumAudioData = mDrumKit.GetRawAudioDataForNote( (Music.PITCH)( songNoteData[i].PercussionData.Hits[j] ) );

                    // Put all of the samples from the drum's audio data into the song audio data.
                    int audioDataIndex = 0;
                    int drumDataIndex = offset;

                    // Account for silencing hi-hats if needed.
                    int numSamplesUntilNextHiHat = numSamples;
                    if( songNoteData[i].PercussionData.Hits[j] == Music.DRUM.HIHAT_O )
                    {
                        int nextHiHatIndex = i+1;
                        bool nextHiHatFound = false;

                        // Get the next hi hat hit by iterating through the notes.
                        while( nextHiHatIndex != i && !nextHiHatFound )
                        {
                            // Make sure we don't have any index out of bounds errors.
                            if( nextHiHatIndex == numNotes )
                            {
                                nextHiHatIndex = 0;
                            }

                            // See if this note has hi-hat hits. Break the loop if so.
                            if( songNoteData[nextHiHatIndex].PercussionData.HasHiHat )
                            {
                                nextHiHatFound = true;
                                numSamplesUntilNextHiHat = songNoteData[nextHiHatIndex].TotalOffset - songNoteData[i].TotalOffset;
                                if( numSamplesUntilNextHiHat < 0 )
                                {
                                    numSamplesUntilNextHiHat = ( -1 * numSamplesUntilNextHiHat ) + ( numSamples - songNoteData[i].TotalOffset );
                                }
                            }
                            else
                            {
                                // Go to the next note if this note doesn't have any hi-hat hits.
                                nextHiHatIndex++;
                            }
                        }
                    }

                    // Put the audio data for the drum into the song.
                    while( audioDataIndex < drumAudioData[velIndex].Length && numSamplesUntilNextHiHat > 0 && drumDataIndex < numSamples )
                    {
                        songAudioData[0][drumDataIndex] += ( drumAudioData[velIndex][audioDataIndex] * velFactor );
                        drumDataIndex++;
                        audioDataIndex++;
                        numSamplesUntilNextHiHat--;
                    }

                }
            }
        }

        // Set the song output object's audio data and begin playing the song.
        mSongOutput.SetAudioData( songAudioData, mMixer, null );
        mSongOutput.BeginNotePlaying( 1f, 0 );
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
        while ( !found && i < DEBUG_numMusicalTypingKeys && i < mNumActiveNotes )
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
