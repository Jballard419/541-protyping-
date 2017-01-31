//---------------------------------------------------------------------------- 
// /Resources/Music/Song.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: A script that handles playing demo songs in the 
//              AudioTestingInterface scene.
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class ATI_DemoSongButtonHandler : MonoBehaviour
{

    public bool DrumLoopSelector = false; // Is this a drum loop selector?

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    bool mActive = true; // Is the demo song menu active?
    private bool mFinished = false; //!< Is the song finished?
    private bool mPaused = false; //!< Is the song paused?
    private bool mPlaying = false; //!< Is the song playing?
    Button mPlayPauseButton = null; // The button to play the selected demo song.
    Button mStopButton = null; //!< The button to stop the song.
    Dropdown mSongSelectionMenu = null; // The dropdown menu to select a demo song.
    Slider mBPMSlider = null; // The slider to control the song's bpm.
    Song mSong = null; // The loaded demo song.
    Sprite[] mButtonImages = null; //!< The images for the buttons
    Text mBPMSliderText = null; // The text to show the current bpm for the song. 
    VirtualInstrumentManager mVIM = null; // The virtual instrument manager.

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 
    void Start()
    {
        // Get the button images.
        mButtonImages = new Sprite[3];
        mButtonImages[0] = Resources.Load<Sprite>( "Audio/Images/play_button" );
        mButtonImages[1] = Resources.Load<Sprite>( "Audio/Images/pause_button" );
        mButtonImages[2] = Resources.Load<Sprite>( "Audio/Images/stop_button" );

        // Get the virtual instrument manager.
        mVIM = GameObject.Find( "VirtualInstrumentManager" ).GetComponent<VirtualInstrumentManager>();

        // Account for the VIM changing instruments.
        mVIM.ChangeInstrument.AddListener( HandleInstrumentChange );

        // Set up the selection menu.
        mSongSelectionMenu = gameObject.GetComponent<Dropdown>();
        mSongSelectionMenu.options.Clear();

        if( !DrumLoopSelector )
        {
            mSongSelectionMenu.AddOptions( mVIM.SongManager.GetCombinedSongNames() );
            mSongSelectionMenu.AddOptions( mVIM.SongManager.GetMelodyNames() );
            mSongSelectionMenu.onValueChanged.AddListener( LoadDemoSong );

            // Load the default demo song.
            mSong = mVIM.SongManager.GetCombinedSongs()[0];
        }
        else
        {
            mSongSelectionMenu.AddOptions( mVIM.SongManager.GetDrumLoopNames() );
            mSongSelectionMenu.onValueChanged.AddListener( LoadDemoSong );
            mSong = mVIM.SongManager.GetDrumLoops()[0];
        }

        // Set up the play button.
        mPlayPauseButton = gameObject.transform.GetChild( 5 ).GetChild( 0 ).GetComponent<Button>();
        mPlayPauseButton.onClick.AddListener( OnPlayPauseButtonClicked );

        // Set up the stop button
        mStopButton = gameObject.transform.GetChild( 6 ).GetChild( 0 ).GetComponent<Button>();
        mStopButton.onClick.AddListener( OnStopButtonClicked );
        mStopButton.transform.parent.gameObject.SetActive( false );

        // Set up the BPM slider.
        mBPMSlider = gameObject.transform.GetChild( 4 ).GetComponent<Slider>();
        mBPMSlider.value = mSong.GetBPM();
        mBPMSlider.onValueChanged.AddListener( ChangeBPM );

        // Set up the BPM slider's label.
        mBPMSliderText = mBPMSlider.transform.GetChild( 3 ).GetComponent<Text>();
        mBPMSliderText.text = "BPM: " + mSong.GetBPM().ToString();


        

        // Add a listener for when the song is finished if needed.
        if( !DrumLoopSelector )
        {
            mVIM.AudioFinished.AddListener( SetFinished );
        }
    }

    private void Update()
    {
        if( mFinished )
        {
            mFinished = false;
            mStopButton.transform.parent.gameObject.SetActive( false );
            mPlaying = false;
            mPaused = false;
            mPlayPauseButton.image.sprite = mButtonImages[0];

        }
    }

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Loads a demo song.
    public void LoadDemoSong( int aIndex )
    {
        mSong = null;
        mSong = mVIM.SongManager.GetSongByName( mSongSelectionMenu.options[aIndex].text );
        mBPMSlider.value = mSong.GetBPM();
    }

    //---------------------------------------------------------------------------- 
    // Event Handlers
    //---------------------------------------------------------------------------- 

    // Handles when the BPM slider changes.
    public void ChangeBPM( float aBPM )
    {
        // Update the BPM slider's label.
        mBPMSliderText.text = "BPM: " + aBPM.ToString();

        // Update the loaded song.
        mSong.SetBPM( aBPM );
    }

    // Handles the VIM changing instruments
    public void HandleInstrumentChange( Music.INSTRUMENT_TYPE aType )
    {
        if( aType == Music.INSTRUMENT_TYPE.DRUM_KIT && !DrumLoopSelector )
        {
            mActive = false;
            mBPMSlider.gameObject.SetActive( false );
            mPlayPauseButton.transform.parent.gameObject.SetActive( false );
            mStopButton.transform.parent.gameObject.SetActive( false );
            mSongSelectionMenu.gameObject.SetActive( false );
        }
        else if( !mActive )
        {
            mActive = true;
            mBPMSlider.gameObject.SetActive( true );
            mPlayPauseButton.transform.parent.gameObject.SetActive( true );
            mSongSelectionMenu.gameObject.SetActive( true );
        }
    }

    // Plays the loaded song when the play button is clicked.
    public void OnPlayPauseButtonClicked()
    {
        if( DrumLoopSelector )
        {
            if( mPlaying && !mPaused )
            {
                mVIM.PauseDrumLoop.Invoke();
                mPlayPauseButton.image.sprite = mButtonImages[0];
                mPaused = true;
            }
            else if( mPaused )
            {
                mVIM.ResumeDrumLoop.Invoke();
                mPlayPauseButton.image.sprite = mButtonImages[1];
                mPaused = false;
                mPlaying = true;
            }
            else
            {
                mVIM.PlayDrumLoop.Invoke( mSong );
                mStopButton.transform.parent.gameObject.SetActive( true );
                mPlayPauseButton.image.sprite = mButtonImages[1];
                mPlaying = true;
            }
        }
        else
        {
            if( mPlaying && !mPaused )
            {
                mVIM.PauseSong.Invoke();
                mPlayPauseButton.image.sprite = mButtonImages[0];
                mPaused = true;
            }
            else if( mPaused )
            {
                mVIM.ResumeSong.Invoke();
                mPlayPauseButton.image.sprite = mButtonImages[1];
                mPaused = false;
                mPlaying = true;
            }
            else
            {
                mVIM.PlaySong.Invoke( mSong );
                mStopButton.transform.parent.gameObject.SetActive( true );
                mPlayPauseButton.image.sprite = mButtonImages[1];
                mPlaying = true;
            }
        }
    }

    public void OnStopButtonClicked()
    {
        Assert.IsTrue( mPlaying || mPaused, "Somehow we tried to stop a song that wasn't playing." );

        mStopButton.transform.parent.gameObject.SetActive( false );
        mPlaying = false;
        mPaused = false;
        mPlayPauseButton.image.sprite = mButtonImages[0];

        if( DrumLoopSelector )
        {
            mVIM.StopDrumLoop.Invoke();
        }
        else
        {
            mVIM.StopSong.Invoke();
        }
    }

    public void SetFinished()
    {
        mFinished = true;
    }
}
