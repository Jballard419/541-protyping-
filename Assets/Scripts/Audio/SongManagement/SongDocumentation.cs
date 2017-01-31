/** 
 * @defgroup SongGroup Songs
 * @ingroup Audio
 * @brief The backend for songs and lessons from an audio perspective.
 * @{
 * @section DocSongTerminology Terminology
 * @subsection DocSongMelody Melody
 * A @link Song::SongType type@endlink of Song that contains only @link Music::PITCH pitches@endlink. @see VirtualInstrumentManager::OnPlaySongEvent
 * @subsection DocSongDrumLoop Drum Loop
 * A @link Song::SongType type@endlink of Song that contains only @link Music::DRUM drums@endlink. These are looped when played. @see VirtualInstrumentManager::OnPlayDrumLoopEvent
 * @subsection DocSongCombined Combined
 * A @link Song::SongType type@endlink of Song that has both @link Music::PITCH pitches@endlink and @link Music::DRUM drums@endlink. @see VirtualInstrumentManager::OnPlaySongEvent
 * 
 * @section DocSongFileFormat Song File Format
 * There are @link Song::SongType three types@endlink of Songs which can be stored in a file. Which type the Song is will affect @link Song::WriteSongToFile how it is saved@endlink.
 * For all @link Song::SongType song types@endlink, the files are saved in Assets/StreamingAssets/Songs/.
 * @note The values from @link DocMusicEnums Music enums@endlink are typecast to integers for saving. Ex: @link Music::PITCH.C4 Music.PITCH.C4@endlink is saved as 48. 
 * 
 * @subsection DocSongFileFormatMelody File Format for a Melody
 * If the song is a @link DocSongMelody melody@endlink, then the file is saved as \"MELODY_@link Song::mName songName@endlink\.song\"\.
 * The format of the file is
 * @li First line: @link Song::mName "Name of song"@endlink 
 * @li Second Line: \"@link Song::SongType songType@endlink;@link DefBPM defaultBPM@endlink;@link Music::TimeSignature.BeatsPerMeasure beatsPerMeasure@endlink;@link Music::TimeSignature.BaseBeat baseBeat@endlink\"
 * @li Rest of lines: \"@link Music::MelodyNote.Pitches pitch0,pitch1,...,pitchN@endlink;@link Music::MelodyNote.Length noteLength@endlink;@link Music::CombinedNote.OffsetFromPrevNote offsetFromPrevNote@endlink;@link Music::MelodyNote.Velocity melodyVelocity@endlink\"
 * @li Example from \"MELODY_Melody Example.song\": 
@verbatim 
Melody Example
1;111;4;6
36,40,43;10;12;100
41;2;5;70
52;2;4;70
34;2;4;70
36,40;6;4;70
36,43;6;4;70
36,41;6;6;70
52,57,60,48;10;4;70
55,52,48,36;10;6;70
@endverbatim
 * 
 * @subsection DocSongFileFormatDrumLoop File Format for a Drum Loop
 * If the song is a @link DocSongDrumLoop drum loop@endlink, then the file is saved as \"DRUMLOOP_@link Song::mName songName@endlink\.song\"\.
 * The format of the file is:
 * @li First line: @link Song::mName "Name of song"@endlink 
 * @li Second Line: \"@link Song::SongType songType@endlink;@link DefBPM defaultBPM@endlink;@link Music::TimeSignature.BeatsPerMeasure beatsPerMeasure@endlink;@link Music::TimeSignature.BaseBeat baseBeat@endlink\"
 * @li Rest of lines: \"@link Music::PercussionNote.Hits drum0,drum1,...,drumN@endlink;@link Music::CombinedNote.OffsetFromPrevNote offsetFromPrevNote@endlink;@link Music::PercussionNote.Velocity drumVelocity@endlink\"
 * @li Example from \"DRUMLOOP_Drum Loop Example.song\": 
@verbatim
Drum Loop Example
2;120;4;6
0,6;4;100
0;4;100
6,2,0;4;100
0;4;100
10,0;4;100
0;2;100
1;2;100
0;2;100
6,2;2;100
0;4;100
@endverbatim
 * 
 * @subsection DocSongFileFormatCombined File Format for a Combined Song
 * If the song @link DocSongCombined has both melody and percussion@endlink, then it will be saved as \"SONG_@link Song::mName songName@endlink\.song\"\.
 * The format of the file is:
 * @li First line: @link Song::mName "Name of song"@endlink 
 * @li Second Line: \"@link Song::SongType songType@endlink;@link DefBPM defaultBPM@endlink;@link Music::TimeSignature.BeatsPerMeasure beatsPerMeasure@endlink;@link Music::TimeSignature.BaseBeat baseBeat@endlink\"
 * @li Rest of lines: \"@link Music::MelodyNote.Pitches pitch0,pitch1,...,pitchN@endlink\|@link Music::PercussionNote.Hits drum0,drum1,...,drumN@endlink;@link Music::MelodyNote.Length noteLength@endlink;@link Music::CombinedNote.OffsetFromPrevNote offsetFromPrevNote@endlink;@link Music::MelodyNote.Velocity melodyVelocity@endlink\|@link Music::PercussionNote.Velocity drumVelocity@endlink\"
 * @note If @link Music::PITCH pitches@endlink or @link Music::DRUM drums@endlink don't exist for a @link Music::CombinedNote note@endlink, then \"null\" is used in the first section of lines 3+ and the @link DefVel velocity@endlink for the missing type is set to 0. 
 * @li Example from \"SONG_Combined Example.song\": 
@verbatim
Combined Example
3;203;4;6
48,52,55,58|0,8;6;12;100|100
null|8,0;12;4;0|100
48,52,55,60|2,8,4;4;4;100|100
50,53,58|0,8;6;4;100|100
null|8,0;12;4;0|100
57|0,8;4;4;100|100
48,60,64,52,55|0,2,4,8;4;4;100|100
48,60,52,64,67,55,72|0,2,4,8;4;4;100|100
@endverbatim
 * @}

/**
 * @defgroup DocSong Song
 * @ingroup SongGroup
 * @brief @copybrief Song
 * @{
 * @section DocSongInfo Information
 * @copydetails Song
 * 
 * 
 * @section DocSongConst Constants
 * @copydoc SongConst
 * @n @link SongConst More details@endlink.
 * 
 * @section DocSongEnums Enums
 * @copydoc SongEnums
 * @n @link SongEnums More details@endlink.
 * 
 * @section DocSongStructs Structs
 * @copydoc SongStructs
 * @n @link SongStructs More details@endlink.
 * 
 * @section DocSongPrivVar Private Variables
 * @copydoc SongPrivVar
 * @n @link SongPrivVar More details@endlink.
 * 
 * @section DocSongConstruct Constructors
 * @copydoc SongConstruct
 * @n @link SongConstruct More details@endlink.
 * 
 * @section DocSongPubFunc Public Functions
 * @copydoc SongPubFunc
 * @n @link SongPubFunc More details@endlink.
 * 
 * @section DocSongPrivFunc Private Functions
 * @copydoc SongPrivFunc
 * @n @link SongPrivFunc More details@endlink.
 *  
 * @section DocSongStatFunc Static Functions
 * @copydoc SongStatFunc
 * @n @link SongStatFunc More details@endlink. 
 * 
 * @section DocSongCode Code
 * @includelineno Song.cs 
 * @}
*/

/**
 * @defgroup DocSM Song Manager
 * @ingroup SongGroup
 * @brief @copybrief SongManagerClass
 * @{
 * @section DocSMInfo Information
 * @copydetails SongManagerClass
 * 
 * @section DocSMPrivVar Private Variables
 * @copydoc SMPrivVar
 * @n @link SMPrivVar More details@endlink.
 * 
 * @section DocSMConstruct Constructors
 * @copydoc SMConstruct
 * @n @link SMConstruct More details@endlink.
 * 
 * @section DocSMPubFunc Public Functions
 * @copydoc SMPubFunc
 * @n @link SMPubFunc More details@endlink.
 * 
 * @section DocSMPrivFunc Private Variables
 * @copydoc SMPrivFunc
 * @n @link SMPrivFunc More details@endlink.
 * 
 * @section DocSMCode Code
 * @includelineno SongManagerClass.cs
 * @}
*/