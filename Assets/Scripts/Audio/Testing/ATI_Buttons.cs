using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * @class ATI_Buttons
 * @brief Handler for buttons on the Audio Testing Interface.
*/
public class ATI_Buttons : MonoBehaviour
{

    /*************************************************************************//** 
    * @defgroup ATIButtonsPrivVar Private Variables
    * @ingroup DocATIButtons
    * These are references to the buttons.
    * @{
    ****************************************************************************/
    private Button      mLoadSongCreationButton = null; //!< The button that loads the song creation interface when clicked.
    private Button      mLoadKeyboardSceneButton = null; //!< The button that loads the keyboard scene.

    /*************************************************************************//** 
    * @}
    * @defgroup ATIButtonsUnity Unity Functions
    * @ingroup DocATIButtons
    * These are functions automatically called by Unity.
    * @{
    ****************************************************************************/

    /**
     * @brief Gets references to the buttons and adds their listeners. Called when the object is created.
    */
    void Awake()
    {
        // Get the buttons.
        mLoadSongCreationButton = transform.GetChild( 0 ).GetComponent<Button>();
        Assert.IsNotNull( mLoadSongCreationButton, "Could not find the button to load the Song Creation Interface!" );

        mLoadKeyboardSceneButton = transform.GetChild( 1 ).GetComponent<Button>();
        Assert.IsNotNull( mLoadKeyboardSceneButton, "Could not find the button to load the Keyboard scene!" );

        // Add the listeners.
        mLoadSongCreationButton.onClick.AddListener( OnLoadSongCreationButtonClicked );
        mLoadKeyboardSceneButton.onClick.AddListener( OnLoadKeyboardSceneButtonClicked );
    }

    /*************************************************************************//** 
    * @{
    * @defgroup ATIButtonsHandlers Event Handlers
    * @ingroup DocATIButtons
    * These are functions used by ATI_Buttons to handle the buttons being clicked.
    * @}
    ****************************************************************************/

    /**
    * @brief Loads the song creation interface scene.
    */
    private void OnLoadKeyboardSceneButtonClicked()
    {
        SceneManager.LoadScene( "KeyboardScene", LoadSceneMode.Single );
    }

    /**
     * @brief Loads the song creation interface scene.
    */
    private void OnLoadSongCreationButtonClicked()
    {
        SceneManager.LoadScene( "SongCreationInterfaceScene" );
    }
    /** @} */
}
