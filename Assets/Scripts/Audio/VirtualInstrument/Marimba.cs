using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Assertions;

/**
 * @class Marimba
 * @brief A specific type of @link VI Virtual Instrument@endlink that uses marimba samples.
 * 
 * The lowest supported @link Music::PITCH pitch@endlink of the marimba is @link Music::PITCH.C2 C2@endlink.
 * @n The highest supported @link Music::PITCH pitch@endlink of the marimba is @link Music::PITCH.C7 C7@endlink.
 * @n The marimba does not support @link DefBID Built-In Dynamics@endlink.
*/
public class Marimba : VirtualInstrument
{
    /*************************************************************************//** 
    * @defgroup MarConstruct Constructors
    * @ingroup DocMar
    * These are functions that create a new instance of a Marimba.
    * @{
    *****************************************************************************/

    /**
     * @brief Creates a new Marimba instance.
     * @param[in] aParent The @link VIM Virtual Instrument Manager@endlink that will manage this instrument.
     * @return A newly created Marimba @link VI Virtual Instrument@endlink.
    */
    public Marimba( VirtualInstrumentManager aParent ) : base( aParent )
    {

        // Set default values
        mIsDrum = false;
        mLowestSupportedPitch = Music.PITCH.C2;
        mHighestSupportedPitch = Music.PITCH.C7;
        mNumSupportedPitches = 61;
        mSampleRate = 44100;
        mSampleInterval = 1f / mSampleRate;

        // Call functions to set the values relating to built-in dynamics, and create the filenames for each sample.
        InitializeBuiltInDynamics();
        CreateFilenames();

        // Set that this instrument is loaded.
        mLoaded = true;

    }
    
    /*************************************************************************//** 
    * @}
    * @defgroup MarVirtFunc Implementations of Pure Virtual Functions
    * @ingroup DocMar
    * These are functions from the @link VIBase base class@endlink that are given implementations specific to the Marimba.
    * @{
    *****************************************************************************/

    /**
     * @brief Initializes values related to the @link DefBID Built-In Dynamics@endlink for this instrument.
     * 
     * There aren't built-in dynamics for the marimba, so the dynamics and thresholds are set to null.
    */
    protected override void InitializeBuiltInDynamics()
    {
        // There are no built-in dynamics for the marimba.
        mNumBuiltInDynamics = 0;
        mBuiltInDynamics = null;
        mBuiltInDynamicsThresholds = null;

        // Allocate the audio data container.
        mAudioData = new float[1][][];
        mAudioData[0] = new float[Music.MAX_SUPPORTED_NOTES][];
    }

    /**
     * @brief Creates the filenames of the WAV files used to create the Marimba.
     * 
     * The files are stored in "Audio/VirtualInstrument/Marimba/Samples".
    */ 
    protected override void CreateFilenames()
    {
        // Set the base filepath and number of files.
        mFilepath = "Audio/VirtualInstrument/Marimba/Samples/";
        mNumFiles = 61;

        // Initialize the array of filenames.
        mFilenames = new string[mNumFiles];

        // Iterate through each dynamics value and create filenames for each supported note.
        int index = 0;
        for( int i = (int)mLowestSupportedPitch; i <= (int)mHighestSupportedPitch; i++ )
        {
            mFilenames[index] = mFilepath + Music.NoteToString( i );
            index++;
        }
    }
    /** @} */
}
