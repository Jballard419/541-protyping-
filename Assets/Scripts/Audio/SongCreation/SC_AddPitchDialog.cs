#if SONG_CREATION_ENABLED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

/** 
 * @class SC_AddPitchDialog
 * @brief A dialog for adding @link Music::PITCH pitches@endlink to a @link Music::CombinedNote note@endlink in the @link DocSC Song Creation Interface@endlink.
*/
public class SC_AddPitchDialog : MonoBehaviour {

    /*************************************************************************//** 
    * @defgroup SC_APDConst Constants
    * @ingroup DocSC_APD
    * These are constants that are used to get paths for the prefabs.
    * @{
    *****************************************************************************/
    private const string TOGGLE_PREFAB_PATH = "Audio/Prefabs/SongCreation/TogglePrefab"; //!< The path to load the toggle switches for selecting @link Music::PITCH pitches@endlink.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_APDEventTypes Event Types
    * @ingroup DocSC_APD
    * These are the types of events for the SC_AddPitchDialog
    * @{
    *****************************************************************************/

    /**
     * @brief This event is used to notify the @link DocSC_NDia Note Dialog@endlink that the SC_AddPitchDialog has finished and there are new @link Music::PITCH pitches@endlink to add.
     * @note The parameters are the @link Music::PITCH pitches@endlink, their @link Music::NoteLength length@endlink, and their @link DefVel Note Velocity@endlink.
     * @see SC_NoteDialog::OnPitchesAdded
    */
    public class AddPitchDialogFinishedEvent : UnityEvent<Music.PITCH[], Music.NoteLength, int>
    {
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_APDEvents Events
    * @ingroup DocSC_APD
    * These are the events used for the SC_AddPitchDialog.
    * @{
    *****************************************************************************/
    public AddPitchDialogFinishedEvent AddPitchDialogFinished; //!< The event that will be invoked in order to notify the @link DocSC_NDia Note Dialog@endlink that @link Music::PITCH pitches@endlink are ready to be added.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_APDPrivVar Private Variables
    * @ingroup DocSC_APD
    * These are variables that are used internally by the SC_AddPitchDialog
    * @{
    *****************************************************************************/
    private Button mCancelButton = null; //!< The button for cancelling the dialog.
    private Button mDoneButton = null; //!< The button for finishing the dialog.
    private GameObject mPitchToggleDisplay = null; //!< The panel for showing the toggle switches.
    private List<Toggle> mPitchToggles = null; //!< The toggle switches for selecting @link Music::PITCH pitches@endlink.
    private List<Music.PITCH> mSelectedPitches = null; //!< The selected @link Music::PITCH pitches@endlink.
    private SC_LengthOffsetSelectionPanel mLengthSelector = null; //!< The panel to select a @link Music::NoteLength note's length@endlink.
    private SongCreationManager.SC_InputFieldAndSlider mVelocityHandler = null; //!< The handler for setting the @link DefVel Note Velocity@endlink.
    private Text mSelectedPitchDisplay = null; //!< The text that displays the selected @link Music::PITCH pitches@endlink.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_APDUnity Unity Functions
    * @ingroup DocSC_APD
    * These are functions automatically called by Unity.
    * @{
    *****************************************************************************/

    /**
     * @brief Initializes the dialog.
    */
    private void Awake()
    {
        // Get the display for showing selected pitches.
        mSelectedPitchDisplay = transform.GetChild( 0 ).GetChild( 0 ).GetComponent<Text>();
        mSelectedPitchDisplay.text = "Added Pitches: ";

        // Get the display for showing the toggle switches
        mPitchToggleDisplay = transform.GetChild( 1 ).GetChild( 0 ).GetChild( 0 ).gameObject;

        // Get the velocity handler.
        mVelocityHandler = transform.GetChild( 4 ).gameObject.AddComponent<SongCreationManager.SC_InputFieldAndSlider>();
        mVelocityHandler.SetReferenceToInputField( transform.GetChild( 3 ).gameObject.GetComponent<InputField>() );
        mVelocityHandler.SetValue( 70 );

        // Get the length selector.
        mLengthSelector = transform.GetChild( 6 ).gameObject.AddComponent<SC_LengthOffsetSelectionPanel>();
        mLengthSelector.SetSelected( new Music.NoteLength( Music.NOTE_LENGTH_BASE.Q ) );

        // Get the buttons.
        mCancelButton = transform.GetChild( 7 ).GetChild( 1 ).GetComponent<Button>();
        mCancelButton.onClick.AddListener( OnCancelButtonClicked );
        mDoneButton = transform.GetChild( 7 ).GetChild( 0 ).GetComponent<Button>();
        mDoneButton.onClick.AddListener( OnDoneButtonClicked );
        mDoneButton.gameObject.SetActive( false );

        // Set up the toggle switches.
        mPitchToggles = new List<Toggle>();
        for( int i = 0; i < Music.MAX_SUPPORTED_NOTES + 1; i++ )
        {
            // Create the toggle switch.
            GameObject newToggleObj = Instantiate( Resources.Load<GameObject>( TOGGLE_PREFAB_PATH ) );
            Toggle tog = newToggleObj.GetComponent<Toggle>();
            tog.isOn = false;
            tog.transform.SetParent( mPitchToggleDisplay.transform, false );
            tog.onValueChanged.AddListener( OnPitchSelected );
            tog.transform.GetChild( 1 ).GetComponent<Text>().text = Music.NoteToString( i );

            // Add the toggle switch to the list.
            mPitchToggles.Add( tog );
        }

        // Set up the selected pitches.
        mSelectedPitches = new List<Music.PITCH>();

        // Set up the event.
        AddPitchDialogFinished = new AddPitchDialogFinishedEvent();
	}

    /**
     * @brief Used to clean up when the object is destroyed.
    */
    private void OnDestroy()
    {
        foreach( Toggle tog in mPitchToggles )
        {
            if( tog != null )
            {
                DestroyImmediate( tog, false );
            }
        }
        if( mPitchToggles != null )
        {
            mPitchToggles.Clear();
        }

    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_APDHandlers Event Handlers
    * @ingroup DocSC_APD
    * These are functions called by the SC_AddPitchDialog in order to handle events.
    * @{
    *****************************************************************************/

    /**
     * @brief Sets the default length
     * @param[in] aLength The default length
    */
    public void SetDefaultLength( Music.NoteLength aLength )
    {
        mLengthSelector.SetSelected( aLength );
    }

    /**
     * @brief Handles the cancel button being clicked.
    */
    private void OnCancelButtonClicked()
    {
        DestroyImmediate( gameObject, false );
    }

    /**
     * @brief Handles the done button being clicked.
    */
    private void OnDoneButtonClicked()
    {
        AddPitchDialogFinished.Invoke( mSelectedPitches.ToArray(), mLengthSelector.GetSelected(), (int)mVelocityHandler.GetValue() );
        DestroyImmediate( gameObject, false );
    }

    /**
     * @brief Handles when a todocpitch is selected
    */
    private void OnPitchSelected( bool a )
    {
        mSelectedPitches.Clear();

        // Get the selected pitches.
        int numPitches = 0;
        for( int i = 0; i < mPitchToggles.Count; i++ )
        {
            if( mPitchToggles[i].isOn )
            {
                numPitches++;
                mSelectedPitches.Add( (Music.PITCH)i );
            }
        }

        // Set the selected pitches display text.
        mSelectedPitchDisplay.text = "Added Pitches: ";
        foreach( Music.PITCH pitch in mSelectedPitches )
        {
            mSelectedPitchDisplay.text += Music.NoteToString( pitch ) + " ";
        }

        // Show the done button if pitches are selected.
        if( numPitches > 0 )
        {
            mDoneButton.gameObject.SetActive( true );
        }
        else
        {
            mDoneButton.gameObject.SetActive( false );
        }
    }
    /** @} */
}

#endif
