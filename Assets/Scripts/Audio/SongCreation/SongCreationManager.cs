#if SONG_CREATION_ENABLED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
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
    private const string LOAD_SONG_DIALOG_PATH = "Audio/Prefabs/SongCreation/LoadSongDialogPrefab"; //!< The path to load the @link DocSC_LSD Load Song Dialog@endlink's prefab.
    private const string NOTE_DIALOG_PATH = "Audio/Prefabs/SongCreation/NoteDialogPrefab";

    /*************************************************************************//** 
    * @}
    * @defgroup SCMNestClass Nested Classes
    * @ingroup DocSCM
    * These are classes that are nested inside of the SongCreationManager. They are used only within the @link DocSC Song Creation Interface@endlink.
    * @{
    *****************************************************************************/
    
    /**
     * @class SC_InputFieldAndSlider
     * @brief A simple class that connects an arbitrary slider and input field.
    */
    public class SC_InputFieldAndSlider : MonoBehaviour, IEndDragHandler
    {
        /*************************************************************************//** 
        * @defgroup SC_IFASEventType Event Types
        * @ingroup DocSC_IFAS
        * These are The types of events for the SC_InputFieldAndSlider.
        * @{
        *****************************************************************************/

        /**
         * @brief The type of event that is invoked whenever the value of the input field or slider finishes changing.
        */
        public class ValueChangedEvent : UnityEvent<float> {}

        /*************************************************************************//**
         * @} 
        * @defgroup SC_IFASEvents Events
        * @ingroup DocSC_IFAS
        * These are the events for the SC_InputFieldAndSlider.
        * @{
        *****************************************************************************/
        public ValueChangedEvent ValueChanged; //!< The event that is invoked when the value of the input field or slider finishes changing.

        /*************************************************************************//** 
        * @}
        * @defgroup SC_IFASPrivVar Private Variables
        * @ingroup DocSC_IFAS
        * These are private variables for the SC_InputFieldAndSlider.
        * @{
        *****************************************************************************/
        private bool mAsInt = false; //!< Should the value be an integer or a float?
        private float mValue = 0f;
        private InputField mInputField = null;
        private Slider mSlider = null;

        /*************************************************************************//** 
        * @}
        * @defgroup SC_IFASUnity Unity Functions
        * @ingroup DocSC_IFAS
        * These are functions automatically called by Unity for the SC_InputFieldAndSlider.
        * @{
        *****************************************************************************/

        /** Initializes the object by getting the input field and the slider. */
        private void Awake()
        {
            // Get the slider and add its listener.
            mSlider = gameObject.GetComponent<Slider>();
            mSlider.onValueChanged.AddListener( OnSliderValueChanged );
            Assert.IsNotNull( mSlider, "InputField/Slider handler could not find the slider!" );

            ValueChanged = new ValueChangedEvent();
        }

        /*************************************************************************//** 
        * @}
        * @defgroup SC_IFASPubFunc Public Functions
        * @ingroup DocSC_IFAS
        * These are functions for other classes to interact with the SC_InputFieldAndSlider.
        * @{
        *****************************************************************************/

        /**
         * @brief Gets the value of the input field/slider.
         * @return The value of the input field/slider.
        */
        public float GetValue()
        {
            return mValue;
        }

        /**
         * @brief Sets whether or not the value should be an integer.
         * @param[in] aAsInt Whether or not the value should be an integer.
        */
        public void SetAsInt( bool aAsInt )
        {
            mAsInt = aAsInt;
        }

        /**
         * @brief Sets the input field that this should update.
         * @param[in] aInputField The input field that should be updated.
        */
        public void SetReferenceToInputField( InputField aInputField )
        {
            mInputField = aInputField;
            mInputField.onEndEdit.AddListener( OnInputFieldEdit );
            if( mAsInt )
            {
                mInputField.text = ( (int)mSlider.value ).ToString();
            }
            else
            {
                mInputField.text = mSlider.value.ToString( "F2" );
            }
        }

        /**
         * @brief Sets the value of the input field and slider.
         * @param[in] aValue The new value.
        */
        public void SetValue( float aValue )
        {
            if( mAsInt )
            {
                mInputField.text = ( (int)aValue ).ToString();
            }
            else
            {
                mInputField.text = aValue.ToString( "F2" );
            }
            mSlider.value = aValue;
            mValue = aValue;
        }

        /*************************************************************************//** 
        * @}
        * @defgroup SC_IFASHandlers Event Handlers
        * @ingroup DocSC_IFAS
        * These are functions that the SC_InputFieldAndSlider uses to handle events.
        * @{
        *****************************************************************************/

        /**
         * @brief Handles the slider stopping a drag.
         * @param[in] aPointerData The data relating to the drag event.
        */
        public void OnEndDrag( PointerEventData aPointerData )
        {
            if( mAsInt )
            {
                mInputField.text = ( (int)mSlider.value ).ToString();
            }
            else
            {
                mInputField.text = mSlider.value.ToString( "F2" );
            }
            mValue = mSlider.value;
            ValueChanged.Invoke( mSlider.value );
        }

        /**
         * @brief Handles an edit in the Input Field
         * @param[in] aInput The string put into the input field.
        */
        private void OnInputFieldEdit( string aInput )
        {
            float value = float.Parse( aInput );
            if( value >= mSlider.minValue && value <= mSlider.maxValue )
            {
                mSlider.value = value;
                ValueChanged.Invoke( mValue );
            }
            else
            {
                if( mAsInt )
                {
                    mInputField.text = ( (int)mValue ).ToString();
                }
                else
                {
                    mInputField.text = mValue.ToString( "F2" );
                }
            }
        }

        /**
         * @brief Handles the slider's value changing before the drag is complete.
         * @param[in] aNewValue The new value of the slider.
        */
        private void OnSliderValueChanged( float aNewValue )
        {
            if( mAsInt )
            {
                mInputField.text = ( (int)aNewValue ).ToString();
            }
            else
            {
                mInputField.text = aNewValue.ToString( "F2" );
            }
        }

        /** @} */
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SCMPrivVar Private Variables
    * @ingroup DocSCM
    * These are variables that are used internally by the SongCreationManager
    * @{
    ******************************************************************************/
    private Button mATIButton = null; //!< The button to reload the audio testing interface.
    private Button mLoadSongButton = null; //!< The button to load a song.
    private Button mNewNoteButton = null; //!< The button to add a new note with the chosen values.
    private Button mPlaySongButton = null; //!< The button to play the song. 
    private Button mSaveSongButton = null; //!< The button to save the song to a file.
    private InputField mSongNameInputField = null; //!< The input field to name the song.
    private int mEditIndex = -1; //!< The index of the todocnote being edited.
    private SC_NoteDisplayContainer mNoteDisplay = null; //!< The container to show the notes for the song.
    private SC_InputFieldAndSlider mBPMSlider = null; //!< The slider for the BPM of the song.
    private Song mSong = null; //!< The song being created.
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

        // Set up the slider for the song's default BPM.
        mBPMSlider = gameObject.transform.GetChild( 2 ).GetChild( 2 ).gameObject.AddComponent<SC_InputFieldAndSlider>();
        mBPMSlider.SetReferenceToInputField( transform.GetChild( 2 ).GetChild( 1 ).GetComponent<InputField>() );
        mBPMSlider.SetAsInt( true );
        mBPMSlider.ValueChanged.AddListener( OnBPMChange );

        // Set up the input field for naming the song.        
        mSongNameInputField = gameObject.transform.GetChild( 3 ).gameObject.GetComponent<InputField>();
        mSongNameInputField.onEndEdit.AddListener( mSong.SetName );

        // Set up the buttons.
        mNewNoteButton = gameObject.transform.GetChild( 5 ).GetChild( 1 ).gameObject.GetComponent<Button>();
        mNewNoteButton.onClick.AddListener( OnNewNoteButtonClicked );
        mSaveSongButton = gameObject.transform.GetChild( 5 ).GetChild( 2 ).gameObject.GetComponent<Button>();
        mSaveSongButton.onClick.AddListener( OnSaveSong );
        mPlaySongButton = gameObject.transform.GetChild( 5 ).GetChild( 0 ).GetComponent<Button>();
        mPlaySongButton.onClick.AddListener( OnPlaySong );
        mATIButton = gameObject.transform.GetChild( 4 ).GetComponent<Button>();
        mATIButton.onClick.AddListener( UnloadSongCreationInterface );
        mLoadSongButton = transform.GetChild( 5 ).GetChild( 3 ).GetComponent<Button>();
        mLoadSongButton.onClick.AddListener( OnLoadSongButtonClicked );
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SCMPubFunc Public Functions
    * @ingroup DocSCM
    * These are functions that allow for other classes to get information from the SongCreationManager.
    * @{
    ******************************************************************************/

    /**
     * @brief Gets the number of todocnotes in the Song.
     * @return The number of todocnotes in the Song.
    */
    public int GetNumNotes()
    {
        return mSong.GetNumNotes();
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
    public void OnCreateNote( Music.CombinedNote aNote )
    {
        // Add the note to the song.
        mSong.AddNote( aNote );

        // Add the note to the note display.
        mNoteDisplay.AddNote( aNote );
    }

    /**
     * @brief Handles when a note panel is selected for editing.
     * @param[in] aPanel The panel representing the note that we're editing.
     * 
     * @see SongCreationManager::OnModifyNote
    */
    public void HandleEditNote( SC_NoteDisplayPanel aPanel )
    {
        // Create a new note dialog.
        GameObject dialogObj = Instantiate( Resources.Load<GameObject>( NOTE_DIALOG_PATH ) );
        dialogObj.transform.SetParent( transform );

        // Get the dialog's script and add the listener for when it's finished.
        SC_NoteDialog dialog = dialogObj.transform.GetChild( 0 ).GetComponent<SC_NoteDialog>();
        dialog.LoadNoteIntoDialog( aPanel.GetNote() );
        mEditIndex = aPanel.GetNoteIndex();
        dialog.NoteDialogFinished.AddListener( OnModifyNote );
    }

    /**
     * @brief Moves a todocnote earlier in the Song.
     * @param[in] aNotePanel The panel that triggered the event.
    */
    public void HandleMoveNoteLeft( SC_NoteDisplayPanel aNotePanel )
    {
        // Get the note to move and the note that was previously to the left.
        Music.CombinedNote noteToMove = aNotePanel.GetNote();
        int noteToMoveIndex = aNotePanel.GetNoteIndex();
        Music.CombinedNote noteToLeft = mSong.GetNote( noteToMoveIndex - 1 );

        // Swap the notes.
        mSong.ReplaceNote( noteToMove, noteToMoveIndex - 1 );
        mSong.ReplaceNote( noteToLeft, noteToMoveIndex );

        // Reload the song.
        OnLoadSong( mSong );
    }

    /**
     * @brief Moves a todocnote earlier in the Song.
     * @param[in] aNotePanel The panel that triggered the event.
    */
    public void HandleMoveNoteRight( SC_NoteDisplayPanel aNotePanel )
    {
        // Get the note to move and the note that was previously to the left.
        Music.CombinedNote noteToMove = aNotePanel.GetNote();
        int noteToMoveIndex = aNotePanel.GetNoteIndex();
        Music.CombinedNote noteToRight = mSong.GetNote( noteToMoveIndex + 1 );

        // Swap the notes.
        mSong.ReplaceNote( noteToMove, noteToMoveIndex + 1 );
        mSong.ReplaceNote( noteToRight, noteToMoveIndex );

        // Reload the song.
        OnLoadSong( mSong );
    }

    /**
     * @brief Handles a todocnote being removed.
     * @param[in] aPanel The todocpanel representing the todocnote being removed.
    */
    public void HandleRemoveNote( SC_NoteDisplayPanel aPanel )
    {
        mSong.RemoveNote( aPanel.GetNoteIndex() );
        OnLoadSong( mSong );
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

        // Set the song name
        mSongNameInputField.text = mSong.GetName();
        mSongNameInputField.onEndEdit.AddListener( mSong.SetName );

        // Set the BPM slider.
        mBPMSlider.SetValue( (float)mSong.GetBPM() );

        // Clear the notes.
        mNoteDisplay.ClearNotes();

        // Add the notes of the song to the Note Display Panel.
        List<Music.CombinedNote> notes = mSong.GetAllNotes();
        foreach( Music.CombinedNote note in notes )
        {
            mNoteDisplay.AddNote( note );
        }
    }

    /**
     * @brief Modifies a @link Music::CombinedNote note@endlink in the Song.
     * 
     * @see Song::ReplaceNote
    */
    public void OnModifyNote( Music.CombinedNote aNote )
    {
        mSong.ReplaceNote( aNote, mEditIndex );
        OnLoadSong( mSong );
    }

    /**
     * @brief Handles the Add Note button being clicked by loading a @link DocSC_NDia Note Dialog@endlink.
    */
    public void OnNewNoteButtonClicked()
    {
        // Load the dialog.
        GameObject dialogObj = Instantiate( Resources.Load<GameObject>( NOTE_DIALOG_PATH ) );
        dialogObj.transform.SetParent( transform, true );
        SC_NoteDialog dialog = dialogObj.transform.GetChild( 0 ).GetComponent<SC_NoteDialog>();

        // Get the initial offset for the note dialog. 
        Music.NoteLength dialogOffset = new Music.NoteLength( Music.NOTE_LENGTH_BASE.NONE );
        if( mSong.GetNumNotes() > 0 )
        {
            // Get the last note to find the initial offset.
            int lastNoteIndex = mSong.GetNumNotes() - 1;
            Music.CombinedNote lastNote = mSong.GetNote( lastNoteIndex );

            // If there are pitches in the last note, then set the dialog's offset to 
            // be the shortest length of all of the pitches in the last note.
            if( lastNote.NumPitches > 0 )
            {
                Music.NoteLength shortestLength = lastNote.MusicalNote.Lengths[0];
                for( int i = 1; i < lastNote.NumPitches; i++ )
                {
                    if( lastNote.MusicalNote.Lengths[i] < shortestLength )
                    {
                        shortestLength = lastNote.MusicalNote.Lengths[i];
                    }
                }
                dialogOffset = shortestLength;
            }
            // If there are not pitches in the last note, then set it to
            // the last note's offset.
            else
            {
                dialogOffset = lastNote.OffsetFromPrevNote;
            }
        }

        dialog.SetDefaultOffset( dialogOffset );
        dialog.NoteDialogFinished.AddListener( OnCreateNote );
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
            mSong.ReplaceNote( firstNote, 0 );
        }
        mVIM.SongManager.AddSong( mSong );
        mSong.WriteSongToFile();
    }

    /**
     * @brief Goes back to the AudioTestingInterface scene.
    */
    public void UnloadSongCreationInterface()
    {
        SceneManager.LoadScene( "AudioTestingInterface" );
    }
    /** @} */
}
#endif