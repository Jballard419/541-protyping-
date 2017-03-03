#if SONG_CREATION_ENABLED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

/** 
 * @class SC_AddDrumDialog
 * @brief A dialog for adding @link Music::DRUM drums@endlink to a @link Music::CombinedNote note@endlink in the @link DocSC Song Creation Interface@endlink.
*/
public class SC_AddDrumDialog : MonoBehaviour
{

    /*************************************************************************//** 
    * @defgroup SC_ADDConst Constants
    * @ingroup DocSC_ADD
    * These are constants that are used to get paths for the prefabs.
    * @{
    *****************************************************************************/
    private const string TOGGLE_PREFAB_PATH = "Audio/Prefabs/SongCreation/TogglePrefab"; //!< The path to load the toggle switches for selecting @link Music::DRUM drums@endlink.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_ADDEventTypes Event Types
    * @ingroup DocSC_ADD
    * These are the types of events for the SC_AddDrumDialog
    * @{
    *****************************************************************************/

    /**
     * @brief This event is used to notify the @link DocSC_NDia Note Dialog@endlink that the SC_AddDrumDialog has finished and there are new @link Music::DRUM drums@endlink to add.
     * @note The parameters are the @link Music::DRUM drums@endlink, their @link Music::NoteLength length@endlink, and their @link DefVel Note Velocity@endlink.
     * @see SC_NoteDialog::OnDrumsAdded
    */
    public class AddDrumDialogFinishedEvent : UnityEvent<Music.DRUM[], int>
    {
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_ADDEvents Events
    * @ingroup DocSC_ADD
    * These are the events used for the SC_AddDrumDialog.
    * @{
    *****************************************************************************/
    public AddDrumDialogFinishedEvent AddDrumDialogFinished; //!< The event that will be invoked in order to notify the @link DocSC_NDia Note Dialog@endlink that @link Music::DRUM drums@endlink are ready to be added.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_ADDPrivVar Private Variables
    * @ingroup DocSC_ADD
    * These are variables that are used internally by the SC_AddDrumDialog
    * @{
    *****************************************************************************/
    private Button mCancelButton = null; //!< The button for cancelling the dialog.
    private Button mDoneButton = null; //!< The button for finishing the dialog.
    private GameObject mDrumToggleDisplay = null; //!< The panel for showing the toggle switches.
    private List<Toggle> mDrumToggles = null; //!< The toggle switches for selecting @link Music::DRUM drums@endlink.
    private List<Music.DRUM> mSelectedDrums = null; //!< The selected @link Music::DRUM drums@endlink.
    private SongCreationManager.SC_InputFieldAndSlider mVelocityHandler = null; //!< The handler for setting the @link DefVel Note Velocity@endlink.
    private Text mSelectedDrumDisplay = null; //!< The text that displays the selected @link Music::DRUM drums@endlink.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_ADDUnity Unity Functions
    * @ingroup DocSC_ADD
    * These are functions automatically called by Unity.
    * @{
    *****************************************************************************/

    /**
     * @brief Initializes the dialog.
    */
    private void Awake()
    {

        // Get the display for showing selected drums.
        mSelectedDrumDisplay = transform.GetChild( 0 ).GetChild( 0 ).GetComponent<Text>();
        mSelectedDrumDisplay.text = "Added Drums: ";

        // Get the display for showing the toggle switches
        mDrumToggleDisplay = transform.GetChild( 1 ).GetChild( 0 ).GetChild( 0 ).gameObject;

        // Get the velocity handler.
        mVelocityHandler = transform.GetChild( 4 ).gameObject.AddComponent<SongCreationManager.SC_InputFieldAndSlider>();
        mVelocityHandler.SetReferenceToInputField( transform.GetChild( 3 ).gameObject.GetComponent<InputField>() );
        mVelocityHandler.SetAsInt( true );
        mVelocityHandler.SetValue( 70 );

        // Get the buttons.
        mCancelButton = transform.GetChild( 5 ).GetChild( 1 ).GetComponent<Button>();
        mCancelButton.onClick.AddListener( OnCancelButtonClicked );
        mDoneButton = transform.GetChild( 5 ).GetChild( 0 ).GetComponent<Button>();
        mDoneButton.onClick.AddListener( OnDoneButtonClicked );
        mDoneButton.gameObject.SetActive( false );

        // Set up the toggle switches.
        mDrumToggles = new List<Toggle>();
        for( int i = 0; i < Music.MAX_SUPPORTED_DRUMS; i++ )
        {
            // Create the toggle switch.
            GameObject newToggleObj = Instantiate( Resources.Load<GameObject>( TOGGLE_PREFAB_PATH ) );
            Toggle tog = newToggleObj.GetComponent<Toggle>();
            tog.isOn = false;
            tog.transform.SetParent( mDrumToggleDisplay.transform, false );
            tog.onValueChanged.AddListener( OnDrumSelected );
            tog.transform.GetChild( 1 ).GetComponent<Text>().text = Music.DrumToString( i );

            // Add the toggle switch to the list.
            mDrumToggles.Add( tog );
        }

        // Set up the selected drums.
        mSelectedDrums = new List<Music.DRUM>();

        // Set up the event.
        AddDrumDialogFinished = new AddDrumDialogFinishedEvent();
    }

    /**
     * @brief Used to clean up when the object is destroyed.
    */
    private void OnDestroy()
    {
        foreach( Toggle tog in mDrumToggles )
        {
            if( tog != null )
            {
                DestroyImmediate( tog, false );
            }
        }
        if( mDrumToggles != null )
        {
            mDrumToggles.Clear();
        }

    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_ADDHandlers Event Handlers
    * @ingroup DocSC_ADD
    * These are functions called by the SC_AddDrumDialog in order to handle events.
    * @{
    *****************************************************************************/

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
        AddDrumDialogFinished.Invoke( mSelectedDrums.ToArray(), (int)mVelocityHandler.GetValue() );
        DestroyImmediate( gameObject, false );
    }

    /**
     * @brief Handles when a todocdrum is selected
    */
    private void OnDrumSelected( bool a )
    {
        mSelectedDrums.Clear();
        int numDrums = 0;

        // Get the selected drums.
        for( int i = 0; i < mDrumToggles.Count; i++ )
        {
            if( mDrumToggles[i].isOn )
            {
                numDrums++;
                mSelectedDrums.Add( (Music.DRUM)i );
            }
        }

        // Set the selected drum display string.
        mSelectedDrumDisplay.text = "Added Drums: ";
        foreach( Music.DRUM drum in mSelectedDrums )
        {
            mSelectedDrumDisplay.text += Music.DrumToString( drum ) + " ";
        }

        // Show the done button if drums are selected.
        if( numDrums > 0 )
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
