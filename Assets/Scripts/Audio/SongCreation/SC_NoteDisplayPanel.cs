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
    private Button mRemoveNoteButton = null; //!< The button to remove this note.
    private EventTrigger mEditTrigger = null; //!< The edit trigger.
    private Image mLengthImage = null; //!< The image to show the note's length
    private Image mOffsetImage = null; //!< The image to show the note's offset from the previous note.
    private int mNoteIndex = 0; //!< The inde of this note.
    private SC_MeasureDisplayPanel mParent = null; //!< The parent container.
    private Music.NOTE_LENGTH mOffset = Music.NOTE_LENGTH.NONE; //!< The offset.
    private SongCreationManager mSongCreationHandler = null; //!< The song creation scene handler.
    private Text mDrums = null; //!< The text that displays the note's drums.
    private Text mPitches = null; //!< The text that displays the note's pitches.
    private Text mVelocity = null; //!< The text that displays the note's velocity.

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

        // Set up the OnClick trigger.
        mEditTrigger = gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry click = new EventTrigger.Entry();
        click.eventID = EventTriggerType.PointerClick;
        click.callback.AddListener( ( data ) => { TriggerEditEvent( (PointerEventData)data ); } );
        mEditTrigger.triggers.Add( click );

        // Setup the children objects.
        mLengthImage = gameObject.transform.GetChild( 3 ).GetChild( 1 ).GetComponent<Image>();
        mOffsetImage = gameObject.transform.GetChild( 2 ).GetChild( 1 ).GetComponent<Image>();
        mPitches = gameObject.transform.GetChild( 1 ).GetChild( 0 ).GetComponent<Text>();
        mVelocity = gameObject.transform.GetChild( 4 ).GetChild( 0 ).GetComponent<Text>();
        mRemoveNoteButton = gameObject.transform.GetChild( 0 ).gameObject.GetComponent<Button>();
        mRemoveNoteButton.onClick.AddListener( OnRemoveButtonClicked );
        mDrums = gameObject.transform.GetChild( 5 ).GetChild( 0 ).GetComponent<Text>();
    }

    /*************************************************************************//** 
    * @defgroup SC_NDPPubFunc Public Functions
    * @ingroup DocSC_NDP
    * Functions that allow for other classes to interact with the SC_NoteDisplayPanel.
    *****************************************************************************/

    /**
     * @brief Gets the image used to represent the length of the note.
     * @return The image used to represent the length of the note.
    */
    public Sprite GetLengthImage()
    {
        return mLengthImage.sprite;
    }

    /**
     * @brief Gets the measure panel that is managing this panel.
     * @return The parent measure panel.
    */
    public SC_MeasureDisplayPanel GetMeasurePanel()
    {
        return mParent;
    }

    /**
     * @brief Gets the index for this note.
     * @return The index of the note in the Song
    */
    public int GetNoteIndex()
    {
        return mNoteIndex;
    }

    /**
     * @brief Gets the offset of this note.
     * @return The @link Music::NOTE_LENGTH length of time@endlink between the previous note and this one.
    */
    public Music.NOTE_LENGTH GetOffset()
    {
        return mOffset;
    }

    /**
     * @brief Sets the drum string for the note.
     * @param[in] aDrumString A string representing the drums of the note.
    */
    public void SetDrums( string aDrumString )
    {
        mDrums.text = aDrumString;
    }

    /**
     * @brief Sets the image used to represent the @link Music::NOTE_LENGTH length@endlink of the @link Music::CombinedNote note@endlink.
     * @param[in] aImage The image representing the length of the note.
    */
    public void SetLengthImage( Sprite aImage )
    {
        if( aImage == null )
        {
            mLengthImage.color = new Color32( 255, 255, 255, 0 );
        }
        else
        {
            mLengthImage.sprite = aImage;
            mLengthImage.color = new Color32( 255, 255, 255, 255 );
        }
    }

    /**
     * @brief Sets the @link Music::CombinedNote::OffsetFromPrevNote offset@endlink of the @link Music::CombinedNote note@endlink being displayed.
     * @param[in] aOffset The offset from the previous @link Music::CombinedNote note@endlink.
    */
    public void SetOffset( Music.NOTE_LENGTH aOffset )
    {
        mOffset = aOffset;
    }

    /**
     * @brief Sets the image used to represent the @link Music::CombinedNote::OffsetFromPrevNote offset@endlink of the @link Music::CombinedNote note@endlink being displayed.
     * @param[in] aImage The image representing the @link Music::CombinedNote::OffsetFromPrevNote offset@endlink of the @link Music::CombinedNote note@endlink being displayed.
    */
    public void SetOffsetImage( Sprite aImage )
    {
        if( aImage == null )
        {
            mOffsetImage.color = new Color32( 255, 255, 255, 0 );
        }
        else
        {
            mOffsetImage.sprite = aImage;
            mOffsetImage.color = new Color32( 255, 255, 255, 255 );
        }

    }

    /**
     * @brief Sets the @link DocSC_MDP parent container@endlink.
     * @param[in] aParent The @link DocSC_MDP parent container@endlink.
     * 
     * @see MeasureDisplayPanel::mNotes
    */
    public void SetParentContainer( SC_MeasureDisplayPanel aParent )
    {
        mParent = aParent;
    }

    /**
     * @brief Sets the index for this @link Music::CombinedNote note@endlink in the Song.
     * @param[in] aIndex The index of the @link Music::CombinedNote note@endlink in the Song.
    */
    public void SetNoteIndex( int aIndex )
    {
        mNoteIndex = aIndex;
    }

    /**
     * @brief Sets the string that represents the @link Music::PITCH pitches@endlink in the @link Music::CombinedNote note@endlink being displayed.
     * @param[in] aPitchString The string representing the @link Music::PITCH pitches@endlink in the @link Music::CombinedNote note@endlink being displayed.
    */
    public void SetPitches( string aPitchString )
    {
        mPitches.text = aPitchString;
    }

    /**
     * Sets the string representing the @link DefVel velocity@endlink of the @link Music::CombinedNote note@endlink.
     * @param[in] aVelocityString The string representing the @link DefVel velocity@endlink of the @link Music::CombinedNote note@endlink.
    */
    public void SetVelocity( string aVelocityString )
    {
        mVelocity.text = aVelocityString;
    }

    /**
     * @brief Sets that we're no longer editing the note that this panel represents.
    */
    public void StopEditing()
    {
        // Change the panel background color back to its original color.
        gameObject.GetComponent<Image>().color = new Color32( 58, 58, 58, 255 );
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_NDPHandlers Event Handlers
    * @ingroup DocSC_NDP
    * Functions that are called to handle events.
    * @{
    *****************************************************************************/

    /**
     * @brief Handles when the @link SC_NoteDisplayPanel::mRemoveNoteButton remove button@endlink is clicked.
    */
    public void OnRemoveButtonClicked()
    {
        // Set the note index for all notes after this one
        int sib = transform.GetSiblingIndex() + 1;
        int index = mNoteIndex;
        while( sib < transform.parent.childCount )
        {
            if( transform.parent.GetChild( sib ).GetComponent<SC_NoteDisplayPanel>() != null )
            {
                transform.parent.GetChild( sib ).GetComponent<SC_NoteDisplayPanel>().SetNoteIndex( index );
                index++;
            }
            sib++;
        }

        // Signal that this note display should be removed.
        mParent.OnRemoveNote( this );

        // Signal that this note should be removed from the song.
        GameObject.Find( "SongCreationCanvas" ).GetComponent<SongCreationManager>().OnRemoveNote( mNoteIndex );
    }

    /** 
     * @brief Handles clicking on the background panel which means that we're either beginning an edit to this @link Music::CombinedNote note@endlink or cancelling it.
     * @param[in] aPointerData The PointerEventData representing the mouse click.
    */
    private void TriggerEditEvent( PointerEventData aPointerData )
    {
        if( aPointerData.button == PointerEventData.InputButton.Left )
        {
            // Change the color of the background panel to show that we're editing the note that it 
            // represents.
            gameObject.GetComponent<Image>().color = new Color32( 116, 116, 116, 255 );

            // Send the event to the song creation handler.
            mSongCreationHandler.OnEditEvent( this );
        }
    }
    /** @} */
}
 
#endif
 
 
 
 