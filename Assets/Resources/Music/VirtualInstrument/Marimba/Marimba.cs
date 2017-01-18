//---------------------------------------------------------------------------- 
// /Resources/Music/VirtualInstrument/Marimba/Marimba.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: A specific instance of a VirtualInstrument that uses marimba
//              samples. 
//----------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Assertions;

public class Marimba : VirtualInstrument
{
    //---------------------------------------------------------------------------- 
    // Constructors
    //---------------------------------------------------------------------------- 

    // Creates a new piano instance.
    // IN: aParent The VirtualInstrumentManager that will manage this instrument.
    // OUT: A newly created Piano VirtualInstrument
    public Marimba( VirtualInstrumentManager aParent ) : base( aParent )
    {

        // Set default values
        mIsDrum = false;
        mLowestSupportedNote = Music.PITCH.C2;
        mHighestSupportedNote = Music.PITCH.C7;
        mNumSupportedNotes = 61;
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
    // There aren't built-in dynamics for the marimba.
    protected override void InitializeBuiltInDynamics()
    {
        // There are no built-in dynamics for the marimba.
        mNumBuiltInDynamics = 0;
        mBuiltInDynamics = null;
        mBuiltInDynamicsThresholds = null;
    }

    // Creates the filenames for each marimba sample. 
    protected override void CreateFilenames()
    {
        // Set the base filepath and number of files.
        mFilepath = "Music/VirtualInstrument/Marimba/Samples/";
        mNumFiles = 61;

        // Initialize the array of filenames.
        mFilenames = new string[mNumFiles];

        // Iterate through each dynamics value and create filenames for each supported note.
        int index = 0;
        for( int i = (int)mLowestSupportedNote; i <= (int)mHighestSupportedNote; i++ )
        {
            mFilenames[index] = mFilepath + Music.NoteToString( i );
            index++;
        }
    }
}
