/**
 * @defgroup DocSC Song Creation
 * @ingroup Audio
 * @brief Allows for creating @link Song Songs@endlink. Used only for development; not a feature.
 * @{
 * @section DocSCIntro Introduction
 * The song creation interface allows for creating a Song note-by-note.
 * 
 * @image html SongCreationInterface.png
 * @}
 * 
 * @defgroup DocSC_ND Note Display
 * @ingroup DocSC
 * @brief Modules for displaying the @link Music::CombinedNote notes@endlink in the Song being created.
 * 
 * @defgroup DocSC_PDS Pitch and Drum Selection
 * @ingroup DocSC
 * @brief Modules for selecting the @link Music::PITCH pitches@endlink and @link Music::DRUM drums@endlink in a @link Music::CombinedNote note@endlink.
 * 
 * @defgroup DocSC_LOS Length and Offset Selection
 * @ingroup DocSC
 * @brief Modules for selecting the @link Music::NOTE_LENGTH_BASE length@endlink and @link Music::CombinedNote.OffsetFromPrevNote offsets@endlink of @link Music::CombinedNote notes@endlink in the Song being created.
*/

/**
* @defgroup DocSC_NDP Note Display Panel
* @ingroup DocSC_ND
* @brief @copybrief SC_NoteDisplayPanel
* @{
* @section intro Introduction
* @copydetails SC_NoteDisplayPanel
*
* @section DocSC_NDPPrivVar Private Variables
* @copydoc SC_NDPPrivVar
* @n @link SC_NDPPrivVar More details@endlink.
*
* @section DocSC_NDPUnity Unity Functions
* @copydoc SC_NDPUnity
* @n @link SC_NDPUnity More details@endlink.
*
* @section DocSC_NDPPubFunc Public Functions
* @copydoc SC_NDPPubFunc
* @n @link SC_NDPPubFunc More details@endlink.
* 
* @section DocSC_NDPHandlers Event Handlers
* @copydoc SC_NDPHandlers
* @n @link SC_NDPHandlers More details@endlink.
* 
* @section DocSC_NDPCode Code
* @includelineno SC_NoteDisplayPanel.cs
* @}
*/

/**
 * @defgroup DocSC_MDP Measure Display Panel
 * @ingroup DocSC_ND
 * @brief @copybrief SC_MeasureDisplayPanel
 * @{
 * @section DocSC_MDPInfo Information
 * @copydetails SC_MeasureDisplayPanel
 * 
 * @section DocSC_MDPConst Constants
 * @copydoc SC_MDPConst
 * @n @link SC_MDPConst More details@endlink.
 * 
 * @section DocSC_MDPPrivVar Private Variables
 * @copydoc SC_MDPPrivVar
 * @n @link SC_MDPPrivVar More details@endlink.
 * 
 * @section DocSC_MDPUnity Unity Functions
 * @copydoc SC_MDPUnity
 * @n @link SC_MDPUnity More details@endlink.
 * 
 * @section DocSC_MDPPubFunc Public Functions
 * @copydoc SC_MDPPubFunc
 * @n @link SC_MDPPubFunc More details@endlink.
 * 
 * @section DocSC_MDPHandlers Event Handlers
 * @copydoc SC_MDPHandlers
 * @n @link SC_MDPHandlers More details@endlink.
 * 
 * @section DocSC_MDPCode Code
 * @includelineno SC_MeasureDisplayPanel.cs
 * @}
*/

/**
 * @defgroup DocSC_NDC Note Display Container
 * @ingroup DocSC_ND
 * @brief @copybrief SC_NoteDisplayContainer
 * @{
 * @section DocSC_NDCInfo Information
 * @copydetails SC_NoteDisplayContainer
 * 
 * @section DocSC_NDCConst Constants
 * @copydoc SC_NDCConst
 * @n @link SC_NDCConst More details@endlink. 
 * 
 * @section DocSC_NDCPrivVar Private Variables
 * @copydoc SC_NDCPrivVar
 * @n @link SC_NDCPrivVar More details@endlink.
 * 
 * @section DocSC_NDCUnity Unity Functions
 * @copydoc SC_NDCUnity
 * @n @link SC_NDCUnity More details@endlink.
 * 
 * @section DocSC_NDCPubFunc Public Functions
 * @copydoc SC_NDCPubFunc
 * @n @link SC_NDCPubFunc More details@endlink.
 * 
 * @section DocSC_NDCHandlers Event Handlers
 * @copydoc SC_NDCHandlers
 * @n @link SC_NDCHandlers More details@endlink.  
 * 
 * @section DocSC_NDCCode Code
 * @includelineno SC_NoteDisplayContainer.cs
 * @}  
*/

/** 
 * @defgroup DocSC_LSD Load Song Dialog
 * @ingroup DocSC
 * @brief @copybrief SC_LoadSongDialog
 * @{
 * @section DocSC_LSDInfo Information
 * @copydetails SC_LoadSongDialog
 * 
 * @section DocSC_LSDEventTypes Event Types
 * @copydoc SC_LSDEventTypes
 * @n @link SC_LSDEventTypes More details@endlink.
 * 
 * @section DocSC_LSDEvents Events
 * @copydoc SC_LSDEvents
 * @n @link SC_LSDEvents More details@endlink.
 * 
 * @section DocSC_LSDPrivVar Private Variables
 * @copydoc SC_LSDPrivVar
 * @n @link SC_LSDPrivVar More details@endlink.
 * 
 * @section DocSC_LSDUnity Unity Functions
 * @copydoc SC_LSDUnity
 * @n @link SC_LSDUnity More details@endlink.
 * 
 * @section DocSC_LSDHandlers Event Handlers
 * @copydoc SC_LSDHandlers
 * @n @link SC_LSDHandlers More details@endlink.
 * 
 * @section DocSC_LSDCode Code
 * @includelineno SC_LoadSongDialog.cs
 * @}
 */

/**
 * @defgroup DocSC_PST Pitch Selection Trigger
 * @ingroup DocSC_PDS
 * @brief @copybrief SC_PitchSelectionTrigger
 * @{
 * @section DocSC_PSTPrivVar Private Variables
 * @copydoc SC_PSTPrivVar
 * @n @link SC_PSTPrivVar More details@endlink.
 * 
 * @section DocSC_PSTUnity Unity Functions
 * @copydoc SC_PSTUnity
 * @n @link SC_PSTUnity More details@endlink.
 * 
 * @section DocSC_PSTPubFunc Public Functions
 * @copydoc SC_PSTPubFunc
 * @n @link SC_PSTPubFunc More details@endlink.
 * 
 * @section DocSC_PSTHandlers Event Handlers
 * @copydoc SC_PSTHandlers
 * @n @link SC_PSTHandlers More details@endlink.
 * 
 * @section DocSC_PSTCode Code
 * @includelineno SC_PitchSelectionTrigger.cs
 * @}
 */

/**
 * @defgroup DocSC_PSC Pitch Selection Container
 * @ingroup DocSC_PDS
 * @brief @copybrief SC_PitchSelectionContainer
 * @{
 * @section DocSC_PSCInfo Information
 * @copydetails SC_PitchSelectionContainer
 * 
 * @section DocSC_PSCTerminology Terminology
 * @subsection DocSC_PSCRest Rest Note
 * In the context of the Pitch Selection Container, a rest note is a note that contains
 * no sound, but is used for spacing @link Music::CombinedNote notes@endlink. If the @link SC_PitchSelectionContainer::mRest rest toggle@endlink is
 * turned on, then all @link Music::PITCH pitches@endlink are unset so that no sound plays. @n The
 * @link Music::PITCH.REST rest note pitch@endlink is the last value of the
 * @link Music::PITCH pitch enum@endlink. 
 * 
 * @section DocSC_PSCPrivVar Private Variables
 * @copydoc SC_PSCPrivVar
 * @n @link SC_PSCPrivVar More details@endlink.
 * 
 * @section DocSC_PSCPubFunc Public Functions
 * @copydoc SC_PSCPubFunc
 * @n @link SC_PSCPubFunc More details@endlink.
 * 
 * @section DocSC_PSCHandlers Event Handlers
 * @copydoc SC_PSCHandlers
 * @n @link SC_PSCHandlers More details@endlink.
 *  
 * @section DocSC_PSCCode Code
 * @includelineno SC_PitchSelectionContainer.cs
 * @}
*/

/**
* @defgroup DocSCM Song Creation Manager
* @ingroup DocSC
* @brief @copybrief SongCreationManager
* @{
* @section DocSCMInfo Information
* @copydetails SongCreationManager
*
* @section DocSCMConst Constants
* @copydoc SCMConst
* @n @link SCMConst More details@endlink.
* 
* @section DocSCMPrivVar Private Variables
* @copydoc SCMPrivVar
* @n @link SCMPrivVar More details@endlink.
*
* @section DocSCMUnity Unity Functions
* @copydoc SCMUnity
* @n @link SCMUnity More details@endlink.
*
* @section DocSCMHandlers Event Handlers
* @copydoc SCMHandlers
* @n @link SCMHandlers More details@endlink.
* 
* @section DocSCMCode Code
* @includelineno SongCreationManager.cs
* @}
*/
