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
    private int                        mNumSongs = 0; // The number of loaded songs.
    private List<Song>                 mSongs = null; // The loaded songs.

    //---------------------------------------------------------------------------- 
    // Unity Functions
    //---------------------------------------------------------------------------- 
    private void Awake()
    {
        // Initialize the list of songs.
        mSongs = new List<Song>();

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
        if( aSong == null )
        {
            Assert.IsNotNull( aSong, "Tried to add a song that didn't exist!" );
            return;
        }

        mSongs.Add( aSong );
    }

    // Gets the number of loaded songs.
    // OUT: The number of loaded songs.
    public int GetNumberOfSongs()
    {
        return mNumSongs;
    }

    // Gets a specific song.
    // IN: aIndex The index of the song to get.
    // OUT: The song found at the given index.
    public Song GetSong( int aIndex )
    {
        // Sanity Check
        if( aIndex >= mSongs.Count )
        {
            Assert.IsTrue( aIndex < mSongs.Count, "Tried to get a song that didn't exist!" );
            return null;
        }

        return mSongs[aIndex];
    }

    // Gets a specific song name.
    // IN: aIndex The index of the song whose name is retrieved.
    // OUT: The name of the specified song.
    public string GetSongName( int aIndex )
    {
        // Sanity Check
        if( aIndex >= mSongs.Count )
        {
            Assert.IsTrue( aIndex < mSongs.Count, "Tried to get the name of a song that didn't exist!" );
            return null;
        }

        return mSongs[aIndex].GetName();
    }

    // Gets all of the names of the loaded songs.
    // OUT: A list of all of the songs' names.
    public List<string> GetSongNames()
    {
        // Initialize the returned list.
        List<string> returned = new List<string>();

        // Add all of the song names to the returned list.
        foreach( Song song in mSongs )
        {
            returned.Add( song.GetName() );
        }

        // Return the song names.
        return returned;
    }

    // Gets all of the songs.
    // OUT: The loaded songs.
    public List<Song> GetSongs()
    {
        // Sanity Check
        if( mSongs == null )
        {
            Assert.IsNotNull( mSongs, "Tried to retrieve songs from the song manager before the manager was initialized!" );
            return null;
        }

        return mSongs;
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
            if( file.Name.StartsWith( "SONG_" ) && file.Name.EndsWith( ".song" ) )
            {
                mSongs.Add( new Song( file.FullName ) );
                mNumSongs++;
            }
        }
    }

    // Removes a song from the SongManager.
    // IN: aIndex The index of the song to remove.
    public void RemoveSong( int aIndex )
    {
        // Sanity Check
        if( aIndex >= mSongs.Count )
        {
            Assert.IsTrue( aIndex < mSongs.Count, "Tried to get a song that didn't exist!" );
            return;
        }

        mSongs.RemoveAt( aIndex );
    }
}
