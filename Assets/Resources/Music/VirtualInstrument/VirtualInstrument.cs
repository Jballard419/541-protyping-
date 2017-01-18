//---------------------------------------------------------------------------- 
// /Resources/Music/VirtualInstrument/VirtualInstrument.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: A parent class that contains generic functions and values 
//     relating to virtual instruments such as retrieving raw audio data from 
//     sample files and note mapping. This class is meant to be the base 
//     class for implementing specific instruments. Its only valid use to other  
//     classes that don't inherit it is to be a generic type that is checked at
//     runtime. Also, virtual instruments are meant to be used as-is, so there 
//     is not much modification that can be done on this class by other classes 
//     that do not inherit from it.
//----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

public class VirtualInstrument
{
    //---------------------------------------------------------------------------- 
    // Protected Variables
    //----------------------------------------------------------------------------
    protected bool                               mIsDrum; // Whether or not the virtual instrument is a drum. 
    protected bool                               mLoaded; // Whether or not the virtual instrument is loaded.
    protected float                              mSampleInterval; // The sample interval of the virtual instrument
    protected float[][][]                        mAudioData; // The samples of the virtual instrument
    protected int                                mNumFiles; // The number of sample files
    protected int                                mNumBuiltInDynamics; // The number of built in dynamics
    protected int                                mNumSupportedNotes; // The number of notes supported by the instrument
    protected int                                mSampleRate; // The sample rate of the virtual instrument
    protected int[]                              mBuiltInDynamicsThresholds; // The thresholds for when to use a specific dynamic
    protected Music.PITCH                        mLowestSupportedNote; // The lowest supported note of the instrument
    protected Music.PITCH                        mHighestSupportedNote; // The highest supported note of the instrument
    protected string                             mFilepath; // The base filepath for the samples.
    protected string[]                           mFilenames; // An array of filenames for the samples.
    protected string[]                           mBuiltInDynamics; // The names of the built in dynamics
    protected VirtualInstrumentManager           mParent; // The manager for this instrument


    //---------------------------------------------------------------------------- 
    // Constructors
    //---------------------------------------------------------------------------- 
    
    // Generic constructor. Child classes should have more detailed constructors, 
    // but this will be called for each child class as well. 
    // IN: aParent The parent manager for this instrument.
    // OUT: A newly created VirtualInstrument.
    public VirtualInstrument( VirtualInstrumentManager aParent )
    {
        mParent = aParent;
    }

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Gets the index of the outer array to use for a given velocity.
    // IN: aVelocity The given velocity from 0 (silent) to 100 (max volume).
    // OUT: The index of the outer array to use for the given velocity.
    public int GetBuiltInDynamicsThresholdIndex( int aVelocity )
    {
        // If there aren't any built-in dynamics, then just return 0.
        if ( mNumBuiltInDynamics == 0 )
        {
            return 0;
        }
        // If there are built-in dynamics, then start from the highest and work down to find
        // which one to use. 
        else
        {
            int dynamicsIndex = 0;
            for ( int i = mNumBuiltInDynamics - 1; i > -1; i-- )
            {
                if ( aVelocity <= mBuiltInDynamicsThresholds[i] )
                {
                    dynamicsIndex = i;
                }
            }
            return dynamicsIndex;
        }
    }

    // Gets the built-in dynamics thresholds for the instrument.
    // OUT: The thresholds for mapping a file to a velocity.
    public int[] GetBuiltInDynamicsThresholds()
    {
        return mBuiltInDynamicsThresholds;
    }

    // Gets the adjusted velocity factor from 0 to 1.0. 
    // IN: aVelocity The velocity for the note from 0 to 100.
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

    // Gets the highest note that this instrument supports.
    // OUT: The highest note that this instrument supports.
    public Music.PITCH GetHighestSupportedNote()
    {
        return mHighestSupportedNote;
    }

    // Gets the lowest note that this instrument supports.
    // OUT: The lowest note that this instrument supports.
    public Music.PITCH GetLowestSupportedNote()
    {
        return mLowestSupportedNote;
    }

    // Gets the number of notes that this instrument supports.
    // OUT: The number of notes that this instrument supports.
    public int GetNumOfSupportedNotes()
    {
        return mNumSupportedNotes;
    }

    // Gets the raw audio data of each built-in dynamics value for a given note. 
    // IN: aNote The note for which the data is retrieved.
    // OUT: The raw audio data for each built-in dynamics value for the given note
    public float[][] GetRawAudioDataForNote( Music.PITCH aNote )
    {
        Assert.IsTrue( (int)aNote >= (int)mLowestSupportedNote && (int)aNote <= (int)mHighestSupportedNote,
            "Tried to load the audio data for the note " + Music.NoteToString( aNote ) + " which is not a note that is supported by this instrument!" );
        Assert.IsNotNull( mAudioData, "Tried to get data from a non-loaded virtual instrument!" );
        Assert.IsNotNull( mAudioData[0][(int)aNote],
            "Tried to get data from a virtual instrument for the note " + Music.NoteToString( aNote ) + " which was not loaded!" );

        float[][] data = null;

        // If there aren't any built-in dynamics, then use a hard-coded index and copy the data to the array from the audio clip.
        if ( mNumBuiltInDynamics == 0 )
        {
            data = new float[1][];
            int dataLength = mAudioData[0][(int)aNote].Length;
            data[0] = new float[dataLength];
            for( int i = 0; i < dataLength; i++ )
            {
                data[0][i] = mAudioData[0][(int)aNote][i];
            }
        }
        // If there are built-in dynamics, then get the data from the audio clips for each corresponding file.
        else
        {
            data = new float[mNumBuiltInDynamics][];
            int dataLength = 0;
            for( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                dataLength = mAudioData[i][(int)aNote].Length;
                data[i] = new float[dataLength];
                for( int j = 0; j < dataLength; j++ )
                {
                    data[i][j] = mAudioData[i][(int)aNote][j];
                }
            }
        }

        return data;
    }

    // Gets the sample interval of the instrument.
    // OUT: The sample interval of the instrument.
    public float GetSampleInterval()
    {
        return mSampleInterval;
    }

    // Gets the sample rate of the instrument.
    // OUT: The sample rate of the instrument.
    public int GetSampleRate()
    {
        return mSampleRate;
    }

    // Returns whether or not the instrument has loaded.
    // OUT: True if the instrument has loaded. False otherwise.
    public bool IsLoaded()
    {
        return mLoaded;
    }

    // Checks if a given note is able to be played by this instrument.
    // OUT: True if the instrument can play the note. False otherwise.
    public bool IsNoteSupported( Music.PITCH aNote )
    {
        return ( (int)aNote >= (int)mLowestSupportedNote && (int)aNote <= (int)mHighestSupportedNote );
    }

    // Normalizes the audio clips after their data has been loaded.
    // IN: aAudioClip The audio clips to normalize.
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

            // Set the normalize factor for each built-in dynamic.
            float[] normalizedPeaks = new float[mNumBuiltInDynamics];
            for( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                normalizedPeaks[i] = -.1f * ( (float)mBuiltInDynamicsThresholds[i] / (float)mBuiltInDynamicsThresholds[mNumBuiltInDynamics - 1] );
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

                for( int j = (int)mLowestSupportedNote; j <= (int)mHighestSupportedNote; j++ )
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
            for( int i = (int)mLowestSupportedNote; i <= (int)mHighestSupportedNote; i++ )
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

    //---------------------------------------------------------------------------- 
    // Protected Functions
    //---------------------------------------------------------------------------- 

    // Loads the audio clips for the instrument. CreateFilenames must be called prior to this function
    // in order to actually find the files. 
    protected void LoadAudioClips()
    {
        // Initialize index variables so that all of the files can be iterated through and appropriately 
        // assigned to an array of audio clips. 
        int index = (int)mLowestSupportedNote;
        int fileIndex = 0;
        AudioClip[][] audioClips = null;
        // If there aren't any built-in dynamics, then use a hard-coded index of 0 for the outer array.
        if ( mNumBuiltInDynamics == 0 )
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
            while ( index <= (int)mHighestSupportedNote )
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
            for ( int i = 0; i < mNumBuiltInDynamics; i++ )
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
                while ( index <= (int)mHighestSupportedNote )
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
                index = (int)mLowestSupportedNote;
            }
        }
        // Normalize the audio clips.
        NormalizeAudioClips( audioClips );
    }

    //---------------------------------------------------------------------------- 
    // Pure Virtual Functions
    //---------------------------------------------------------------------------- 

    // Creates the filenames for the sample files. 
    protected virtual void CreateFilenames() { }

    // Initializes variables related to built-in dynamics 
    protected virtual void InitializeBuiltInDynamics() { }
    

}
