#if SONG_CREATION_ENABLED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/**
* @class SC_NoteDisplayPanel
* @brief Class that displays a specific @link Music::CombinedNote note@endlink of the Song that is being created.
* 
* This class is held in the @link DocSC_NDC Note Display Container@endlink and
* managed by the @link DocSC_MDP measure display panel@endlink.
*/
public class SC_NoteDisplayPanel : MonoBehaviour
{
    /*************************************************************************//** 
    * @defgroup SC_NDPPrivVar Private Variables
    * @ingroup DocSC_NDP
    * Private variables that are used internally by the SC_NoteDisplayPanel.
    * @{
    *****************************************************************************/
    private Button mMoveLeftButton = null; //!< The button to move the docnote earlier in the Song.
    private Button mMoveRightButton = null; //!< The button to move the docnote later in the Song.
    private Button mEditButton = null; //!< The button to edit the docnote.
    private Button mRemoveButton = null; //!< The button to remove this docnote.
    private int mNoteIndex = -1; //!< The index of this todocnote in the Song.
    private Music.CombinedNote mNote; //!< The represented todocnote.
    private SongCreationManager mSongCreationHandler = null; //!< todoc The song creation scene handler.
    private Text mNoLengthText = null; //!< The text that displays if the todocnote has no todoclength.
    private Text mMultipleLengthText = null; //!< The text that displays if the todocnote has todocpitches with multiple todoclengths.
    private Text mNoOffsetText = null; //!< The text that displays if the todoc note has no todocoffset.
    private Text mDrums = null; //!< The text that displays the todocnote's drums.
    private Text mPitches = null; //!< The text that displays the todocnote's todocpitches.
    private Text mPitchVelocity = null; //!< The text that displays the todocnote's todocvelocity.
    private Text mDrumVelocity = null; //!< The text that displays the todocnote's todocvelocity.
    private Transform mLengthPanel = null; //!< The image to show the todocnote's length
    private Transform mBaseLengthPanel = null; //!< The panel to show the todocnote's todoclength.
    private Transform mDotLengthPanel = null; //!< The panel to show if the todocnote is todocdotted.
    private Transform mTripletLengthPanel = null; //!< The panel to show if the todocnote is a todoctriplet.
    private Transform mOffsetPanel = null; //!< The image to show the todocnote's offset from the previous note.
    private Transform mBaseOffsetPanel = null; //!< The panel to show the base todocoffset.
    private Transform mDotOffsetPanel = null; //!< The panel to show if the todocoffset is todocdotted.
    private Transform mTripletOffsetPanel = null; //!< The panel to show if the todocoffset is todoctriplet.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDPUnity Unity Functions
    * @ingroup DocSC_NDP
    * Functions that are called automatically by Unity.
    * @{
    *****************************************************************************/

    /**
     * @brief Initializes the SC_NoteDisplayPanel by getting references to its child objects and adding event handlers.
    */
    private void Awake()
    {
        // Set the parent handler.
        mSongCreationHandler = GameObject.Find( "SongCreationCanvas" ).GetComponent<SongCreationManager>();

        // Set up the panel to show the length.
        mLengthPanel = transform.GetChild( 0 ).GetChild( 1 );
        mBaseLengthPanel = mLengthPanel.GetChild( 0 );
        mDotLengthPanel = mLengthPanel.GetChild( 1 );
        mTripletLengthPanel = mLengthPanel.GetChild( 2 );
        mNoLengthText = mLengthPanel.GetChild( 4 ).GetComponent<Text>();
        mMultipleLengthText = mLengthPanel.GetChild( 3 ).GetComponent<Text>();
        mLengthPanel.gameObject.SetActive( false );

        // Set up the panel to show the offset from the previous note.
        mOffsetPanel = transform.GetChild( 1 ).GetChild( 1 );
        mBaseOffsetPanel = mOffsetPanel.GetChild( 0 );
        mDotOffsetPanel = mOffsetPanel.GetChild( 1 );
        mTripletOffsetPanel = mOffsetPanel.GetChild( 2 );
        mNoOffsetText = mOffsetPanel.GetChild( 3 ).GetComponent<Text>();
        mOffsetPanel.gameObject.SetActive( false );

        // Set up the pitch display text.
        mPitches = transform.GetChild( 2 ).GetChild( 0 ).GetComponent<Text>();

        // Set up the drum display text.
        mDrums = transform.GetChild( 3 ).GetChild( 0 ).GetComponent<Text>();

        // Set up the velocity display panels.
        mPitchVelocity = transform.GetChild( 4 ).GetChild( 0 ).GetComponent<Text>();
        mDrumVelocity = transform.GetChild( 5 ).GetChild( 0 ).GetComponent<Text>();

        // Set up the button to move left.
        mMoveLeftButton = transform.GetChild( 6 ).GetChild( 0 ).GetComponent<Button>();
        mMoveLeftButton.gameObject.SetActive( false );

        // Set up the button to edit the note.
        mEditButton = transform.GetChild( 6 ).GetChild( 1 ).GetComponent<Button>();
        mEditButton.onClick.AddListener( OnEditButtonClicked );

        // Set up the button to remove the note.
        mRemoveButton = transform.GetChild( 6 ).GetChild( 2 ).GetComponent<Button>();
        mRemoveButton.onClick.AddListener( OnRemoveButtonClicked );

        // Set up the button to move right.
        mMoveRightButton = transform.GetChild( 6 ).GetChild( 3 ).GetComponent<Button>();
        mMoveRightButton.gameObject.SetActive( false );
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDPPubFunc Public Functions
    * @ingroup DocSC_NDP
    * Functions that allow for other classes to interact with the SC_NoteDisplayPanel.
    * @{
    *****************************************************************************/

    /**
     * @brief Gets the index for this note.
     * @return The index of the note in the Song
    */
    public int GetNoteIndex()
    {
        return mNoteIndex;
    }

    /**
     * @brief Gets the todocnote being represented.
     * @return the todocnote being represented.
    */
    public Music.CombinedNote GetNote()
    {
        return mNote;
    }

    /**
     * @brief Loads a todocnote into the display panel
     * @param[in] aNote The todocnote to display
     * @param[in] aNoteIndex The index of the todocnote in the overall Song.
    */
    public void LoadNote( Music.CombinedNote aNote, int aNoteIndex )
    {
        // Set the note being represented.
        mNote = aNote;
        mNoteIndex = aNoteIndex;

        // Update the length panel.
        UpdateLengthPanel();

        // Update the offset panel.
        UpdateOffsetPanel();

        // Update the pitches
        UpdatePitches();

        // Update the drums
        UpdateDrums();

        // Display the buttons 
        if( mNoteIndex != 0 )
        {
            mMoveLeftButton.gameObject.SetActive( true );
            mMoveLeftButton.onClick.AddListener( OnMoveLeftButtonClicked );
        }

        if( mNoteIndex != mSongCreationHandler.GetNumNotes() - 1 )
        {
            mMoveRightButton.gameObject.SetActive( true );
            mMoveRightButton.onClick.AddListener( OnMoveRightButtonClicked );
        }
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDPPrivFunc Private Functions
    * @ingroup DocSC_NDP
    * Functions that are used internally by the SC_NoteDisplayPanel.
    * @{
    *****************************************************************************/

    /**
     * @brief Updates the todocdrums and their todocvelocities.
    */
    private void UpdateDrums()
    {
        if( mNote.NumDrums > 0 )
        {
            mDrums.gameObject.SetActive( true );
            mDrums.text = "";

            // Update the drum display text 
            for( int i = 0; i < mNote.NumDrums; i++ )
            {
                mDrums.text += Music.DrumToString( mNote.Drums.Hits[i] ) + " "; 
            }

            // Update the drum velocity display text.
            mDrumVelocity.text = "Multiple";
            bool multiple = false;
            float vel = mNote.Drums.Velocities[0];
            int index = 1;
            while( index < mNote.NumDrums && !multiple )
            {
                if( vel != mNote.Drums.Velocities[index] )
                {
                    multiple = true;
                }
                index++;
            }

            if( !multiple )
            {
                mDrumVelocity.text = vel.ToString( "F2" );
            }
        }
        // Put "None" for the drums and "N/A" for the drum velocity if there are no drums.
        else
        {
            mDrums.text = "None";
            mDrumVelocity.text = "N/A";
        }
    }

    /**
     * @brief Updates the todocdrums and their todocvelocities.
    */
    private void UpdatePitches()
    {
        if( mNote.NumPitches > 0 )
        {
            mPitches.text = "";

            // Update the drum display text 
            for( int i = 0; i < mNote.NumPitches; i++ )
            {
                mPitches.text += Music.NoteToString( mNote.MusicalNote.Pitches[i] ) + " ";
            }

            // Update the drum velocity display text.
            mPitchVelocity.text = "Multiple";
            bool multiple = false;
            float vel = mNote.MusicalNote.Velocities[0];
            int index = 1;
            while( index < mNote.NumPitches && !multiple )
            {
                if( vel != mNote.MusicalNote.Velocities[index] )
                {
                    multiple = true;
                }
                index++;
            }

            if( !multiple )
            {
                mPitchVelocity.text = vel.ToString( "F2" );
            }
        }
        // Put "None" for the pitches and "N/A" for the pitch velocity if there are no pitches.
        else
        {
            mPitches.text = "None";
            mPitchVelocity.text = "N/A";
        }
    }

    /**
    * @brief Updates the todoclength panel when a todocnote is loaded into the display panel. 
    * @param[in] aNote The todocnote to display
    */
    private void UpdateLengthPanel()
    {
        // Set the length panel.
        mLengthPanel.gameObject.SetActive( true );

        if( mNote.NumPitches > 0 )
        {
            // See whether or not there are multiple lengths.
            int index = 1;
            bool multiple = false;
            Music.NoteLength firstLength = mNote.MusicalNote.Lengths[0];
            while( index < mNote.NumPitches && !multiple )
            {
                if( mNote.MusicalNote.Lengths[index] != firstLength )
                {
                    multiple = true;
                }
                index++;
            }

            // Display that there are multiple lengths if needed.
            if( multiple )
            {
                mMultipleLengthText.gameObject.SetActive( true );
                mNoLengthText.gameObject.SetActive( false );
                mBaseLengthPanel.gameObject.SetActive( false );
                mDotLengthPanel.gameObject.SetActive( false );
                mTripletLengthPanel.gameObject.SetActive( false );
            }
            // Display the common length if all pitches have the same length.
            else
            {
                mMultipleLengthText.gameObject.SetActive( false );

                // If the length is none, then display it.
                if( firstLength.BaseLength == Music.NOTE_LENGTH_BASE.NONE )
                {
                    mNoLengthText.gameObject.SetActive( true );
                }
                // Display the length as an image if there is a length.
                else
                {
                    mNoLengthText.gameObject.SetActive( false );
                    mBaseLengthPanel.gameObject.SetActive( true );
                    mBaseLengthPanel.GetChild( 0 ).GetComponent<Image>().sprite = Music.GetImageForNoteLength( firstLength.BaseLength );

                    // Display the length modifiers if needed.
                    mDotLengthPanel.gameObject.SetActive( firstLength.Dot );
                    mTripletLengthPanel.gameObject.SetActive( firstLength.Triplet );
                }
            }
        }
        // Handle when there are no pitches
        else
        {
            mNoLengthText.gameObject.SetActive( true );

            mBaseLengthPanel.gameObject.SetActive( false );
            mDotLengthPanel.gameObject.SetActive( false );
            mTripletLengthPanel.gameObject.SetActive( false );
            mMultipleLengthText.gameObject.SetActive( false );
        }
    }

    /**
    * @brief Updates the todocoffset panel when a todocnote is loaded into the display panel. 
    */
    private void UpdateOffsetPanel()
    {
        // Set the offset panel.
        mOffsetPanel.gameObject.SetActive( true );

        // If there is no offset, then display "None".
        if( mNote.OffsetFromPrevNote.BaseLength == Music.NOTE_LENGTH_BASE.NONE )
        {
            mNoOffsetText.gameObject.SetActive( true );
            mBaseOffsetPanel.gameObject.SetActive( false );
            mDotOffsetPanel.gameObject.SetActive( false );
            mTripletOffsetPanel.gameObject.SetActive( false );
        }
        // Display the offset as an image if there is an offset.
        else
        {
            mNoOffsetText.gameObject.SetActive( false );
            mBaseOffsetPanel.gameObject.SetActive( true );
            mBaseOffsetPanel.GetChild( 0 ).GetComponent<Image>().sprite = Music.GetImageForNoteLength( mNote.OffsetFromPrevNote.BaseLength );

            // Display the offset modifiers if needed.
            mDotOffsetPanel.gameObject.SetActive( mNote.OffsetFromPrevNote.Dot );
            mTripletOffsetPanel.gameObject.SetActive( mNote.OffsetFromPrevNote.Triplet );
        }
    }


    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDPHandlers Event Handlers
    * @ingroup DocSC_NDP
    * Functions that are called to handle events.
    * @{
    *****************************************************************************/

    /**
     * @brief Handles the EditButton being clicked
    */
    private void OnEditButtonClicked()
    {
        mSongCreationHandler.HandleEditNote( this );
    }

    /**
     * @brief Handles when the MoveLeftButton was clicked.
    */
    private void OnMoveLeftButtonClicked()
    {
        mSongCreationHandler.HandleMoveNoteLeft( this );
    }

    /**
     * @brief Handles when the MoveRightButton was clicked.
    */
    private void OnMoveRightButtonClicked()
    {
        mSongCreationHandler.HandleMoveNoteRight( this );
    }

    /**
     * @brief Handles when the @link SC_NoteDisplayPanel::mRemoveNoteButton remove button@endlink is clicked.
    */
    public void OnRemoveButtonClicked()
    {
        mSongCreationHandler.HandleRemoveNote( this );
    }

    /** @} */
}
 
#endif
 
 
 
 