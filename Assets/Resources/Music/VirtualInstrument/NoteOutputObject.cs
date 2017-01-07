//---------------------------------------------------------------------------- 
// /Resources/Music/VirtualInstrument/NoteOutputObject.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: GameObject that handles the actual output of sound. This 
//     should only be implemented as a component of a VirtualInstrumentManager 
//     object.  
//---------------------------------------------------------------------------- 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Assertions;

public class NoteOutputObject : MonoBehaviour
{
    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private AudioSource                mSource; // The AudioSource component of this object
    private AudioReverbFilter          mReverbFilter; // A reverb filter for the sound if it is needed.
    private AudioEchoFilter            mEchoFilter; // An echo filter for the sound if it is needed.
    private bool                       mAudioDataBeingUsed; // Whether or not OnAudioFilterRead is currently using the audio data
    private bool                       mLoaded; // Whether or not this object has loaded.
    private bool                       mNewNote; // Whether or not a new note needs to be started.
    private bool                       mNotePlaying; // Whether or not the note is currently playing.
    private bool                       mNoteRelease; // Whether or not the note has been released.
    private float                      mNewNoteVelocityFactor; // The velocity of a new note mapped to the range [0,1]
    private float                      mVelocityFactor; // A percentage mapping a given velocity to the output volume
    private float[][]                  mAudioData; // A container for raw audio data.
    private int                        mCounter; // A counter to keep track of the current position in the raw audio data.
    private int                        mDynamicsIndex; // The index corresponding to which built-in dynamics value is currently in use.
    private int                        mNewNoteDynamicsIndex; // The dynamics index of the new note.
    private int                        mNumBuiltInDynamics; // The number of built-in dynamics values.
    private int[]                      mBuiltInDynamicsThresholds; // The thresholds that map a velocity to a built-in dynamics value.
    private int[]                      mEndSampleIndices; // The indices corresponding to the last sample in the audio data.

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Called when the object is created. This function sets the initial values for each variable.
    private void Awake()
    {
        mAudioDataBeingUsed = false;
        mLoaded = false;
        mCounter = 0;
        mEndSampleIndices = null;
        mAudioData = null;
        mNewNote = false;
        mNumBuiltInDynamics = 0;
        mDynamicsIndex = 0;
        mVelocityFactor = 1f;
        mNoteRelease = false;

        // Create an Audio Source and attach it as a component to this object. 
        mSource = gameObject.GetComponent<AudioSource>();
        if( mSource != null )
        {
            DestroyImmediate( mSource );
        }

        mSource = gameObject.AddComponent<AudioSource>();
        mSource.enabled = true;
        mSource.playOnAwake = false;
        mSource.minDistance = 10000f;
        mSource.maxDistance = 10000000f;
        mSource.volume = 1f;

        mEchoFilter = gameObject.GetComponent<AudioEchoFilter>();
        if( mEchoFilter != null )
        {
            DestroyImmediate( mEchoFilter );
        }

        mEchoFilter = gameObject.AddComponent<AudioEchoFilter>();
        mEchoFilter.enabled = true;
        mEchoFilter.wetMix = 1f;
        mEchoFilter.decayRatio = 0.25f;
        mEchoFilter.delay = 100f;


        mReverbFilter = gameObject.GetComponent<AudioReverbFilter>();
        if( mReverbFilter != null )
        {
            DestroyImmediate( mReverbFilter );
        }
        mReverbFilter = gameObject.AddComponent<AudioReverbFilter>();
            
        mReverbFilter.enabled = true;


    }

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Removes the audio data and sets relevant variables to default values.
    public void RemoveAudioData()
    {
        mLoaded = false;
        mNoteRelease = false;
        mNewNote = false;
        mNotePlaying = false;

        // Remove the audio data array.
        if( mAudioData != null )
        {
            if ( mNumBuiltInDynamics == 0 )
            {
                mAudioData[0] = null;
            }
            else
            {
                for ( int i = 0; i < mNumBuiltInDynamics; i++ )
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
        mNumBuiltInDynamics = 0;

        // If we previously loaded data, then we need to stop the source and mark that 
        // the output object is not properly loaded.
        if( mLoaded )
        {
            mSource.Stop();
            mLoaded = false;
        }

    }

    // Sets the audio data for this object
    // IN: aAudioData The raw audio data for each dynamic threshold. If there are not any 
    //         dynamics thresholds to account for, then it is just the raw audio data for a
    //         single note.
    // IN: aThresholds Optional values for mapping which audio to play for a given velocity. 
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
            for ( int i = 0; i < aThresholds.Length; i++ )
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
        if ( mNumBuiltInDynamics == 0 )
        {
            mAudioData = new float[1][];
            mAudioData[0] = new float[aAudioData[0].Length];
            mEndSampleIndices = new int[1];
            mEndSampleIndices[0] = aAudioData[0].Length - 1;
            for ( int i = 0; i < aAudioData[0].Length; i++ )
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
            for ( int i = 0; i < mNumBuiltInDynamics; i++ )
            {
                mAudioData[i] = new float[aAudioData[i].Length];
                mEndSampleIndices[i] = aAudioData[i].Length - 1;
                for ( int j = 0; j < mAudioData[i].Length; j++ )
                {
                    mAudioData[i][j] = aAudioData[i][j];
                }
            }
        }

        // Mark that this object is loaded. 
        mLoaded = true;
    }

    //---------------------------------------------------------------------------- 
    // Event Handlers
    //---------------------------------------------------------------------------- 

    // Marks that the note should begin fading out. This should only be called from 
    // VirtualInstrumentManager's OnNoteFadeOut Event.
    public void BeginNoteFadeOut()
    {
        if ( mLoaded )
        {
            // Set that the note should fade out. 
            // Actually processing the fade out will be handled by the onAudioFilterRead function.
            mNoteRelease = true;
        }
    }

    // Handler for setting the note to be played. Should be called from VirtualInstrumentManager's OnNotePlay event. 
    // IN: aVelocity The velocity of the note to be played. Ranges from 0 (silent) to 100 (max volume).
    public void BeginNotePlaying( int aVelocity )
    {
        Assert.IsTrue( aVelocity <= 100, "VirtualInstrumentOutput was given a velocity greater than 100!" );

        if ( mLoaded )
        {

            // Calculate the velocity multiplier. The multiplier is a percentage that is used to adjust the
            // levels of the audio data to modify the output volume. 
            // If built-in dynamics are supported, then the velocity multiplier will range from 0.5 to 1.0.
            if ( mNumBuiltInDynamics != 0 )
            {
                // See which built-in dynamics value we need to use. Start at the top threshold and work down.
                for ( int i = mNumBuiltInDynamics - 1; i > -1; i-- )
                {
                    if ( aVelocity <= mBuiltInDynamicsThresholds[i] )
                    {
                        mNewNoteDynamicsIndex = i;

                        // Calculate the velocity factor which will range from 0.5 to 1.0.
                        if ( i == 0 )
                        {
                            mNewNoteVelocityFactor = .5f +
                                    ( ( .5f / (float)mBuiltInDynamicsThresholds[0] ) * ( (float)aVelocity ) );
                        }
                        else
                        {
                            mNewNoteVelocityFactor = .5f +
                                ( ( .5f / (float)( mBuiltInDynamicsThresholds[i] - mBuiltInDynamicsThresholds[i - 1] ) ) * ( aVelocity - mBuiltInDynamicsThresholds[i - 1] ) );
                        }
                    }
                }
            }
            // If built-in dynamics are not supported, then just use the given velocity as a percentage. 
            else
            {
                mNewNoteVelocityFactor = (float)aVelocity / 100f;
                mNewNoteDynamicsIndex = 0;
            }

            mNewNote = true;

        }

    }

    // Handler that is called whenever the audio buffer is refilled. This handler will pass appropriate sections 
    // of the raw audio data to the buffer and which will cause the sound to actually be played. This handler 
    // is automatically called at intervals of ~23ms 
    // INOUT: data The raw audio data that will be played. This will be modified inside the function to create 
    //             a custom "filter". The only modifications here are replacing whatever was passed in with the 
    //             raw audio data and modifying the volume according to the velocity given in the note play event. 
    // IN: channels The number of channels in the audio data. Not too relevent at this moment.
    private void OnAudioFilterRead( float[] data, int channels )
    {
        // Only generate the sound if it's loaded.
        if( mLoaded )
        {
            mAudioDataBeingUsed = true;
            if( mNewNote )
            {
                // Handle starting a new note by setting the relevant member variables
                mCounter = 0;
                mVelocityFactor = mNewNoteVelocityFactor;
                mDynamicsIndex = mNewNoteDynamicsIndex;
                mNewNote = false;
                mNoteRelease = false;
                mNotePlaying = true;
            }
            else if( mNoteRelease )
            {
                // If the note has been released, then set the velocity factor to 
                // decrease each time this function is called.
                mVelocityFactor *= .99f;
            }


            if( mNotePlaying )
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
                    mCounter = 0;
                    mNotePlaying = false;
                    mNoteRelease = false;
                }
                // If we haven't reached the end of the audio data yet, then increase the counter.
                else
                {
                    mCounter += data.Length;
                }
            }
            mAudioDataBeingUsed = false;
        }
    }

    // Toggles whether or not the echo filter is active.
    public void ToggleEcho( bool aOn )
    {
        if( aOn )
        {
            mEchoFilter.enabled = true;
        }
        else
        {
            mEchoFilter.enabled = false;
        }
    }

    public void ToggleReverb( bool aOn )
    {
        if( aOn )
        {
            mReverbFilter.enabled = true;
        }
        else
        {
            mReverbFilter.enabled = false;
        }
    }
}
