using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * @class SongManagerClass
 * @brief C# class that handles loading and playing Songs. 
 * 
 * @note Meant to only be used as a child of the @link VIM Virtual Instrument Manager@endlink.
 * 
 * @see VirtualInstrumentManager::SongManager
*/
public class SongManagerClass 
{
    /*************************************************************************//** 
     * @defgroup SMPrivVar Private Variables
     * @ingroup DocSM
     * Variables that are used internally by the SongManagerClass.
     * @{
    *****************************************************************************/
    private Dictionary<string,Song>    mCombinedSongs = null; //!< The loaded songs that @link DocSongCombined have both percussion and melody@endlink.
    private Dictionary<string,Song>    mDrumLoops = null; //!< The loaded @link DocSongDrumLoop drum loops@endlink.
    private Dictionary<string,Song>    mMelodies = null; //!< The loaded @link DocSongMelody melodies@endlink.
    private int                        mNumCombinedSongs = 0; //!< The number of loaded songs that @link DocSongCombined have both percussion and melody@endlink.
    private int                        mNumDrumLoops = 0; //!< The number of loaded @link DocSongDrumLoop drum loops@endlink.
    private int                        mNumMelodies = 0; //!< The number of loaded @link DocSongMelody melodies@endlink.

    /*************************************************************************//** 
     * @}
     * @defgroup SMConstruct Constructors
     * @ingroup DocSM
     * Constructors to create a SongManagerClass. Should only be used in the @link VirtualInstrumentManager::Awake awake function@endlink of the @link VIM VirtualInstrumentManager@endlink.
     * @{
    *****************************************************************************/

    /**
     * @brief Initializes the SongManagerClass by creating its lists of songs.
    */
    public SongManagerClass()
    {
        // Initialize the dictionaries of songs.
        mCombinedSongs = new Dictionary<string, Song>();
        mDrumLoops = new Dictionary<string, Song>();
        mMelodies = new Dictionary<string, Song>();

        // Load the songs in the folder.
        LoadSongs();
    }

    /*************************************************************************//** 
     * @}
     * @defgroup SMPubFunc Public Functions
     * @ingroup DocSM
     * Functions that allow other classes to interact with the SongManagerClass.
     * @{
    *****************************************************************************/

    /**
     * @brief Adds a Song to the SongManager
     * @param[in] aSong A new Song.
    */
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

    /**
     * @brief Gets the names of the loaded @link DocSongCombined songs that have both percussion and melody@endlink.
     * @return A list of the names of the loaded @link DocSongCombined songs that have both percussion and melody@endlink.
    */
    public List<string> GetCombinedSongNames()
    {
        List<string> loopNames = new List<string>();
        foreach( string name in mCombinedSongs.Keys )
        {
            loopNames.Add( name );
        }

        return loopNames;
    }

    /**
     * @brief Gets the loaded @link DocSongCombined songs that have both percussion and melody@endlink.
     * @return A list of the loaded @link DocSongCombined songs that have both percussion and melody@endlink.
    */
    public List<Song> GetCombinedSongs()
    {
        List<Song> returned = new List<Song>();

        foreach( Song loop in mCombinedSongs.Values )
        {
            returned.Add( loop );
        }

        return returned;
    }

    /**
     * @brief Gets the names of the loaded @link DocSongDrumLoop drum loops@endlink.
     * @return A list of the names of the loaded @link DocSongDrumLoop drum loops@endlink.
    */
    public List<string> GetDrumLoopNames()
    {
        List<string> loopNames = new List<string>();
        foreach( string name in mDrumLoops.Keys )
        {
            loopNames.Add( name );
        }

        return loopNames;
    }

    /**
     * @brief Gets the loaded @link DocSongDrumLoop drum loops@endlink.
     * @return A list of the loaded @link DocSongDrumLoop drum loops@endlink.
    */
    public List<Song> GetDrumLoops()
    {
        List<Song> returned = new List<Song>();

        foreach( Song loop in mDrumLoops.Values )
        {
            returned.Add( loop );
        }

        return returned;
    }

    /**
     * @brief Gets the names of the loaded @link DocSongMelody melodies@endlink.
     * @return A list of the names of the loaded @link DocSongMelody melodies@endlink.
    */
    public List<string> GetMelodyNames()
    {
        List<string> loopNames = new List<string>();
        foreach( string name in mMelodies.Keys )
        {
            loopNames.Add( name );
        }

        return loopNames;
    }

    /**
     * @brief Gets the loaded @link DocSongMelody melodies@endlink.
     * @return A list of the loaded @link DocSongMelody melodies@endlink.
    */
    public List<Song> GetMelodies()
    {
        List<Song> returned = new List<Song>();

        foreach( Song loop in mMelodies.Values )
        {
            returned.Add( loop );
        }

        return returned;
    }

    /**
     * @brief Gets the number of loaded Songs that @link DocSongCombined have both drums and melody@endlink.
     * @return The number of loaded songs that @link DocSongCombined have both drums and melody@endlink.
    */
    public int GetNumberOfCombinedSongs()
    {
        return mNumCombinedSongs;
    }

    /**
     * @brief Gets the number of loaded @link DocSongDrumLoop drum loops@endlink.
     * @return The number of loaded @link DocSongDrumLoop drum loops@endlink.
    */
    public int GetNumberOfDrumLoops()
    {
        return mNumDrumLoops;
    }

    /** 
     * @brief Gets the number of loaded @link DocSongMelody melodies@endlink.
     * @return The number of loaded @link DocSongMelody melodies@endlink.
    */
    public int GetNumberOfMelodies()
    {
        return mNumMelodies;
    }

    /** 
     * @brief Gets a specific Song.
     * @param[in] aSongName The name of the Song to get.
     * @return The Song with the given name. 
     * 
     * @note Asserts if the name is not found, or returns null if compiled as a non-development build.
    */
    public Song GetSongByName( string aSongName )
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
        else if( mMelodies.ContainsKey( aSongName ) )
        {
            return mMelodies[aSongName];
        }
        else
        {
            return null;
        }
    }

    /**
     * @brief Gets the names of the loaded Songs.
     * @return A list of all of the Songs\' names.
    */
    public List<string> GetSongNames()
    {
        // Initialize the returned list.
        List<string> returned = new List<string>();

        // Get the song names.
        Dictionary<string,Song>.KeyCollection combinedSongNames = mCombinedSongs.Keys;
        Dictionary<string,Song>.KeyCollection drumLoopNames = mDrumLoops.Keys;
        Dictionary<string,Song>.KeyCollection melodyNames = mMelodies.Keys;

        // Get the combined song names.
        foreach( string name in combinedSongNames )
        {
            returned.Add( name );
        }

        // Get all of the drum loop names.
        foreach( string name in drumLoopNames )
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

    /** 
     * @brief Gets all of the Songs.
     * @return A list of all of the loaded Songs.
    */
    public List<Song> GetSongs()
    {
        List<Song> returned = new List<Song>();

        // Get the songs.
        Dictionary<string,Song>.ValueCollection combinedSongs = mCombinedSongs.Values;
        Dictionary<string,Song>.ValueCollection drumLoops = mDrumLoops.Values;
        Dictionary<string,Song>.ValueCollection melodies = mMelodies.Values;

        // Get the combined songs.
        foreach( Song song in combinedSongs )
        {
            returned.Add( song );
        }

        // Get all of the drum loops
        foreach( Song song in drumLoops )
        {
            returned.Add( song );
        }

        // Get the all of the melodies.
        foreach( Song song in melodies )
        {
            returned.Add( song );
        }

        // Return the songs.
        return returned;
    }

    /*************************************************************************//** 
     * @}
     * @defgroup SMPrivFunc Private Functions
     * @ingroup DocSM
     * Functions that are used internally by the SongManagerClass.
     * @{
    *****************************************************************************/

    /**
     * @brief Loads Songs in the folder "APP_PATH/StreamingAssets/Audio/Songs/".
     * @see @link DocSongFileFormat Song File Format@endlink Song::LoadSongFromFile
    */
    private void LoadSongs()
    {
        // Get the info for the files in the folder.
        DirectoryInfo info = new DirectoryInfo( Song.SONG_FILE_PATH );

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
    /** @} */
}
