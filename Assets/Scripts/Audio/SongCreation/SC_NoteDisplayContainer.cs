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
    private int mCurrentMeasure = -1; //!< The current @link DocSC_MDP measure@endlink.
    private List<SC_MeasureDisplayPanel> mMeasures = null; //!< The @link DocSC_MDP measures@endlink in the container.
    private Sprite[] mSprites = null; //!< The images to show the @link Music::MelodyNote.Length note lengths@endlink and @link Music::CombinedNote.OffsetFromPrevNote offsets@endlink.

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
        // Set up the measures.
        mMeasures = new List<SC_MeasureDisplayPanel>();

        // Load the note length/offset images
        mSprites = new Sprite[13];
        mSprites[0] = Resources.Load<Sprite>( "Audio/Images/32nd" );
        mSprites[1] = Resources.Load<Sprite>( "Audio/Images/dotted32nd" );
        mSprites[2] = Resources.Load<Sprite>( "Audio/Images/16th" );
        mSprites[3] = Resources.Load<Sprite>( "Audio/Images/dotted16th" );
        mSprites[4] = Resources.Load<Sprite>( "Audio/Images/eighth" );
        mSprites[5] = Resources.Load<Sprite>( "Audio/Images/dottedEighth" );
        mSprites[6] = Resources.Load<Sprite>( "Audio/Images/quarter" );
        mSprites[7] = Resources.Load<Sprite>( "Audio/Images/dottedQuarter" );
        mSprites[8] = Resources.Load<Sprite>( "Audio/Images/half" );
        mSprites[9] = Resources.Load<Sprite>( "Audio/Images/dottedHalf" );
        mSprites[10] = Resources.Load<Sprite>( "Audio/Images/whole" );
        mSprites[11] = Resources.Load<Sprite>( "Audio/Images/dottedWhole" );
        mSprites[12] = null;
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
     * @param[in] aMelodyVelocity The @link DefVel velocity@endlink of the @link Music::MelodyNote pitches@endlink in the @link Music::CombinedNote note@endlink.
     * @param[in] aLength The @link Music::NOTE_LENGTH length@endlink of the @link Music::CombinedNote note@endlink.
     * @param[in] aPitches The @link Music::PITCH pitches@endlink in the @link Music::CombinedNote note@endlink.
     * @param[in] aDrumVelocity The @link DefVel velocity@endlink of the @link Music::PercussionNote drums@endlink in the @link Music::CombinedNote note@endlink.
     * @param[in] aDrumHits The @link Music::DRUM drums@endlink that are hit for this @link Music::CombinedNote note@endlink.
     * @param[in] aOffsetFromPrevNote The @link Music::CombinedNote.OffsetFromPrevNote offset from the previous note@endlink.
     * 
     * This function just updates the index of the @link Music::CombinedNote note@endlink in the song 
     * and @link SC_MeasureDisplayPanel::AddNote sends it to the current measure@endlink for it to handle adding it.
    */
    public void AddNote( int aMelodyVelocity, Music.NOTE_LENGTH aLength, Music.PITCH[] aPitches, int aDrumVelocity, Music.DRUM[] aDrumHits, Music.NOTE_LENGTH aOffsetFromPrevNote )
    {
        // Create a measure if none are present.
        if( mCurrentMeasure == -1 )
        {
            GameObject newMeasure = Instantiate( Resources.Load<GameObject>( MEASURE_PANEL_PREFAB_PATH ) );
            Assert.IsNotNull( newMeasure, "Could not load the MeasurePanel prefab!" );

            // Add the measure panel to the list.
            mMeasures.Add( newMeasure.AddComponent<SC_MeasureDisplayPanel>() );

            // Increase the current measure.
            mCurrentMeasure++;

            // Set the values for the new measure toggle.
            mMeasures[mCurrentMeasure].transform.GetChild( 0 ).GetChild( 1 ).GetComponent<Text>().text = "Measure " + ( mCurrentMeasure + 1 ).ToString();
            mMeasures[mCurrentMeasure].transform.SetParent( gameObject.transform );
            mMeasures[mCurrentMeasure].SetParentContainer( this );
            mMeasures[mCurrentMeasure].transform.localScale = Vector3.one;
            mMeasures[mCurrentMeasure].SetToggle( true );
        }
        mMeasures[mCurrentMeasure].AddNote( mNumNotes, aMelodyVelocity, aLength, aPitches, aDrumVelocity, aDrumHits, aOffsetFromPrevNote );
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
        mNumNotes = 0;
    }

    /**
     * @brief Gets the current @link DocSC_MDP measure@endlink.
     * @return The current @link DocSC_MDP measure@endlink.
    */
    public SC_MeasureDisplayPanel GetCurrentMeasureObject()
    {
        return mMeasures[mCurrentMeasure];
    }

    /**
     * @brief Gets the images used to represent a note's @link Music::MelodyNote::Length length@endlink/@link Music::CombinedNote.OffsetFromPrevNote offset@endlink.
     * @return The images used to represent a note's @link Music::MelodyNote::Length length@endlink/@link Music::CombinedNote.OffsetFromPrevNote offset@endlink.
    */
    public Sprite[] GetSprites()
    {
        return mSprites;
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
     * @param[in] aFullMeasure The @link DocSC_MDP measure@endlink that filled up.
     * @param[in] aSpillover How much did the new note exceed the limit of the @link DocSC_MDP measure@endlink.
     * @param[in] aMelodyVelocity The @link DefVel velocity@endlink of the @link Music::MelodyNote pitches@endlink in the @link Music::CombinedNote note@endlink.
     * @param[in] aLength The @link Music::NOTE_LENGTH length@endlink of the @link Music::CombinedNote note@endlink.
     * @param[in] aPitches The @link Music::PITCH pitches@endlink in the @link Music::CombinedNote note@endlink.
     * @param[in] aDrumVelocity The @link DefVel velocity@endlink of the @link Music::PercussionNote drums@endlink in the @link Music::CombinedNote note@endlink.
     * @param[in] aDrumHits The @link Music::DRUM drums@endlink that are hit for this @link Music::CombinedNote note@endlink.
     * @param[in] aOffsetFromPrevNote The @link Music::CombinedNote.OffsetFromPrevNote offset from the previous note@endlink.
     * 
     * This function creates a new @link DocSC_MDP measure@endlink and puts 
     * @link SC_NoteDisplayContainer::AddNote the note@endlink that couldn't be added to the last
     * @link DocSC_MDP measure@endlink into the new one.
    */
    public void HandleFullMeasure( SC_MeasureDisplayPanel aFullMeasure, float aSpillover,
        int aMelodyVelocity, Music.NOTE_LENGTH aLength, Music.PITCH[] aPitches, int aDrumVelocity, Music.DRUM[] aDrumHits, Music.NOTE_LENGTH aOffsetFromPrevNote )
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
        mMeasures[mCurrentMeasure].transform.SetParent( gameObject.transform );
        mMeasures[mCurrentMeasure].SetParentContainer( this );
        mMeasures[mCurrentMeasure].transform.localScale = Vector3.one;
        mMeasures[mCurrentMeasure].SetToggle( true );

        // Handle Spillover from the previous measure.
        mMeasures[mCurrentMeasure].SetPercentageUsed( 0f - aSpillover );

        // Add the note to the new measure.
        mMeasures[mCurrentMeasure].AddNote( mNumNotes, aMelodyVelocity, aLength, aPitches, aDrumVelocity, aDrumHits, aOffsetFromPrevNote );

        // Make only the new measure be shown.
        HandleMeasureToggled( mMeasures[mCurrentMeasure] );

    }

    /**
     * @brief Handles when a @link DocSC_MDP measure@endlink has all of its @link Music::CombinedNote notes@endlink removed by deleting the @link DocSC_MDP measure object@endlink.
     * @param[in] aMeasure The @link DocSC_MDP measure@endlink that was deleted.
    */
    public void HandleMeasureDeleted( SC_MeasureDisplayPanel aMeasure )
    {
        // Find the measure that is deleted.
        int index = mMeasures.IndexOf( aMeasure );
        mCurrentMeasure--;

        // Remove the measure and show the previous one if there is one. 
        mMeasures.RemoveAt( index );
        if( index > 0 )
        {
            mMeasures[index - 1].SetToggle( true );
        }

        // Delete the measure.
        DestroyImmediate( aMeasure.gameObject, false );
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

    /**
     * @brief Handles when a note is removed by decreasing the count of notes in the song.
     * 
     * @see SC_MeasureDisplayPanel::RemoveNote
    */
    public void OnRemoveNote()
    {
        mNumNotes--;
    }
}
 
 