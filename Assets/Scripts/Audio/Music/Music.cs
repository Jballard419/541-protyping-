﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * @class Music
 * @brief A container for everything related to music such as pitches and note lengths. This class does not need to be initialized to use.
 * 
 * This class provides structs, enums, constants, and static functions related to 
 * abstract representations of music. "Abstract representation" meaning that this class
 * only deals with the musical notation side of things rather than the audio data. 
 * For examples, note length from this class would be given as something like a quarter note 
 * rather than a number of waveform samples.
 * 
 * @nosubgrouping
*/
public class Music
{

    /*************************************************************************//** 
    * @defgroup MusicConstants Constants
    * @ingroup DocMusic
    * These are constants used in the @link MusicStatFunc static functions below@endlink.
    ****************************************************************************/
    /** @{ */
    public static float DOT_MODIFIER = 1.5f;
    public static float TRIPLET_MODIFIER = 0.66666666666666666666666666666666667f;
    public static string[] NOTE_LENGTH_BASE_IMAGE_PATHS =
    {
        "Audio/Images/32nd", "Audio/Images/16th", "Audio/Images/eighth", "Audio/Images/quarter", 
        "Audio/Images/half", "Audio/Images/whole"
    }; //!< The paths to load the images used to represent the @link NOTE_LENGTH_BASE note lengths@endlink.
    public static string[] DRUM_STRING =
        { "KICK_1", "KICK_2", "SNARE_1", "SNARE_RIM", "SNARE_2", "LOWTOM_1",
        "HIHAT_C", "LOWTOM_2", "HIHAT_P", "MIDTOM_1", "HIHAT_O", "MIDTOM_2",
        "HIGHTOM_1", "CRASH_1", "HIGHTOM_2", "RIDE", "CRASH_2", "RIDE_BELL" }; //!< An array of strings that represent the name of each type of drum.
    public static string[] PITCH_STRING = { "C", "CS", "D", "DS", "E", "F", "FS", "G", "GS", "A", "AS", "B" }; //!< An array of strings that represent each pitch in an octave.
    public static short NUM_NOTES_IN_OCTAVE = 12; //!< The number of notes in an octave
    public static int MAX_SUPPORTED_NOTES = 120; //!< The maximum number of pitches that a VirtualInstrument could support.
    public static int MAX_SUPPORTED_DRUMS = 18; //!< The maximum number of drums that a DrumKit could support.
    /** @} */

    /*************************************************************************//** 
     * @defgroup MusicEnums Enums
     * @ingroup DocMusic
     * These are enums which help other classes organize anything related to music.
     * Enums are pretty much just integer arrays with unique identifiers instead of indices. They're used to help make code more readable.
     * An example of using an enum would be: @code Music.PITCH newNotePitch = Music.PITCH.C4; @endcode 
     * The actual values of each member of these enums (remember that it's pretty much an array of integers)
     * range from 0 to the number of members minus 1, and these values can be retrieved by typecasting.
     * For example: 
     * @code int noteIndex = (int)Music.PITCH.C4; // noteIndex now holds the value 48. @endcode 
     * noteIndex holds the value 48 because @link Music::PITCH::C4 C4@endlink is the 49th member of the @link Music::PITCH pitch enum@endlink which starts from 0.
     * The members values can be typecast to int,float,... and vice-versa.
     * For example: 
     * @code Music.PITCH newPitch = (Music.PITCH)48; // newPitch now holds the value Music.PITCH.C4 @endcode
     ****************************************************************************/
    /** @{ */

    /**
    * @enum DRUM
    * @brief The possible drums/cymbals that can be played
    */
    public enum DRUM
    {
        KICK_1,
        KICK_2,
        SNARE_1,
        SNARE_RIM,
        SNARE_2,
        LOWTOM_1,
        HIHAT_C,
        LOWTOM_2,
        HIHAT_P,
        MIDTOM_1,
        HIHAT_O,
        MIDTOM_2,
        HIGHTOM_1,
        CRASH_1,
        HIGHTOM_2,
        RIDE,
        CRASH_2,
        RIDE_BELL
    }

    /**
     * @enum INSTRUMENT_TYPE
     * 
     * @brief The types of instruments that are currently supported.
    */
    public enum INSTRUMENT_TYPE
    {
        PIANO, //!< A Piano
        MARIMBA, //!< A Marimba
        DRUM_KIT //!< A DrumKit
    };

    /**
     * @enum NOTE_LENGTH_BASE
     * @brief Abstract representations of a note's length.
     * 
     * As of now, we only support 32nd notes up to whole notes and their dotted equivalents.
     * Triplets are not currently supported, but will be in the near future.
    */
    public enum NOTE_LENGTH_BASE
    {
        TS, //!< 32nd note.
        S, //!< 16th note.
        E, //!< Eighth note.
        Q, //!< Quarter note.
        H, //!< Half note.
        W, //!< Whole note.
        NONE //!< No length 
    };

    /** 
     * @enum PITCH
     * @brief The possible pitches that can be played. The range is from C0 to B9 with an extra entry for a rest note (No pitch).
     * 
     * The base pitches are C, C#, D, D# E, F, F#, G, G#, A, A#, and B. These 12 notes make up an octave.
     * For a pitch C4, the C refers to the base pitch, and the 4 refers to the specific octave. 
     * @n For reference, C4 is known as Middle C. 
    */
    public enum PITCH
    {
        C0,
        CS0,
        D0,
        DS0,
        E0,
        F0,
        FS0,
        G0,
        GS0,
        A0,
        AS0,
        B0,
        C1,
        CS1,
        D1,
        DS1,
        E1,
        F1,
        FS1,
        G1,
        GS1,
        A1,
        AS1,
        B1,
        C2,
        CS2,
        D2,
        DS2,
        E2,
        F2,
        FS2,
        G2,
        GS2,
        A2,
        AS2,
        B2,
        C3,
        CS3,
        D3,
        DS3,
        E3,
        F3,
        FS3,
        G3,
        GS3,
        A3,
        AS3,
        B3,
        C4,
        CS4,
        D4,
        DS4,
        E4,
        F4,
        FS4,
        G4,
        GS4,
        A4,
        AS4,
        B4,
        C5,
        CS5,
        D5,
        DS5,
        E5,
        F5,
        FS5,
        G5,
        GS5,
        A5,
        AS5,
        B5,
        C6,
        CS6,
        D6,
        DS6,
        E6,
        F6,
        FS6,
        G6,
        GS6,
        A6,
        AS6,
        B6,
        C7,
        CS7,
        D7,
        DS7,
        E7,
        F7,
        FS7,
        G7,
        GS7,
        A7,
        AS7,
        B7,
        C8,
        CS8,
        D8,
        DS8,
        E8,
        F8,
        FS8,
        G8,
        GS8,
        A8,
        AS8,
        B8,
        C9,
        CS9,
        D9,
        DS9,
        E9,
        F9,
        FS9,
        G9,
        GS9,
        A9,
        AS9,
        B9,
        REST
    }
    /** @} */

    /*************************************************************************//** 
    * @defgroup MusicStructs Structs
    * @ingroup DocMusic
    * These are structs used to hold abstract representations of Music. There are three structs
    * for different types of notes (@link Music::MelodyNote melody@endlink, @link Music::PercussionNote percussion@endlink, 
    * and @link Music::CombinedNote both@endlink) and one for representing
    * a @link Music::TimeSignature time signature@endlink.
    ****************************************************************************/
    /** @{ */


    /**
     * @struct Music::MelodyNote
     * @brief A struct that provides an abstract representation of a note for a non-DrumKit VirtualInstrument.
    */
    public struct MelodyNote
    {
        public int NumPitches; //!< The number of @link PITCH pitches@endlink in the note.
        public int[] Velocities; //!< The velocities of the @link PITCH pitches@endlink.
        public NoteLength[] Lengths; //!< The lengths of the pitches. @see NOTE_LENGTH_BASE
        public PITCH[] Pitches; //!< The pitches of the note. @see PITCH

        public MelodyNote( int[] aVelocities = null, NoteLength[] aLengths = null, PITCH[] aPitches = null )
        {
            if( aVelocities != null )
            {
                Assert.IsTrue( aVelocities != null && aVelocities.Length == aLengths.Length && aPitches.Length == aLengths.Length,
                    "Tried to make a melody note with a varied number of lengths, velocities, and pitches!" );
            }
            Velocities = aVelocities;
            Lengths = aLengths;
            Pitches = aPitches;

            if( aPitches != null )
            {
                NumPitches = aPitches.Length;
            }
            else
            {
                NumPitches = 0;
            }
        }
    }

    /**
     * @struct Music::PercussionNote
     * @brief A struct that provides an abstract representation of a note for a DrumKit.
    */
    public struct PercussionNote
    {
        public bool HasHiHat; //!< Whether or not the note contains a hi-hat hit. This is used to silence open hi-hat hits when a closed/pedal hit occurs.
        public int NumHits; //!< The number of @link DRUM drums/cymbals@endlink that are hit.
        public int[] Velocities; //!< The @link DefVel velocities@endlink of the @link DRUM drum hits@endlink.
        public DRUM[] Hits; //!< The drums that are hit during this note.

        public PercussionNote( int[] aVelocities = null, DRUM[] aDrums = null )
        {
            if( aVelocities != null )
            {
                Assert.IsTrue( aVelocities != null && aVelocities.Length == aDrums.Length,
                    "Tried to make a percussion note with a varied number of velocities and drums!" );
            }
            Velocities = aVelocities;
            Hits = aDrums;
            HasHiHat = false;
            if( Hits != null )
            {
                for( int i = 0; i < Hits.Length && !HasHiHat; i++ )
                {
                    if( Hits[i] == DRUM.HIHAT_O || Hits[i] == DRUM.HIHAT_C || Hits[i] == DRUM.HIHAT_P )
                    {
                        HasHiHat = true;
                    }
                }
            }

            if( aDrums != null )
            {
                NumHits = aDrums.Length;
            }
            else
            {
                NumHits = 0;
            }
        }
    }

    /**
     * @struct Music::CombinedNote
     * @brief A struct that provides an abstract representation of a note that has both drums and melody.
     * 
     * This struct combines the Music::MelodyNote and the Music::PercussionNote structs into one complete note.
    */
    public struct CombinedNote
    {
        public int NumDrums; //!< The number of @link DRUM drum/cymbal hits@endlink in the note.
        public int NumPitches; //!< The number of @link PITCH pitches@endlink in the note.
        public MelodyNote MusicalNote; //!< The @link Music::MelodyNote melody note@endlink
        public NoteLength OffsetFromPrevNote; //!< The offset from the previous note.
        public PercussionNote Drums; //!< The @link Music::PercussionNote drum note@endlink.

        public CombinedNote( MelodyNote aMelodyNote, PercussionNote aPercussionNote, NoteLength aOffset )
        {
            MusicalNote = aMelodyNote;
            NumPitches = MusicalNote.NumPitches;
            Drums = aPercussionNote;
            NumDrums = Drums.NumHits;
            OffsetFromPrevNote = aOffset;
        }
    }

    /**
     * @struct Music::NoteLength
     * @brief A struct that allows for modification of the @link Music::NOTE_LENGTH_BASE base note length
    */
    public struct NoteLength
    {
        public bool Dot; //!< Is the note dotted?
        public bool Triplet; //!< Is the note a triplet
        public NOTE_LENGTH_BASE BaseLength; //!< @copydoc Music::NOTE_LENGTH_BASE

        public NoteLength( NOTE_LENGTH_BASE aBaseLength )
        {
            BaseLength = aBaseLength;
            Dot = false;
            Triplet = false;
        }

        public NoteLength( NOTE_LENGTH_BASE aBaseLength, bool aDot, bool aTriplet )
        {
            Dot = aDot;
            Triplet = aTriplet;
            BaseLength = aBaseLength;
        }

        public NoteLength( string aString )
        {
            Assert.IsTrue( aString.Length == 3, "Tried to initialize a NoteLength struct with an improperly-formatted string!" );
            
            BaseLength = (NOTE_LENGTH_BASE)( int.Parse( aString.Substring( 0, 1 ) ) );
            if( int.Parse( aString.Substring( 1, 1 ) ) == 1 )
            {
                Dot = true;
            }
            else
            {
                Dot = false;
            }
            
            if( int.Parse( aString.Substring( 2, 1 ) ) == 1 )
            {
                Triplet = true;
            }
            else
            {
                Triplet = false;
            }
        }

        public override bool Equals( System.Object aObj )
        {
            return aObj is NoteLength && this == (NoteLength)aObj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            string returned = "";
            returned += ( (int)BaseLength ).ToString();
            if( Dot )
            {
                returned += "1";
            }
            else
            {
                returned += "0";
            }

            if( Triplet )
            {
                returned += "1";
            }
            else
            {
                returned += "0";
            }
            return returned;
        }

        public static bool operator !=( NoteLength aNoteLength, NoteLength aOtherNoteLength )
        {
            return !( aNoteLength == aOtherNoteLength );
        }

        public static bool operator ==( NoteLength aNoteLength, NoteLength aOtherNoteLength )
        {
            if( GetNoteLengthRelativeToQuarterNote( aNoteLength ) == GetNoteLengthRelativeToQuarterNote( aOtherNoteLength ) )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator <( NoteLength aNoteLength, NoteLength aOtherNoteLength )
        {
            return GetNoteLengthRelativeToQuarterNote( aNoteLength ) < GetNoteLengthRelativeToQuarterNote( aOtherNoteLength );
        }

        public static bool operator >( NoteLength aNoteLength, NoteLength aOtherNoteLength )
        {
            return GetNoteLengthRelativeToQuarterNote( aNoteLength ) > GetNoteLengthRelativeToQuarterNote( aOtherNoteLength );
        }

        public static bool operator >=( NoteLength aNoteLength, NoteLength aOtherNoteLength )
        {
            return GetNoteLengthRelativeToQuarterNote( aNoteLength ) >= GetNoteLengthRelativeToQuarterNote( aOtherNoteLength );
        }

        public static bool operator <=( NoteLength aNoteLength, NoteLength aOtherNoteLength )
        {
            return GetNoteLengthRelativeToQuarterNote( aNoteLength ) <= GetNoteLengthRelativeToQuarterNote( aOtherNoteLength );
        }

    }

    /** 
     * @struct Music::TimeSignature
     * @brief A struct that represents a time signature.
    */
    public struct TimeSignature
    {
        public int BeatsPerMeasure; //!< The number of beats per measure (Top part of time signature)
        public NOTE_LENGTH_BASE BaseBeat; //!< The base beat (Bottom part of time signature). For example, 3/4 would have 3 BeatsPerMeasure and a BaseBeat of NOTE_LENGTH_BASE::Q. 
    }

    /** @} */

    /*************************************************************************//** 
     * @defgroup MusicStatFunc Static Functions
     * @ingroup DocMusic
     * These are functions that can be used to help utilize the @link MusicEnums enums@endlink
     * and @link MusicStructs structs@endlink from the Music class.
     ****************************************************************************/
    /** @{ */

    /**
     * @brief Creates a new CombinedNote struct from the parameters.
     * @param[in] aMelodyVelocity The @link DefVel Note Velocities@endlink of the MelodyNote
     * @param[in] aLengths The length of the @link PITCH pitches@endlink in the MelodyNote
     * @param[in] aPitches The pitches of the MelodyNote
     * @param[in] aDrumVelocities The @link DefVel Note Velocities@endlink of the PercussionNote
     * @param[in] aDrumHits The hits of the PercussionNote
     * @param[in] aOffsetFromPrevNote The offset from the previous CombinedNote.
    */
    public static CombinedNote CreateNote( int[] aMelodyVelocities, NoteLength[] aLengths, PITCH[] aPitches, int[] aDrumVelocities, DRUM[] aDrumHits, NoteLength aOffsetFromPrevNote )
    {
        // Create the new note struct.
        CombinedNote newNote = new CombinedNote();

        // Set up the melody note.
        newNote.MusicalNote.NumPitches = aPitches.Length;
        newNote.MusicalNote.Velocities = aMelodyVelocities;
        newNote.MusicalNote.Lengths = aLengths;
        newNote.MusicalNote.Pitches = aPitches;

        // Set up the percussion note.
        newNote.Drums.NumHits = aDrumHits.Length;
        newNote.Drums.Velocities = aDrumVelocities;
        newNote.Drums.Hits = aDrumHits;
        newNote.Drums.HasHiHat = false;
        if( aDrumHits != null )
        {
            // Check if the new note has any hi hat hits.
            newNote.Drums.HasHiHat = false;
            for( int i = 0; i < aDrumHits.Length; i++ )
            {
                if( aDrumHits[i] == Music.DRUM.HIHAT_C ||
                    aDrumHits[i] == Music.DRUM.HIHAT_O ||
                    aDrumHits[i] == Music.DRUM.HIHAT_P )
                {
                    newNote.Drums.HasHiHat = true;
                }
            }
        }

        // Set the offset.
        newNote.OffsetFromPrevNote = aOffsetFromPrevNote;

        // Return the new note.
        return newNote;
    }

    /**
     * @brief Returns a string based on the given DRUM.
     * @param[in] aDrum The given DRUM
     * 
     * @see DRUM_STRING
    */
    public static string DrumToString( DRUM aDrum )
    {
        return DRUM_STRING[(int)aDrum];
    }

    /**
     * @brief Overloaded version of DrumToString( DRUM ) that takes an integer instead.
     * @param[in] aDrumIndex The index of the drum in the DRUM enum.
     * 
     * @see DRUM DRUM_STRING
    */
    public static string DrumToString( int aDrumIndex )
    {
        return DRUM_STRING[aDrumIndex];
    }

    /**
     * @brief Gets the image used to represent a @link NOTE_LENGTH_BASE note's length@endlink.
     * @param[in] aLength The @link NOTE_LENGTH_BASE note length@endlink whose image representation is being obtained.
     * @return The image representation of the given @link NOTE_LENGTH_BASE note length@endlink.
    */
    public static Sprite GetImageForNoteLength( NOTE_LENGTH_BASE aLength )
    {
        // Declare the returned Sprite variable.
        Sprite returned = null;

        // If the given length is not "None", then use the path in the NOTE_LENGTH_BASE_IMAGE_PATHS array. 
        // Otherwise, keep the returned variable set to null.
        if( aLength != NOTE_LENGTH_BASE.NONE )
        {
            returned = Resources.Load<Sprite>( NOTE_LENGTH_BASE_IMAGE_PATHS[(int)aLength] );
            Assert.IsNotNull( returned, "Could not load the note length image at the path " + NOTE_LENGTH_BASE_IMAGE_PATHS[(int)aLength] + "!" );
        }

        return returned;
    }

    /**
     * @brief Gets the percentage that a note length takes up in a measure for the given time signature.
     * @param[in] aLength the given NOTE_LENGTH_BASE that represents the length of a note.
     * @param[in] aTimeSignature The given TimeSignature
    */
    public static float GetNoteLengthRelativeToMeasure( NoteLength aLength, TimeSignature aTimeSignature )
    {
        // Relate everything to quarter notes.
        float quarterNoteLength = 1f;

        // Get the ratio of quarter note to beat.
        quarterNoteLength = GetNoteLengthRelativeToQuarterNote( new NoteLength( aTimeSignature.BaseBeat ) );

        // Get the percentage that a quarter note takes up in the measure.
        quarterNoteLength /= (float)aTimeSignature.BeatsPerMeasure;

        return GetNoteLengthRelativeToQuarterNote( aLength ) * quarterNoteLength;
    }

    /**
     * @brief Gets the @link Music::NoteLength length of a note@endlink relative to a quarter note.
     * @param[in] aNoteLength The @link Music::NoteLength length of the note@endlink being compared.
     * @return The ratio of @link Music::NoteLength the note length@endlink relative to a quarter note.
    */
    public static float GetNoteLengthRelativeToQuarterNote( NoteLength aNoteLength )
    {
        // Declare a variable to return the ratio
        float ratio = 1f;

        // Make sure there is a length.
        if( aNoteLength.BaseLength == NOTE_LENGTH_BASE.NONE )
        {
            return 0;
        }

        // Set the ratio according to the base length.
        switch( aNoteLength.BaseLength )
        {
            case NOTE_LENGTH_BASE.TS:
                ratio /= 8f;
                break;
            case NOTE_LENGTH_BASE.S:
                ratio /= 4f;
                break;
            case NOTE_LENGTH_BASE.E:
                ratio /= 2f;
                break;
            case NOTE_LENGTH_BASE.Q:
                break;
            case NOTE_LENGTH_BASE.H:
                ratio *= 2f;
                break;
            case NOTE_LENGTH_BASE.W:
                ratio *= 4f;
                break;
            default:
                break;
        }

        // Apply the dot and triplet modifiers if needed.
        if( aNoteLength.Dot )
        {
            ratio *= DOT_MODIFIER;
        }
        if( aNoteLength.Triplet )
        {
            ratio *= TRIPLET_MODIFIER;
        }

        // Return the ratio.
        return ratio;
    }

    /** 
     * @brief Returns whether or not a note corresponds to a white or black key.
     * @param[in] aPitchIndex The index of the @link PITCH pitch@endlink being checked.
     * @return True if the pitch corresponds to a black key. False otherwise.
    */
    public static bool IsPitchABlackKey( int aPitchIndex )
    {
        Assert.IsTrue( aPitchIndex >= 0 && aPitchIndex <= MAX_SUPPORTED_NOTES, 
            "Tried to find whether a pitch with index " + aPitchIndex.ToString() + " was a black key or not, but the pitch was out of range!" );
        Assert.IsTrue( aPitchIndex < (int)PITCH.REST, "Tried to see whether a rest note was a black key or not!" );

        // Get the base pitch (the pitch's place in its octave.)
        int basePitch = aPitchIndex % 12;

        // Check whether the key is a black key or not.
        if( basePitch == 1 || basePitch == 3 || basePitch == 6 || basePitch == 8 || basePitch == 10 )
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    /** 
     * @brief Overloaded function that returns whether or not a note corresponds to a white or black key.
     * @param[in] aPitch The pitch being checked.
     * @return True if the pitch corresponds to a black key. False otherwise.
    */
    public static bool IsPitchABlackKey( PITCH aPitch )
    {
        return IsPitchABlackKey( (int)aPitch );
    }

    /**
     * @brief Gives a string representing a PITCH.
     * @param[in] aNoteValue The PITCH to return a string representation of.
     * 
     * @see PITCH PITCH_STRING
    */ 
    public static string NoteToString( PITCH aNoteValue )
    {
        return NoteToString( (int)aNoteValue );
    }

    /**
     * @brief Overloaded function that gives a string representing a pitch. This function takes an integer instead of a PITCH
     * @param[in] aNoteValue The index in the PITCH enum that corresponds to a pitch to return a string representation of.
     * 
     * @see PITCH PITCH_STRING
    */
    public static string NoteToString( int aNoteValue )
    {
        int pitchIndex = aNoteValue % 12;
        int octave = aNoteValue / 12;
        if( octave == 10 && pitchIndex == 0 )
        {
            return "Rest";
        }
        return PITCH_STRING[pitchIndex] + octave.ToString();
    }

    /**
     * @brief Returns a 4/4 time signature.
    */
    public static TimeSignature TIME_SIGNATURE_4_4()
    {
        TimeSignature returned;
        returned.BeatsPerMeasure = 4;
        returned.BaseBeat = NOTE_LENGTH_BASE.Q;
        return returned;
    }

    /**
     * @brief Returns a 3/4 time signature.
    */
    public static TimeSignature TIME_SIGNATURE_3_4()
    {
        TimeSignature returned;
        returned.BeatsPerMeasure = 3;
        returned.BaseBeat = NOTE_LENGTH_BASE.Q;
        return returned;
    }

    /**
     * @brief Returns a 6/8 time signature.
    */
    public static TimeSignature TIME_SIGNATURE_6_8()
    {
        TimeSignature returned;
        returned.BeatsPerMeasure = 6;
        returned.BaseBeat = NOTE_LENGTH_BASE.E;
        return returned;
    }
    /** @} */
}
