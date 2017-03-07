using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class CountdownDisplay : MonoBehaviour
{
    private bool mActive = false; //!< Is the countdown display active?
    private Countdown mCountdown = null; //!< The countdown that is being displayed.
    private Text mCountdownText = null; //!< The text that displays the countdown.

    /**
     * @brief Initializes the countdown display.
    */
    private void Awake()
    {
        // Get a reference to the countdown text.
        mCountdownText = transform.GetChild( 0 ).GetChild( 0 ).GetComponent<Text>();
        Assert.IsNotNull( mCountdownText, "Could not get the text displaying the countdown!" );

        // Add the countdown object as a component.
        mCountdown = gameObject.AddComponent<Countdown>();

        // Add a listener for when the countdown is finished.
        mCountdown.CountdownFinished.AddListener( OnCountdownFinished );

        #if DEBUG_COUNTDOWN_DISPLAY
            ShowCountdown( 3f );
        #else
            // Make the countdown display inactive.
            gameObject.SetActive( false );
        #endif


    }

    /**
     * @brief Updates the countdown display.
    */
    private void Update()
    {
        // If the display is active, then update the text.
		if( mActive )
        {
            mCountdownText.text = Mathf.CeilToInt( mCountdown.GetRemainingTime() ).ToString();
        }
	}

    /**
     * @brief Shows the countdown display.
     * @param[in] aLength The length of the countdown display.
     * @param[in] The length of increments in the countdown. Defaults to 1. @see Countdown::mIncrementLength.
    */
    public void ShowCountdown( float aLength, float aIncrementLength = 1f )
    {
        // Show the display by making the gameobject active.
        gameObject.SetActive( true );
        mActive = true;

        // Set the beginning text.
        mCountdownText.text = Mathf.CeilToInt( aLength / aIncrementLength ).ToString();

        // Begin the countdown.
        mCountdown.BeginCountdown( aLength, aIncrementLength );
    }

    private void OnCountdownFinished()
    {
        // Mark that the countdown display is no longer active.
        mActive = false;
        gameObject.SetActive( false );
    }
}
