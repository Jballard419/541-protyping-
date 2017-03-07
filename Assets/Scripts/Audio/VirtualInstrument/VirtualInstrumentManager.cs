using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using UnityEngine.Events;

#if DEBUG
    using UnityEngine.SceneManagement;
#endif

/**
 * @class VirtualInstrumentManager
 * @brief The central hub for all audio management.
 * 
 * The VirtualInstrumentManager is a script for a Unity GameObject that manages a virtual instrument and its output.
 * Its responsibilities include: 
 *     @li Initialization of the audio code 
 *     @li Serving as the bridge between non-audio objects and audio objects. 
 *     @li Connecting each NoteOutputObject which handles the output of audio to the VirtualInstrument which holds the audio data. 
 *     @li Mapping each Song to audio data from the VirtualInstrument and sending it to the proper NoteOutputObject. 
 *     @li Sending parameters for audio effects to the AudioMixer provided by Unity. 
 *     @li Handling Musical Typing Events. 
 *     @li Passing data to ATI_Diagnostics. 
 *     
 *     @nosubgrouping
*/
public class VirtualInstrumentManager : MonoBehaviour
{
    /*************************************************************************//** 
    * @defgroup VIMConst Constants
    * @ingroup VIM
    * These are constants that are used in the initialization of the VirtualInstrumentManager.
    *****************************************************************************/
    /** @{ */
    private const Music.PITCH DEFAULT_LOWEST_PITCH = Music.PITCH.C3; //!< The default lowest loaded pitch.
    private const Music.PITCH DEFAULT_HIGHEST_PITCH = Music.PITCH.C5; //!< The default highest loaded pitch.
    private const Music.INSTRUMENT_TYPE DEFAULT_INSTRUMENT_TYPE = Music.INSTRUMENT_TYPE.PIANO; //!< The default loaded instrument.
    /** @} */

    /*************************************************************************//** 
    * @defgroup VIMEventTypes Event Types
    * @ingroup VIM
    * These are classes that inherit from UnityEvent with varying parameters. They provide a base type for the @link VIMEvents events @endlink
    *****************************************************************************/
    /** @{ */

    /**
     * @brief Notifies when the audio is finished. There's no handler for this, but it can be used by other classes who need to be notified.
    */
    public class AudioFinishedEvent : UnityEvent
    {
    }

    /**
     * @brief A type of event that is invoked whenever the range of playable pitches should change. 
     * 
     * The parameter for this type of event is a Music.PITCH which is the lowest pitch
     * of the new range.
     * 
     * @see Music Music::PITCH
    */
    public class ChangeNoteRangeEvent : UnityEvent<Music.PITCH>
    {
    }

    /** 
     * @brief A type of event that is invoked whenever a new instrument should be loaded. 
     * 
     * The parameter for this type of event is a Music.INSTRUMENT_TYPE which is the 
     * type of instrument that should be loaded.
     * 
     * @see Music Music::INSTRUMENT_TYPE
    */
    public class ChangeInstrumentEvent : UnityEvent<Music.INSTRUMENT_TYPE>
    {
    }

    /** 
     * @brief A type of event that is invoked in order to signal that the drum kit has loaded. 
     * @see VirtualInstrumentManager::mDrumKit DrumKit VirtualInstrument
    */
    public class DrumKitLoadedEvent : UnityEvent
    {
    }

    /** 
     * @brief A type of event that is invoked in order to signal that the instrument has loaded. 
     * @see VirtualInstrumentManager::mInstrument VirtualInstrument 
    */
    public class InstrumentLoadedEvent : UnityEvent
    {
    }

    /** 
     * @brief A type of event that is invoked in order to modify the echo filter.
     * 
     * The parameter for this type of event is an EchoFilterParameters struct.
     *  
     * @see VirtualInstrumentManager::EchoFilterParameters  
    */
    public class ModifyEchoFilterEvent : UnityEvent<EchoFilterParameters>
    {
    }

    /** 
     * @brief A type of event that is invoked in order to modify the reverb filter.
     * 
     * The parameter for this type of event is an ReverbFilterParameters struct.
     *  
     * @see VirtualInstrumentManager::ReverbFilterParameters
    */
    public class ModifyReverbFilterEvent : UnityEvent<ReverbFilterParameters>
    {
    }

    /** 
     * @brief A type of event that is invoked in order to pause a drum loop.
     *  
     * @see Song SongManagerClass NoteOutputObject::PauseAudio
    */
    public class PauseDrumLoopEvent : UnityEvent
    {
    }

    /** 
     * @brief A type of event that is invoked in order to pause a song.
     *  
     * @see Song SongManagerClass NoteOutputObject::PauseAudio
    */
    public class PauseSongEvent : UnityEvent
    {
    }

    /** 
     * @brief A type of event that is invoked in order to play a drum loop.
     * 
     * The parameter for this type of event is a Song with the Song::SongType::DrumLoop. 
     * The song can be obtained from the SongManagerClass, which the VirtualInstrumentManager holds a
     * reference to with VirtualInstrumentManager::SongManager.
     *  
     * @see Song Song::SongType SongManagerClass VirtualInstrumentManager::SongManager
    */
    public class PlayDrumLoopEvent : UnityEvent<Song>
    {
    }

    /**
     * @brief A type of event that is invoked whenever a note should be played. 
     * 
     * The parameters for this type of event are a Music.PITCH which is the pitch
     * of the note to play and an integer which is the velocity at which to play it.
     * 
     * @see Music Music::PITCH
    */
    public class PlayNoteEvent : UnityEvent<Music.PITCH, int>
    {
    }

    /** 
     * @brief A type of event that is invoked in order to play a song.
     * 
     * The parameter for this type of event is a Song. The song can be obtained
     * from the SongManagerClass, which the VirtualInstrumentManager holds a
     * reference to with VirtualInstrumentManager::SongManager.
     *  
     * @see Song SongManagerClass VirtualInstrumentManager::SongManager
    */
    public class PlaySongEvent : UnityEvent<Song>
    {
    }

    /**
     * @brief A type of event that is invoked whenever a song needs to be queued for later playback.
     * 
     * The parameter for this type of event is the song that needs to be queued.
     * 
     * @see Song
    */
    public class QueueSongEvent : UnityEvent<Song>
    {
    }

    /** 
     * @brief A type of event that is invoked whenever a note should fade out as though the key was released. 
     * 
     * The parameter for this type of event is a Music.PITCH which is the pitch
     * of the note that was released.
     * 
     * @see Music Music::PITCH
    */
    public class ReleaseNoteEvent : UnityEvent<Music.PITCH>
    {
    }

    /** 
     * @brief Resumes a drum loop.
     * 
     * @see Song SongManagerClass NoteOutputObject::ResumeAudio
    */
    public class ResumeDrumLoopEvent : UnityEvent
    {
    }

    /** 
     * @brief Resumes a song.
     * 
     * @see Song SongManagerClass NoteOutputObject::ResumeAudio
    */
    public class ResumeSongEvent : UnityEvent
    {
    }

    /** 
     * @brief Stops a drum loop.
     * 
     * @see Song SongManagerClass NoteOutputObject::StopAudio
    */
    public class StopDrumLoopEvent : UnityEvent
    {
    }

    /** 
     * @brief Stops a song.
     * 
     * @see Song SongManagerClass NoteOutputObject::StopAudio
    */
    public class StopSongEvent : UnityEvent
    {
    }

    /** @} */

    /*************************************************************************//** 
    * @defgroup filterParams Audio Effect Parameters
    * @ingroup VIM
    * These are structs that contain parameters for the audio effects.
    *****************************************************************************/
    /** @{ */

    /** 
     * @struct VirtualInstrumentManager::EchoFilterParameters
     * @brief A struct that holds the parameters for the echo filter.
     * 
     * The parameters other than Active are used only if Active is true.
     * If Active is false, then a set of parameters is used that negates the filter.
    */
    public struct EchoFilterParameters
    {
        public bool Active;
        public float Decay;
        public float Delay;
        public float DryMix;
        public float WetMix;
    }

    /** 
     * @struct VirtualInstrumentManager::ReverbFilterParameters
     * @brief A struct that holds the parameters for the reverb filter.
     * 
     * The parameters other than Active are used only if Active is true.
     * If Active is false, then a set of parameters is used that negates the filter.
    */
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

    /** @} */


    /*************************************************************************//** 
    * @defgroup VIMEvents Events
    * @ingroup VIM
    * These are the events that are used to iteract with the VirtualInstrumentManager.
    *****************************************************************************/
    /** @{ */
    public AudioFinishedEvent          AudioFinished; //!< @see VirtualInstrumentManager::AudioFinishedEvent
    public ChangeNoteRangeEvent        ChangeNoteRange; //!< @see VirtualInstrumentManager::ChangeNoteRangeEvent
    public ChangeInstrumentEvent       ChangeInstrument; //!< @see VirtualInstrumentManager::ChangeInstrumentEvent
    public DrumKitLoadedEvent          DrumKitLoaded; //!< @see VirtualInstrumentManager::DrumKitLoadedEvent
    public InstrumentLoadedEvent       InstrumentLoaded; //!< @see VirtualInstrumentManager::InstrumentLoadedEvent
    public ModifyEchoFilterEvent       ModifyEchoFilter; //!< @see VirtualInstrumentManager::ModifyEchoFilterEvent
    public ModifyReverbFilterEvent     ModifyReverbFilter; //!< @see VirtualInstrumentManager::ModifyReverbFilterEvent
    public PauseDrumLoopEvent          PauseDrumLoop; //!< @see VirtualInstrumentManager::PauseDrumLoopEvent
    public PauseSongEvent              PauseSong; //!< @see VirtualInstrumentManager::PauseSongEvent
    public PlayDrumLoopEvent           PlayDrumLoop; //!< @see VirtualInstrumentManager::PlayDrumLoopEvent
    public PlayNoteEvent               PlayNote; //!< @see VirtualInstrumentManager::PlayNoteEvent
    public PlaySongEvent               PlaySong; //!< @see VirtualInstrumentManager::PlaySongEvent
    public QueueSongEvent              QueueSong; //!< @see VirtualInstrumentManager::QueueSongEvent
    public ReleaseNoteEvent            ReleaseNote; //!< @see VirtualInstrumentManager::ReleaseNoteEvent
    public ResumeDrumLoopEvent         ResumeDrumLoop; //!< @see VirtualInstrumentManager::ResumeDrumLoopEvent
    public ResumeSongEvent             ResumeSong; //!< @see VirtualInstrumentManager::ResumeSongEvent
    public StopDrumLoopEvent           StopDrumLoop; //!< @see VirtualInstrumentManager::StopDrumLoopEvent
    public StopSongEvent               StopSong; //!< @see VirtualInstrumentManager::StopSongEvent
    /** @} */

    /*************************************************************************//** 
    * @defgroup VIMPub Public Variables
    * @ingroup VIM
    * These are variables that can be used in classes outside the VirtualInstrumentManager
    *****************************************************************************/
    /** @{ */
    public bool                        ShowDiagnostics = true; //!< Whether or not to show the @link ATI_Diagnostics diagnostic overlay@endlink. 
    public SongManagerClass            SongManager; //!< The song manager. @see SongManagerClass                 
                                                    /** @} */

    /*************************************************************************//** 
    * @defgroup VIMPriv Private Variables
    * @ingroup VIM
    * These are variables used internally by the VirtualInstrumentManager.
    *****************************************************************************/
    /** @{ */
    #if DEBUG_AUDIO_DIAGNOSTICS
        private ATI_Diagnostics mDiagnosticsHandler; //!< The handler for displaying audio diagnostics.
    #endif
    private AudioMixer                 mMixer; //!< The audio mixer that all audio output will be routed through.
    private bool                       mReady; //!< Whether or not the manager is ready to play notes.
    private int                        mNumActiveNotes; //!< The number of currently active notes.
    private Music.INSTRUMENT_TYPE      mInstrumentType; //!< The type of instrument that is currently loaded.
    private Music.PITCH                mLowestActiveNote; //!< The lowest currently active note.
    private Music.PITCH                mHighestActiveNote; //!< The highest currently active note.
    private Music.PITCH[]              mActiveNotes; //!< An array that holds all of the currently active notes.
    #if DEBUG_MUSICAL_TYPING
        private MusicalTypingHandler   mMusicalTypingHandler = null; //!< The handler for @link MusicalTypingHandler Musical Typing@endlink.
    #endif
    private NoteOutputObject           mDrumLoopOutput; //!< A NoteOutputObject, but the note is actually a drum loop.
    private NoteOutputObject           mSongOutput; //!< A NoteOutputObject, but the note is actually a song.
    private NoteOutputObject[]         mOutputs; //!< An array that holds the NoteOutputObjects that actually handle sound output.
    private Song                       mQueuedSong = null; //!< The song that is currently queued.
    private VirtualInstrument          mDrumKit; //!< A drum kit virtual instrument for drum loops.
    private VirtualInstrument          mInstrument; //!< The loaded virtual instrument that this object will manage.
    /** @} */

    /*************************************************************************//** 
    * @defgroup VIMUnity Unity Functions
    * @ingroup VIM
    * These are functions that are built-in to Unity. Since the VirtualInstrumentManager is a Unity GameObject, these are called automatically.
    *****************************************************************************/
    /** @{ */

    /**
     * @brief Called when the object is created. Sets the default values, sets up the events, and begins loading the instrument.
    */
    public void Awake()
    {
        mReady = false;

        #if DEBUG_AUDIO_DIAGNOSTICS
            // Set up the audio diagnostics scene
            GameObject diagnosticsContainer = Instantiate( Resources.Load<GameObject>( "Audio/Prefabs/AudioDiagnosticsPrefab" ) );
            Assert.IsNotNull( diagnosticsContainer, "Could not load the diagnostics prefab!" );
            mDiagnosticsHandler = diagnosticsContainer.transform.GetChild( 0 ).GetComponent<ATI_Diagnostics>();
            Assert.IsNotNull( mDiagnosticsHandler, "Could not get the diagnostics handler!" );
            if( !ShowDiagnostics )
            {
                diagnosticsContainer.SetActive( false );
            }
        #endif

        #if DEBUG_MUSICAL_TYPING
            mMusicalTypingHandler = gameObject.AddComponent<MusicalTypingHandler>();
            Assert.IsNotNull( mMusicalTypingHandler, "Could not load Musical Typing!" );
        #endif

        // Set up the Song Manager.
        SongManager = new SongManagerClass();

        // Set up the events.
        SetUpEvents();

        // Set default values for the member variables.
        SetDefaultValues();

        // Begin loading the default virtual instrument which is a piano.
        StartCoroutine( LoadInstrument( mInstrumentType, ( returned ) => { mInstrument = returned; } ) );
        StartCoroutine( LoadDrumKit( ( returned ) => { mDrumKit = returned; } ) );
    }

    /**
     * @brief Destroys the @link VIM Virtual Instrument Manager@endlink.
    */
    private void OnDestroy()
    {
        // Destroy everything created by the VIM.
        if( mDrumLoopOutput != null )
        {
            DestroyImmediate( mDrumLoopOutput.gameObject, false );
            mDrumLoopOutput = null;
        }

        if( mSongOutput != null )
        {
            DestroyImmediate( mSongOutput.gameObject, false );
            mSongOutput = null;
        }

        for( int i = 0; i < mNumActiveNotes; i++ )
        {
            if( mOutputs != null && mOutputs[i] != null )
            {
                DestroyImmediate( mOutputs[i].gameObject, false );
                mOutputs[i] = null;
            }
        }
        mOutputs = null;

        #if DEBUG_AUDIO_DIAGNOSTICS
            if( mDiagnosticsHandler != null )
            {
                DestroyImmediate( mDiagnosticsHandler.transform.parent.gameObject, false );
                mDiagnosticsHandler = null;
            }
        #endif

        // Clean up
        GC.Collect();
        Resources.UnloadUnusedAssets();
    }

    /** @} */

    /*************************************************************************//** 
    * @defgroup VIMPubFunc Public Functions
    * @ingroup VIM
    * These are functions that allow other classes to retrieve information from the VirtualInstrumentManager
    *****************************************************************************/
    /** @{ */

    /**
     * @brief Gets the array of active notes.
     * @return The array of active notes.
     */
    public Music.PITCH[] GetActiveNotes()
    {
        return mActiveNotes;
    }

    #if DEBUG_AUDIO_DIAGNOSTICS
        /** 
        * @brief Gets the @link ATI_Diagnostics audio diagnostics handler@endlink.
        * @return The @link ATI_Diagnostics audio diagnostics handler@endlink.
        */
        public ATI_Diagnostics GetDiagnosticsHandler()
        {
            return mDiagnosticsHandler;
        }
    #endif

    /**
     * @brief Gets the highest active note.
    */
    public Music.PITCH GetHighestActiveNote()
    {
        return mHighestActiveNote;
    }

    /**
     * @brief Gets the highest note that is supported by the loaded instrument
    */
    public Music.PITCH GetHighestSupportedNote()
    {
        return mInstrument.GetHighestSupportedPitch();
    }

    /**
     * @brief Gets the loaded VirtualInstrument
    */
    public VirtualInstrument GetInstrument()
    {
        return mInstrument;
    }

    /**
     * @brief Gets the lowest active note.
    */
    public Music.PITCH GetLowestActiveNote()
    {
        return mLowestActiveNote;
    }

    /**
     * @brief Gets the lowest note that is supported by the loaded instrument.
    */
    public Music.PITCH GetLowestSupportedNote()
    {
        return mInstrument.GetLowestSupportedPitch();
    }

    #if DEBUG_MUSICAL_TYPING
        /** Gets the @link MusicalTypingHandler Musical Typing Handler@endlink.
         * @return The @link MusicalTypingHandler Musical Typing Handler@endlink.
        */
        public MusicalTypingHandler GetMusicalTypingHandler()
        {
            return mMusicalTypingHandler;
        }
    #endif

    /**
     * @brief Gets the number of notes that currently can be played
    */
    public int GetNumActiveNotes()
    {
        return mNumActiveNotes;
    }
    /** @} */

    /*************************************************************************//** 
    * @defgroup VIMPrivFunc Private Functions
    * @ingroup VIM
    * These are functions that the VirtualInstrumentManager uses internally to perform its tasks.
    *****************************************************************************/
    /** @{ */

    /**
     * @brief Loads each NoteOutputObject that is used to output the audio.
     * 
     * This function loads a NoteOutputObject for every currently playable note, and 
     * uses the loaded VirtualInstrument to set the audio data that they will output.
     * 
     * @see NoteOutputObject
    */
    private void LoadNoteOutputObjects()
    {
        Assert.IsNotNull( mInstrument, "Tried to load NoteOutputObjects when the instrument was null!" );

        if( mOutputs == null )
        {
            // Initialize the array of NoteOutputObjects.
            mOutputs = new NoteOutputObject[mNumActiveNotes];

            // For each note, clone a NoteOutputObject.
            GameObject clone = null;
            for( int i = 0; i < mNumActiveNotes; i++ )
            {
                clone = Instantiate( Resources.Load<GameObject>( "Audio/Prefabs/NoteOutputObjectPrefab" ) );
                clone.transform.position = transform.position;
                mOutputs[i] = clone.GetComponent<NoteOutputObject>();
                clone = null;
            }
        }

        // Set the audio data of the NoteOutputObject and make sure that they don't loop..
        for( int i = 0; i < mNumActiveNotes; i++ )
        {
            mOutputs[i].gameObject.name = Music.NoteToString( mActiveNotes[i] ) + "NoteOutputObjectContainer";
            mOutputs[i].SetAudioData( mInstrument.GetAudioDataForPitch( mActiveNotes[i] ), mMixer, mInstrument.GetBuiltInDynamicsThresholds() );
            mOutputs[i].SetLoop( false );
            mOutputs[i].SetVIM( this );
        }

        // Cleanup.
        GC.Collect();
        Resources.UnloadUnusedAssets();

        // Set that we're ready for note events.
        mReady = true;

        Debug.Log( "NoteOutputObjects are loaded" );
    }

    /** 
     * @brief Sets up the events and adds their listeners. Should only be called from VirtualInstrumentManager::Awake().
    */
    private void SetUpEvents()
    {
        // Initialize the events.
        AudioFinished = new AudioFinishedEvent();
        ChangeNoteRange = new ChangeNoteRangeEvent();
        ChangeInstrument = new ChangeInstrumentEvent();
        InstrumentLoaded = new InstrumentLoadedEvent();
        ModifyEchoFilter = new ModifyEchoFilterEvent();
        ModifyReverbFilter = new ModifyReverbFilterEvent();
        PauseDrumLoop = new PauseDrumLoopEvent();
        PauseSong = new PauseSongEvent();
        PlayDrumLoop = new PlayDrumLoopEvent();
        PlayNote = new PlayNoteEvent();
        PlaySong = new PlaySongEvent();
        QueueSong = new QueueSongEvent();
        ReleaseNote = new ReleaseNoteEvent();
        ResumeDrumLoop = new ResumeDrumLoopEvent();
        ResumeSong = new ResumeSongEvent();
        StopDrumLoop = new StopDrumLoopEvent();
        StopSong = new StopSongEvent();

        // Add their listeners.
        ChangeNoteRange.AddListener( OnChangeNoteRangeEvent );
        ChangeInstrument.AddListener( OnChangeInstrumentEvent );
        InstrumentLoaded.AddListener( OnInstrumentLoaded );
        ModifyEchoFilter.AddListener( OnModifyEchoFilterEvent );
        ModifyReverbFilter.AddListener( OnModifyReverbFilterEvent );
        PauseDrumLoop.AddListener( OnPauseDrumLoopEvent );
        PauseSong.AddListener( OnPauseSongEvent );
        PlayDrumLoop.AddListener( OnPlayDrumLoopEvent );
        PlayNote.AddListener( OnPlayNoteEvent );
        PlaySong.AddListener( OnPlaySongEvent );
        ReleaseNote.AddListener( OnReleaseNoteEvent );
        ResumeDrumLoop.AddListener( OnResumeDrumLoopEvent );
        ResumeSong.AddListener( OnResumeSongEvent );
        StopDrumLoop.AddListener( OnStopDrumLoopEvent );
        StopSong.AddListener( OnStopSongEvent );


    }

    /** 
     * @brief Sets the default values for the VirtualInstrumentManager. Should only be called from VirtualInstrumentManager::Awake().
    */
    private void SetDefaultValues()
    {
        mMixer = Resources.Load<AudioMixer>( "Audio/VirtualInstrument/VirtualInstrumentAudioMixer" );
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
        GameObject songOutputContainer = Instantiate( Resources.Load<GameObject>( "Audio/Prefabs/NoteOutputObjectPrefab" ) );
        Assert.IsNotNull( songOutputContainer, "Could not load NoteOutputObject prefab!" );
        songOutputContainer.name = "Song Output";
        songOutputContainer.transform.position = transform.position;
        mSongOutput = songOutputContainer.GetComponent<NoteOutputObject>();
        mSongOutput.SetLoop( false );
        mSongOutput.SetVIM( this );
        mSongOutput.ShouldNotifyWhenFinished( true );

        GameObject drumOutputContainer = Instantiate( Resources.Load<GameObject>( "Audio/Prefabs/NoteOutputObjectPrefab" ) );
        Assert.IsNotNull( songOutputContainer, "Could not load NoteOutputObject prefab!" );
        drumOutputContainer.name = "Drum Loop Output";
        drumOutputContainer.transform.position = transform.position;
        mDrumLoopOutput = drumOutputContainer.GetComponent<NoteOutputObject>();
        mDrumLoopOutput.SetLoop( true );
        mDrumLoopOutput.SetVIM( this );
    }
    /** @} */

    /*************************************************************************//** 
    * @defgroup VIMCoroutines Coroutines
    * @ingroup VIM
    * These are functions that are started by the VirtualInstrumentManager which allows it to perform other tasks while waiting.
    *****************************************************************************/
    /** @{ */

    /** 
     * @brief Coroutine to load the drum kit. 
     * @param[out] aDrumKitCallback Callback to allow returning the loaded instrument to the calling context.
     * 
     * @see DrumKit Music::INSTRUMENT_TYPE VirtualInstrumentManager::mDrumKit
    */ 
    private IEnumerator LoadDrumKit( System.Action<VirtualInstrument> aDrumKitCallback )
    {
        // Load a new instrument
        VirtualInstrument returned = new DrumKit( this );

        // Return the loaded instrument and invoke the instrument's LoadEvent.
        aDrumKitCallback( returned );
        //DrumKitLoaded.Invoke();
        yield return null;
    }

    /** 
     * @brief Coroutine to load an instrument. 
     * @param[in] aINSTRUMENT_TYPE The type of instrument to load. 
     * @param[out] aInstrumentCallback Callback to allow returning the loaded instrument to the calling context.
     * 
     * @see VirtualInstrument Music::INSTRUMENT_TYPE VirtualInstrumentManager::mInstrument
    */
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
    /** @} */

    /*************************************************************************//** 
    * @defgroup VIMHandlers Event Handlers
    * @ingroup VIM
    * These are functions that the VirtualInstrumentManager uses to handle invoked @link VIMEvents events @endlink
    *****************************************************************************/
    /** @{ */

    /**
     * @brief Called whenever an instrument should be loaded. 
     * @param[in] aNewInstrumentType The type of instrument that should be loaded
     * 
     * @see Music::INSTRUMENT_TYPE VirtualInstrumentManager::ChangeInstrumentEvent
    */
    public void OnChangeInstrumentEvent( Music.INSTRUMENT_TYPE aNewInstrumentType )
    {
        // Mark that we're not ready for note events.
        mReady = false;
        mInstrument = null;

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

    /**
     * @brief Called whenever the range of active notes should be changed.
     * @param[in] aNewLowestPitch The lowest pitch in the new range.
     * 
     * @see Music::PITCH VirtualInstrumentManager::ChangeNoteRangeEvent
    */
    public void OnChangeNoteRangeEvent( Music.PITCH aNewLowestPitch )
    {
        if( mReady )
        {
            mReady = false;

            // Make sure that we can change to the note range given.
            int newHighestIndex = (int)aNewLowestPitch + mNumActiveNotes - 1;
            if( !mInstrument.IsPitchSupported( aNewLowestPitch ) || !mInstrument.IsPitchSupported( (Music.PITCH)newHighestIndex ) )
            {
                Assert.IsTrue( false, "Tried to change the note range to include notes that aren't supported by the instrument!" );
                return;
            }

            // Set the active note variables accordingly.
            mLowestActiveNote = aNewLowestPitch;
            mHighestActiveNote = (Music.PITCH)( (int)aNewLowestPitch + mNumActiveNotes - 1 );
            for( int i = 0; i < mNumActiveNotes; i++ )
            {
                mActiveNotes[i] = (Music.PITCH)( (int)aNewLowestPitch + i );
            }

            // Load the note output objects.
            LoadNoteOutputObjects();
        }

    }

    /**
     * @brief Called whenever the instrument has finished loading. It begins loading the NoteOutputObjects. 
     * 
     * @see VirtualInstrumentManager::InstrumentLoadedEvent NoteOutputObject
    */
    public void OnInstrumentLoaded()
    {
        Assert.IsNotNull( mInstrument, "OnInstrumentLoaded was called, but the instrument was not loaded." );

        LoadNoteOutputObjects();
    }

    /**
     * @brief Called whenever the echo filter should be modified.
     * @param[in] aParameters The new parameters of the echo filter
     * 
     * @see VirtualInstrumentManager::ModifyEchoFilterEvent
    */
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

    /**
     * @brief Called whenever the reverb filter should be modified.
     * @param[in] aParameters The new parameters of the reverb filter
     * 
     * @see VirtualInstrumentManager::ModifyReverbFilterEvent
    */
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

    /**
     * @brief Handles pausing a drum loop.
    */
    public void OnPauseDrumLoopEvent()
    {
        mDrumLoopOutput.PauseAudio();
    }

    /**
     * @brief Handles pausing a song.
    */
    public void OnPauseSongEvent()
    {
        mSongOutput.PauseAudio();
    }

    /** 
     * @brief Begins playing a drum loop. It creates an array of audio data from each note of the Song and sends it to the right NoteOutputObject.
     * @param[in] aLoop The drum loop to play.
     * 
     * This function begins playing drum loops which are handled differently from other songs. 
     * Special handling of offsets and the number of waveform samples in the loop is required in
     * order to make the loop as seamless as possible.
     *  
     */
    public void OnPlayDrumLoopEvent( Song aLoop )
    {
        // Get the song's note data.
        Song.CombinedNoteData[] loopNoteData = aLoop.GetNoteData();

        // Get the number of notes in the loop.
        int numNotes = loopNoteData.Length;

        // Get the number of waveform samples in the loop.
        int numSamples = loopNoteData[numNotes - 1].TotalOffset;

        // Get the start index for the loop.
        int startIndex = loopNoteData[0].TotalOffset;

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
            // In order to loop, the offset for the last note is set to 0.
            if( i == numNotes - 1 )
            {
                offset = 0;
            }
            else
            {
                offset = loopNoteData[i].TotalOffset;
            }
            
            // Add the audio data for all of the drums in the note.
            for( int j = 0; j < loopNoteData[i].PercussionData.Hits.Length; j++ )
            {
                // Get the velocity from the note.
                velocity = loopNoteData[i].PercussionData.Velocities[j];

                // Get the associated dynamics index and velocity factor.
                velIndex = mDrumKit.GetBuiltInDynamicsThresholdIndex( velocity );
                velFactor = mDrumKit.GetAdjustedVelocityFactor( velocity );

                // Get the audio data for the note.
                float[][] drumAudioData = mDrumKit.GetAudioDataForPitch( (Music.PITCH)( loopNoteData[i].PercussionData.Hits[j] ) );

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

                // Reset the drum audio data.
                drumAudioData = null;
            }
        }

        // Set the song output object's audio data and begin playing the song.
        mDrumLoopOutput.SetAudioData( loopAudioData, mMixer, null );
        mDrumLoopOutput.SetLoop( true );
        mDrumLoopOutput.BeginPlaying( 1f, 0, startIndex );

        loopNoteData = null;
        loopAudioData = null;
    }

    /**
     * @brief Called whenever a note should be played. Uses the appropriate NoteOutputObject to play the note.
     * @param[in] aNoteToPlay The pitch of the note to play
     * @param[in] aVelocity The velocity at which to play the pitch.
     * 
     * @see VirtualInstrumentManager::PlayNoteEvent Music::PITCH NoteOutputObject::BeginPlaying
    */
    public void OnPlayNoteEvent( Music.PITCH aNoteToPlay, int aVelocity )
    {
        if( mReady )
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
                        mOutputs[(int)Music.DRUM.HIHAT_O].BeginPlaying( 0, 0 );
                        mOutputs[(int)Music.DRUM.HIHAT_P].BeginPlaying( 0, 0 );
                        break;
                    case Music.DRUM.HIHAT_O:
                        mOutputs[(int)Music.DRUM.HIHAT_C].BeginPlaying( 0, 0 );
                        mOutputs[(int)Music.DRUM.HIHAT_P].BeginPlaying( 0, 0 );
                        break;
                    case Music.DRUM.HIHAT_P:
                        mOutputs[(int)Music.DRUM.HIHAT_C].BeginPlaying( 0, 0 );
                        mOutputs[(int)Music.DRUM.HIHAT_O].BeginPlaying( 0, 0 );
                        break;
                    default:
                        break;
                }
            }

            // Get the built-in dynamics index and the velocity factor.
            int velIndex = mInstrument.GetBuiltInDynamicsThresholdIndex( aVelocity );
            float velFactor = mInstrument.GetAdjustedVelocityFactor( aVelocity );

            // Begin playing the note.
            mOutputs[noteIndex].BeginPlaying( velFactor, velIndex, 0 );
        }

    }

    /** 
     * @brief Begins playing a song. It creates an array of audio data from each note of the Song and sends it to the right NoteOutputObject.
     * @param[in] aSong The song to play.  
    */
    public void OnPlaySongEvent( Song aSong )
    {
        // Queue the song if needed.
        if( mQueuedSong != aSong )
        {
            OnQueueSongEvent( aSong );
        }

        // Play the song.
        mSongOutput.BeginPlaying( 1f, 0 );
    }

    /**
     * @brief Called whenever a song needs to be queued through a QueueSongEvent.
     * @param[in] aSong The song that needs to be queued.
    */
    public void OnQueueSongEvent( Song aSong )
    {
        // Get the song's note data.
        Song.CombinedNoteData[] songNoteData = aSong.GetNoteData();

        // Get the number of notes in the song.
        int numNotes = songNoteData.Length;

        // Get the total number of samples in the song.
        int numSamples = 0;

        // If the song is not a drum loop, then base the number of samples off of the pitches in the song.
        if( aSong.GetSongType() != Song.SongType.DrumLoop )
        {
            for( int i = 0; i < numNotes; i++ )
            {
                if( songNoteData[i].MelodyData.Pitches != null )
                {
                    for( int j = 0; j < songNoteData[i].MelodyData.Pitches.Length; j++ )
                    {
                        numSamples = Mathf.Max( ( songNoteData[i].MelodyData.NumSamples[j] + songNoteData[i].TotalOffset ), numSamples );
                    }
                }
                else
                {
                    numSamples = Mathf.Max( songNoteData[i].TotalOffset, numSamples );
                }
            }
        }
        // If the song is a drum loop, then base the number of samples off of the drums in the song.
        else
        {
            numSamples = 0;
            for( int i = 0; i < songNoteData[numNotes - 1].PercussionData.Hits.Length; i++ )
            {
                if( numSamples < ( songNoteData[numNotes - 1].TotalOffset +
                    mDrumKit.GetAudioDataForPitch( (Music.PITCH)( songNoteData[numNotes - 1].PercussionData.Hits[i] ) )[0].Length ) )
                {
                    numSamples = songNoteData[numNotes - 1].TotalOffset +
                        mDrumKit.GetAudioDataForPitch( (Music.PITCH)( songNoteData[numNotes - 1].PercussionData.Hits[i] ) )[0].Length;
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

                // Add the audio data for all of the pitches in the note.
                for( int j = 0; j < songNoteData[i].MelodyData.Pitches.Length; j++ )
                {
                    // Get the pitch's velocity.
                    velocity = songNoteData[i].MelodyData.Velocities[j];

                    // Get the associated dynamics index and velocity factor.
                    velIndex = mInstrument.GetBuiltInDynamicsThresholdIndex( velocity );
                    velFactor = mInstrument.GetAdjustedVelocityFactor( velocity );

                    // Get the audio data for the note.
                    float[][] pitchAudioData = mInstrument.GetAudioDataForPitch( songNoteData[i].MelodyData.Pitches[j] );

                    // Put all of the samples from the pitch's audio data into the song audio data.
                    int k = 0;
                    while( k < songNoteData[i].MelodyData.NumSamples[j] && k < pitchAudioData[velIndex].Length )
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

                    // Cleanup
                    pitchAudioData = null;
                }


            }

            // If the note has drums, then add them.
            if( songNoteData[i].PercussionData.Hits != null )
            {
                // Get the offset and velocity from the note.
                offset = songNoteData[i].TotalOffset;

                // Add the audio data for all of the drums in the note.
                for( int j = 0; j < songNoteData[i].PercussionData.Hits.Length; j++ )
                {
                    // Get the drum's velocity
                    velocity = songNoteData[i].PercussionData.Velocities[j];

                    // Get the associated dynamics index and velocity factor.
                    velIndex = mDrumKit.GetBuiltInDynamicsThresholdIndex( velocity );
                    velFactor = mDrumKit.GetAdjustedVelocityFactor( velocity );

                    // Get the audio data for the note.
                    float[][] drumAudioData = mDrumKit.GetAudioDataForPitch( (Music.PITCH)( songNoteData[i].PercussionData.Hits[j] ) );

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

                    // Cleanup
                    drumAudioData = null;
                }
            }
        }

        // Set the song output object's audio data and begin playing the song.
        mSongOutput.SetAudioData( songAudioData, mMixer, null );

        // Cleanup
        songAudioData = null;
        Resources.UnloadUnusedAssets();
    }

    /**
     * @brief Called whenever a note should be released. Uses the appropriate NoteOutputObject to release the note.
     * @param[in] aNoteToRelease The pitch of the note to release
     * 
     * @see VirtualInstrumentManager::PlayNoteEvent Music::PITCH NoteOutputObject::BeginRelease
    */
    public void OnReleaseNoteEvent( Music.PITCH aNoteToRelease )
    {
        // Don't account for note releases for drums.
        if( mInstrumentType != Music.INSTRUMENT_TYPE.DRUM_KIT )
        {
            Assert.IsTrue( (int)aNoteToRelease >= (int)mLowestActiveNote && (int)aNoteToRelease <= (int)mHighestActiveNote,
                "Tried to fade out the note " + Music.NoteToString( aNoteToRelease ) + " which is not active!" );
            int noteIndex = (int)aNoteToRelease - (int)mLowestActiveNote;
            mOutputs[noteIndex].BeginRelease();
        }
    }

    /**
     * @brief Handles resuming a drum loop.
    */
    public void OnResumeDrumLoopEvent()
    {
        mDrumLoopOutput.ResumeAudio();
    }

    /**
     * @brief Handles resuming a song.
    */
    public void OnResumeSongEvent()
    {
        mSongOutput.ResumeAudio();
    }

    /**
     * @brief Handles stopping a drum loop.
    */
    public void OnStopDrumLoopEvent()
    {
        mDrumLoopOutput.StopAudio();
    }

    /**
     * @brief Handles stopping a song.
    */
    public void OnStopSongEvent()
    {
        mSongOutput.StopAudio();
    }


    /** @} */
}
