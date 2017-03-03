using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using Leap.Unity;
using UnityEngine.VR.WSA.Input;
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
    private BoxCollider mCollider = null; //!< The collider for the object.
    private int mNumCyclesSincePressed = 0; //!< The number of cycles since pressed.
    private int mNumCyclesSinceReleased = 0;
    private float mCurrentRotation = 0f; //!< The current angle of the key.
    private KeyCode mKeyCode; //!< The keycode to check for when testing physics.
    private KeyContainer mContainer = null; //!< The parent @link DocKeyContain container@endlink.
    private Rigidbody mRigidBody = null; //!< The rigidbody representing the key.
    private Vector3 mHingePosition; //!< The position that the key rotates around.
    private Vector3 mPosition; //!< The position of the key.
    private Vector3 mPressedPosition;
	private Vector3 mCollisionPosition;
	private Vector3 mLocalPosition;
    private Rigidbody mCollided = null;

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
        gameObject.layer = 8;
        mInteractionBehaviour = GetComponent<InteractionBehaviour>();
        mKeyCode = (KeyCode)System.Enum.Parse( typeof( KeyCode ), Key );
        mRigidBody = GetComponent<Rigidbody>();
        mRigidBody.useGravity = false;
        mRigidBody.centerOfMass = new Vector3( -1f, 0f, 0f );
        mCollider = GetComponent<BoxCollider>();

        PianoKeyPressed = new PianoKeyPressedEvent();
        PianoKeyReleased = new PianoKeyReleasedEvent();
    }

    /**
     * @brief Called every frame. Used for updating the key's position in order to simulate its movement.
     * 
     * Checks if the appropriate key was pressed/released and moves the key if so.
     */
    void Update()
    {
        // If the key was pressed, then move it according to the impact velocity.
        if( mKeyPressed && mCollided != null )
        {
            // Get the new rotation.
            /*      mNumCyclesSincePressed++;
                  float percentage = Mathf.Min( -1f * Torque / 100f * mNumCyclesSincePressed, 1f );

                  // Set the rotation.
                  transform.Translate( mPressedPosition * percentage ), Space.World );
                  transform.rotation = Quaternion.Slerp( Quaternion.Euler( 0, 0, 0 ), Quaternion.Euler( 0, 0, -7f ), percentage );
                  mCurrentRotation = transform.rotation.eulerAngles.z;*/
            //transform.LookAt( new Vector3( transform.position.x + mCollider.bounds.size.x, mCollided.transform.position.y, transform.position.z + mCollider.bounds.size.z ) );
            

            // Get the rotation needed.

			Vector3 vectorFromHandToKey = mCollided.transform.position - mCollisionPosition;
			float newRotation = -1f * Mathf.Abs( Mathf.Atan2( vectorFromHandToKey.y, vectorFromHandToKey.x ) * Mathf.Rad2Deg );

            // Make sure that the rotation is within the limits.
            newRotation = Mathf.Min( 0, newRotation );
			newRotation = Mathf.Max( mContainer.keyAngle, newRotation );

            // Get the new position (for simulating hinge)
            Vector3 newPosition = mPressedPosition;
            newPosition.x *= ( newRotation / mContainer.keyAngle );
            newPosition.y *= ( newRotation / mContainer.keyAngle );
			newPosition.z = transform.localPosition.z;

            // Update the transform's position and rotation.
            transform.rotation = Quaternion.Euler( 0.0f, 0.0f, newRotation );
            mCurrentRotation = transform.rotation.eulerAngles.z;
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
			
            // Get the new position (for simulating hinge)
			Vector3 newPosition = mPressedPosition;
			newPosition.x = ( mPressedPosition.x * ( mCurrentRotation / mContainer.keyAngle ) ) + mLocalPosition.x;
			newPosition.y = ( mPressedPosition.y * ( mCurrentRotation / mContainer.keyAngle ) ) + mLocalPosition.y;
			newPosition.z = transform.localPosition.z;
			transform.localPosition = newPosition;
            mCurrentRotation = Mathf.Max( -12, transform.rotation.eulerAngles.z );
            mCurrentRotation = Mathf.Min( 0, transform.rotation.eulerAngles.z );
        }

        if( !mKeyPressed && gameObject.layer != 8 )
        {
            gameObject.layer = 8;
        }
    }

    private void OnTriggerEnter( Collider other )
    {
        // Make sure that we only collide with hands.
        if( other.gameObject.name == "BrushHand_L" || other.gameObject.name == "BrushHand_R" )
        {
            // Only handle collisions if the hands are moving down.
            Vector3 pointOfImpact = mCollider.ClosestPointOnBounds( other.transform.position );
			Vector3 impactVel = other.attachedRigidbody.GetRelativePointVelocity( mCollisionPosition );
            // Make sure that we aren't already pushing down the key.
            if( !mKeyPressed && impactVel.y < 0 )
            {
                mCollided = other.attachedRigidbody;
                mKeyPressed = true;
                mNumCyclesSinceReleased = 0;

				Vector3 vectorFromHandToKey = mCollided.transform.position - mCollisionPosition;
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

    /*************************************************************************//** 
     * @}
     * @defgroup PianoKeyPubFunc Public Functions
     * @ingroup DocPianoKey
     * Functions for other classes to configure the PianoKey.
     * @{
    *****************************************************************************/

    /**
     * @brief Sets the @link DocKeyContain parent container@endlink.
     * @param[in] aContainer The parent container.
    */
    public void SetParentContainer( KeyContainer aContainer )
    {
        mContainer = aContainer;
        mInteractionBehaviour.Manager = mContainer.mInteractionManager;
    }

    public void SetHingePosition( Vector3 aPosition )
    {
        mHingePosition = aPosition;
        mPosition = transform.position;
		mLocalPosition = transform.localPosition;
		mCollisionPosition = transform.position;
		mCollisionPosition.x -= mCollider.bounds.extents.x;
		mCollisionPosition.y += mCollider.bounds.extents.y;
    }

	public void SetPressedPosition( Vector3 aPosition )
	{
		mPressedPosition.x = transform.localPosition.x + aPosition.x;
		mPressedPosition.y = transform.localPosition.y + aPosition.y;
		mPressedPosition.z = transform.localPosition.z;
	}
    /** @} */
}
