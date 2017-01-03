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
using UnityEngine.Assertions;

public class NoteOutputObject : MonoBehaviour
{
    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private AudioSource                mSource; // The AudioSource component of this object
    private bool                       mAudioDataBeingModified; // Whether or not the audio buffer is currently in use.
    private bool                       mKeyReleased; // Whether or not the corresponding key has been released.
    private bool                       mLoaded; // Whether or not this object has loaded.
    private bool                       mNoteIsPlaying; // Whether or not the corresponding note is currently being played.
    private float                      mVelocityFactor; // A percentage mapping a given velocity to the output volume 
    private float[][]                  mAudioData; // A container for raw audio data.
    private int                        mCounter; // A counter to keep track of the current position in the raw audio data.
    private int                        mDynamicsIndex; // The index corresponding to which built-in dynamics value is currently in use.
    private int                        mNumBuiltInDynamics; // The number of built-in dynamics values.
    private int                        mSampleRate; // The sample rate of the audio data.
    private int[]                      mBuiltInDynamicsThresholds; // The thresholds that map a velocity to a built-in dynamics value.
    private int[]                      mEndSampleIndices; // The indices corresponding to the last sample in the audio data.

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Called when the object is created. This function sets the initial values for each variable.
    private void Awake()
    {
        mLoaded = false;
        mCounter = 0;
        mEndSampleIndices = null;
        mAudioData = null;
        mNoteIsPlaying = false;
        mNumBuiltInDynamics = 0;
        mDynamicsIndex = 0;
        mVelocityFactor = 1;
        mAudioDataBeingModified = false;
        mKeyReleased = true;

        // Create an Audio Source and attach it as a component to this object. 
        mSource = gameObject.AddComponent<AudioSource>();
        mSource.enabled = true;
        mSource.playOnAwake = false;
        mSource.minDistance = 10000f;
        mSource.maxDistance = 10000000f;
        mSource.volume = 1f;
    }

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Removes the audio data and sets relevant variables to default values.
    public void RemoveAudioData()
    {
        // Note: Might not need this assert...
        Assert.IsFalse( mNoteIsPlaying, "Tried to unload a virtual instrument output object while it was playing!" );
        mNoteIsPlaying = false;

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
    public void SetAudioData( float[][] aAudioData, int aSampleRate = 44100, int[] aThresholds = null )
    {
        // Remove any existing audio data.
        RemoveAudioData();

        // Set the sample rate.
        mSampleRate = aSampleRate;

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

        // Initialize the audio data array and copy the values from the given parameter.
        // If we don't have to worry about built in dynamics, then use hard-coded indices 
        // for the outer array.
        if( mNumBuiltInDynamics == 0 )
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
        // Set that the note should fade out. The filter in OnAudioFilterRead will handle everything 
        // else.
        mKeyReleased = true;
    }

    // Handler for setting the note to be played. Should be called from VirtualInstrumentManager's OnNotePlay event. 
    // IN: aVelocity The velocity of the note to be played. Ranges from 0 (silent) to 100 (max volume).
    public void BeginNotePlaying( int aVelocity )
    {
        Assert.IsTrue( aVelocity <= 100, "VirtualInstrumentOutput was given a velocity greater than 100!" );

        // Only want to perform this function if the note is not currently playing or if the key was hit while
        // the note was fading out from a previous hit. 
        if ( mKeyReleased && mLoaded )
        {

            // Don't mess with the buffer data if it is being read/modified! Note: Might need to implement better 
            // synchronization control. 
            while ( mAudioDataBeingModified );
            mAudioDataBeingModified = true;


            // Set the counter to 0 so that playback begins at the start of the sample file. 
            mCounter = 0;

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
                        mDynamicsIndex = i;

                        // Calculate the velocity factor which will range from 0.5 to 1.0.
                        if ( i == 0 )
                        {
                            mVelocityFactor = .5f +
                                    ( ( .5f / (float)mBuiltInDynamicsThresholds[0] ) * ( (float)aVelocity ) );
                        }
                        else
                        {
                            mVelocityFactor = .5f +
                                ( ( .5f / (float)( mBuiltInDynamicsThresholds[i] - mBuiltInDynamicsThresholds[i - 1] ) ) * ( aVelocity - mBuiltInDynamicsThresholds[i - 1] ) );
                        }
                    }
                }
            }
            // If built-in dynamics are not supported, then just use the given velocity as a percentage. 
            else
            {
                mVelocityFactor = (float)aVelocity / 100f;
            }

            // Set that the note is playing, the key is not released, and the audio data is not being modified anymore. 
            mKeyReleased = false;
            mNoteIsPlaying = true;
            mAudioDataBeingModified = false;
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
        // Only generate the sound if we need to. 
        if( mNoteIsPlaying && !mAudioDataBeingModified )
        {
            // Set that the buffer is in use.
            mAudioDataBeingModified = true;

            // Account for fading out the note on release. If we are fading out, then skip to 100ms in the 
            // sample if we haven't reached that point yet. Note: May need to be adjusted. 
            if ( mKeyReleased && mVelocityFactor > 0f ) 
            {
                mVelocityFactor -= 0.01f ;
            }

            // Get the remaining number of samples left in the audio data. 
            int remainingSamples = mEndSampleIndices[mDynamicsIndex] - mCounter;

            // If there is not enough remaining data, then partially fill the array with the remaining data 
            // and fill the rest of the array with zeroes. Also reset the counter and update mNoteIsPlaying to 
            // false so that the note will stop playing after this loop.
            if ( data.Length > remainingSamples )
            {
                for ( int i = 0; i < remainingSamples; i++ )
                {
                    data[i] = mAudioData[mDynamicsIndex][mCounter + i] * mVelocityFactor;
                }
                for ( int i = remainingSamples; i < data.Length; i++ )
                {
                    data[i] = 0f;
                }
                mCounter = 0;
                mNoteIsPlaying = false;
            }

            // If there is more than enough remaining data, then fill the array with data and update the counter.
            else
            {
                for ( int i = 0; i < data.Length; i++ )
                {
                    data[i] = mAudioData[mDynamicsIndex][mCounter + i] * mVelocityFactor;
                }
                mCounter += data.Length;
            }

            // Set that the audio buffer is no longer being modified.
            mAudioDataBeingModified = false;
        }
    }
}
