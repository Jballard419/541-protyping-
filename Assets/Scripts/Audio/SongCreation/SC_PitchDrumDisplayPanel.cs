using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

/**
 * @class SC_PitchDrumDisplayPanel
 * @brief A panel for displaying a pitch and being able to set its values.
*/
public class SC_PitchDrumDisplayPanel : MonoBehaviour
{
    /*************************************************************************//** 
    * @defgroup SC_PDDPPrivVar Private Variables
    * @ingroup DocSC_PDDP
    * These are variables that are used internally by the SC_PitchDrumDisplayPanel
    * @{
    *****************************************************************************/
    private bool mIsDrum = false; //!< Whether or not the panel is representing a @link Music::DRUM drum@endlink.
    private Button mRemoveButton = null; //!< The button to remove the @link Music::PITCH pitch@endlink/@link Music::DRUM drum@endlink.
    private int mNoteVelocity = 100; //!< The @link DefVel Note Velocity of the represented @link Music::PITCH pitch@endlink/@link Music::DRUM drum@endlink.
    private GameObject mDotModifierPanel = null; //!< The panel for displaying whether or not the @link Music::NoteLength.Dot note is dotted@endlink.
    private GameObject mLengthPanel = null; //!< The panel for displaying a @link Music::NOTE_LENGTH_BASE note's base length@endlink.
    private GameObject mLengthAndModifiersPanel = null; //!< The panel to display the @link Music::NoteLength note's length with modifiers@endlink.
    private GameObject mTripletModifierPanel = null; //!< The panel to display whether or not the @link Music::NoteLength.Triplet note is a triplet@endlink.
    private Music.PITCH mPitch; //!< The @link Music::MelodyNote note@endlink being represented if not representing drums.
    private Music.DRUM mDrum; //!< The drum hit being represented if it this panel is representing a drum.
    private Music.NoteLength mLength; //!< The @link Music::NoteLength length of the pitch being represented@endlink.
    private SC_NoteDialog mParent = null; //!< The todocparent.
    private SongCreationManager.SC_InputFieldAndSlider mNoteVelocityHandler; //!< The handler for setting the @link DefVel Note Velocity@endlink of the @link Music::PITCH pitch@endlink/@link Music::DRUM drum@endlink
    private Text mPitchDisplay = null; //!< The text displaying the represented @link Music::PITCH pitch@endlink/@link Music::DRUM drum@endlink.
    
    /*************************************************************************//** 
    * @}
    * @defgroup SC_PDDPUnityFunctions Unity Functions
    * @ingroup DocSC_PDDP
    * These are functions that are automatically called by Unity.
    * @{
    *****************************************************************************/

    /**
     * @brief Gets references to the objects in the panel.
    */
    private void Awake ()
    {
        // Get the label for the represented pitch/drum.
        mPitchDisplay = transform.GetChild( 0 ).GetComponent<Text>();

        // Get the length/modifier panel and its children.
        mLengthAndModifiersPanel = transform.GetChild( 1 ).gameObject;
        mLengthPanel = mLengthAndModifiersPanel.transform.GetChild( 0 ).gameObject;
        mDotModifierPanel = mLengthAndModifiersPanel.transform.GetChild( 1 ).gameObject;
        mTripletModifierPanel = mLengthAndModifiersPanel.transform.GetChild( 2 ).gameObject;

        // Get the Note Velocity Slider
        mNoteVelocityHandler = transform.GetChild( 3 ).gameObject.AddComponent<SongCreationManager.SC_InputFieldAndSlider>();
        mNoteVelocityHandler.SetReferenceToInputField( transform.GetChild( 2 ).GetComponent<InputField>() );
        mNoteVelocityHandler.SetAsInt( true );
        mNoteVelocityHandler.ValueChanged.AddListener( OnNoteVelocityChanged );

        // Get the Remove Button.
        mRemoveButton = transform.GetChild( 4 ).GetComponent<Button>();
        mRemoveButton.onClick.AddListener( OnRemoveButtonClicked );
	}

    /*************************************************************************//** 
    * @}
    * @defgroup SC_PDDPPubFunc Public Functions
    * @ingroup DocSC_PDDP
    * These are functions that allow other classes to interact with the SC_PitchDrumDisplayPanel.
    * @{
    *****************************************************************************/

    /**
     * @brief Gets the represented @link Music::PercussionNote percussion note@endlink.
     * @return The represented @link Music::PercussionNote percussion note@endlink.
    */
    public Music.DRUM GetDrum()
    {
        Assert.IsTrue( mIsDrum, "Tried to get the percussion note from a pitch/drum display panel that was representing a melody note!" );
        return mDrum;
    }

    /**
     * @brief Gets the @link Music::NoteLength length@endlink of the represented @link Music::PITCH pitch@endlink if this panel is representing a @link Music::PITCH pitch@endlink.
     * @return The @link Music::NoteLength length@endlink of the represented @link Music::PITCH pitch@endlink if this panel is representing a @link Music::PITCH pitch@endlink.
    */
    public Music.NoteLength GetLengthOfPitch()
    {
        Assert.IsFalse( mIsDrum, "Tried to get the length of a pitch from a pitch/drum display panel that represented a drum!" );

        return mLength;
    }

    /** 
    * @brief Sets up the SC_PitchDrumDisplayPanel as a display panel for a @link Music::DRUM drum@endlink.
    * @param[in] aDrum The @link Music::DRUM drum@endlink that is being represented.
    * @param[in] aNoteVelocity @copydoc SC_PitchDrumDisplayPanel::mNoteVelocity
    */
    public void InitializeAsDrumDisplay( Music.DRUM aDrum, int aNoteVelocity )
    {
        // Set that the panel is representing drums.
        mIsDrum = true;

        // Set the drum
        mDrum = aDrum;

        // Set the pitch display text.
        mPitchDisplay.text = "Drum:\n" + Music.DrumToString( aDrum );

        // Hide the length/modifiers panel.
        mLengthAndModifiersPanel.SetActive( false );

        // Set the velocity value.
        mNoteVelocity = aNoteVelocity;
        mNoteVelocityHandler.SetValue( aNoteVelocity );
    }

    /** 
     * @brief Sets up the SC_PitchDrumDisplayPanel as a display panel for @link Music::PITCH pitches@endlink.
     * @param[in] aPitch The @link Music::PITCH pitch@endlink that is being represented.
     * @param[in] aLength @copydoc SC_PitchDrumDisplayPanel::mLength
     * @param[in] aNoteVelocity @copydoc SC_PitchDrumDisplayPanel::mNoteVelocity
    */
    public void InitializeAsPitchDisplay( Music.PITCH aPitch, Music.NoteLength aLength, int aNoteVelocity )
    {
        // Set that the panel is not representing drums.
        mIsDrum = false;

        // Set the pitch.
        mPitch = aPitch;

        // Set the pitch display text.
        mPitchDisplay.text = "Pitch:\n" + Music.NoteToString( aPitch );

        // Set the length.
        mLength = aLength;

        // See if we need to set the length image..
        if( aLength.BaseLength == Music.NOTE_LENGTH_BASE.NONE )
        {
            mLengthPanel.SetActive( false );
            mDotModifierPanel.SetActive( false );
            mTripletModifierPanel.SetActive( false );
        }
        else
        {
            // Set the length image.
            mLengthPanel.transform.GetChild( 1 ).GetComponent<Image>().sprite = Music.GetImageForNoteLength( aLength.BaseLength );

            // Set the modifier images.
            if( !aLength.Dot )
            {
                mDotModifierPanel.SetActive( false );
            }
            if( !aLength.Triplet )
            {
                mTripletModifierPanel.SetActive( false );
            }
        }

        
        // Set the velocity.
        mNoteVelocity = aNoteVelocity;
        mNoteVelocityHandler.SetValue( aNoteVelocity );
    }

    /**
     * @brief Gets whether or not the panel is representing a todocdrum
     * @return Whether or not the panel is representing a todocdrum
    */
    public bool IsDrum()
    {
        return mIsDrum;
    }

    /**
     * @brief Gets the represented @link Music::MelodyNote melody note@endlink.
     * @return The represented @link Music::MelodyNote melody note@endlink.
    */
    public Music.PITCH GetPitch()
    {
        Assert.IsFalse( mIsDrum, "Tried to get the pitch from a pitch/drum display panel that was representing a drum!" );
        return mPitch;
    }

    /**
     * @brief Gets the @link DefVel Note Velocity for the @link Music::PITCH pitch@endlink/@link Music::DRUM drum@endlink.
     * @return The @link DefVel Note Velocity for the @link Music::PITCH pitch@endlink/@link Music::DRUM drum@endlink.
    */
    public int GetNoteVelocity()
    {
        return mNoteVelocity;
    }

    /**
     * @brief Sets the todocvelocity.
     * @param[in] aVelocity The new todocvelocity
    */
    public void SetNoteVelocity( float aVelocity )
    {
        mNoteVelocity = (int)aVelocity;
        mNoteVelocityHandler.SetValue( aVelocity );
    }

    /**
     * @brief Sets the todocparent
     * @param[in] aParent The todocparent
    */
    public void SetParentContainer( SC_NoteDialog aParent )
    {
        mParent = aParent;
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_PDDPHandlers Event Handlers
    * @ingroup DocSC_PDDP
    * These are functions that are called by the SC_PitchDrumDisplayPanel in response to events.
    * @{
    *****************************************************************************/

    /**
     * @brief Handles a change in the @link SC_PitchDrumDisplayPanel::mNoteVelocitySlider Note Velocity Slider@endlink.
     * @param[in] aNewNoteVelocity The new @link DefVel Note Velocity@endlink
     * 
     * This function updates the @link SC_PitchDrumDisplayPanel::mNoteVelocity member variable@endlink and the
     * @link SC_PitchDrumDisplayPanel::mInputField input field@endlink.
    */
    private void OnNoteVelocityChanged( float aNewNoteVelocity )
    {
        Assert.IsTrue( aNewNoteVelocity >= 0 && aNewNoteVelocity <= 100, "Somehow we tried giving a velocity outside of the range from 0-100. Velocity Given: " + aNewNoteVelocity.ToString() );

        // Update the member variable.
        mNoteVelocity = (int)aNewNoteVelocity;
    }

    /**
     * @brief Handles when the remove button is clicked.
    */
    private void OnRemoveButtonClicked()
    {
        mParent.HandlePanelRemoved( this );
        Destroy( gameObject );
    }
    /** @} */
}
