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
    // Public Variables
    //---------------------------------------------------------------------------- 
    public UnityEvent LoadEvent; // Invoked on the parent VirtualInstrumentManager when the instrument has finished loading.


    //---------------------------------------------------------------------------- 
    // Protected Variables
    //---------------------------------------------------------------------------- 
    protected AudioClip[][]                      mAudioClips; // The samples of the virtual instrument
    protected bool                               mLoaded; // Whether or not the virtual instrument is loaded.
    protected float                              mSampleInterval; // The sample interval of the virtual instrument
    protected int                                mNumFiles; // The number of sample files
    protected int                                mNumBuiltInDynamics; // The number of built in dynamics
    protected int                                mNumSupportedNotes; // The number of notes supported by the instrument
    protected int                                mSampleRate; // The sample rate of the virtual instrument
    protected int[]                              mBuiltInDynamicsThresholds; // The thresholds for when to use a specific dynamic
    protected Music.NOTE                         mLowestSupportedNote; // The lowest supported note of the instrument
    protected Music.NOTE                         mHighestSupportedNote; // The highest supported note of the instrument
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
        LoadEvent = new UnityEvent();
        LoadEvent.AddListener( aParent.OnInstrumentLoaded );
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
            for ( int i = mNumBuiltInDynamics - 1; i > -1; i++ )
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

    // Gets the highest note that this instrument supports.
    // OUT: The highest note that this instrument supports.
    public Music.NOTE GetHighestSupportedNote()
    {
        return mHighestSupportedNote;
    }

    // Gets the lowest note that this instrument supports.
    // OUT: The lowest note that this instrument supports.
    public Music.NOTE GetLowestSupportedNote()
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
    public float[][] GetRawAudioDataForNote( Music.NOTE aNote )
    {
        Assert.IsTrue( (int)aNote >= (int)mLowestSupportedNote && (int)aNote <= (int)mHighestSupportedNote,
            "Tried to load the audio data for a note that is not supported by the instrument!" );
        Assert.IsNotNull( mAudioClips, "Tried to get data from a non-loaded virtual instrument!" );
        Assert.IsNotNull( mAudioClips[0][(int)aNote], "Tried to get data from a virtual instrument for a note that was not loaded!" );

        // Allocate a 2-D float array that will be returned.
        float[][] data;

        // If there aren't any built-in dynamics, then use a hard-coded index and copy the data to the array from the audio clip.
        if ( mNumBuiltInDynamics == 0 )
        {
            data = new float[1][];
            data[0] = new float[mAudioClips[0][(int)aNote].samples];
            mAudioClips[0][(int)aNote].GetData( data[0], 0 );
        }
        // If there are built-in dynamics, then get the data from the audio clips for each corresponding file.
        else
        {
            data = new float[mNumBuiltInDynamics][];
            for ( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                data[i] = new float[mAudioClips[i][(int)aNote].samples];
                mAudioClips[i][(int)aNote].GetData( data[i], 0 );
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
    public bool IsNoteSupported( Music.NOTE aNote )
    {
        return ( (int)aNote >= (int)mLowestSupportedNote && (int)aNote <= (int)mHighestSupportedNote );
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

        // If there aren't any built-in dynamics, then use a hard-coded index of 0 for the outer array.
        if ( mNumBuiltInDynamics == 0 )
        {

            // Initialize the array of audio clips. In order to account for instruments differing in the range of notes 
            // that they support, the inner audio clip array is set to have an element for every possible note. Unsupported
            // notes will have null audio clips at their indices.    
            mAudioClips = new AudioClip[1][];
            mAudioClips[0] = new AudioClip[Music.MAX_SUPPORTED_NOTES];

            // The indices of the loaded audio clips are mapped to their corresponding note. 
            while ( index <= (int)mHighestSupportedNote )
            {
                // Load the audio clip into the audio clip array.
                mAudioClips[0][index] = Resources.Load<AudioClip>( mFilenames[fileIndex] );
                Assert.IsNotNull( mAudioClips[0][index], "Failed to load audioclip from file " + mFilenames[fileIndex] );

                // Load the audio data for the audio clip
                mAudioClips[0][index].LoadAudioData();

                // Increment the index variables.
                index++;
                fileIndex++;
                Assert.IsTrue( fileIndex < mNumFiles, "Tried to load more files than were available. Recheck how many files are availabled for the piano virtual instrument" );
            }
        }
        // If built-in dynamics are available for this instrument, then load audio clips for each built-in dynamics value.
        else
        {
            // Initializ the outer array of audio clips
            mAudioClips = new AudioClip[mNumBuiltInDynamics][];

            // Iterate through each outer array.
            for ( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                // Initialize the inner array of audio clips. In order to account for instruments differing in the range of notes 
                // that they support, the inner audio clip array is set to have an element for every possible note. Unsupported
                // notes will have null audio clips at their indices.    
                mAudioClips[i] = new AudioClip[Music.MAX_SUPPORTED_NOTES];

                // The indices of the loaded audio clips are mapped to their corresponding note. 
                while ( index <= (int)mHighestSupportedNote )
                {
                    // Load the audio clip into the audio clip array.
                    mAudioClips[i][index] = Resources.Load<AudioClip>( mFilenames[fileIndex] );
                    Assert.IsNotNull( mAudioClips[0][index], "Failed to load audioclip from file " + mFilenames[fileIndex] );

                    // Load the audio data for the audio clip
                    mAudioClips[i][index].LoadAudioData();

                    // Increment the index variables.
                    index++;
                    fileIndex++;
                    Assert.IsTrue( fileIndex <= mNumFiles, "Tried to load more files than were available. Recheck how many files are availabled for the piano virtual instrument" );
                }
                // Reset the note index when going to the next outer array. 
                index = (int)mLowestSupportedNote;
            }
        }
    }

    //---------------------------------------------------------------------------- 
    // Pure Virtual Functions
    //---------------------------------------------------------------------------- 

    // Creates the filenames for the sample files. 
    protected virtual void CreateFilenames() { }

    // Initializes variables related to built-in dynamics 
    protected virtual void InitializeBuiltInDynamics() { }
    

}
