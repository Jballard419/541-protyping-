using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using Leap.Unity;

using Leap.Unity.Interaction;

/**
 * @class PianoKey
 * @brief A script for handling the behavior of a piano key on the keyboard
 * 
 * This class handles the behavior of a Unity GameObject that represents a piano key
 * on the keyboard. 
 * */
public class PianoKey : MonoBehaviour
{
    /*************************************************************************//** 
     * @defgroup PianoKeyTypes Types
     * @ingroup DocPianoKey
     * Custom types related to a PianoKey.
     * @{
    ****************************************************************************/
    public enum KEY_STATE
    {
        DISABLED, //!< The key is currently disabled (no collisions allowed and no simulation occurring.)
        IDLE, //!< The key is currently idle (allows for collisions). 
        SIMULATED_PRESS, //!< The key is currently simulating itself being pressed.
        SIMULATED_HOLD, //!< The key is currently simulating being held down.
        SIMULATED_RELEASE, //!< The key is currently simulating its release.
        PRESSED, //!< The key has been hit.
        HELD_DOWN, //!< The key is being held down.
        RELEASED //!< The key has been released.
    };

    /*************************************************************************//** 
     * @}
     * @defgroup PianoKeyEventTypes Event Types
     * @ingroup DocPianoKey
     * The types of events for a PianoKey.
     * @{
    ****************************************************************************/
    public class PianoKeyPressedEvent : UnityEvent<PianoKey, int> { } //!< Used for informing the container about a key press.
    public class PianoKeyReleasedEvent : UnityEvent<PianoKey> { } //!< Used for informing the container about a key press.

    /*************************************************************************//** 
     * @}
     * @defgroup PianoKeyEvents Events
     * @ingroup DocPianoKey
     * Events used by the PianoKey.
     * @{
    ****************************************************************************/
    public PianoKeyPressedEvent PianoKeyPressed = null; //!< Used for testing the container. Will probably need to change later.
    public PianoKeyReleasedEvent PianoKeyReleased = null; //!< Used for testing the container. Will probably need to change later.

    /*************************************************************************//** 
     * @}
     * @defgroup PianoKeyPubVar Public Variables
     * @ingroup DocPianoKey
     * Variables for setting attributes of a PianoKey in the Unity Editor.
     * @{
    ****************************************************************************/
    public float Torque; //!< The velocity of the key object.
    public int NumberOfCyclesBeforeRelease = 3;
    public LayerMask LayersWhoseCollisionShouldBeHandled = 1 << 9;
    public Music.PITCH Pitch = Music.PITCH.C4; //!< The pitch that this key represents.
    #if DEBUG_KEYS
        public int DEBUG_NumberOfCyclesBeforeReporting = 10;
        private int DEBUG_mNumCycles = 0;
    #endif

    // I put a default value since it'll mess up the dynamic loading otherwise.
    public string Key = "A"; //!< The key that begins pressing the  key. 
    public InteractionBehaviour mInteractionBehaviour = null; //!< The interaction behaviour for the key

    /*************************************************************************//** 
     * @}
     * @defgroup PianoKeyPrivVar Private Variables
     * @ingroup DocPianoKey
     * Variables used internally by the PianoKey.
     * @{
     ****************************************************************************/
    private bool mKeyPressed = false; //!< Has the note started?
    private bool mEnabled = true; //!< Can the user interact with the key?
    private BoxCollider mCollider = null; //!< The collider for the object.
    private Countdown mSimulationCountdown = null; //!< A countdown attached to the key. Used for simulation of pressing, holding, and releasing the key.
    private int mNumCyclesSincePressed = 0; //!< The number of cycles since pressed.
    private int mNumCyclesSinceReleased = 0; 
    private float mCurrentRotation = 0f; //!< The current angle of the key.
    private KeyCode mKeyCode; //!< The keycode to check for when testing physics.
    private KeyContainer mContainer = null; //!< The parent @link DocKeyContain container@endlink.
    private KEY_STATE mState = KEY_STATE.IDLE;
    private Rigidbody mRigidBody = null; //!< The rigidbody representing the key.
    private Vector3 mPressedPosition_L; //!< The local position of the key when it is pressed.
	private Vector3 mReferencePosition_W; //!< The reference world position for collisions.
	private Vector3 mIdlePosition_L; //!< The local position of the key when it is not pressed or moving.
    private VirtualInstrumentManager mVIM = null; //!< The bridge to the audio.
    private Rigidbody mCollided = null; //!< The rigidbody that collided with the key.

    /*************************************************************************//** 
     * @}
     * @defgroup PianoKeyUnity Unity Functions
     * @ingroup DocPianoKey
     * Built-in Unity functions for creating and updating the PianoKey.
     * @{
     ****************************************************************************/

    /**
     * @brief Called when object is initialized.
     * 
     * Sets the key that is used to simulate pressing the object and gets the RigidBody
     * and sets that it shouldn't use gravity.
     */
    void Awake()
    {
        // Set the collision layer.
        gameObject.layer = 8;
        mInteractionBehaviour = GetComponent<InteractionBehaviour>();

        // Get the rigidbody.
        mRigidBody = GetComponent<Rigidbody>();

        // Get the collider.
        mCollider = GetComponent<BoxCollider>();

        // Set up the events.
        PianoKeyPressed = new PianoKeyPressedEvent();
        PianoKeyReleased = new PianoKeyReleasedEvent();
    }

    /**
     * @brief Draws information about the key to the scene editor.
    */
    private void OnDrawGizmos()
    {
        // Draw the reference position.
        Gizmos.DrawIcon( mReferencePosition_W, "hinge", false );

        // Draw a line from the key to the collided object.
        if( mKeyPressed && mCollided != null )
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine( transform.position, mCollided.transform.position );
        }
    }

    /**
     * @brief Called every frame. Used for updating the key's position in order to simulate its movement.
     * 
     * Checks if the appropriate key was pressed/released and moves the key if so.
     */
    void Update()
    {
        // Simulate the key's physics if needed.
        if( mEnabled )
        {
            SimulateKeyPhysics();
        }
    }



    /*************************************************************************//** 
     * @}
     * @defgroup PianoKeyPubFunc Public Functions
     * @ingroup DocPianoKey
     * Functions for other classes to configure the PianoKey.
     * @{
    *****************************************************************************/

    /**
     * @brief Gives a countdown to the key. 
     * @param[in] aCountdown The countdown for the key. 
     * 
     * @note The countdown is passed by reference so that multiple keys can share one. Make sure that this is taken into account when modifying the countdown.
    */
    public void SetCountdown( ref Countdown aCountdown )
    {
        mSimulationCountdown = aCountdown;
    }

    /**
     * @brief Sets the color of the key.
     * @param[in] aColor The new color of the key.
     * 
     * @note Should only be used in Learn Mode. The color of keys for Practice and Recital modes are handled by giving the key a countdown.
    */ 
    public void SetKeyColor( Color aColor )
    {
        gameObject.GetComponent<Renderer>().material.color = aColor;
    }

    /**
     * @brief Sets whether the user can interact with the key.
     * @param[in] aKeyEnabled True if the user can interact with the key. False otherwise.
    */
    public void SetKeyEnabled( bool aKeyEnabled )
    {
        mEnabled = aKeyEnabled;

        // Make sure that the key is in the idle position if it is disabled.
        if( !mEnabled )
        {
            transform.localPosition = mIdlePosition_L;
            mCurrentRotation = 0f;
            transform.rotation = Quaternion.Euler( Vector3.zero );
        }
    }

    /**
     * @brief Sets the @link DocKeyContain parent container@endlink.
     * @param[in] aContainer The parent container.
    */
    public void SetParentContainer( KeyContainer aContainer )
    {
        mContainer = aContainer;
        mInteractionBehaviour.Manager = mContainer.mInteractionManager;
    }

    public void SetReferencePosition( Vector3 aPosition )
    {
		mIdlePosition_L = transform.localPosition;
		mReferencePosition_W = aPosition;
    }

	public void SetPressedPosition( Vector3 aPosition )
	{
        mPressedPosition_L = aPosition;
	}

    /**
     * @brief Sets the reference to the audio
     * @param[in] aVIM The reference to the audio.
    */
    public void SetVIM( ref VirtualInstrumentManager aVIM )
    {
        mVIM = aVIM;
    } 
    /** @} */

    /**
     * @brief Simulates the key's physics and movement.
    */
    private void SimulateKeyPhysics()
    {
        // If the key was pressed, then move it according to the position of the hand.
        if( mKeyPressed && mCollided != null )
        {
            // Get the rotation needed.
            Vector3 vectorFromHandToKey = mCollided.transform.position - mReferencePosition_W;
            float newRotation = -1f * Mathf.Abs( Mathf.Atan2( vectorFromHandToKey.y, vectorFromHandToKey.x ) * Mathf.Rad2Deg );

            // Make sure that the rotation is within the limits.
            newRotation = Mathf.Min( 0, newRotation );
            newRotation = Mathf.Max( mContainer.keyAngle, newRotation );

            // Get the new position (for simulating hinge)
            Vector3 newPosition = Vector3.Slerp( mIdlePosition_L, mPressedPosition_L, newRotation / mContainer.keyAngle );
            newPosition.z = transform.localPosition.z;

            // Update the transform's position and rotation.
            transform.rotation = Quaternion.Euler( 0.0f, 0.0f, newRotation );
            mCurrentRotation = newRotation;
            transform.localPosition = newPosition;

            // Update the number of cycles that have occured since the key was pressed.
            mNumCyclesSincePressed++;
            gameObject.layer = 10;
        }

        // See if the key was released. Handled here due to unreliability of the OnTriggerExit function.
        if( mKeyPressed && mNumCyclesSincePressed >= NumberOfCyclesBeforeRelease )
        {
            // Check if any parts of the hand are still touching the key.
            Collider[] collisions = Physics.OverlapBox( transform.position, mCollider.bounds.extents, transform.rotation,
                LayersWhoseCollisionShouldBeHandled, QueryTriggerInteraction.Ignore );

            // If no parts of the hands are touching the key, then release it.
            if( collisions.Length == 0 )
            {
                if( mNumCyclesSinceReleased >= NumberOfCyclesBeforeRelease )
                {
                    // Mark that it is released and notify the Virtual Instrument Manager.
                    mKeyPressed = false;
                    PianoKeyReleased.Invoke( this );
                    mNumCyclesSincePressed = 0;
                    // Set the layer back to interactive.
                    gameObject.layer = 8;
                }
                else
                {
                    mNumCyclesSinceReleased++;
                }
            }
        }

        // If the key was released, then move it back.
        if( !mKeyPressed && mNumCyclesSinceReleased >= NumberOfCyclesBeforeRelease )
        {
            gameObject.layer = 8;
            mNumCyclesSinceReleased++;
            transform.rotation = Quaternion.Slerp( Quaternion.Euler( 0, 0, mCurrentRotation ), Quaternion.Euler( 0, 0, 0 ), Mathf.Min( mNumCyclesSinceReleased * Time.deltaTime, 1f ) );

            // Update the rotation.
            mCurrentRotation = transform.rotation.eulerAngles.z;
            if( mCurrentRotation > 0 )
            {
                mCurrentRotation -= 360f;
            }
            mCurrentRotation = Mathf.Max( -12, mCurrentRotation );
            mCurrentRotation = Mathf.Min( 0, mCurrentRotation );

            // Get the new position (for simulating hinge)
            Vector3 newPosition = Vector3.Slerp( mIdlePosition_L, mPressedPosition_L, mCurrentRotation / mContainer.keyAngle );
            newPosition.z = transform.localPosition.z;
            transform.localPosition = newPosition;
        }

        // See if we're stuck in noclip when we shouldn't be and reset the collision layer if so.
        if( !mKeyPressed && gameObject.layer != 8 )
        {
            gameObject.layer = 8;
        }
    }

    /** 
     * @brief Simulates the key being pressed.
     * @param[in] aHoldLength The length of time (in seconds) that the key should be held for.
     * @param[in] aVelocity The velocity at which the key is played.
    */
    private void SimulatePlayingKey( float aHoldLength, float aVelocity )
    {
        Assert.IsFalse( mEnabled, "You shouldn't be trying to simulate playing a key when it's enabled!" );
        Assert.IsTrue( aVelocity >= 0 && aVelocity <= 100, "Tried to simulate playing a key with a velocity that was out of range!" );

    }

    private void OnTriggerEnter( Collider other )
    {
        // Make sure that we're enabled and that we only collide with hands.
        if( mEnabled && ( other.gameObject.name == "BrushHand_L" || other.gameObject.name == "BrushHand_R" ) )
        {
            // Only handle collisions if the hands are moving down.
            Vector3 pointOfImpact = mCollider.ClosestPointOnBounds( other.transform.position );
            Vector3 impactVel = other.attachedRigidbody.GetRelativePointVelocity( mReferencePosition_W );
            // Make sure that we aren't already pushing down the key.
            if( !mKeyPressed && impactVel.y < 0 )
            {
                mCollided = other.attachedRigidbody;
                mKeyPressed = true;
                mNumCyclesSinceReleased = 0;
                mNumCyclesSincePressed = 0;

                Vector3 vectorFromHandToKey = mCollided.transform.position - mReferencePosition_W;
                float newRotation = Mathf.Atan2( vectorFromHandToKey.y, vectorFromHandToKey.x ) * Mathf.Rad2Deg;
                // Make sure that the rotation is within the limits.
                newRotation = Mathf.Min( 0, newRotation );
                newRotation = Mathf.Max( mContainer.keyAngle, newRotation );

                // Play the sound of the key.
                PianoKeyPressed.Invoke( this, (int)Mathf.Max( 100f * ( newRotation / mContainer.keyAngle ), 100 ) );

                #if DEBUG_KEYS
                    if( DEBUG_mNumCycles % DEBUG_NumberOfCyclesBeforeReporting == 0 )
                    {
                        Debug.Log( "Point of impact: " + pointOfImpact.ToString() );
                        Debug.Log( "Key representing " + Music.NoteToString( Pitch ) + " was pressed." );
                    }
                    DEBUG_mNumCycles++;
                #endif
            }
        }
    }
}
