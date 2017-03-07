using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/**
 * @class LessonManager
 * @brief Performs the handling for lessons.
 * 
 * This class provides the handling for all lessons, including Learn, Practice,
 * and Recital modes.
*/
public class LessonManager : MonoBehaviour
{

    /**
     * @brief The types of lessons that are supported.
    */
    public enum LESSON_MODE
    {
        LEARN, //!< Learning a song/concept with stop-and-go playing. 
        PRACTICE, //!< Practicing specific sections of a song.
        RECITAL, //!< Playing a song all the way through.
        INACTIVE //!< The lesson manager is currently inactive.
    };

    private float mPercentageHit = 0.0f; //!< The percentage of notes that were hit correctly.
    private float mTimingWindow = 0.0f; //!< The amount of time that a note is possible to be hit correctly. Half of the window is before the note and half of it is after.
    private Countdown mCountdown = null; //!< A reference to the display that counts down before the lesson begins.
    private int mNumCorrect = -1; //!< The number of pitches that were hit correctly.
    private int mNumMissed = -1;  //!< The number of pitches that were missed.
    private int mNumPitches = -1; //!< The number of pitches in the lesson.
    private KeyContainer mKeyContainer = null; //!< The loader and handler for the piano keys.
    private LESSON_MODE mMode = LESSON_MODE.INACTIVE; //!< The current lesson mode.
    private Song mSong = null; //!< The song that the lesson is based on.
    private VirtualInstrumentManager mVIM = null; //!< The bridge to the audio.

    /**
     * @brief Initializes the lesson manager
    */
    private void Awake()
    {
        // Keep the lesson manager persistent.
        DontDestroyOnLoad( this );

        // Get a reference to the audio.
        GameObject vimObject = GameObject.Find( "VirtualInstrumentManager" );
        Assert.IsNotNull( vimObject, "The Virtual Instrument Manager is not loaded in the scene!" );
        mVIM = vimObject.GetComponent<VirtualInstrumentManager>();
        Assert.IsNotNull( mVIM, "Could not get a reference to the Virtual Instrument Manager!" );
        
	}
	
	// Update is called once per frame
	private void Update()
    {
		
	}


}
