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

    // The possible types of songs.
    public enum SongType
    {
        Empty,
        Melody,
        DrumLoop,
        CombinedMelodyAndPercussion
    }

    // A struct for drum data which is used as a bridge between the abstract drum objects and the audio data.
    public struct PercussionData
    {
        public bool HasHiHat;
        public int Velocity;
        public Music.DRUM[] Hits;
    }

    // A struct for musical note data which is used as a bridge between the abstract musical note and the audio data.
    public struct MelodyNoteData
    {
        public int NumSamples;
        public int Velocity;
        public Music.PITCH[] Pitches;
    }

    // A struct to contain data for both the drums and the melody.
    public struct CombinedNoteData
    {
        public int TotalOffset;
        public MelodyNoteData MelodyData;
        public PercussionData PercussionData;
    }

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private int mBPM = 120; // The default BPM of the song.
    private int mNumDrumNotes; // The number of notes that have drums.
    private int mNumMusicalNotes; // The number of notes that have pitches.
    private List<Music.CombinedNote> mNotes = null; // The notes of the song.
    private Music.TimeSignature mTimeSignature; // The time signature of the song.
    private SongType mType = SongType.Empty; // The type of song.
    private string mName = null; // The name of the song.

    //---------------------------------------------------------------------------- 
    // Constructors
    //---------------------------------------------------------------------------- 

    // The default constructor 
    public Song()
    {
        // Set the member variables.
        mNotes = new List<Music.CombinedNote>();
        mTimeSignature = Music.TIME_SIGNATURE_4_4();
        mBPM = 120;
        mName = "Untitled";
    }

    // A constructor to create a song from a file.
    public Song( string aSongFilePath )
    {
        mNotes = new List<Music.CombinedNote>();
        CreateSongFromFile( aSongFilePath );
    }

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Adds a note to the song.
    public void AddNote( Music.CombinedNote aNewNote )
    {
        // Update the number of musical notes in the song if needed.
        if( aNewNote.MusicalNote.Pitches != null )
        {
            mNumMusicalNotes++;
        }

        // Update the number of notes with drum hits in the song if needed.
        if( aNewNote.Drums.Hits != null )
        {
            mNumDrumNotes++;
        }

        // Add the note to the list.
        mNotes.Add( aNewNote );

        // Update the song type.
        UpdateSongType();
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
    public CombinedNoteData[] GetNoteData()
    {
        // Allocate the returned array.
        CombinedNoteData[] returned = new CombinedNoteData[mNotes.Count];

        // Set up variables to keep track of a note's number of samples and total offset.
        int offset = 0;
        int numSamp = 0;

        // Iterate through each note in the list.
        int index = 0;
        foreach( Music.CombinedNote note in mNotes )
        {
            // Get the number of samples and the total offset for the note.
            numSamp = GetNoteLengthInSamples( mBPM, 44100, note.MusicalNote.Length, mTimeSignature );
            offset += GetNoteLengthInSamples( mBPM, 44100, note.OffsetFromPrevNote, mTimeSignature );

            // Set the values for the NoteData's MelodyData.
            returned[index].MelodyData.NumSamples = numSamp;
            returned[index].MelodyData.Pitches = note.MusicalNote.Pitches;
            returned[index].MelodyData.Velocity = note.MusicalNote.Velocity;

            // Set the values for the NoteData's PercussionData.
            returned[index].PercussionData.Velocity = note.Drums.Velocity;
            returned[index].PercussionData.Hits = note.Drums.Hits;
            returned[index].PercussionData.HasHiHat = note.Drums.HasHiHat;

            // Set the offset for the NoteData.
            returned[index].TotalOffset = offset;

            // Go to the next note.
            index++;
        }

        return returned;
    }

    // Returns the notes in the song.
    public List<Music.CombinedNote> GetNotes()
    {
        return mNotes;
    }

    // Gets the number of notes.
    public int GetNumNotes()
    {
        return mNotes.Count;
    }

    // Gets the type of song that this is.
    public SongType GetSongType()
    {
        return mType;
    }

    // Gets the time signature of the song.
    // OUT: The time signature of the song.
    public Music.TimeSignature GetTimeSignature()
    {
        return mTimeSignature;
    }

    // Removes a note at a specific index from the song.
    public void RemoveNote( int aIndex )
    {
        Assert.IsTrue( aIndex < mNotes.Count, 
            "Tried to remove a note at the index " + aIndex.ToString() + " from a song with only " + mNotes.Count.ToString() + " notes in it!" );

        // Get the note to be removed.
        Music.CombinedNote removedNote = mNotes[aIndex];

        // Account for how many musical notes and drum notes are left in the song.
        if( removedNote.MusicalNote.Pitches != null )
        {
            mNumMusicalNotes--;
        }
        if( removedNote.Drums.Hits != null )
        {
            mNumDrumNotes--;
        }

        // Update the song type.
        UpdateSongType();

        // Remove the note.
        mNotes.RemoveAt( aIndex );
    }

    // Replaces a note at a specific index.
    public void ReplaceNote( Music.CombinedNote aNote, int aIndex )
    {
        Assert.IsTrue( aIndex < mNotes.Count,
             "Tried to replace a note at the index " + aIndex.ToString() + " for a song with only " + mNotes.Count.ToString() + " notes in it!" );

        // Get the replaced note.
        Music.CombinedNote replacedNote = mNotes[aIndex];

        // Account for changes in the number of musical notes and drum notes in the song.
        if( replacedNote.MusicalNote.Pitches != null && aNote.MusicalNote.Pitches == null )
        {
            mNumMusicalNotes--;
        }
        if( replacedNote.Drums.Hits != null && aNote.Drums.Hits == null )
        {
            mNumDrumNotes--;
        }

        // Update the song type.
        UpdateSongType();

        // Replace the note.
        mNotes[aIndex] = aNote;
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
    public void SetNotes( List<Music.CombinedNote> aNotes )
    {
        mNotes = aNotes;
    }

    // Sets the time signature of the song.
    public void SetTimeSignature( Music.TimeSignature aTimeSignature )
    {
        mTimeSignature.BeatsPerMeasure = aTimeSignature.BeatsPerMeasure;
        mTimeSignature.BaseBeat = aTimeSignature.BaseBeat;
    }

    // Writes the song to a file. The file will be stored in Resources/Music/Songs/.
    // If the song is combined melody and percussion, it will be named "SONG_songName.song"
    // If the song is just melody, it will be named "MELODY_songName.song"
    // If the song is a drum loop, it will be named "DRUMLOOP_songName.song"
    // The format of the file is:
    //     First line: "Name of song"
    //     Second line: "songType;defaultBPM;TimeSignature.BeatsPerMeasure;TimeSignature.BaseBeat"
    //     Rest of lines: 
    //        if CombinedMelodyAndPercussion: "pitch1,pitch2,...,pitchN|drum1,drum2,...,drumN;NoteLength;OffsetFromPreviousNote;MelodyVelocity|DrumVelocity"
    //        if DrumLoop: "drum1,drum2,...,drumN;OffsetFromPreviousNote;DrumVelocity"
    //        if Melody: "pitch1,pitch2,...,pitchN;NoteLength;OffsetFromPreviousNote;MelodyVelocity
    //
    // note: The values from Music enums are typecast to integers for saving. Ex: Music.PITCH.C4 is saved as 48. 
    public void WriteSongToFile()
    {
        // Set up the filepath.
        string filepath = Application.dataPath + "/Resources/Music/Songs/";
        switch( mType )
        {
            case SongType.DrumLoop:
                filepath += "DRUMLOOP_" + mName + ".song";
                break;
            case SongType.Melody:
                filepath += "MELODY_" + mName + ".song";
                break;
            case SongType.CombinedMelodyAndPercussion:
                filepath += "SONG_" + mName + ".song";
                break;
            default:
                break;
        }

        // Set an array for the contents of the file
        string[] contents = new string[mNotes.Count + 2];

        // Put the name of the song in first line to be saved.
        contents[0] = mName;

        // Put the song type, default BPM and the time signature values in the second line to be saved.
        contents[1] = ( (int)mType ).ToString() + ";" + mBPM.ToString() + ";" + 
            mTimeSignature.BeatsPerMeasure.ToString() + ";" + ( (int)mTimeSignature.BaseBeat ).ToString();

        // Put all the notes into the lines from line 3 to line [numNotes].  
        for( int i = 0; i < mNotes.Count; i++ )
        {
            // Set up the note string.
            contents[i + 2] = "";

            // If pitches exist for the song, then add them.
            if( mType != SongType.DrumLoop )
            {
                // Add the pitches if they exist for this note.
                if( mNotes[i].MusicalNote.Pitches != null )
                {
                    // Add the pitches to the string
                    for( int j = 0; j < mNotes[i].MusicalNote.Pitches.Length; j++ )
                    {
                        contents[i + 2] += ( (int)mNotes[i].MusicalNote.Pitches[j] ).ToString();

                        // If this is not the last pitch of the note, then add a comma
                        if( j + 1 != mNotes[i].MusicalNote.Pitches.Length )
                        {
                            contents[i + 2] += ",";
                        }
                        // If this is the last pitch of the note, then add a bar or a semicolon depending on if there are drums.
                        else
                        {
                            if( mType == SongType.CombinedMelodyAndPercussion )
                            {
                                contents[i + 2] += "|";
                            }
                            else
                            {
                                contents[i + 2] += ";";
                            }
                        }
                    }
                }
                // If pitches don't exist for the note, then put null.
                else
                {
                    contents[i + 2] += "null|";
                }
            }

            // If drums exist for the song, then add them.
            if( mType != SongType.Melody )
            {
                // If there are drums for this note, then add them. 
                if( mNotes[i].Drums.Hits != null )
                {
                    // Add the drums to the string
                    for( int j = 0; j < mNotes[i].Drums.Hits.Length; j++ )
                    {
                        contents[i + 2] += ( (int)mNotes[i].Drums.Hits[j] ).ToString();

                        // If this is not the last drum hit of the note, then add a comma
                        if( j + 1 != mNotes[i].Drums.Hits.Length )
                        {
                            contents[i + 2] += ",";
                        }
                        // If this is the last pitch of the note, then add a semicolon.
                        else
                        {
                            contents[i + 2] += ";";
                        }
                    }
                }
            }            

            // If the song is not a drum loop, then add the note length.
            if( mType != SongType.DrumLoop )
            {
                contents[i + 2] += ( ( (int)mNotes[i].MusicalNote.Length ).ToString() + ";" );
            }

            // Add the offset from the previous note.
            contents[i + 2] += ( ( (int)mNotes[i].OffsetFromPrevNote ).ToString() + ";" );

            // If the song is not a drum loop, then add the musical note velocity.
            if( mType != SongType.DrumLoop )
            {
                contents[i + 2] += mNotes[i].MusicalNote.Velocity.ToString();
            }

            // Put a bar if both drums and pitches exist for the song.
            if( mType == SongType.CombinedMelodyAndPercussion )
            {
                contents[i + 2] += "|";
            }

            // If the song has drums, then put the velocity for the drums for this note.
            if( mType != SongType.Melody )
            {
                contents[i + 2] += mNotes[i].Drums.Velocity;
            }

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
        // Create some variables for parsing the file.
        Music.PITCH[] curNotePitches = null;
        Music.DRUM[] curNoteDrums = null;
        Music.NOTE_LENGTH curNoteLength = Music.NOTE_LENGTH.NONE;
        int splitLineIndex = 0;
        int curNoteMelodyVelocity = 0;
        int curNoteDrumVelocity = 0;

        // Open the file stream.
        StreamReader parser = new StreamReader( aSongFilePath );

        // Get the name of the song
        string curLine = parser.ReadLine();
        mName = curLine;

        // Get the song type.
        curLine = parser.ReadLine();
        string[] splitLine = curLine.Split( ';' );
        mType = (SongType)int.Parse( splitLine[0] );

        // Get the default BPM
        mBPM = int.Parse( splitLine[1] );

        // Get the time signature
        mTimeSignature.BeatsPerMeasure = int.Parse( splitLine[2] );
        mTimeSignature.BaseBeat = (Music.NOTE_LENGTH)int.Parse( splitLine[3] );

        // Get the notes of the song.
        curLine = parser.ReadLine();
        while( curLine != null )
        {
            // Get the line and reset the split line index.
            splitLine = curLine.Split( ';' );
            splitLineIndex = 0;

            // Get the pitches for the note if needed.
            if( mType != SongType.DrumLoop )
            {
                curNotePitches = ParsePitches( splitLine[splitLineIndex] );
            }

            // Get the drums for the note if needed.
            if( mType != SongType.Melody )
            {
                curNoteDrums = ParseDrums( splitLine[splitLineIndex] );
            }

            // Go to the next section of the line.
            splitLineIndex++;

            // If needed, get the melody note length.
            if( mType != SongType.DrumLoop )
            {
                curNoteLength = (Music.NOTE_LENGTH)int.Parse( splitLine[splitLineIndex] );
                splitLineIndex++;
            }

            // Get the offset from the previous note.
            Music.NOTE_LENGTH offset = (Music.NOTE_LENGTH)int.Parse( splitLine[splitLineIndex] );
            splitLineIndex++;

            // Get the velocity/velocities of the note.
            if( mType == SongType.CombinedMelodyAndPercussion )
            {
                string[] velocityString = splitLine[splitLineIndex].Split( '|' );
                curNoteMelodyVelocity = int.Parse( velocityString[0] );
                curNoteDrumVelocity = int.Parse( velocityString[1] );
            }
            else if( mType == SongType.Melody )
            {
                curNoteMelodyVelocity = int.Parse( splitLine[splitLineIndex] );
            }
            else
            {
                curNoteDrumVelocity = int.Parse( splitLine[splitLineIndex] );
            }

            // Add the note.
            AddNote( Music.CreateNote( curNoteMelodyVelocity, curNoteLength, curNotePitches, curNoteDrumVelocity, curNoteDrums, offset ) );

            // Get the next line
            curLine = parser.ReadLine();
        }
    }

    // Parses the pitches from a string.
    // IN: aStringFromFile The string to parse.
    private Music.DRUM[] ParseDrums( string aStringFromFile )
    {
        Music.DRUM[] parsedDrums = null;

        // Get the entire string of drums.
        string drumString = aStringFromFile;
        if( mType == SongType.CombinedMelodyAndPercussion )
        {
            drumString = aStringFromFile.Split( '|' )[1];
        }

        // If there are drums for the note, then parse each drum.
        if( drumString != "null" )
        {
            // Split the string into individual drums.
            string[] drums = drumString.Split( ',' );

            // Iterate through each drum and parse it.
            parsedDrums = new Music.DRUM[drums.Length];
            for( int i = 0; i < drums.Length; i++ )
            {
                parsedDrums[i] = (Music.DRUM)int.Parse( drums[i] );
            }
        }

        // Return the parsed drums.
        return parsedDrums;
    }

    // Parses the pitches from a string.
    // IN: aStringFromFile The string to parse.
    private Music.PITCH[] ParsePitches( string aStringFromFile )
    {
        Music.PITCH[] parsedPitches = null;

        // Get the entire string of pitches.
        string pitchString = aStringFromFile;
        if( mType == SongType.CombinedMelodyAndPercussion )
        {
            pitchString = aStringFromFile.Split( '|' )[0];
        }

        // If there are pitches for the note, then parse each pitch.
        if( pitchString != "null" )
        {
            // Split the string into individual pitches.
            string[] pitches = pitchString.Split( ',' );

            // Iterate through each pitch and parse it.
            parsedPitches = new Music.PITCH[pitches.Length];
            for( int i = 0; i < pitches.Length; i++ )
            {
                parsedPitches[i] = (Music.PITCH)int.Parse( pitches[i] );
            }
        }

        // Return the parsed pitches.
        return parsedPitches;
    }

    // Updates the type of song that this is.
    private void UpdateSongType()
    {
        if( mNumDrumNotes > 0 && mNumMusicalNotes > 0 )
        {
            mType = SongType.CombinedMelodyAndPercussion;
        }
        else if( mNumMusicalNotes > 0 )
        {
            mType = SongType.Melody;
        }
        else if( mNumDrumNotes > 0 )
        {
            mType = SongType.DrumLoop;
        }
        else
        {
            mType = SongType.Empty;
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
