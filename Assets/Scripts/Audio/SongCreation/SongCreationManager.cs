#if SONG_CREATION_ENABLED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * @class SongCreationManager
 * @brief A script that manages all of the @link DocSC Song Creation Interface@endlink.
 * 
 * This class is the central hub for all of the @link DocSC Song Creation Interface@endlink.
 * It connects each modules within the interface to each other and uses them to 
 * create a Song.
*/
public class SongCreationManager : MonoBehaviour
{
    /*************************************************************************//** 
    * @defgroup SCMConst Constants
    * @ingroup DocSCM
    * These are paths to the prefabs that are used in the @link DocSC Song Creation Interface@endlink.
    * @{
    *****************************************************************************/
    private string LOAD_SONG_DIALOG_PATH = "Audio/Prefabs/SongCreation/LoadSongDialogPrefab"; //!< The path to load the @link DocSC_LSD Load Song Dialog@endlink's prefab.

    /*************************************************************************//** 
    * @}
    * @defgroup SCMNestClass Nested Classes
    * @ingroup DocSCM
    * These are classes that are nested inside of the SongCreationManager. They are used only within the @link DocSC Song Creation Interface@endlink.
    * @{
    *****************************************************************************/
    
    //---------------------------------------------------------------------------- 
    // Class that handles selecting a note length/offset.
    //----------------------------------------------------------------------------
    private class SongCreationSelectionContainer : MonoBehaviour
    {
        //---------------------------------------------------------------------------- 
        // Nested class that uses this class as a handler.
        //---------------------------------------------------------------------------- 
        private class SongCreationSelectionTrigger : MonoBehaviour
        {
            //---------------------------------------------------------------------------- 
            // Private Variables
            //---------------------------------------------------------------------------- 
            private bool mSelected = false; // Whether or not this object is selected.
            private SongCreationSelectionContainer mContainer = null; // The parent container.
            private Toggle mToggle = null; // The associated toggle switch.

            //---------------------------------------------------------------------------- 
            // Unity Functions
            //---------------------------------------------------------------------------- 
            private void Awake()
            {
                // Get the toggle and set its listener
                mToggle = gameObject.GetComponent<Toggle>();
                mToggle.onValueChanged.AddListener( OnSelected );
            }

            //---------------------------------------------------------------------------- 
            // Public Functions
            //---------------------------------------------------------------------------- 

            // Sets the parent container
            public void SetContainer( SongCreationSelectionContainer aContainer )
            {
                mContainer = aContainer;
            }

            // Sets whether or not the object is selected.
            public void SetSelected( bool aSelected )
            {
                mSelected = aSelected;

                // Change the color to indicate that this is or isn't the selected object
                ChangeColor();

                // Set the value of the toggle.
                mToggle.onValueChanged.RemoveListener( OnSelected );
                mToggle.isOn = aSelected;
                mToggle.onValueChanged.AddListener( OnSelected );
            }

            //---------------------------------------------------------------------------- 
            // Private Functions
            //---------------------------------------------------------------------------- 

            // Changes color to show whether or not this object is selected.
            private void ChangeColor()
            {
                if( mSelected )
                {
                    mToggle.transform.GetChild( 0 ).GetComponent<Image>().color = new Color32( 255, 255, 255, 118 );

                }
                else
                {
                    mToggle.transform.GetChild( 0 ).GetComponent<Image>().color = new Color32( 255, 255, 255, 255 );
                }
            }

            //---------------------------------------------------------------------------- 
            // Event Handlers
            //---------------------------------------------------------------------------- 

            // Handles the object being selected or unselected.
            public void OnSelected( bool aSelected )
            {
                // If this object is now selected, then change the color and send it to the handler.
                if( aSelected )
                {
                    mSelected = aSelected;
                    ChangeColor();
                    mContainer.HandleToggle( this );
                }
                // This object can not be unselected by clicking on it. Set the value back to selected.
                else
                {
                    mToggle.onValueChanged.RemoveListener( OnSelected );
                    mToggle.isOn = true;
                    mToggle.onValueChanged.AddListener( OnSelected );
                }

            }
        }

        //---------------------------------------------------------------------------- 
        // Private Variables
        //----------------------------------------------------------------------------
        private int mSelectedIndex = 0; // The index of the current selection
        private SongCreationSelectionTrigger[] mTriggers = null; // The triggers that pass a selection event to this handler.


        //---------------------------------------------------------------------------- 
        // Unity Functions
        //----------------------------------------------------------------------------
        private void Awake()
        {
            // Set up the triggers.
            mTriggers = new SongCreationSelectionTrigger[13];
            for( int i = 0; i < 13; i++ )
            {
                mTriggers[i] = gameObject.transform.GetChild( i + 1 ).gameObject.AddComponent<SongCreationSelectionTrigger>();
                mTriggers[i].SetContainer( this );
                mTriggers[i].SetSelected( false );
            }

            // Set the default selection (Quarter note).
            mTriggers[12].SetSelected( true );
            mSelectedIndex = 12;
        }

        //---------------------------------------------------------------------------- 
        // Public Functions
        //----------------------------------------------------------------------------

        // Gets the current selection.
        public Music.NOTE_LENGTH GetSelected()
        {
            return (Music.NOTE_LENGTH)mSelectedIndex;
        }

        // Sets the current selection.
        public void SetSelected( Music.NOTE_LENGTH aSelection )
        {
            // Update the private variable.
            mSelectedIndex = (int)aSelection;

            // Update the child objects.
            for( int i = 0; i < 13; i++ )
            {
                if( i != mSelectedIndex )
                {
                    mTriggers[i].SetSelected( false );
                }
                else
                {
                    mTriggers[i].SetSelected( true );
                }
            }
        }

        //---------------------------------------------------------------------------- 
        // Event Handlers
        //----------------------------------------------------------------------------

        // Handles a new selection.
        // IN: aTrigger The trigger that is now the new selection.
        private void HandleToggle( SongCreationSelectionTrigger aTrigger )
        {
            // Set all other triggers to unselected.
            for( int i = 0; i < 13; i++ )
            {
                if( mTriggers[i] != aTrigger )
                {
                    mTriggers[i].SetSelected( false );
                }

                // Update the current selection.
                else
                {
                    mSelectedIndex = i;
                }
            }
        }
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SCMPrivVar Private Variables
    * @ingroup DocSCM
    * These are variables that are used internally by the SongCreationManager
    * @{
    ******************************************************************************/
    private bool mEditing = false; //!< Whether or not a note is being modified.
    private Button mATIButton = null; //!< The button to reload the audio testing interface.
    private Button mLoadSongButton = null; //!< The button to load a song.
    private Button mNewNoteButton = null; //!< The button to add a new note with the chosen values.
    private Button mPlaySongButton = null; //!< The button to play the song. 
    private Button mResetPitchesButton = null; //!< The button to reset the pitch selections.
    private Button mSaveSongButton = null; //!< The button to save the song to a file.
    private InputField mSongNameInputField = null; //!< The input field to name the song.
    private Music.NOTE_LENGTH mLastLength = Music.NOTE_LENGTH.NONE; //!< The length used to create the last note. Used for special handling of drum loops.
    private SC_NoteDisplayContainer mNoteDisplay = null; //!< The container to show the notes for the song.
    private SC_NoteDisplayPanel mEditPanel = null; //!< The panel that was selected to be edited.
    private SC_PitchSelectionContainer mDrumSelector = null; //!< The container for choosing drums.
    private SC_PitchSelectionContainer mPitchSelector = null; //!< The container for choosing pitches.
    private Slider mBPMSlider = null; //!< The slider for the BPM of the song.
    private Slider mVelocitySlider = null; //!< The slider for the velocity of the new note.
    private Song mSong = null; //!< The song being created.
    private SongCreationSelectionContainer mLengthPanel = null; //!< The panel to choose a length for the new note.
    private SongCreationSelectionContainer mOffsetPanel = null; //!< The panel to choose an offset for the new note.
    private VirtualInstrumentManager mVIM = null; //!< The virtual instrument manager

    /*************************************************************************//** 
    * @}
    * @defgroup SCMUnity Unity Functions
    * @ingroup DocSCM
    * These are functions called automatically by Unity.
    * @{
    ******************************************************************************/

    /**
     * @brief Initializes the @link DocSC Song Creation Interface@endlink by getting references to all of the modules within the scene.
    */
    void Awake()
    {
        // Initialize the song.
        mSong = new Song();

        // Get the virtual instrument manager.
        mVIM = GameObject.Find( "VirtualInstrumentManager" ).GetComponent<VirtualInstrumentManager>();

        // Set up the container for showing the song's notes.
        mNoteDisplay = gameObject.transform.GetChild( 1 ).GetChild( 0 ).GetChild( 0 ).gameObject.AddComponent<SC_NoteDisplayContainer>();

        // Set up the container for selecting pitches.
        mPitchSelector = gameObject.transform.GetChild( 2 ).GetChild( 0 ).GetChild( 0 ).gameObject.AddComponent<SC_PitchSelectionContainer>();
        mPitchSelector.SetUpAsPitchSelector();

        // Set up the panels for choosing a note's length and offset.
        mLengthPanel = gameObject.transform.GetChild( 3 ).gameObject.AddComponent<SongCreationSelectionContainer>();
        mLengthPanel.SetSelected( Music.NOTE_LENGTH.Q );
        mOffsetPanel = gameObject.transform.GetChild( 4 ).gameObject.AddComponent<SongCreationSelectionContainer>();

        // Set up the slider for the note's velocity.
        mVelocitySlider = gameObject.transform.GetChild( 5 ).GetChild( 0 ).GetComponent<Slider>();
        mVelocitySlider.onValueChanged.AddListener( OnVelocityChange );

        // Set up the slider for the song's default BPM.
        mBPMSlider = gameObject.transform.GetChild( 5 ).GetChild( 1 ).GetComponent<Slider>();
        mBPMSlider.onValueChanged.AddListener( OnBPMChange );

        // Set up the input field for naming the song.        
        mSongNameInputField = gameObject.transform.GetChild( 6 ).gameObject.GetComponent<InputField>();
        mSongNameInputField.onEndEdit.AddListener( mSong.SetName );

        // Set up the buttons.
        mNewNoteButton = gameObject.transform.GetChild( 7 ).gameObject.GetComponent<Button>();
        mNewNoteButton.onClick.AddListener( OnCreateNote );
        mResetPitchesButton = gameObject.transform.GetChild( 8 ).GetComponent<Button>();
        mResetPitchesButton.onClick.AddListener( mPitchSelector.ResetPitches );
        mSaveSongButton = gameObject.transform.GetChild( 9 ).gameObject.GetComponent<Button>();
        mSaveSongButton.onClick.AddListener( OnSaveSong );
        mPlaySongButton = gameObject.transform.GetChild( 10 ).GetComponent<Button>();
        mPlaySongButton.onClick.AddListener( OnPlaySong );
        mATIButton = gameObject.transform.GetChild( 11 ).GetComponent<Button>();
        mATIButton.onClick.AddListener( UnloadSongCreationInterface );
        mLoadSongButton = transform.GetChild( 13 ).GetComponent<Button>();
        mLoadSongButton.onClick.AddListener( OnLoadSongButtonClicked );

        // Set up the drum selector.
        mDrumSelector = gameObject.transform.GetChild( 12 ).GetChild( 0 ).GetChild( 0 ).gameObject.AddComponent<SC_PitchSelectionContainer>();
        mDrumSelector.SetUpAsDrumSelector();
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SCMHandlers Event Handlers
    * @ingroup DocSCM
    * These are functions called by the SongCreationManager to handle events.
    * @{
    ******************************************************************************/

    /**
     * @brief Handles a change in the song's default BPM.
     * @param[in] aBPM The new default BPM.
     * 
     * Gets the value from the @link SongCreationManager::mBPMSlider BPM slider@endlink 
     * and @link Song::SetBPM sends it@endlink to the @link SongCreationManager::mSong Song@endlink.
     * 
     * @see SongCreationManager::mBPMSlider
    */
    public void OnBPMChange( float aBPM )
    {
        // Update the BPM slider's label.
        mBPMSlider.transform.GetChild( 3 ).GetComponent<Text>().text = "Default BPM: " + aBPM.ToString();

        // Update the song.
        mSong.SetBPM( (int)aBPM );
    }

    /**
     * @brief Creates a new note in the song.
     * 
     * Gets the currently selected values from the modules in the scene uses them
     * to @link Music::CreateNote create@endlink a new @link Music::CombinedNote note@endlink.
     * 
     * The function won't do anything if no @link SongCreationManager::mPitchSelector pitches@endlink
     * or @link SongCreationManager::mDrumSelector drums@endlink are selected.
     * 
     * @see SongCreationManager::mNewNoteButton
    */
    public void OnCreateNote()
    {
        // Sanity check.
        if( mEditing )
        {
            Assert.IsFalse( !mEditing, "Tried to create a note in the song creation interface when we should have been editing one!" );
            return;
        }

        // Get the selected pitches
        Music.PITCH[] pitches = mPitchSelector.GetSelectedPitches();

        // Get the selected drums.
        Music.PITCH[] drums = mDrumSelector.GetSelectedPitches();

        // Make sure that some pitches or drums are actually selected.
        if( pitches != null || drums != null )
        {
            // Get the offset and velocity.
            Music.NOTE_LENGTH length = Music.NOTE_LENGTH.NONE;
            mLastLength = mLengthPanel.GetSelected();
            Music.NOTE_LENGTH offset = mOffsetPanel.GetSelected();
            int pitchVelocity = 0;
            int drumVelocity = 0;

            // If the song doesn't have any notes, then set the offset of the first note to none.
            if( mSong.GetNumNotes() == 0 )
            {
                offset = Music.NOTE_LENGTH.NONE;
            }

            // Convert the selected pitches from the drum selector into actual drum hits.
            Music.DRUM[] Hits = null;
            if( drums != null )
            {
                Hits = new Music.DRUM[drums.Length];
                for( int i = 0; i < drums.Length; i++ )
                {
                    Hits[i] = (Music.DRUM)drums[i];
                }
            }


            // Update the velocity/length for the pitches and drums if needed.
            if( pitches != null )
            {
                pitchVelocity = (int)mVelocitySlider.value;
                length = mLengthPanel.GetSelected();
            }

            if( drums != null )
            {
                drumVelocity = (int)mVelocitySlider.value;
            }


            // Add the note to the song.
            mSong.AddNote( Music.CreateNote( pitchVelocity, length, pitches, drumVelocity, Hits, offset ) );

            // Add the note to the note display.
            mNoteDisplay.AddNote( pitchVelocity, length, pitches, drumVelocity, Hits, offset );

            // Reset the pitches.
            mPitchSelector.ResetPitches();
            mDrumSelector.ResetPitches();

            // Update the selection of the offset panel since most cases will use the previous length.
            mOffsetPanel.SetSelected( mLengthPanel.GetSelected() );
        }
    }

    /**
     * @brief Handles when a note panel is selected for editing.
     * @param[in] aPanel The panel representing the note that we're editing.
     * 
     * @see SongCreationManager::OnModifyNote
    */
    public void OnEditEvent( SC_NoteDisplayPanel aPanel )
    {
        // Handle the case where we begin editing a note.
        if( mEditPanel == null || mEditPanel != aPanel )
        {
            // Handle switching from editing one panel to editing a different one.
            if( mEditPanel != null )
            {
                // Stop editing the current note/note panel.
                mEditPanel.StopEditing();

                // Change the note/note panel being edited to the new one.
                mEditPanel = aPanel;
            }
            // Handle when we start editing a panel.
            else
            {
                // Update the new note button to be a modify note button.
                mNewNoteButton.transform.GetChild( 0 ).GetComponent<Text>().text = "Modify Note";
                mNewNoteButton.onClick.RemoveListener( OnCreateNote );
                mNewNoteButton.onClick.AddListener( OnModifyNote );

                // Set the variables related to editing a note.
                mEditPanel = aPanel;
                mEditing = true;
            }

            // Get the note being modified.
            Music.CombinedNote noteBeingModified = mSong.GetNote( aPanel.GetNoteIndex() );

            // Set the pitch selector to have the pitches from the note.
            mPitchSelector.SetPitches( noteBeingModified.MusicalNote.Pitches );

            // Set the drum selector to have the drums from the note.
            Music.PITCH[] drumsAsPitches = null;
            if( noteBeingModified.Drums.Hits != null )
            {
                drumsAsPitches = new Music.PITCH[noteBeingModified.Drums.Hits.Length];
                int index = 0;
                foreach( Music.DRUM drum in noteBeingModified.Drums.Hits )
                {
                    drumsAsPitches[index] = (Music.PITCH)drum;
                    index++;
                }
            }
            mDrumSelector.SetPitches( drumsAsPitches );
            mLengthPanel.SetSelected( noteBeingModified.MusicalNote.Length );
            mOffsetPanel.SetSelected( noteBeingModified.OffsetFromPrevNote );
            mVelocitySlider.value = Mathf.Max( noteBeingModified.Drums.Velocity, noteBeingModified.MusicalNote.Velocity );
        }
        // Handle the case where we're editing a note/note panel and it calls this 
        // event while it's being edited. This means that the edit should be cancelled.
        else
        {
            // Change the new note button back to its original state.
            mNewNoteButton.transform.GetChild( 0 ).GetComponent<Text>().text = "Insert Note";
            mNewNoteButton.onClick.RemoveListener( OnModifyNote );
            mNewNoteButton.onClick.AddListener( OnCreateNote );

            // Mark that the panel is no longer being edited.
            mEditPanel.StopEditing();

            // Reset the variables related to editing a note.
            mLengthPanel.SetSelected( Music.NOTE_LENGTH.Q );
            mOffsetPanel.SetSelected( Music.NOTE_LENGTH.NONE );
            mVelocitySlider.value = 100;
            mPitchSelector.ResetPitches();
            mDrumSelector.ResetPitches();
            mEditPanel = null;
            mEditing = false;
        }
    }

    /**
     * @brief Handles the @link SongCreationManager::mLoadSongButton Load Song button@endlink being clicked by loading the @link DocSC_LSD Load Song Dialog@endlink.
    */
    private void OnLoadSongButtonClicked()
    {
        // Load prefab.
        GameObject dialogObject = Instantiate( Resources.Load<GameObject>( LOAD_SONG_DIALOG_PATH ) );
        Assert.IsNotNull( dialogObject, "Could not load the SC_LoadSongDialog!" );

        // Get the dialog component.
        SC_LoadSongDialog dialog = dialogObject.GetComponent<SC_LoadSongDialog>();

        // Put the dialog at the right place in the hierarchy.
        dialog.transform.SetParent( transform );

        // Add the listener.
        dialog.SongSelected.AddListener( OnLoadSong );
    }

    /**
     * @brief Loads a Song into the @link DocSC Song Creation Interface@endlink.
     * @param[in] aSong The song to load.
     * 
     * @see LoadSongDialog
    */
    private void OnLoadSong( Song aSong )
    {
        // Set mSong
        mSong = aSong;

        // Clear the notes.
        mNoteDisplay.ClearNotes();

        // Add the notes of the song to the Note Display Panel.
        List<Music.CombinedNote> notes = mSong.GetAllNotes();
        foreach( Music.CombinedNote note in notes )
        {
            mNoteDisplay.AddNote( note.MusicalNote.Velocity, note.MusicalNote.Length, note.MusicalNote.Pitches, note.Drums.Velocity,
                note.Drums.Hits, note.OffsetFromPrevNote );
        }
    }

    /**
     * @brief Modifies a @link Music::CombinedNote note@endlink in the Song.
     * 
     * @see Song::ReplaceNote
    */
    public void OnModifyNote()
    {
        // Make sure that we're actually editing a note.
        if( mEditing )
        {
            // Sanity check.
            if( mEditPanel == null )
            {
                Assert.IsNotNull( mEditPanel, "Tried to modify a note, but the note panel in the song creator was null!" );
                return;
            }

            // Get the pitches 
            Music.PITCH[] pitches = mPitchSelector.GetSelectedPitches();

            // Get the drums
            Music.PITCH[] drums = mDrumSelector.GetSelectedPitches();

            // Make sure that some pitches or drums are actually selected.
            if( pitches != null || drums != null )
            {
                // Get the index of the note in the song and the images for the length/offset.
                int index = mEditPanel.GetNoteIndex();
                Sprite[] images = mNoteDisplay.GetSprites();

                // Convert the selected pitches from the drum selector into actual drum hits.
                Music.DRUM[] Hits = null;
                if( drums != null )
                {
                    Hits = new Music.DRUM[drums.Length];
                    for( int i = 0; i < drums.Length; i++ )
                    {
                        Hits[i] = (Music.DRUM)drums[i];
                    }
                }

                // Set the values of the note struct.
                Music.NOTE_LENGTH length = Music.NOTE_LENGTH.NONE;
                Music.NOTE_LENGTH offset = mOffsetPanel.GetSelected();
                int pitchVelocity = 0;
                int drumVelocity = 0;

                // Update the velocity for the pitches and drums if needed.
                if( pitches != null )
                {
                    pitchVelocity = (int)mVelocitySlider.value;
                    length = mLengthPanel.GetSelected();
                }

                if( drums != null )
                {
                    drumVelocity = (int)mVelocitySlider.value;
                }

                // Replace the note in the song with the modified note.
                mSong.ReplaceNote( Music.CreateNote( pitchVelocity, length, pitches, drumVelocity, Hits, offset ), index );

                // Get the string of pitches for the modified note.
                string pitchString = "None";
                if( pitches != null )
                {
                    pitchString = "";
                    for( int i = 0; i < pitches.Length; i++ )
                    {
                        if( pitches[i] == Music.PITCH.REST )
                        {
                            pitchString += "Rest ";
                        }
                        else
                        {
                            pitchString += ( Music.NoteToString( pitches[i] ) + " " );
                        }
                    }
                }

                // Get the string of drums for the modified note.
                string drumString = "None";
                if( Hits != null )
                {
                    drumString = "";
                    foreach( Music.DRUM drum in Hits )
                    {
                        drumString += Music.DrumToString( drum ) + " ";
                    }
                }

                // Update the panel representing the note.
                mEditPanel.SetDrums( drumString );
                mEditPanel.SetPitches( pitchString );
                mEditPanel.SetLengthImage( images[(int)length] );
                mEditPanel.SetOffsetImage( images[(int)offset] );
                mEditPanel.SetOffset( offset );
                mEditPanel.SetVelocity( mVelocitySlider.value.ToString() );

                // Mark that the panel is no longer being edited.
                mEditPanel.StopEditing();

                // Update the new note button so that it shows that we're no longer editing a note.
                mNewNoteButton.transform.GetChild( 0 ).GetComponent<Text>().text = "Insert Note";
                mNewNoteButton.onClick.RemoveListener( OnModifyNote );
                mNewNoteButton.onClick.AddListener( OnCreateNote );

                // Reset the variables related to editing a note.
                mEditPanel = null;
                mEditing = false;

                // Reset the pitch selections.
                mPitchSelector.ResetPitches();
                mDrumSelector.ResetPitches();
            }
        }
    }

    /**
     * @brief Plays the Song being created.
     * 
     * @see VirtualInstrumentManager::PlaySongEvent
    */
    public void OnPlaySong()
    {
        mVIM.PlaySong.Invoke( mSong );
    }

    /**
     * @brief Removes a @link Music::CombinedNote note@endlink from the Song being created.
     * @param[in] aIndex The index of the @link Music::CombinedNote note to remove@endlink in the Song being created.
     * 
     * @see Song::OnRemoveNote
    */
    public void OnRemoveNote( int aIndex )
    {
        if( mSong.GetNumNotes() > 0 )
        {
            mSong.RemoveNote( aIndex );
        }
    }

    /**
     * @brief Saves the Song to a file
     * 
     * This function adds the song to the @link VirtualInstrumentManager::SongManager Song Manager@endlink and also saves the Song.
     * to a file.
     * 
     * @see Song::WriteSongToFile
    */
    public void OnSaveSong()
    {
        // Special handling to make drum loops loop properly. The first hit has an initial offset
        // equal to the length used to create the last note.
        if( mSong.GetSongType() == Song.SongType.DrumLoop )
        {
            Music.CombinedNote firstNote = mSong.GetAllNotes()[0];
            firstNote.OffsetFromPrevNote = mLastLength;
            mSong.ReplaceNote( firstNote, 0 );
        }
        mVIM.SongManager.AddSong( mSong );
        mSong.WriteSongToFile();
    }

    /** 
     * @brief Handles a change in the @link DefVel velocity@endlink for a @link Music::CombinedNote note@endlink in the Song being created.
     * @param[in] aVelocity The new @link DefVel velocity@endlink.
     * 
     * @see SongCreationManager::mVelocitySlider
    */
    public void OnVelocityChange( float aVelocity )
    {
        // Update the text of the velocity slider's label.
        mVelocitySlider.transform.GetChild( 3 ).GetComponent<Text>().text = "Velocity: " + aVelocity.ToString();
    }

    /**
     * @brief Goes back to the AudioTestingInterface scene.
    */
    public void UnloadSongCreationInterface()
    {
        SceneManager.UnloadSceneAsync( "SongCreationInterfaceScene" );
    }
    /** @} */
}
#endif