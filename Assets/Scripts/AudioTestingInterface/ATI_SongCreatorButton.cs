//---------------------------------------------------------------------------- 
// /Scripts/AudioTestingInterface/ATI_SongCreatorButton.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: Script that handles loading the SongCreationInterface when the
//              button is pressed.
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ATI_SongCreatorButton : MonoBehaviour {

    //---------------------------------------------------------------------------- 
    // Private variables
    //---------------------------------------------------------------------------- 
    private Button                     LoadSongCreationButton = null; // The button that loads the song creation interface when clicked.

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Use this for initialization
    void Start () {
        LoadSongCreationButton = gameObject.GetComponent<Button>();
        LoadSongCreationButton.onClick.AddListener( LoadSongCreationInterface );
	}

    //---------------------------------------------------------------------------- 
    // Event Handlers
    //---------------------------------------------------------------------------- 

    // Loads the song creation interface scene.
    void LoadSongCreationInterface()
    {
        SceneManager.LoadScene( "SongCreationInterface", LoadSceneMode.Additive );
    }
}
