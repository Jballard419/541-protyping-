using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

/**
 * @class Countdown
 * @brief Provides a countdown timer and display.
*/
public class Countdown : MonoBehaviour
{
    public UnityEvent CountdownFinished = null; //!< The event that is invoked when the countdown is finished.

    private bool mCountdownActive = false; //!< Is the countdown currently active?
    private float mIncrementLength = 1f; //!< The percentage of a second that is used as increments. Example: Wanting the countdown in terms of the length of a beat in a song.
    private float mTime = 0.0f; //!< The time that has passed since the countdown started.
    private float mCountdownLength = 0.0f; //!< The length of the countdown.

	/** 
     * @brief Initializes the countdown display.
    */
	private void Awake()
    {
        // Set up the event for when the countdown is finished.
        CountdownFinished = new UnityEvent();
	}
	
	/**
     * @brief Updates the countdown if it is active.
    */
	void Update()
    {
        // See if the countdown is active.
        if( mCountdownActive )
        {
            // Update the time.
            mTime += ( Time.deltaTime * mIncrementLength );

            // See if the countdown needs to stop.
            if( mTime >= mCountdownLength )
            {
                // Invoke the finished event.
                CountdownFinished.Invoke();

                // Reset the variables.
                mTime = 0.0f;
                mCountdownLength = 0.0f;
                mCountdownActive = false;
            }
        }
	}

    /**
     * @brief Begins the countdown
     * @param[in] aLength The length of the countdown in seconds.
    */
    public void BeginCountdown( float aLength )
    {
        // Set the member variables. The actual countdown handling will take place in Update(). 
        mTime = 0.0f;
        mCountdownLength = aLength * mIncrementLength;
        mCountdownActive = true;
    }

    /**
     * @brief Overloaded function that begins the countdown while setting the length of increments.
     * @param[in] aLength The length of the countdown in seconds.
     * @param[in] aIncrementLength Optional parameter for setting the length of increments. Defaults to 1 which means that the countdown is in terms of seconds.
    */
    public void BeginCountdown( float aLength, float aIncrementLength )
    {
        // Update the increment length.
        mIncrementLength = aIncrementLength;

        // Set the member variables. The actual countdown handling will take place in Update(). 
        mTime = 0.0f;
        mCountdownLength = aLength * mIncrementLength;
        mCountdownActive = true;
    }

    /**
     * @brief Cancels the countdown.
    */
    public void CancelCountdown()
    {
        mTime = 0.0f;
        mCountdownLength = 0.0f;
        mCountdownActive = false;
    }

    /**
     * @brief Overloaded function that cancels the countdown but provides the remaining time as an output parameter.
     * @param[out] remainingTime The time remaining when the countdown was cancelled.
     * @param[in] aAsIncrement Return the remaining time in terms of increments? Default is false meaning that the remaining time is returned in seconds.
    */
    public void CancelCountdown( out float remainingTime )
    {
        // Get the remaining time
        remainingTime = GetRemainingTime();

        // Cancel the countdown
        CancelCountdown();
    }

    /**
     * @brief Gets the remaining time in the countdown.
     * @return The remaining time in the countdown.
    */
    public float GetRemainingTime()
    {
        return ( ( mCountdownLength - mTime ) / mIncrementLength );
    }

    /**
     * @brief Sets the increment length.
     * @param[in] aIncrementLength The length of an increment.
    */
    public void SetIncrementLength( float aIncrementLength )
    {
        mIncrementLength = aIncrementLength;
    }
}
