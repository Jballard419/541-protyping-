/**
 * @defgroup Audio Audio
 * @brief Everything related to playing audio.
 * @{
 * @section AudioInfo Information
 * Samples for the piano and marimba were obtained from Electronic %Music Studios at the University of Iowa (http://theremin.music.uiowa.edu/MIS.html). 
 * These samples are provided from Electronic %Music Studios to the public for any use without restrictions.
 * @n Samples for the drums were obtained from DrumGizmo (http://www.drumgizmo.org/wiki/doku.php?id=about) 
 * and are used under the Creative Commons Attribution 3.0 Unported license (https://creativecommons.org/licenses/by/3.0/). 
 * The kit used was the DRSKit (http://www.drumgizmo.org/wiki/doku.php?id=kits:drskit). 
 * 
 * 
 * @subsection DefVel Velocity
 * Velocity is a term taken from MIDI that refers to the volume of the note. 
 * Unlike MIDI where the velocity ranges from 0-127, the velocity of notes for
 * this project range from 0-100 so that they can be used like a percentage.
 * @n A velocity of 0 means that the note is silent. A velocity of 100
 * means that the note is as loud as it can possibly be.
 * 
 * @subsection DefBID Built-In Dynamics
 * This term refers to the capability that some instruments have for using 
 * different sound files depending on the velocity of a note.
 * @n Right now only the Piano supports this. There are three different sound
 * files for each pitch which are labelled pp (pianissimo), mf (mezzoforte), or 
 * ff (fortissimo). 
 * 
 * @subsubsection DefBIDThresh Built-In Dynamics Thresholds
 * This term refers to the thresholds for when to use a specific Built-In Dynamic.
 * For example, the thresholds for the Piano are [50, 75, 100]. This means that velocities
 * of 50 and under use the pp files, velocities of 51-75 use the mf files, and velocities of
 * 76-100 use the ff files.
 * 
 * @subsection DefBPM BPM
 * This term is an abbreviation for Beats Per Minute. It refers to the tempo of a song.
 * 
 * @}
*/

/**
 * @defgroup AudioManagement Audio Management
 * @ingroup Audio
 * @brief Everything related to managing the audio.
*/

/**
 * @defgroup VIM Virtual Instrument Manager
 * @ingroup AudioManagement
 * @brief @copybrief VirtualInstrumentManager
 * @{
 * @section VIMInfo Information
 * @copydetails VirtualInstrumentManager
 * 
 * @section DocVIMConst Constants
 * @copydoc VIMConst 
 * @n @link VIMConst More details.@endlink
 * 
 * @section DocVIMEventTypes Event Types
 * @copydoc VIMEventTypes
 * @n @link VIMEventTypes More details @endlink
 * 
 * @section DocVIMEvents Events
 * @copydoc VIMEvents
 * @n @link VIMEvents More details @endlink
 * 
 * @section DocVIMEffectParams Audio Effect Parameters
 * @copydoc filterParams
 * @n @link filterParams More details@endlink.
 * 
 * @section DocVIMPubVar Public Variables
 * @copydoc VIMPub
 * @n @link VIMPub More details@endlink.
 * 
 * @section DocVIMPrivVar Private Variables
 * @copydoc VIMPriv
 * @n @link VIMPriv More details@endlink.
 * 
 * @section DocVIMUnity Unity Functions
 * @copydoc VIMUnity
 * @n @link VIMUnity More details@endlink.
 * 
 * @section DocVIMPubFunc Public Functions
 * @copydoc VIMPubFunc
 * @n @link VIMPubFunc More details@endlink.
 * 
 * @section DocVIMCoroutines Coroutines
 * @copydoc VIMCoroutines
 * @n @link VIMCoroutines More details@endlink.
 * 
 * @section DocVIMPrivFunc Private Functions
 * @copydoc VIMPrivFunc
 * @n @link VIMPrivFunc More details@endlink.
 * 
 * @section DocVIMHandlers Event Handlers
 * @copydoc VIMHandlers
 * @n @link VIMHandlers More details@endlink.
 * 
 * @section DocVIMCode Code
 * @includelineno VirtualInstrumentManager.cs
 * @}
*/

/**
 * @defgroup VI Virtual Instruments
 * @ingroup AudioManagement
 * @brief Used to load and store audio data for a specific instrument.
 * @{
 * @section VIInfo Information
 * Virtual instruments are responsible for loading the proper wav files, normalizing them, 
 * and providing the audio data from them to the @link VIM Virtual Instrument Manager@endlink.
 * 
 * @section VISuppInst Supported Instruments
 * Right now we only support a Piano, Marimba, and @link DrumKit Drum Kit@endlink.
 * 
 * @}
*/

/**
 * @defgroup VIBase VirtualInstrument
 * @ingroup VI
 * @brief The base class that all types of @link VI Virtual Instruments@endlink implement.
 * @{
 * @section DocVIBaseInfo Information
 * @copydetails VirtualInstrument 
 * 
 * @section DocVIBaseConst Constants
 * @copydoc VIBaseConst
 * @n @link VIBaseConst More details@endlink.
 * 
 * @section DocVIBaseProVar Protected Variables
 * @copydoc VIBaseProVar
 * @n @link VIBaseProVar More details@endlink.
 * 
 * @section DocVIBaseConstruct Constructors
 * @copydoc VIBaseConstruct
 * @n @link VIBaseConstruct More details@endlink.
 * 
 * @section DocVIBasePubFunc Public Functions
 * @copydoc VIBasePubFunc
 * @n @link VIBasePubFunc More details@endlink.
 * 
 * @section DocVIBaseProFunc Protected Functions
 * @copydoc VIBaseProFunc
 * @n @link VIBaseProFunc More details@endlink.
 * 
 * @section DocVIBasePrivFunc Private Functions
 * @copydoc VIBasePrivFunc
 * @n @link VIBasePrivFunc More details@endlink.
 * 
 * @section DocVIBaseVirtFunc Pure Virtual Functions
 * @copydoc VIBaseVirtFunc
 * @n @link VIBaseVirtFunc More details@endlink.
 * 
 * @section DocVICode Code
 * @includelineno VirtualInstrument.cs
 * @}
*/

/**
 * @defgroup DocPiano Piano
 * @ingroup VI
 * @brief @copybrief Piano
 * @{
 * @section DocPianoIntro Introduction
 * @copydetails Piano
 * 
 * @section DocPianoConstruct Constructors
 * @copydoc PianoConstruct
 * @n @link PianoConstruct More details@endlink.
 * 
 * @section DocPianoVirtFunc Implemented Virtual Functions
 * @copydoc PianoVirtFunc
 * @n @link PianoVirtFunc More details@endlink.
 * 
 * @section DocPianoCode Code
 * @includelineno Piano.cs
 * @}
*/

/**
 * @defgroup DocMar Marimba
 * @ingroup VI
 * @brief @copybrief Marimba
 * @{
 * @section DocMarInfo Information
 * @copydetails Marimba
 * 
 * @section DocMarConstruct Constructors
 * @copydoc MarConstruct
 * @n @link MarConstruct More details@endlink.
 * 
 * @section DocMarVirtFunc Implemented Virtual Functions
 * @copydoc MarVirtFunc
 * @n @link MarVirtFunc More details@endlink.
 * 
 * @section DocMarCode Code
 * @includelineno Marimba.cs
 * @}
*/

/**
 * @defgroup DocDrum Drum Kit
 * @ingroup VI
 * @brief @copybrief DrumKit
 * @{
 * @section DocDrumInfo Information
 * @copydetails DrumKit
 * 
 * @section DocDrumConstruct Constructors
 * @copydoc DrumConstruct
 * @n @link DrumConstruct More details@endlink.
 * 
 * @section DocDrumVirtFunc Implemented Virtual Functions
 * @copydoc DrumVirtFunc
 * @n @link DrumVirtFunc More details@endlink.
 * 
 * @section DocDrumCode Code
 * @includelineno DrumKit.cs
 * @}
*/

/**
 * @defgroup DocNOO NoteOutputObject
 * @ingroup AudioManagement
 * @brief @copybrief NoteOutputObject
 * @{
 * @section DocNOOIntro Introduction
 * @copydetails NoteOutputObject
 * 
 * @section DocNOOPrivVar Private Variables
 * @copydoc NOOPrivVar
 * @n @link NOOPrivVar More details@endlink.
 * 
 * @section DocNOOUnity Unity Functions
 * @copydoc NOOUnity
 * @n @link NOOUnity More details@endlink.
 * 
 * @section DocNOOPubFunc Public Functions
 * @copydoc NOOPubFunc
 * @n @link NOOPubFunc More details@endlink.
 * 
 * @section DocNOOPrivFunc Private Functions
 * @copydoc NOOPrivFunc
 * @n @link NOOPrivFunc More details@endlink.
 * 
 * @section DocNOOHandlers Event Handlers
 * @copydoc NOOHandlers
 * @n @link NOOHandlers More details@endlink.
 * 
 * @section DocNOOCode Code
 * @includelineno NoteOutputObject.cs
 * @}
*/
