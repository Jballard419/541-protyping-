using System.Collections;
using System.Collections.Generic;

/**
 * @class DrumKit
 * @brief A specific instance of a VirtualInstrument that uses drum/cymbal samples. 
 * 
 * Currently, the drum kit supports playing @link Music::DRUM 19 separate drums/cymbals@endlink.
 * The kit only uses 13 sounds though, as the snare/tom sound files are duplicated for 
 * two entries each. This is done so that fast series of notes can be played on a tom
 * or snare by spreading them across two keys rather than one. @n
 * Though there is @link Music::DRUM an enum for drums@endlink, the @link VIBase base class@endlink
 * deals in @link Music::PITCH pitches@endlink. In order to account for this without
 * having to duplicate a lot of code, the DrumKit actually acts on the pitch range
 * from @link Music::PITCH::C0 C0@endlink to @link Music::PITCH::F1 F1@endlink. 
 * @n Currently, @link DefBID Built-In Dynamics@endlink are not supported for the 
 * DrumKit, but they will be in the future.
 * @see VirtualInstrumentManager::mDrumLoopOutput Music::INSTRUMENT_TYPE::DRUM_KIT VirtualInstrument::mIsDrum
*/
public class DrumKit : VirtualInstrument
{
    /*************************************************************************//** 
     * @defgroup DrumConstruct Constructors
     * @ingroup DocDrum
     * Constructors to create the DrumKit.
     * @{
    *****************************************************************************/

    /**
     * @brief Creates a new Drum Kit instance.
     * @param[in] aParent The @link VIM Virtual Instrument Manager@endlink that will manage this instrument.
     * @return A newly created DrumKit
    */
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

    /*************************************************************************//** 
     * @}
     * @defgroup DrumVirtFunc Implemented Virtual Functions
     * @ingroup DocDrum
     * Functions from the @link VIBase base class@endlink that are implemented 
     * specifically for DrumKits
     * @{
    *****************************************************************************/

    /**
     * @brief Initializes values related to the @link DefBID Built-In Dynamics@endlink for this instrument.
     * 
     * @note There are \(currently\) no @link DefBID Built-In Dynamics@endlink for the DrumKit. This just sets the variables such that other functions will know that it is not supported.
    */
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

    /**
     * @brief Creates the filenames for each drum wav file.
     * 
     * The files are stored in "Resources/Audio/VirtualInstrument/DrumKit/Samples".
    */ 
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
