//---------------------------------------------------------------------------- 
// /Resources/Music/VirtualInstrument/Piano/Piano.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: A specific instance of a VirtualInstrument that uses piano
//              samples. 
//----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Assertions;

public class Piano : VirtualInstrument
{

    //---------------------------------------------------------------------------- 
    // Constructors
    //---------------------------------------------------------------------------- 

    // Creates a new piano instance.
    // IN: aParent The VirtualInstrumentManager that will manage this instrument.
    // OUT: A newly created Piano VirtualInstrument
    public Piano( VirtualInstrumentManager aParent ) : base( aParent )
    {

        // Set default values
        mLowestSupportedNote = Music.PITCH.B0;
        mHighestSupportedNote = Music.PITCH.C8;
        mNumSupportedNotes = 86;
        mSampleRate = 44100;
        mSampleInterval = 1f / mSampleRate;

        // Call functions to set the values relating to built-in dynamics, 
        // create the filenames for each sample, and load audio clips for each sample.
        InitializeBuiltInDynamics();
        CreateFilenames();
        LoadAudioClips();
        
        // Set that this instrument is loaded.
        mLoaded = true;

    }

    //---------------------------------------------------------------------------- 
    // Implementations of inherited pure virtual functions
    //---------------------------------------------------------------------------- 

    // Initializes values related to the built-in dynamics for this instrument.
    // The piano samples are available in 3 separate dynamics:
    // pp: pianissimo (Very Soft), mf: mezzo-forte (Half-loud), and ff: fortissimo (Very Loud). 
    protected override void InitializeBuiltInDynamics()
    {
        mNumBuiltInDynamics = 3;
        mBuiltInDynamics = new string[mNumBuiltInDynamics];
        mBuiltInDynamics[0] = "pp";
        mBuiltInDynamics[1] = "mf";
        mBuiltInDynamics[2] = "ff";
        mBuiltInDynamicsThresholds = new int[mNumBuiltInDynamics];

        // Velocities between 0 & 50 will play the pianissimo samples.
        // Velocities between 51 & 75 will play the mezzoforte samples.
        // Velocities between 75 & 100 will play the fortissimo samples.  
        mBuiltInDynamicsThresholds[0] = 50;
        mBuiltInDynamicsThresholds[1] = 75;
        mBuiltInDynamicsThresholds[2] = 100;
    }

    // Creates the filenames for each piano sample. 
    protected override void CreateFilenames()
    {
        // Set the base filepath and number of files.
        mFilepath = "Music/VirtualInstrument/Piano/Samples/";
        mNumFiles = 258;

        // Initialize the array of filenames.
        mFilenames = new string[mNumFiles];

        // Iterate through each dynamics value and create filenames for each supported note.
        int index = 0;
        for( int i = 0; i < mNumBuiltInDynamics; i++ )
        {
            for( int j = (int)mLowestSupportedNote; j <= (int)mHighestSupportedNote; j++ )
            {
                mFilenames[index] = mFilepath + mBuiltInDynamics[i] + "/" + Music.NoteToString( j ) + mBuiltInDynamics[i];
                index++;
            }
        }
    }
}
