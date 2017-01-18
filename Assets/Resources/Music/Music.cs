//---------------------------------------------------------------------------- 
// /Resources/Music/Music.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: A container for constants and static functions that are needed 
//              for implementing music-related functions, objects, and 
//              classes. 
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music {

    //---------------------------------------------------------------------------- 
    // Constants
    //---------------------------------------------------------------------------- 

    public static string[] PITCH_STRING = { "C", "CS", "D", "DS", "E", "F", "FS", "G", "GS", "A", "AS", "B" };
    public static short NUM_NOTES_IN_OCTAVE = 12;
    public static float SEMITONE_FACTOR = 1.059463f;
    public static int MAX_SUPPORTED_NOTES = 120;

    //---------------------------------------------------------------------------- 
    // Types
    //---------------------------------------------------------------------------- 

    // The possible instrument types.
    public enum INSTRUMENT_TYPE
    {
        PIANO,
        MARIMBA,
        DRUM_KIT
    };

    // An abstract musical note.
    public struct Note
    {
        public int Velocity;
        public PITCH[] Pitches;
        public NOTE_LENGTH Length;
        public NOTE_LENGTH Offset;
    }

    // The length of a note.
    public enum NOTE_LENGTH
    {
        T, // 32nd note.
        D_T, // Dotted 32nd note.
        S, // 16th note.
        D_S, // Dotted 16th note.
        E, // Eighth note.
        D_E, // Dotted eighth note.
        Q, // Quarter note.
        D_Q, // Dotted quarter note.
        H, // Half note.
        D_H, // Dotted half note.
        W, // Whole note.
        D_W, // Dotted whole note.
        NONE // Used when there are no offsets (i.e. chords)
    };

    // The possible pitches that can be played.
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

    // A container that represents a time signature.
    public struct TimeSignature
    {
        public int BeatsPerMeasure;
        public NOTE_LENGTH BaseBeat;
    } 

    //---------------------------------------------------------------------------- 
    // Static Functions
    //---------------------------------------------------------------------------- 
    public static float GetNoteLengthRelativeToMeasure( NOTE_LENGTH aLength, TimeSignature aTimeSignature )
    {
        float quarterNoteLength = 1f;
        switch( aTimeSignature.BaseBeat )
        {
            case NOTE_LENGTH.E:
                quarterNoteLength *= 2f;
                break;
            default:
                break;
                
        }

        quarterNoteLength /= (float)aTimeSignature.BeatsPerMeasure;

        switch( aLength )
        {
            case Music.NOTE_LENGTH.T:
                return quarterNoteLength / 8f;
            case Music.NOTE_LENGTH.D_T:
                return 1.5f * quarterNoteLength / 8f;
            case Music.NOTE_LENGTH.S:
                return quarterNoteLength / 4f;
            case Music.NOTE_LENGTH.D_S:
                return 1.5f * quarterNoteLength / 4f;
            case Music.NOTE_LENGTH.E:
                return quarterNoteLength / 2f;
            case Music.NOTE_LENGTH.D_E:
                return 1.5f * quarterNoteLength / 2f;
            case Music.NOTE_LENGTH.Q:
                return quarterNoteLength;
            case Music.NOTE_LENGTH.D_Q:
                return quarterNoteLength * 1.5f;
            case Music.NOTE_LENGTH.H:
                return quarterNoteLength * 2f;
            case Music.NOTE_LENGTH.D_H:
                return quarterNoteLength * 3f;
            case Music.NOTE_LENGTH.W:
                return quarterNoteLength * 4f;
            case Music.NOTE_LENGTH.D_W:
                return quarterNoteLength * 6f;
            default:
                return 0;
        }
    }

    // Converts a pitch to a string.
    public static string NoteToString( PITCH aNoteValue )
    {
        return NoteToString( (int)aNoteValue );
    }

    // Overloaded function that converts a pitch to a string
    public static string NoteToString( int aNoteValue )
    {
        int pitchIndex = aNoteValue % 12;
        int octave = aNoteValue / 12;
        return PITCH_STRING[pitchIndex] + octave.ToString();
    }

    // Returns a 4/4 time signature.
    public static TimeSignature TIME_SIGNATURE_4_4()
    {
        TimeSignature returned;
        returned.BeatsPerMeasure = 4;
        returned.BaseBeat = NOTE_LENGTH.Q;
        return returned;
    }

    // Returns a 3/4 time signature.
    public static TimeSignature TIME_SIGNATURE_3_4()
    {
        TimeSignature returned;
        returned.BeatsPerMeasure = 3;
        returned.BaseBeat = NOTE_LENGTH.Q;
        return returned;
    }

    // Returns a 6/8 time signature.
    public static TimeSignature TIME_SIGNATURE_6_8()
    {
        TimeSignature returned;
        returned.BeatsPerMeasure = 6;
        returned.BaseBeat = NOTE_LENGTH.E;
        return returned;
    }

}
