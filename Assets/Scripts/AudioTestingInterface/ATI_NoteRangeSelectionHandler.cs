//---------------------------------------------------------------------------- 
// /Scripts/AudioTestingInterface/ATI_NoteRangeSelectionHandler.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: Script that handles selecting the note range in the 
//              AudioTestingInterface scene.
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ATI_NoteRangeSelectionHandler : MonoBehaviour {



#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private Slider                     rangeSlider; // The slider that allows for changing the note range.
    private string                     rangeText = "Range: C4 to FS5"; // The text that is used to display the current range.
    private Text                       textObject; // The text object that is used to display the current range.
    private VirtualInstrumentManager   vmm = null; // The virtual instrument manager.
#endif

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Use this for initialization
    void Start () {

#if DEBUG && DEBUG_MUSICAL_TYPING

        // Get the virtual instrument manager
        vmm = GameObject.Find("Main Camera").GetComponent<VirtualInstrumentManager>();

        // Get the slider and set its min and max values.
        rangeSlider = gameObject.GetComponent<Slider>();
        rangeSlider.minValue = (int)vmm.GetInstrument().GetLowestSupportedNote();
        rangeSlider.maxValue = (int)vmm.GetInstrument().GetHighestSupportedNote() - 24;

        // Get the text object.
        textObject = gameObject.transform.GetChild( 3 ).GetComponent<Text>();

#endif

    }
	
	// Update is called once per frame
	void Update () {

	}

#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Event Handlers
    //---------------------------------------------------------------------------- 

    // Handles the value of the slider being changed.
    // IN: aNewLowestValue The new value that is the lowest note of the range.
    public void OnLowestSliderValueChanged( float aNewLowestValue )
    {
        // Update the text object.
        rangeText = "Range: " + Music.NoteToString( (int)aNewLowestValue ) + " to " + Music.NoteToString( (int)aNewLowestValue + 18 );
        textObject.text = rangeText;

        // Invoke the virtual instrument manager's ChangeNoteRange event.
        vmm.ChangeNoteRange.Invoke( (Music.PITCH)rangeSlider.value );
    }

#endif

}
