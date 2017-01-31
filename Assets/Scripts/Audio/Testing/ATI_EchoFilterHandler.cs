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
using UnityEngine.SceneManagement;

public class ATI_EchoFilterHandler : ATI.AudioEffectHandler
{

#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private GameObject                                     mDecayContainer = null; // The input field to get the value for decay.
    private GameObject                                     mDelayContainer = null; // The input field to get the value for delay.
    private GameObject                                     mDryMixContainer = null; // The input field to get the value for the dry mix.
    private GameObject                                     mWetMixContainer = null; // The input field to get the value for the wet mix.
    private VirtualInstrumentManager.EchoFilterParameters  mParams; // The echo filter parameters.

#endif 

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Use this for initialization
    new void Start()
    {

#if DEBUG && DEBUG_MUSICAL_TYPING
        // Call the base start function and set the parameter scene name.
        base.Start();
        mParamSceneName = "EchoFilterParametersScene";
#endif

    }

#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Protected Functions
    //---------------------------------------------------------------------------- 

    // Sets the default echo filter parameters.
    protected override void SetDefaultParameters()
    {
        // Set the default parameters.
        mParams.Active = false;
        mParams.Decay = 0.25f;
        mParams.Delay = 200f;
        mParams.DryMix = 1f;
        mParams.WetMix = 1f;
    }

    // Sends the chosen parameters to the VirtualInstrumentManager
    protected override void SendParametersToVIM()
    {
        // Invoke the virtual instrument manager's ModifyEchoFilterEvent.
        mVIM.ModifyEchoFilter.Invoke( mParams );
    }

    // Turns on the effect.
    protected override void TurnOnEffect()
    {
        // Mark that the effect is enabled in both the parameters and the member variable.
        mEnabled = true;
        mParams.Active = true;

        // Change the button image.
        mToggleImage.sprite = mButtonImages[1];

        // Send the parameters to the Virtual Instrument Manager.
        SendParametersToVIM();
    }

    // Turns off the effect.
    protected override void TurnOffEffect()
    {
        // Mark that the effect is no longer enabled in both the parameters
        // and the member variable.
        mEnabled = false;
        mParams.Active = false;

        // Change the button image.
        mToggleImage.sprite = mButtonImages[0];

        // Set the parameter objects to null.
        mParamObject = null;
        mDryMixContainer = null;
        mWetMixContainer = null;
        mTriggers = null;
        mDecayContainer = null;
        mDelayContainer = null;

        // Send the parameters to the Virtual Instrument Manager.
        SendParametersToVIM();
    }

    //---------------------------------------------------------------------------- 
    // Event Handlers
    //---------------------------------------------------------------------------- 

    // Handles a change in the decay parameter.
    // IN: aValue The parameter's new value.
    protected override void HandleDecayChange( float aValue )
    {
        mParams.Decay = aValue / 100f;
        SendParametersToVIM();
    }

    // Handles a change in the delay parameter.
    // IN: aValue The parameter's new value.
    protected override void HandleDelayChange( float aValue )
    {
        mParams.Delay = aValue;
        SendParametersToVIM();
    }

    // Handles a change in the dry mix parameter.
    // IN: aValue The parameter's new value.
    protected override void HandleDryMixChange( float aValue )
    {
        mParams.DryMix = aValue / 100f;
        SendParametersToVIM();
    }

    // Handles when the scene has finished loading.
    protected override void HandleSceneLoad()
    {
        // Initialize the AudioEffectParameterTrigger array.
        mTriggers = new ATI.AudioEffectParameterTrigger[4];

        // Get the dry mix parameter object and set its values.
        mDryMixContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 2 ).gameObject;
        mTriggers[0] = mDryMixContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[0].SetHandler( this );
        mTriggers[0].SetType( ATI.AudioEffectParameterType.DryMix );
        mTriggers[0].SetRange( 0f, 100f );
        mTriggers[0].SetValue( mParams.DryMix * 100f );

        // Get the wet mix parameter object and set its values.
        mWetMixContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 3 ).gameObject;
        mTriggers[1] = mWetMixContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[1].SetHandler( this );
        mTriggers[1].SetType( ATI.AudioEffectParameterType.WetMix );
        mTriggers[1].SetRange( 0f, 100f );
        mTriggers[1].SetValue( mParams.WetMix * 100f );

        // Get the delay parameter object and set its values.
        mDelayContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 0 ).gameObject;
        mTriggers[2] = mDelayContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[2].SetHandler( this );
        mTriggers[2].SetType( ATI.AudioEffectParameterType.Delay );
        mTriggers[2].SetRange( 10f, 5000f );
        mTriggers[2].SetValue( mParams.Delay );

        // Get the decay parameter object and set its values.
        mDecayContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 1 ).gameObject;
        mTriggers[3] = mDecayContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[3].SetHandler( this );
        mTriggers[3].SetType( ATI.AudioEffectParameterType.Decay );
        mTriggers[3].SetRange( 0f, 100f );
        mTriggers[3].SetValue( mParams.Decay * 100f );
    }

    // Handles a change in the wet mix parameter.
    // IN: aValue The parameter's new value.
    protected override void HandleWetMixChange( float aValue )
    {
        mParams.WetMix = aValue / 100f;
        SendParametersToVIM();
    }
#endif

}
