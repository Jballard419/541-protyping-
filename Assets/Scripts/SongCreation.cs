//---------------------------------------------------------------------------- 
// /Scripts/SongCreation.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: A script that handles all of the SongCreationInterface.
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Assertions;

public class SongCreation : MonoBehaviour {

    //---------------------------------------------------------------------------- 
    // TYPES
    //----------------------------------------------------------------------------

    //---------------------------------------------------------------------------- 
    // Class that displays a specific note of the song that is being created.
    //----------------------------------------------------------------------------
    public class NoteDisplayPanel : MonoBehaviour
    {
        //---------------------------------------------------------------------------- 
        // Private Variables 
        //----------------------------------------------------------------------------
        private Button mRemoveNoteButton = null; // The button to remove this note.
        private EventTrigger mEditTrigger = null;
        private Image mLengthImage = null; // The image to show the note's length
        private Image mOffsetImage = null; // The image to show the note's offset from the previous note.
        private int mNoteIndex = 0; // The inde of this note.
        private MeasureDisplayPanel mParent = null; // The parent container.
        private Music.NOTE_LENGTH mOffset = Music.NOTE_LENGTH.NONE;
        private SongCreation mSongCreationHandler = null;
        private Text mPitches = null; // The text that displays the note's pitches.
        private Text mVelocity = null; // The text that displays the note's velocity.



        //---------------------------------------------------------------------------- 
        // Unity Functions
        //----------------------------------------------------------------------------
        private void Awake()
        {
            // Set the private variables.
            mSongCreationHandler = GameObject.Find( "SongCreationCanvas" ).GetComponent<SongCreation>();

            // Set up the OnClick trigger.
            mEditTrigger = gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry click = new EventTrigger.Entry();
            click.eventID = EventTriggerType.PointerClick;
            click.callback.AddListener( ( data ) => { TriggerEditEvent( (PointerEventData)data ); } );
            mEditTrigger.triggers.Add( click );

            // Setup the children objects.
            mLengthImage = gameObject.transform.GetChild( 3 ).GetChild( 1 ).GetComponent<Image>();
            mOffsetImage = gameObject.transform.GetChild( 2 ).GetChild( 1 ).GetComponent<Image>();
            mPitches = gameObject.transform.GetChild( 1 ).GetChild( 0 ).GetComponent<Text>();
            mVelocity = gameObject.transform.GetChild( 4 ).GetChild( 0 ).GetComponent<Text>();
            mRemoveNoteButton = gameObject.transform.GetChild( 0 ).gameObject.GetComponent<Button>();
            mRemoveNoteButton.onClick.AddListener( OnRemoveButtonClicked );
        }    

        //---------------------------------------------------------------------------- 
        // Public Functions
        //----------------------------------------------------------------------------

        // Gets the image used to represent the length of the note.
        public Sprite GetLengthImage()
        {
            return mLengthImage.sprite;
        }

        // Gets the measure panel that is managing this panel.
        public MeasureDisplayPanel GetMeasurePanel()
        {
            return mParent;
        }

        // Gets the index for this note.
        public int GetNoteIndex()
        {
            return mNoteIndex;
        }

        // Gets the offset of this note.
        public Music.NOTE_LENGTH GetOffset()
        {
            return mOffset;
        }   

        // Sets the image used to represent the length of the note.
        public void SetLengthImage( Sprite aImage )
        {
            if( aImage == null )
            {
                mLengthImage.color = new Color32( 255, 255, 255, 0 );
            }
            else
            {
                mLengthImage.sprite = aImage;
                mLengthImage.color = new Color32( 255, 255, 255, 255 );
            }
        }

        // Sets the offset of this note.
        public void SetOffset( Music.NOTE_LENGTH aOffset )
        {
            mOffset = aOffset;
        }

        // Sets the image used to represent the offset of the note.
        public void SetOffsetImage( Sprite aImage )
        {
            if( aImage == null )
            {
                mOffsetImage.color = new Color32( 255, 255, 255, 0 );
            }
            else
            {
                mOffsetImage.sprite = aImage;
                mOffsetImage.color = new Color32( 255, 255, 255, 255 );
            }

        }

        // Sets the parent container.
        public void SetParentContainer( MeasureDisplayPanel aParent )
        {
            mParent = aParent;
        }

        // Sets the index for this note.
        public void SetNoteIndex( int aIndex )
        {
            mNoteIndex = aIndex;
        }

        // Sets the pitch display of the note.
        public void SetPitches( string aPitchString )
        {
            mPitches.text = aPitchString;
        }

        // Sets the velocity display of the note.
        public void SetVelocity( string aVelocityString )
        {
            mVelocity.text = aVelocityString;
        }

        // Sets that we're no longer editing the note that this panel represents.
        public void StopEditing()
        {
            // Change the panel background color back to its original color.
            gameObject.GetComponent<Image>().color = new Color32( 58, 58, 58, 255 );
        }

        //---------------------------------------------------------------------------- 
        // Event Handlers
        //----------------------------------------------------------------------------

        // Handles when the remove button is clicked on the note display.
        public void OnRemoveButtonClicked()
        {
            // Set the note index for all notes after this one
            int sib = transform.GetSiblingIndex() + 1;
            int index = mNoteIndex;
            while( sib < transform.parent.childCount )
            {
                if( transform.parent.GetChild( sib ).GetComponent<NoteDisplayPanel>() != null )
                {
                    transform.parent.GetChild( sib ).GetComponent<NoteDisplayPanel>().SetNoteIndex( index );
                    index++;
                }
                sib++;
            }

            // Signal that this note display should be removed.
            mParent.RemoveNote( this );

            // Signal that this note should be removed from the song.
            GameObject.Find( "SongCreationCanvas" ).GetComponent<SongCreation>().OnRemoveNote( mNoteIndex );
        }

        // Handles clicking on the background panel which means that we're either
        // beginning an edit to this note or cancelling it.
        private void TriggerEditEvent( PointerEventData data )
        {
            if( data.button == PointerEventData.InputButton.Left )
            {
                // Change the color of the background panel to show that we're editing the note that it 
                // represents.
                gameObject.GetComponent<Image>().color = new Color32( 116, 116, 116, 255 );

                // Send the event to the song creation handler.
                mSongCreationHandler.OnEditEvent( this );
            }
        }
    }

    //---------------------------------------------------------------------------- 
    // Class that displays a specific measure of the song that is being created.
    //----------------------------------------------------------------------------
    public class MeasureDisplayPanel : MonoBehaviour
    {
        //---------------------------------------------------------------------------- 
        // Private variables
        //----------------------------------------------------------------------------
        private bool mFirstOverallNote = false; // Does this measure contain the first overall note, and is it the only note?
        private float mPercentageUsed = 0f; // The percentage of the measure used.
        private List<NoteDisplayPanel> mNotes = null; // The notes in the measure.
        private NoteDisplayContainer mParent = null; // The parent container 
        private Toggle mShowMeasureToggle = null; // The toggle switch for showing this measure.

        //---------------------------------------------------------------------------- 
        // Unity Functions
        //----------------------------------------------------------------------------
        private void Awake()
        {
            // Set the toggle.
            mShowMeasureToggle = gameObject.transform.GetChild( 0 ).GetComponent<Toggle>();
            mShowMeasureToggle.onValueChanged.AddListener( OnShowToggle );

            // Set up the list of note display panels.
            mNotes = new List<NoteDisplayPanel>();
        }

        //---------------------------------------------------------------------------- 
        // Public Functions
        //----------------------------------------------------------------------------

        // Adds a note to the note display.
        public void AddNote( Music.PITCH[] aPitches, Music.NOTE_LENGTH aLength, Music.NOTE_LENGTH aOffset, int aVelocity, int aNoteIndex )
        {
            Sprite[] sprites = mParent.GetSprites();

            // Need special handling for the very first note due to having to use it as a base to copy from.
            if( mFirstOverallNote )
            {
                // Mark that we would no longer be adding the very first note.
                mFirstOverallNote = false;

                // Set the pitches.
                string pitches = "";
                foreach( Music.PITCH pitch in aPitches )
                {
                    if( pitch == Music.PITCH.REST )
                    {
                        pitches += "Rest ";
                    }
                    else
                    {
                        pitches += ( Music.NoteToString( pitch ) + " " );
                    }
                }
                mNotes[0].SetPitches( pitches );

                // Set the images.
                mNotes[0].SetLengthImage( sprites[(int)aLength] );
                mNotes[0].SetOffsetImage( null );

                // Set the offset and velocity.
                mNotes[0].SetOffset( aOffset );
                mNotes[0].SetVelocity( aVelocity.ToString() );
                mNotes[0].SetNoteIndex( aNoteIndex );
            }
            else
            {
                // If this is not the first overall note, then calculate the percentage used up in the measure.
                float newPercentage = mPercentageUsed + Music.GetNoteLengthRelativeToMeasure( aOffset, Music.TIME_SIGNATURE_4_4() );

                // If there is no more room in the measure for this note, then send it back to the parent.
                if( newPercentage >= 1f )
                {
                    mParent.HandleFullMeasure( this, newPercentage - mPercentageUsed, aPitches, aLength, aOffset, aVelocity );
                }

                // If there is more room, then create the new panel and add it to this measure.
                else
                {
                    // Update the percentage used.
                    mPercentageUsed = newPercentage;

                    // Create a new object and set it as a child. Clone the last note from the previous measure if this measure is empty.
                    GameObject clone = null;
                    if( mNotes.Count == 0 )
                    {
                        clone = Instantiate( mParent.transform.GetChild( mParent.GetCurrentMeasureObject().transform.GetSiblingIndex() - 1 ).gameObject );

                    }
                    // Clone the first note of this measure if the measure is not empty.
                    else
                    {
                        clone = Instantiate( mNotes[0].gameObject );
                    }

                    // Set the values for the note's transform.
                    clone.transform.SetParent( gameObject.transform.parent );
                    clone.transform.localScale = mParent.transform.GetChild( 1 ).localScale;

                    // Set the pitch string.
                    string pitchString = "";
                    for( int i = 0; i < aPitches.Length; i++ )
                    {
                        if( aPitches[i] == Music.PITCH.REST )
                        {
                            pitchString += "Rest ";
                        }
                        else
                        {
                            pitchString += ( Music.NoteToString( aPitches[i] ) + " " );
                        }
                    }

                    // Initialize the cloned panel.
                    NoteDisplayPanel newPanel = clone.GetComponent<NoteDisplayPanel>();
                    newPanel.SetPitches( pitchString );
                    newPanel.SetLengthImage( sprites[(int)aLength] );
                    newPanel.SetOffsetImage( sprites[(int)aOffset] );
                    newPanel.SetOffset( aOffset );
                    newPanel.SetVelocity( aVelocity.ToString() );
                    newPanel.SetParentContainer( this );
                    newPanel.SetNoteIndex( aNoteIndex );

                    // Add the panel to the list.
                    mNotes.Add( newPanel );
                }
            }
        }

        // Sets whether or not this contains the first overall note.
        public void SetIsFirstOverallNote( bool aFirstOverallNote )
        {
            mFirstOverallNote = aFirstOverallNote;
        }

        // Sets the parent container.
        public void SetParentContainer( NoteDisplayContainer aParent )
        {
            // Set the parent.
            mParent = aParent;

            // If this is the first measure, then add the existing note panel to the list.
            if( mFirstOverallNote )
            {
                mNotes.Add( gameObject.transform.parent.GetChild( 1 ).gameObject.AddComponent<NoteDisplayPanel>() );
                mNotes[0].SetParentContainer( this );
            }
        }

        // Sets the percentage of the measure that is used.
        public void SetPercentageUsed( float aPercent )
        {
            mPercentageUsed = aPercent;
        }

        // Sets whether or not the measure should be shown.
        public void SetToggle( bool aShowMeasure )
        {
            // Show/Hide the notes in the measure depending on the given bool.
            foreach( NoteDisplayPanel note in mNotes )
            {
                note.gameObject.SetActive( aShowMeasure );
            }

            // Update the toggle value.
            mShowMeasureToggle.onValueChanged.RemoveListener( OnShowToggle );
            mShowMeasureToggle.isOn = aShowMeasure;
            mShowMeasureToggle.onValueChanged.AddListener( OnShowToggle );
        }   

        //---------------------------------------------------------------------------- 
        // Event Handlers
        //----------------------------------------------------------------------------

        // Handles showing the measure or hiding it.
        public void OnShowToggle( bool aShowMeasure )
        {
            mParent.HandleMeasureToggled( this );
        }

        // Removes a note from the container. 
        public void RemoveNote( NoteDisplayPanel aNote )
        {
            // Handle the case where this is the only measure and we're removing the only note.
            if( gameObject.transform.GetChild( 0 ).GetChild( 1 ).GetComponent<Text>().text == "Measure 1" && mNotes.Count == 1 )
            {
                mFirstOverallNote = true;
            }

            // If there is more than one note in the measure, then just remove it from the measure.
            if( mNotes.Count > 1 )
            {

                // Find the note.
                bool found = false;
                int index = 0;
                while( !found && index < mNotes.Count )
                {
                    if( aNote == mNotes[index] )
                    {
                        found = true;
                    }
                    else
                    {
                        index++;
                    }
                }
                NoteDisplayPanel temp = mNotes[index];

                // Remove the note from the list.
                mNotes.RemoveAt( index );

                // Update the percentage used.
                mPercentageUsed -= Music.GetNoteLengthRelativeToMeasure( temp.GetOffset(), Music.TIME_SIGNATURE_4_4() );

                // Destroy the note object.
                DestroyImmediate( temp.gameObject, false );

            }
            // Make sure that we don't destroy the only note panel.
            else if( mFirstOverallNote )
            {
                mNotes[0].SetLengthImage( null );
                mNotes[0].SetOffset( Music.NOTE_LENGTH.NONE );
                mNotes[0].SetOffsetImage( null );
                mNotes[0].SetPitches( "" );
                mNotes[0].SetVelocity( "70" );
            }
            // If there aren't any notes left, then handle this measure being deleted.
            else
            {
                NoteDisplayPanel temp = mNotes[0];
                mNotes.Clear();
                DestroyImmediate( temp.gameObject, false );
                mParent.HandleMeasureDeleted( this );
            }
            mParent.OnRemoveNote();
        }
    }

    //---------------------------------------------------------------------------- 
    // Class that contains the measure/note displays.
    //----------------------------------------------------------------------------
    public class NoteDisplayContainer : MonoBehaviour
    {
        //---------------------------------------------------------------------------- 
        // Private Variables
        //----------------------------------------------------------------------------
        private int mNumNotes = 0; // The number of overall notes in the container.
        private int mCurrentMeasure = 0; // The current measure.
        private List<MeasureDisplayPanel> mMeasures = null; // The measures in the container.
        private Sprite[] mSprites = null; // The images to show the note lengths and offsets.

        //---------------------------------------------------------------------------- 
        // Unity Functions
        //----------------------------------------------------------------------------
        private void Awake()
        {
            // Set up the measures.
            mMeasures = new List<MeasureDisplayPanel>();
            mMeasures.Add( gameObject.transform.GetChild( 0 ).gameObject.AddComponent<MeasureDisplayPanel>() );
            mMeasures[0].SetIsFirstOverallNote( true );
            mMeasures[0].SetParentContainer( this );

            // Load the note length/offset images
            mSprites = new Sprite[13];
            mSprites[0] = Resources.Load<Sprite>( "Music/Images/32nd" );
            mSprites[1] = Resources.Load<Sprite>( "Music/Images/dotted32nd" );
            mSprites[2] = Resources.Load<Sprite>( "Music/Images/16th" );
            mSprites[3] = Resources.Load<Sprite>( "Music/Images/dotted16th" );
            mSprites[4] = Resources.Load<Sprite>( "Music/Images/eighth" );
            mSprites[5] = Resources.Load<Sprite>( "Music/Images/dottedEighth" );
            mSprites[6] = Resources.Load<Sprite>( "Music/Images/quarter" );
            mSprites[7] = Resources.Load<Sprite>( "Music/Images/dottedQuarter" );
            mSprites[8] = Resources.Load<Sprite>( "Music/Images/half" );
            mSprites[9] = Resources.Load<Sprite>( "Music/Images/dottedHalf" );
            mSprites[10] = Resources.Load<Sprite>( "Music/Images/whole" );
            mSprites[11] = Resources.Load<Sprite>( "Music/Images/dottedWhole" );
            mSprites[12] = null;
        }

        //---------------------------------------------------------------------------- 
        // Public Functions
        //----------------------------------------------------------------------------

        // Adds a note to the container
        public void AddNote( Music.PITCH[] aPitches, Music.NOTE_LENGTH aLength, Music.NOTE_LENGTH aOffset, int aVelocity )
        {     
            mMeasures[mCurrentMeasure].AddNote( aPitches, aLength, aOffset, aVelocity, mNumNotes );
            mNumNotes++;
        }

        // Gets the current measure.
        public MeasureDisplayPanel GetCurrentMeasureObject()
        {
            return mMeasures[mCurrentMeasure];
        }

        // Gets the images used to represent a note's length/offset
        public Sprite[] GetSprites()
        {
            return mSprites;
        }

        //---------------------------------------------------------------------------- 
        // Event Handlers
        //----------------------------------------------------------------------------

        // Handles when a measure becomes full.
        public void HandleFullMeasure( MeasureDisplayPanel aFullMeasure, float aSpillover, Music.PITCH[] aPitches, Music.NOTE_LENGTH aLength, Music.NOTE_LENGTH aOffset, int aVelocity )
        {
            // Create a new measure toggle.
            GameObject clone = Instantiate( mMeasures[0].gameObject );
            mMeasures.Add( clone.GetComponent<MeasureDisplayPanel>() );

            // Increase the current measure.
            mCurrentMeasure++;

            // Set the values for the new measure toggle.
            mMeasures[mCurrentMeasure].transform.GetChild( 0 ).GetChild( 1 ).GetComponent<Text>().text = "Measure " + ( mCurrentMeasure + 1 ).ToString();
            mMeasures[mCurrentMeasure].SetIsFirstOverallNote( false );
            mMeasures[mCurrentMeasure].transform.SetParent( gameObject.transform );
            mMeasures[mCurrentMeasure].SetParentContainer( this );
            mMeasures[mCurrentMeasure].transform.localScale = mMeasures[0].transform.localScale;
            mMeasures[mCurrentMeasure].SetToggle( true );

            // Handle Spillover from the previous measure.
            mMeasures[mCurrentMeasure].SetPercentageUsed( 0f - aSpillover );

            // Add the note to the new measure.
            mMeasures[mCurrentMeasure].AddNote( aPitches, aLength, aOffset, aVelocity, mNumNotes );

            // Make only the new measure be shown.
            HandleMeasureToggled( mMeasures[mCurrentMeasure] );

        }

        // Handles when a measure has all of its notes removed.
        public void HandleMeasureDeleted( MeasureDisplayPanel aMeasure )
        {
            // Find the measure that is deleted.
            int index = mMeasures.IndexOf( aMeasure );
            mCurrentMeasure--;

            // Remove the measure and show the previous one. 
            if( index >= 0 )
            {
                mMeasures.RemoveAt( index );
                mMeasures[index - 1].SetToggle( true );
            }
            else
            {
                mMeasures.RemoveAt( index );
                mMeasures[index].SetToggle( true );
            }

            // Delete the measure.
            DestroyImmediate( aMeasure.gameObject, false );
        }

        // Handles when a measure is toggled.
        public void HandleMeasureToggled( MeasureDisplayPanel aMeasure )
        {
            // Set that only the toggled measure should be shown.
            foreach( MeasureDisplayPanel measure in mMeasures )
            {
                if( aMeasure != measure )
                {
                    measure.SetToggle( false );
                }
                else
                {
                    measure.SetToggle( true );
                }
            }
        }

        // Handles when a note is removed.
        public void OnRemoveNote()
        {
            mNumNotes--;
        }
    }

    //---------------------------------------------------------------------------- 
    // Class that contains the pitch selections.
    //----------------------------------------------------------------------------
    private class PitchSelectionContainer : MonoBehaviour
    {
        //---------------------------------------------------------------------------- 
        // Nested class that uses this class as a handler.
        //---------------------------------------------------------------------------- 
        private class PitchSelectionTrigger : MonoBehaviour
        {
            //---------------------------------------------------------------------------- 
            // Private Variables
            //---------------------------------------------------------------------------- 
            private int mIndex = -1; // The index of the trigger in the parent handler
            private PitchSelectionContainer mHandler = null; // The parent handler
            private Toggle mToggle; // The associated toggle switch.

            //---------------------------------------------------------------------------- 
            // Unity Functions
            //---------------------------------------------------------------------------- 
            private void Awake()
            {
                // Get the toggle and add its listener.
                mToggle = gameObject.GetComponent<Toggle>();
                mToggle.onValueChanged.AddListener( OnPitchSelected );
            }

            //---------------------------------------------------------------------------- 
            // Public functions
            //---------------------------------------------------------------------------- 

            // Sets the parent handler.
            public void SetHandler( PitchSelectionContainer aHandler )
            {
                mHandler = aHandler;
            }

            // Sets the index.
            public void SetIndex( int aIndex )
            {
                mIndex = aIndex;
            }

            // Sets whether or not this pitch is selected.
            public void SetSelection( bool aSelected )
            {
                // Update the toggle switch.
                mToggle.onValueChanged.RemoveListener( OnPitchSelected );
                mToggle.isOn = aSelected;
                mToggle.onValueChanged.AddListener( OnPitchSelected );
            }

            //---------------------------------------------------------------------------- 
            // Event Handlers
            //---------------------------------------------------------------------------- 

            // Handles when a pitch is selected.
            public void OnPitchSelected( bool aSelected )
            {
                mHandler.HandlePitchSelection( aSelected, mIndex );
            }
        }

        //---------------------------------------------------------------------------- 
        // Private Variables
        //---------------------------------------------------------------------------- 
        private bool mRestNote = false; // Is this note currently set to be a rest note?
        private List<int> mSelectedPitches = null; // The selected pitches
        private PitchSelectionTrigger[] mPitchSelectionTriggers = null; // The triggers that fire off an event when a pitch is selected.
        private Toggle mRest = null; // The toggle switch for setting the note as a rest.
        private Toggle[] mPitches = null; // The toggle switches for each pitch

        //---------------------------------------------------------------------------- 
        // Unity Functions
        //---------------------------------------------------------------------------- 
        private void Awake()
        {
            // Create the list of selected pitches, the array of toggle switches, and the array of triggers.
            mSelectedPitches = new List<int>();
            mPitches = new Toggle[Music.MAX_SUPPORTED_NOTES + 1];
            mPitchSelectionTriggers = new PitchSelectionTrigger[Music.MAX_SUPPORTED_NOTES + 1];

            // Get the first toggle switch and set its text
            mPitches[0] = gameObject.transform.GetChild( 0 ).GetComponent<Toggle>();
            mPitches[0].transform.GetChild( 1 ).GetComponent<Text>().text = "C0";

            // Set up the first pitch selection trigger.
            mPitchSelectionTriggers[0] = mPitches[0].gameObject.AddComponent<PitchSelectionTrigger>();
            mPitchSelectionTriggers[0].SetHandler( this );
            mPitchSelectionTriggers[0].SetIndex( 0 );
            mPitchSelectionTriggers[0].SetSelection( false );

            // Clone the first pitch object and modify it for each pitch.
            string temp = null;
            for( int i = 1; i < Music.MAX_SUPPORTED_NOTES; i++ )
            {
                // Copy the first pitch and set its parent.
                mPitches[i] = Instantiate( mPitches[0] ).GetComponent<Toggle>();
                mPitches[i].transform.SetParent( gameObject.transform );

                // Set the copy's text.
                temp = Music.NoteToString( i );
                if( temp.Contains( "S" ) )
                {
                    char[] tempArray = temp.ToCharArray();
                    tempArray[1] = '#';
                    temp = new string( tempArray );
                }
                mPitches[i].transform.GetChild( 1 ).GetComponent<Text>().text = temp;

                // Set up the clone's PitchSelectionTrigger.
                mPitchSelectionTriggers[i] = mPitches[i].GetComponent<PitchSelectionTrigger>();
                mPitchSelectionTriggers[i].SetIndex( i );
                mPitchSelectionTriggers[i].SetHandler( this );

            }

            // Make sure that everything is sized appropriately.
            mPitches[0].transform.localScale = mPitches[1].transform.localScale;

            // Create the rest note selection.
            mRest = Instantiate( mPitches[0] ).GetComponent<Toggle>();
            DestroyImmediate( mRest.GetComponent<PitchSelectionTrigger>(), false );
            mRest.transform.SetParent( gameObject.transform );
            mRest.onValueChanged.AddListener( OnRestToggle );
            mRest.transform.GetChild( 1 ).GetComponent<Text>().text = "Rest";
            mRest.transform.localScale = mPitches[1].transform.localScale;
        }

        //---------------------------------------------------------------------------- 
        // Public Functions
        //---------------------------------------------------------------------------- 

        // Gets the selected pitches.
        public Music.PITCH[] GetSelectedPitches()
        {
            Music.PITCH[] returned = null;

            // If we're currently a rest note, return the rest pitch
            if( mRestNote )
            {
                returned = new Music.PITCH[1];
                returned[0] = Music.PITCH.REST;
            }
            // If we're not currently a rest note, then return all of the selected pitches.
            else
            {
                int index = 0;
                returned = new Music.PITCH[mSelectedPitches.Count];
                foreach( int pitch in mSelectedPitches )
                {
                    returned[index] = (Music.PITCH)mSelectedPitches[index];
                    index++;
                }
            }
            return returned;
        }

        // Resets the selected pitches.
        public void ResetPitches()
        {
            // Reset the child objects.
            foreach( int pitch in mSelectedPitches )
            {
                mPitchSelectionTriggers[pitch].SetSelection( false );
            }

            // Update the rest note pitch.
            mRestNote = false;


            // Clear the list.
            mSelectedPitches.Clear();
        }

        //---------------------------------------------------------------------------- 
        // Event Handlers
        //---------------------------------------------------------------------------- 

        // Handles a pitch being selected.
        // IN: aSelected Is the pitch selected or unselected?
        // IN: aIndex the index of the pitch that was selected/unselected.
        public void HandlePitchSelection( bool aSelected, int aIndex )
        {
            // If this pitch was selected and the note is note currently set to be a rest note,
            // then add it to the list.
            if( aSelected && !mRestNote )
            {
                mSelectedPitches.Add( aIndex );
                foreach( int pitch in mSelectedPitches )
                {
                    Debug.Log( Music.NoteToString( pitch ) );
                }

            }
            // If the pitch was unselected and the note is not currently set to be a rest note, 
            // then remove it from the list.
            else if( !mRestNote )
            {
                mSelectedPitches.Remove( aIndex );
                foreach( int pitch in mSelectedPitches )
                {
                    Debug.Log( Music.NoteToString( pitch ) );
                }
            }
            // If the pitch was selected and we are currently set to be a rest note, then unselect the note.
            else if( aSelected && mRestNote )
            {
                mPitchSelectionTriggers[aIndex].SetSelection( false );
            }
        }

        // Handles switching between this note being a rest note and not being a rest note.
        public void OnRestToggle( bool aOn )
        {
            mRestNote = aOn;
            Debug.Log( "Rest toggle: " + aOn );

            // If this note is now a rest note, then make sure that no other pitches are selected.
            if( mRestNote )
            {
                foreach( int pitch in mSelectedPitches )
                {
                    mPitchSelectionTriggers[pitch].SetSelection( false );
                }
            }
            // If this note is no longer a rest note, then restore the selected pitches.
            else
            {
                foreach( int pitch in mSelectedPitches )
                {
                    mPitchSelectionTriggers[pitch].SetSelection( true );
                }
            }
        }
    }

    //---------------------------------------------------------------------------- 
    // Class that handles selecting a note length/offset.
    //----------------------------------------------------------------------------
    private class SongCreationSelectionContainer : MonoBehaviour
    {
        //---------------------------------------------------------------------------- 
        // Nested class that uses this class as a handler.
        //---------------------------------------------------------------------------- 
        private class SongCreationSelectionTrigger : MonoBehaviour
        {
            //---------------------------------------------------------------------------- 
            // Private Variables
            //---------------------------------------------------------------------------- 
            private bool mSelected = false; // Whether or not this object is selected.
            private SongCreationSelectionContainer mContainer = null; // The parent container.
            private Toggle mToggle = null; // The associated toggle switch.

            //---------------------------------------------------------------------------- 
            // Unity Functions
            //---------------------------------------------------------------------------- 
            private void Awake()
            {
                // Get the toggle and set its listener
                mToggle = gameObject.GetComponent<Toggle>();
                mToggle.onValueChanged.AddListener( OnSelected );
            }

            //---------------------------------------------------------------------------- 
            // Public Functions
            //---------------------------------------------------------------------------- 

            // Sets the parent container
            public void SetContainer( SongCreationSelectionContainer aContainer )
            {
                mContainer = aContainer;
            }

            // Sets whether or not the object is selected.
            public void SetSelected( bool aSelected )
            {
                mSelected = aSelected;

                // Change the color to indicate that this is or isn't the selected object
                ChangeColor();

                // Set the value of the toggle.
                mToggle.onValueChanged.RemoveListener( OnSelected );
                mToggle.isOn = aSelected;
                mToggle.onValueChanged.AddListener( OnSelected );
            }

            //---------------------------------------------------------------------------- 
            // Private Functions
            //---------------------------------------------------------------------------- 

            // Changes color to show whether or not this object is selected.
            private void ChangeColor()
            {
                if( mSelected )
                {
                    mToggle.transform.GetChild( 0 ).GetComponent<Image>().color = new Color32( 255, 255, 255, 118 );

                }
                else
                {
                    mToggle.transform.GetChild( 0 ).GetComponent<Image>().color = new Color32( 255, 255, 255, 255 );
                }
            }

            //---------------------------------------------------------------------------- 
            // Event Handlers
            //---------------------------------------------------------------------------- 
            
            // Handles the object being selected or unselected.
            public void OnSelected( bool aSelected )
            {
                // If this object is now selected, then change the color and send it to the handler.
                if( aSelected )
                {
                    mSelected = aSelected;
                    ChangeColor();
                    mContainer.HandleToggle( this );
                }
                // This object can not be unselected by clicking on it. Set the value back to selected.
                else
                {
                    mToggle.onValueChanged.RemoveListener( OnSelected );
                    mToggle.isOn = true;
                    mToggle.onValueChanged.AddListener( OnSelected );
                }

            }
        }

        //---------------------------------------------------------------------------- 
        // Private Variables
        //----------------------------------------------------------------------------
        private int mSelectedIndex = 0; // The index of the current selection
        private SongCreationSelectionTrigger[] mTriggers = null; // The triggers that pass a selection event to this handler.
        

        //---------------------------------------------------------------------------- 
        // Unity Functions
        //----------------------------------------------------------------------------
        private void Awake()
        {
            // Set up the triggers.
            mTriggers = new SongCreationSelectionTrigger[13];
            for( int i = 0; i < 13; i++ )
            {
                mTriggers[i] = gameObject.transform.GetChild( i + 1 ).gameObject.AddComponent<SongCreationSelectionTrigger>();
                mTriggers[i].SetContainer( this );
                mTriggers[i].SetSelected( false );
            }

            // Set the default selection (Quarter note).
            mTriggers[12].SetSelected( true );
            mSelectedIndex = 12;
        }

        //---------------------------------------------------------------------------- 
        // Public Functions
        //----------------------------------------------------------------------------

        // Gets the current selection.
        public Music.NOTE_LENGTH GetSelected()
        {
            return (Music.NOTE_LENGTH)mSelectedIndex;
        }

        // Sets the current selection.
        public void SetSelected( Music.NOTE_LENGTH aSelection )
        {
            // Update the private variable.
            mSelectedIndex = (int)aSelection;

            // Update the child objects.
            for( int i = 0; i < 13; i++ )
            {
                if( i != mSelectedIndex )
                {
                    mTriggers[i].SetSelected( false );
                }
                else
                {
                    mTriggers[i].SetSelected( true );
                }
            }
        }

        //---------------------------------------------------------------------------- 
        // Event Handlers
        //----------------------------------------------------------------------------

        // Handles a new selection.
        // IN: aTrigger The trigger that is now the new selection.
        private void HandleToggle( SongCreationSelectionTrigger aTrigger )
        {
            // Set all other triggers to unselected.
            for( int i = 0; i < 13; i++ )
            {
                if( mTriggers[i] != aTrigger )
                {
                    mTriggers[i].SetSelected( false );
                }

                // Update the current selection.
                else
                {
                    mSelectedIndex = i;
                }
            }
        }
    }

    //---------------------------------------------------------------------------- 
    // Private Variables
    //----------------------------------------------------------------------------
    private bool mEditing = false; // Are we editing a note?
    private Button mATIButton = null; // The button to reload the audio testing interface.
    private Button mNewNoteButton = null; // The button to add a new note with the chosen values.
    private Button mPlaySongButton = null; // The button to play the song. 
    private Button mResetPitchesButton = null; // The button to reset the pitch selections.
    private Button mSaveSongButton = null; // The button to save the song to a file.
    private InputField mSongNameInputField = null; // The input field to name the song.
    private NoteDisplayContainer mNoteDisplay = null; // The container to show the notes for the song.
    private NoteDisplayPanel mEditPanel = null; // The panel that was selected to be edited.
    private PitchSelectionContainer mPitchSelector = null; // The container for choosing pitches.
    private Slider mBPMSlider = null; // The slider for the BPM of the song.
    private Slider mVelocitySlider = null; // The slider for the velocity of the new note.
    private Song mSong = null; // The song being created.
    private SongCreationSelectionContainer mLengthPanel = null; // The panel to choose a length for the new note.
    private SongCreationSelectionContainer mOffsetPanel = null; // The panel to choose an offset for the new note.
    private VirtualInstrumentManager mVIM = null; // The virtual instrument manager

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //----------------------------------------------------------------------------
    void Awake()
    {
        // Initialize the song.
        mSong = new Song();

        // Get the virtual instrument manager.
        mVIM = GameObject.Find( "VirtualInstrumentManager" ).GetComponent<VirtualInstrumentManager>();

        // Set up the container for showing the song's notes.
        mNoteDisplay = gameObject.transform.GetChild( 1 ).GetChild( 0 ).GetChild( 0 ).gameObject.AddComponent<NoteDisplayContainer>();

        // Set up the container for selecting pitches.
        mPitchSelector = gameObject.transform.GetChild( 2 ).GetChild( 0 ).GetChild( 0 ).gameObject.AddComponent<PitchSelectionContainer>();

        // Set up the panels for choosing a note's length and offset.
        mLengthPanel = gameObject.transform.GetChild( 3 ).gameObject.AddComponent<SongCreationSelectionContainer>();
        mOffsetPanel = gameObject.transform.GetChild( 4 ).gameObject.AddComponent<SongCreationSelectionContainer>();

        // Set up the slider for the note's velocity.
        mVelocitySlider = gameObject.transform.GetChild( 5 ).GetChild( 0 ).GetComponent<Slider>();
        mVelocitySlider.onValueChanged.AddListener( OnVelocityChange );

        // Set up the slider for the song's default BPM.
        mBPMSlider = gameObject.transform.GetChild( 5 ).GetChild( 1 ).GetComponent<Slider>();
        mBPMSlider.onValueChanged.AddListener( OnBPMChange );

        // Set up the input field for naming the song.        
        mSongNameInputField = gameObject.transform.GetChild( 6 ).gameObject.GetComponent<InputField>();
        mSongNameInputField.onEndEdit.AddListener( mSong.SetName );

        // Set up the buttons.
        mNewNoteButton = gameObject.transform.GetChild( 7 ).gameObject.GetComponent<Button>();
        mNewNoteButton.onClick.AddListener( OnCreateNote );
        mResetPitchesButton = gameObject.transform.GetChild( 8 ).GetComponent<Button>();
        mResetPitchesButton.onClick.AddListener( mPitchSelector.ResetPitches );
        mSaveSongButton = gameObject.transform.GetChild( 9 ).gameObject.GetComponent<Button>();
        mSaveSongButton.onClick.AddListener( OnSaveSong );
        mPlaySongButton = gameObject.transform.GetChild( 10 ).GetComponent<Button>();
        mPlaySongButton.onClick.AddListener( OnPlaySong );
        mATIButton = gameObject.transform.GetChild( 11 ).GetComponent<Button>();
        mATIButton.onClick.AddListener( UnloadSongCreationInterface );
    }

    //---------------------------------------------------------------------------- 
    // Event Handlers
    //----------------------------------------------------------------------------

    // Handles a change in the song's default BPM.
    // IN: aBPM The new default BPM.
    public void OnBPMChange( float aBPM )
    {
        // Update the BPM slider's label.
        mBPMSlider.transform.GetChild( 3 ).GetComponent<Text>().text = "Default BPM: " + aBPM.ToString();

        // Update the song.
        mSong.SetBPM( (int)aBPM );
    }

    // Creates a new note in the song.
    public void OnCreateNote()
    {
        // Sanity check.
        if( mEditing )
        {
            Assert.IsFalse( !mEditing, "Tried to create a note in the song creation interface when we should have been editing one!" );
            return;
        }

        // Get the selected pitches
        Music.PITCH[] pitches = mPitchSelector.GetSelectedPitches();

        // Make sure that some pitches are actually selected.
        if( pitches.Length != 0 )
        {
            // Get the length, offset, and velocity.
            Music.NOTE_LENGTH length = mLengthPanel.GetSelected();
            Music.NOTE_LENGTH offset = mOffsetPanel.GetSelected();
            int velocity = (int)mVelocitySlider.value;

            // If the song doesn't have any notes, then set the offset of the first note to none.
            if( mSong.GetNumNotes() == 0 )
            {
                offset = Music.NOTE_LENGTH.NONE;
            }

            // Add the note to the song.
            mSong.AddNote( velocity, length, offset, pitches );

            // Add the note to the note display.
            mNoteDisplay.AddNote( pitches, length, offset, velocity );

            // Reset the pitches.
            mPitchSelector.ResetPitches();

            // Update the selection of the offset panel since most cases will use the previous length.
            mOffsetPanel.SetSelected( mLengthPanel.GetSelected() );
        }
    }

    // Handles when a note panel is selected for editing.
    // IN: aPanel The panel representing the note that we're editing.
    public void OnEditEvent( NoteDisplayPanel aPanel )
    {
        // Handle the case where we begin editing a note.
        if( mEditPanel == null )
        {
            // Update the new note button to be a modify note button.
            mNewNoteButton.transform.GetChild( 0 ).GetComponent<Text>().text = "Modify Note";
            mNewNoteButton.onClick.RemoveListener( OnCreateNote );
            mNewNoteButton.onClick.AddListener( OnModifyNote );

            // Set the variables related to editing a note.
            mEditPanel = aPanel;
            mEditing = true;
        }
        // Handle the case where a note/note panel is already being edited, 
        // but a different one calls this event.
        else if( mEditPanel != aPanel )
        {
            // Stop editing the current note/note panel.
            mEditPanel.StopEditing();

            // Change the note/note panel being edited to the new one.
            mEditPanel = aPanel;
        }
        // Handle the case where we're editing a note/note panel and it calls this 
        // event while it's being edited. This means that the edit should be cancelled.
        else
        {
            // Change the new note button back to its original state.
            mNewNoteButton.transform.GetChild( 0 ).GetComponent<Text>().text = "Insert Note";
            mNewNoteButton.onClick.RemoveListener( OnModifyNote );
            mNewNoteButton.onClick.AddListener( OnCreateNote );

            // Mark that the panel is no longer being edited.
            mEditPanel.StopEditing();

            // Reset the variables related to editing a note.
            mEditPanel = null;
            mEditing = false;
        }
    }

    // Modifies a note in the song.
    public void OnModifyNote()
    {
        // Make sure that we're actually editing a note.
        if( mEditing )
        {
            // Sanity check.
            if( mEditPanel == null )
            {
                Assert.IsNotNull( mEditPanel, "Tried to modify a note, but the note panel in the song creator was null!" );
                return;
            }

            // Get the pitches 
            Music.PITCH[] pitches = mPitchSelector.GetSelectedPitches();

            // Make sure that some pitches are actually selected.
            if( pitches.Length != 0 )
            {
                // Get the index of the note in the song and the images for the length/offset.
                int index = mEditPanel.GetNoteIndex();
                Sprite[] images = mNoteDisplay.GetSprites();

                // Create a new note struct.
                Music.Note noteReplacement;

                // Set the values of the note struct.
                noteReplacement.Pitches = pitches;
                noteReplacement.Length = mLengthPanel.GetSelected();
                noteReplacement.Offset = mOffsetPanel.GetSelected();
                noteReplacement.Velocity = (int)mVelocitySlider.value;

                // Replace the note in the song with the modified note.
                mSong.ReplaceNote( noteReplacement, index );

                // Get the string of pitches for the modified note.
                string pitchString = "";
                for( int i = 0; i < pitches.Length; i++ )
                {
                    if( pitches[i] == Music.PITCH.REST )
                    {
                        pitchString += "Rest ";
                    }
                    else
                    {
                        pitchString += ( Music.NoteToString( pitches[i] ) + " " );
                    }
                }

                // Update the panel representing the note.
                mEditPanel.SetPitches( pitchString );
                mEditPanel.SetLengthImage( images[(int)noteReplacement.Length] );
                mEditPanel.SetOffsetImage( images[(int)noteReplacement.Offset] );
                mEditPanel.SetOffset( noteReplacement.Offset );
                mEditPanel.SetVelocity( noteReplacement.Velocity.ToString() );

                // Mark that the panel is no longer being edited.
                mEditPanel.StopEditing();

                // Update the new note button so that it shows that we're no longer editing a note.
                mNewNoteButton.transform.GetChild( 0 ).GetComponent<Text>().text = "Insert Note";
                mNewNoteButton.onClick.RemoveListener( OnModifyNote );
                mNewNoteButton.onClick.AddListener( OnCreateNote );

                // Reset the variables related to editing a note.
                mEditPanel = null;
                mEditing = false;

                // Reset the pitch selections.
                mPitchSelector.ResetPitches();
            }
        }            
    }

    // Play the song being created.
    public void OnPlaySong()
    {
        mVIM.PlaySong.Invoke( mSong );
    }

    // Removes a note from the song being created.
    public void OnRemoveNote( int aIndex )
    {
        if( mSong.GetNumNotes() > 0 )
        {
            mSong.RemoveNote( aIndex );
        }
    }

    // Save the song to file.
    public void OnSaveSong()
    {
        mVIM.SongManager.AddSong( mSong );
        mSong.WriteSongToFile();   
    }

    // Handles a change in the velocity for a note.
    // IN: aVelocity The new velocity.
    public void OnVelocityChange( float aVelocity )
    {
        // Update the text of the velocity slider's label.
        mVelocitySlider.transform.GetChild( 3 ).GetComponent<Text>().text = "Velocity: " + aVelocity.ToString();
    }

    // Goes back to the AudioTestingInterface scene.
    public void UnloadSongCreationInterface()
    {
        SceneManager.UnloadSceneAsync( "SongCreationInterface" );
    }
  
}
