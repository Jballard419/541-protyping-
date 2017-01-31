#if SONG_CREATION_ENABLED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * @class SC_PitchSelectionTrigger
 * @brief Informs the @link DocSC_PSC parent handler@endlink about a @link Music::PITCH pitch@endlink or @link Music::DRUM drum@endlink being selected in the @link DocSC Song Creation Interface@endlink.
*/
public class SC_PitchSelectionTrigger : MonoBehaviour
{
    /*************************************************************************//** 
    * @defgroup SC_PSTPrivVar Private Variables
    * @ingroup DocSC_PST
    * These are variables that are used internally by the SC_PitchSelectionTrigger
    * @{
    ******************************************************************************/
    private int mIndex = -1; //!< The index of this trigger in @link DocSC_PSC the parent handler@endlink.
    private SC_PitchSelectionContainer mHandler = null; //!< @link DocSC_PSC The parent handler@endlink.
    private Toggle mToggle; //!< The associated toggle switch that indicates whether or not the represented @link Music::PITCH pitch@endlink or @link Music::DRUM drum@endlink is selected.

    /*************************************************************************//** 
    * @}
    * @defgroup SC_PSTUnity Unity Functions
    * @ingroup DocSC_PST
    * These are functions that are automatically called by Unity.
    * @{
    ******************************************************************************/

    /**
     * @brief Initializes the SC_PitchSelectionTrigger by getting a reference to its @link SC_PitchSelectionTrigger::mToggle toggle switch@endlink.
    */
    private void Awake()
    {
        // Get the toggle and add its listener.
        mToggle = gameObject.GetComponent<Toggle>();
        mToggle.onValueChanged.AddListener( OnPitchSelected );
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_PSTPubFunc Public Functions
    * @ingroup DocSC_PST
    * These are functions that allow other classes to interact with the SC_PitchSelectionTrigger.
    * @{
    ******************************************************************************/

    /**
     * @brief Sets @link DocSC_PSC the parent handler@endlink.
     * @param[in] aHandler @link DocSC_PSC The parent handler@endlink.
    */
    public void SetHandler( SC_PitchSelectionContainer aHandler )
    {
        mHandler = aHandler;
    }

    /**
     * @brief Sets the index of this trigger in @link DocSC_PSC the parent handler@endlink.
     * @param[in] aIndex The index of this trigger in @link DocSC_PSC the parent handler@endlink.
    */
    public void SetIndex( int aIndex )
    {
        mIndex = aIndex;
    }

    /**
     * @brief Sets whether or not this trigger is selected.
     * @param[in] aSelected True if the trigger is selected. False otherwise.
    */
    public void SetSelection( bool aSelected )
    {
        // Update the toggle switch.
        mToggle.onValueChanged.RemoveListener( OnPitchSelected );
        mToggle.isOn = aSelected;
        mToggle.onValueChanged.AddListener( OnPitchSelected );
    }

    /*************************************************************************//** 
    * @}
    * @defgroup SC_PSTHandlers Event Handlers
    * @ingroup DocSC_PST
    * These are functions that are called by the SC_PitchSelectionTrigger in response to events.
    * @{
    ******************************************************************************/

    /**
     * @brief Handles when a the trigger is selected via the @link SC_PitchSelectionTrigger::mToggle associated toggle switch@endlink by @link SC_PitchSelectionContainer::HandlePitchSelection passing the event@endlink to the @link DocSC_PSC parent handler@endlink.
     * @param[in] aSelected True if the trigger was selected. False otherwise.
    */
    public void OnPitchSelected( bool aSelected )
    {
        mHandler.HandlePitchSelection( aSelected, mIndex );
    }
    /** @} */
}
#endif