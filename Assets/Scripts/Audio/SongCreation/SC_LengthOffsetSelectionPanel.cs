using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

/**
 * @class SC_LengthOffsetSelectionPanel
 * @brief A class that handles selecting a @link Music::NoteLength note's length@endlink or @link Music::CombinedNote.OffsetFromPrevNote offset from the previous note@endlink.
*/
public class SC_LengthOffsetSelectionPanel : MonoBehaviour
{

    /*************************************************************************//** 
    * @defgroup SC_LOSPPrivVar Private Variables
    * @ingroup DocSC_LOSP
    * Variables that are used internally by the SC_LengthOffsetSelectionPanel.
    * @{
    *****************************************************************************/
    private Color32 mSelectedColor; //!< The color of toggles that are selected.
    private Color32 mUnselectedColor; //!< The color of toggles that are unselected.
    private Music.NoteLength mNoteLength; //!< The @link Music::NoteLength length of the note@endlink that is being selected.
    private Toggle mDotToggle = null; //!< The toggle to indicate whether or not the @link Music::CombinedNote note@endlink is @link Music::NoteLength.Dot dotted@endlink.
    private Toggle mTripletToggle = null; //!< The toggle to indicate whether or not the @link Music::CombinedNote note@endlink is a @link Music::NoteLength.Triplet triplet@endlink.
    private Toggle[] mBaseLengthToggles = null; //!< The toggle switches for selecting the @link Music::NOTE_LENGTH_BASE base note length@endlink.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_LOSPUnity Unity Functions
    * @ingroup DocSC_LOSP
    * Functions that are called automatically by Unity.
    * @{
    *****************************************************************************/

    /**
     * @brief Initializes the SC_LengthOffsetSelectionPanel@endlink.
    */
    private void Awake()
    {
        mDotToggle = transform.GetChild( 0 ).GetComponent<Toggle>();
        mDotToggle.onValueChanged.AddListener( OnDotToggleValueChanged );
        mTripletToggle = transform.GetChild( 1 ).GetComponent<Toggle>();
        mTripletToggle.onValueChanged.AddListener( OnTripletToggleValueChanged );
        mBaseLengthToggles = new Toggle[7];
        for( int i = 0; i < 7; i++ )
        {
            mBaseLengthToggles[i] = transform.GetChild( i + 2 ).GetComponent<Toggle>();
            mBaseLengthToggles[i].onValueChanged.AddListener( OnBaseLengthToggleChanged );
            mBaseLengthToggles[i].isOn = false;
        }
        mSelectedColor = new Color32( 255, 255, 255, 118 );
        mUnselectedColor = new Color32( 255, 255, 255, 255 );  
	}

    /*************************************************************************//** 
    * @}
    * @defgroup DocSC_LOSPPubFunc Public Functions
    * @ingroup DocSC_LOSP
    * Functions that allow other classes to interact with the SC_LengthOffsetSelectionPanel.
    * @{
    *****************************************************************************/

    /**
     * @brief Gets the @link Music::NoteLength note length@endlink from the panel.
     * @return The selected @link Music::NoteLength note length@endlink.
    */
    public Music.NoteLength GetSelected()
    {
        return mNoteLength;
    }

    /**
     * @brief Sets the @link Music::NoteLength note length@endlink for the panel.
     * @param[in] aNoteLength The @link Music::NoteLength note length@endlink for the panel.
    */
    public void SetSelected( Music.NoteLength aNoteLength )
    {
        mNoteLength = aNoteLength;
        mDotToggle.isOn = aNoteLength.Dot;
        mTripletToggle.isOn = aNoteLength.Triplet;
        mBaseLengthToggles[(int)aNoteLength.BaseLength].isOn = true;
        UpdateGraphics();
    }

    /*************************************************************************//** 
    * @}
    * @defgroup DocSC_LOSPPrivFunc Private Functions
    * @ingroup DocSC_LOSP
    * Functions that are used internally by the SC_LengthOffsetSelectionPanel.
    * @{
    *****************************************************************************/
    
    /**
     * @brief Updates the graphics of the toggle switches to show which ones are selected.
    */
    private void UpdateGraphics()
    {
        // Update the Dot toggle graphic.
        if( mNoteLength.Dot )
        {
            mDotToggle.transform.GetChild( 0 ).GetComponent<Image>().color = mSelectedColor;
        }
        else
        {
            mDotToggle.transform.GetChild( 0 ).GetComponent<Image>().color = mUnselectedColor;
        }

        // Update the Triplet toggle graphic.
        if( mNoteLength.Triplet )
        {
            mTripletToggle.transform.GetChild( 0 ).GetComponent<Image>().color = mSelectedColor;
        }
        else
        {
            mTripletToggle.transform.GetChild( 0 ).GetComponent<Image>().color = mUnselectedColor;
        }

        // Update the base length toggle graphics.
        for( int i = 0; i < 7; i++ )
        {
            if( mBaseLengthToggles[i].isOn )
            {
                mBaseLengthToggles[i].transform.GetChild( 0 ).GetComponent<Image>().color = mSelectedColor;
            }
            else
            {
                mBaseLengthToggles[i].transform.GetChild( 0 ).GetComponent<Image>().color = mUnselectedColor;
            }
        }
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_LOSPHandlers Event Handlers
    * @ingroup DocSC_LOSP
    * Functions that are called by the SC_LengthOffsetSelectionPanel in order to handle events.
    * @{
    *****************************************************************************/

    /**
     * @brief Handles the @link Music::NoteLength.Dot note being dotted or undotted@endlink.
     * @param[in] aIsDotted Whether or not the @link Music::NoteLength note length@endlink is @link Music::NoteLength.Dot dotted@endlink.
    */
    private void OnDotToggleValueChanged( bool aIsDotted )
    {
        mNoteLength.Dot = aIsDotted;
        UpdateGraphics();
    }

    /**
     * @brief Handles one of the @link SC_LengthOffsetSelectionPanel::mBaseLengthToggles base length toggles@endlink being toggled.
     * @param[in] aSelected True if the toggle is selected. False otherwise.
    */
    private void OnBaseLengthToggleChanged( bool aSelected )
    {
        // Set the selected base length
        if( aSelected )
        {
            for( int i = 0; i < 7; i++ )
            {
                if( mBaseLengthToggles[i].isOn )
                {
                    mNoteLength.BaseLength = (Music.NOTE_LENGTH_BASE)i;
                }
            }
            UpdateGraphics();
        }
    }

    /**
     * @brief Handles the @link Music::NoteLength length of the note@endlink being a @link Music::NoteLength.Triplet triplet@endlink.
     * @param[in] aIsTriplet Whether or not the @link Music::NoteLength length of the note@endlink is a @link Music::NoteLength.Triplet triplet@endlink.
    */
    private void OnTripletToggleValueChanged( bool aIsTriplet )
    {
        // Update the selected NoteLength.
        mNoteLength.Triplet = aIsTriplet;
        UpdateGraphics();
    }
    /** @} */
}
