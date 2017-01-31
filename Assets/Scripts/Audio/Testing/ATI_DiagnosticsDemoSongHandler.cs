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

public class ATI_DiagnosticsDemoSongHandler : MonoBehaviour
{
    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    Button mPlayButton = null; // The button to play the selected demo song.
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

        // Set up the selection menu.
        mSongSelectionMenu = gameObject.GetComponent<Dropdown>();
        mSongSelectionMenu.options.Clear();

        mSongSelectionMenu.AddOptions( mVIM.SongManager.GetCombinedSongNames() );
        mSongSelectionMenu.AddOptions( mVIM.SongManager.GetMelodyNames() );
        mSongSelectionMenu.onValueChanged.AddListener( LoadDemoSong );

        // Load the default demo song.
        mSong = mVIM.SongManager.GetSongs()[0];

        // Set up the play button
        mPlayButton = transform.GetChild( 5 ).GetComponent<Button>();
        mPlayButton.onClick.AddListener( OnPlayButtonClicked );

        // Set up the BPM slider.
        mBPMSlider = gameObject.transform.GetChild( 4 ).GetComponent<Slider>();
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

    // Plays the loaded song when the play button is clicked.
    public void OnPlayButtonClicked()
    {
        mVIM.PlaySong.Invoke( mSong );
    }

}
