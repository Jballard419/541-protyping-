//---------------------------------------------------------------------------- 
// /Scripts/AudioTestingInterface/ATI_EchoFilterHandler.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: Script that handles testing the echo filter and changing its
//              parameters in the AudioTestingInterface scene.
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ATI_EchoFilterHandler : MonoBehaviour {

#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Types
    //---------------------------------------------------------------------------- 

    // A class that is attached to each input field to manage sending the value appropriately.
    private class ATI_EchoFilterInputFieldHandler : MonoBehaviour
    {
        private string type = null; // The type of input field.
        private InputField field; // The associated input field.
        private ATI_EchoFilterHandler parentHandler; // The parent handler.

        private void Start()
        {
            // Get the associated input field and set a handler to handle when
            // editing the field is finished.
            field = gameObject.GetComponent<InputField>();
            field.onEndEdit.AddListener( SendChangeToHandler );
        }

        private void Update()
        {
        }

        // Handler to get a change in value and send it to the parent handler.
        private void SendChangeToHandler( string aNewValue )
        {
            float newValue = float.Parse( aNewValue );

            // Delay ranges from 10 to 5000. The other values range from 0 to 1 but 
            // are given from the input field as 0 to 100% so they must be divided by 100.
            if( type == "Delay" )
            {
                parentHandler.GetChangeFromInputField( type, newValue );
            }
            else
            {
                parentHandler.GetChangeFromInputField( type, ( newValue / 100f ) );
            }

        }

        // Sets the values for the type and the parent handler.
        public void SetValues( string aType, ATI_EchoFilterHandler aParent )
        {
            type = aType;
            parentHandler = aParent;
        }


    }

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private bool                                           active = false; // Is the filter active?
    private GameObject                                     decayInput = null; // The input field to get the value for decay.
    private GameObject                                     delayInput = null; // The input field to get the value for delay.
    private GameObject                                     dryInput = null; // The input field to get the value for the dry mix.
    private GameObject                                     echoToggleObject = null; // The object that toggles if the filter is active or not.
    private GameObject                                     wetInput = null; // The input field to get the value for the wet mix.
    private Sprite[]                                       toggleImages = null; // The images to show whether or not the filter is active.
    private VirtualInstrumentManager.EchoFilterParameters  echoParams; // The echo filter parameters.
    private VirtualInstrumentManager                       vmm = null; // The virtual instrument manager.

#endif 

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Use this for initialization
    void Start ()
    {

#if DEBUG && DEBUG_MUSICAL_TYPING

        // Get the virtual instrument manager.
        vmm = GameObject.Find( "Main Camera" ).GetComponent<VirtualInstrumentManager>();

        // Set the default parameters.
        echoParams.Active = false;
        echoParams.Decay = 0.25f;
        echoParams.Delay = 200f;
        echoParams.DryMix = 1f;
        echoParams.WetMix = 1f;

        // Set up values for the images that will be used to denote if the filter is active or not.
        toggleImages = new Sprite[2];
        toggleImages[0] = Resources.Load<Sprite>( "Music/Images/off_button" );
        toggleImages[1] = Resources.Load<Sprite>( "Music/Images/on_button" );
        echoToggleObject = gameObject.transform.GetChild( 3 ).gameObject;
        echoToggleObject.GetComponent<Toggle>().onValueChanged.AddListener( OnEchoFilterToggle );

        // Set values for the input fields.
        SetInputFieldValues();

#endif

    }
	
	// Update is called once per frame
	void Update ()
    {
	}


#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Gets a change in value from one of the input fields
    // IN: aType The type of input field.
    // IN: aNewValue The value that has been changed.
    public void GetChangeFromInputField( string aType, float aNewValue )
    {
        float oldValue = aNewValue;
        
        // Set the new value, but keep track of the old value in case the new value is invalid.
        switch( aType )
        {
            case "Decay":
                oldValue = echoParams.Decay;
                echoParams.Decay = aNewValue;
                break;
            case "Delay":
                oldValue = echoParams.Delay;
                echoParams.Delay = aNewValue;
                break;
            case "DryMix":
                oldValue = echoParams.DryMix;
                echoParams.DryMix = aNewValue;
                break;
            case "WetMix":
                oldValue = echoParams.WetMix;
                echoParams.WetMix = aNewValue;
                break;
            default:
                break;
        }

        // If the new value is valid, then send the parameters.
        if( vmm.CheckEchoFilterParameters( echoParams ) )
        {
            SendEchoFilterParameters();
        }

        // If the new value is not valid, then reset the input field's text and 
        // set the parameter back to its old value.
        else
        {
            switch( aType )
            {
                case "Decay":
                    echoParams.Decay = oldValue;
                    decayInput.transform.FindChild( "EchoDecayInputText" ).GetComponent<Text>().text 
                        = oldValue.ToString();
                    break;
                case "Delay":
                    echoParams.Delay = oldValue;
                    delayInput.transform.FindChild( "EchoDelayInputText" ).GetComponent<Text>().text
                        = oldValue.ToString();
                    break;
                case "DryMix":
                    echoParams.DryMix = oldValue;
                    dryInput.transform.FindChild( "EchoDryMixInputText" ).GetComponent<Text>().text
                        = oldValue.ToString();
                    break;
                case "WetMix":
                    echoParams.WetMix = oldValue;
                    wetInput.transform.FindChild( "EchoWetMixInputText" ).GetComponent<Text>().text
                        = oldValue.ToString();
                    break;
                default:
                    break;
            }
        }
    }

    //---------------------------------------------------------------------------- 
    // Private Functions
    //---------------------------------------------------------------------------- 

    // Sends the chosen parameters to the VirtualInstrumentManager
    private void SendEchoFilterParameters()
    {
        if( active )
        {
            vmm.ModifyEchoFilter.Invoke( echoParams );
        }
    }

    // Sets the values for each input field.
    private void SetInputFieldValues()
    {
        // Get each of the input fields and set the values.
        ATI_EchoFilterInputFieldHandler temp;

        delayInput = gameObject.transform.GetChild( 2 ).gameObject;
        temp = delayInput.AddComponent<ATI_EchoFilterInputFieldHandler>();
        temp.SetValues( "Delay", this );

        decayInput = gameObject.transform.GetChild( 1 ).gameObject;
        temp = decayInput.AddComponent<ATI_EchoFilterInputFieldHandler>();
        temp.SetValues( "Decay", this );

        dryInput = gameObject.transform.GetChild( 0 ).GetChild( 1 ).gameObject;
        temp = dryInput.AddComponent<ATI_EchoFilterInputFieldHandler>();
        temp.SetValues( "DryMix", this );

        wetInput = gameObject.transform.GetChild( 0 ).GetChild( 0 ).gameObject;
        temp = wetInput.AddComponent<ATI_EchoFilterInputFieldHandler>();
        temp.SetValues( "WetMix", this );
    }

    //---------------------------------------------------------------------------- 
    // Event Handlers
    //---------------------------------------------------------------------------- 

    // Handles when the filter is toggled on or off.
    // IN: aOn Is the filter turned on?
    public void OnEchoFilterToggle( bool aOn )
    {
        if( aOn )
        {
            echoToggleObject.transform.GetChild( 0 ).GetComponent<Image>().sprite = toggleImages[1];
            active = true;
            echoParams.Active = true;
            SendEchoFilterParameters();
        }
        else
        {
            echoToggleObject.transform.GetChild( 0 ).GetComponent<Image>().sprite = toggleImages[0];
            active = false;
            echoParams.Active = false;
            SendEchoFilterParameters();
        }
    }

#endif

}
