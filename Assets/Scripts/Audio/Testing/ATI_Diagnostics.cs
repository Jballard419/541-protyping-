/** @addtogroup Audio
 *  @{
 *  @addtogroup AudioTesting
 *  @{
 */
#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/** @class ATI_Diagnostics
 *  @brief Displays audio diagnostic information.
 *  
 *  This class handles the ATI_AudioDiagnosticsScene which 
 *  displays information regarding the audio in order 
 *  to ensure that everything is working properly.
 *  
 *  @nosubgrouping
 */  
public class ATI_Diagnostics : MonoBehaviour {

    /*************************************************************************//** 
     * @}
     * @name Events
     * @{
     ****************************************************************************/
    public UnityEvent SetInputTime; //!< The event that is invoked when the note event occurs.
    public UnityEvent SetOutputTime; //!< The event that is invoked when the note output occurs.

    /*************************************************************************//** 
     * @}
     * @name Private Variables
     * @{
     ****************************************************************************/
    private System.DateTime mInputTime; //!< A DateTime object to keep track of the note's beginning time.
    private System.DateTime mOutputTime; //!< A DateTime object to keep track of when the note actually plays.
    private Text mLatencyText = null; //!< The text object which displays latency.

    /*************************************************************************//** 
     * @}
     * @name Unity Functions
     * @{
     ****************************************************************************/

    /**
     * @brief Called when the object is created. Gets the display text and sets up events.
     */ 
    void Awake()
    {
        // Get the text to display latency.
        mLatencyText = transform.GetChild( 0 ).GetComponent<Text>();
        mLatencyText.text = "Latency: N/A";

        // Set up the input and output time.
        mInputTime = System.DateTime.Now;
        mOutputTime = System.DateTime.Now;

        // Set up the events:
        SetInputTime = new UnityEvent();
        SetInputTime.AddListener( UpdateInputTime );
        SetOutputTime = new UnityEvent();
        SetOutputTime.AddListener( UpdateOutputTime );
    }

    /**
     * @brief Called every frame. Used to update text.
    */
    private void Update()
    {
        // Update the display.
        mLatencyText.text = "Latency: " + ( mOutputTime - mInputTime ).ToString();
    }

    /*************************************************************************//** 
     * @}
     * @name Event Handlers
     * @{
     ****************************************************************************/

    /**
     * @brief Update the input time
     */
    private void UpdateInputTime()
    {
        mInputTime = System.DateTime.Now;
    }

    /** 
     * @brief Updates the output time
     */
    private void UpdateOutputTime()
    {
        // Get the output time.
        mOutputTime = System.DateTime.Now;
        GC.Collect();
    }
}
#endif
/** @} @} */
