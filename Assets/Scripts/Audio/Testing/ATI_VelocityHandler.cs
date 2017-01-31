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

public class ATI_VelocityHandler : ATI.SliderHandler
{

#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Constants
    //---------------------------------------------------------------------------- 
    private string[] keys =
        { "a: ", "w: ", "s: ", "e: ", "d: ", "f: ", "t: ", "g: ", "y: ", "h: ", "u: ", "j: ", "k: ", "o: ", "l: ", "p: ", ";: ", "': ", "]: " };

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private ATI.SliderTrigger          mHighestRandomSliderTrigger = null; // The trigger for the slider that handles the highest random velocity.
    private ATI.SliderTrigger          mLowestRandomSliderTrigger = null; // The trigger for the slider that handles the lowest random velocity.
    private bool                       mRandomize = false; // Should the musical typing velocities be randomized?
    private Slider                     mLowestRandomSlider = null; // The slider that handles the lowest random velocity.
    private Slider                     mHighestRandomSlider = null; // The slider that handles the highest random velocity.
    private Sprite[]                   mSprites = null; // The images for mRandomToggle
    private Text[]                     mText = null; // The text objects for each slider.
    private Toggle                     mRandomToggle = null; // The toggle switch for randomizing the musical typing velocities.

#endif

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 

    // Use this for initialization
    new void Start()
    {

#if DEBUG && DEBUG_MUSICAL_TYPING

        // Call the ATI.SliderHandler start function.
        base.Start();

        // Set the images for the randomize musical typing velocities switch.
        mSprites = new Sprite[2];
        mSprites[0] = Resources.Load<Sprite>( "Audio/Images/off_button" );
        mSprites[1] = Resources.Load<Sprite>( "Audio/Images/on_button" );

        // Set the toggle switch for randomizing musical typing velocities
        mRandomToggle = gameObject.transform.GetChild( 0 ).GetComponent<Toggle>();
        mRandomToggle.onValueChanged.AddListener( OnRandomizeVelocitySwitch );

        // Set the lowest random value slider.
        mLowestRandomSlider = gameObject.transform.GetChild( 1 ).GetComponent<Slider>();
        mLowestRandomSliderTrigger = mLowestRandomSlider.gameObject.AddComponent<ATI.SliderTrigger>();
        mLowestRandomSliderTrigger.SetHandler( this );
        mLowestRandomSliderTrigger.SetType( ATI.SliderType.LowestRandomKeyVelocity );

        // Set the highest random value slider.
        mHighestRandomSlider = gameObject.transform.GetChild( 2 ).GetComponent<Slider>();
        mHighestRandomSliderTrigger = mHighestRandomSlider.gameObject.AddComponent<ATI.SliderTrigger>();
        mHighestRandomSliderTrigger.SetHandler( this );
        mHighestRandomSliderTrigger.SetType( ATI.SliderType.HighestRandomKeyVelocity );

        // Initialize the sliders, their text, and their triggers.
        mNumSliders = 19;
        mSliders = new Slider[mNumSliders];
        mText = new Text[mNumSliders];
        mSliderTriggers = new ATI.SliderTrigger[mNumSliders];

        for( int i = 0; i < mNumSliders; i++ )
        {
            // For each slider, put it in the array and add the slider trigger.
            mSliders[i] = gameObject.transform.GetChild( i + 3 ).GetComponent<Slider>();
            mSliderTriggers[i] = mSliders[i].gameObject.AddComponent<ATI.SliderTrigger>();
            mSliderTriggers[i].SetType( ATI.SliderType.MusicalTypingKeyVelocity );
            mSliderTriggers[i].SetHandler( this );

            // For each text, put it in the array and set its value
            mText[i] = mSliders[i].gameObject.transform.GetChild( 4 ).GetComponent<Text>();
            mText[i].text = keys[i] + "100";
        }
#endif

    }


    // Update is called once per frame
    void Update()
    {

    }

#if DEBUG && DEBUG_MUSICAL_TYPING

    //---------------------------------------------------------------------------- 
    // Private Functions
    //---------------------------------------------------------------------------- 

    // Randomizes the musical typing key velocities.
    private void RandomizeKeyVelocities()
    {
        mRandomize = mRandomToggle.enabled;
        mVIM.GetMusicalTypingHandler().RandomizeVelocities = mRandomize;
        mVIM.GetMusicalTypingHandler().SetRandomVelocityRange( (int)mLowestRandomSlider.value, (int)mHighestRandomSlider.value );
    }

    //---------------------------------------------------------------------------- 
    // Event Handlers 
    //---------------------------------------------------------------------------- 

    // Handles a change in the highest random velocity.
    // IN: aEndDrag Has the slider finished changing values?
    protected override void HandleHighestRandomVelocityChange( bool aEndDrag )
    {
        // Set the text and update the lowest random velocity slider's max value.
        mHighestRandomSlider.transform.GetChild( 4 ).GetComponent<Text>().text = "Highest: " + mHighestRandomSlider.value.ToString();
        mLowestRandomSlider.maxValue = mHighestRandomSlider.value;

        // If the value is set, then randomize the velocities for each key.
        if( aEndDrag && mRandomize )
        {
            RandomizeKeyVelocities();
        }
    }

    // Handles a change in the lowest random velocity. 
    // IN: aEndDrag Has the slider finished changing values?
    protected override void HandleLowestRandomVelocityChange( bool aEndDrag )
    {
        // Set the text and update the highest random velocity slider's min value.
        mLowestRandomSlider.transform.GetChild( 4 ).GetComponent<Text>().text = "Lowest: " + mLowestRandomSlider.value.ToString();
        mHighestRandomSlider.minValue = mLowestRandomSlider.value;

        // If the value is set, then randomize the velocities for each key.
        if( aEndDrag && mRandomize )
        {
            RandomizeKeyVelocities();
        }
    }

    // Invokes the virtual instrument manager's ChangeKeyVelocityEvent.
    // IN: aSliderIndex the index of the slider that triggered the event.
    protected override void HandleMusicalTypingKeyVelocityChange( int aSliderIndex )
    {
        // Change the text and pass the value to the virtual instrument manager.
        mText[aSliderIndex].text = keys[aSliderIndex] + mSliders[aSliderIndex].value.ToString();
        mVIM.GetMusicalTypingHandler().SetKeyVelocity( aSliderIndex, (int)mSliders[aSliderIndex].value );
    }

    // Handles when the switch for randomizing musical typing key velocities is toggled.
    // IN: aSwitch Is the switch now on or off?
    public void OnRandomizeVelocitySwitch( bool aSwitch )
    {
        if( aSwitch )
        {
            // If the switch has been turned on, update the image and text and randomize the musical typing
            // key velocities.
            mRandomize = true;
            mRandomToggle.transform.GetChild( 1 ).GetComponent<Image>().sprite = mSprites[1];
            mRandomToggle.transform.GetChild( 2 ).GetComponent<Text>().text = "Randomize Velocities:\nOn";
            RandomizeKeyVelocities();
        }
        else
        {
            // If the switch has been turned off, then update the image and the text.
            mRandomToggle.transform.GetChild( 1 ).GetComponent<Image>().sprite = mSprites[0];
            mRandomToggle.transform.GetChild( 2 ).GetComponent<Text>().text = "Randomize Velocities:\nOff";
            mRandomize = false;
        }
    }

#endif

}
