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
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ATI {

    public enum SliderType
    {
        MusicalTypingKeyVelocity,
        LowestRandomKeyVelocity,
        HighestRandomKeyVelocity,
        NoteRange,
        Uninitialized
    };

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
        public void SetHandler( SliderHandler aHandler )
        {
            mHandler = aHandler;
        }

        // Sets the slider type.
        public void SetType( SliderType aType )
        {
            mSliderType = aType;
        }

        // Handles when a slider is being dragged, but not yet released.
        public void OnDrag( PointerEventData aEventData )
        {
            if( aEventData.dragging )
            {
                mHandler.HandleDrag( mSlider, mSliderType );
            }
        }

        // Handles when a slider is finished being dragged.
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
        protected VirtualInstrumentManager       mVIM = null; // The virtual instrument manager

        //---------------------------------------------------------------------------- 
        // Unity Functions
        //---------------------------------------------------------------------------- 
        public void Start()
        {
            // Get the virtual instrument manager.
            mVIM = GameObject.Find( "Main Camera" ).GetComponent<VirtualInstrumentManager>();
        }

        //---------------------------------------------------------------------------- 
        // Public Functions
        //---------------------------------------------------------------------------- 

        // Gets the index of the slider.
        // IN: aSlider The slider to get the index for.
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
            }
        }

        //---------------------------------------------------------------------------- 
        // Pure Virtual Functions
        //---------------------------------------------------------------------------- 
        protected virtual void HandleMusicalTypingKeyVelocityChange( int aSliderIndex ) { }
        protected virtual void HandleLowestRandomVelocityChange( bool aEndDrag ) { }
        protected virtual void HandleHighestRandomVelocityChange( bool aEndDrag ) { }
        protected virtual void HandleNoteRangeChange( bool aEndDrag ) { }
    }
}
