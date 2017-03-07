using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

/**
 * @class StopAndGoHandler
 * @brief Handles Learn Mode for both songs and theory. 
*/
public class StopAndGoHandler : MonoBehaviour
{
    /**
     * @brief The possible states of the stop and go handler.
    */
    public enum SAG_STATE
    {
        UNINITIALIZED, //!< The stop and go handler is waiting to be initialized.
        IDLE, //!< The stop and go handler has been initialized, but is not currently running.
        DEMO_SONG, //!< A demo of the song is being shown.
        DEMO_SONG_FINISHED, //!< The demo of the song is finished. 
        DEMO_KEY, //!< A demo of what to hit is currently being shown.
        WAITING, //!< Waiting for the user to hit the current key hit.
        WAITING_FOR_CHORD, //!< The user has hit one or more of the keys in the key hit, but they have not yet hit all of them.
        CORRECTLY_HIT //!< The user hit the current key hit correctly.
    }

    public UnityEvent KeyHitCorrectly = null; //!< An event that notifies of a key being hit correctly.

    private int mCurrentKeyHitIndex = 0; //!< The index of the current key hit.
    private Lesson mLesson = null; //!< The lesson being learned.
    private PianoKey[] mKeys = null; //!< The loaded keys.
    private SAG_STATE mState = SAG_STATE.UNINITIALIZED; //!< The current state of the stop and go handler.

	/**
     * @brief Initializes Learn Mode.
    */
	private void Awake()
    {
        KeyHitCorrectly = new UnityEvent();
	}
	
	// Update is called once per frame
	private void Update ()
    {
		
	}

    /**
     * @brief Sets the lesson for the stop and go handler.
     * @param[in] aLesson The lesson for the stop and go handler.
     * @param[in] aKeys The loaded keys for the lesson.
    */
    public void SetLessonAndKeys( Lesson aLesson, PianoKey[] aKeys )
    {
        mLesson = aLesson;
        mKeys = aKeys;
    }

    /** 
     * @brief Begins Demoing the song in the lesson.
    */
    public void BeginSongDemo()
    {
        mState = SAG_STATE.DEMO_SONG;
        
    }

    public void DemoSong()
    {

    }

}
