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

    public static string[] PITCH_STRING = { "C", "CS", "D", "DS", "E", "F", "FS", "G", "GS", "A", "AS", "B" };
    public static short NUM_NOTES_IN_OCTAVE = 12;
    public static float SEMITONE_FACTOR = 1.059463f;
    public static int MAX_SUPPORTED_NOTES = 120;
    public enum INSTRUMENT_TYPE
    {
        PIANO,
        MARIMBA
    };

    public struct Note
    {
        public int Length;
        public PITCH Pitch;
        public int Velocity;
        public int OffsetSamples;
    }

    public static Note CreateNote( int aLength, PITCH aPitch, int aVelocity, int aOffsetSamples )
    {
        Note newNote;
        newNote.Length = aLength;
        newNote.Pitch = aPitch;
        newNote.Velocity = aVelocity;
        newNote.OffsetSamples = aOffsetSamples;
        return newNote;
    }

    public enum NOTE_LENGTH
    {
        T,
        S,
        E,
        Q,
        H,
        W
    };

    public static NOTE_LENGTH[] DemoSongNoteLengths =
        {
            NOTE_LENGTH.Q, NOTE_LENGTH.E, NOTE_LENGTH.E, NOTE_LENGTH.S, NOTE_LENGTH.S, NOTE_LENGTH.S, NOTE_LENGTH.S, NOTE_LENGTH.E, NOTE_LENGTH.E, 
            NOTE_LENGTH.Q, NOTE_LENGTH.Q, NOTE_LENGTH.E, NOTE_LENGTH.E, NOTE_LENGTH.E, NOTE_LENGTH.E, 
            NOTE_LENGTH.Q, NOTE_LENGTH.Q, NOTE_LENGTH.Q, NOTE_LENGTH.Q, 
            NOTE_LENGTH.E, NOTE_LENGTH.E, NOTE_LENGTH.E, NOTE_LENGTH.E, NOTE_LENGTH.W
        };

    public static PITCH[] DemoSongPitches =
    {
        PITCH.D4, PITCH.REST, PITCH.A4, PITCH.GS4, PITCH.A4, PITCH.GS4, PITCH.A4, PITCH.GS4, PITCH.A4,
        PITCH.F4, PITCH.D4, PITCH.REST, PITCH.D4, PITCH.F4, PITCH.A4,
        PITCH.AS4, PITCH.F4, PITCH.AS4, PITCH.C5,
        PITCH.A4, PITCH.AS4, PITCH.A4, PITCH.AS4, PITCH.A4, 
    };

    public static Queue<Note> GetDemoSong( int aBPM )
    {
        Queue<Note> returned = new Queue<Note>();
        int offset = 0;
        int samp = 0;

        for( int i = 0; i < 24; i++ )
        {
            samp = GetNoteLengthInSamples( aBPM, 44100, DemoSongNoteLengths[i] );
            returned.Enqueue( CreateNote( samp, DemoSongPitches[i], 70, offset ) );
            if( DemoSongPitches[i] != PITCH.REST )
            {
                PITCH lower = (PITCH)( (int)DemoSongPitches[i] - 12 );
                returned.Enqueue( CreateNote( samp, lower, 70, offset ) );
            }

            offset += samp;
        }

        return returned;
    }

    public static int GetNoteLengthInSamples( int aBPM, int aSampleRate, NOTE_LENGTH aNoteLength )
    {
        float beatsPerSecond = (float)aBPM / 60f;
        float numSamplesPerBeat = 2f * ( 1f / beatsPerSecond ) * (float)aSampleRate;

        switch( aNoteLength )
        {
            case NOTE_LENGTH.T:
                return (int)( numSamplesPerBeat / 8f );
            case NOTE_LENGTH.S:
                return (int)( numSamplesPerBeat / 4f );
            case NOTE_LENGTH.Q:
                return (int)( numSamplesPerBeat );
            case NOTE_LENGTH.E:
                return (int)( numSamplesPerBeat / 2f );
            case NOTE_LENGTH.H:
                return (int)( numSamplesPerBeat * 2f );
            case NOTE_LENGTH.W:
            default:
                return (int)( numSamplesPerBeat * 4f );
        }
    }


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

    public static string NoteToString( PITCH aNoteValue )
    {
        return NoteToString( (int)aNoteValue );
    }

    public static string NoteToString( int aNoteValue )
    {
        int pitchIndex = aNoteValue % 12;
        int octave = aNoteValue / 12;
        return PITCH_STRING[pitchIndex] + octave.ToString();
    }
}
