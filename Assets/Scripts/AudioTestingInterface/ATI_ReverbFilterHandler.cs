//---------------------------------------------------------------------------- 
// /Scripts/AudioTestingInterface/ATI_ReverbFilterHandler.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: Script that handles testing the reverb filter and changing its
//              parameters in the AudioTestingInterface scene.
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ATI_ReverbFilterHandler : ATI.AudioEffectHandler
{

#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Types
    //---------------------------------------------------------------------------- 



    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private GameObject                                          mDryLevelContainer = null; // The input field to get the value for the dry level
    private GameObject                                          mRoomContainer = null; // The input field to get the value for the room level
    private GameObject                                          mRoomHFContainer = null; // The input field to get the value for the high-frequency room level
    private GameObject                                          mDecayTimeContainer = null; // The input field to get the value for the decay time
    private GameObject                                          mDecayHFRatioContainer = null; // The input field to get the value for the ratio of high-frequency decay time to low-frequency decay time.
    private GameObject                                          mReflectionsContainer = null; // The input field to get the value for the level of early reflections.
    private GameObject                                          mReflectDelayContainer = null; // The input field to get the value for the delay time of early reflections.
    private GameObject                                          mReverbContainer = null; // The input field to get the value for the late reverberation level.
    private GameObject                                          mReverbDelayContainer = null; // The input field to get the value for the late reverberation delay time.
    private GameObject                                          mDiffusionContainer = null; // The input field to get the value for the echo density
    private GameObject                                          mDensityContainer = null; // The input field to get the value for the modal density
    private GameObject                                          mHFReferenceContainer = null; // The input field to get the value for the reference for high-frequency
    private GameObject                                          mRoomLFContainer = null; // The input field to get the value for the low-frequency room level
    private GameObject                                          mLFReferenceContainer = null; // The input field to get the value for the reference for low-frequency
    private VirtualInstrumentManager.ReverbFilterParameters     mParams; // The reverb filter parameters.

#endif 

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Use this for initialization
    new void Start()
    {

#if DEBUG && DEBUG_MUSICAL_TYPING
        base.Start();
        mParamSceneName = "ReverbFilterParametersScene";
#endif

    }

#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    //---------------------------------------------------------------------------- 
    // Private Functions
    //---------------------------------------------------------------------------- 

    protected override void SetDefaultParameters()
    {
        // Set the default parameters.
        mParams.Active = false;
        mParams.DryLevel = 0f;
        mParams.Room = -10000f;
        mParams.RoomHF = 0f;
        mParams.DecayTime = 1f;
        mParams.DecayHFRatio = .5f;
        mParams.Reflections = -10000f;
        mParams.ReflectDelay = 0.02f;
        mParams.Reverb = 0f;
        mParams.ReverbDelay = 0.04f;
        mParams.Diffusion = 100f;
        mParams.Density = 100f;
        mParams.HFReference = 5000f;
        mParams.RoomLF = 0f;
        mParams.LFReference = 250f;
    }

    // Sends the chosen parameters to the VirtualInstrumentManager
    protected override void SendParametersToVIM()
    {
        if( mEnabled )
        {
            mVIM.ModifyReverbFilter.Invoke( mParams );
        }

    }

    // Turns on the reverb filter.
    protected override void TurnOnEffect()
    {
        // Set that the filter is active.
        mEnabled = true;
        mParams.Active = true;
        
        // Change the button image.
        mToggleImage.sprite = mButtonImages[1];

        // Send the parameters to the virtual instrument manager.
        SendParametersToVIM();
    }

    // Turns off the reverb filter.
    protected override void TurnOffEffect()
    {
        // Set that the filter is off.
        mEnabled = false;
        mParams.Active = false;

        // Change the button image.
        mToggleImage.sprite = mButtonImages[0];

        // Reset the parameters object's parent and unload the scene so that the object is automatically destroyed.
        mParamObject.transform.SetParent( SceneManager.GetSceneByName( "ReverbFilterParametersScene" ).GetRootGameObjects()[0].transform );
        SceneManager.UnloadSceneAsync( mParamSceneName );

        // Reset the containers and arrays.
        mParamObject = null;
        mDryLevelContainer = null;
        mRoomContainer = null;
        mRoomHFContainer = null; 
        mDecayTimeContainer = null; 
        mDecayHFRatioContainer = null; 
        mReflectionsContainer = null; 
        mReflectDelayContainer = null; 
        mReverbContainer = null; 
        mReverbDelayContainer = null; 
        mDiffusionContainer = null; 
        mDensityContainer = null; 
        mHFReferenceContainer = null; 
        mRoomLFContainer = null; 
        mLFReferenceContainer = null;
        mTriggers = null;

    }

    protected override void HandleSceneLoad()
    {
        mTriggers = new ATI.AudioEffectParameterTrigger[14];

        mDryLevelContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 0 ).gameObject;
        mTriggers[0] = mDryLevelContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[0].SetHandler( this );
        mTriggers[0].SetType( ATI.AudioEffectParameterType.DryLevel );
        mTriggers[0].SetRange( -10000f, 0f );
        mTriggers[0].SetValue( mParams.DryLevel );

        mRoomContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 1 ).gameObject;
        mTriggers[1] = mRoomContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[1].SetHandler( this );
        mTriggers[1].SetType( ATI.AudioEffectParameterType.Room );
        mTriggers[1].SetRange( -10000f, 0f );
        mTriggers[1].SetValue( mParams.Room );

        mRoomHFContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 2 ).gameObject;
        mTriggers[2] = mRoomHFContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[2].SetHandler( this );
        mTriggers[2].SetType( ATI.AudioEffectParameterType.RoomHF );
        mTriggers[2].SetRange( -10000f, 0f );
        mTriggers[2].SetValue( mParams.RoomHF );

        mDecayTimeContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 3 ).gameObject;
        mTriggers[3] = mDecayTimeContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[3].SetHandler( this );
        mTriggers[3].SetType( ATI.AudioEffectParameterType.DecayTime );
        mTriggers[3].SetRange( 0.1f, 20f );
        mTriggers[3].SetValue( mParams.DecayTime );

        mDecayHFRatioContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 4 ).gameObject;
        mTriggers[4] = mDecayHFRatioContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[4].SetHandler( this );
        mTriggers[4].SetType( ATI.AudioEffectParameterType.DecayHFRatio );
        mTriggers[4].SetRange( 0.1f, 2f );
        mTriggers[4].SetValue( mParams.DecayHFRatio );

        mReflectionsContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 5 ).gameObject;
        mTriggers[5] = mReflectionsContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[5].SetHandler( this );
        mTriggers[5].SetType( ATI.AudioEffectParameterType.Reflections );
        mTriggers[5].SetRange( -10000f, 1000f );
        mTriggers[5].SetValue( mParams.Reflections );


        mReflectDelayContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 6 ).gameObject;
        mTriggers[6] = mReflectDelayContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[6].SetHandler( this );
        mTriggers[6].SetType( ATI.AudioEffectParameterType.ReflectDelay );
        mTriggers[6].SetRange( -10000f, 2000f );
        mTriggers[6].SetValue( mParams.ReflectDelay );


        mReverbContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 7 ).gameObject;
        mTriggers[7] = mReverbContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[7].SetHandler( this );
        mTriggers[7].SetType( ATI.AudioEffectParameterType.Reverb );
        mTriggers[7].SetRange( -10000f, 2000f );
        mTriggers[7].SetValue( mParams.Reverb );

        mReverbDelayContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 8 ).gameObject;
        mTriggers[8] = mReverbDelayContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[8].SetHandler( this );
        mTriggers[8].SetType( ATI.AudioEffectParameterType.ReverbDelay );
        mTriggers[8].SetRange( 0f, 0.1f );
        mTriggers[8].SetValue( mParams.ReverbDelay );

        mDiffusionContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 9 ).gameObject;
        mTriggers[9] = mDiffusionContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[9].SetHandler( this );
        mTriggers[9].SetType( ATI.AudioEffectParameterType.Diffusion );
        mTriggers[9].SetRange( 0f, 100f );
        mTriggers[9].SetValue( mParams.Diffusion );

        mDensityContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 10 ).gameObject;
        mTriggers[10] = mDensityContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[10].SetHandler( this );
        mTriggers[10].SetType( ATI.AudioEffectParameterType.Density );
        mTriggers[10].SetRange( 0f, 100f );
        mTriggers[10].SetValue( mParams.Density );

        mHFReferenceContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 11 ).gameObject;
        mTriggers[11] = mHFReferenceContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[11].SetHandler( this );
        mTriggers[11].SetType( ATI.AudioEffectParameterType.HFReference );
        mTriggers[11].SetRange( 20f, 20000f );
        mTriggers[11].SetValue( mParams.HFReference );

        mRoomLFContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 12 ).gameObject;
        mTriggers[12] = mRoomLFContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[12].SetHandler( this );
        mTriggers[12].SetType( ATI.AudioEffectParameterType.RoomLF );
        mTriggers[12].SetRange( -10000f, 0f );
        mTriggers[12].SetValue( mParams.RoomLF );

        mLFReferenceContainer = mParamObject.transform.GetChild( 0 ).GetChild( 0 ).GetChild( 13 ).gameObject;
        mTriggers[13] = mLFReferenceContainer.AddComponent<ATI.AudioEffectParameterTrigger>();
        mTriggers[13].SetHandler( this );
        mTriggers[13].SetType( ATI.AudioEffectParameterType.LFReference );
        mTriggers[13].SetRange( 20f, 1000f );
        mTriggers[13].SetValue( mParams.LFReference );
    }

    //---------------------------------------------------------------------------- 
    // Event Handlers
    //---------------------------------------------------------------------------- 

    protected override void HandleDryLevelChange( float aValue )
    {
        mParams.DryLevel = aValue;
        SendParametersToVIM();
    }
    protected override void HandleRoomChange( float aValue )
    {
        mParams.Room = aValue;
        SendParametersToVIM();
    }
    protected override void HandleRoomHFChange( float aValue )
    {
        mParams.RoomHF = aValue;
        SendParametersToVIM();
    }
    protected override void HandleDecayTimeChange( float aValue )
    {
        mParams.DecayTime = aValue;
        SendParametersToVIM();
    }
    protected override void HandleDecayHFRatioChange( float aValue )
    {
        mParams.DecayHFRatio = aValue;
        SendParametersToVIM();
    }
    protected override void HandleReflectionsChange( float aValue )
    {
        mParams.Reflections = aValue;
        SendParametersToVIM();
    }
    protected override void HandleReflectDelayChange( float aValue )
    {
        mParams.ReflectDelay = aValue;
        SendParametersToVIM();
    }
    protected override void HandleReverbChange( float aValue )
    {
        mParams.Reverb = aValue;
        SendParametersToVIM();
    }
    protected override void HandleReverbDelayChange( float aValue )
    {
        mParams.ReverbDelay = aValue;
        SendParametersToVIM();
    }
    protected override void HandleDiffusionChange( float aValue )
    {
        mParams.Diffusion = aValue;
        SendParametersToVIM();
    }
    protected override void HandleDensityChange( float aValue )
    {
        mParams.Density = aValue;
        SendParametersToVIM();
    }
    protected override void HandleHFReferenceChange( float aValue )
    {
        mParams.HFReference = aValue;
        SendParametersToVIM();
    }
    protected override void HandleRoomLFChange( float aValue )
    {
        mParams.RoomLF = aValue;
        SendParametersToVIM();
    }
    protected override void HandleLFReferenceChange( float aValue )
    {
        mParams.LFReference = aValue;
        SendParametersToVIM();
    }
#endif

}
