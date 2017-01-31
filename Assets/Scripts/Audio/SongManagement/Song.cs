using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/**
 * @class Song
 * @brief A C# class that represents a song.
 * 
 * This class's responsibilities include:
 * @li Serving as a container for a pre-arranged series of @link Music::CombinedNote notes@endlink.
 * @li Converting the @link Music::CombinedNote abstract representions of musical notes@endlink into @link Song::CombinedNoteData a form that relates to the actual waveforms of the audio@endlink.
 * @li Saving and loading copies of itself to/from a file.
*/
public class Song
{
    /*************************************************************************//** 
     * @defgroup SongConst Constants
     * @ingroup DocSong
     * Constants used in order to set attributes of the Song.
     * @{
    *****************************************************************************/
    public static string SONG_FILE_PATH = Application.streamingAssetsPath + "/Songs/"; //!< The path of the folder that contains Song files.

    /*************************************************************************//** 
     * @}
     * @defgroup SongEnums Enums
     * @ingroup DocSong
     * Enums used to specify attributes of the Song.
     * @{
    *****************************************************************************/

    /**
     * @enum SongType
     * @brief The possible types of songs.
    */
    public enum SongType
    {
        Empty, //!< An empty Song. 
        Melody, //!< A Song that contains only @link Music::PITCH pitches@endlink.
        DrumLoop, //!< A Song that contains only @link Music::DRUM drums@endlink.
        CombinedMelodyAndPercussion //!< A song that contains both @link Music::PITCH pitches@endlink and @link Music::DRUM drums@endlink.
    }

    /*************************************************************************//** 
     * @}
     * @defgroup SongStructs Structs
     * @ingroup DocSong
     * Structs for holding data about the @link Music::CombinedNote notes@endlink in the Song as they relate to audio waveforms.
     * @{
    *****************************************************************************/

    /**
     * @struct Song::PercussionData
     * @brief A struct for data related to @link Music::PercussionNote drums@endlink.
    */
    public struct PercussionData
    {
        public bool HasHiHat; //!< Whether or not the @link Music::PercussionNote note@endlink contains a hi-hat hit. Used for silencing open hi-hats when a closed/pedal hi-hat hit occurs.  
        public int Velocity; //!< The @link DefVel velocity@endlink of the @link Music::PercussionNote.Hits drum hits@endlink in the note.
        public Music.DRUM[] Hits; //!< The @link Music::PercussionNote.Hits drum hits@endlink in the note.
    }

    /**
     * @struct Song::MelodyNoteData
     * @brief A struct for data related to @link Music::MelodyNote notes that contain pitches@endlink.
    */
    public struct MelodyNoteData
    {
        public int NumSamples; //!< The @link Music::MelodyNote.Length length of the note@endlink in terms of the number of waveform samples that should be output. 
        public int Velocity; //!< The @link DefVel velocity@endlink of the note.
        public Music.PITCH[] Pitches; //!< The @link Music::MelodyNote.Pitches pitches@endlink in the note.
    }

    /**
     * @struct Song::CombinedNoteData
     * @brief A struct that contains data for both @link Music::PercussionNote drums@endlink and the @link Music::MelodyNote melody@endlink.
    */
    public struct CombinedNoteData
    {
        public int TotalOffset; //!< The number of waveform samples before the note should be played. Important: This offset is relative to the song as a whole, not just the previous note like in @link Music::CombinedNote the abstract version of this struct@endlink.
        public MelodyNoteData MelodyData; //!< The data related to playing @link Music::MelodyNote pitches@endlink.
        public PercussionData PercussionData; //!< The data related to playing @link Music::PercussionNote drum hits@endlink.
    }

    /*************************************************************************//** 
     * @}
     * @defgroup SongPrivVar Private Variables
     * @ingroup DocSong
     * Variables that contain information about the Song and are used internally by the Song.
     * @{
    *****************************************************************************/
    private int mBPM = 120; //!< The default @link DefBPM BPM@endlink of the Song.
    private int mNumDrumNotes; //!< The number of @link Music::CombinedNote notes@endlink that have @link Music::PercussionNote drums@endlink.
    private int mNumMusicalNotes; //!< The number of @link Music::CombinedNote notes@endlink that have @link Music::MelodyNote pitches@endlink.
    private List<Music.CombinedNote> mNotes = null; //!< The @link Music::CombinedNote notes@endlink of the Song.
    private Music.PITCH mHighestPitch = Music.PITCH.C0; //!< The highest @link Music::PITCH pitch@endlink in the Song.
    private Music.PITCH mLowestPitch = Music.PITCH.B9; //!< The lowest @link Music::PITCH pitch@endlink in the Song.
    private Music.TimeSignature mTimeSignature; //!< The @link Music::TimeSignature time signature@endlink of the Song.
    private SongType mType = SongType.Empty; //!< The @link Song::SongType type of song@endlink that this Song is.
    private string mName = null; //!< The name of the Song.

    /*************************************************************************//** 
     * @}
     * @defgroup SongConstruct Constructors
     * @ingroup DocSong
     * Constructors to create a new Song instance.
     * @{
    *****************************************************************************/

    /**
     * @brief The default constructor. 
     * @return A new empty Song. 
     * Initializes the @link Song::mNotes list of notes@endlink and sets the @link Song::mBPM BPM@endlink to 120, 
     * the @link Music::TimeSignature time signature@endlink to @link Music::TIME_SIGNATURE_4_4 4/4@endlink 
     * and the @link Song::mName name of the Song@endlink to "Untitled".
    */
    public Song()
    {
        // Set the member variables.
        mNotes = new List<Music.CombinedNote>();
        mTimeSignature = Music.TIME_SIGNATURE_4_4();
        mBPM = 120;
        mName = "Untitled";
    }

    /**
     * @brief A constructor that creates the song by @link Song::LoadSongFromFile loading a file@endlink.
     * @param[in] aSongFilePath The filepath to the file representing the Song to load.
     * @return A new Song filled with information from the file.
    */
    public Song( string aSongFilePath )
    {
        mNotes = new List<Music.CombinedNote>();
        LoadSongFromFile( aSongFilePath );
    }

    /*************************************************************************//** 
     * @}
     * @defgroup SongPubFunc Public Functions
     * @ingroup DocSong
     * Functions that allow other classes to interact with the Song.
     * @{
    *****************************************************************************/

    /** 
     * @brief Adds a @link Music::CombinedNote note@endlink to the Song.
     * @param[in] aNewNote The @link Music::CombinedNote note@endlink to add.
    */
    public void AddNote( Music.CombinedNote aNewNote )
    {
        // Update the number of musical notes in the song if needed.
        if( aNewNote.MusicalNote.Pitches != null )
        {
            mNumMusicalNotes++;

            for( int i = 0; i < aNewNote.MusicalNote.Pitches.Length; i++ )
            {
                // Update the highest pitch.
                if( aNewNote.MusicalNote.Pitches[i] != Music.PITCH.REST && mHighestPitch < aNewNote.MusicalNote.Pitches[i] )
                {
                    mHighestPitch = aNewNote.MusicalNote.Pitches[i];
                }

                // Update the lowest pitch.
                if( aNewNote.MusicalNote.Pitches[i] != Music.PITCH.REST && mLowestPitch > aNewNote.MusicalNote.Pitches[i] )
                {
                    mLowestPitch = aNewNote.MusicalNote.Pitches[i];
                }
            }

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

    /**
     * @brief Gets the BPM of the song
     * @return The BPM of the song as an integer.
    */
    public int GetBPM()
    {
        return mBPM;
    }

    /** 
     * @brief Gets the highest pitch in the song.
     * @return The highest pitch in the song
    */
    public Music.PITCH GetHighestPitch()
    {
        Assert.IsTrue( mType != SongType.Empty && mType != SongType.DrumLoop,
            "Tried to get information about pitches in a song that had no pitches!" );

        return mHighestPitch;
    }

    /** 
     * @brief Gets the lowest pitch in the song.
     * @return The lowest pitch in the song
    */
    public Music.PITCH GetLowestPitch()
    {
        Assert.IsTrue( mType != SongType.Empty && mType != SongType.DrumLoop,
            "Tried to get information about pitches in a song that had no pitches!" );

        return mLowestPitch;
    }

    /** 
     * @brief Gets the name of the song.
     * @return The name of the song as a string.
    */
    public string GetName()
    {
        return mName;
    }

    /**
     * @brief Gets a @link Music::CombinedNote note@endlink in the Song.
     * @param[in] aIndex The index of the note in the song.
     * @return The note at the specified index in the song. 
    */
    public Music.CombinedNote GetNote( int aIndex )
    {
        Assert.IsTrue( mNotes.Count > 0, "Tried to get a note from an empty song!" );
        return mNotes[aIndex];
    }

    /**
     * @brief Converts the @link Music::CombinedNote notes@endlink in the song to NoteData.
     * @return An array of @link Song::CombinedNoteData note data@endlink representing the @link Music::CombinedNote notes@endlink in the Song.
     * 
     * This function is used to convert the Song's @link Music::CombinedNote abstract information@endlink
     * which is based around musical notation into @link Song::CombinedNoteData specific information@endlink
     * that can be used to map it to audio data and waveform samples.
     * 
     * @see VirtualInstrumentManager::OnPlaySongEvent VirtualInstrumentManager::OnPlayDrumLoopEvent NoteOutputObject::OnAudioFilterRead
    */  
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

    /**
     * @brief Returns a list of the @link Music::CombinedNote notes@endlink in the Song.
     * @return A list of the @link Music::CombinedNote notes@endlink in the Song.
    */
    public List<Music.CombinedNote> GetAllNotes()
    {
        return mNotes;
    }

    /**
     * @brief Gets the number of @link Music::CombinedNote notes@endlink in the Song.
     * @return The number of @link Music::CombinedNote notes@endlink in the Song.
    */
    public int GetNumNotes()
    {
        return mNotes.Count;
    }

    /**
     * @brief Gets the @link Song::SongType type of song@endlink that the Song is.
     * @return The @link Song::SongType type of song@endlink that the Song is.
    */
    public SongType GetSongType()
    {
        return mType;
    }

    /**
     * @brief Gets the @link Music::TimeSignature time signature@endlink of the Song.
     * @return The @link Music::TimeSignature time signature@endlink of the Song.
    */
    public Music.TimeSignature GetTimeSignature()
    {
        return mTimeSignature;
    }

    /** 
     * @brief Removes a note from the song.
     * @param[in] aIndex The index of the note to remove.
    */
    public void RemoveNote( int aIndex )
    {
        Assert.IsTrue( aIndex < mNotes.Count,
            "Tried to remove a note at the index " + aIndex.ToString() + " from a song with only " + mNotes.Count.ToString() + " notes in it!" );

        // Get the note to be removed.
        Music.CombinedNote removedNote = mNotes[aIndex];

        // Remove the note.
        mNotes.RemoveAt( aIndex );

        // Update information about the pitches in the song..
        if( removedNote.MusicalNote.Pitches != null )
        {
            // Decrease the number of musical notes.
            mNumMusicalNotes--;

            // See if we need to set a new highest/lowest pitch.
            foreach( Music.PITCH pitch in removedNote.MusicalNote.Pitches )
            {
                if( pitch != Music.PITCH.REST && pitch == mHighestPitch )
                {
                    mHighestPitch = Music.PITCH.C0;
                    SearchForHighestPitch();
                }
                if( pitch != Music.PITCH.REST && pitch == mLowestPitch )
                {
                    mLowestPitch = Music.PITCH.B9;
                    SearchForLowestPitch();
                }
            }
        }

        // Update information about the drums in the song.
        if( removedNote.Drums.Hits != null )
        {
            mNumDrumNotes--;
        }

        // Update the song type.
        UpdateSongType();

        // Clean up.
        GC.Collect();
        Resources.UnloadUnusedAssets();
    }

    /**
     * @brief Replaces a note at a given index.
     * @param[in] aNote The note that will be put into the song at the given index.
     * @param[in] aIndex The index of the place to insert the note.
    */
    public void ReplaceNote( Music.CombinedNote aNote, int aIndex )
    {
        Assert.IsTrue( aIndex < mNotes.Count,
             "Tried to replace a note at the index " + aIndex.ToString() + " for a song with only " + mNotes.Count.ToString() + " notes in it!" );

        // Get the replaced note.
        Music.CombinedNote replacedNote = mNotes[aIndex];

        // Replace the note.
        mNotes[aIndex] = aNote;

        // See if we need to update the highest/lowest pitch.
        if( replacedNote.MusicalNote.Pitches != null )
        {
            // Decrease the number of musical notes.
            mNumMusicalNotes--;

            // See if we need to set a new highest/lowest pitch.
            foreach( Music.PITCH pitch in replacedNote.MusicalNote.Pitches )
            {
                if( pitch != Music.PITCH.REST && pitch == mHighestPitch )
                {
                    mHighestPitch = Music.PITCH.C0;
                    SearchForHighestPitch();
                }
                if( pitch != Music.PITCH.REST && pitch == mLowestPitch )
                {
                    mLowestPitch = Music.PITCH.B9;
                    SearchForLowestPitch();
                }
            }
        }

        // Account for changes in the number of musical notes and drum notes in the song.
        if( replacedNote.MusicalNote.Pitches != null && aNote.MusicalNote.Pitches == null )
        {
            mNumMusicalNotes--;
        }
        else if( replacedNote.MusicalNote.Pitches == null && aNote.MusicalNote.Pitches != null )
        {
            mNumMusicalNotes++;
        }
        if( replacedNote.Drums.Hits != null && aNote.Drums.Hits == null )
        {
            mNumDrumNotes--;
        }
        else if( replacedNote.Drums.Hits == null && aNote.Drums.Hits != null )
        {
            mNumDrumNotes++;
        }


        // Update the song type.
        UpdateSongType();
    }

    /**
     * @brief Sets the default @link DefBPM BPM@endlink of the Song.
     * @param[in] aBPM The new default @link DefBPM BPM@endlink of the Song.
    */
    public void SetBPM( int aBPM )
    {
        mBPM = aBPM;
    }

    /**
     * @brief Sets the default @link DefBPM BPM@endlink of the Song.
     * @param[in] aBPM The new default @link DefBPM BPM@endlink of the Song.
    */
    public void SetBPM( float aBPM )
    {
        mBPM = (int)aBPM;
    }

    /** 
     * @brief Sets the @link Song::mName name@endlink of the Song.
     * @param[in] aSongName The @link Song::mName name@endlink of the Song.
    */
    public void SetName( string aSongName )
    {
        mName = aSongName;
    }

    /**
     * @brief Sets the @link Music::TimeSignature time signature@endlink of the Song.
     * @param[in] aTimeSignature The @link Music::TimeSignature time signature@endlink of the Song.
    */
    public void SetTimeSignature( Music.TimeSignature aTimeSignature )
    {
        mTimeSignature.BeatsPerMeasure = aTimeSignature.BeatsPerMeasure;
        mTimeSignature.BaseBeat = aTimeSignature.BaseBeat;
    }

    /** 
     * @brief Writes the song to a file. The file will be stored in StreamingAssets/Songs/.
     * @see @link DocSongFileFormat Song File Format@endlink
    */
    public void WriteSongToFile()
    {
        // Set up the filepath.
        string filepath = SONG_FILE_PATH;
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

    /*************************************************************************//** 
     * @}
     * @defgroup SongPrivFunc Private Functions
     * @ingroup DocSong
     * Functions used internally by the Song.
     * @{
    *****************************************************************************/

    /**
     * @brief Loads a @link DocSongFileFormat Song file@endlink and uses it to set the values for this Song.  
     * @param[in] aSongFilePath The path to the Song file.
     * @see @link DocSongFileFormat Song File Format@endlink
    */
    private void LoadSongFromFile( string aSongFilePath )
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

    /** 
     * @brief Parses the @link Music::DRUM drums@endlink from a string in a @link DocSongFileFormat Song file@endlink.
     * @param[in] aStringFromFile The string to parse.
     * @see @link DocSongFileFormat Song File Format@endlink
    */
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

    /** 
     * @brief Parses the @link Music::PITCH pitches@endlink from a string in a @link DocSongFileFormat Song file@endlink.
     * @param[in] aStringFromFile The string to parse.
     * @see @link DocSongFileFormat Song File Format@endlink
    */
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

    /**
     * @brief Searches through the notes for the highest pitch. 
     * 
     * This function is called when a note that contains the highest
     * pitch in the song is removed or replaced. We have to then
     * find the new highest pitch in the song.
    */
    private void SearchForHighestPitch()
    {
        // Look through each note.
        foreach( Music.CombinedNote note in mNotes )
        {
            // If the note has pitches, then see if it has the highest pitch.
            if( note.MusicalNote.Pitches != null )
            {
                // Look through each pitch in the note.
                foreach( Music.PITCH pitch in note.MusicalNote.Pitches )
                {
                    // Update the highest pitch if needed.
                    if( pitch != Music.PITCH.REST && pitch > mHighestPitch )
                    {
                        mHighestPitch = pitch;
                    }
                }
            }
        }
    }

    /**
     * @brief Searches through the notes for the lowest pitch. 
     * 
     * This function is called when a note that contains the lowest
     * pitch in the song is removed or replaced. We have to then
     * find the new lowest pitch in the song.
    */
    private void SearchForLowestPitch()
    {
        // Look through each note.
        foreach( Music.CombinedNote note in mNotes )
        {
            // If the note has pitches, then see if it has the lowest pitch.
            if( note.MusicalNote.Pitches != null )
            {
                // Look through each pitch in the note.
                foreach( Music.PITCH pitch in note.MusicalNote.Pitches )
                {
                    // Update the lowest pitch if needed.
                    if( pitch != Music.PITCH.REST && pitch < mLowestPitch )
                    {
                        mLowestPitch = pitch;
                    }
                }
            }
        }
    }

    /**
     * @brief Updates the @link Song::SongType type of song@endlink that the Song is.
    */
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

    /*************************************************************************//** 
     * @}
     * @defgroup SongStatFunc Static Functions
     * @ingroup DocSong
     * Functions related to @link Song Songs@endlink that can be used without having to have an actual instance of a Song.
     * @{
    *****************************************************************************/

    /** 
     * @brief Gets the number of waveform samples required to accurately represent the @link Music::NOTE_LENGTH length of a note@endlink for a given @link DefBPM BPM@endlink and @link Music::TimeSignature time signature@endlink.
     * @param[in] aBPM The @link DefBPM BPM@endlink that should be accounted for.
     * @param[in] aSampleRate The sample rate of the note. Might need to remove since most likely everything's going to have a 44\.1KHz sample rate.
     * @param[in] aNoteLength The @link Music::NOTE_LENGTH note length@endlink that's being converted into a number of waveform samples.
     * @param[in] aTimeSignature The @link Music::TimeSignature time signature@endlink that should be accounted for.
     * @return The number of waveform samples that represents the given @link Music::NOTE_LENGTH note length@endlink at the given @link DefBPM BPM@endlink and @link Music::TimeSignature time signature@endlink.
     * 
    */
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
                return (int)( 1.5f * numSamplesPerBeat / ( 2f * numQuarterNotesPerBeat ) );
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
    /** @} */
}
