using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * @class VirtualInstrument
 * @brief A base class that contains generic functions and values relating to @link VI Virtual Instruments@endlink.
 * 
 * The base VirtualInstrument class performs tasks such as retrieving raw audio data from 
 * wav files and note mapping. This is a C# class rather than a Unity-specific class, so it
 * is not attached to any GameObject itself. This is because it should only interact with the
 * @link VIM Virtual Instrument Manager@endlink, and other classes should go through the 
 * @link VIM manager@endlink to get information about the instrument. This hiding of information
 * is done for ease of using the audio code with other parts of the program by making all interactions
 * with the audio go through a common hub.
 * 
 * @n This class is meant to be the base class for implementing specific instruments. It is not
 * meant to be used by itself as anything other than a container that is checked at runtime.
 * Also, every @link VI Virtual Instrument@endlink is meant to be used as-is, so there 
 * is no modification that can be done on this class by other classes that do not inherit it.
 * 
 * @nosubgrouping
 */
public class VirtualInstrument
{
    /*************************************************************************//** 
     * @defgroup VIBaseConst Constants
     * @ingroup VIBase
     * Constants used in order to set attributes of the @link VI Virtual Instrument@endlink.
     * @{
    *****************************************************************************/
    public static float SAMPLE_INTERVAL = 1.1337e-5f; //!< The number of seconds between waveform samples.
    private const float NORMALIZED_PEAK = -.1f; //!< The peak of the waveform after it's normalized.

    /*************************************************************************//** 
     * @defgroup VIBaseProVar Protected Variables
     * @ingroup VIBase
     * Variables that will be implemented by subclasses.
     * @{
    *****************************************************************************/
    protected bool                               mIsDrum; //!< Whether or not the virtual instrument is a @link DrumKit drum kit@endlink. 
    protected bool                               mLoaded; //!< Whether or not the virtual instrument is loaded.
    protected float                              mSampleInterval; //!< The waveform sample interval of the virtual instrument
    protected float[][][]                        mAudioData; //!< The waveform samples of the virtual instrument. The indices are mapped according to [@link DefBID BuiltInDynamicsIndex@endlink][@link Music::PITCH Pitch@endlink][WaveformSample].
    protected int                                mNumFiles; //!< The number of sample files
    protected int                                mNumBuiltInDynamics; //!< The number of @link DefBID Built-in Dynamics@endlink.
    protected int                                mNumSupportedPitches; //!< The number of @link Music::PITCH pitches@endlink supported by the instrument.
    protected int                                mSampleRate; //!< The sample rate of the virtual instrument
    protected int[]                              mBuiltInDynamicsThresholds; //!< The @link DefBIDThresh thresholds@endlink for when to use a specific sound file
    protected Music.PITCH                        mLowestSupportedPitch; //!< The lowest supported @link Music::PITCH pitch@endlink of the instrument
    protected Music.PITCH                        mHighestSupportedPitch; //!< The highest supported @link Music::PITCH pitch@endlink of the instrument
    protected string                             mFilepath; //!< The base filepath for the samples.
    protected string[]                           mFilenames; //!< An array of filenames for the samples.
    protected string[]                           mBuiltInDynamics; //!< The names of the @link DefBID Built-In Dynamics@endlink.
    protected VirtualInstrumentManager           mParent; //!< The @link VIM manager@endlink for this instrument

    /*************************************************************************//** 
     * @}
     * @defgroup VIBaseConstruct Constructors
     * @ingroup VIBase
     * Constructors to create the @link VirtualInstrument Virtual Instrument@endlink.
     * @{
    *****************************************************************************/

    /**
     * @brief Generic constructor. Sets default values and values that are common to all @link VI Virtual Instruments@endlink. 
     * @param[in] aParent The parent @link VIM manager@endlink for this instrument.
     *  
     * Specific @link VI Virtual Instruments@endlink (such as the Piano) have their own constructor that is more
     * specialized, but this will be called for each child class as well. 
     */
    public VirtualInstrument( VirtualInstrumentManager aParent )
    {
        mParent = aParent;
    }

    /*************************************************************************//** 
     * @}
     * @defgroup VIBasePubFunc Public Functions
     * @ingroup VIBase
     * Functions for other classes (usually the @link VIM parent manager@endlink) to get values from the instrument.
     * @{
     ****************************************************************************/

    /** 
     * @brief Gets the index for which sound file to use for a @link Music::PITCH pitch@endlink at a given @link DefVel velocity@endlink.
     * @param[in] aVelocity The given @link DefVel velocity@endlink from 0 (silent) to 100 (max volume).
     * @return The index of the sound file to use.
     * 
     * For instruments with @link DefBID Built-In Dynamics@endlink (such as the Piano), this function takes a @link DefVel velocity@endlink
     * and returns the index of the sound file to use for playing the sound. 
     * If a function does not support @link DefBID Built-In Dynamics@endlink, this function returns 0.
     * 
     * @see @link DefBIDThresh Built-In Dynamics Thresholds@endlink.
     *  */
    public int GetBuiltInDynamicsThresholdIndex( int aVelocity )
    {
        // If there aren't any built-in dynamics, then just return 0.
        if( mNumBuiltInDynamics == 0 )
        {
            return 0;
        }
        // If there are built-in dynamics, then start from the highest and work down to find
        // which one to use. 
        else
        {
            int dynamicsIndex = 0;
            for( int i = mNumBuiltInDynamics - 1; i > -1; i-- )
            {
                if( aVelocity <= mBuiltInDynamicsThresholds[i] )
                {
                    dynamicsIndex = i;
                }
            }
            return dynamicsIndex;
        }
    }

    /**
     * @brief Gets the ranges for which sound file to use for a @link DefVel velocity@endlink. 
     * @return If the instrument supports @link DefBID Built-In Dynamics@endlink, then return its @link DefBIDThresh thresholds@endlink. Return null otherwise.
     */
    public int[] GetBuiltInDynamicsThresholds()
    {
        return mBuiltInDynamicsThresholds;
    }


    /**
     * @brief Gets the factor by which a note's waveform is scaled in order to account for @link DefVel velocity@endlink.
     * @param[in] aVelocity The @link DefVel velocity@endlink for the note from 0 (silent) to 100 (max volume).
     * @return A float between 0.0 and 1.0 where 0.0 means that the waveform should be made silent and 1.0 means that it should be unaltered.
     * 
     * Instruments without @link DefBID Built-In Dynamics@endlink just divide the given @link DefVel velocity@endlink by 100, but instruments
     * with @link DefBID Built-in Dynamics@endlink map the factor to a ratio involving the @link DefBIDThresh Built-In Dynamics thresholds@endlink
     * so that the volumes for @link DefVel velocities@endlink near a @link DefBIDThresh threshold@endlink don't have significant differences.
     */
    public float GetAdjustedVelocityFactor( int aVelocity )
    {
        int dynamicsIndex = 0;
        float velocityFactor = aVelocity / 100f;

        // Calculate the velocity multiplier. The multiplier is a percentage that is used to adjust the
        // levels of the audio data to modify the output volume. 
        // If built-in dynamics are supported, then the velocity multiplier will range from the lower threshold divided by the higher threshold to 1.0.
        if( mNumBuiltInDynamics != 0 )
        {
            // See which built-in dynamics value we need to use. Start at the top threshold and work down.
            for( int i = mNumBuiltInDynamics - 1; i > -1; i-- )
            {
                if( aVelocity <= mBuiltInDynamicsThresholds[i] )
                {
                    dynamicsIndex = i;
                }
            }
            // Calculate the velocity factor.
            float inEnd = (float)mBuiltInDynamicsThresholds[dynamicsIndex];
            float outEnd = 1f;
            float inStart = 0f;
            float outStart = 0f;
            if( dynamicsIndex != 0 )
            {
                inStart = (float)mBuiltInDynamicsThresholds[dynamicsIndex - 1];
                outStart = (float)mBuiltInDynamicsThresholds[dynamicsIndex - 1] / (float)mBuiltInDynamicsThresholds[dynamicsIndex];
            }

            velocityFactor = outStart + ( ( ( outEnd - outStart ) / ( inEnd - inStart ) ) * ( (float)aVelocity - inStart ) );
        }
        // If built-in dynamics are not supported, then just use the given velocity as a percentage. 
        return velocityFactor;
    }

    /**
     * @brief Gets the highest @link Music::PITCH pitch@endlink that this instrument can play.
     * @return The highest @link Music::PITCH pitch@endlink that this instrument can play.
     */
    public Music.PITCH GetHighestSupportedPitch()
    {
        return mHighestSupportedPitch;
    }

    /**
     * @brief Gets the lowest @link Music::PITCH pitch@endlink that this instrument can play.
     * @return The lowest @link Music::PITCH pitch@endlink supported by the instrument.
     */
    public Music.PITCH GetLowestSupportedPitch()
    {
        return mLowestSupportedPitch;
    }

    /**
     * @brief Gets the total number of @link Music::PITCH pitches@endlink that this instrument can play.
     * @return The total number of @link Music::PITCH pitches@endlink that the instrument can play.
     */
    public int GetNumOfSupportedPitches()
    {
        return mNumSupportedPitches;
    }

    /** 
     * @brief Gets the raw audio data of each built-in dynamics value for a given @link Music::PITCH pitch@endlink. 
     * @param[in] aPitch The @link Music::PITCH pitch@endlink for which the data is retrieved.
     * @return a 2-D array of floats where the indices correspond to [@link DefBID BuiltInDynamicsIndex@endlink][WaveformSample]. 
     * 
     * The array is retrived by mapping the given @link Music::PITCH pitch@endlink to the second index of the mAudioData member.
     */
    public float[][] GetAudioDataForPitch( Music.PITCH aPitch )
    {
        Assert.IsTrue( (int)aPitch >= (int)mLowestSupportedPitch && (int)aPitch <= (int)mHighestSupportedPitch,
            "Tried to load the audio data for the note " + Music.NoteToString( aPitch ) + " which is not a note that is supported by this instrument!" );
        Assert.IsNotNull( mAudioData, "Tried to get data from a non-loaded virtual instrument!" );

        // If we need to load the clip, then load it.
        if( mAudioData[0][(int)aPitch] == null )
        {
            LoadAudioClipForPitch( aPitch );
        }

        // Declare the returned array.
        float[][] data = null;

        // If there aren't any built-in dynamics, then use a hard-coded index and copy the data to the array from the audio clip.
        if( mNumBuiltInDynamics == 0 )
        {
            data = new float[1][];
            int dataLength = mAudioData[0][(int)aPitch].Length;
            data[0] = new float[dataLength];
            for( int i = 0; i < dataLength; i++ )
            {
                data[0][i] = mAudioData[0][(int)aPitch][i];
            }
        }
        // If there are built-in dynamics, then get the data from the audio clips for each corresponding file.
        else
        {
            data = new float[mNumBuiltInDynamics][];
            int dataLength = 0;
            for( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                dataLength = mAudioData[i][(int)aPitch].Length;
                data[i] = new float[dataLength];
                for( int j = 0; j < dataLength; j++ )
                {
                    data[i][j] = mAudioData[i][(int)aPitch][j];
                }
            }
        }

        return data;
    }

    /**
     * @brief Gets the time interval between waveform samples for this instrument
     * @return The time interval between waveform samples for this instrument
     */
    public float GetSampleInterval()
    {
        return mSampleInterval;
    }

    /**
     * @brief Gets the sample rate for the wav files used to create the instrument.
     * @return The sample rate of the wav files used to create the instrument.
     */
    public int GetSampleRate()
    {
        return mSampleRate;
    }

    /**
     * @brief Gets whether or not the instrument has loaded.
     * @return True if the instrument has loaded. False otherwise.
     */
    public bool IsLoaded()
    {
        return mLoaded;
    }

    /**
     * @brief Gets whether or not the instrument can play a specific @link Music::PITCH pitch@endlink.
     * @param[in] aPitch The @link Music::PITCH pitch@endlink that is being checked.
     * @return True if the @link Music::PITCH pitch@endlink can be played by the instrument. False otherwise.
     */
    public bool IsPitchSupported( Music.PITCH aPitch )
    {
        return ( (int)aPitch >= (int)mLowestSupportedPitch && (int)aPitch <= (int)mHighestSupportedPitch );
    }

    /*************************************************************************//** 
     * @}
     * @defgroup VIBaseProFunc Protected Functions
     * @ingroup VIBase
     * Functions that are used by the subclasses.
     * @{
     ****************************************************************************/


    /** 
     * @brief Loads all the audio clips associated with a pitch.
     * @param[in] aPitch The pitch to load the audio clips for.
    */   
    protected void LoadAudioClipForPitch( Music.PITCH aPitch )
    {
        Assert.IsTrue( aPitch >= mLowestSupportedPitch && aPitch <= mHighestSupportedPitch,
            "Tried to load the pitch " + Music.NoteToString( aPitch ) + ", but that's out of the instrument's range from " +
            Music.NoteToString( mLowestSupportedPitch ) + " to " + Music.NoteToString( mHighestSupportedPitch ) );

        // Get the file index.
        int fileIndex = (int)aPitch - (int)mLowestSupportedPitch;
        
        // Get the audio clips for the pitch.
        AudioClip[] clips = null;

        // If Built-In Dynamics are not supported, then just get the one clip.
        if( mNumBuiltInDynamics == 0 )
        {
            // Get the audio file.
            clips = new AudioClip[1];
            clips[0] = Resources.Load<AudioClip>( mFilenames[fileIndex] );
            Assert.IsNotNull( clips[0], "Failed to load audioclip from file " + mFilenames[fileIndex] );

            // Load the audio data.
            clips[0].LoadAudioData();

            // Normalize the clip.
            NormalizeAudioClipsForPitch( aPitch, clips );
        }
        // If Built-In Dynamics are supported, then get all of the associated clips.
        else
        {
            // Allocate space for the clips.
            clips = new AudioClip[mNumBuiltInDynamics];

            // Load the clips.
            for( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                // Load the wav file.
                clips[i] = Resources.Load<AudioClip>( mFilenames[fileIndex] );
                Assert.IsNotNull( clips[0], "Failed to load audioclip from file " + mFilenames[fileIndex] );

                // Load the audio data into the AudioClip.
                clips[i].LoadAudioData();

                // Go to the next file.
                fileIndex += mNumSupportedPitches;
            }

            // Normalize the clips.
            NormalizeAudioClipsForPitch( aPitch, clips );
        }

        // Clean up.
        clips = null;
        Resources.UnloadUnusedAssets();
    }

    /**
    * @brief Loads the audio clips for the instrument. VirtualInstrument::CreateFilenames must be called prior to this function in order to actually find the files. 
    * 
    * This function handles the loading of the audio clips, getting their audio data, and passing the audio data to
    * VirtualInstrument::NormalizeAudioClips().
    */
    protected void LoadAudioClips()
    {
        // Initialize index variables so that all of the files can be iterated through and appropriately 
        // assigned to an array of audio clips. 
        int index = (int)mLowestSupportedPitch;
        int fileIndex = 0;
        AudioClip[][] audioClips = null;
        // If there aren't any built-in dynamics, then use a hard-coded index of 0 for the outer array.
        if( mNumBuiltInDynamics == 0 )
        {
            // Initialize the array of audio clips. In order to account for instruments differing in the range of notes 
            // that they support, the inner audio clip array is set to have an element for every possible note. Unsupported
            // notes will have null audio clips at their indices. Drum kits will not have to worry about this.   
            audioClips = new AudioClip[1][];

            if( mIsDrum )
            {
                audioClips[0] = new AudioClip[Music.MAX_SUPPORTED_DRUMS];
            }
            else
            {
                audioClips[0] = new AudioClip[Music.MAX_SUPPORTED_NOTES];
            }


            // The indices of the loaded audio clips are mapped to their corresponding note. 
            while( index <= (int)mHighestSupportedPitch )
            {
                // Load the audio clip into the audio clip array.
                audioClips[0][index] = Resources.Load<AudioClip>( mFilenames[fileIndex] );
                Assert.IsNotNull( audioClips[0][index], "Failed to load audioclip from file " + mFilenames[fileIndex] );

                // Load the audio data for the audio clip
                audioClips[0][index].LoadAudioData();

                // Increment the index variables.
                index++;
                fileIndex++;
                Assert.IsTrue( fileIndex <= mNumFiles, "Tried to load more files than were available. Recheck how many files are availabled for the virtual instrument" );
            }
        }
        // If built-in dynamics are available for this instrument, then load audio clips for each built-in dynamics value.
        else
        {
            // Initializ the outer array of audio clips
            audioClips = new AudioClip[mNumBuiltInDynamics][];

            // Iterate through each outer array.
            for( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                // Initialize the inner array of audio clips. In order to account for instruments differing in the range of notes 
                // that they support, the inner audio clip array is set to have an element for every possible note. Unsupported
                // notes will have null audio clips at their indices. Drum kits will not have to worry about this.

                if( mIsDrum )
                {
                    audioClips[i] = new AudioClip[Music.MAX_SUPPORTED_DRUMS];
                }
                else
                {
                    audioClips[i] = new AudioClip[Music.MAX_SUPPORTED_NOTES];

                }

                // The indices of the loaded audio clips are mapped to their corresponding note. 
                while( index <= (int)mHighestSupportedPitch )
                {
                    // Load the audio clip into the audio clip array.
                    audioClips[i][index] = Resources.Load<AudioClip>( mFilenames[fileIndex] );
                    Assert.IsNotNull( audioClips[0][index], "Failed to load audioclip from file " + mFilenames[fileIndex] );

                    // Load the audio data for the audio clip
                    audioClips[i][index].LoadAudioData();

                    // Increment the index variables.
                    index++;
                    fileIndex++;
                    Assert.IsTrue( fileIndex <= mNumFiles, "Tried to load more files than were available. Recheck how many files are availabled for the piano virtual instrument" );
                }
                // Reset the note index when going to the next outer array. 
                index = (int)mLowestSupportedPitch;
            }
        }
        // Normalize the audio clips.
        NormalizeAudioClips( audioClips );

        // Get rid of unused assets.
        audioClips = null;
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    /*************************************************************************//** 
     * @}
     * @defgroup VIBasePrivFunc Private Functions
     * @ingroup VIBase
     * Functions common to all types of instruments.
     * @{
     ****************************************************************************/

    /**
     * @brief Normalizes the data from the audio clips and puts them in the mAudioData member variable.
     * @param[in] aPitch The pitch that corresponds to the audio clips.
     * @param[in] aClips The clips to normalize.
     * 
     * This function makes it so that the waveforms of the samples used for the instrument
     * have a peak of -0.1. If @link DefBID Built-In Dynamics@endlink are supported, then the peak is multiplied
     * by a ratio of the @link DefBIDThresh Built-In Dynamics thresholds@endlink. 
     * For example, waveforms that are used for @link DefVel velocities@endlink from 50-75 would have a peak 
     * that is 75% of the peak of the waveforms used for @link DefVel velocities@endlink from 76-100. 
    */
    private void NormalizeAudioClipsForPitch( Music.PITCH aPitch, AudioClip[] aClips )
    {
        // Make some temp variables for iterating through the data in the clips.
        float max = 0f;
        float[] dataFromClip = null;
        int clipLength = 0;
        int pitchIndex = (int)aPitch;

        // If the instrument doesn't support Built-In Dynamics, then the array is actually just one clip.
        if( mNumBuiltInDynamics == 0 )
        {
            // Get the length of the clip.
            clipLength = aClips[0].samples;

            // Get the data of the clip
            dataFromClip = new float[clipLength];
            aClips[0].GetData( dataFromClip, 0 );

            // Allocate a place in the audio data container.
            mAudioData[0][pitchIndex] = new float[clipLength];

            // Get the max value of this clip's data.
            for( int i = 0; i < clipLength; i++ )
            {
                if( Mathf.Abs( dataFromClip[i] ) > max )
                {
                    max = Mathf.Abs( dataFromClip[i] );
                }
            }

            // Set the normalize factor.
            float normalizeFactor = NORMALIZED_PEAK / max;

            // Normalize the clip and put it in the audio data container
            for( int i = 0; i < clipLength; i++ )
            {
                mAudioData[0][pitchIndex][i] = dataFromClip[i] * normalizeFactor;
            }
        }
        // If the instrument does support Built-In Dynamics, then we need to iterate through each
        // file associated with the clip.
        else
        {
            // Set up the normalized peaks for each Built-In Dynamic.
            float[] normalizedPeaks = new float[mNumBuiltInDynamics];
            for( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                normalizedPeaks[i] = NORMALIZED_PEAK * ( (float)mBuiltInDynamicsThresholds[i] / (float)mBuiltInDynamicsThresholds[mNumBuiltInDynamics - 1] );
            }

            // Iterate through each clip associated with the pitch.
            for( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                // Get the length of the clip data. 
                clipLength = aClips[i].samples;

                // Initialize the clip's spot in the audio data container.
                mAudioData[i][pitchIndex] = new float[clipLength];

                // Get the clip's data
                dataFromClip = new float[clipLength];
                aClips[i].GetData( dataFromClip, 0 );

                // Get the max value of the clip data.
                for( int j = 0; j < clipLength; j++ )
                {
                    if( Mathf.Abs( dataFromClip[j] ) > max )
                    {
                        max = Mathf.Abs( dataFromClip[j] );
                    }
                }

                // Get the normalize factor for this clip.
                float normalizeFactor = normalizedPeaks[i] / max;

                // Normalize the clip and put the normalized data into the audio data array.
                for( int j = 0; j < clipLength; j++ )
                {
                    mAudioData[i][pitchIndex][j] = dataFromClip[j] * normalizeFactor;
                }

                // Reset the max value for the next clip.
                max = 0f;
            }
        }
    }

    /**
     * @brief Normalizes the data from the audio clips and puts them in the mAudioData member variable.
     * @param[in] aAudioClips The audio clips that were loaded for this instrument.
     * 
     * This function makes it so that the waveforms of the samples used for the instrument
     * have a peak of -0.1. If @link DefBID Built-In Dynamics@endlink are supported, then the peak is multiplied
     * by a ratio of the @link DefBIDThresh Built-In Dynamics thresholds@endlink. 
     * For example, waveforms that are used for @link DefVel velocities@endlink from 50-75 would have a peak 
     * that is 75% of the peak of the waveforms used for @link DefVel velocities@endlink from 76-100. 
     */
    private void NormalizeAudioClips( AudioClip[][] aAudioClips )
    {
        float max = 0f;
        float[] temp = null;
        int dataLength = 0;

        // Iterate through every audio clip.
        if( mNumBuiltInDynamics != 0 )
        {
            // Initialize the audio data 3-D array.
            mAudioData = new float[mNumBuiltInDynamics][][];

            // Set up the normalized peaks for each Built-In Dynamic.
            float[] normalizedPeaks = new float[mNumBuiltInDynamics];
            for( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                normalizedPeaks[i] = NORMALIZED_PEAK * ( (float)mBuiltInDynamicsThresholds[i] / (float)mBuiltInDynamicsThresholds[mNumBuiltInDynamics - 1] );
            }

            // Normalize all of the clips
            for( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                // Initialize the interior 2-D array of the audio data and account for drums.
                if( mIsDrum )
                {
                    mAudioData[i] = new float[Music.MAX_SUPPORTED_DRUMS][];
                }
                else
                {
                    mAudioData[i] = new float[Music.MAX_SUPPORTED_NOTES][];
                }

                for( int j = (int)mLowestSupportedPitch; j <= (int)mHighestSupportedPitch; j++ )
                {
                    // Get the length of the clip data and initialize the inner audio data array. 
                    dataLength = aAudioClips[i][j].samples;
                    mAudioData[i][j] = new float[dataLength];

                    // Get this clip's data
                    temp = new float[dataLength];
                    aAudioClips[i][j].GetData( temp, 0 );

                    // Get the max value of the clip data.
                    for( int k = 0; k < dataLength; k++ )
                    {
                        if( Mathf.Abs( temp[k] ) > max )
                        {
                            max = Mathf.Abs( temp[k] );
                        }
                    }

                    // Get the normalize factor for this clip.
                    float normalizeFactor = normalizedPeaks[i] / max;

                    // Normalize the clip and put the normalized data into the audio data array.
                    for( int k = 0; k < dataLength; k++ )
                    {
                        mAudioData[i][j][k] = temp[k] * normalizeFactor;
                    }

                    // Reset the max value for the next clip.
                    max = 0f;
                }
            }
        }
        // Iterate through every audio clip.
        else
        {
            // Allocate the audio data 3-D array and its interior 2-D array.
            mAudioData = new float[1][][];

            // Account for drums when allocating the 2-D array.
            if( mIsDrum )
            {
                mAudioData[0] = new float[Music.MAX_SUPPORTED_DRUMS][];
            }
            else
            {
                mAudioData[0] = new float[Music.MAX_SUPPORTED_NOTES][];
            }

            // Set the normalized peak.
            float normalizedPeak = -.1f;

            // Normalize all of the audio clips.
            for( int i = (int)mLowestSupportedPitch; i <= (int)mHighestSupportedPitch; i++ )
            {
                dataLength = aAudioClips[0][i].samples;
                mAudioData[0][i] = new float[dataLength];

                // Get the clip data
                temp = new float[dataLength];
                aAudioClips[0][i].GetData( temp, 0 );

                // Get the max value of this clip's data.
                for( int j = 0; j < dataLength; j++ )
                {
                    if( Mathf.Abs( temp[j] ) > max )
                    {
                        max = Mathf.Abs( temp[j] );
                    }
                }

                // Set the normalize factor.
                float normalizeFactor = normalizedPeak / max;

                // Normalize the clip.
                for( int j = 0; j < aAudioClips[0][i].samples; j++ )
                {
                    mAudioData[0][i][j] = temp[j] * normalizeFactor;
                }

                // Reset the max value for the next clip.
                max = 0f;
            }
        }
    }

    /*************************************************************************//** 
     * @}
     * @defgroup VIBaseVirtFunc Pure Virtual Functions
     * @ingroup VIBase
     * Functions that subclasses are required to implement.
     * @{
     ****************************************************************************/

    /**
     * @brief Creates the filenames for the sample files. 
     */
    protected virtual void CreateFilenames() { }

    /**
     * @brief Initializes variables related to @link DefBID Built-In Dynamics@endlink
     */
    protected virtual void InitializeBuiltInDynamics() { }

    /** @} */
}
