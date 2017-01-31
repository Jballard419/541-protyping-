#if SONG_CREATION_ENABLED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * @class SC_PitchSelectionContainer 
 * @brief C# Class that contains the @link DocSC_PST pitch and drum selections@endlink for a @link Music::CombinedNote note@endlink in the @link DocSC Song Creation Interface@endlink.
 * 
 * Though called a \"Pitch Selection Container\", this class is also used for selecting
 * @link Music::DRUM drums@endlink. The @link Music::DRUM drums@endlink and @link Music::PITCH pitches@endlink 
 * are typecast to integer in the @link SC_PitchSelectionContainer::mSelectedPitches list of selected
 * pitches@endlink anyway, so there is not much of a difference in how each are handled by this class. 
*/
public class SC_PitchSelectionContainer : MonoBehaviour
{

    /*************************************************************************//** 
     * @defgroup SC_PSCPrivVar Private Variables
     * @ingroup DocSC_PSC
     * Variables used internally by the SC_PitchSelectionContainer
     * @{
    *****************************************************************************/
    private bool mRestNote = false; //!< Is this note currently set to be a rest note?
    private List<int> mSelectedPitches = null; //!< The selected @link Music::PITCH pitches@endlink or @link Music::DRUM drums@endlink
    private SC_PitchSelectionTrigger[] mPitchSelectionTriggers = null; //!< The @link DocSC_PST triggers@endlink that fire off an event when a @link Music::PITCH pitch@endlink or @link Music::DRUM drum@endlink is selected.
    private Toggle mRest = null; //!< The toggle switch for setting the @link Music::CombinedNote note@endlink as a @link DocSC_PSCRest rest note@endlink.
    private Toggle[] mPitches = null; //!< The toggle switches for each @link Music::PITCH pitch@endlink/@link Music::DRUM drum@endlink.

    /*************************************************************************//** 
     * @}
     * @defgroup SC_PSCPubFunc Public Functions
     * @ingroup DocSC_PSC
     * Functions for other classes to interact with the SC_PitchSelectionContainer
     * @{
    *****************************************************************************/

    /**
     * @brief Gets the currently selected @link Music::PITCH pitches@endlink/@link Music::DRUM drums@endlink.
     * @return The currently selected @link Music::PITCH pitches@endlink/@link Music::DRUM drums@endlink.
    */
    public Music.PITCH[] GetSelectedPitches()
    {
        Music.PITCH[] returned = null;

        // If we're currently a rest note, return the rest pitch
        if( mRestNote )
        {
            returned = new Music.PITCH[1];
            returned[0] = Music.PITCH.REST;
        }
        // If there aren't any pitches selected, then return null.
        else if( mSelectedPitches.Count == 0 )
        {
            return null;
        }
        // If we're not currently a rest note and some pitches are selected, then return all of the selected pitches.
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

    /**
     * @brief Resets the selected selected @link Music::PITCH pitches@endlink/@link Music::DRUM drums@endlink.
    */
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

    /**
     * @brief Sets the selected @link Music::PITCH pitches@endlink/@link Music::DRUM drums@endlink
     * @param[in] aPitches The @link Music::PITCH pitches@endlink/@link Music::DRUM drums@endlink that should be selected.
    */
    public void SetPitches( Music.PITCH[] aPitches )
    {
        // Reset the pitches.
        ResetPitches();

        // Make sure that there are pitches to set.
        if( aPitches != null )
        {
            // Set the pitches
            foreach( Music.PITCH pitch in aPitches )
            {
                if( pitch != Music.PITCH.REST )
                {
                    mPitchSelectionTriggers[(int)pitch].SetSelection( true );
                    mSelectedPitches.Add( (int)pitch );
                }
                else
                {
                    mRestNote = true;
                    mRest.isOn = true;
                }
            }
        }
    }


    /**
     * @brief Sets up this selection container as a selector for @link Music::DRUM drums@endlink.
     * 
     * The only difference is the ability to set the selected @link Music::PITCH pitch@endlink as a
     * @link DocSC_PSCRest rest note@endlink, the number of @link SC_PitchSelectionContainer::mPitches toggle switches@endlink,
     * their text, and the number of @link SC_PitchSelectionContainer::mPitchSelectionTriggers triggers@endlink.
    */
    public void SetUpAsDrumSelector()
    {
        // Create the list of selected pitches, the array of toggle switches, and the array of triggers.
        mSelectedPitches = new List<int>();
        mPitches = new Toggle[Music.MAX_SUPPORTED_DRUMS];
        mPitchSelectionTriggers = new SC_PitchSelectionTrigger[Music.MAX_SUPPORTED_DRUMS];

        // Get the first toggle switch and set its text
        mPitches[0] = gameObject.transform.GetChild( 0 ).GetComponent<Toggle>();
        mPitches[0].transform.GetChild( 1 ).GetComponent<Text>().text = "KICK_1";

        // Set up the first pitch selection trigger.
        mPitchSelectionTriggers[0] = mPitches[0].gameObject.AddComponent<SC_PitchSelectionTrigger>();
        mPitchSelectionTriggers[0].SetHandler( this );
        mPitchSelectionTriggers[0].SetIndex( 0 );
        mPitchSelectionTriggers[0].SetSelection( false );

        // Clone the first pitch object and modify it for each drum.
        for( int i = 1; i < Music.MAX_SUPPORTED_DRUMS; i++ )
        {
            // Copy the first drum and set its parent.
            mPitches[i] = Instantiate( mPitches[0] ).GetComponent<Toggle>();
            mPitches[i].transform.SetParent( gameObject.transform );

            // Set the copy's text
            mPitches[i].transform.GetChild( 1 ).GetComponent<Text>().text = Music.DrumToString( i );

            // Set up the clone's SC_PitchSelectionTrigger.
            mPitchSelectionTriggers[i] = mPitches[i].GetComponent<SC_PitchSelectionTrigger>();
            mPitchSelectionTriggers[i].SetIndex( i );
            mPitchSelectionTriggers[i].SetHandler( this );

        }

        // Make sure that everything is sized appropriately.
        mPitches[0].transform.localScale = mPitches[1].transform.localScale;
    }

    /**
     * @brief Sets up this selection container as a selector for @link Music::PITCH pitches@endlink.
     * 
     * The only difference is the ability to set the selected @link Music::PITCH pitch@endlink as a
     * @link DocSC_PSCRest rest note@endlink, the number of @link SC_PitchSelectionContainer::mPitches toggle switches@endlink,
     * their text, and the number of @link SC_PitchSelectionContainer::mPitchSelectionTriggers triggers@endlink.
    */
    public void SetUpAsPitchSelector()
    {
        // Create the list of selected pitches, the array of toggle switches, and the array of triggers.
        mSelectedPitches = new List<int>();
        mPitches = new Toggle[Music.MAX_SUPPORTED_NOTES + 1];
        mPitchSelectionTriggers = new SC_PitchSelectionTrigger[Music.MAX_SUPPORTED_NOTES + 1];

        // Get the first toggle switch and set its text
        mPitches[0] = gameObject.transform.GetChild( 0 ).GetComponent<Toggle>();
        mPitches[0].transform.GetChild( 1 ).GetComponent<Text>().text = "C0";

        // Set up the first pitch selection trigger.
        mPitchSelectionTriggers[0] = mPitches[0].gameObject.AddComponent<SC_PitchSelectionTrigger>();
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

            // Set up the clone's SC_PitchSelectionTrigger.
            mPitchSelectionTriggers[i] = mPitches[i].GetComponent<SC_PitchSelectionTrigger>();
            mPitchSelectionTriggers[i].SetIndex( i );
            mPitchSelectionTriggers[i].SetHandler( this );

        }

        // Make sure that everything is sized appropriately.
        mPitches[0].transform.localScale = mPitches[1].transform.localScale;

        // Create the rest note selection.
        mRest = Instantiate( mPitches[0] ).GetComponent<Toggle>();
        DestroyImmediate( mRest.GetComponent<SC_PitchSelectionTrigger>(), false );
        mRest.transform.SetParent( gameObject.transform );
        mRest.onValueChanged.AddListener( OnRestToggle );
        mRest.transform.GetChild( 1 ).GetComponent<Text>().text = "Rest";
        mRest.transform.localScale = mPitches[1].transform.localScale;
    }

    /*************************************************************************//** 
     * @}
     * @defgroup SC_PSCHandlers Event Handlers
     * @ingroup DocSC_PSC
     * Functions called by the SC_PitchSelectionContainer in response to events.
     * @{
    *****************************************************************************/

    /**
     * Handles @link SC_PitchSelectionTrigger::OnPitchSelected a pitch being selected@endlink.
     * @param[in] aSelected True if the @link Music::PITCH pitch@endlink/@link Music::DRUM drum@endlink is selected. False otherwise.
     * @param[in] aIndex The index of the @link DocSC_PST trigger@endlink that was selected/unselected.
    */
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

    /**
     * @brief Handles switching between this @link Music::CombinedNote note@endlink being a @link DocSC_PSCRest rest note@endlink or not.
     * @param[in] aIsRestNote True if the @link Music::CombinedNote note@endlink is a @link DocSC_PSCRest rest note@endlink. False otherwise.
    */
    public void OnRestToggle( bool aIsRestNote )
    {
        mRestNote = aIsRestNote;

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
    /** @} */
}
#endif