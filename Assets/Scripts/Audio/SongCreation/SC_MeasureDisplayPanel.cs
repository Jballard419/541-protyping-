#if SONG_CREATION_ENABLED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * @class SC_MeasureDisplayPanel
 * @brief Class that handles a specific measure of the Song that is being created.
 * 
 * This class allows for showing specific measures of the Song at a time
 * in order to cut down on clutter.
*/
public class SC_MeasureDisplayPanel : MonoBehaviour
{
    /*************************************************************************//** 
    * @defgroup SC_MDPConst Constants
    * @ingroup DocSC_MDP
    * Constants used by the SC_MeasureDisplayPanel.
    * @{
    *****************************************************************************/
    private string NOTE_DISPLAY_PANEL_PATH = "Audio/Prefabs/SongCreation/NoteDisplayPanelPrefab"; //!< The path to load the @link DocSC_NDP SC_NoteDisplayPanel@endlink's prefab.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_MDPPrivVar Private Variables
    * @ingroup DocSC_MDP
    * Variables used internally by the SC_MeasureDisplayPanel.
    * @{
    *****************************************************************************/
    private float mPercentageUsed = 0f; //!< The percentage of the measure used.
    private List<SC_NoteDisplayPanel> mNotes = null; //!< The notes in the measure.
    private SC_NoteDisplayContainer mParent = null; //!< The parent container 
    private Toggle mShowMeasureToggle = null; //!< The toggle switch for showing this measure.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_MDPUnity Unity Functions
    * @ingroup DocSC_MDP
    * Functions called automatically by Unity.
    * @{
    *****************************************************************************/

    /**
     * @brief Initializes the SC_MeasureDisplayPanel
    */
    private void Awake()
    {
        // Set the toggle.
        mShowMeasureToggle = transform.GetChild( 0 ).GetComponent<Toggle>();
        mShowMeasureToggle.onValueChanged.AddListener( OnShowToggle );

        // Set up the list of note display panels.
        mNotes = new List<SC_NoteDisplayPanel>();
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_MDPPubFunc Public Functions
    * @ingroup DocSC_MDP
    * Functions for other classes to interact with the SC_MeasureDisplayPanel.
    * @{
    *****************************************************************************/

    /**
     * @brief Adds a @link Music::CombinedNote note@endlink to the @link SC_MeasureDisplayPanel::mNotes list of notes managed by this panel@endlink. 
     * @param[in] aNote The todoc note to add to the measure.
     * @param[in] aNoteIndex The index of the @link Music::CombinedNote note@endlink in the overall Song.
     * 
     * This function also creates the @link DocSC_NDP SC_NoteDisplayPanel object@endlink for the @link Music::CombinedNote note@endlink.
    */
    public void AddNote( Music.CombinedNote aNote, int aNoteIndex )
    {
        // Calculate the percentage used up in the measure.
        float newPercentage = mPercentageUsed + Music.GetNoteLengthRelativeToMeasure( aNote.OffsetFromPrevNote, Music.TIME_SIGNATURE_4_4() );

        // If there is no more room in the measure for this note, then send it back to the parent.
        if( newPercentage >= 1f )
        {
            mParent.HandleFullMeasure( newPercentage - mPercentageUsed, aNote );
        }

        // If there is more room, then create the new panel and add it to this measure.
        else
        {
            // Update the percentage used.
            mPercentageUsed = newPercentage;

            // Create a SC_NoteDisplayPanel
            GameObject clone = null;
            clone = Instantiate( Resources.Load<GameObject>( NOTE_DISPLAY_PANEL_PATH ) );
            Assert.IsNotNull( clone, "Could not load SC_NoteDisplayPanel prefab!" );

            // Set the values for the note's transform.
            clone.transform.SetParent( transform.parent );

            // Initialize the cloned panel.
            SC_NoteDisplayPanel newPanel = clone.AddComponent<SC_NoteDisplayPanel>();
            newPanel.LoadNote( aNote, aNoteIndex );

            // Add the panel to the list.
            mNotes.Add( newPanel );
        }
    }

    /**
     * @brief Clears the notes in the measure and self-destructs
    */
    public void ClearMeasure()
    {
        foreach( SC_NoteDisplayPanel note in mNotes )
        {
            DestroyImmediate( note.gameObject, false );
        }
        DestroyImmediate( gameObject, false );
    }

    /**
     * @brief Sets the @link DocSC_NDC parent container@endlink.
     * @param[in] aParent The @link DocSC_NDC parent container@endlink.
    */
    public void SetParentContainer( SC_NoteDisplayContainer aParent )
    {
        // Set the parent.
        mParent = aParent;
    }

    /**
     * @brief Sets the percentage of the measure that is filled.
     * @param[in] aPercent How full the measure is.
     * 
     * @see Song::GetNoteLengthRelativeToMeasure
    */
    public void SetPercentageUsed( float aPercent )
    {
        mPercentageUsed = aPercent;
    }

    /**
     * @brief Sets whether or not the measure should be shown.
     * @param[in] aShowMeasure True if the measure should be shown. False otherwise.
    */
    public void SetToggle( bool aShowMeasure )
    {
        // Show/Hide the notes in the measure depending on the given bool.
        foreach( SC_NoteDisplayPanel note in mNotes )
        {
            note.gameObject.SetActive( aShowMeasure );
        }

        // Update the toggle value.
        mShowMeasureToggle.onValueChanged.RemoveListener( OnShowToggle );
        mShowMeasureToggle.isOn = aShowMeasure;
        mShowMeasureToggle.onValueChanged.AddListener( OnShowToggle );
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_MDPHandlers Event Handlers
    * @ingroup DocSC_MDP
    * Functions called by the SC_MeasureDisplayPanel in order to handle events.
    * @{
    *****************************************************************************/

    /**
     * @brief Handles showing the measure or hiding it. 
     * @param[in] aShowMeasure True if the measure should be shown. False otherwise.
     * 
     * This just passes it from the @link SC_MeasureDisplayPanel::mShowMeasureToggle toggle switch@endlink 
     * to the @link SC_NoteDisplayContainer::HandleMeasureToggled parent's handling function@endlink.
    */
    public void OnShowToggle( bool aShowMeasure )
    {
        mParent.HandleMeasureToggled( this );
    }
    /** @} */
}

#endif