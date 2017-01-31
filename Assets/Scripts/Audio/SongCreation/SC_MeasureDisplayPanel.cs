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
        mShowMeasureToggle = gameObject.transform.GetChild( 0 ).GetComponent<Toggle>();
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
     * @param[in] aNoteIndex The index of the @link Music::CombinedNote note@endlink in the overall Song.
     * @param[in] aMelodyVelocity The @link DefVel velocity@endlink of the @link Music::MelodyNote pitches@endlink in the @link Music::CombinedNote note@endlink.
     * @param[in] aLength The @link Music::NOTE_LENGTH length@endlink of the @link Music::CombinedNote note@endlink.
     * @param[in] aPitches The @link Music::PITCH pitches@endlink in the @link Music::CombinedNote note@endlink.
     * @param[in] aDrumVelocity The @link DefVel velocity@endlink of the @link Music::PercussionNote drums@endlink in the @link Music::CombinedNote note@endlink.
     * @param[in] aDrumHits The @link Music::DRUM drums@endlink that are hit for this @link Music::CombinedNote note@endlink.
     * @param[in] aOffsetFromPrevNote The @link Music::CombinedNote.OffsetFromPrevNote offset from the previous note@endlink.
     * 
     * This function also creates the @link DocSC_NDP SC_NoteDisplayPanel object@endlink for the @link Music::CombinedNote note@endlink.
    */
    public void AddNote( int aNoteIndex, int aMelodyVelocity, Music.NOTE_LENGTH aLength, Music.PITCH[] aPitches,
        int aDrumVelocity, Music.DRUM[] aDrumHits, Music.NOTE_LENGTH aOffsetFromPrevNote )
    {
        // Declare variables to help create the note.
        Sprite[] sprites = mParent.GetSprites();
        string pitches = "None";
        string drums = "None";


        // Calculate the percentage used up in the measure.
        float newPercentage = mPercentageUsed + Music.GetNoteLengthRelativeToMeasure( aOffsetFromPrevNote, Music.TIME_SIGNATURE_4_4() );

        // If there is no more room in the measure for this note, then send it back to the parent.
        if( newPercentage >= 1f )
        {
            mParent.HandleFullMeasure( this, newPercentage - mPercentageUsed, aMelodyVelocity, aLength, aPitches, aDrumVelocity, aDrumHits, aOffsetFromPrevNote );
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
            clone.transform.localScale = Vector3.one;

            // Set the pitch string if needed.
            if( aPitches != null )
            {
                pitches = "";
                for( int i = 0; i < aPitches.Length; i++ )
                {
                    if( aPitches[i] == Music.PITCH.REST )
                    {
                        pitches += "Rest ";
                    }
                    else
                    {
                        pitches += ( Music.NoteToString( aPitches[i] ) + " " );
                    }
                }
            }

            // Set the drum string if needed.
            if( aDrumHits != null )
            {
                drums = "";
                foreach( Music.DRUM drum in aDrumHits )
                {
                    drums += Music.DrumToString( drum ) + " ";
                }
            }

            // Initialize the cloned panel.
            SC_NoteDisplayPanel newPanel = clone.AddComponent<SC_NoteDisplayPanel>();
            newPanel.SetDrums( drums );
            newPanel.SetPitches( pitches );
            newPanel.SetLengthImage( sprites[(int)aLength] );
            newPanel.SetOffsetImage( sprites[(int)aOffsetFromPrevNote] );
            newPanel.SetOffset( aOffsetFromPrevNote );
            newPanel.SetVelocity( aMelodyVelocity.ToString() );
            newPanel.SetParentContainer( this );
            newPanel.SetNoteIndex( aNoteIndex );

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

    /**
     * @brief Removes a @link Music::CombinedNote note@endlink.
     * @param[in] aNoteDisplayPanel The panel displaying the note to be removed.
    */ 
    public void OnRemoveNote( SC_NoteDisplayPanel aNoteDisplayPanel )
    {
        // If there is more than one note in the measure, then just remove it from the measure.
        if( mNotes.Count > 1 )
        {
            // Find the note.
            bool found = false;
            int index = 0;
            while( !found && index < mNotes.Count )
            {
                if( aNoteDisplayPanel == mNotes[index] )
                {
                    found = true;
                }
                else
                {
                    index++;
                }
            }
            SC_NoteDisplayPanel temp = mNotes[index];

            // Remove the note from the list.
            mNotes.RemoveAt( index );

            // Update the percentage used.
            mPercentageUsed -= Music.GetNoteLengthRelativeToMeasure( temp.GetOffset(), Music.TIME_SIGNATURE_4_4() );

            // Destroy the note object.
            DestroyImmediate( temp.gameObject, false );

        }
        // If there aren't any notes left, then handle this measure being deleted.
        else
        {
            SC_NoteDisplayPanel temp = mNotes[0];
            mNotes.Clear();
            DestroyImmediate( temp.gameObject, false );
            mParent.HandleMeasureDeleted( this );
        }
        mParent.OnRemoveNote();
    }
    /** @} */
}

#endif