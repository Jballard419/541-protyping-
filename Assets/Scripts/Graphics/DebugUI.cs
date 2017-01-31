using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/** 
 * @class DebugUI
 * @brief Handles the UI used for debugging in the KeyboardScene.
*/
public class DebugUI : MonoBehaviour {

    /*************************************************************************//** 
    * @defgroup DebUIPrivVar Private Variables
    * @ingroup DocDebUI
    * These are variables used internally by the DebugUI.
    * @{
    ****************************************************************************/
    private Button mGoToATIButton = null; //!< The button to load the Audio Testing Interface.

    /*************************************************************************//** 
    * @}
    * @defgroup DebUIUnity Unity Functions
    * @ingroup DocDebUI
    * These are functions that are called automatically by Unity.
    * @{
    ****************************************************************************/

    /**
     * @brief Initializes the behavior for the button that loads the Audio Testing Interface.
    */
    void Awake()
    {
        // Get the button to load the Audio Testing Interface.
        mGoToATIButton = transform.GetChild( 1 ).GetComponent<Button>();
        Assert.IsNotNull( mGoToATIButton, "Could not find the button to go to the Audio Testing Interface!" );

        // Add the listener for the button to load the Audio Testing Interface.
        mGoToATIButton.onClick.AddListener( OnGoToATIButtonClicked );
	}

    /*************************************************************************//** 
    * @}
    * @defgroup DebUIHandlers Event Handlers
    * @ingroup DocDebUI
    * These are functions that are used by the DebugUI to handle events.
    * @{
    ****************************************************************************/

    /**
     * @brief Loads the Audio Testing Interface.
    */
    private void OnGoToATIButtonClicked()
    {
        SceneManager.LoadScene( "AudioTestingInterface", LoadSceneMode.Single );
    }
    /** @} */
}
