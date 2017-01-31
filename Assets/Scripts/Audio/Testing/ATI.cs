//---------------------------------------------------------------------------- 
// /Scripts/AudioTestingInterface/ATI.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: Container for generic classes and functions that are used for 
//              objects in the AudioTestingInterface scene. 
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ATI
{

    //---------------------------------------------------------------------------- 
    // Types
    //---------------------------------------------------------------------------- 

    // The type of slider.
    public enum SliderType
    {
        MusicalTypingKeyVelocity,
        LowestRandomKeyVelocity,
        HighestRandomKeyVelocity,
        NoteRange,
        AudioEffectParameter,
        Uninitialized
    };

    // The type of audio effect parameter.
    public enum AudioEffectParameterType
    {
        Delay,
        Decay,
        DryMix,
        WetMix,
        DryLevel,
        Room,
        RoomHF,
        DecayTime,
        DecayHFRatio,
        Reflections,
        ReflectDelay,
        Reverb,
        ReverbDelay,
        Diffusion,
        Density,
        HFReference,
        RoomLF,
        LFReference,
        Uninitialized
    }

    // Class that handles when a slider in the AudioTestingInterface is dragged 
    public class SliderTrigger : MonoBehaviour,
        UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IEndDragHandler
    {
        //---------------------------------------------------------------------------- 
        // Private Variables
        //---------------------------------------------------------------------------- 
        private Slider                 mSlider; // The associated slider.
        private SliderHandler          mHandler; // The parent handler
        private SliderType             mSliderType; // The type of slider.

        //---------------------------------------------------------------------------- 
        // Unity Functions
        //---------------------------------------------------------------------------- 
        public void Awake()
        {
            mSlider = gameObject.transform.GetComponent<Slider>();
            mHandler = null;
            mSliderType = SliderType.Uninitialized;
        }

        //---------------------------------------------------------------------------- 
        // Public functions
        //---------------------------------------------------------------------------- 

        // Sets the parent handler
        // IN: aHandler The parent handler
        public void SetHandler( SliderHandler aHandler )
        {
            mHandler = aHandler;
        }

        // Sets the slider type.
        // IN: aType The slider type.
        public void SetType( SliderType aType )
        {
            mSliderType = aType;
        }

        //---------------------------------------------------------------------------- 
        // Event Handlers
        //---------------------------------------------------------------------------- 

        // Handles when a slider is being dragged, but not yet released.
        // IN: aEventData The drag event data.
        public void OnDrag( PointerEventData aEventData )
        {
            if( aEventData.dragging )
            {
                mHandler.HandleDrag( mSlider, mSliderType );
            }
        }

        // Handles when a slider is finished being dragged.
        // IN: aEventData The EndDragEvent data.
        public void OnEndDrag( PointerEventData aEventData )
        {
            mHandler.HandleDragEnd( mSlider, mSliderType );
        }
    }

    // Generic class that handles events from multiple slider triggers.
    // Meant to be inherited rather than used.
    public class SliderHandler : MonoBehaviour
    {
        //---------------------------------------------------------------------------- 
        // Protected Variables
        //---------------------------------------------------------------------------- 
        protected int                            mNumSliders; // The number of sliders that are being managed.
        protected Slider[]                       mSliders; // The sliders that are being managed.
        protected SliderTrigger[]                mSliderTriggers; // The triggers for the sliders that are being managed.
        protected VirtualInstrumentManager       mVIM = null; // The virtual instrument manager

        //---------------------------------------------------------------------------- 
        // Unity Functions
        //---------------------------------------------------------------------------- 
        public void Start()
        {
            // Get the virtual instrument manager.
            mVIM = GameObject.Find( "VirtualInstrumentManager" ).GetComponent<VirtualInstrumentManager>();
        }

        //---------------------------------------------------------------------------- 
        // Public Functions
        //---------------------------------------------------------------------------- 

        // Gets the index of the slider.
        // IN: aSlider The slider to get the index for.
        // OUT: The index of the slider.
        public int GetSliderIndex( Slider aSlider )
        {
            if( mSliders != null )
            {
                // If the slider is in the container, then return its index.
                for( int i = 0; i < mNumSliders; i++ )
                {
                    if( mSliders[i] == aSlider )
                    {
                        return i;
                    }
                }
            }

            // If the slider is not in the container, then something has gone wrong.
            else
            {
                Assert.IsTrue( false, "Slider Handler tried to handle a slider event, but the handler was uninitialized!" );
            }

            return -1;

        }

        //---------------------------------------------------------------------------- 
        // Event Handlers
        //---------------------------------------------------------------------------- 

        // Handle a drag event.
        // IN: aSlider The slider the triggered the event.
        // IN: aSliderType The type of slider that triggered the event.
        public void HandleDrag( Slider aSlider, SliderType aSliderType )
        {
            // Call the appropriate function for the slider type.
            switch( aSliderType )
            {
                case SliderType.MusicalTypingKeyVelocity:
                    HandleMusicalTypingKeyVelocityChange( GetSliderIndex( aSlider ) );
                    break;
                case SliderType.LowestRandomKeyVelocity:
                    HandleLowestRandomVelocityChange( false );
                    break;
                case SliderType.HighestRandomKeyVelocity:
                    HandleHighestRandomVelocityChange( false );
                    break;
                case SliderType.NoteRange:
                    HandleNoteRangeChange( false );
                    break;
                case SliderType.AudioEffectParameter:
                    SendSliderChangeToHandler();
                    break;
                default:
                    break;
            }
        }

        // Handles a finished drag event.
        // IN: aSlider The slider the triggered the event.
        // IN: aSliderType The type of slider that triggered the event.
        public void HandleDragEnd( Slider aSlider, SliderType aSliderType )
        {
            // Call the appropriate function for the slider type.
            switch( aSliderType )
            {
                case SliderType.MusicalTypingKeyVelocity:
                    HandleMusicalTypingKeyVelocityChange( GetSliderIndex( aSlider ) );
                    break;
                case SliderType.LowestRandomKeyVelocity:
                    HandleLowestRandomVelocityChange( true );
                    break;
                case SliderType.HighestRandomKeyVelocity:
                    HandleHighestRandomVelocityChange( true );
                    break;
                case SliderType.NoteRange:
                    HandleNoteRangeChange( true );
                    break;
                case SliderType.AudioEffectParameter:
                    SendSliderChangeToHandler();
                    break;
                default:
                    break;
            }
        }

        //---------------------------------------------------------------------------- 
        // Pure Virtual Functions
        //---------------------------------------------------------------------------- 
        protected virtual void HandleMusicalTypingKeyVelocityChange( int aSliderIndex ) { }
        protected virtual void HandleLowestRandomVelocityChange( bool aEndDrag ) { }
        protected virtual void HandleHighestRandomVelocityChange( bool aEndDrag ) { }
        protected virtual void HandleNoteRangeChange( bool aEndDrag ) { }
        protected virtual void SendSliderChangeToHandler() { }
    }

    // A class that is attached to each input field to manage sending the value appropriately.
    public class AudioEffectParameterTrigger : SliderHandler
    {
        //---------------------------------------------------------------------------- 
        // Private Variables
        //---------------------------------------------------------------------------- 
        private AudioEffectHandler               mHandler; // The parent handler.
        private AudioEffectParameterType         mType = AudioEffectParameterType.Uninitialized; // The type of input field.
        private float                            mValue = 0f; // The current value.
        private float[]                          mRange = null; // The range of possible values.
        private InputField                       mField = null; // The associated input field.

        //---------------------------------------------------------------------------- 
        // Unity Functions
        //---------------------------------------------------------------------------- 

        void Awake()
        {
            // Get the associated input field and set a handler to handle when
            // editing the field is finished.
            mField = gameObject.transform.GetChild( 0 ).GetComponent<InputField>();
            mField.onEndEdit.AddListener( SendInputFieldChangeToHandler );

            // Get the associated slider and add its trigger 
            mSliders = new Slider[1];
            mSliderTriggers = new SliderTrigger[1];
            mSliders[0] = gameObject.transform.GetChild( 1 ).GetComponent<Slider>();
            mSliderTriggers[0] = mSliders[0].gameObject.AddComponent<SliderTrigger>();

            // Set the values for the slider trigger.
            mSliderTriggers[0].SetHandler( this );
            mSliderTriggers[0].SetType( SliderType.AudioEffectParameter );

        }

        //---------------------------------------------------------------------------- 
        // Public Functions
        //---------------------------------------------------------------------------- 

        // Sets the handler for this audio effect parameter trigger.
        // IN: aHandler The parent handler.
        public void SetHandler( AudioEffectHandler aHandler )
        {
            mHandler = aHandler;
        }

        // Sets the range of possible values for this parameter trigger.
        // IN: aLowest The lowest possible value for the parameter.
        // IN: aHighest The highest possible value for the parameter.
        public void SetRange( float aLowest, float aHighest )
        {
            // Allocate the range array if needed.
            if( mRange == null )
            {
                mRange = new float[2];
            }

            // Update the member variables
            mRange[0] = aLowest;
            mRange[1] = aHighest;

            // Update the slider.
            mSliders[0].minValue = aLowest;
            mSliders[0].maxValue = aHighest;
        }

        // Sets the type of audio effect parameter for this trigger.
        // IN: aType The type of parameter.
        public void SetType( AudioEffectParameterType aType )
        {
            mType = aType;
        }

        // Sets the value of the parameter.
        // IN: aValue The new value of the parameter.
        public void SetValue( float aValue )
        {
            // Update the member variable.
            mValue = aValue;

            // Update the slider.
            mSliders[0].value = aValue;

            // Update the input field.
            mField.text = mValue.ToString( "F2" );
        }

        //---------------------------------------------------------------------------- 
        // Event Handlers
        //---------------------------------------------------------------------------- 

        // Handler to get a change in value and send it to the parent handler.
        // IN: aNewValue The new value of the input field.
        private void SendInputFieldChangeToHandler( string aNewValue )
        {
            // Get the new value as a float.
            float value = float.Parse( aNewValue );

            // Make sure that we have a range of possible values.
            if( mRange == null )
            {
                Assert.IsTrue( false, "Tried to handle a change in the input field for an audio effect, but the trigger's range was uninitialized!" );
                return;
            }

            // If the new value is within the range, then update the slider and send the change to the handler.
            if( value >= mRange[0] && value <= mRange[1] )
            {
                mValue = value;
                mSliders[0].value = value;
                mHandler.HandleAudioEffectParameterChange( mType, value );
            }

            // If the new value is not within the range, then reset it.
            else
            {
                mField.text = mValue.ToString( "F2" );
            }
        }

        // Handles a change in the slider value.
        protected override void SendSliderChangeToHandler()
        {
            // Update the member variable, input field, and send the change 
            // to the parent handler.
            mValue = mSliders[0].value;
            mField.text = mValue.ToString( "F2" );
            mHandler.HandleAudioEffectParameterChange( mType, mValue );
        }
    }

    // Handles events from an AudioEffectParameterTrigger.
    public class AudioEffectHandler : MonoBehaviour
    {
        //---------------------------------------------------------------------------- 
        // Protected Variables
        //---------------------------------------------------------------------------- 
        protected AsyncOperation                 mSceneLoadOperation = null; // The operation to load a scene to show the parameters.
        protected AudioEffectParameterTrigger[]  mTriggers = null; // The triggers for when parameters change.
        protected bool                           mEnabled = false; // Is the audio effect turned on?
        protected GameObject                     mParamObject = null; // The parameters object.
        protected Image                          mToggleImage = null; // The image component of the toggle switch.
        protected Sprite[]                       mButtonImages = null; // The images to use for toggling the effect.
        protected string                         mParamSceneName = null; // The name of the parameters scene.
        protected Toggle                         mToggleSwitch = null; // The toggle switch for this audio effect.
        protected VirtualInstrumentManager       mVIM = null; // The Virtual Instrument Manager.

        //---------------------------------------------------------------------------- 
        // Unity Functions
        //---------------------------------------------------------------------------- 

        // Initializes values.
        public void Start()
        {
            // Get the virtual instrument manager.
            mVIM = GameObject.Find( "VirtualInstrumentManager" ).GetComponent<VirtualInstrumentManager>();

            // Get the toggle switch and add its listener.
            mToggleSwitch = gameObject.transform.GetChild( 0 ).GetComponent<Toggle>();
            mToggleSwitch.onValueChanged.AddListener( OnToggle );

            // Get the toggle image.
            mToggleImage = mToggleSwitch.transform.GetChild( 0 ).GetComponent<Image>();

            // Load the images for the toggle switch.
            mButtonImages = new Sprite[2];
            mButtonImages[0] = Resources.Load<Sprite>( "Audio/Images/off_button" );
            mButtonImages[1] = Resources.Load<Sprite>( "Audio/Images/on_button" );

            // Set the default parameters.
            SetDefaultParameters();

        }

        // Called every frame.
        private void Update()
        {
            // If we are currently loading a scene, then see if it is done.
            if( mSceneLoadOperation != null && mEnabled )
            {
                // If the scene is done, then get the parameter container and set it 
                // underneath this object.
                if( mSceneLoadOperation.isDone )
                {
                    // Get the parameter container.
                    mParamObject = SceneManager.GetSceneByName( mParamSceneName ).GetRootGameObjects()[0].transform.GetChild( 0 ).gameObject;
                    mParamObject.transform.SetParent( gameObject.transform, true );

                    // Set the container's position.
                    Vector3 temp = new Vector3( 0f, 0f, 0f );
                    temp.y -= ( ( mParamObject.GetComponent<RectTransform>().rect.height * mParamObject.transform.localScale.y / 2f ) +
                        ( gameObject.GetComponent<RectTransform>().rect.height / 2f ) );
                    temp.x += ( ( mParamObject.GetComponent<RectTransform>().rect.width / 2f ) -
                        ( gameObject.GetComponent<RectTransform>().rect.width / 2f ) );
                    mParamObject.transform.localPosition = temp;

                    // Reset the scene load operation.
                    mSceneLoadOperation = null;

                    // Make the specific subclass handle the scene being loaded.
                    HandleSceneLoad();
                }
            }
        }

        //---------------------------------------------------------------------------- 
        // Event Handlers
        //---------------------------------------------------------------------------- 

        // Handles the audio effect being turned on or off.
        // IN: aEnabled Is the effect now on or off?
        public void OnToggle( bool aEnabled )
        {
            // If the effect is now on, handle turning on the effect.
            if( aEnabled )
            {
                // Sanity Check.
                if( mParamSceneName == null )
                {
                    Assert.IsTrue( false, "Turned on an uninitialized audio effect!" );
                    return;
                }

                // Make the subclass turn on the effect.
                TurnOnEffect();

                // Start loading the parameter scene.
                mSceneLoadOperation = SceneManager.LoadSceneAsync( mParamSceneName, LoadSceneMode.Additive );
            }
            // If the effect is now off, then turn off the effect.
            else
            {
                // Unload the parameter scene.
                mParamObject.transform.SetParent( SceneManager.GetSceneByName( mParamSceneName ).GetRootGameObjects()[0].transform );
                SceneManager.UnloadSceneAsync( mParamSceneName );

                // Make the subclass turn off the effect.
                TurnOffEffect();
            }
        }

        // Handles a change in an audio effect parameter
        // IN: aType The type of parameter.
        // IN: aValue The new value of the parameter.
        public void HandleAudioEffectParameterChange( AudioEffectParameterType aType, float aValue )
        {
            switch( aType )
            {
                case AudioEffectParameterType.Decay:
                    HandleDecayChange( aValue );
                    break;
                case AudioEffectParameterType.Delay:
                    HandleDelayChange( aValue );
                    break;
                case AudioEffectParameterType.DryMix:
                    HandleDryMixChange( aValue );
                    break;
                case AudioEffectParameterType.WetMix:
                    HandleWetMixChange( aValue );
                    break;
                case AudioEffectParameterType.DryLevel:
                    HandleDryLevelChange( aValue );
                    break;
                case AudioEffectParameterType.Room:
                    HandleRoomChange( aValue );
                    break;
                case AudioEffectParameterType.RoomHF:
                    HandleRoomHFChange( aValue );
                    break;
                case AudioEffectParameterType.DecayTime:
                    HandleDecayTimeChange( aValue );
                    break;
                case AudioEffectParameterType.DecayHFRatio:
                    HandleDecayHFRatioChange( aValue );
                    break;
                case AudioEffectParameterType.Reflections:
                    HandleReflectionsChange( aValue );
                    break;
                case AudioEffectParameterType.ReflectDelay:
                    HandleReflectDelayChange( aValue );
                    break;
                case AudioEffectParameterType.Reverb:
                    HandleReverbChange( aValue );
                    break;
                case AudioEffectParameterType.ReverbDelay:
                    HandleReverbDelayChange( aValue );
                    break;
                case AudioEffectParameterType.Diffusion:
                    HandleDiffusionChange( aValue );
                    break;
                case AudioEffectParameterType.Density:
                    HandleDensityChange( aValue );
                    break;
                case AudioEffectParameterType.HFReference:
                    HandleHFReferenceChange( aValue );
                    break;
                case AudioEffectParameterType.RoomLF:
                    HandleRoomLFChange( aValue );
                    break;
                case AudioEffectParameterType.LFReference:
                    HandleLFReferenceChange( aValue );
                    break;
                default:
                    break;
            }
        }

        //---------------------------------------------------------------------------- 
        // Pure Virtual Functions
        //---------------------------------------------------------------------------- 
        protected virtual void HandleDecayChange( float aValue ) { }
        protected virtual void HandleDelayChange( float aValue ) { }
        protected virtual void HandleDryMixChange( float aValue ) { }
        protected virtual void HandleWetMixChange( float aValue ) { }
        protected virtual void HandleDryLevelChange( float aValue ) { }
        protected virtual void HandleRoomChange( float aValue ) { }
        protected virtual void HandleRoomHFChange( float aValue ) { }
        protected virtual void HandleDecayTimeChange( float aValue ) { }
        protected virtual void HandleDecayHFRatioChange( float aValue ) { }
        protected virtual void HandleReflectionsChange( float aValue ) { }
        protected virtual void HandleReflectDelayChange( float aValue ) { }
        protected virtual void HandleReverbChange( float aValue ) { }
        protected virtual void HandleReverbDelayChange( float aValue ) { }
        protected virtual void HandleDiffusionChange( float aValue ) { }
        protected virtual void HandleDensityChange( float aValue ) { }
        protected virtual void HandleHFReferenceChange( float aValue ) { }
        protected virtual void HandleRoomLFChange( float aValue ) { }
        protected virtual void HandleLFReferenceChange( float aValue ) { }
        protected virtual void SetDefaultParameters() { }
        protected virtual void HandleSceneLoad() { }
        protected virtual void SendParametersToVIM() { }
        protected virtual void TurnOnEffect() { }
        protected virtual void TurnOffEffect() { }
    }
}
