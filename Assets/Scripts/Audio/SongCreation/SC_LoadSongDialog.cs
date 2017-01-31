#if SONG_CREATION_ENABLED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

/**
 * @class SC_LoadSongDialog
 * @brief A dialog that loads a song into the @link DocSC Song Creation Interface@endlink.
*/
public class SC_LoadSongDialog : MonoBehaviour
{
    /*************************************************************************//** 
    * @defgroup SC_LSDEventTypes Event Types
    * @ingroup DocSC_LSD
    * These are the types of events for the @link DocSC_LSD dialog to load a song@endlink.
    * @{
    ******************************************************************************/

    /**
     * @brief A type of event that is invoked when a song is selected to be loaded.
     * 
     * @see SC_LoadSongDialog::OnSongSelectedEvent
    */
    public class SongSelectedEvent : UnityEvent<Song> { }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_LSDEvents Events
    * @ingroup DocSC_LSD
    * These are the events for the @link DocSC_LSD dialog to load a song@endlink.
    * @{
    ******************************************************************************/
    public SongSelectedEvent SongSelected; //!< The event that is invoked when a song is selected. No handler for this event in this class. It's meant to notify other classes.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_LSDPrivVar Private Variables
    * @ingroup DocSC_LSD
    * These are variables that are used internally by the @link DocSC_LSD handler 
    * for loading a song into the Song Creation Interface@endlink.
    * @{
    ******************************************************************************/
    private Button mCancelButton = null; //!< The button to cancel loading a song.
    private Button mLoadButton = null; //!< The button to load the selected song.
    private Dropdown mSongSelectionMenu = null; //!< The dropdown menu to select a song to load.
    private Song mSelectedSong = null; //!< The currently selected song.
    private VirtualInstrumentManager mVIM = null; //!< The @link VIM Virtual Instrument Manager@endlink.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_LSDUnity Unity Functions
    * @ingroup DocSC_LSD
    * These are functions automatically called by Unity
    * @{
    ******************************************************************************/

    /** 
     * @brief Initializes the SC_LoadSongDialog by getting references to its components.
    */
    private void Awake()
    {
        // Get the virtual instrument manager.
        mVIM = GameObject.Find( "VirtualInstrumentManager" ).GetComponent<VirtualInstrumentManager>();
        Assert.IsNotNull( mVIM, "Could not get a reference to the VirtualInstrumentManager!" );

        // Set up the event.
        SongSelected = new SongSelectedEvent();

        // Set up the selection menu by getting a reference to it and clearing its options. Then, fill its options
        // with the songs in the SongManager. Also add its listener.
        mSongSelectionMenu = transform.GetChild( 1 ).GetChild( 0 ).GetComponent<Dropdown>();
        mSongSelectionMenu.options.Clear();
        mSongSelectionMenu.AddOptions( mVIM.SongManager.GetSongNames() );
        mSongSelectionMenu.onValueChanged.AddListener( OnSongSelected );

        // Get the default 
        mSelectedSong = mVIM.SongManager.GetSongs()[0];

        // Set up the buttons.
        mLoadButton = transform.GetChild( 1 ).GetChild( 2 ).GetComponent<Button>();
        mLoadButton.onClick.AddListener( OnLoadButtonClicked );
        mCancelButton = transform.GetChild( 1 ).GetChild( 1 ).GetComponent<Button>();
        mCancelButton.onClick.AddListener( OnCancelButtonClicked );

    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_LSDHandlers Event Handlers
    * @ingroup DocSC_LSD
    * These are functions called by the SC_LoadSongDialog to handle events.
    * @{
    ******************************************************************************/

    /**
     * @brief Gets the selected Song from the @link SongManagerClass song manager@endlink.
     * @param[in] aIndex The index of the selected song.
    */
    private void OnSongSelected( int aIndex )
    {
        mSelectedSong = mVIM.SongManager.GetSongByName( mSongSelectionMenu.options[aIndex].text );
    }

    /**
     * @brief Handles the @link SC_LoadSongDialog::mCancelButton Cancel button@endlink being clicked with self-destruction.
    */
    private void OnCancelButtonClicked()
    {
        DestroyImmediate( gameObject, false );
    }

    /**
     * @brief Handles the @link SC_LoadSongDialog::mLoadButton Load button@endlink being clicked by invoking the SongSelectedEvent and destroying itself.
    */
    private void OnLoadButtonClicked()
    {
        SongSelected.Invoke( mSelectedSong );
        DestroyImmediate( gameObject, false );
    }

    /** @} */
}

#endif
