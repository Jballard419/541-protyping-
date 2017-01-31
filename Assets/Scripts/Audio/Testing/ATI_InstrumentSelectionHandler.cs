//---------------------------------------------------------------------------- 
// /Scripts/AudioTestingInterface/ATI_InstrumentSelectionHandler.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: Script that handles selecting between different instruments in
//              the AudioTestingInterface scene.
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATI_InstrumentSelectionHandler : MonoBehaviour
{



#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    VirtualInstrumentManager mVIM = null; // The virtual instrument manager.

#endif


    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Use this for initialization
    void Start()
    {

#if DEBUG && DEBUG_MUSICAL_TYPING
        mVIM = GameObject.Find( "VirtualInstrumentManager" ).GetComponent<VirtualInstrumentManager>();
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

    // Handles a change in the selected instrument
    // IN: aSelection The index of the selected instrument. See the Music.INSTRUMENT_TYPE
    //     enum for which instrument the index corresponds to.
    public void OnInstrumentSelected( int aSelection )
    {
        mVIM.ChangeInstrument.Invoke( (Music.INSTRUMENT_TYPE)aSelection );
    }

#endif
}
