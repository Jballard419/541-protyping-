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
using UnityEngine.UI;

public class ATI_DemoSongButtonHandler : MonoBehaviour {

    public bool DrumLoopSelector = false; // Is this a drum loop selector?

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    bool mActive = true; // Is the demo song menu active?
    Button mPlayButton = null; // The button to play the selected demo song.
    Dropdown mSongSelectionMenu = null; // The dropdown menu to select a demo song.
    Slider mBPMSlider = null; // The slider to control the song's bpm.
    Song mSong = null; // The loaded demo song.
    Text mBPMSliderText = null; // The text to show the current bpm for the song. 
    VirtualInstrumentManager mVIM = null; // The virtual instrument manager.

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 
    void Start ()
    {
        // Get the virtual instrument manager.
        mVIM = GameObject.Find( "VirtualInstrumentManager" ).GetComponent<VirtualInstrumentManager>();

        // Account for the VIM changing instruments.
        mVIM.ChangeInstrument.AddListener( HandleInstrumentChange );

        // Set up the selection menu.
        mSongSelectionMenu = gameObject.GetComponent<Dropdown>();
        mSongSelectionMenu.options.Clear();

        if( !DrumLoopSelector )
        {
            mSongSelectionMenu.AddOptions( mVIM.SongManager.GetSongNames() );
            mSongSelectionMenu.onValueChanged.AddListener( LoadDemoSong );
            
            // Load the default demo song.
            mSong = mVIM.SongManager.GetSongs()[0];
        }
        else
        {
            mSongSelectionMenu.AddOptions( mVIM.SongManager.GetDrumLoopNames() );
            mSongSelectionMenu.onValueChanged.AddListener( LoadDemoSong );
            mSong = mVIM.SongManager.GetDrumLoops()[0];
        }
       
        // Set up the play button.
        mPlayButton = gameObject.transform.GetChild( 4 ).GetComponent<Button>();
        mPlayButton.onClick.AddListener( OnPlayButtonClicked );



        // Set up the BPM slider.
        mBPMSlider = gameObject.transform.GetChild( 5 ).GetComponent<Slider>();
        mBPMSlider.value = mSong.GetBPM();
        mBPMSlider.onValueChanged.AddListener( ChangeBPM );

        // Set up the BPM slider's label.
        mBPMSliderText = mBPMSlider.transform.GetChild( 3 ).GetComponent<Text>();
        mBPMSliderText.text = "BPM: " + mSong.GetBPM().ToString();
	}

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Loads a demo song.
    public void LoadDemoSong( int aIndex )
    {
        mSong = mVIM.SongManager.GetSong( mSongSelectionMenu.options[aIndex].text );
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
            mPlayButton.gameObject.SetActive( false );
            mSongSelectionMenu.gameObject.SetActive( false );
        }
        else if( !mActive )
        {
            mActive = true;
            mBPMSlider.gameObject.SetActive( true );
            mPlayButton.gameObject.SetActive( true );
            mSongSelectionMenu.gameObject.SetActive( true );
        }
    }

    // Plays the loaded song when the play button is clicked.
    public void OnPlayButtonClicked()
    {
        if( DrumLoopSelector )
        {
            mVIM.PlayDrumLoop.Invoke( mSong );
        }
        else
        {
            mVIM.PlaySong.Invoke( mSong );
        }
    }

}
