#if DEBUG_MUSICAL_TYPING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/** 
 * @class MusicalTypingHandler
 * @brief Allows for testing the audio with the computer keyboard.
 * 
 * Musical Typing allows for using a computer keyboard to debug sound output by 
 * triggering note events for a 19-note range whenever specific keys on the 
 * keyboard are pressed/released. The velocities for each key can be set with
 * DEBUG_SetMusicalTypingKeyVelocity().
 * The keys that are used are (in this order)
 * a, w, s, e, d, f, t, g, y, h, u, j, k, o, l, p, ;, ', and ] 
*/
public class MusicalTypingHandler : MonoBehaviour {

    /*************************************************************************//** 
    * @defgroup MusTypConst Musical Typing Constants
    * @ingroup MusTyp
    * Constants used to handle @link MusTyp Musical Typing@endlink.
    ****************************************************************************/
    /** @{ */
    private static int DEBUG_numMusicalTypingKeys = 19; //!< The number of Musical Typing keys.
    private static KeyCode[] DEBUG_musicalTypingKeys =
        {
            KeyCode.A,
            KeyCode.W,
            KeyCode.S,
            KeyCode.E,
            KeyCode.D,
            KeyCode.F,
            KeyCode.T,
            KeyCode.G,
            KeyCode.Y,
            KeyCode.H,
            KeyCode.U,
            KeyCode.J,
            KeyCode.K,
            KeyCode.O,
            KeyCode.L,
            KeyCode.P,
            KeyCode.Semicolon,
            KeyCode.Quote,
            KeyCode.RightBracket
        }; //!< The keys that are used for musical typing.
    /** @} */

    /*************************************************************************//** 
    * @defgroup MusTypPubVar Musical Typing Public Variables
    * @ingroup MusTyp
    * Variables used to set options for @link MusTyp Musical Typing@endlink.
    ****************************************************************************/
    /** @{ */
    public bool                          MusicalTypingEnabled = true; //!< Is musical typing enabled? Default is true when the preprocessor flag is set.
    public bool                          RandomizeVelocities = false; //!< Should the velocities used for simulating a NotePlayEvent be randomized? Default is false.
    /** @} */

    /*************************************************************************//** 
    * @defgroup MusTypPrivVar Musical Typing Private Variables
    * @ingroup MusTyp
    * Variables used to construct @link MusTyp Musical Typing@endlink events.
    ****************************************************************************/
    /** @{ */
    #if DEBUG_AUDIO_DIAGNOSTICS
        private ATI_Diagnostics               mDiagnosticsHandler = null; //!< The handler for audio diagnostics display
    #endif
    private bool[]                        mPressedKeys = null; //!< The keys that are currently pressed.
    private int[]                         mKeyVelocities; //!< The velocities that will be used whenever musical typing simulates a note event.
    private int[]                         mRandomVelocityRange; //!< The range used to randomize Musical Typing velocities. Default is 0 to 100.
    private VirtualInstrumentManager      mVIM = null; //!< The parent VirtualInstrumentManager
    /** @} */

    /*************************************************************************//** 
    * @defgroup MusTypUnity Musical Typing Unity Functions
    * @ingroup MusTyp
    * Variables used to construct @link MusTyp Musical Typing@endlink events.
    ****************************************************************************/
    /** @{ */

    /**
     * @brief Initializes the MusicalTypingHandler by getting the parent VirtualInstrumentManager and calling SetMusicalTypingVariables.
    */
    private void Awake()
    {
        // Get the Virtual Instrument Manager
        mVIM = gameObject.GetComponent<VirtualInstrumentManager>();
        Assert.IsNotNull( mVIM, "Musical Typing couldn't get the VirtualInstrumentManager!" );

        // Set the default variables
        SetMusicalTypingVariables();
    }

    /**
     * @brief Called whenever there is a GUI event. Used to see if the event is supposed to start a Musical Typing Event.
     * 
     * @see VirtualInstrumentManager::DEBUG_HandleMusicalTypingEvent
    */
    void OnGUI()
    {
        // If musical typing is enabled, then watch for key events and send them along to the handler.
        if( MusicalTypingEnabled && Event.current.isKey &&
            ( Input.GetKeyDown( Event.current.keyCode ) || Input.GetKeyUp( Event.current.keyCode ) ) )
        {
            OnMusicalTypingEvent( Event.current );
        }
    }
    /** @} */

    /*************************************************************************//** 
    * @defgroup MusTypPubFunc Musical Typing Public Functions
    * @ingroup MusTyp
    * Functions used by other classes to set detailed options for @link MusTyp Musical Typing@endlink.
    ****************************************************************************/
    /** @{ */

    /**
     * @brief Sets the velocity used when a specific key on the computer keyboard generates a musical typing event.
     * @param[in] aKeyIndex The index of the key whose Musical Typing velocity is being modified.
     * @param[in] aKeyVelocity The new velocity for the key.
    */
    public void SetKeyVelocity( int aKeyIndex, int aKeyVelocity )
    {
        Assert.IsTrue( aKeyIndex >= 0 && aKeyIndex < 19, "Tried to change the Musical Typing velocity for a non-existent key!" );
        Assert.IsTrue( aKeyVelocity >= 0 && aKeyVelocity <= 100, "Gave a velocity of " + aKeyVelocity.ToString() + ", but the range is 0-100!" );

        mKeyVelocities[aKeyIndex] = aKeyVelocity;
    }

    /**
     * @brief Sets the range used for randomizing Musical Typing velocities.
     * @param[in] aLowestVelocity The lowest velocity of the range.
     * @param[in] aHighestVelocity The highest velocity of the range.
    */
    public void SetRandomVelocityRange( int aLowestVelocity, int aHighestVelocity )
    {
        mRandomVelocityRange[0] = aLowestVelocity;
        mRandomVelocityRange[1] = aHighestVelocity;
    }
    /** @} */

    /*************************************************************************//** 
    * @defgroup MusTypPrivFunc Musical Typing Private Functions
    * @ingroup MusTyp
    * Functions used internally by @link MusTyp Musical Typing@endlink.
    ****************************************************************************/
    /** @{ */

    /** 
     * @brief Sets the default values for musical typing.
    */
    private void SetMusicalTypingVariables()
    {
        #if DEBUG_AUDIO_DIAGNOSTICS
            // Get the diagnostics handler
            mDiagnosticsHandler = mVIM.GetDiagnosticsHandler();
        #endif

        // Set up the key velocities and the pressed keys.
        mKeyVelocities = new int[DEBUG_numMusicalTypingKeys];
        mPressedKeys = new bool[DEBUG_numMusicalTypingKeys];
        for( int i = 0; i < DEBUG_numMusicalTypingKeys; i++ )
        {
            mKeyVelocities[i] = 100;
            mPressedKeys[i] = false;
        }

        // Set up the default range used when velocities are random.
        mRandomVelocityRange = new int[2];
        mRandomVelocityRange[0] = 0;
        mRandomVelocityRange[1] = 100;
    }
    /** @} */

    /*************************************************************************//** 
    * @defgroup MusTypHandlers Musical Typing Event Handlers
    * @ingroup MusTyp
    * Functions used to handle @link MusTyp Musical Typing@endlink events.
    ****************************************************************************/
    /** @{ */

    /**
     * @brief Handler for Musical Typing Events 
     * @param[in] aKeyEvent A GUI keyboard event triggered by a key being pressed or released.
     * 
     * This function sees if a pressed/released key is one that is used for Musical Typing. 
     * If so, it simulates a note event based on the pressed/released key.
    */
    private void OnMusicalTypingEvent( Event aKeyEvent )
    {
        // Check if a musical typing key is pressed or released and fire off the 
        // corresponding PlayNote or NoteFadeOut event if so. The debug velocity is 
        // used for the events.
        int i = 0;
        int numActive = mVIM.GetNumActiveNotes();
        while( MusicalTypingEnabled && i < DEBUG_numMusicalTypingKeys && i < numActive )
        {
            // See if the key is a Musical Typing key.
            if( aKeyEvent.keyCode == DEBUG_musicalTypingKeys[i] )
            {
                // If the key was pressed, then simulate a NotePlayEvent.
                if( aKeyEvent.type == EventType.KeyDown && !mPressedKeys[i] )
                {
                    #if DEBUG_AUDIO_DIAGNOSTICS
                        mDiagnosticsHandler.SetInputTime.Invoke();
                    #endif

                    // Update the pressed keys
                    mPressedKeys[i] = true;

                    // Get the velocity that should be used.
                    int velocity = mKeyVelocities[i];
                    if( RandomizeVelocities )
                    {
                        velocity = UnityEngine.Random.Range( mRandomVelocityRange[0], mRandomVelocityRange[1] );
                    }
                    mVIM.PlayNote.Invoke( mVIM.GetActiveNotes()[i], velocity );
                }
                // If the key was released, then simulate a ReleaseNoteEvent.
                if( aKeyEvent.type == EventType.KeyUp )
                {
                    // Update the pressed keys
                    mPressedKeys[i] = false;

                    mVIM.ReleaseNote.Invoke( mVIM.GetActiveNotes()[i] );
                }
            }
            i++;
        }
    }

    /** @} */

}
#endif
