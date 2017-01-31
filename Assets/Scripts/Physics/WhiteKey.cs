using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

/**
 * @class WhiteKey
 * @brief A script for handling the behavior of a white key on the keyboard
 * 
 * This class handles the behavior of a Unity GameObject that represents a white key
 * on the keyboard. 
 * */
public class WhiteKey : MonoBehaviour
{
    /*************************************************************************//** 
     * @}
     * @defgroup WhiteKeyEventTypes Event Types
     * @ingroup DocWhiteKey
     * The types of events for a WhiteKey.
     * @{
    ****************************************************************************/
        public class WhiteKeyPressedEvent : UnityEvent<WhiteKey> { } //!< Used for testing the container. Will probably need to change later.
        public class WhiteKeyReleasedEvent : UnityEvent<WhiteKey> { } //!< Used for testing the container. Will probably need to change later.

    /*************************************************************************//** 
     * @}
     * @defgroup WhiteKeyEvents Events
     * @ingroup DocWhiteKey
     * Events used by the WhiteKey.
     * @{
    ****************************************************************************/
    public WhiteKeyPressedEvent WhiteKeyPressed = null; //!< Used for testing the container. Will probably need to change later.
    public WhiteKeyReleasedEvent WhiteKeyReleased = null; //!< Used for testing the container. Will probably need to change later.

    /*************************************************************************//** 
     * @}
     * @defgroup WhiteKeyPubVar Public Variables
     * @ingroup DocWhiteKey
     * Variables for setting attributes of a WhiteKey in the Unity Editor.
     * @{
    ****************************************************************************/
    public float Velocity; //!< The velocity of the key object.
    public Music.PITCH Pitch = Music.PITCH.C4; //!< The pitch that this key represents.

    // I put a default value since it'll mess up the dynamic loading otherwise.
    public string Key = "A"; //!< The key that begins pressing the white key. 

    /*************************************************************************//** 
     * @}
     * @defgroup WhiteKeyPrivVar Private Variables
     * @ingroup DocWhiteKey
     * Variables used internally by the WhiteKey.
     * @{
     ****************************************************************************/
    private bool mKeyPressed = false; //!< Has the note started?
    private KeyCode mKeyCode; //!< The keycode to check for when testing physics.
    private KeyContainer mContainer = null; //!< The parent @link DocKeyContain container@endlink.
    private Rigidbody mRigidBody; //!< The rigidbody representing the key.

    /*************************************************************************//** 
     * @}
     * @defgroup WhiteKeyUnity Unity Functions
     * @ingroup DocWhiteKey
     * Built-in Unity functions for creating and updating the WhiteKey.
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
        mKeyCode = (KeyCode)System.Enum.Parse( typeof( KeyCode ), Key );
        mRigidBody = GetComponent<Rigidbody>();
        mRigidBody.useGravity = false;
        WhiteKeyPressed = new WhiteKeyPressedEvent();
        WhiteKeyReleased = new WhiteKeyReleasedEvent();
    }

    /**
     * @brief Called every frame. Used for loading the VirtualInstrumentManager.
     * 
     * Checks if audio should be loaded/unloaded and handles the asynchronous scene
     * operation that does so.
     */
    void Update()
    {
    }

    /**
     * @brief Called every physics step. Used for applying torque to the RigidBody.
     * 
     * Checks if the appropriate key was pressed/released and applies a torque if so.
     * It also calls the functions that send input to the VirtualInstrumentManager if
     * needed.
     */
    void FixedUpdate()
    {
        //float move = Input.GetAxis ("Horizontal");
        Vector3 movement = new Vector3( 0, 0.0f, -1f );
        if( Input.GetKey( mKeyCode ) )
        {
            //Debug.Log( key );
            mRigidBody.AddTorque( movement );

            // Handle playing the sound if audio is loaded.
            if( !mKeyPressed )
            {
                Debug.Log( "Key representing " + Music.NoteToString( Pitch ) + " was pressed." );
                mKeyPressed = true;
                WhiteKeyPressed.Invoke( this );
            }
        }
        else
        {
            mRigidBody.AddTorque( movement * -1f );

            // Handle releasing the note if audio is loaded
            if( mKeyPressed )
            {
                Debug.Log( "Key representing " + Music.NoteToString( Pitch ) + " was released." );
                mKeyPressed = false;
                WhiteKeyReleased.Invoke( this );
            }
        }
    }

    /*************************************************************************//** 
     * @}
     * @defgroup WhiteKeyPubFunc Public Functions
     * @ingroup DocWhiteKey
     * Functions for other classes to configure the WhiteKey.
     * @{
    *****************************************************************************/

    /**
     * @brief Sets the @link DocKeyContain parent container@endlink.
     * @param[in] aContainer The parent container.
    */
    public void SetParentContainer( KeyContainer aContainer )
    {
        mContainer = aContainer;
    }
    /** @} */
}
