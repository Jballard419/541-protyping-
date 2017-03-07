using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * @class Lesson
 * @brief The base class that all types of lessons will inheirit.
 * 
 * This class provides a common base for Learn, Practice, and Recital Modes
*/
public class Lesson
{
    private int mBPM = 120; //!< The BPM of the lesson. This can vary in Practice mode, but is dictated by the Song in Recital mode.
    private int mNumKeyHits = 0; //!< The number of key hits in the lesson.
    private KeyHit[] mKeyHits = null; //!< The key hits in the lesson
    private Music.PITCH[] mKeyRange = null; //!< The range of keys that should be shown for the song.
    private Song mSong = null; //!< The song featured in the lesson.

    /**
     * @struct KeyHit
     * @brief A struct that contains the keys to hit at a specific point in time.
    */
    public struct KeyHit
    {
        public float TimeSinceStart; //!< The time since the start of the song that the keys need to be hit.
        public int NumKeys; //!< The number of keys in the key hit. All of the keys must be hit in order for it to count.
        public Music.PITCH[] Keys; //!< The keys that must be hit.

        /**
         * @brief Constructs a new key hit.
        */
        public KeyHit( float aTimeSinceStart = 0, int aNumKeys = 0, Music.PITCH[] aKeys = null )
        {
            TimeSinceStart = aTimeSinceStart;
            NumKeys = aNumKeys;
            Keys = aKeys;
        }
    };

    /**
     * @brief Default Constructor
     * @param[in] aSong The song that the lesson is based on.
    */
    public Lesson( Song aSong )
    {
        // Set the song member variable and the BPM.
        mSong = aSong;
        mBPM = mSong.GetBPM();

        // Get the range of loaded keys.
        mKeyRange = new Music.PITCH[2];
        mKeyRange[0] = (Music.PITCH)( Mathf.Max( (int)mSong.GetLowestPitch() - 4, (int)Music.PITCH.B0 ) );
        mKeyRange[1] = (Music.PITCH)( Mathf.Min( (int)mSong.GetHighestPitch() + 4, (int)Music.PITCH.B9 ) );

        // Convert the song into a lesson.
        ConvertSongIntoLesson();
    }

    /**
     * @brief Overloaded constructor that allows for specifying a BPM
     * @param[in] aSong The song that the lesson is based on.
     * @param[in] aBPM The BPM that the lesson will be at.
    */
    public Lesson( Song aSong, int aBPM )
    {
        // Set the song member variable and the BPM.
        mSong = aSong;
        mBPM = aBPM;
        mSong.SetBPM( aBPM );

        // Get the range of loaded keys.
        mKeyRange = new Music.PITCH[2];
        mKeyRange[0] = (Music.PITCH)( Mathf.Max( (int)mSong.GetLowestPitch() - 4, (int)Music.PITCH.B0 ) );
        mKeyRange[1] = (Music.PITCH)( Mathf.Min( (int)mSong.GetHighestPitch() + 4, (int)Music.PITCH.B9 ) );

        // Convert the song into a lesson.
        ConvertSongIntoLesson();
    }

    /**
     * @brief Default destructor
    */
    ~Lesson()
    {
        mKeyHits = null;
        mSong = null;
        mKeyRange = null;
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    /**
     * @brief Gets the lowest and highest keys that should be loaded.
     * @return The lowest and highest keys that should be loaded.
    */
    public Music.PITCH[] GetKeyRange()
    {
        return mKeyRange;
    }

    /**
     * @brief Gets the key hits for the lesson.
     * @return The key hits for the lesson as an array.
    */
    public KeyHit[] GetAllKeyHits()
    {
        return mKeyHits;
    }

    /**
     * @brief Gets a single key hit.
     * @param[in] aKeyHitIndex The index of the key hit to get.
     * @return The key hit specified by the index.
    */
    public KeyHit GetKeyHit( int aKeyHitIndex )
    {
        Assert.IsNotNull( mKeyHits, "Tried to get key hits from a lesson that wasn't initialized!" );
        Assert.IsTrue( aKeyHitIndex < mNumKeyHits, "Tried to get a key hit with an index that was greater than the number of key hits in the lesson!" );
        return mKeyHits[aKeyHitIndex];
    }

    /**
     * @brief Gets the number of key hits in the lesson.
     * @return The number of key hits in the lesson.
    */
    public int GetNumKeyHits()
    {
        return mNumKeyHits;
    }

    /**
     * @brief Gets the song that the lesson is based on.
     * @return The song that the lesson is based on.
    */
    public Song GetSong()
    {
        return mSong;
    }

    /**
     * @brief Sets a new BPM for the lesson.
     * @param[in] aBPM The new BPM for the lesson.
     * 
     * @note Try not to use this too much since (as of now, might be fixed later) it needs to reload the key hits.
    */
    public void SetBPM( int aBPM )
    {
        mBPM = aBPM;
        mSong.SetBPM( aBPM );
        mKeyHits = null;
        ConvertSongIntoLesson();
    }
    
    /**
     * @brief Goes through the song and figures out the key hits.
    */
    private void ConvertSongIntoLesson()
    {
        // Initialize variables to keep track of the notes.
        List<KeyHit> keyHits = new List<KeyHit>();
        List<Music.CombinedNote> allNotes = mSong.GetAllNotes();
        Music.TimeSignature timeSig = mSong.GetTimeSignature();
        int numKeysInHit = 0;
        int numNotes = mSong.GetNumNotes();
        float time = 0f;

        // Go through all of the notes and add key hits based on them if applicable.
        for( int i = 0; i < numNotes; i++ )
        {
            // Get the note.
            Music.CombinedNote note = mSong.GetNote( i );

            // Update the time.
            time += Song.GetNoteLengthInSeconds( mBPM, note.OffsetFromPrevNote, timeSig );

            // See if the note needs a key hit created for it.
            if( note.NumPitches != 0 )
            {
                if( note.MusicalNote.Pitches[0] != Music.PITCH.REST )
                {
                    // Create the new key hit and add it to the list.
                    keyHits.Add( new KeyHit( time, note.MusicalNote.NumPitches, note.MusicalNote.Pitches ) );

                    // Update the number of key hits.
                    mNumKeyHits++;
                }
            }
        }

        // Set the member variable.
        mKeyHits = keyHits.ToArray();
    }

}
