using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

/** 
 * @class SC_NoteDialog
 * @brief A dialog for loading or modifying a @link Music::CombinedNote note@endlink in the @link DocSC Song Creation Interface@endlink.
*/
public class SC_NoteDialog : MonoBehaviour
{

    /*************************************************************************//** 
    * @defgroup SC_NDiaConst Constants
    * @ingroup DocSC_NDia
    * These are constants that are used to get paths for the prefabs.
    * @{
    *****************************************************************************/
    private const string PDDP_PREFAB_PATH = "Audio/Prefabs/SongCreation/PitchDrumDisplayPanelPrefab"; //!< The path to load @link DocSC_PDDP Pitch/Drum Display Panel@endlink prefab.
    private const string ADD_PITCH_DIALOG_PREFAB_PATH = "Audio/Prefabs/SongCreation/AddPitchDialogPrefab"; //!< The path to load the @link DocSC_APD Add Pitch Dialog@endlink prefab.
    private const string ADD_DRUM_DIALOG_PREFAB_PATH = "Audio/Prefabs/SongCreation/AddDrumDialogPrefab"; //!< The path to load the @link DocSC_ADD Add Drum Dialog@endlink prefab.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDiaEventTypes Event Types
    * @ingroup DocSC_NDia
    * These are the types of events for the SC_NoteDialog.
    * @{
    *****************************************************************************/

    /**
     * @brief This event is used to notify the @link DocSCM Song Creation Manager@endlink that the @link Music::CombinedNote note@endlink is done being added/modified.
     * @note The parameter is a @link Music::CombinedNote note with percussion and melody@endlink. This is the note that is being returned from the note dialog.
     * @see SongCreationManager::OnNoteDialogFinished
    */
    public class NoteDialogFinishedEvent : UnityEvent<Music.CombinedNote> {}

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDiaEvents Events
    * @ingroup DocSC_NDia
    * These are the events used for the SC_NoteDialog.
    * @{
    *****************************************************************************/
    public NoteDialogFinishedEvent NoteDialogFinished; //!< The event that will be invoked in order to notify the @link DocSCM Song Creation Manager@endlink that the @link Music::CombinedNote note@endlink is done being added/modified.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDiaPrivVar Private Variables
    * @ingroup DocSC_NDia
    * These are variables that are used internally by the SC_PitchDrumDisplayPanel
    * @{
    *****************************************************************************/
    private Button mAddDrumButton = null; //!< The button to add a @link Music::DRUM drum@endlink to the @link Music::CombinedNote note@endlink.
    private Button mAddPitchButton = null; //!< The button to add a @link Music::PITCH pitch@endlink@endlink to the @link Music::CombinedNote note@endlink.
    private Button mCancelButton = null; //!< The button to begin cancel the dialog.
    private Button mDoneButton = null; //!< The button to finish the dialog.
    private Transform mMelodyDisplay = null; //!< The scrollview to display the @link Music::MelodyNote melody note@endlink.
    private Transform mPercussionDisplay = null; //!< The scrollview to display the @link Music::MelodyNote melody note@endlink.
    private List<SC_PitchDrumDisplayPanel> mDrums = null; //!< The representation of the portion of the overall @link Music::CombinedNote note@endlink relating to @link Music::DRUM drums@endlink. 
    private List<SC_PitchDrumDisplayPanel> mPitches = null; //!< The representation of the portion of the overall @link Music::CombinedNote note@endlink relating to @link Music::PITCH pitches@endlink. 
    private SC_LengthOffsetSelectionPanel mOffsetPanel = null; //!< The handler for setting the @link Music::CombinedNote.OffsetFromPrevNote offset from the previous note@endlink.
    private SongCreationManager.SC_InputFieldAndSlider mGlobalDrumVelocityHandler = null; //!< The handler for setting the @link DefVel Note Velocity@endlink for all of the @link Music::DRUM drums@endlink in the @link Music::CombinedNote note@endlink being represented.
    private SongCreationManager.SC_InputFieldAndSlider mGlobalPitchVelocityHandler = null; //!< The handler for setting the @link DefVel Note Velocity@endlink for all of the @link Music::PITCH pitches@endlink in the @link Music::CombinedNote note@endlink being represented.

    private void Awake ()
    {
        // Initialize the lists.
        mPitches = new List<SC_PitchDrumDisplayPanel>();
        mDrums = new List<SC_PitchDrumDisplayPanel>();

        // Get the display panels.
        mMelodyDisplay = transform.GetChild( 1 ).GetChild( 0 ).GetChild( 0 );
        mPercussionDisplay = transform.GetChild( 3 ).GetChild( 0 ).GetChild( 0 );

        // Set up the global pitch velocity.
        mGlobalPitchVelocityHandler = transform.GetChild( 6 ).gameObject.AddComponent<SongCreationManager.SC_InputFieldAndSlider>();
        mGlobalPitchVelocityHandler.SetReferenceToInputField( transform.GetChild( 5 ).GetComponent<InputField>() );
        mGlobalPitchVelocityHandler.SetAsInt( true );
        mGlobalPitchVelocityHandler.SetValue( 70f );
        mGlobalPitchVelocityHandler.ValueChanged.AddListener( OnGlobalPitchVelocityChanged );

        // Set up the global drum velocity.
        mGlobalDrumVelocityHandler = transform.GetChild( 9 ).gameObject.AddComponent<SongCreationManager.SC_InputFieldAndSlider>();
        mGlobalDrumVelocityHandler.SetReferenceToInputField( transform.GetChild( 8 ).GetComponent<InputField>() );
        mGlobalDrumVelocityHandler.SetAsInt( true );
        mGlobalDrumVelocityHandler.SetValue( 70f );
        mGlobalDrumVelocityHandler.ValueChanged.AddListener( OnGlobalDrumVelocityChanged );

        // Set up the offset panel
        mOffsetPanel = transform.GetChild( 11 ).gameObject.AddComponent<SC_LengthOffsetSelectionPanel>();
        mOffsetPanel.SetSelected( new Music.NoteLength( Music.NOTE_LENGTH_BASE.NONE ) );

        // Get the done button and add its listener.
        mDoneButton = transform.GetChild( 12 ).GetChild( 0 ).GetComponent<Button>();
        mDoneButton.onClick.AddListener( OnDoneButtonClicked );
        mDoneButton.gameObject.SetActive( false );

        // Get the Add Pitch Button and set its listener
        mAddPitchButton = transform.GetChild( 12 ).GetChild( 1 ).GetComponent<Button>();
        mAddPitchButton.onClick.AddListener( OnAddPitchButtonClicked );

        // Get the Add Drum Button and set its listener.
        mAddDrumButton = transform.GetChild( 12 ).GetChild( 2 ).GetComponent<Button>();
        mAddDrumButton.onClick.AddListener( OnAddDrumButtonClicked );

        // Get the cancel button and set its listener.
        mCancelButton = transform.GetChild( 12 ).GetChild( 3 ).GetComponent<Button>();
        mCancelButton.onClick.AddListener( OnCancelButtonClicked );

        NoteDialogFinished = new NoteDialogFinishedEvent();
	}

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDiaPubFunc Public Functions
    * @ingroup DocSC_NDia
    * These are functions that allow for interaction between the SC_NoteDialog and other classes.
    * @{
    *****************************************************************************/

    /**
     * @brief Loads a @link Music::CombinedNote note@endlink into the SC_NoteDialog
     * @param[in] aNote The @link Music::CombinedNote note@endlink to load.
    */
    public void LoadNoteIntoDialog( Music.CombinedNote aNote )
    {
        Music.MelodyNote melody = aNote.MusicalNote;
        Music.PercussionNote percussion = aNote.Drums;

        // Add display panels.
        if( melody.NumPitches > 0 )
        {
            // Add each pitch in the note.
            int index = 0;
            foreach( Music.PITCH pitch in melody.Pitches )
            {
                GameObject clone = Instantiate( Resources.Load<GameObject>( PDDP_PREFAB_PATH ) );
                clone.transform.SetParent( mMelodyDisplay );

                SC_PitchDrumDisplayPanel panel = clone.AddComponent<SC_PitchDrumDisplayPanel>();
                panel.InitializeAsPitchDisplay( melody.Pitches[index], melody.Lengths[index], melody.Velocities[index] );
                panel.SetParentContainer( this );
                mPitches.Add( panel );
                index++;
                clone = null;
            }
        }

        // Add each drum in the note.
        if( percussion.NumHits > 0 )
        {
            int index = 0;
            foreach( Music.DRUM drum in percussion.Hits )
            {
                GameObject clone = Instantiate( Resources.Load<GameObject>( PDDP_PREFAB_PATH ) );
                clone.transform.SetParent( mPercussionDisplay );

                SC_PitchDrumDisplayPanel panel = clone.AddComponent<SC_PitchDrumDisplayPanel>();
                panel.InitializeAsDrumDisplay( percussion.Hits[index], percussion.Velocities[index] );
                panel.SetParentContainer( this );
                mDrums.Add( panel );
                index++;
                clone = null;
            }
        }

        // Set the offset panel.
        mOffsetPanel.SetSelected( aNote.OffsetFromPrevNote );

        // Set the Done button
        UpdateDoneButton();
    }

    /**
     * @brief Sets the default todocoffset for the dialog. Usually based on the previous todocnote in the Song.
     * @param[in] aOffset The default todocoffset.
    */
    public void SetDefaultOffset( Music.NoteLength aOffset )
    {
        mOffsetPanel.SetSelected( aOffset );
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDiaPrivFunc Private Functions
    * @ingroup DocSC_NDia
    * These are functions that are used internally by the SC_NoteDialog.
    * @{
    *****************************************************************************/

    /**
     * @brief Hides buttons that can't be used.
    */
    private void UpdateDoneButton()
    {
        if( mDrums.Count == 0 && mPitches.Count == 0 )
        {
            mDoneButton.gameObject.SetActive( false );
        }
        else
        {
            mDoneButton.gameObject.SetActive( true );
        }
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDiaHandlers Event Handlers
    * @ingroup DocSC_NDia
    * These are variables that are used internally by the SC_PitchDrumDisplayPanel
    * @{
    *****************************************************************************/

    /**
    * @brief Handles the Add Pitch button being clicked by loading the @link DocSC_APD Add Pitch Dialog@endlink.
    */
    private void OnAddDrumButtonClicked()
    {
        // Load the Add Pitch Dialog.
        GameObject dialogObj = Instantiate( Resources.Load<GameObject>( ADD_DRUM_DIALOG_PREFAB_PATH ) );
        dialogObj.transform.SetParent( transform, false );
        SC_AddDrumDialog dialog = dialogObj.GetComponent<SC_AddDrumDialog>();
        dialog.AddDrumDialogFinished.AddListener( OnDrumsAdded );
    }

    /**
     * @brief Handles the Add Pitch button being clicked by loading the @link DocSC_APD Add Pitch Dialog@endlink.
    */
    private void OnAddPitchButtonClicked()
    {
        // Load the Add Pitch Dialog.
        GameObject dialogObj = Instantiate( Resources.Load<GameObject>( ADD_PITCH_DIALOG_PREFAB_PATH ) );
        dialogObj.transform.SetParent( transform, false );
        SC_AddPitchDialog dialog = dialogObj.GetComponent<SC_AddPitchDialog>();
        dialog.SetDefaultLength( mOffsetPanel.GetSelected() );
        dialog.AddPitchDialogFinished.AddListener( OnPitchesAdded );
    }

    /** 
     * @brief Handles the cancel button being clicked.
    */
    private void OnCancelButtonClicked()
    {
        DestroyImmediate( transform.parent.gameObject, false );
    }

    /**
     * @brief Handles the done button being clicked.
    */
    private void OnDoneButtonClicked()
    {
        // Set up the pitches.
        int numPitches = mPitches.Count;
        Music.PITCH[] pitches = null;
        int[] pitchVelocities = null;
        Music.NoteLength[] lengths = null;

        // If there are pitches, then get them from the panels.
        if( numPitches != 0 )
        {
            // Get all of the pitches, velocities, and lengths.
            pitches = new Music.PITCH[numPitches];
            pitchVelocities = new int[numPitches];
            lengths = new Music.NoteLength[numPitches];
            int index = 0;
            foreach( SC_PitchDrumDisplayPanel pitch in mPitches )
            {
                pitches[index] = pitch.GetPitch();
                pitchVelocities[index] = pitch.GetNoteVelocity();
                lengths[index] = pitch.GetLengthOfPitch();
                index++;
            }
        }

        // Create the melody note.
        Music.MelodyNote melody = new Music.MelodyNote( pitchVelocities, lengths, pitches );

        // Set up the drums.
        int numDrums = mDrums.Count;
        Music.DRUM[] drums = null;
        int[] drumVelocities = null;

        // If there are drums, then get them from the panels.
        if( numDrums != 0 )
        {
            // Get all of the drums and their velocities.
            drums = new Music.DRUM[numDrums];
            drumVelocities = new int[numDrums];
            int index = 0;
            foreach( SC_PitchDrumDisplayPanel drum in mDrums )
            {
                drums[index] = drum.GetDrum();
                drumVelocities[index] = drum.GetNoteVelocity();
                index++;
            }
        }

        // Create the percussion note.
        Music.PercussionNote percussion = new Music.PercussionNote( drumVelocities, drums );

        // Get the offset of the note.
        Music.NoteLength offset = mOffsetPanel.GetSelected();

        // Create the note.
        Music.CombinedNote note = new Music.CombinedNote( melody, percussion, offset );

        // Invoke the event which signals the dialog being finished.
        NoteDialogFinished.Invoke( note );

        // Self-destruct.
        DestroyImmediate( transform.parent.gameObject, false );
    }

    /**
     * @brief Handles @link Music::DRUM drums@endlink being added.
     * @param[in] aDrums The selected @link Music::DRUM drums@endlink.
     * @param[in] aVelocity The selected @link DefVel velocity@endlink.
    */
    private void OnDrumsAdded( Music.DRUM[] aDrums, int aVelocity )
    {
        // Add the pitches to the display container and the melody note.
        foreach( Music.DRUM drum in aDrums )
        {
            // Create a pitch/drum display panel and get its script.
            GameObject clone = Instantiate( Resources.Load<GameObject>( PDDP_PREFAB_PATH ) );
            SC_PitchDrumDisplayPanel newPanel = clone.AddComponent<SC_PitchDrumDisplayPanel>();

            // Initialize the panel's values and add it to the list.
            newPanel.InitializeAsDrumDisplay( drum, aVelocity );
            newPanel.SetParentContainer( this );
            mDrums.Add( newPanel );

            // Put the new panel in the melody display.
            clone.transform.SetParent( mPercussionDisplay, false );
        }
        UpdateDoneButton();
    }

    /**
     * @brief Handles the global todocdrum todocvelocity changing
     * @param[in] aNewVelocity The new global todocdrum todocvelocity.
    */
    private void OnGlobalDrumVelocityChanged( float aNewVelocity )
    {
        foreach( SC_PitchDrumDisplayPanel drum in mDrums )
        {
            drum.SetNoteVelocity( aNewVelocity );
        }
    }

    /**
     * @brief Handles the global todocpitch todocvelocity changing
     * @param[in] aNewVelocity The new global todocpitch todocvelocity.
    */
    private void OnGlobalPitchVelocityChanged( float aNewVelocity )
    {
        foreach( SC_PitchDrumDisplayPanel pitch in mPitches )
        {
            pitch.SetNoteVelocity( aNewVelocity );
        }
    }

    /**
     * @brief Handles @link Music::PITCH pitches@endlink being added.
     * @param[in] aPitches The selected @link Music::PITCH pitches@endlink.
     * @param[in] aLength The selected @link Music::NoteLength length@endlink.
     * @param[in] aVelocity The selected @link DefVel velocity@endlink.
    */
    private void OnPitchesAdded( Music.PITCH[] aPitches, Music.NoteLength aLength, int aVelocity )
    {
        // Add the pitches to the display container and the melody note.
        foreach( Music.PITCH pitch in aPitches )
        {
            // Create a pitch/drum display panel and get its script.
            GameObject clone = Instantiate( Resources.Load<GameObject>( PDDP_PREFAB_PATH ) );
            SC_PitchDrumDisplayPanel newPanel = clone.AddComponent<SC_PitchDrumDisplayPanel>();

            // Initialize the panel's values and add it to the list.
            newPanel.InitializeAsPitchDisplay( pitch, aLength, aVelocity );
            newPanel.SetParentContainer( this );
            mPitches.Add( newPanel );

            // Put the new panel in the melody display.
            clone.transform.SetParent( mMelodyDisplay, false );
        }
        UpdateDoneButton();
    }

    /**
     * @brief Handles a panel being removed.
     * @param[in] aPanel The todocpanel that was removed.
    */
    public void HandlePanelRemoved( SC_PitchDrumDisplayPanel aPanel )
    {
        if( aPanel.IsDrum() )
        {
            mDrums.Remove( aPanel );
        }
        else
        {
            mPitches.Remove( aPanel );
        }
    }

    /**
     * @brief Handles a todoc pitch being removed
     * @param[in] aPanel The todocpanel representing the removed todocpitch.
    */
    public void HandlePitchRemoved( SC_PitchDrumDisplayPanel aPanel )
    {
        mPitches.Remove( aPanel );
    }
    /** @} */

}
