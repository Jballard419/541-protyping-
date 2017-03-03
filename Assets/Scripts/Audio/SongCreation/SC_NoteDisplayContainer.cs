using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * @class SC_NoteDisplayContainer
 * @brief Connects the SC_MeasureDisplayPanel objects to the @link DocSC Song Creation Interface@endlink and provides handling for them.
*/
public class SC_NoteDisplayContainer : MonoBehaviour
{
    /*************************************************************************//** 
    * @defgroup SC_NDCConst Constants
    * @ingroup DocSC_NDC
    * Constants used by the SC_NoteDisplayContainer.
    * @{
    *****************************************************************************/
    private const string MEASURE_PANEL_PREFAB_PATH = "Audio/Prefabs/SongCreation/MeasurePanelPrefab"; //!< The path to load the prefab for the @link DocSC_MDP measure display panel objects@endlink.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDCPrivVar Private Variables
    * @ingroup DocSC_NDC
    * Variables used internally by the SC_NoteDisplayContainer.
    * @{
    *****************************************************************************/
    private int mNumNotes = 0; //!< The number of overall @link Music::CombinedNote notes@endlink in the container.
    private int mCurrentMeasure = 0; //!< The current @link DocSC_MDP measure@endlink.
    private List<SC_MeasureDisplayPanel> mMeasures = null; //!< The @link DocSC_MDP measures@endlink in the container.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDCUnity Unity Functions
    * @ingroup DocSC_NDC
    * Functions called automatically by Unity.
    * @{
    *****************************************************************************/

    /**
     * @brief Initializes the SC_NoteDisplayContainer.
    */
    private void Awake()
    {
        // Set up the list of measures.
        mMeasures = new List<SC_MeasureDisplayPanel>();

        // Create the first measure.
        GameObject firstMeasureObj = Instantiate( Resources.Load<GameObject>( MEASURE_PANEL_PREFAB_PATH ) );
        SC_MeasureDisplayPanel firstMeasure = firstMeasureObj.AddComponent<SC_MeasureDisplayPanel>();
        mMeasures.Add( firstMeasure );
        mMeasures[mCurrentMeasure].transform.GetChild( 0 ).GetChild( 1 ).GetComponent<Text>().text = "Measure " + ( mCurrentMeasure + 1 ).ToString();
        mMeasures[mCurrentMeasure].transform.SetParent( transform );
        mMeasures[mCurrentMeasure].SetParentContainer( this );
        mMeasures[mCurrentMeasure].SetToggle( true );
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDCPubFunc Public Functions
    * @ingroup DocSC_NDC
    * Functions for other classes to interact with the SC_NoteDisplayContainer.
    * @{
    *****************************************************************************/

    /**
     * @brief Adds a @link Music::CombinedNote note@endlink to the @link DocSC_MDP current measure@endlink. 
     * @param[in] aNote The todocnote to add.
     * 
     * This function just updates the index of the @link Music::CombinedNote note@endlink in the song 
     * and @link SC_MeasureDisplayPanel::AddNote sends it to the current measure@endlink for it to handle adding it.
    */
    public void AddNote( Music.CombinedNote aNote )
    {
        mMeasures[mCurrentMeasure].AddNote( aNote, mNumNotes );
        mNumNotes++;
    }

    /** 
     * @brief Clears all of the notes.
    */
    public void ClearNotes()
    {
        while( mCurrentMeasure >= 0 )
        {
            mMeasures[mCurrentMeasure].ClearMeasure();
            mMeasures.RemoveAt( mCurrentMeasure );
            mCurrentMeasure--;
        }
        mCurrentMeasure = 0;
        mNumNotes = 0;

        GameObject firstMeasureObj = Instantiate( Resources.Load<GameObject>( MEASURE_PANEL_PREFAB_PATH ) );
        SC_MeasureDisplayPanel firstMeasure = firstMeasureObj.AddComponent<SC_MeasureDisplayPanel>();
        mMeasures.Add( firstMeasure );
        mMeasures[mCurrentMeasure].transform.GetChild( 0 ).GetChild( 1 ).GetComponent<Text>().text = "Measure " + ( mCurrentMeasure + 1 ).ToString();
        mMeasures[mCurrentMeasure].transform.SetParent( transform );
        mMeasures[mCurrentMeasure].SetParentContainer( this );
        mMeasures[mCurrentMeasure].SetToggle( true );
    }

    /**
     * @brief Gets the current @link DocSC_MDP measure@endlink.
     * @return The current @link DocSC_MDP measure@endlink.
    */
    public SC_MeasureDisplayPanel GetCurrentMeasureObject()
    {
        return mMeasures[mCurrentMeasure];
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDCHandlers Event Handlers
    * @ingroup DocSC_NDC
    * Functions that are called by the SC_NoteDisplayContainer to handle events.
    * @{
    *****************************************************************************/

    /**
     * @brief Handler for when a @link DocSC_MDP measure@endlink fills up.
     * @param[in] aSpillover How much did the new note exceed the limit of the @link DocSC_MDP measure@endlink.
     * @param[in] aNote The todocnote that was attempted to be added to the todocmeasure.
     * 
     * This function creates a new @link DocSC_MDP measure@endlink and puts 
     * @link SC_NoteDisplayContainer::AddNote the note@endlink that couldn't be added to the last
     * @link DocSC_MDP measure@endlink into the new one.
    */
    public void HandleFullMeasure( float aSpillover, Music.CombinedNote aNote )
    {
        // Create a new measure toggle.
        GameObject clone = Instantiate( Resources.Load<GameObject>( MEASURE_PANEL_PREFAB_PATH ) );
        Assert.IsNotNull( clone, "Could not load the MeasurePanel prefab!" );

        // Add the measure panel to the list.
        mMeasures.Add( clone.AddComponent<SC_MeasureDisplayPanel>() );

        // Increase the current measure.
        mCurrentMeasure++;

        // Set the values for the new measure toggle.
        mMeasures[mCurrentMeasure].transform.GetChild( 0 ).GetChild( 1 ).GetComponent<Text>().text = "Measure " + ( mCurrentMeasure + 1 ).ToString();
        mMeasures[mCurrentMeasure].transform.SetParent( transform );
        mMeasures[mCurrentMeasure].SetParentContainer( this );
        mMeasures[mCurrentMeasure].SetToggle( true );

        // Handle Spillover from the previous measure.
        mMeasures[mCurrentMeasure].SetPercentageUsed( 0f - aSpillover );

        // Add the note to the new measure.
        mMeasures[mCurrentMeasure].AddNote( aNote, mNumNotes );

        // Make only the new measure be shown.
        HandleMeasureToggled( mMeasures[mCurrentMeasure] );
    }

    /**
     * @brief Handles when a @link DocSC_MDP measure@endlink is @link SC_MeasureDisplayPanel::OnShowToggle toggled@endlink.
     * @param[in] aMeasure The @link DocSC_MDP measure@endlink that was toggled.
     * This function sets only the toggled @link DocSC_MDP measure@endlink to be shown.
    */
    public void HandleMeasureToggled( SC_MeasureDisplayPanel aMeasure )
    {
        // Set that only the toggled measure should be shown.
        foreach( SC_MeasureDisplayPanel measure in mMeasures )
        {
            if( aMeasure != measure )
            {
                measure.SetToggle( false );
            }
            else
            {
                measure.SetToggle( true );
            }
        }
    }
}
 
 