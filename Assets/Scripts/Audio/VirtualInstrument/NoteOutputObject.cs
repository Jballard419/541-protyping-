using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Assertions;

/**
 * @class NoteOutputObject
 * @brief Unity GameObject that handles the actual output of sound.
 * 
 * This class handles the sound output for @link Song Songs@endlink and @link VirtualInstrumentManager::PlayNoteEvent Notes@endlink.
 * @link VirtualInstrumentManager::mOutputs Copies of this object@endlink are @link VirtualInstrumentManager::LoadNoteOutputObjects dynamically created@endlink 
 * by the @link VIM Virtual Instrument Manager@endlink for each @link VirtualInstrumentManager::mActiveNotes note that can be played@endlink.
 * Separate copies are also created to @link VirtualInstrumentManager::mSongOutput handle playing Songs@endlink
 * and @link VirtualInstrumentManager::mDrumLoopOutput drum loops@endlink.
 * @n This class should only be handled by the @link VIM Virtual Instrument Manager@endlink.
*/
public class NoteOutputObject : MonoBehaviour
{
    /*************************************************************************//** 
    * @defgroup NOOPrivVar Private Variables
    * @ingroup DocNOO
    * These are variables used internally by the NoteOutputObject.
    * @{
    *****************************************************************************/
    #if DEBUG_AUDIO_DIAGNOSTICS
        private ATI_Diagnostics mDiagnosticsHandler = null; //!< The audio diagnostics handler.
    #endif
    private AudioSource                mSource; //!< The AudioSource component of this object
    private bool                       mAudioDataBeingUsed; //!< Whether or not OnAudioFilterRead is currently using the audio data
    private bool                       mLoaded; //!< Whether or not this object has loaded.
    private bool                       mLoop; //!< Whether or not the audio should loop.
    private bool                       mNewNote; //!< Whether or not a new note needs to be started.
    private bool                       mNotifyWhenFinished; //!< Should it notify the parent when the audio has finished playing?
    private bool                       mPaused; //!< Is the audio paused?
    private bool                       mResume; //!< Whether or not the audio should resume from its previous position.
    private bool                       mNotePlaying; //!< Whether or not the note is currently playing.
    private bool                       mNoteRelease; //!< Whether or not the note has been released.
    #if DEBUG_AUDIO_DIAGNOSTICS
        private bool mReported = false; //!< Whether or not we have reported to the audio diagnostics handler.
    #endif
    private float                      mNewNoteVelocityFactor; //!< The velocity of a new note mapped to the range [0,1]
    private float                      mVelocityFactor; //!< A percentage mapping a given velocity to the output volume
    private float[][]                  mAudioData; //!< A container for raw audio data.
    private int                        mCounter; //!< A counter to keep track of the current position in the raw audio data.
    private int                        mDynamicsIndex; //!< The index corresponding to which built-in dynamics value is currently in use.
    private int                        mNewNoteDynamicsIndex; //!< The dynamics index of the new note.
    private int                        mNewNoteStartIndex; //!< The index from which to start playing the audio.
    private int                        mNumBuiltInDynamics; //!< The number of built-in dynamics values.
    private int[]                      mBuiltInDynamicsThresholds; //!< The thresholds that map a velocity to a built-in dynamics value.
    private int[]                      mEndSampleIndices; //!< The indices corresponding to the last sample in the audio data.
    private VirtualInstrumentManager   mVIM = null; //!< The parent VirtualInstrumentManager.

    /*************************************************************************//** 
    * @}
    * @defgroup NOOUnity Unity Functions
    * @ingroup DocNOO
    * These are functions called automatically by Unity.
    * @{
    *****************************************************************************/

    /**
     * @brief Called when the object is created. This function creates an audio source and sets the initial values for each variable.
    */
    private void Awake()
    {
        // Set the values of the member variables
        mAudioDataBeingUsed = false;
        mLoaded = false;
        mCounter = 0;
        mEndSampleIndices = null;
        mAudioData = null;
        mNewNote = false;
        mResume = false;
        mNumBuiltInDynamics = 0;
        mDynamicsIndex = 0;
        mVelocityFactor = 1f;
        mNoteRelease = false;
        mLoop = false;
        mNotifyWhenFinished = false;

        // Destroy the audio source if it already exists.
        mSource = gameObject.GetComponent<AudioSource>();
        if( mSource != null )
        {
            DestroyImmediate( mSource, false );
        }

        // Add a new audio source to this object and set its values.
        mSource = gameObject.AddComponent<AudioSource>();
        mSource.enabled = true;
        mSource.playOnAwake = false;
        mSource.minDistance = 0f;
        mSource.maxDistance = 0.01f;
    }

    /*************************************************************************//** 
    * @}
    * @defgroup NOOPubFunc Public Functions
    * @ingroup DocNOO
    * These are functions other classes use to interact with the NoteOutputObject.
    * @{
    *****************************************************************************/

    /**
    * @brief Handler for setting the note to be played. Should be called from @link VirtualInstrumentManager::PlayNoteEvent the manager's PlayNote event@endlink.
    * @param[in] aVelocityFactor The adjusted velocity of the note to be played. Ranges from 0 (silent) to 1.0 (max volume).
    * @param[in] aDynamicsIndex the index of the built-in dynamics.
    * @param[in] aStartIndex Where the note/song/loop should begin playing. Defaults to 0.
    * 
    * This function signals to OnAudioFilterRead that it should begin playing audio. 
    * In order to avoid threading issues, the signalling mechanism functions somewhat 
    * like a Moore state machine where it'll have an effect on the next cycle of OnAudioFilterRead.
    */
    public void BeingPlaying( float aVelocityFactor, int aDynamicsIndex, int aStartIndex = 0 )
    {
        Assert.IsTrue( aVelocityFactor <= 1f, "NoteOutputObject was given a velocity factor greater than 1!" );

        if( mLoaded )
        {
            mNewNoteVelocityFactor = aVelocityFactor;
            mNewNoteDynamicsIndex = aDynamicsIndex;
            mNewNoteStartIndex = aStartIndex;
            mNewNote = true;
            mNoteRelease = false;
            mPaused = false;
        }
    }

    /**
     * @brief Marks that the audio should begin fading out. 
     * 
     * This should only be called from @link VIM the manager's@endlink @link VirtualInstrumentManager::ReleaseNote release note event@endlink.
    */
    public void BeginRelease()
    {
        if( mLoaded && mNotePlaying )
        {
            // Set that the note should fade out. 
            // Actually processing the fade out will be handled by the onAudioFilterRead function.
            mNoteRelease = true;
        }
    }

    /**
     * @brief Gets whether or not the audio should loop.
     * @return True if the audio should loop. False otherwise.
    */
    public bool GetLoop()
    {
        return mLoop;
    }

    /** 
     * @brief Pauses the audio.
     * 
     * @see VirtualInstrumentManager::PauseSongEvent VirtualInstrumentManager::PauseDrumLoopEvent
    */
    public void PauseAudio()
    {
        mPaused = true;
    }

    /**
     * @brief Resumes the audio.
     * 
     * @see VirtualInstrumentManager::ResumeSongEvent VirtualInstrumentManager::ResumeDrumLoopEvent
    */
    public void ResumeAudio()
    {
        mPaused = false;
        mNotePlaying = true;
    }

    /**
     * @brief Sets the audio data for this object
     * @param[in] aAudioData The raw audio data for each @link DefBID Built-In Dynamic@endlink if supported by the instrument. It is just the raw audio data for a single @link Music::PITCH@endlink otherwise.
     * @param[in] aMixer The audio mixer to route the audio output to.
     * @param[in] aThresholds The @link DefBIDThresh Built-In Dynamics thresholds@endlink if supported by the instrument. Default is null for instruments that don't support @link DefBID Built-In Dynamics@endlink.
    */
    public void SetAudioData( float[][] aAudioData, AudioMixer aMixer, int[] aThresholds = null )
    {
        mLoaded = false;

        while( mAudioDataBeingUsed ) ;

        // Remove any existing audio data.
        RemoveAudioData();

        // Set the values related to built-in dynamics if necessary. 
        if( aThresholds != null )
        {
            mNumBuiltInDynamics = aThresholds.Length;
            mBuiltInDynamicsThresholds = new int[mNumBuiltInDynamics];
            for( int i = 0; i < aThresholds.Length; i++ )
            {
                mBuiltInDynamicsThresholds[i] = aThresholds[i];
            }
        }
        else
        {
            mBuiltInDynamicsThresholds = null;
            mDynamicsIndex = 0;
            mNumBuiltInDynamics = 0;
            mCounter = 0;
        }

        // Set the output mixer.
        mSource.outputAudioMixerGroup = aMixer.FindMatchingGroups( "Master" )[0];

        // Initialize the audio data array and copy the values from the given parameter.
        // If we don't have to worry about built in dynamics, then use hard-coded indices 
        // for the outer array.
        if( mNumBuiltInDynamics == 0 )
        {
            mAudioData = new float[1][];
            mAudioData[0] = new float[aAudioData[0].Length];
            mEndSampleIndices = new int[1];
            mEndSampleIndices[0] = aAudioData[0].Length - 1;
            for( int i = 0; i < aAudioData[0].Length; i++ )
            {
                mAudioData[0][i] = aAudioData[0][i];
            }
        }
        // If there are built in dynamics, then iterate through each one.
        else
        {
            mAudioData = new float[mNumBuiltInDynamics][];
            mEndSampleIndices = new int[mNumBuiltInDynamics];
            // int bufferLength = 0;
            for( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                mAudioData[i] = new float[aAudioData[i].Length];
                mEndSampleIndices[i] = aAudioData[i].Length - 1;
                for( int j = 0; j < mAudioData[i].Length; j++ )
                {
                    mAudioData[i][j] = aAudioData[i][j];
                }
            }
        }

        // Mark that this object is loaded. 
        mLoaded = true;
    }

    /**
     * @brief Sets whether or not the audio should loop.
     * @param[in] aLoop Whether or not the audio should loop.
    */
    public void SetLoop( bool aLoop )
    {
        mLoop = aLoop;
    }

    /**
     * @brief Sets the parent VirtualInstrumentManager and sets up diagnostics if needed.
     * @param[in] aVIM The parent VirtualInstrumentManager.
    */
    public void SetVIM( VirtualInstrumentManager aVIM )
    {
        mVIM = aVIM;
        #if DEBUG_AUDIO_DIAGNOSTICS
            mDiagnosticsHandler = mVIM.GetDiagnosticsHandler();
            mReported = false;
        #endif
    }

    /** 
     * @brief Sets whether or not an others should be notified when the audio has finished playing.
     * @param[in] aShouldNotify Whether or not others should be notified when the audio has finished playing
    */
    public void ShouldNotifyWhenFinished( bool aShouldNotify )
    {
        mNotifyWhenFinished = aShouldNotify;
    }

    /**
     * @brief Stops playing the audio.
    */
    public void StopAudio()
    {
        mNotePlaying = false;
        mPaused = false;
    }

    /*************************************************************************//** 
    * @}
    * @defgroup NOOPrivFunc Private Functions
    * @ingroup DocNOO
    * These are functions that are used internally by the NoteOutputObject.
    * @{
    *****************************************************************************/

    /**
     * @brief Removes the audio data and resets relevant variables to default values.
    */
    private void RemoveAudioData()
    {
        mLoaded = false;
        mNoteRelease = false;
        mNewNote = false;
        mNotePlaying = false;

        // Remove the audio data array.
        if( mAudioData != null )
        {
            if( mNumBuiltInDynamics == 0 )
            {
                mAudioData[0] = null;
            }
            else
            {
                for( int i = 0; i < mNumBuiltInDynamics; i++ )
                {
                    mAudioData[i] = null;
                }
            }
            mAudioData = null;
        }

        // Set relevant variables to default values. This isn't a destructor, it's just
        // a function to give the output object a clean slate. 
        mEndSampleIndices = null;
        mCounter = 0;
        mBuiltInDynamicsThresholds = null;
        mNumBuiltInDynamics = 0;

        // Clean up.
        GC.Collect();
    }

    /*************************************************************************//** 
    * @}
    * @defgroup NOOHandlers Event Handlers
    * @ingroup DocNOO
    * These are functions that are automatically called when an event is invoked.
    * @{
    *****************************************************************************/

    /**
     * @brief Handler that is called whenever the audio buffer is refilled. 
     * @param[inout] data The raw audio data that will be played. This will be replaced by the function with a section of the audio data if needed. 
     * @param[in] channels The number of channels in the audio data. Not too relevent at this moment.
     * 
     * If the audio is playing, then this handler will pass the appropriate section
     * of the raw audio data to the buffer which will cause the sound to actually be played. 
     * It also keeps track of the position in the audio data, so that the sections are iterated through.
     * @n This handler is automatically called whenever the buffer needs to be refilled,
     * which is at intervals of ~23ms.
    */
    private void OnAudioFilterRead( float[] data, int channels )
    {
        // Only generate the sound if it's loaded.
        if( mLoaded )
        {
            mAudioDataBeingUsed = true;
            // Check for a new note.
            if( mNewNote )
            {
                // Handle starting a new note by setting the relevant member variables
                mVelocityFactor = mNewNoteVelocityFactor;
                mDynamicsIndex = mNewNoteDynamicsIndex;
                if( !mResume )
                {
                    mCounter = mNewNoteStartIndex;
                }
                mNewNote = false;
                mNoteRelease = false;
                mNotePlaying = true;
                mResume = false;
                #if DEBUG_AUDIO_DIAGNOSTICS
                    mReported = false;
                #endif
            }
            // Check for a note release.
            else if( mNoteRelease )
            {
                // If the note has been released, then set the velocity factor to 
                // decrease each time this function is called.
                mVelocityFactor -= ( 1f / 100f );
            }
            // Check for pausing the audio.
            else if( mPaused )
            {
                mNotePlaying = false;
            }
            if( mNotePlaying )
            {
                // If the note hasn't faded out, then play it.
                if( mVelocityFactor > 0 )
                {
                    // See if we should loop or not.
                    if( mLoop )
                    {
                        // Retrieve the audio data.
                        for( int i = 0; i < data.Length; i++ )
                        {
                            if( mCounter == mEndSampleIndices[mDynamicsIndex] )
                            {
                                mCounter = 0;
                            }
                            data[i] = mAudioData[mDynamicsIndex][mCounter] * mVelocityFactor;
                            mCounter++;
                        }
                    }
                    // If we shouldn't loop, then make sure that we stop playing right before the end index.
                    else
                    {
                        // If we're currently playing a note then retrieve the audio data. 
                        for( int i = 0; i < data.Length && ( mCounter + i ) < mEndSampleIndices[mDynamicsIndex]; i++ )
                        {
                            data[i] = mAudioData[mDynamicsIndex][mCounter + i] * mVelocityFactor;
                        }

                        // If we've reached the end of the audio data, then the note is no longer playing so
                        // we should reset some variables.
                        if( mCounter + data.Length >= mEndSampleIndices[mDynamicsIndex] )
                        {
                            if( mNotifyWhenFinished )
                            {
                                mVIM.AudioFinished.Invoke();
                            }
                            mCounter = 0;
                            mNotePlaying = false;
                            mNoteRelease = false;
                        }
                        // If we haven't reached the end of the audio data yet, then increase the counter.
                        else
                        {
                            mCounter += data.Length;
                        }

                        #if DEBUG_AUDIO_DIAGNOSTICS
                            // Notify the diagnostics.
                            if( mDiagnosticsHandler != null && !mReported )
                            {
                                mReported = true;
                                mDiagnosticsHandler.SetOutputTime.Invoke();
                            }
                        #endif
                    }

                }
                // If the note has faded out, then the note is no longer playing so
                // we should reset some variables.
                else
                {
                    mCounter = 0;
                    mNotePlaying = false;
                    mNoteRelease = false;
                }
            }
            mAudioDataBeingUsed = false;
        }
    }
    /** @} */
}
