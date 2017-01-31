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

public class ATI_NoteRangeSelectionHandler : ATI.SliderHandler
{

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Use this for initialization
    new void Start()
    {

#if DEBUG && DEBUG_MUSICAL_TYPING

        // Call the base start function.
        base.Start();

        // Get the slider and set up its trigger
        ATI.SliderTrigger st = null;
        mSliders = new Slider[1];
        mSliders[0] = gameObject.transform.GetComponent<Slider>();
        st = mSliders[0].gameObject.AddComponent<ATI.SliderTrigger>();
        st.SetHandler( this );
        st.SetType( ATI.SliderType.NoteRange );

        mVIM.InstrumentLoaded.AddListener( HandleInstrumentLoaded );
#endif

    }

    // Update is called once per frame
    void Update()
    {

    }

#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Event Handlers
    //---------------------------------------------------------------------------- 

    protected override void HandleNoteRangeChange( bool aEndDrag )
    {
        // Set the text.
        gameObject.transform.GetChild( 3 ).GetComponent<Text>().text = "Range: " + Music.NoteToString( (int)mSliders[0].value )
            + " to " + Music.NoteToString( (int)mSliders[0].value + mVIM.GetNumActiveNotes() - 1 );

        // If the value has been set, then invoke the virtual instrument manager's ChangeNoteRange event.
        if( aEndDrag )
        {
            mVIM.ChangeNoteRange.Invoke( (Music.PITCH)mSliders[0].value );
        }
    }

    private void HandleInstrumentLoaded()
    {
        // Update the slider.
        mSliders[0].minValue = (float)mVIM.GetLowestSupportedNote();
        mSliders[0].maxValue = (float)( mVIM.GetHighestSupportedNote() - mVIM.GetNumActiveNotes() + 1 );
        mSliders[0].value = (float)mVIM.GetLowestActiveNote();



        // Update the slider text.
        // Set the text.
        gameObject.transform.GetChild( 3 ).GetComponent<Text>().text = "Range: " + Music.NoteToString( (int)mSliders[0].value )
            + " to " + Music.NoteToString( (int)mSliders[0].value + mVIM.GetNumActiveNotes() - 1 );
    }

#endif

}
