﻿//---------------------------------------------------------------------------- 
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
    private bool                       mAudioDataBeingUsed; // Whether or not OnAudioFilterRead is currently using the audio data
    private bool                       mLoaded; // Whether or not this object has loaded.
    private bool                       mLoop; // Whether or not the audio should loop.
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
        // Set the values of the member variables
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
        mLoop = false;

        // Make sure that this isn't destroyed when a new scene is loaded.
        DontDestroyOnLoad( this );

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
        mSource.minDistance = 10000f;
        mSource.maxDistance = 10000000f;
        //mSource.volume = 1f;
    }

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Gets whether or not the audio should loop.
    // OUT: True if the audio should loop. False otherwise.
    public bool GetLoop()
    {
        return mLoop;
    }

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

    // Sets whether or not the audio should loop.
    // IN: aLoop Whether or not the audio should loop.
    public void SetLoop( bool aLoop )
    {
        mLoop = aLoop;
    }

    // Stops the audio from playing (actually it makes it silent, but same thing for our purposes).
    public void StopPlaying()
    {
        mNotePlaying = false;
    }

    //---------------------------------------------------------------------------- 
    // Event Handlers
    //---------------------------------------------------------------------------- 

    // Marks that the note should begin fading out. This should only be called from 
    // VirtualInstrumentManager's OnNoteFadeOut Event.
    public void BeginNoteFadeOut()
    {
        if ( mLoaded && mNotePlaying )
        {
            // Set that the note should fade out. 
            // Actually processing the fade out will be handled by the onAudioFilterRead function.
            mNoteRelease = true;
        }
    }

    // Handler for setting the note to be played. Should be called from VirtualInstrumentManager's OnNotePlay event. 
    // IN: aVelocityFactor The adjusted velocity of the note to be played. Ranges from 0 (silent) to 1.0 (max volume).
    // IN: aDynamicsIndex the index of the built-in dynamics.
    public void BeginNotePlaying( float aVelocityFactor, int aDynamicsIndex )
    {
        Assert.IsTrue( aVelocityFactor <= 1f, "NoteOutputObject was given a velocity factor greater than 1!" );

        if ( mLoaded )
        {
            mNewNoteVelocityFactor = aVelocityFactor;
            mNewNoteDynamicsIndex = aDynamicsIndex;
            mNewNote = true;
            mNoteRelease = false;
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
                mVelocityFactor -= ( 1f / 100f );
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
}
