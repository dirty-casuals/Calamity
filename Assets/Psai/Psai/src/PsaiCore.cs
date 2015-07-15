//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;

namespace psai.net
{

    /// <summary>
    /// The PsaiInfo class is used to retrieve information about psai's momentary state. 
    /// </summary>
    /// <seealso cref="psai.net.PsaiCore.GetPsaiInfo()"/>
    public class PsaiInfo
    {        
        /// <summary>
        /// the current state of psai (see <see cref="PsaiState">PsaiState</see> structure for more info)
        /// </summary>
        public PsaiState psaiState;			

        /// <summary>
        /// will differ from psaiSate when psai is transitioning to Silence Mode or Rest Mode.
        /// </summary>
        public PsaiState upcomingPsaiState;     

        /// <summary>
        ///  the id of the last Basic Mood triggered
        /// </summary>
        public int lastBasicMoodThemeId;			

        /// <summary>
        /// the id of the Theme that is currently playing (or just about to switch to)
        /// </summary>
        public int effectiveThemeId;

        /// <summary>
        /// the id of the theme that will be played next after the current Theme
        /// </summary>
        public int upcomingThemeId;			

        /// <summary>
        /// the current dynamic Intensity level
        /// </summary>
        public float currentIntensity;         
     
        /// <summary>
        /// the dynamic intensity level that will be switched to after the current Segment is over.
        /// </summary>
        public float upcomingIntensity;           

        /// <summary>
        /// the number of queued Themes that will be played back after the current Theme has ended.
        /// </summary>
        public int themesQueued;

        /// <summary>
        /// the id of the Segment that will be played next, or -1 if it has not yet been evaluated
        /// </summary>
        public int targetSegmentId;			

        /// <summary>
        /// hold true if the automatic decrease of dynamic Intensity is currently disabled        
        /// </summary>
        public bool intensityIsHeld;        

        /// <summary>
        /// holds true if psai is about to transition to the last Basic Mood that was triggered, after a call to <see cref="psai.net.PsaiCore.ReturnToLastBasicMood">ReturnToLastBasicMood(false)</see>
        /// </summary>
        public bool returningToLastBasicMood;           

        /// <summary>
        /// holds the number of remaining milliseconds that psai will stay in Rest Mode. Holds 0 if not in Rest Mode.
        /// </summary>
        public int remainingMillisecondsInRestMode;      

        /// <summary>
        /// holds true if playback has been paused, after a call to <see cref="psai.net.PsaiCore.SetPaused">SetPaused(true)</see>
        /// </summary>
        public bool paused;                         
    };


    /// <summary>
    /// The SoundtrackInfo class is used to retrieve information about the psai Soundtrack currently loaded. 
    /// </summary>
    /// <seealso cref="psai.net.PsaiCore.GetSoundtrackInfo"/>
    public class SoundtrackInfo
    {
        /// <summary>
        /// the number of Themes currently loaded
        /// </summary>
        public int themeCount;

        /// <summary>
        /// an array of length themeCount, that will hold all the Theme ids of the Soundtrack currently loaded
        /// </summary>
        public int[] themeIds;					
    }


    /// <summary>
    /// The ThemeInfo struct is used to query information about the Theme with the given id. 
    /// </summary>
    /// <seealso cref="psai.net.PsaiCore.GetThemeInfo"/>
	public class ThemeInfo
	{
        /// <summary>
        /// The id of the Theme, which is unique for each Soundtrack.
        /// </summary>
		public int id;

        /// <summary>
        /// The Theme's ThemeType
        /// </summary>
		public ThemeType type;

        /// <summary>
        /// an array containing the ids of all Segments of this Theme
        /// </summary>
		public int[] segmentIds;						

        /// <summary>
        /// the Theme's name
        /// </summary>
        public string name;


        public override string ToString()
        {
            return id + ": " + name +  " [" + type + "]";
        }
	};


    /// <summary>
    /// The SegmentInfo struct is used to query information about the Segment with the given id. 
    /// </summary>
    /// <seealso cref="psai.net.PsaiCore.GetSegmentInfo"/>
	public class SegmentInfo
	{
        /// <summary>
        /// the Segment's id, which is unique for each Soundtrack
        /// </summary>
		public int id;

        /// <summary>
        /// a bitwise combination of the Segment's Suitabilities
        /// </summary>
		public int segmentSuitabilitiesBitfield;				

        /// <summary>
        /// the musical intensity of this Segment, as classified within the psai Editor.
        /// </summary>
		public float intensity;

        /// <summary>
        /// the id of the Segment's Theme
        /// </summary>
		public int themeId;

        /// <summary>
        /// the number of times this Segment has been played so far since the soundtrack has been loaded
        /// </summary>
		public int playcount;

        /// <summary>
        /// the Segment's name
        /// </summary>
        public string name;

        /// <summary>
        /// the full length of the Segment including its pre- and postbeat region, in milliseconds
        /// </summary>
        public int fullLengthInMilliseconds;

        /// <summary>
        /// the length of the Segment's prebeat region in milliseconds
        /// </summary>
        public int preBeatLengthInMilliseconds;

        /// <summary>
        /// the length of the Segment's postbeat region in milliseconds
        /// </summary>
        public int postBeatLengthInMilliseconds;
	};

    /// <summary>
    /// The PsaiCore class provides access to all of psai's functionality.
    /// </summary>
    public class PsaiCore
    {
        private Logik m_logik;

        static PsaiCore s_singleton;

        /// <summary>
        /// Returns an instance of PsaiCore as a Singleton.
        /// <remarks>The PsaiCore class provides this Singleton for convenience, so you can easily access your psai soundtrack from all classes.</remarks>
        /// </summary>
        /// <value>gets the reference to the PsaiCore Singleton</value>
        public static PsaiCore Instance
        {
            get
            {
                if (s_singleton == null)
                {
                    s_singleton = new PsaiCore();
                }
                return s_singleton;
            }

            set
            {
                s_singleton = null;
            }
        }

        public static bool IsInstanceInitialized()
        {
            return s_singleton != null;
        }


        public PsaiCore()
        {
            m_logik = new Logik();
        }

        /// <summary>
        /// Sets the maximum latency in milliseconds that is needed by the target platform to buffer soundfiles from the storage medium.
        /// </summary>
        /// <remarks>As there is currently no mechanism within Unity to check the actual latency needed by the target device to 
        /// buffer and play back a sound, we solve this by providing a maximum latency value that should be enough for each given platform,
        /// and we delay all playback by this value. Please note that these value not only depends on the target platform, but also
        /// on the system specifications (like weaker/older mobile phones usually need more time to buffer), but also on the storage
        /// media (optical drives take much longer that harddrives). 
        /// We provide default values for all the platforms supported by Unity that will be set automatically and will work in most cases.
        /// However you may choose to finetune these settings. Lower latency settings will improve overall reactivity of your soundtrack,
        /// but might result in dropouts.
        /// </remarks>
        /// <seealso cref="psai.net.PsaiCore.SetMaximumLatencyNeededByPlatformToPlayBackBufferedSounddata"/>
        /// <param name="latencyInMilliseconds">the buffering latency in milliseconds</param>
        /// <returns>PsaiResult.OK if successful</returns>
        public PsaiResult SetMaximumLatencyNeededByPlatformToBufferSounddata(int latencyInMilliseconds)
        {
            return m_logik.SetMaximumLatencyNeededByPlatformToBufferSounddata(latencyInMilliseconds);
        }


        /// <summary>
        /// Sets the maximum latency in milliseconds that is needed by the target platform to play back prebuffered sounddata.
        /// </summary>
        /// <seealso cref="SetMaximumLatencyNeededByPlatformToBufferSounddata"/>
        /// <param name="latencyInMilliseconds">the buffering latency in milliseconds</param>
        /// <returns>PsaiResult.OK if successful</returns>
        public PsaiResult SetMaximumLatencyNeededByPlatformToPlayBackBufferedSounddata(int latencyInMilliseconds)
        {
            return m_logik.SetMaximumLatencyNeededByPlatformToPlayBackBufferedSounds(latencyInMilliseconds);
        }

        /// <summary>
        /// Sets the detail level of information written to the output console and log file.
        /// </summary>
        /// <remarks>LogLevel.errors will only report severe errors, whereas LogLevel.warnings will display errors and warnings.
        /// LogLevel.info will report errors, warnings and general information about calls to the psai API.
        /// </remarks>
        /// <param name="newLogLevel">the desired level of logging information</param>
        /// <returns>PsaiResult.OK if successful</returns>
        public void SetLogLevel(LogLevel newLogLevel)
	    {
		    m_logik.setLogLevel(newLogLevel);
	    }

        /// <summary>
        /// Loads the binary soundtrack configuration file created by the PsaiEditor
        /// </summary>
        /// <param name="pathToPcbFile">a file path to the binary psai soundtrack file created by the psaiEditor during export.</param>
        /// <returns>PsaiResult.OK if successful</returns>
        public PsaiResult LoadSoundtrack(string pathToPcbFile)
        {
            return m_logik.LoadSoundtrack(pathToPcbFile);
        }


        /// <summary>
        /// Loads the binary soundtrack configuration file created by the PsaiEditor
        /// </summary>
        /// <param name="pathToPcbFile">a file path to the binary psai soundtrack file created by the psaiEditor during export.</param>
        /// <returns>PsaiResult.OK if successful</returns>
        public PsaiResult LoadSoundtrackFromProjectFile(string pathToProjectFile)
        {
            return m_logik.LoadSoundtrackFromProjectFile(pathToProjectFile);
        }


        public PsaiResult LoadSoundtrackByPsaiProject(psai.Editor.PsaiProject project, string fullPathToProjectFile)
        {
            return m_logik.LoadSoundtrackByPsaiProject(project, fullPathToProjectFile);
        }

        /// <summary>
        /// Request for playing a certain Theme at the desired intensity
        /// </summary>
        /// <remarks>
        /// Please note that there is a hierarchy among the different types of Themes, which will e.g. prevent a Base Mood from interrupting a Theme of type Action Event.
        /// Likewise, a Theme of type Shock Event will interrupt any other Theme that may be currently playing. Please refer to the psai Manual included in the psai SDK for
        /// a description of all the Theme Types and their playback priorities. The general interruption behaviour is as follows:
        /// If a Theme of a lower priority is currently playing when a new Theme is triggered, the psai soundtrack will play the newly triggered Theme immediately and quickly
        /// fade out
        /// the previous Theme. This way it is possible to build up a stack of interrupted Themes: A Base Mood may be interrupted by an Action Event, which is then interrupted
        /// by a Shock Event. When the intensity of the Shock Event has dropped to zero, psai will return to the Action Event, starting with the intensity level that was up at
        /// the time of its interruption. Likewise, when the intensity of the Action Event has reached zero intensity, psai will continue with the Basic Mood.
        /// If another Theme of the same priority is playing when a Theme has been triggered, psai will switch to the next Theme as soon as the Segment currently playing has
        /// reached its end. This can only
        /// work if there is at least one Segment in the newly triggered Theme, that has been marked within the psai® Editor as a compatible follower to the Segment currently playing.
        /// Please refer to the psai® Editor documentation for more information about Segment compatibilites.
        /// If the newly triggered Theme is the very same Theme that is currently playing, psai will set the internal Intensity level to the Intensity-argument of the new trigger-call.
        /// Thus, triggering the same Theme over and over again will not result in an accumulation of the triggered intensity values. Please see the 'Intensity'-section of the psai Manual
        /// for more information about psai's Intensity concept.
        /// If a Theme of type Basic Mood is triggered while a Theme of higher priority is playing, psai will internally store the triggered Basic Mood as the one to switch to, when
        /// the Intensity level of all stacked Themes has dropped to zero. All other trigger-calls to Themes of lower priority are ignored completely.
        /// The Theme will be playing for a timespan as defined by the member "music duration" within the psai Editor. The intensity falloff rate will be automatically adjusted to reach
        /// zero accordingly. To manually override this setting, call the overloaded version of TriggerMusicTheme() with the additional musicDuration parameter.
        /// Troubleshooting: If the soundtrack does not react as expected, please check your 'psai.log' file in your '[current user]/Documents/psai' folder to see what happened, and check back
        /// with your composer to make sure that the Types of the affected Themes have been assigned correctly within the psai Editor authoring software.
        /// </remarks>
        /// <param name="themeId">The id of the Theme to play</param>
        /// <param name="intensity">The initial intensity value. The valid range is between 0.0f and 1.0f.</param>
        /// <returns>
        ///  <list type="table">
        ///  <listheader>
        ///     <term>term</term>
        ///     <description>description</description>
        ///  </listheader>
        ///    <item>
        ///      <term>"PsaiResult.OK</term>
        ///      <description> if successful</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.initialization_error</term>
        ///      <description>psai has not been initialized correctly. See psai.log for more information.</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.unknown_theme</term>
        ///      <description>the requested Theme does not exist in the current soundtrack</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.commandIgnoredMenuModeActive</term>
        ///      <description>the command was ignored, call <see cref="MenuModeLeave">MenuModeLeave()</see> first.</description>
        ///    </item>
        /// </list>
        /// </returns>       
        public PsaiResult TriggerMusicTheme(int themeId, float intensity)
        {            
            return m_logik.TriggerMusicTheme(themeId, intensity);
        }


        /// <summary>
        /// Request for playing a certain Theme at the desired intensity, for the given duration.
        /// </summary>
        /// <seealso cref="PsaiCore.TriggerMusicTheme(int, float)"/>
        /// <param name="themeId">The id of the Theme to play</param>
        /// <param name="intensity">The initial intensity value. The valid range is between 0.0f and 1.0f.</param>
        /// <param name="musicDurationInSeconds">the desired play duration (seconds) of the Theme after this single trigger call</param>
        /// <returns>PsaiResult.OK if successful</returns>
        public PsaiResult TriggerMusicTheme(int themeId, float intensity, int musicDurationInSeconds)
        {
            return m_logik.TriggerMusicTheme(themeId, intensity, musicDurationInSeconds);
        }        

        /// <summary>
        /// Increases (or decreases) the current dynamic intensity level, without changing the intensity falloff slope.
        /// </summary>
        /// <remarks> The resulting intensity value will be limited to a value between 0.0f and 1.0f.</remarks>
        /// <param name="deltaIntensity">a positive or negative delta value between 0.0f and 1.0f</param>
        /// <returns>PsaiResult.OK if successful</returns>
        public PsaiResult AddToCurrentIntensity(float deltaIntensity)
        {
            return m_logik.AddToCurrentIntensity(deltaIntensity, false);
        }


        /// <summary>
        /// Stops the music either by fading out quickly, or by enqueuing and End-Segment.
        /// </summary>
        /// <remarks>
        /// If the "immediately" parameter is set to false, psai will wait for the current Segment to finish, then play an End-
        /// Segment of the current Theme, then stop the music. Psai will remain silent until you explicitly trigger another
        /// Theme by calling TriggerMusicTheme().
        /// </remarks>
        /// <param name="immediately">passing 'true' will stop the playback by a quick fadeout; 'false' will smoothly end the music via the shortest path to a Segment that has the END-Suitability set.</param>
        /// <returns>
        ///  <list type="table">
        ///    <item>
        ///      <term>"PsaiResult.OK</term>
        ///      <description> if successful</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.initialization_error</term>
        ///      <description>psai has not been initialized correctly. See psai.log for more information.</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.unknown_theme</term>
        ///      <description>the requested Theme does not exist in the current soundtrack</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.commandIgnoredMenuModeActive</term>
        ///      <description>the command was ignored, call <see cref="MenuModeLeave">MenuModeLeave()</see> first.</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.commandIgnored</term>
        ///      <description>psai is already in Silence Mode</description>
        ///    </item>
        /// </list>
        /// </returns>  
        public PsaiResult StopMusic(bool immediately)
        {
            return m_logik.StopMusic(immediately);
        }


        /// <summary>
        /// Ends the current Theme and directly returns to the most recently triggered Basic Mood.
        /// </summary>
        /// <remarks>
        /// The transition to the Basic Mood will be interrupted by any call to TriggerMusicTheme().
        /// If you prefer to let the music keep silent for some time before playing the last Basic Mood again,
        /// use GoToRest().
        /// </remarks>
        /// <param name="immediately">true: quick fadeout, false: play an End-Segment</param>
        /// <returns>
        ///  <list type="table">
        ///    <item>
        ///      <term>"PsaiResult.OK</term>
        ///      <description> if successful</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.initialization_error</term>
        ///      <description>psai has not been initialized correctly. See psai.log for more information.</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.unknown_theme</term>
        ///      <description>the requested Theme does not exist in the current soundtrack</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.commandIgnoredMenuModeActive</term>
        ///      <description>the command was ignored, call <see cref="MenuModeLeave">MenuModeLeave()</see> first.</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.commandIgnored</term>
        ///      <description>the Basic Mood is already playing</description>
        ///    </item>
        /// </list>
        /// </returns>  
        public PsaiResult ReturnToLastBasicMood(bool immediately)
        {
            return m_logik.ReturnToLastBasicMood(immediately);
        }



        /// <summary>
        /// Stops the Theme currently playing. Psai will keep silent for some time and then wake up with the Basic Mood that was triggered the last.
        /// </summary>
        /// <remarks>
        /// The period of time psai will remain silent can be authored per Basic Mood in the psai Editor.
        /// </remarks>
        /// <param name="immediately">True: Go to rest immediately by fading out. False: play an End-Segment</param>
        /// <param name="fadeOutSeconds">the fade-out time in seconds</param>
        /// <returns></returns>
        public PsaiResult GoToRest(bool immediately, float fadeOutSeconds)
        {
            return m_logik.GoToRest(immediately, (int)(fadeOutSeconds * 1000));
        }


        public PsaiResult GoToRest(bool immediately, float fadeOutSeconds, int restTimeMin, int restTimeMax)
        {
            return m_logik.GoToRest(immediately, (int)(fadeOutSeconds * 1000), restTimeMin, restTimeMax);
        }


        /// <summary>
        /// Deactivates/reactivates the automatic decrease of the dynamic Intensity while the current Theme is playing.
        /// </summary>
        /// <remarks>
        /// Calling HoldCurrentIntensity(true) will keep the intensity on the current level while the current theme is playing.
        /// The automatic decrease will continue as soon as holdCurrentIntensity(false) is called, or when the playing theme is
        /// interrupted or forced to end, e.g. by calling StopMusic() or ReturnToBase(). Triggering the same theme again will change
        /// the constant intensity to the newly triggered intensity, but will not result in reactivating the automatic decrease.
        /// Note: Calls to holdCurrentIntensity() will be ignored while in Menu Mode or in Cutscene Mode. Call MenuModeLeave() or CutsceneLeave() first.
        /// </remarks>
        /// <param name="hold">pass true to hold the Intensity, false to reactivate the automatic decrease.</param>
        /// <returns>
        ///  <list type="table">
        ///    <item>
        ///      <term>"PsaiResult.OK</term>
        ///      <description> if successful</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.commandIgnoredr</term>
        ///      <description>ignored because the intensity is already being held</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.commandIgnoredMenuModeActive</term>
        ///      <description>the command was ignored, call <see cref="MenuModeLeave">MenuModeLeave()</see> first.</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.commandIgnoredCutsceneActive</term>
        ///      <description>the command was ignored, call <see cref="CutSceneLeave">CutSceneLeave()</see> first.</description>
        ///    </item>
        /// </list>
        /// </returns>  
        public PsaiResult HoldCurrentIntensity(bool hold)
        {
            return m_logik.HoldCurrentIntensity(hold);
        }

        /// <summary>
        /// Returns a string holding the version of the psai library.
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            return m_logik.getVersion();
        }

        /// <summary>
        /// Returns a string holding a description of the last error that occurred at runtime.
        /// </summary>
        /// <returns></returns>
        public string GetLastError()
        {
            return Logger.Instance.GetLastError();
        }

        /// <summary>
        /// Needs to be called within your gameloop to keep psai going.
        /// </summary>
        /// <returns>PsaiResult.OK if successful</returns>
        public PsaiResult Update()
        {
            return m_logik.Update();
        }

        /// <summary>
        /// Returns the psai master playback volume.
        /// </summary>
        /// <returns>volume between 0.0f and 1.0f</returns>
        public float GetVolume()
        {
            return m_logik.getVolume();
        }

        /// <summary>
        /// Sets the psai master playback volume.
        /// </summary>
        /// <param name="volume">volume between 0.0f and 1.0f</param>
        public void SetVolume(float volume)
        {
            m_logik.setVolume(volume);
        }

        /// <summary>
        /// Pauses or resumes all psai playback.
        /// </summary>
        /// <param name="setPaused">true to pause, false to resume</param>
        public void SetPaused(bool setPaused)
        {
            m_logik.setPaused(setPaused);
        }

        // 
        
        /// <summary>
        /// [DEPRECATED] Use <see cref="GetPsaiInfo">GetPsaiInfo()</see> instead.
        /// </summary>
        /// <returns>the current intensity value between 0.0f and 1.0f</returns>
        public float GetCurrentIntensity()
        {
            return m_logik.getCurrentIntensity();
        }

        /// <summary>
        /// Returns information about the current state of the psai engine.
        /// </summary>
        /// <returns>
        /// a datastructure of type <see cref="PsaiInfo">PsaiInfo</see>
        /// </returns>
        public PsaiInfo GetPsaiInfo()
        {
            return m_logik.getPsaiInfo();
        }

        /// <summary>
        /// Returns information about the psai soundtrack currently loaded.
        /// </summary>
        /// <returns>
        /// a datastructure of type <see cref="psai.net.SoundtrackInfo">SoundtrackInfo</see>
        /// </returns>
        public SoundtrackInfo GetSoundtrackInfo()
        {
            return m_logik.m_soundtrack.getSoundtrackInfo();
        }

        /// <summary>
        /// Returns information about the Theme with the given themeId.
        /// </summary>
        /// <remarks>
        /// Use PsaiCore.GetSoundtrackInfo() to retrieve a list of themeIds.
        /// </remarks>
        /// <param name="themeId">The id of the Theme as set in the psai Editor.</param>
        /// <returns>
        /// a datastructure of type <see cref="psai.net.ThemeInfo">ThemeInfo</see>
        /// </returns>
        public ThemeInfo GetThemeInfo(int themeId)
        {
            return m_logik.m_soundtrack.getThemeInfo(themeId);
        }

        /// <summary>
        /// Returns information about the Segment with the given segmentId.
        /// </summary>
        /// <remarks>
        /// Use PsaiCore.GetThemeInfo() to retrieve a list of segmentIds.
        /// </remarks>
        /// <param name="segmentId">the Segment's id</param>
        /// <returns>
        /// a datastructure of type <see cref="psai.net.SegmentInfo">SegmentInfo</see>
        /// </returns>
        public SegmentInfo GetSegmentInfo(int segmentId)
        {
            return m_logik.m_soundtrack.getSegmentInfo(segmentId);
        }

        /// <summary>
        /// Returns the id of the Segment that's currently playing.
        /// </summary>
        /// <returns>the id of the current Segment</returns>
        public int GetCurrentSegmentId()
        {
            return m_logik.getCurrentSnippetId();
        }


        /// <summary>
        /// Returns the id of the Theme that's currently playing (or just about to switch to).
        /// [DEPRECATED] Use GetPsaiInfo().effectiveThemeId .
        /// </summary>
        /// <returns>the id of the Theme currently playing</returns>
        public int GetCurrentThemeId()
        {
            return m_logik.getEffectiveThemeId();
        }


        /// <summary>
        /// Returns the number of remaining milliseconds until the current Segment playback has reached its end, including the PostBeat region
        /// </summary>
        /// <returns>the remaining milliseconds, or -1 if no Segment is currently playing</returns>
        public int GetRemainingMillisecondsOfCurrentSegmentPlayback()
        {
            return m_logik.GetRemainingMillisecondsOfCurrentSegmentPlayback();
        }

        /// <summary>
        /// Returns the number of remaining milliseconds until the next Segment will start playing.
        /// </summary>
        /// <returns>remaining milliseconds, or -1 if no Segment is scheduled.</returns>
        public int GetRemainingMillisecondsUntilNextSegmentStart()
        {
            return m_logik.GetRemainingMillisecondsUntilNextSegmentStart();
        }


        /// <summary>
        /// Activates the Menu Mode and plays a given Theme as the menu background music.
        /// </summary>
        /// <remarks>
        /// The Menu Mode is designed for all kinds of in-game menus, where the gameplay is interrupted and frozen
        /// In Menu Mode no intensity curve will be applied, so the music holds the intensity-level just 
        /// like a Continuous Action Theme. When the Player returns to the game, call menuModeLeave() to switch back to the previous state.
        /// </remarks>
        /// <seealso cref="MenuModeLeave"/>
        /// <param name="menuThemeId">the id of the theme to play in the background while in menu mode.</param>
        /// <param name="menuThemeIntensity">the static intensity of the menu Theme playback</param>
        /// <returns>PsaiResult.OK if successful</returns>
        public PsaiResult MenuModeEnter(int menuThemeId, float menuThemeIntensity)
        {
            return m_logik.MenuModeEnter(menuThemeId, menuThemeIntensity);
        }

        /// <summary>
        /// Leaves the Menu Mode. See <see cref="MenuModeEnter">MenuModeEnter</see> for more information.
        /// </summary>
        /// <returns>PsaiResult.OK if successful</returns>
        public PsaiResult MenuModeLeave()
        {
            return m_logik.MenuModeLeave();
        }

        /// <summary>
        /// Returns true if psai is currently in Menu Mode, false otherwise.
        /// </summary>
        /// <returns>
        /// true if the MenuMode is active, false otherwise
        /// </returns>
        public bool MenuModeIsActive()
        {
            return m_logik.menuModeIsActive();
        }

        /// <summary>
        /// Returns true if psai is currently in Cutscene Mode, false otherwise.
        /// </summary>
        /// <returns>
        /// true if psai is in Cutscene Mode
        /// </returns>
        public bool CutSceneIsActive()
        {
            return m_logik.cutSceneIsActive();
        }



        /// <summary>
        /// Enters a cutscene, using the given Theme as the background music.
        /// </summary>
        /// <remarks>
        /// The Cutscene Mode is intended for non-interactive movie-like sequences where
        /// the regular gameplay is
        ///  interrupted. Similar to the Menu Mode, the Cutscene Mode jumps out of regular playback
        ///  and interrupts any theme currently playing, and immediately switching to the music for
        ///  the cutscene.
        ///  You can use a theme of any given Theme Type as a cutscene theme, for the regular
        ///  playback hierarchy of themes is ignored during cutscene mode.
        ///  This allows you to re-use regular themes of your game soundtrack for a cutscene. 
        ///  If you use made-to-measure music for a cutscene, we recommend creating a new
        ///  theme containing a single Segment in the default group. Make sure the Segment has
        ///  the Suitability START.
        ///  Intensity levels will only matter as long as your cutscene theme contains
        ///  more than a single Segment. While in Cutscene Mode, the intensity will stay
        ///  on a constant level until the cutscene is left. 
        ///  To do leave the cutscene call CutSceneLeave().
        /// </remarks>
        /// <param name="themeId">the id of the Theme to be played during the cutscene</param>
        /// <param name="intensity">the static intensity by which to play the cutscene Theme.</param>
        /// <returns>
        ///  <list type="table">
        ///    <item>
        ///      <term>"PsaiResult.OK</term>
        ///      <description> if successful</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.commandIgnoredMenuModeActive</term>
        ///      <description>the command was ignored, call <see cref="MenuModeLeave">MenuModeLeave()</see> first.</description>
        ///    </item>
        ///    <item>
        ///      <term>"PsaiResult.commandIgnoredCutsceneActive</term>
        ///      <description>the command was ignored, psai is already in Cutscene Mode.</description>
        ///    </item>
        /// </list>
        /// </returns>  
        public PsaiResult CutSceneEnter(int themeId, float intensity)
        {
            return m_logik.CutSceneEnter(themeId, intensity);
        }

        /// <summary>
        /// Leaves the CutScene Mode. See <see cref="CutSceneEnter">CutSceneEnter</see> for more information.
        /// </summary>
        /// <param name="immediately">passing true will leave the Cutscene by a quick fadeout. Passing false will switch back smoothly using the shortest path of compatible Segments.</param>
        /// <param name="reset">pass true if you want to clear the queue of interrupted Themes, that may have stacked up when the Cutscene had been entered.</param>
        /// <returns>PsaiResult.OK if successful</returns>
       	public PsaiResult CutSceneLeave(bool immediately, bool reset)
	    {
		    return m_logik.CutSceneLeave(immediately, reset);
	    }


        /// <summary>
        /// Immediately plays back the given Segment.
        /// </summary>
        /// <remarks>
        /// This method is mainly intended for testing or debugging purposes.
        /// </remarks>
        /// <param name="segmentId">the id of the Segment to play</param>
        /// <returns>PsaiResult.OK if successful</returns>
        public PsaiResult PlaySegment(int segmentId)
        {
            return m_logik.PlaySegmentLayeredAndImmediately(segmentId);
        }

        /// <summary>
        /// Returns true if there is at least one Segment in the target Theme that is marked as directly compatible to the source Segment.
        /// </summary>
        /// <remarks>
        /// If this method returns true, this means that a direct transition from the sourceSegment to the target Theme is possible. Respectively, 
        /// if the target Thme is of type Highlight Layer, a compatible Segment exists that will be layered over the sourceSegment if the
        /// Highlight Layer is triggered while the source Segment is playing.
        /// If no compatible Segment exists, the trigger call will be ignored in case of Highlight Layers. For other Themes types,
        /// psai will play the shortest Sequence of compatible Segments until the target Theme is be reached. If no compatible sequence exists, you will be warned by the Psai Editor upon export / audit.
        /// </remarks>
        /// <param name="sourceSegmentId">the id of the Source Segment</param>
        /// <param name="targetThemeId">the id of the Theme to transition to</param>
        /// <returns></returns>
        public bool CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(int sourceSegmentId, int targetThemeId)
        {
            return m_logik.CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(sourceSegmentId, targetThemeId);
        }


        public void AddLoggerOutput(psai.net.LoggerOutput loggerOutput)
        {
            m_logik.AddLoggerOutput(loggerOutput);
        }

        /// <summary>
        /// Performs platform-specific cleanup.
        /// </summary>
        public void Release()
        {
            m_logik.Release();
            m_logik = null;
        }
    }
}
