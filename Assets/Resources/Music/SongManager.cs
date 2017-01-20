//---------------------------------------------------------------------------- 
// /Resources/Music/SongManager.cs
// Unnamed VR Virtual Piano Project 
// Created for the classes EECS 541 & 542 at the University of Kansas
// Team: Dylan Egnoske, James Ballard, Justin Arnspiger, Quinten Johnson 
// 
// Description: Object that handles loading and playing songs. Meant to only
//              be used as a child of the Virtual Instrument Manager.
//---------------------------------------------------------------------------- 
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class SongManagerClass : MonoBehaviour {

    //---------------------------------------------------------------------------- 
    // Private Variables
    //---------------------------------------------------------------------------- 
    private Dictionary<string,Song>    mCombinedSongs = null; // The loaded songs that have drums and melody.
    private Dictionary<string,Song>    mDrumLoops = null; // The loaded drum loops.
    private Dictionary<string,Song>    mMelodies = null; // The loaded songs.
    private int                        mNumCombinedSongs = 0; // The number of loaded songs that have drums and melody.
    private int                        mNumDrumLoops = 0; // The number of loaded drum loops.
    private int                        mNumMelodies = 0; // The number of loaded melodies.

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 
    private void Awake()
    {
        // Initialize the dictionaries of songs.
        mCombinedSongs = new Dictionary<string,Song>();
        mDrumLoops = new Dictionary<string, Song>();
        mMelodies = new Dictionary<string, Song>();

        // Load the songs in the folder.
        LoadSongs();
    }

    //---------------------------------------------------------------------------- 
    // Public Functions
    //---------------------------------------------------------------------------- 

    // Adds a song to the SongManager
    // IN: aSong A new song.
    public void AddSong( Song aSong )
    {
        // Sanity Check
        Assert.IsNotNull( aSong, "Tried to add a song that didn't exist!" );

        // Create the song entry.
        switch( aSong.GetSongType() )
        {
            case Song.SongType.Melody:
                mMelodies.Add( aSong.GetName(), aSong );
                break;
            case Song.SongType.DrumLoop:
                mDrumLoops.Add( aSong.GetName(), aSong );
                mNumDrumLoops++;
                break;
            case Song.SongType.CombinedMelodyAndPercussion:
                mCombinedSongs.Add( aSong.GetName(), aSong );
                mNumCombinedSongs++;
                break;
        }
    }

    // Returns the names of the loaded drum loops.
    public List<string> GetDrumLoopNames()
    {
        List<string> loopNames = new List<string>();
        foreach( string name in mDrumLoops.Keys )
        {
            loopNames.Add( name );
        }

        return loopNames;
    }

    // Gets the loaded drum loops.
    // OUT: A List of the loaded drum loops.
    public List<Song> GetDrumLoops()
    {
        List<Song> returned = new List<Song>();

        foreach( Song loop in mDrumLoops.Values )
        {
            returned.Add( loop );
        }

        return returned;
    }

    // Gets the number of loaded songs that have both drums and melody.
    // OUT: The number of loaded songs that have both drums and melody.
    public int GetNumberOfCombinedSongs()
    {
        return mNumCombinedSongs;
    }

    // Gets the number of loaded drum loops.
    // OUT: The number of loaded drum loops.
    public int GetNumberOfDrumLoops()
    {
        return mNumDrumLoops;
    }

    // Gets the number of loaded melodies.
    // OUT: The number of loaded melodies.
    public int GetNumberOfMelodies()
    {
        return mNumMelodies;
    }

    // Gets a specific song.
    // IN: aIndex The index of the song to get.
    // OUT: The song found at the given index.
    public Song GetSong( string aSongName )
    {
        // Sanity Check
        Assert.IsTrue( mMelodies.ContainsKey( aSongName ) || mDrumLoops.ContainsKey( aSongName ) || mCombinedSongs.ContainsKey( aSongName ),
            "Tried to get the song " + aSongName + " which does not exist!" );

        // Get the song.
        if( mCombinedSongs.ContainsKey( aSongName ) )
        {
            return mCombinedSongs[aSongName];
        }
        else if( mDrumLoops.ContainsKey( aSongName ) )
        {
            return mDrumLoops[aSongName];
        }
        else
        {
            return mMelodies[aSongName];
        }
    }

    // Gets all of the names of the loaded songs.
    // OUT: A list of all of the songs' names.
    public List<string> GetSongNames()
    {
        // Initialize the returned list.
        List<string> returned = new List<string>();

        // Get the song names.
        Dictionary<string,Song>.KeyCollection combinedSongNames = mCombinedSongs.Keys;
        Dictionary<string,Song>.KeyCollection melodyNames = mMelodies.Keys;

        // Get the combined song names.
        foreach( string name in combinedSongNames )
        {
            returned.Add( name );
        }

        // Get the melody names.
        foreach( string name in melodyNames )
        {
            returned.Add( name );
        }

        // Return the list of song names.
        return returned;
    }

    // Gets all of the songs.
    // OUT: The loaded songs.
    public List<Song> GetSongs()
    {
        List<Song> returned = new List<Song>();
        
        // Get the songs.
        Dictionary<string,Song>.ValueCollection combinedSongs = mCombinedSongs.Values;
        Dictionary<string,Song>.ValueCollection melodies = mMelodies.Values;

        // Get the combined song names.
        foreach( Song song in combinedSongs )
        {
            returned.Add( song );
        }

        // Get the melody names.
        foreach( Song song in melodies )
        {
            returned.Add( song );
        }

        // return the songs.
        return returned;
    }

    // Loads songs in the folder "APP_PATH/Resources/Music/Songs/".
    // Songs have the format "SONG_[songname].song
    public void LoadSongs()
    {
        // Get the info for the files in the folder.
        DirectoryInfo info = new DirectoryInfo( Application.dataPath + "/Resources/Music/Songs/" );

        // Iterate through each file in the folder and load a new song if the filename matches 
        // the format.
        foreach( FileInfo file in info.GetFiles() )
        {
            if( file.Name.EndsWith( ".song" ) )
            {
                // Load the new song.
                Song newSong = new Song( file.FullName );

                // Add the new song to the proper container.
                if( file.Name.StartsWith( "SONG_" ) )
                {
                    mCombinedSongs.Add( newSong.GetName(), newSong );
                    mNumCombinedSongs++;
                }
                else if( file.Name.StartsWith( "MELODY_" ) )
                {
                    mMelodies.Add( newSong.GetName(), newSong );
                    mNumMelodies++;
                }
                else if( file.Name.StartsWith( "DRUMLOOP" ) )
                {
                    mDrumLoops.Add( newSong.GetName(), newSong );
                    mNumDrumLoops++;
                }
            }

        }
    }
}
