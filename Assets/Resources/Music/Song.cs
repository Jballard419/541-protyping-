//---------------------------------------------------------------------------- 
// /Resources/Music/Song.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: A class that represents a song.
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.IO;

public class Song
{
    //---------------------------------------------------------------------------- 
    // Types
    //---------------------------------------------------------------------------- 

    // A struct for note data which is used as a bridge between the general music objects and the audio data.
    public struct NoteData
    {
        public int NumSamples;
        public int TotalOffset;
        public int Velocity;
        public Music.PITCH[] Pitches;
    }

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private int mBPM = 120; // The default BPM of the song.
    private List<Music.Note> mNotes = null; // The notes of the song.
    private Music.TimeSignature mTimeSignature; // The time signature of the song.
    private string mName = null; // The name of the song.

    //---------------------------------------------------------------------------- 
    // Constructors
    //---------------------------------------------------------------------------- 

    // The default constructor 
    public Song()
    {
        // Set the member variables.
        mNotes = new List<Music.Note>();
        mTimeSignature = Music.TIME_SIGNATURE_4_4();
        mBPM = 120;
        mName = "Untitled";
    }

    // A constructor to create a song from a file.
    public Song( string aSongFilePath )
    {
        mNotes = new List<Music.Note>();
        CreateSongFromFile( aSongFilePath );
    }

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Adds a note to the song.
    public void AddNote( int aVelocity, Music.NOTE_LENGTH aLength, Music.NOTE_LENGTH aOffset, Music.PITCH[] aPitches )
    {
        // Create the note struct.
        Music.Note added;
        added.Velocity = aVelocity;
        added.Length = aLength;
        added.Offset = aOffset;
        added.Pitches = aPitches;

        // Add the note to the list.
        mNotes.Add( added );
    }

    // Gets the BPM of the song.
    public int GetBPM()
    {
        return mBPM;
    }

    // Gets the name of the song.
    public string GetName()
    {
        return mName;
    }

    // Converts the notes in the song to NoteData structs.
    public NoteData[] GetNoteData()
    {
        // Allocate the returned array.
        NoteData[] returned = new NoteData[mNotes.Count];

        // Set up variables to keep track of a note's number of samples and total offset.
        int offset = 0;
        int numSamp = 0;

        // Iterate through each note in the list.
        int index = 0;
        foreach( Music.Note note in mNotes )
        {
            // Get the number of samples and the total offset for the note.
            numSamp = GetNoteLengthInSamples( mBPM, 44100, note.Length, mTimeSignature );
            offset += GetNoteLengthInSamples( mBPM, 44100, note.Offset, mTimeSignature );

            // Set the values for the NoteData struct.
            returned[index].NumSamples = numSamp;
            returned[index].Pitches = note.Pitches;
            returned[index].TotalOffset = offset;
            returned[index].Velocity = note.Velocity;

            index++;
        }

        return returned;
    }

    // Returns the notes in the song.
    public List<Music.Note> GetNotes()
    {
        return mNotes;
    }

    // Gets the number of notes.
    public int GetNumNotes()
    {
        return mNotes.Count;
    }

    // Removes a note at a specific index from the song.
    public void RemoveNote( int aIndex )
    {
        if( aIndex < mNotes.Count )
        {
            mNotes.RemoveAt( aIndex );
        }
        else
        {
            Assert.IsTrue( false, "Tried to remove a note that didn't exist!" );
        }

    }

    // Replaces a note at a specific index.
    public void ReplaceNote( Music.Note aNote, int aIndex )
    {
        if( aIndex < mNotes.Count )
        {
            mNotes[aIndex] = aNote;
        }
        else
        {
            Assert.IsTrue( false, "Tried to replace a note that didn't exist!" );
        }
    }

    // Sets the BPM of the song.
    public void SetBPM( int aBPM )
    {
        mBPM = aBPM;
    }

    // Sets the BPM of a song.
    public void SetBPM( float aBPM )
    {
        mBPM = (int)aBPM;
    }

    // Sets the name of the song.
    public void SetName( string aSongName )
    {
        mName = aSongName;
    }

    // Sets the notes of the song.
    public void SetNotes( List<Music.Note> aNotes )
    {
        mNotes = aNotes;
    }

    // Sets the time signature of the song.
    public void SetTimeSignature( Music.TimeSignature aTimeSignature )
    {
        mTimeSignature.BeatsPerMeasure = aTimeSignature.BeatsPerMeasure;
        mTimeSignature.BaseBeat = aTimeSignature.BaseBeat;
    }

    // Writes the song to a file. The filename will be "SONG_[mName]" and it will be stored in Resources/Music/Songs/
    // The format of the file is:
    //     First line: "Name of song"
    //     Second line: "defaultBPM;TimeSignature.BeatsPerMeasure;TimeSignature.BaseBeat"
    //     Rest of lines: "pitch1,pitch2,...,pitchN;NoteLength;OffsetFromPreviousNote;Velocity"
    // 
    // note: The values from Music enums are typecast to integers for saving. Ex: Music.PITCH.C4 is saved as 48. 
    public void WriteSongToFile()
    {
        // Set up the filepath.
        string filepath = Application.dataPath + "/Resources/Music/Songs/SONG_" + mName;

        // Set an array for the contents of the file
        string[] contents = new string[mNotes.Count + 2];

        // Put the name of the song in first line to be saved.
        contents[0] = mName;

        // Put the default BPM and the timesignature values in the second line to be saved.
        contents[1] = mBPM.ToString() + ";" + mTimeSignature.BeatsPerMeasure.ToString() + ";" +
            ( (int)mTimeSignature.BaseBeat ).ToString();

        // Put all the notes into the lines from line 3 to line [numNotes].  
        for( int i = 0; i < mNotes.Count; i++ )
        {
            // Set up the note string.
            contents[i + 2] = "";

            // Add the pitches to the string
            for( int j = 0; j < mNotes[i].Pitches.Length; j++ )
            {
                contents[i + 2] += ( (int)mNotes[i].Pitches[j] ).ToString();

                // If this is not the last pitch of the note, then add a comma
                if( j + 1 != mNotes[i].Pitches.Length )
                {
                    contents[i + 2] += ",";
                }
                // If this is the last pitch of the note, then add a semicolon.
                else
                {
                    contents[i + 2] += ";";
                }
            }

            // Add the length, offset, and velocity to the string
            contents[i + 2] += ( ( (int)mNotes[i].Length ).ToString() + ";" );
            contents[i + 2] += ( ( (int)mNotes[i].Offset ).ToString() + ";" );
            contents[i + 2] += mNotes[i].Velocity.ToString();
        }

        // Write the contents to the file.
        System.IO.File.WriteAllLines( filepath, contents );
    }

    //---------------------------------------------------------------------------- 
    // Private Functions
    //---------------------------------------------------------------------------- 

    // Creates a song from a file.
    private void CreateSongFromFile( string aSongFilePath )
    {
        // Open the file stream.
        StreamReader parser = new StreamReader( aSongFilePath );

        // Get the name of the song
        string curLine = parser.ReadLine();
        mName = curLine;

        // Get the default BPM 
        curLine = parser.ReadLine();
        string[] splitLine = curLine.Split( ';' );
        mBPM = int.Parse( splitLine[0] );

        // Get the time signature
        mTimeSignature.BeatsPerMeasure = int.Parse( splitLine[1] );
        mTimeSignature.BaseBeat = (Music.NOTE_LENGTH)int.Parse( splitLine[2] );

        // Get the notes of the song.
        curLine = parser.ReadLine();
        while( curLine != null )
        {
            splitLine = curLine.Split( ';' );

            // Get the pitches of the note
            string[] pitches = splitLine[0].Split( ',' );
            Music.PITCH[] readPitches = new Music.PITCH[pitches.Length];
            for( int i = 0; i < pitches.Length; i++ )
            {
                readPitches[i] = (Music.PITCH)int.Parse( pitches[i] );
            }

            // Get the note length and the offset from the previous note.
            Music.NOTE_LENGTH length = (Music.NOTE_LENGTH)int.Parse( splitLine[1] );
            Music.NOTE_LENGTH offset = (Music.NOTE_LENGTH)int.Parse( splitLine[2] );

            // Get the velocity
            int velocity = int.Parse( splitLine[3] );

            // Add the note.
            AddNote( velocity, length, offset, readPitches );

            // Get the next line
            curLine = parser.ReadLine();
        }
    }

    //---------------------------------------------------------------------------- 
    // Static Functions
    //---------------------------------------------------------------------------- 

    // Converts the given note length to a number of samples adjusted for BPM and time signature.
    public static int GetNoteLengthInSamples( int aBPM, int aSampleRate, Music.NOTE_LENGTH aNoteLength, Music.TimeSignature aTimeSignature )
    {
        // Initialize variables for calculating the note length.
        float beatsPerSecond = (float)aBPM / 60f;

        // Since the audio data is split into two channels with even indices for the data 
        // in the left channel and odd indices for the data in the right channel, the
        // actual conversion for number of samples per second is 2 * sample rate.
        float numSamplesPerBeat = 2f * ( 1f / beatsPerSecond ) * (float)aSampleRate;

        // Calculate the length for various time signatures by relating the base beat to 
        // a quarter note. 
        float numQuarterNotesPerBeat = 1f;

        // Right now, only provide support for base beats of 4 and 8.
        switch( aTimeSignature.BaseBeat )
        {
            case Music.NOTE_LENGTH.E:
                numQuarterNotesPerBeat /= 2f;
                break;
            default:
                break;
        }

        // Calculate the note length in samples.
        switch( aNoteLength )
        {
            case Music.NOTE_LENGTH.T:
                return (int)( numSamplesPerBeat / ( 8f * numQuarterNotesPerBeat ) );
            case Music.NOTE_LENGTH.D_T:
                return (int)( 1.5f * numSamplesPerBeat / ( 8f * numQuarterNotesPerBeat ) );
            case Music.NOTE_LENGTH.S:
                return (int)( numSamplesPerBeat / ( 4f * numQuarterNotesPerBeat ) );
            case Music.NOTE_LENGTH.D_S:
                return (int)( 1.5f * numSamplesPerBeat / ( 4f * numQuarterNotesPerBeat ) );
            case Music.NOTE_LENGTH.E:
                return (int)( numSamplesPerBeat / ( 2f * numQuarterNotesPerBeat ) );
            case Music.NOTE_LENGTH.D_E:
                return (int)( 1.5f * numSamplesPerBeat / ( ( 4f / 3f ) * numQuarterNotesPerBeat ) );
            case Music.NOTE_LENGTH.Q:
                return (int)( numSamplesPerBeat / numQuarterNotesPerBeat );
            case Music.NOTE_LENGTH.D_Q:
                return (int)( numSamplesPerBeat * ( 1.5f / numQuarterNotesPerBeat ) );
            case Music.NOTE_LENGTH.H:
                return (int)( numSamplesPerBeat * ( 2f / numQuarterNotesPerBeat ) );
            case Music.NOTE_LENGTH.D_H:
                return (int)( numSamplesPerBeat * ( 3f / numQuarterNotesPerBeat ) );
            case Music.NOTE_LENGTH.W:
                return (int)( numSamplesPerBeat * ( 4f / numQuarterNotesPerBeat ) );
            case Music.NOTE_LENGTH.D_W:
                return (int)( numSamplesPerBeat * ( 6f / numQuarterNotesPerBeat ) );
            default:
                return 0;
        }
    }
}
