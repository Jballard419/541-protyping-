//---------------------------------------------------------------------------- 
// /Resources/Music/VirtualInstrument/DrumKit/DrumKit.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: A specific instance of a VirtualInstrument that uses drum
//              samples. 
//----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Assertions;

public class DrumKit : VirtualInstrument
{

    //---------------------------------------------------------------------------- 
    // Constructors
    //---------------------------------------------------------------------------- 

    // Creates a new Drum Kit instance.
    // IN: aParent The VirtualInstrumentManager that will manage this instrument.
    // OUT: A newly created Drum Kit VirtualInstrument
    public DrumKit( VirtualInstrumentManager aParent ) : base( aParent )
    {

        // Set default values
        mIsDrum = true;
        mLowestSupportedPitch = Music.PITCH.C0;
        mHighestSupportedPitch = Music.PITCH.F1;
        mNumSupportedPitches = 18;
        mSampleRate = 44100;
        mSampleInterval = 1f / mSampleRate;

        // Call functions to set the values relating to built-in dynamics and create the filenames for each sample.
        InitializeBuiltInDynamics();
        CreateFilenames();

        // Set that this instrument is loaded.
        mLoaded = true;

    }

    //---------------------------------------------------------------------------- 
    // Implementations of inherited pure virtual functions
    //---------------------------------------------------------------------------- 

    // Initializes values related to the built-in dynamics for this instrument.
    protected override void InitializeBuiltInDynamics()
    {
        // There are no built-in dynamics for the drum kit (yet).
        mNumBuiltInDynamics = 0;
        mBuiltInDynamics = null;
        mBuiltInDynamicsThresholds = null;

        // Allocate the audio data container.
        mAudioData = new float[1][][];
        mAudioData[0] = new float[Music.MAX_SUPPORTED_DRUMS][];
    }

    // Creates the filenames for each piano sample. 
    protected override void CreateFilenames()
    {
        // Set the base filepath and number of files.
        mFilepath = "Audio/VirtualInstrument/DrumKit/Samples/";
        mNumFiles = 18;

        // Initialize the array of filenames.
        mFilenames = new string[mNumFiles];

        // Iterate through each dynamics value and create filenames for each supported note.
        int index = 0;
        for( int i = (int)mLowestSupportedPitch; i <= (int)mHighestSupportedPitch; i++ )
        {
            mFilenames[index] = mFilepath + Music.DrumToString( i );
            index++;
        }
    }
}
