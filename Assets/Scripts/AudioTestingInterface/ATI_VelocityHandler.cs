//---------------------------------------------------------------------------- 
// /Scripts/AudioTestingInterface/ATI_VelocityHandler.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: Script that manages the velocities that are used when 
//              Musical typing simulates a note event in the 
//              AudioTestingInterface scene. 
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ATI_VelocityHandler : MonoBehaviour {

#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Constants
    //---------------------------------------------------------------------------- 

    private string[] keys =
        { "a: ", "w: ", "s: ", "e: ", "d: ", "f: ", "t: ", "g: ", "y: ", "h: ", "u: ", "j: ", "k: ", "o: ", "l: ", "p: ", ";: ", "': ", "]: " };

    //---------------------------------------------------------------------------- 
    // Types
    //---------------------------------------------------------------------------- 

    // Nested class that handles sending a new slider value to the parent handler.
    public class VelocitySlider : MonoBehaviour
    {
        public ATI_VelocityHandler     parentHandler = null; // The parent handler.
        public Slider                  slider = null; // The associated slider.

        private void Start()
        {
            // Get the associated slider and add the listener. 
            slider = gameObject.GetComponent<Slider>();
            slider.onValueChanged.AddListener( OnValueChange );

            // Get the parent handler
            parentHandler = gameObject.transform.parent.GetComponent<ATI_VelocityHandler>();
        }

        private void Update()
        {
        }

        // Handler that passes a new value to the parent handler.
        // IN: aValue The new value.
        public void OnValueChange( float aValue )
        {
            parentHandler.InvokeChangeKeyVelocityEvent( aValue, slider );
        }
    }

#endif

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private VirtualInstrumentManager vmm = null;           // The virtual instrument manager.
    private Slider[] mSliders = null;                      // The sliders for each key 
    private Text[] mText = null;                           // The text objects for each slider.

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Use this for initialization
    void Start () {
   
#if DEBUG && DEBUG_MUSICAL_TYPING

        // Get the virtual instrument manager.
        vmm = GameObject.Find( "Main Camera" ).GetComponent<VirtualInstrumentManager>();

        // Allocate the slider and text arrays.
        mSliders = new Slider[19];
        mText = new Text[19];


        for( int i = 0; i < 19; i++ )
        {
            // For each slider, put it in the array and add the nested class as a component
            mSliders[i] = gameObject.transform.GetChild( i ).GetComponent<Slider>();
            mSliders[i].gameObject.AddComponent<VelocitySlider>();

            // For each text, put it in the array and set its value
            mText[i] = gameObject.transform.GetChild( i ).GetChild( 4 ).GetComponent<Text>();
            mText[i].text = keys[i] + "100";
        }
    }
#endif

    // Update is called once per frame
    void Update () {
		
	}

#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Invokes the virtual instrument manager's ChangeKeyVelocityEvent.
    public void InvokeChangeKeyVelocityEvent( float aValue, Slider aSlider )
    {
        for( int i = 0; i < 19; i++ )
        {
            if( aSlider == mSliders[i] )
            {
                mText[i].text = keys[i] + aValue.ToString();
                vmm.DEBUG_ChangeKeyVelocity.Invoke( i, (int)aValue );
            }
        }
    }
#endif

}
