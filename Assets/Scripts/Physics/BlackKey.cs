using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/** 
 * @class BlackKey
 * @brief A script for handling the behavior of a black key on the keyboard.
 *  
 * This class defines the behavior for a Unity GameObject that represents
 * a black key on the keyboard.
*/

public class BlackKey : MonoBehaviour
{
    /*************************************************************************//** 
     * @}
     * @defgroup BlackKeyEventTypes Event Types
     * @ingroup DocBlackKey
     * The types of events for the @link DocBlackKey black keys@endlink.
     * @{
    ****************************************************************************/
    public class BlackKeyPressedEvent : UnityEvent<BlackKey> { } //!< Used for testing the container. Will probably need to change later.
    public class BlackKeyReleasedEvent : UnityEvent<BlackKey> { } //!< Used for testing the container. Will probably need to change later.

    /*************************************************************************//** 
     * @}
     * @defgroup BlackKeyEvents Events
     * @ingroup DocBlackKey
     * Events used by the @link DocBlackKey black keys@endlink
     * @{
    ****************************************************************************/
    public BlackKeyPressedEvent BlackKeyPressed = null; //!< Used for testing the container. Will probably need to change later.
    public BlackKeyReleasedEvent BlackKeyReleased = null; //!< Used for testing the container. Will probably need to change later.

    /*************************************************************************//** 
     * @}
     * @defgroup BlackKeyPubVar Public Variables
     * @ingroup DocBlackKey
     * Variables to set attributes of a BlackKey in the Unity Editor.
     * @{
    ****************************************************************************/
    public float Velocity; //!< The velocity of the key object.
    public Music.PITCH Pitch = Music.PITCH.CS4; //!< The pitch that this key represents.

    // I put a default value since it'll mess up the dynamic loading otherwise.
    public string Key = "A"; //!< The key that begins pressing the black key. 

    /*************************************************************************//** 
     * @}
     * @defgroup BlackKeyPrivVar Private Variables
     * @ingroup DocBlackKey
     * Variables used internally by the BlackKey
     * @{
     ****************************************************************************/
    private bool mKeyPressed = false; //!< Has the note started?
    private KeyCode mKeyCode; //!< The keycode to check for when testing physics.
    private KeyContainer mContainer = null; //!< The parent @link DocKeyContain container@endlink.
    private Rigidbody mRigidBody; //!< The rigidbody representing the key.

    /*************************************************************************//** 
     * @}
     * @defgroup BlackKeyUnity Unity Functions
     * @ingroup DocBlackKey
     * Built-in Unity functions for creating and updating the BlackKey.
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
        BlackKeyPressed = new BlackKeyPressedEvent();
        BlackKeyReleased = new BlackKeyReleasedEvent();
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
     * @brief Called every physics step. Used for applying force to the RigidBody.
     * 
     * Checks if the appropriate key was pressed and applies a force if so.
     * It also calls the functions that send input to the VirtualInstrumentManager if
     * needed.
     */
    void FixedUpdate()
    {
        //float move = Input.GetAxis ("Horizontal");
        Vector3 movement = new Vector3( 0, -1f, 0 );
        if( Input.GetKey( mKeyCode ) )
        {
            //Debug.Log( Key );
            mRigidBody.AddForce( movement );

            // Let others know that this key was pressed.
            if( !mKeyPressed )
            {
                Debug.Log( "Key representing " + Music.NoteToString( Pitch ) + " was pressed." );
                mKeyPressed = true;
                BlackKeyPressed.Invoke( this );
            }

        }
        else
        {
            // If the key is released, then let others know.
            if( mKeyPressed )
            {
                Debug.Log( "Key representing " + Music.NoteToString( Pitch ) + " was released." );
                mKeyPressed = false;
                BlackKeyReleased.Invoke( this );
            }
        }
    }

    /*************************************************************************//** 
     * @}
     * @defgroup BlackKeyPubFunc Public Functions
     * @ingroup DocBlackKey
     * Functions for other classes to configure the BlackKey.
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


