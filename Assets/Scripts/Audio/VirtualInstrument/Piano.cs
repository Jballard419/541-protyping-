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

/**
 * @class Piano
 * @brief A specific type of @link VI Virtual Instrument@endlink that uses piano samples. 
 * 
 * The lowest @link Music::PITCH pitch@endlink supported by the piano is @link Music::PITCH B0@endlink
 * and the highest @link Music::PITCH pitch@endlink supported is @link Music::PITCH C8@endlink. 
 * 
 * @n This instrument is special (so far) in this project in that it supports 
 * @link DefBID Built-In Dynamics@endlink with three different sound files for each
 * pitch. Go to @link DefBID the section about Built-In Dynamics@endlink for more details.
 * 
 * @n Its wav files are stored in "Resources/Audio/VirtualInstrument/Piano/Samples/ff",
 *  "Resources/Audio/VirtualInstrument/Piano/Samples/mf", and 
 *  "Resources/Audio/VirtualInstrument/Piano/Samples/pp"
*/
public class Piano : VirtualInstrument
{

    /*************************************************************************//** 
     * @defgroup PianoConstruct Constructors
     * @ingroup DocPiano
     * The constructor for the Piano.
     * @{
    *****************************************************************************/

    /**
     * @brief Creates a new Piano instance.
     * @param[in] aParent The @link VIM Virtual Instrument Manager@endlink that will manage this instrument.
     * @return A newly created Piano @link VI Virtual Instrument@endlink
     * 
     * Calls the @link VIBase base constructor@endlink, sets the values specific to 
     * Piano @link VI Virtual Instruments@endlink and begins loading the wav files.
    */
    public Piano( VirtualInstrumentManager aParent ) : base( aParent )
    {

        // Set default values
        mIsDrum = false;
        mLowestSupportedPitch = Music.PITCH.B0;
        mHighestSupportedPitch = Music.PITCH.C8;
        mNumSupportedPitches = 86;
        mSampleRate = 44100;
        mSampleInterval = 1f / mSampleRate;

        // Call functions to set the values relating to built-in dynamics, 
        // create the filenames for each sample, and load audio clips for each sample.
        InitializeBuiltInDynamics();
        CreateFilenames();

        // Set that this instrument is loaded.
        mLoaded = true;

    }

    /*************************************************************************//** 
     * @}
     * @defgroup PianoVirtFunc Implemented Virtual Functions
     * @ingroup DocPiano
     * Implementations of @link VIBaseVirtFunc pure virtual functions@endlink from
     * the @link VIBase base class@endlink. 
     * @{
    *****************************************************************************/

    /**
     * @brief Initializes values related to the @link DefBID Built-In Dynamics@endlink for this instrument and allocates the audio data container.
     * The Piano samples are available in three separate @link DefBID dynamics@endlink :
     * @n pp: pianissimo (Very Soft) for @link DefVel velocities@endlink from 0 to 50.
     * @n mf: mezzo-forte (Half-loud) for @link DefVel velocities@endlink from 51 to 75.
     * @n ff: fortissimo (Very Loud) for @link DefVel velocities@endlink from 76 to 100.
    */ 
    protected override void InitializeBuiltInDynamics()
    {
        // Set the number of Built-In Dynamics.
        mNumBuiltInDynamics = 3;

        // Set up the strings for creating the filenames.
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

        // Allocate the audio data.
        mAudioData = new float[mNumBuiltInDynamics][][];
        for( int i = 0; i < mNumBuiltInDynamics; i++ )
        {
            mAudioData[i] = new float[Music.MAX_SUPPORTED_NOTES][];
        }

    }

    /**
     * @brief Creates the filenames from which to load the wav files.
     * 
     * An example filename would be:
     * @n "Resources/Audio/VirtualInstrument/Piano/Samples/ff/C4ff"
    */
    protected override void CreateFilenames()
    {
        // Set the base filepath and number of files.
        mFilepath = "Audio/VirtualInstrument/Piano/Samples/";
        mNumFiles = 258;

        // Initialize the array of filenames.
        mFilenames = new string[mNumFiles];

        // Iterate through each dynamics value and create filenames for each supported note.
        int index = 0;
        for( int i = 0; i < mNumBuiltInDynamics; i++ )
        {
            for( int j = (int)mLowestSupportedPitch; j <= (int)mHighestSupportedPitch; j++ )
            {
                mFilenames[index] = mFilepath + mBuiltInDynamics[i] + "/" + Music.NoteToString( j ) + mBuiltInDynamics[i];
                index++;
            }
        }
    }
    /** @} */
}
