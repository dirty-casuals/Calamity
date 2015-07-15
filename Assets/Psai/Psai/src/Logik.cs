//-----------------------------------------------------------------------
// <copyright file="Logik.cs" company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


#if (UNITY_EDITOR)
    #undef PSAI_NOLOG
#endif

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace psai.net
{

    /// <summary>
    /// At any point of time psai is in exactly one of the following play modes, which affect the internal playback logic and how psai reacts to incoming commands.
    /// </summary>
    public enum PsaiPlayMode
    {
        /// <summary>
        /// psai's normal ingame play mode
        /// </summary>
        regular = 0,

        /// <summary>
        /// while in Menu Mode, the dynamic intensity is not decreasing and Trigger calls are ignored
        /// </summary>
        menuMode,

        /// <summary>
        /// while in a Cut Scene, the dynamic intensity is not decreasing and trigger calls are ignored.
        /// </summary>
        /// <remarks>
        /// Cut scenes may be interrupted by switching to the Menu Mode and will be resumed when leaving the Menu Mode.
        /// </remarks>
        cutScene
    }

    /// <summary>
    /// The return value of most of psai's api methods
    /// </summary>
	public enum PsaiResult
	{
		none,
		OK,
		alreadyActive, 
		badCommand, 
		channelAllocFailed,
		channelStolen, 
		error_file, 
		file_couldNotSeek,
		file_diskEjected, 
		file_eof,
		file_notFound,
		format_error,
		initialization_error,
		internal_error,
		invalidHandle,
		invalidParam,
		memory_error,
		notReady,
		error_createBufferFailed,
		output_format_error,
		output_init_failed,
		output_failure,
		update_error,
		error_version,
		unknown_theme,
        essential_segment_missing,
		commandIgnored,
		triggerDenied,
		triggerIgnoredFollowingThemeAlreadySet,
		triggerIgnoredLowPriority,
		commandIgnoredMenuModeActive,
		commandIgnoredCutsceneActive
	};

    /// <summary>
    /// At any point of time, psai is in exactly one of the following play states
    /// </summary>
	public enum PsaiState
	{
        /// <summary>
        /// not yet initialized	
        /// </summary>
		notready = 0,

        /// <summary>
        /// in silence mode psai will remain silent until the next theme is explicitly triggered
        /// </summary>
		silence,

        /// <summary>
        /// psai is playing music
        /// </summary>
		playing,

        /// <summary>
        /// psai is in a state of silence, but will re-activate itself automatically at some point of time, depending on the settings of the current Theme
        /// </summary>
		rest
	};


    internal class FadeData
    {
        public int voiceNumber;
        public int delayMillis;            // remaining milliseconds to wait before the fade starts            
        public float fadeoutDeltaVolumePerUpdate;
        public float currentVolume;
    }


    internal class Logik
    {
        private static readonly string PSAI_VERSION = ".NET 1.6.0";

        // redundant in EditorModel.cs
        internal static float COMPATIBILITY_PERCENTAGE_SAME_GROUP = 1.0f;
        internal static float COMPATIBILITY_PERCENTAGE_OTHER_GROUP = 0.5f;
        //

        /// <summary>
        /// The total number of channels count including Highlights
        /// </summary>
        internal static readonly int PSAI_CHANNEL_COUNT = 9;
        internal static readonly int PSAI_CHANNEL_COUNT_HIGHLIGHTS = 2;
        internal static readonly int PSAI_FADING_UPDATE_INVERVAL_MILLIS = 50;

        internal static readonly int PSAI_FADEOUTMILLIS_PLAYIMMEDIATELY = 500;			// fadeout time for the interrupted theme when a higher priority theme is played
        internal static readonly int PSAI_FADEOUTMILLIS_STOPMUSIC = 1000;			// fadeout time for stopMusic(immediately)
        internal static readonly int PSAI_FADEOUTMILLIS_HIGHLIGHT_INTERRUPTED = 2000;

        internal static readonly int SNIPPET_TYPE_MIDDLE_OR_BRIDGE = (int)SegmentSuitability.middle | (int)SegmentSuitability.bridge;

        #if !(PSAI_NOLOG)
            public static string LOGMESSAGE_TRIGGER_IGNORED = "trigger ignored! A Theme of higher priority is currently playing.";
            public static string LOGMESSAGE_TRIGGER_IGNORED_INTENSITYZEROTHEME_ALREADY_SET = "trigger ignored ! an IntensityZeroTheme with higher priority has already been set.";
            public static string LOGMESSAGE_ABORTION_DUE_TO_INITIALIZATION_FAILURE = "abortion due to sound device failure";
	        public static string LOGMESSAGE_TRIGGER_IGNORED_ZEROINTENSITY_AND_THEME_ISNT_CURRENTLY_PLAYING = "trigger ignored ! (triggers of zero intensity are ignored as long as the tiggered theme is not playing right now)";
	        public static string LOGMESSAGE_NO_THEME_SWITCH_THEME_OF_HIGHER_PRIORIY_IS_PLAYING	= "no theme switch: a theme of higher priority is currently playing";
	        public static string LOGMESSAGE_TRIGGER_DENIED	= "Trigger denied ! No compatible Segment found for the Segments that's currently playing.";
        #endif


        // Singleton
        private static Logik s_instance;
        internal static Logik Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new Logik();
                }
                return s_instance;
            }
        }


        private static System.Random s_random;

        //string m_protobufAudioFormatString;

        internal Soundtrack m_soundtrack;

        List<FadeData> m_fadeVoices;
        int m_currentVoiceNumber;			// of the Segment currently playing
        
        int m_targetVoice;                  // Nummer der als nächstens zu belegenen FMod Stimme 

        //LogLevel m_logLevel;

        //AudioPlaybackLayer m_audioPlaybackLayer;
        IPlatformLayer m_platformLayer;

        bool m_initializationFailure;
        internal string m_psaiCoreBinaryFilepath;
        internal string m_psaiCoreBinaryDirectoryName;

        //List<string> m_themeNames;

        private static Stopwatch m_stopWatch;              // used to count the milliseconds since start

        Theme m_lastBasicMood;

        int m_hilightVoiceIndex;                        // Stimmenverwaltung für Highlight

        int m_lastRegularVoiceNumberReturned;            // this was integrated in Version 1.5.16 to make sure that the voice id returned by getNextVoiceId() is also increased if another voice id is being requested (caused by an immediate trigger call) while the an asynchronous load for a channel is still in progress. This resulted in giving out the same channel id twice, messing with the channel state.

        float m_psaiMasterVolume;

        /* the Segment that is playing right now. m_currentSnippetPlaying is set exactly when its playback starts, including the Pre Enter Zone. */
		Segment m_currentSegmentPlaying;								
		int m_currentSnippetTypeRequested;		        /* the type of the snippet that should be currently playing. This may differ from m_currentSnippetPlaying->snippetType, as missing snippets may be substituted with other snippet types */
		Theme m_effectiveTheme;						    /* the theme currently active. Switches like the effective Snippet, but stays valid in rest mode*/	
	
        int m_timeStampCurrentSnippetPlaycall;		        /* the millisElapsedSinceInitialisation when the playback of the currentSnippet was started */
        int m_estimatedTimestampOfTargetSnippetPlayback;
        int m_timeStampOfLastIntensitySetForCurrentTheme;			        // is taken whenever the intensitySlope for a Theme has been set or reset, like when the Theme has started to play or was triggered again. 
        int m_timeStampRestStart;					        // point of time when rest mode had been entered        

		Segment m_targetSegment;						// the very next snippet to be played after the current one (needs to be preloaded)
		int m_targetSegmentSuitabilitiesRequested;					/* the logically correct snippettypes of the targetSnippet. This may differ from m_targetSnippet->snippetType, as missing snippets may be substituted */
		
		float m_currentIntensitySlope;				// the slope that defines how quickly the intensity will fall during the current playback
		float m_lastIntensity;						// the intensity returned in the last call to getCurrentIntensity. Needed to avoid undefined situations when a targetSnippet has already been set		
		bool m_holdIntensity;						// if set to true, getCurrentIntensity() will return the a constant value
		float m_heldIntensity;						// the constant intensity value to return if m_holdIntensity is set
		bool m_scheduleFadeoutUponSnippetPlayback;      // this flag needs to be set if we interrupt a theme and want to fadeout the previous voice as soon as the next Snippet has been loaded.

		float m_startOrRetriggerIntensityOfCurrentTheme;	// the initial intensity with which the current theme was triggered
        int m_lastMusicDuration;
        int m_remainingMusicDurationAtTimeOfHoldIntensity;


		PsaiState m_psaiState;						// the current psaiState
		PsaiState m_psaiStateIntended;				// used for state change requests , e.g. to enter silence mode after the end-snippet has been played
		//bool m_forceZeroIntensity;					// flag to indicate that returnToLastBasicMood() was manually triggered
		List<ThemeQueueEntry> m_themeQueue;		// the queue of themes and their intensity, that will be switched to when the current theme's intensity has reached zero
		PsaiPlayMode m_psaiPlayMode;				// regular, menu mode or cutscene ?	will be set right before PlayTheme is called.
		PsaiPlayMode m_psaiPlayModeIntended;		// when leaving a Cutscene not immediately, the playmode will stay on "cutscene" until SnippetPlaybackInitiated of the first regular theme.
													// m_psaiPlayModeIntended will be set immediately as soon as CutSceneEnter() (or CutSceneLeave() resp.) was called.

		//ErrorHandling m_errorHandling;				// Abort on missing snippets, or try to recover ?		
		bool m_returnToLastBasicMoodFlag;

        string m_fullVersionString;                 // holds the full version of the psai instance, including audiolayer and its version number

        
        //--------------------------------------------
        // rework
        //--------------------------------------------

        PlaybackChannel[] m_playbackChannels = new PlaybackChannel[PSAI_CHANNEL_COUNT];

        internal static int s_audioLayerMaximumLatencyForPlayingbackPrebufferedSounds = 50;     // 200
        internal static int s_audioLayerMaximumLatencyForBufferingSounds = 200;      // 500

        internal static int s_audioLayerMaximumLatencyForPlayingBackUnbufferedSounds;
        internal static int s_updateIntervalMillis = 100;   // this are the guaranteed update inverval in which psaiCore::update() is called. TODO: this should be calculated or read out!


        PsaiTimer m_timerStartSnippetPlayback = new PsaiTimer();
        PsaiTimer m_timerSegmentEndApproaching = new PsaiTimer();
        PsaiTimer m_timerSegmentEndReached = new PsaiTimer();
        PsaiTimer m_timerFades = new PsaiTimer();
        PsaiTimer m_timerWakeUpFromRest = new PsaiTimer();

        int m_timeStampOfLastFadeUpdate;

        /* this is used to store calls to triggerMusicTheme, that did not result in immediate Theme transitions.
         * Instead we store / update the trigger with the highest priority, and process (and reset) it in SnippetEndApproachingHandler() */
        ThemeQueueEntry m_nonInterruptingTriggerOfHighestPriority = new ThemeQueueEntry();


        bool m_paused = false;
        int m_timeStampPauseOn;

        /* as long as this value is > 0 , it will be used to define the Rest Mode duration*/
        int m_restModeSecondsOverride;

        //-------------------------------

        static Logik()
        {
            s_random = new System.Random();
            GetTimestampMillisElapsedSinceInitialisation();      // called once to init the stopwatch
            UpdateMaximumLatencyForPlayingBackUnbufferedSounds();            
        }


        internal void Release()
        {
            for (int i = 0; i < m_playbackChannels.Length; i++)
            {
                m_playbackChannels[i].Release();
            }

            m_platformLayer.Release();
        }

        /// <summary>
        /// Returns a random number within a specified range. The 'max' value will not be included in the results (apparently)
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static int GetRandomInt(int min, int max)
        {            
            int randomNumber = s_random.Next(min, max);
            return randomNumber;
        }

        // returns a random float value between 0.0f and 1.0f
        internal static float GetRandomFloat()
        {
            float result = (float)s_random.NextDouble();
            return result;
        }


        private static void UpdateMaximumLatencyForPlayingBackUnbufferedSounds()
        {
            s_audioLayerMaximumLatencyForPlayingBackUnbufferedSounds = s_audioLayerMaximumLatencyForBufferingSounds + s_audioLayerMaximumLatencyForPlayingbackPrebufferedSounds;
        }


        internal PsaiResult SetMaximumLatencyNeededByPlatformToBufferSounddata(int latencyInMilliseconds)
        {
            if (latencyInMilliseconds >= 0)
            {
                s_audioLayerMaximumLatencyForBufferingSounds = latencyInMilliseconds;
                UpdateMaximumLatencyForPlayingBackUnbufferedSounds();

                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.info <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("Maximum latency needed by platform for buffering audio data set to ");
	                    sb.Append(latencyInMilliseconds);
	                    Logger.Instance.Log(sb.ToString(), LogLevel.info);
                    }
                }
                #endif

                return PsaiResult.OK;
            }
            return PsaiResult.invalidParam;
        }

        internal PsaiResult SetMaximumLatencyNeededByPlatformToPlayBackBufferedSounds(int latencyInMilliseconds)
        {
            if (latencyInMilliseconds >= 0)
            {
                s_audioLayerMaximumLatencyForBufferingSounds = latencyInMilliseconds;
                UpdateMaximumLatencyForPlayingBackUnbufferedSounds();

                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.info <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("Maximum latency needed by platform for playing back buffered audio data set to ");
	                    sb.Append(latencyInMilliseconds);
	                    Logger.Instance.Log(sb.ToString(), LogLevel.info);
                    }
                }
                #endif

                return PsaiResult.OK;
            }
            return PsaiResult.invalidParam;
        }


        internal Logik()
        {
            #if PSAI_STANDALONE
                m_platformLayer = new PlatformLayerStandalone(this);
            #else
                m_platformLayer = new PlatformLayerUnity();
            #endif

            m_platformLayer.Initialize();

            m_soundtrack = new Soundtrack();
            m_themeQueue = new List<ThemeQueueEntry>();                        
            m_fadeVoices = new List<FadeData>();

            for (int i = 0; i < PSAI_CHANNEL_COUNT; i++)
            {
                m_playbackChannels[i] = new PlaybackChannel();
            }


            m_hilightVoiceIndex = -1;
            m_lastRegularVoiceNumberReturned = -1;
            m_currentVoiceNumber = -1;
            m_targetVoice = -1;

            m_psaiMasterVolume = 1.0f;  		   

            m_effectiveTheme = null;
            m_currentSegmentPlaying = null;
            m_currentSnippetTypeRequested = 0; 
            m_targetSegment = null;
            m_targetSegmentSuitabilitiesRequested = 0;

            m_psaiState = PsaiState.notready;
            m_psaiStateIntended = PsaiState.notready;

            m_paused = false;
            
            m_fullVersionString = "psai Version " + PSAI_VERSION;
            #if !(PSAI_NOLOG)
                Logger.Instance.LogLevel = LogLevel.info;
                Logger.Instance.Log(m_fullVersionString, LogLevel.info);
            #endif


            s_instance = this;
        }

        ~Logik()
        {
        }


        internal Logik(string pathToPcbFile) : this()
        {
            LoadSoundtrack(pathToPcbFile);
        }

       
        internal PsaiResult LoadSoundtrackFromProjectFile(string pathToProjectFile)
        {

            psai.Editor.PsaiProject project = null;

            m_psaiCoreBinaryFilepath = pathToProjectFile;
            m_psaiCoreBinaryDirectoryName = Path.GetDirectoryName(pathToProjectFile);

            m_initializationFailure = false;

            using (Stream stream = m_platformLayer.GetStreamOnPsaiCoreBinary(m_psaiCoreBinaryFilepath))
            {
                project = psai.Editor.PsaiProject.LoadProjectFromStream(stream);

                if (project != null)
                {
                    return LoadSoundtrackByPsaiProject(project, pathToProjectFile);
                }                
                else
                {
                    #if !(PSAI_NOLOG)
                        Logger.Instance.Log("failed to load Project!", LogLevel.errors);
                    #endif

                    return PsaiResult.error_file;
                }
            }  
        }

        public PsaiResult LoadSoundtrackByPsaiProject(psai.Editor.PsaiProject psaiProject, string fullProjectPath)
        {
            m_soundtrack = psaiProject.BuildPsaiDotNetSoundtrackFromProject();

            m_psaiCoreBinaryFilepath = fullProjectPath;
            m_psaiCoreBinaryDirectoryName = Path.GetDirectoryName(fullProjectPath);

            InitMembersAfterSoundtrackHasLoaded();

            #if !(PSAI_NOLOG)
            {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
                    Logger.Instance.Log("Soundtrack deserialization succeeded", LogLevel.info);
                }
            }
            #endif

            return PsaiResult.OK;
        }


        internal PsaiResult LoadSoundtrack(string pathToPcbFile)
        {            
            m_psaiCoreBinaryFilepath = pathToPcbFile;
            m_psaiCoreBinaryDirectoryName = Path.GetDirectoryName(m_psaiCoreBinaryFilepath);

		    m_initializationFailure = false;

            m_soundtrack = new Soundtrack();

            PsaiResult psaiResultReadfile = PsaiResult.none;
            using (Stream stream = m_platformLayer.GetStreamOnPsaiCoreBinary(m_psaiCoreBinaryFilepath))
            {
                psaiResultReadfile = Readfile_ProtoBuf(stream);
                if (psaiResultReadfile != PsaiResult.OK)
                {
                    #if !(PSAI_NOLOG)
                        Logger.Instance.Log("failed to load Soundtrack! error=" + psaiResultReadfile, LogLevel.errors);
                    #endif

                    return psaiResultReadfile;
                }
                else
                {
                    #if !(PSAI_NOLOG)
                        Logger.Instance.Log("Soundtrack deserialization succeeded", LogLevel.info);
                    #endif
                }
            }       
            
            InitMembersAfterSoundtrackHasLoaded();
            return PsaiResult.OK;
        }

        private void InitMembersAfterSoundtrackHasLoaded()
        {
            m_themeQueue.Clear();
            m_fadeVoices.Clear();


            foreach (Segment segment in m_soundtrack.m_snippets.Values)
            {
                segment.audioData.filePathRelativeToProjectDir = m_platformLayer.ConvertFilePathForPlatform(segment.audioData.filePathRelativeToProjectDir);
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("converted path of segment " + segment.Name + " to " + segment.audioData.filePathRelativeToProjectDir, LogLevel.debug);
                    }
                }
                #endif
            }

            m_soundtrack.UpdateMaxPreBeatMsOfCompatibleMiddleOrBridgeSnippets();
            m_lastBasicMood = m_soundtrack.getThemeById(GetLastBasicMoodId());

            m_psaiState = PsaiState.silence;
            m_psaiStateIntended = PsaiState.silence;
            m_psaiPlayMode = PsaiPlayMode.regular;
            m_psaiPlayModeIntended = PsaiPlayMode.regular;

            m_returnToLastBasicMoodFlag = false;
            m_holdIntensity = false;

            m_nonInterruptingTriggerOfHighestPriority.themeId = -1;

            m_soundtrack.BuildAllIndirectionSequences();
        }


        internal void setLogLevel(LogLevel newLogLevel)
	    {
			#if !(PSAI_NOLOG)
			{
                if (LogLevel.debug <= newLogLevel)
                {
                    Logger.Instance.Log("setLogLevel() " + newLogLevel, LogLevel.debug);
                }
			}
			#endif

			Logger.Instance.LogLevel = newLogLevel;
        }


        /** returns the id of the theme set as baseTheme. 
	    * If no baseTheme has been triggered yet, the baseTheme with the lowest id is returned, or -1 if no base theme was found at all.
	    */
        internal int GetLastBasicMoodId()
	    {
		    if (m_lastBasicMood != null)
		    {
			    return m_lastBasicMood.id;
		    }
		    else
		    {
                foreach(Theme tmpThema in m_soundtrack.m_themes.Values)
                {
                    if (tmpThema.themeType == ThemeType.basicMood)
                    {
                        m_lastBasicMood = tmpThema;
                        return tmpThema.id;
                    }
                }
                return -1;
            }		
	    }


        internal static bool CheckIfFileExists(string filepath)
        {
            return File.Exists(filepath);
        }


       	int GetRemainingMusicDurationSecondsOfCurrentTheme()
        {
            int millisElapsedSinceLastRetrigger = GetTimestampMillisElapsedSinceInitialisation() - m_timeStampOfLastIntensitySetForCurrentTheme;
            int result = m_lastMusicDuration - (int)(millisElapsedSinceLastRetrigger / 1000);

            #if !(PSAI_NOLOG)
            {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("GetRemainingMusicDurationSecondsOfCurrentTheme=");
	                sb.Append(result);
	                sb.Append("  millisElapsedSinceLastRetrigger=");
	                sb.Append(millisElapsedSinceLastRetrigger);
	                sb.Append("  m_lastMusicDuration=");
	                sb.Append(m_lastMusicDuration);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
            }
            #endif

            return result;
        }



        internal static int GetTimestampMillisElapsedSinceInitialisation()
        {
            if (m_stopWatch == null)
            {
                m_stopWatch = new Stopwatch();
                m_stopWatch.Start();
            }

            return (int)m_stopWatch.ElapsedMilliseconds;
        }



        private PsaiResult Readfile_ProtoBuf(System.IO.Stream stream)
	    {

            if (stream == null)
            {
                return PsaiResult.file_notFound;
            }
            else
            {
                m_soundtrack.Clear();
                m_themeQueue.Clear();

                psai.ProtoBuf_PsaiCoreSoundtrack pbSoundtrack;

                try
                {
	                pbSoundtrack = ProtoBuf.Serializer.Deserialize<psai.ProtoBuf_PsaiCoreSoundtrack>(stream);
                }
                catch (System.Exception ex)
                {
                    Logger.Instance.Log(ex.Message, LogLevel.errors);
                    return PsaiResult.error_file;
                }

                                
                //m_protobufAudioFormatString = pbSoundtrack.audioformat;


                m_soundtrack = new Soundtrack(pbSoundtrack);

                return PsaiResult.OK;
            }
		}

        internal string getVersion()
        {
            return m_fullVersionString;
        }

        // returns a timestamp in milliseconds since initialization (same as 
        // GetTimeMillisElapsedSinceInitialisation() on this platform)
        internal long GetCurrentSystemTimeMillis()
        {
            return GetTimestampMillisElapsedSinceInitialisation();
        }


        /* inits a fadeout for a given voice and starts the updateFades-timer if not running already.
		@param fadeoutMillis the total fadeout timespan in millis
		@param timeOffsetMillis a time offset in milliseconds by which the fadeout will be delayed
	    */
	    void startFade(int voiceId, int fadeoutMillis, int timeOffsetMillis)
	    {

		    if (voiceId > -1)
		    {
			    //boost::recursive_mutex::scoped_lock blockieren(m_pnxLogicMutex);                

			    #if !(PSAI_NOLOG)
			    {
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("startFade()  voiceId=");
	                    sb.Append(voiceId);
	                    sb.Append("  fadeoutMillis=");
	                    sb.Append(fadeoutMillis);
	                    sb.Append("  timeOffsetMillis=");
	                    sb.Append(timeOffsetMillis);
	                    sb.Append("  channelState=");
	                    sb.Append(m_playbackChannels[voiceId].GetCurrentChannelState());
	                    Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                    }
			    }
			    #endif

			    float currentVol = m_playbackChannels[voiceId].FadeOutVolume;	 

			    // check if a fade of the current voice is currently already active			    
                foreach(FadeData fadeData in m_fadeVoices)
                {
                    if (fadeData.voiceNumber == voiceId)
                    {
                        #if !(PSAI_NOLOG)
					    {
                            if (LogLevel.debug <= Logger.Instance.LogLevel)
                            {
                            	Logger.Instance.Log("startFade canceled, because a fadeout for this voiceId already exists", LogLevel.debug);						    
                            }
					    }
					    #endif

                        return;
                    }
                }
                
			    if (currentVol > 0)
			    {
				    AddFadeData(voiceId, fadeoutMillis, currentVol, timeOffsetMillis);

				    // start the fade timer if needed
                    bool isTimerSet = m_timerFades.IsSet();
				    if (isTimerSet == false)
				    {
                        #if !(PSAI_NOLOG)
                        {
                            if (LogLevel.debug <= Logger.Instance.LogLevel)
                            {
                            	Logger.Instance.Log("setting timerFades", LogLevel.debug);
                            }
                        }
                        #endif

                        m_timeStampOfLastFadeUpdate = Logik.GetTimestampMillisElapsedSinceInitialisation();
                        m_timerFades.SetTimer(Logik.PSAI_FADING_UPDATE_INVERVAL_MILLIS, 0);                            
				    }
				    else
				    {
					    #if !(PSAI_NOLOG)
					    {
                            if (LogLevel.debug <= Logger.Instance.LogLevel)
                            {
                            	Logger.Instance.Log("timer PSAI_TIMER_FADES is already set", LogLevel.debug);
                            }
					    }
					    #endif
				    }
			    }
			    else
			    {
				    #if !(PSAI_NOLOG)
				    {
                        if (LogLevel.debug <= Logger.Instance.LogLevel)
                        {
	                        StringBuilder sb = new StringBuilder();
	                        sb.Append("INTERNAL WARNING: startFade() ignored: m_audioPlaybackLayer->getVolume() returned currentVol=");                        
	                        sb.Append(currentVol);
	                        Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                        }
				    }
				    #endif
			    }

		     }
	    }// startFade



        /* adds a fadeData instance to the fadeVoices collection
	    * @param fadeoutMillis the total fadeout time in milliseconds
	    * @param currentVolume the volume of the voice when the fade starts.
	    * @param delayMillis a time offset in milliseconds by which the fadeout will be delayed
	    **/
	    void AddFadeData(int voiceNumber, int fadeoutMillis, float currentVolume, int delayMillis)
	    {
		    FadeData fadeData = new FadeData();
		    fadeData.voiceNumber = voiceNumber;
		    fadeData.fadeoutDeltaVolumePerUpdate = currentVolume / (fadeoutMillis / (float) Logik.PSAI_FADING_UPDATE_INVERVAL_MILLIS);
		    fadeData.currentVolume = currentVolume;
		    fadeData.delayMillis = delayMillis;
		    m_fadeVoices.Add(fadeData);
	    }


        /* returns the next voice index (Round Robin), either for highlights or regular channels.
	    * This method only checks for channel index boundaries and ignores the playback states of the channels.
	    */
        internal int getNextVoiceNumber(bool forHighlight)
        {
            int nextVoiceNumber = 0;

            if (!forHighlight)
            {
                nextVoiceNumber = m_lastRegularVoiceNumberReturned + 1;
                if (nextVoiceNumber >= PSAI_CHANNEL_COUNT - PSAI_CHANNEL_COUNT_HIGHLIGHTS)
                {
                    nextVoiceNumber = 0;
                }

                m_lastRegularVoiceNumberReturned = nextVoiceNumber;
            }
            else
            {
                nextVoiceNumber = m_hilightVoiceIndex + 1;
                if (nextVoiceNumber == 0 || nextVoiceNumber == PSAI_CHANNEL_COUNT)
                {
                    nextVoiceNumber = PSAI_CHANNEL_COUNT - PSAI_CHANNEL_COUNT_HIGHLIGHTS;
                }
            }

            #if !(PSAI_NOLOG)
            {
                /*
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("getNextVoiceNumber() forHighlight=");
	                sb.Append(forHighlight);
	                sb.Append("  ...returning ");
	                sb.Append(nextVoiceNumber);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
                 */
            }
            #endif

            return nextVoiceNumber;
        }


        void PsaiErrorCheck(PsaiResult result, string infoAboutLastCall)
	    {
		    if (result != PsaiResult.OK)
		    {		
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("PSAI error!");
                sb.AppendLine(infoAboutLastCall);
                sb.Append("PsaiResult=");
                sb.Append(result);

                Logger.Instance.Log(sb.ToString(), LogLevel.errors);
		    }
	    }


        int GetMillisElapsedAfterCurrentSnippetPlaycall()
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

            if (m_currentSegmentPlaying != null)
            {
                if (!m_paused)
                {
                    return (int)(GetTimestampMillisElapsedSinceInitialisation() - m_timeStampCurrentSnippetPlaycall);
                }
                else
                {
                    return (m_timeStampPauseOn - m_timeStampCurrentSnippetPlaycall);
                }                
            }
            else
            {
                return 0;
            }
    	}

        internal PsaiResult setPaused(bool setPaused)
        {
            #if !(PSAI_NOLOG)
            {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {           
	                StringBuilder sb = new StringBuilder();
	                sb.Append("Logik.setPaused(");
	                sb.Append(setPaused);
	                sb.Append(") ");
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
            }
            #endif

            if ((setPaused && !m_paused) || (!setPaused && m_paused))
            {
                m_paused = setPaused;
                foreach (PlaybackChannel channel in m_playbackChannels)
                {
                    channel.Paused = setPaused;
                }

                m_timerStartSnippetPlayback.SetPaused(setPaused);
                m_timerSegmentEndApproaching.SetPaused(setPaused);
                m_timerSegmentEndReached.SetPaused(setPaused);
                m_timerWakeUpFromRest.SetPaused(setPaused);

                if (setPaused)
                {
                    m_timeStampPauseOn = GetTimestampMillisElapsedSinceInitialisation();
                    m_lastIntensity = getCurrentIntensity();
                }
                else
                {
                    int pausedPeriod = GetTimestampMillisElapsedSinceInitialisation() - m_timeStampPauseOn;
                    int playbackPeriod = m_timeStampPauseOn - m_timeStampCurrentSnippetPlaycall;
                    m_timeStampCurrentSnippetPlaycall = GetTimestampMillisElapsedSinceInitialisation() - playbackPeriod;
                    m_timeStampOfLastIntensitySetForCurrentTheme += pausedPeriod;
                }

                return PsaiResult.OK;
            }

            return PsaiResult.commandIgnored;
        }


        internal PsaiResult Update()
	    {
            if (!m_paused)
            {
                if (m_timerStartSnippetPlayback.ThresholdHasBeenReached())
                {                    
                    m_timerStartSnippetPlayback.Stop();
                    PlayTargetSegmentImmediately();
                }


                if (m_timerSegmentEndApproaching.ThresholdHasBeenReached())
                {
                    m_timerSegmentEndApproaching.Stop();
                    SegmentEndApproachingHandler();
                }


                if (m_timerSegmentEndReached.ThresholdHasBeenReached())
                {
                    m_timerSegmentEndReached.Stop();
                    SegmentEndReachedHandler();
                }


                if (m_timerWakeUpFromRest.ThresholdHasBeenReached())
                {
                    m_timerWakeUpFromRest.Stop();
                    WakeUpFromRestHandler();
                }

                if (m_timerFades.ThresholdHasBeenReached())
                {
                    m_timerFades.Stop();
                    updateFades();
                }
            }


		    return PsaiResult.OK;
	    }


        void SetThemeAsLastBasicMood(Theme latestBasicMood)
	    {
		    if (latestBasicMood != null)
		    {
			    #if !(PSAI_NOLOG)
			    {
                    if (m_lastBasicMood == null || (m_lastBasicMood != null && m_lastBasicMood!= latestBasicMood))
                    if (LogLevel.info <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log( "Setting Theme " + latestBasicMood.id + " as the Last Basic Mood", LogLevel.info);
                    }
			    }
			    #endif

			    m_lastBasicMood = latestBasicMood;
		    }
		    else
		    {
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log("SetThemeAsLatestBasicMood(): invalid theme argument ! ", LogLevel.warnings);
                    }
                }
                #endif
            }
	    }


        // returns true if any snippet is currently playing, and m_currentTheme is currently set.
	    // Note: There are situations where the music logically stopped (like after StopMusic(immediately), but a fadeout
	    // is still in progress. CheckIfAnyThemeIsCurrentlyPlaying() will return false in these cases.
	    bool CheckIfAnyThemeIsCurrentlyPlaying()
	    {
		    if (m_psaiState == PsaiState.playing)
		    {			
			    if (m_currentSegmentPlaying != null && m_effectiveTheme != null)
				    return true;
		    }

		    return false;
	    }



        internal PsaiResult ReturnToLastBasicMood(bool immediately)
	    {				
		    #if !(PSAI_NOLOG)
                {
                    if (LogLevel.info <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("ReturnToBasicMood(");
	                    sb.Append(immediately);
	                    sb.Append(")  m_psaiState=");
	                    sb.Append(m_psaiState);
	                    sb.Append("  m_currentSnippetTypesRequested=");
	                    sb.Append(Segment.GetStringFromSegmentSuitabilities(m_currentSnippetTypeRequested));
	                    Logger.Instance.Log(sb.ToString(), LogLevel.info);
                    }
                }
		    #endif

		    if (m_initializationFailure)
		    {
			    return PsaiResult.initialization_error;
		    }

		    //////////////////////////////////////////////////////////////////////////

            if (m_paused)
            {
                setPaused(false);
            }


		    if (m_psaiPlayModeIntended == PsaiPlayMode.regular)
		    {
			    switch(m_psaiState)
			    {
                    case PsaiState.playing:
				    {
                        m_themeQueue.Clear();
                        m_holdIntensity = false;
                        m_nonInterruptingTriggerOfHighestPriority.themeId = -1;                        

						if (m_currentSegmentPlaying != null && m_effectiveTheme.themeType != ThemeType.basicMood)
						{
							bool validPathToEndSnippetExists = false;					
							if (!immediately)
							{
								validPathToEndSnippetExists = CheckIfThereIsAPathToEndSegmentForEffectiveSegmentAndLogWarningIfThereIsnt();
							}

							if (immediately || !validPathToEndSnippetExists)
							{
								PlayThemeNowOrAtEndOfCurrentSegment(GetLastBasicMoodId(), m_lastBasicMood.intensityAfterRest, m_lastBasicMood.musicDurationGeneral, true, false);
							}
							else
							{		
								m_psaiStateIntended = PsaiState.playing;		// in case we are interrupting a transition to SILENCE after StopMusic(0) was called
								m_returnToLastBasicMoodFlag = true;
							}
							return PsaiResult.OK;
						}
						else
						{
							#if !(PSAI_NOLOG)
								if (LogLevel.warnings <= Logger.Instance.LogLevel)
								{
									Logger.Instance.Log("ReturnToLastBasicMood() ignored: base theme is already playing", LogLevel.warnings);
								}
							#endif

							return PsaiResult.commandIgnored;
						}
					}
					//break;

				    case PsaiState.rest:
				    case PsaiState.silence:
					    {
						    PlayThemeNowOrAtEndOfCurrentSegment(GetLastBasicMoodId(), m_lastBasicMood.intensityAfterRest, m_lastBasicMood.musicDurationGeneral, true, false);
						    return PsaiResult.OK;
					    }
					    //break;

				    default:
                        {
                            #if !(PSAI_NOLOG)
                            {
                                if (LogLevel.debug <= Logger.Instance.LogLevel)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append("INTERNAL ERROR: unconsidered psaiState in ReturnToLastBasicMood()! m_psaiState=");
                                    sb.Append(m_psaiState);
                                    Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                                }
                            }
                            #endif
                            return PsaiResult.internal_error;
                        }
			    }
		    }
		    else
		    {
			    if (m_psaiPlayModeIntended == PsaiPlayMode.menuMode)
			    {
				    #if !(PSAI_NOLOG)
                        if (LogLevel.warnings <= Logger.Instance.LogLevel)
                        {
                        	Logger.Instance.Log("ReturnToLastBasicMood() ignored: MenuMode is active. Call MenuModeLeave() first.", LogLevel.warnings);
                        }
                    #endif

                    return PsaiResult.commandIgnoredMenuModeActive;
			    }
			    else if (m_psaiPlayModeIntended == PsaiPlayMode.cutScene)
			    {
				    #if !(PSAI_NOLOG)
                        if (LogLevel.warnings <= Logger.Instance.LogLevel)
                        {
                        	Logger.Instance.Log("ReturnToLastBasicMood() ignored: CutScene is active. Call CutsceneLeave() first.", LogLevel.warnings);
                        }
                    #endif

                    return PsaiResult.commandIgnoredCutsceneActive;
			    }
		    }

		    return PsaiResult.internal_error;		// should never be reached
	    }
        

        // returns the themeId of the Theme that is coming up after the current Snippet.
        // Returns -1 if none is set.
        internal int getUpcomingThemeId()
	    {
            switch (m_psaiState)
            {
                case PsaiState.playing:
                    {
                        if (m_nonInterruptingTriggerOfHighestPriority.themeId != -1)
                        {
                            return m_nonInterruptingTriggerOfHighestPriority.themeId;
                        }
                        else
                        {
                            ThemeQueueEntry followingTqe = getFollowingThemeQueueEntry();
                            if (followingTqe != null)
                            {
                                return followingTqe.themeId;
                            }
                        }
                    }
                    break;
            }

		    return -1;
    	}


        internal PsaiResult TriggerMusicTheme(int argThemeId, float argIntensity)
        {
            Theme argTheme = m_soundtrack.getThemeById(argThemeId);

            // theme not found ?
            if (argTheme == null)
            {
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("TriggerMusicTheme() - Theme not found ! themeId=" + argThemeId, LogLevel.errors);
                    }
                }
                #endif

                return PsaiResult.unknown_theme;
            }
            else if (argTheme.m_segments.Count == 0)
		    {
			    #if !(PSAI_NOLOG)
			    {
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();				    
					    sb.Append("TriggerMusicTheme() - Theme ");
	                    sb.Append(argTheme.Name);
	                    sb.Append(" contains no Segments! ");
	                    Logger.Instance.Log(sb.ToString(), LogLevel.errors);
                    }
			    }
			    #endif

			    return PsaiResult.essential_segment_missing;
		    }

            #if !(PSAI_NOLOG)
            {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("TriggerMusicTheme() argThemeId=");
	                sb.Append(argThemeId);
	                sb.Append(" argIntensity=");
	                sb.Append(argIntensity);
	                sb.Append("  [");
	                sb.Append(argTheme.Name);
	                sb.Append("]");
	                Logger.Instance.Log(sb.ToString(), LogLevel.info);
                }
            }
            #endif
           

            return TriggerMusicTheme(argTheme, argIntensity, argTheme.musicDurationGeneral);
        }

        internal PsaiResult TriggerMusicTheme(int argThemeId, float argIntensity, int argMusicDuration)
        {
            Theme argTheme = m_soundtrack.getThemeById(argThemeId);

            // theme not found ?
            if (argTheme == null)
            {
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("TriggerMusicTheme() - theme not found ! themeId=" + argThemeId, LogLevel.warnings);
                    }
                }
                #endif

                return PsaiResult.unknown_theme;
            }

            #if !(PSAI_NOLOG)
            {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("TriggerMusicTheme() argThemeId=");
	                sb.Append(argThemeId);
	                sb.Append(" argIntensity=");
	                sb.Append(argIntensity);
	                sb.Append("  [");
	                sb.Append(argTheme.Name);
	                sb.Append("]");
	                sb.Append("  argMusicDurationInSeconds=");
	                sb.Append(argMusicDuration);
	                Logger.Instance.Log(sb.ToString(), LogLevel.info);
                }
            }
            #endif


            return TriggerMusicTheme(argTheme, argIntensity, argMusicDuration);
        }


        internal PsaiResult TriggerMusicTheme(Theme argTheme, float argIntensity, int argMusicDuration)
        {
            if (m_initializationFailure)
            {
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log(LOGMESSAGE_ABORTION_DUE_TO_INITIALIZATION_FAILURE, LogLevel.errors);
                    }
                }
                #endif

                return PsaiResult.initialization_error;
            }

		    //////////////////////////////////////////////////////////////////////////

            if (m_paused)
            {
                setPaused(false);
            }

            Segment effectiveSegment = GetEffectiveSegment();

            // special treatment for Highlights
            if (argTheme.themeType == ThemeType.highlightLayer)
            {
   				if (CheckIfAnyThemeIsCurrentlyPlaying() == true)
				{
					if (m_effectiveTheme != null && effectiveSegment != null && effectiveSegment.CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(m_soundtrack, argTheme.id) == false)
					{
						#if !(PSAI_NOLOG)
						{
                            Logger.Instance.Log(LOGMESSAGE_TRIGGER_DENIED, LogLevel.warnings);
						}
						#endif	
						return PsaiResult.triggerDenied;
					};
				}

				return startHighlight(argTheme);
            }


		    // return immediately in menu mode
		    if (m_psaiPlayMode == PsaiPlayMode.menuMode)
		    {

			    #if !(PSAI_NOLOG)
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("TriggerMusicTheme() ignored: Menu Mode is active", LogLevel.warnings);
                    }
			    #endif

			    return PsaiResult.commandIgnoredMenuModeActive;
		    }

		    else if (m_psaiPlayModeIntended == PsaiPlayMode.cutScene)
		    {
			    #if !(PSAI_NOLOG)
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("TriggerMusicTheme() ignored: Cutscene is active", LogLevel.warnings);
                    }
			    #endif

			    return PsaiResult.commandIgnoredCutsceneActive;
		    }

		    else if (m_psaiPlayMode == PsaiPlayMode.cutScene && m_psaiStateIntended == PsaiState.silence && m_currentSegmentPlaying != null)
		    {
			    #if !(PSAI_NOLOG)
			    {
                    if (LogLevel.info <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("special case: Cutscene Theme is still playing, continuing with theme " + argTheme.Name, LogLevel.info);
                    }
			    }
			    #endif

			    m_psaiState = PsaiState.playing;
			    m_psaiStateIntended = PsaiState.playing;

                return PlayThemeNowOrAtEndOfCurrentSegment(argTheme.id, argIntensity, argMusicDuration, true, false);						
		    }




		    //////////////////////////////////////////////////////////////////////////
		    // regular Mode
		    //////////////////////////////////////////////////////////////////////////


            // if we trigger a BasicMood shortly after ReturnToBasicMood(by End) has been called, we don't want to cancel the
            // return, but instead switch the Last Basic Mood to return to. In all other cases, cancel the Return process.
            if (m_returnToLastBasicMoodFlag)
            {
                if (argTheme.themeType != ThemeType.basicMood)
                {
                    m_returnToLastBasicMoodFlag = false;
                }                
            }


            if (argTheme.themeType == ThemeType.basicMood)
		    {
			    SetThemeAsLastBasicMood(argTheme);
		    }


            // always clear the Theme Queue, as the most recent trigger call is the one that counts.
            removeFirstFollowingThemeQueueEntry();

		
		    // nothing is playing -> play immediately
		    if (effectiveSegment == null || m_psaiState == PsaiState.silence || m_psaiState == PsaiState.rest)
		    {
                return PlayThemeNowOrAtEndOfCurrentSegment(argTheme.id, argIntensity, argMusicDuration, true, false);
		    }


            // special case: StopMusic(by End Segment) is in progress, and a Theme of lower or same priority is triggered
		    // -> conduct a seamless transition.
		    if (m_psaiStateIntended == PsaiState.silence && effectiveSegment != null)
		    {
                Theme effectiveTheme1 = m_soundtrack.getThemeById(effectiveSegment.ThemeId);
			    ThemeInterruptionBehavior tib = Theme.GetThemeInterruptionBehavior(effectiveTheme1.themeType, argTheme.themeType);

                if (tib == ThemeInterruptionBehavior.at_end_of_current_snippet || tib == ThemeInterruptionBehavior.never)
			    {
				    m_psaiStateIntended = PsaiState.playing;
				    return PlayThemeNowOrAtEndOfCurrentSegment(argTheme.id, argIntensity, argMusicDuration, false, false);
			    }			
		    }


		    // the effective Theme was triggered again? 
		    if (effectiveSegment.ThemeId == argTheme.id)
		    {
			    #if !(PSAI_NOLOG)
			    {
                    /*
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("theme ");
	                    sb.Append(argTheme.Name);
	                    sb.Append(" is already playing and the SegmentEndApproaching timer is pending. Updating intensity to ");
	                    sb.Append(argIntensity);
	                    Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                    }
                    */
			    }
			    #endif
                
                m_nonInterruptingTriggerOfHighestPriority.themeId = -1;		// clear any non-interrupting trigger call we may have received recently!  

                SetCurrentIntensityAndMusicDuration(argIntensity, argMusicDuration, true);
                m_psaiStateIntended = PsaiState.playing;
                return PsaiResult.OK;
		    }

            Theme effectiveTheme = m_soundtrack.getThemeById(effectiveSegment.ThemeId);
		    ThemeInterruptionBehavior themeInteruptionBehavior = Theme.GetThemeInterruptionBehavior(effectiveTheme.themeType, argTheme.themeType);

		    switch (themeInteruptionBehavior)
		    {
		    case ThemeInterruptionBehavior.immediately:
			    {
                    // don't push the interrupted Theme on the stack if GoToRest() or StopMusic() has just been called.
                    if (argTheme.themeType == ThemeType.shock && m_psaiStateIntended == PsaiState.playing) 
                    {
                        PushEffectiveThemeToThemeQueue(PsaiPlayMode.regular);
                    }
                    else
                    {
                        m_nonInterruptingTriggerOfHighestPriority.themeId = -1;
                    }

                    return PlayThemeNowOrAtEndOfCurrentSegment(argTheme.id, argIntensity, argMusicDuration, true, false);                
			    }
			    //break;

		    case ThemeInterruptionBehavior.at_end_of_current_snippet:
			    {
				    return HandleNonInterruptingTriggerCall(argTheme, argIntensity, argMusicDuration);				
			    }
			    //break;

		    case ThemeInterruptionBehavior.never:
			    {
                    if (argTheme.themeType != ThemeType.basicMood)
				    {
					    pushThemeToThemeQueue(argTheme.id, argIntensity, argMusicDuration, false, 0, PsaiPlayMode.regular, false);

					    #if !(PSAI_NOLOG)
					    {
                            if (LogLevel.info <= Logger.Instance.LogLevel)
                            {
	                            StringBuilder sb = new StringBuilder();
	                            sb.Append("Theme ");
	                            sb.Append(argTheme.Name);
	                            sb.Append(" has been queued for direct playback after the intensity of the current Theme has dropped to zero.");
							    Logger.Instance.Log(sb.ToString(), LogLevel.info);
                            }
					    }
					    #endif
				    }
				    return PsaiResult.OK;
			    }
			    //break;
		    }

		

		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.errors <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("INTERNAL ERROR: end of TriggerMusicTheme() reached without returning a proper returnCode. ");
	                sb.Append("argThemeId=");
	                sb.Append(argTheme.id);
	                sb.Append(" m_currentTheme=");
	                sb.Append(m_effectiveTheme);
	                if (m_effectiveTheme != null)
	                {
	                    sb.Append(" m_currentTheme id=");
	                    sb.Append(m_effectiveTheme.id);
	                    sb.Append("  m_currentTheme themeType=");
	                    sb.Append(m_effectiveTheme.themeType);
	                }
	                Logger.Instance.Log(sb.ToString(), LogLevel.errors);
                }
		    }
		    #endif

		    return PsaiResult.internal_error;
        }





        /* clamps the argument to a value between 0.0f and 1.0f
         */
        internal static float ClampPercentValue(float argValue)
        {
            if (argValue > 1.0f)
                return 1.0f;

            if (argValue < 0.0f)
                return 0.0f;

            return argValue;
        }

        internal PsaiResult AddToCurrentIntensity(float deltaIntensity, bool resetIntensityFalloffToFullMusicDuration)
        {

            #if !(PSAI_NOLOG)
               if (LogLevel.debug <= Logger.Instance.LogLevel)
               {
               	    Logger.Instance.Log("AddToCurrentIntensity(" + deltaIntensity + ")", LogLevel.debug);
               }
            #endif


            if (m_psaiState == PsaiState.playing && m_psaiPlayMode == PsaiPlayMode.regular)
            {
                if (m_nonInterruptingTriggerOfHighestPriority.themeId != -1)
                {
                    m_nonInterruptingTriggerOfHighestPriority.startIntensity = ClampPercentValue(m_nonInterruptingTriggerOfHighestPriority.startIntensity + deltaIntensity);
                    Logger.Instance.Log("AddToCurrentIntensity(" + deltaIntensity + ") adding to nonInterruptingTrigger.startIntensity=" + m_nonInterruptingTriggerOfHighestPriority.startIntensity, LogLevel.debug);
                }
                else
                {
                    float newIntensity = ClampPercentValue(getCurrentIntensity() + deltaIntensity);
                    
                    SetCurrentIntensityAndMusicDuration(newIntensity, m_lastMusicDuration, resetIntensityFalloffToFullMusicDuration);
                }
                
                return PsaiResult.OK;
            }

            return PsaiResult.notReady;
        }



        internal PsaiResult PlaySegmentLayeredAndImmediately(int segmentId)
        {
            Segment segment = m_soundtrack.GetSegmentById(segmentId);

            if (segment != null)
            {
                PlaySegmentLayeredAndImmediately(segment);
            }

            return PsaiResult.invalidHandle;
        }



        /** Immediate playback of the given Segment on a separate layer, without affecting the playback logic.
         */
        internal void PlaySegmentLayeredAndImmediately(Segment segment)
        {
            m_hilightVoiceIndex = getNextVoiceNumber(true);

            m_playbackChannels[m_hilightVoiceIndex].StopChannel();
            m_playbackChannels[m_hilightVoiceIndex].ReleaseSegment();

            m_playbackChannels[m_hilightVoiceIndex].FadeOutVolume = 1.0f;
            m_playbackChannels[m_hilightVoiceIndex].ScheduleSegmentPlayback(segment, s_audioLayerMaximumLatencyForBufferingSounds + s_audioLayerMaximumLatencyForPlayingbackPrebufferedSounds);
        }



        /**
	    *                                                                      
	    * @return PSAI_OK ok hightlight is being played
		    PSAI_INFO_TRIGGER_IGNORED_LOW_PRIORITY  trigger ignored, because a highlight of a higher priority is currently being played.
		    PSAI_ERR_ESSENTIAL_SEGMENT_MISSING  ignored, because no compatible Segment could be found.			
	    */
	    PsaiResult startHighlight(Theme highlightTheme)
	    {
		    //boost::recursive_mutex::scoped_lock blockieren(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
			    if (LogLevel.debug <= Logger.Instance.LogLevel)
			    {
			    	Logger.Instance.Log("startHighlight()", LogLevel.debug);
			    }
		    #endif


		    // Highlights are always SNIPPET_TYPE_MIDDLE
		    if (highlightTheme.m_segments.Count > 0)
		    {

                // deine omma

			    Segment tempTeil;

                if (m_currentSegmentPlaying != null)
                {
                    tempTeil = GetBestCompatibleSegment(m_currentSegmentPlaying, highlightTheme.id, getCurrentIntensity(), (int)SegmentSuitability.whatever);
                }
			    else 
			    {
                    int randomSegmentIndex = GetRandomInt(0, highlightTheme.m_segments.Count);  // note: GetRandomInt will never return the max value.
                    tempTeil = m_soundtrack.GetSegmentById(highlightTheme.m_segments[randomSegmentIndex].Id);
			    }

			    if (tempTeil != null)
			    {	
				    //PsaiResult psaiResult;

                    // TODO: check if we need to fade out highlights

				    // is currently a highlight playing, has it a lower priority and is in need of a fade?
                    /*
				    bool playing = false;
				    if (m_hilightVoiceIndex > -1)
				    {
					    psaiResult = m_audioPlaybackLayer->isPlaying(m_hilightVoiceIndex, &playing);
					    if (playing) 
					    {
						    if ( m_priorityOfCurrentHighlight > highlightTheme->priority) 
							    return PSAI_RC_TRIGGER_IGNORED_LOW_PRIORITY;	// no need to do anything further
						    else 
							    startFade(m_hilightVoiceIndex, PSAI_FADEOUTMILLIS_HIGHLIGHT_INTERRUPTED);
					    }
				    }
                     */

                    PlaySegmentLayeredAndImmediately(tempTeil);

                    tempTeil.Playcount++;

				    return PsaiResult.OK;
			    }
			    else
			    {
				    #if !(PSAI_NOLOG)
				    {
                        if (LogLevel.warnings <= Logger.Instance.LogLevel)
                        {
	                        StringBuilder sb = new StringBuilder();
	                        sb.Append("the triggered Highlight Layer ");
	                        sb.Append(highlightTheme.id);
	                        sb.Append(" does not contain a compatible Highlight Segment.");
	                        if (m_currentSegmentPlaying != null)
	                        {
	                            sb.Append(" Current Segment playing:");
	                            sb.Append(m_currentSegmentPlaying.Name);
	                        }
	                        else
	                        {
	                            sb.Append(" (No Segment is currently playing.)");
	                        }
	                        Logger.Instance.Log(sb.ToString(), LogLevel.warnings);
                        }
				    }
				    #endif

				    return PsaiResult.essential_segment_missing;
			    }
		    }
		    else
		    {
			    #if !(PSAI_NOLOG)
			    {
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("the triggered Highlight Theme ");
	                    sb.Append(highlightTheme.id);
	                    sb.Append(" does not contain any Segments.");
	                    Logger.Instance.Log(sb.ToString(), LogLevel.warnings);
                    }
			    }
			    #endif

			    return PsaiResult.essential_segment_missing;
		    }
	    }


        	/** basic internal function for setting the first theme that will be played as soon as the
	    * current theme's intensity has dropped (or has been set) to zero. This method behaves as a push operation, so the parameter will be
	    * the first theme on the theme queue stack.
	    * @param clearThemeQueue pass true to clear the themeQueue, false to enqueue the followingTheme in the themeQueue
	    * @param restTimeMillis if the theme should start in rest mode, pass the resting millis. 0 otherwise.
	    * @param playMode the playMode that will be entered when the theme is popped.
	    * @param holdIntensity true for holding the intensity at a constant level
	    */
	    bool pushThemeToThemeQueue(int themeId, float intensity, int musicDuration, bool clearThemeQueue, int restTimeMillis, PsaiPlayMode playMode, bool holdIntensity)
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("setting the Following Theme to ");
	                sb.Append(themeId);

                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
        	            sb.Append("  intensity= ");
	                    sb.Append(intensity);
	                    sb.Append("  clearThemeQueue=");
	                    sb.Append(clearThemeQueue);
	                    sb.Append("  playmode=");
	                    sb.Append(playMode);
	                    sb.Append("  holdIntensity=");
	                    sb.Append(holdIntensity);
	                    sb.Append("  musicDuration=");
	                    sb.Append(musicDuration);
                    }
	                Logger.Instance.Log(sb.ToString(), LogLevel.info);
                }
		    }
#endif

            if (clearThemeQueue)
		    {
			    m_themeQueue.Clear();
		    }

            Theme theme = m_soundtrack.getThemeById(themeId);
		    if (theme != null)
		    {
			    ThemeQueueEntry newEntry = new ThemeQueueEntry();
			    newEntry.themeId = themeId;
			    newEntry.startIntensity = intensity;
                newEntry.musicDuration = musicDuration;
			    newEntry.restTimeMillis = restTimeMillis;
			    newEntry.playmode = playMode;
			    newEntry.holdIntensity = holdIntensity;

			    #if !(PSAI_NOLOG)
			    {
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append(" m_themeQueue.size()=");
	                    sb.Append(m_themeQueue.Count);
	               	    for (int i=0; i<m_themeQueue.Count; i++)
					    {
						    ThemeQueueEntry tmpEntry = m_themeQueue[i];
	                        sb.Append("   [");
	                        sb.Append(i);
	                        sb.Append("] themeId=");
	                        sb.Append(tmpEntry.themeId);
	                        sb.Append(" startIntensity=");
	                        sb.Append(tmpEntry.startIntensity);
	                    }
	                    Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                    }
			    }
			    #endif

                m_themeQueue.Insert(0, newEntry);
                m_psaiStateIntended = PsaiState.playing;		// in case IntensityZeroHandler() had already been called, we need to reset the psaiStateIntended here
                return true;
		    }
		    else
		    {
			    return false;
		    }
	    }


        ThemeType getThemeTypeOfFirstThemeQueueEntry()
	    {
		    ThemeQueueEntry followingThemeEntry = getFollowingThemeQueueEntry();
		    if (followingThemeEntry != null)
		    {
                Theme followingTheme = m_soundtrack.getThemeById(followingThemeEntry.themeId);
			
			    if (followingTheme != null)
				    return followingTheme.themeType;
		    }

		    return ThemeType.none;
	    }


        internal float getUpcomingIntensity()
        {

            if (m_psaiState == PsaiState.playing)
            {
                if (m_nonInterruptingTriggerOfHighestPriority.themeId != -1)
                {
                    return m_nonInterruptingTriggerOfHighestPriority.startIntensity;
                }
            }

            return getCurrentIntensity();
        }


        internal float getCurrentIntensity()
	    {
		    //boost::recursive_mutex::scoped_lock blockieren(m_pnxLogicMutex);

            if (m_paused)
            {
                return m_lastIntensity;
            }
            else if (m_psaiState == PsaiState.playing && m_psaiStateIntended == PsaiState.playing && !m_returnToLastBasicMoodFlag)
            {
                if (m_holdIntensity)
                {
                    return m_heldIntensity;
                }
                else
                {
                    float resultIntensity = 0.0f;
                    
                    if (m_effectiveTheme == null)
                    {
                        resultIntensity = 0.0f;
                    }
                    else
                    {
                        if (m_targetSegment != null)
                        {                            
                            if (m_currentSegmentPlaying == null || (m_currentSegmentPlaying.ThemeId != m_targetSegment.ThemeId))
                            {
                                return m_targetSegment.Intensity;
                            }                            
                        }

                        int deltaTime = GetTimestampMillisElapsedSinceInitialisation() - m_timeStampOfLastIntensitySetForCurrentTheme;
                        resultIntensity = m_startOrRetriggerIntensityOfCurrentTheme - (deltaTime * m_currentIntensitySlope) / 1000.0f;

                        if (resultIntensity < 0)
                            resultIntensity = 0;                     
                    }
                    m_lastIntensity = resultIntensity;
                    return resultIntensity;
                }
            }

            return 0;
	    }


        /** plays the given snippet either immediately or schedules its playback to the end of current snippet.
	    *
	    */
	    PsaiResult PlaySegment(Segment targetSnippet, bool immediately)
	    {
		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("PlaySegment() name=");
	                sb.Append(targetSnippet.Name);
	                sb.Append("  immediately=");
	                sb.Append(immediately);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
		    }
		    #endif


		    if (m_initializationFailure)
		    {
			    #if !(PSAI_NOLOG)
			    {
				    Logger.Instance.Log("PlaySegment() - abortion due to initialization failure", LogLevel.errors);
			    }
			    #endif

			    return PsaiResult.initialization_error;
		    }

		    PsaiResult	psaiResult;
		
		    // if another target snippet is already set, first check if it's the same, so we can ignore the call
		    /*
            if (m_targetSnippet != null)
		    {
			    if (m_targetSnippet.themeId == targetSnippet.themeId)
			    {
				    #if !(PSAI_NOLOG)
				    {
					    Logger.Instance.Log("ignoring PlaySnippet(), because m_targetSnippet is already set and has the same theme id", LogLevel.debug);
				    }
				    #endif

				    m_targetIntensity = intensityAtStart;
				    m_recalculateIntensitySlopeAtSnippetPlayback = recalculateIntensitySlope;
				    return PsaiResult.OK;		// better return PsaiResult.OK than PSAI_INFO_COMMAND_IGNORED, because it may cause confusion when this return value 
									            // is passed down to triggerMusicTheme().
			    }
			    else
			    {
				    if (!immediately)
				    {
					    // Check if we still have enough time to preload the new targetSnippet										
					    if (m_timeStampCurrentSnippetEndApproaching - GetTimestampMillisElapsedSinceInitialisation() < s_audioLayerMaximumLatencyForBufferingSounds)
					    {
						    // we don't have enough time to preload the snippet
						    // 
						    if (targetSnippet.themeId == m_targetSnippet.themeId && (m_targetSnippetTypeRequested & targetSnippet.SnippetTypeBitfield) > 0)
						    {
							    #if !(PSAI_NOLOG)
							    {
								    Logger.Instance.Log("ignoring PlaySnippet(), because preload time is not sufficient and the m_targetSnippet->snippetType matches the requested snippet type", LogLevel.debug);
							    }
							    #endif

							    m_targetIntensity = intensityAtStart;
							    m_recalculateIntensitySlopeAtSnippetPlayback = recalculateIntensitySlope;
							    return PsaiResult.commandIgnored;
						    }
						    else
						    {
							    #if !(PSAI_NOLOG)
							    {
                                    Logger.Instance.Log("INTERNAL WARNING! we don't have enough time to preload the snippet, and the preloaded snippet has a different themeId and / or a different snippet type. Playback will be delayed", LogLevel.debug);
							    }
							    #endif
						    }
					    }
				    }
			    }
		    }
             */

		    //////////////////////////////////////////////////////////////////////////
		    // at this point it's sure that the requested Segment will be played back, either immediately or after the end of the current Segment
		    // (exception: if stopMusic(immediately) is called)
		    //////////////////////////////////////////////////////////////////////////

 
			m_timerSegmentEndApproaching.Stop();		
		    m_timerStartSnippetPlayback.Stop();

		    m_targetVoice = getNextVoiceNumber(false);

		    // stop channel if necessary

            /* TODO: this still exists in psaiCORE. Commented out because it's screwing
               with updateFades(). why is this necessary ?
            */
            //m_playbackChannels[m_targetVoice].StopChannel();

		    psaiResult =  LoadSegment(targetSnippet, m_targetVoice);
		    PsaiErrorCheck(psaiResult, "LoadSegment()");		

		    // schedule snippet playback
		    int millisUntilNextSnippetPlayback = 0;
		
		    m_targetSegment = targetSnippet;

		    if (immediately || m_currentSegmentPlaying == null )
		    {
                if (m_playbackChannels[m_targetVoice].CheckIfSegmentHadEnoughTimeToLoad())
                {
                    m_estimatedTimestampOfTargetSnippetPlayback = GetTimestampMillisElapsedSinceInitialisation() + s_audioLayerMaximumLatencyForPlayingbackPrebufferedSounds; 
                }
                else
                {
                    m_estimatedTimestampOfTargetSnippetPlayback = GetTimestampMillisElapsedSinceInitialisation() + s_audioLayerMaximumLatencyForPlayingBackUnbufferedSounds; 
                }
                
			    PlayTargetSegmentImmediately();
		    }
		    else
		    {			
			    int millisElapsed = GetMillisElapsedAfterCurrentSnippetPlaycall();
			    millisUntilNextSnippetPlayback = (int)(m_currentSegmentPlaying.audioData.GetFullLengthInMilliseconds() - m_currentSegmentPlaying.audioData.GetPostBeatZoneInMilliseconds() - targetSnippet.audioData.GetPreBeatZoneInMilliseconds() - millisElapsed);

			    if (millisUntilNextSnippetPlayback > s_audioLayerMaximumLatencyForPlayingBackUnbufferedSounds)
			    {
                    m_estimatedTimestampOfTargetSnippetPlayback = Logik.GetTimestampMillisElapsedSinceInitialisation() + millisUntilNextSnippetPlayback;
                    m_timerStartSnippetPlayback.SetTimer(millisUntilNextSnippetPlayback, s_audioLayerMaximumLatencyForPlayingbackPrebufferedSounds);            
			    }
			    else 
			    {
				    #if !(PSAI_NOLOG)
				    {
                        if (LogLevel.debug <= Logger.Instance.LogLevel)
                        {
	                        StringBuilder sb = new StringBuilder();
	                        sb.Append("!!! millisUntilNextSnippetPlayback=");
	                        sb.Append(millisUntilNextSnippetPlayback);
	                        sb.Append("  so we're playing immediately !");
	                        sb.Append("  s_audioLayerMaximumLatencyForPlayingBackUnbufferedSounds=");
	                        sb.Append(s_audioLayerMaximumLatencyForPlayingBackUnbufferedSounds);
	                        sb.Append("  m_currentSegmentPlaying->FullLengthInMs=");
	                        sb.Append(m_currentSegmentPlaying.audioData.GetFullLengthInMilliseconds());
	                        sb.Append("  m_currentSnippetPlaying->PostBeatMs=");
	                        sb.Append(m_currentSegmentPlaying.audioData.GetPostBeatZoneInMilliseconds());
	                        sb.Append("  targetSnippet->PreBeatMs=");
	                        sb.Append(targetSnippet.audioData.GetPreBeatZoneInMilliseconds());   
	                        sb.Append("  m_timeStampCurrentSnippetPlaycall=");
	                        sb.Append(m_timeStampCurrentSnippetPlaycall);
	                        Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                        }
				    }
				    #endif

                    m_estimatedTimestampOfTargetSnippetPlayback = GetTimestampMillisElapsedSinceInitialisation() + s_audioLayerMaximumLatencyForPlayingbackPrebufferedSounds; 
				    PlayTargetSegmentImmediately();						
			    }
		    }
		    return PsaiResult.OK;
	    }


        PsaiResult LoadSegment(Segment snippet, int channelIndex)
        {
            if (snippet == null || channelIndex >= PSAI_CHANNEL_COUNT)
            {
                return PsaiResult.invalidHandle;
            }
            else
            {
                m_playbackChannels[channelIndex].LoadSegment(snippet);
                return PsaiResult.OK;
            }
        }

        
        /* This method will play the target Segment with minimum delay, and set the m_targetSnipet to m_currentSnippet, and m_targetVoice to m_currentVoice.
         */        
	    void PlayTargetSegmentImmediately()
	    {
            #if !(PSAI_NOLOG)
            {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("PlayTargetSegmentImmediately()  m_targetSegmentTypesRequested=");
	                sb.Append(Segment.GetStringFromSegmentSuitabilities(m_targetSegmentSuitabilitiesRequested));
	                sb.Append("  targetSegment=");
	                sb.Append(m_targetSegment.Name);
	                sb.Append("  id=");
	                sb.Append(m_targetSegment.Id);
	                sb.Append("  m_targetVoice=");
	                sb.Append(m_targetVoice);
	                sb.Append("  themeId=");
	                sb.Append(m_targetSegment.ThemeId);
	                sb.Append("  playbackChannel.Segment=" );
	                sb.Append(m_playbackChannels[m_targetVoice].Segment.Name);
	                sb.Append("  millisSinceSegmentLoad=");
	                sb.Append(  m_playbackChannels[m_targetVoice].GetMillisecondsSinceSegmentLoad());
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
            }
            #endif

            int snippetPlaybackDelayMillis = 0;            
            if (m_playbackChannels[m_targetVoice].CheckIfSegmentHadEnoughTimeToLoad())
            {
                snippetPlaybackDelayMillis = m_estimatedTimestampOfTargetSnippetPlayback - GetTimestampMillisElapsedSinceInitialisation();

                #if !(PSAI_NOLOG)
                   if (LogLevel.debug <= Logger.Instance.LogLevel)
                   {
                       Logger.Instance.Log("Segment had enough time to load.   m_estimatedTimestampOfTargetSnippetPlayback=" + m_estimatedTimestampOfTargetSnippetPlayback.ToString(), LogLevel.debug);
                   }
                #endif                
            }
            else
            {
                int millisUntilLoadingWillHaveFinished = m_playbackChannels[m_targetVoice].GetMillisecondsUntilLoadingWillHaveFinished();
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("Segment DID NOT have enough time to load!  missing milliSeconds=" + millisUntilLoadingWillHaveFinished, LogLevel.debug);
                    }
                }
                #endif

                snippetPlaybackDelayMillis = millisUntilLoadingWillHaveFinished + s_audioLayerMaximumLatencyForPlayingbackPrebufferedSounds;
            }

            m_playbackChannels[m_targetVoice].FadeOutVolume = 1.0f;
            m_playbackChannels[m_targetVoice].ScheduleSegmentPlayback(m_targetSegment, snippetPlaybackDelayMillis);            

            if (m_scheduleFadeoutUponSnippetPlayback)
            {
                startFade(m_currentVoiceNumber, PSAI_FADEOUTMILLIS_PLAYIMMEDIATELY, m_targetSegment.audioData.GetPreBeatZoneInMilliseconds() + snippetPlaybackDelayMillis);
                m_scheduleFadeoutUponSnippetPlayback = false;
            }

            m_psaiPlayMode = m_psaiPlayModeIntended;
            m_currentVoiceNumber = m_targetVoice;

            m_currentSegmentPlaying = m_targetSegment;
            m_currentSnippetTypeRequested = m_targetSegmentSuitabilitiesRequested;
            m_currentSegmentPlaying.Playcount++;			//TODO: this should not be increased within menu mode, should it?

            m_timeStampCurrentSnippetPlaycall = GetTimestampMillisElapsedSinceInitialisation() + snippetPlaybackDelayMillis;

            // now set the timers for snippet end approaching and snippet end reached	
            int millisUntilUpcomingSnippetEnd = m_targetSegment.audioData.GetFullLengthInMilliseconds() + snippetPlaybackDelayMillis;            
            int millisUntilNextCalculateCall = (int)(millisUntilUpcomingSnippetEnd 
                - m_targetSegment.audioData.GetPostBeatZoneInMilliseconds() 
                - m_targetSegment.MaxPreBeatMsOfCompatibleSnippetsWithinSameTheme 
                - s_audioLayerMaximumLatencyForPlayingBackUnbufferedSounds      // deterministic calculation up to here...
                -  2 * s_updateIntervalMillis);  // ... now add some extra headroom for processing time


            if (millisUntilNextCalculateCall < 0)
            {                
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append(" psai did not have enough time to evaluate the next Segment in time (missing milliseconds: ");
	                    sb.Append(millisUntilNextCalculateCall);
	                    sb.Append(").");
	                    sb.Append(" This means that either the main region of Segment '");
                        sb.Append(m_currentSegmentPlaying.Name);
                        sb.Append("' (ThemeId=");
                        sb.Append(m_currentSegmentPlaying.ThemeId);
                        sb.Append(") is too short, or that the Prebeat region of at least one of its compatible Segments is too long. ");
                        sb.Append("See the 'best practice' section in the psai manual for more information.");
	                    Logger.Instance.Log(sb.ToString(), LogLevel.warnings);
                    }
                }
#endif

                millisUntilNextCalculateCall = 0;
            }          

            m_targetSegment = null;
            m_psaiState = PsaiState.playing;

            if (millisUntilNextCalculateCall < 0)
            {
                millisUntilNextCalculateCall = 0;
            }
            m_timerSegmentEndApproaching.SetTimer(millisUntilNextCalculateCall, s_updateIntervalMillis);


            // at the moment the SnippetEndReached Timer only fires for the very last snippet
            // so it will be extended with every following snippet
            m_timerSegmentEndReached.SetTimer(millisUntilUpcomingSnippetEnd, 0);

            #if !(PSAI_NOLOG)
            {
                /*
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("m_timer_SegmentEndApproaching should fire in ");
	                sb.Append(millisUntilNextCalculateCall);
	                sb.Append(" ms");
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
	
	                
	                StringBuilder sb2 = new StringBuilder();
	                sb2.Append("m_timer_SegmentEndReached should fire in ");
	                sb2.Append(millisUntilUpcomingSnippetEnd);
	                sb2.Append(" ms");
	                Logger.Instance.Log(sb2.ToString(), LogLevel.debug); 
                }               
                */
            }
            #endif            
	    }


        /*
        float getVolume(int channelIndex)
        {
            return m_playbackChannels[channelIndex].Volume;
        }
        */


        internal float getVolume()
        {
            return m_psaiMasterVolume;
        }

        internal void setVolume(float vol)
	    {		
		    m_psaiMasterVolume = vol;

            if (vol > 1.0)
            {
                m_psaiMasterVolume = 1.0f;
                Logger.Instance.Log("Invalid volume level! please enter values between 0.0 and 1.0. Volume was set to 1.0f.", LogLevel.warnings);
            }
            else
            {
                if (vol < 0.0f)
                {
                    m_psaiMasterVolume = 0.0f;
                    Logger.Instance.Log("Invalid volume level! please enter values between 0.0 and 1.0. Volume was set to 0.0f.", LogLevel.warnings);
                }
            }

            for (int i = 0; i < m_playbackChannels.Length; i++)
            {
                m_playbackChannels[i].MasterVolume = m_psaiMasterVolume;
            }
	    }



        // this wrapper is needed to set the intensity slope correcly, when a theme was popped from the themeQueue
	    PsaiResult PlayThemeNowOrAtEndOfCurrentSegment(ThemeQueueEntry tqe, bool immediately)
	    {
			return PlayThemeNowOrAtEndOfCurrentSegment(tqe.themeId, tqe.startIntensity, tqe.musicDuration, immediately, tqe.holdIntensity);
	    }




        /** internal function to initiate the playback of a theme.
	    * @param themeId     id of the theme to be played 
	    * @param intensity   the desired intensity level [0.0f ... 1.0f]
	    * @param immediately true: play instantly   false: play at end of current snippet
	    * @param SnippetType
	    * @param recalculateIntensitySlope pass true if the intensity slope needs to be reinitialized to the full musical period as defined in the authoring software.
	    * @param holdIntensity pass true if the intensity should be held on a constant level, false otherwise (default is false)
	    * @return PsaiResult.OK ok, theme will be played
	    *		PSAI_INFO_TRIGGER_DENIED trigger was denied as a theme transition is not guaranteed due to missing snippet transitions, and error handling was set to DENY_TRIGGER
	    *		PSAI_ERR_ESSENTIAL_SNIPPET_MISSING a transition could not be achieved because there was no compatible follower to the current snippet, and error handling was set to ABORT or STOP_MUSIC
	    *		
	    */
	    PsaiResult PlayThemeNowOrAtEndOfCurrentSegment(int themeId, float intensity, int musicDuration, bool immediately, bool holdIntensity)
	    {
		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                Theme theme = m_soundtrack.getThemeById(themeId);
	
	                StringBuilder sb = new StringBuilder();
	                sb.Append("PlayThemeNowOrAtEndOfCurrentSegment()  themeId=");
	                sb.Append(themeId);
	
	                if (theme == null)
	                {
	                    sb.Append("THEME NOT FOUND!");
	                }
	                else
	                {
	                    sb.Append(" [");
	                    sb.Append(theme.Name);
	                    sb.Append("]  themeType=");
	                    sb.Append(theme.themeType);
	                }
	
	                sb.Append( " intensity=");
	                sb.Append(intensity);
	                sb.Append(" immediately=");
	                sb.Append(immediately);
	                sb.Append(" holdIntensity=");
	                sb.Append(holdIntensity);
	                sb.Append(" musicDuration=");
	                sb.Append(musicDuration);
	                sb.Append(" m_currentSegmentPlaying=");
	                sb.Append(m_currentSegmentPlaying);
	
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
		    }
		    #endif

            SetCurrentIntensityAndMusicDuration(intensity, musicDuration, true);

            m_psaiStateIntended = PsaiState.playing;
            m_heldIntensity = intensity;

		    // if we're interrupting rest mode, kill the rest wake up timer		    
            if (m_psaiState == PsaiState.rest)
		    {
                m_timerWakeUpFromRest.Stop();
		    }


            

            // choice of Segment Suitability:
            // * Use Start Segments:
            //      1. when playing out of silence
            //      2. if a pure END-Segment is currently playing
            // * Use Middle Segments when staying within the current Theme.
            // * Use Bridge or Middle Segments when transitioning to another Theme.            
            m_targetSegmentSuitabilitiesRequested = (int)SegmentSuitability.start;

            if (m_psaiState == PsaiState.playing)
            {               
                if (m_currentSegmentPlaying != null)
                {
                    if (m_currentSegmentPlaying.IsUsableOnlyAs(SegmentSuitability.end))
                    {
                        m_targetSegmentSuitabilitiesRequested = (int)SegmentSuitability.start;
                    }
                    else
                    {
                        if (getEffectiveThemeId() == themeId)
                        {
                            // we stay within the same Theme, so choose a MIDDLE Segment
                            // NOTE: this may only happen if we return from the Menu or a CutScene to the same Theme. Otherwise,
                            // PlayThemeNowOrAtEndOfCurrentSegment should not be called explicitly
                            m_targetSegmentSuitabilitiesRequested = (int)SegmentSuitability.middle;
                        }
                        else
                        {
                            // upon Theme changes we play BRIDGE or MIDDLE Segments
                            m_targetSegmentSuitabilitiesRequested = SNIPPET_TYPE_MIDDLE_OR_BRIDGE;
                        }
                    }
                }
            }

            m_effectiveTheme = m_soundtrack.getThemeById(themeId);

            Segment targetSnippet;
            if ((m_targetSegmentSuitabilitiesRequested & (int)SegmentSuitability.start) > 0 || GetEffectiveSegment() == null)
            {
                targetSnippet = GetBestStartSegmentForTheme(themeId, intensity);
            }
            else
            {
                targetSnippet = GetBestCompatibleSegment(GetEffectiveSegment(), themeId, intensity, m_targetSegmentSuitabilitiesRequested);
            }


		    //////////////////////////////////////////////////////////////////////////
		    // no compatible Segment could be found !
		    //////////////////////////////////////////////////////////////////////////
		    if (targetSnippet == null)
		    {
				#if !(PSAI_NOLOG)
				{
                    Logger.Instance.Log("essential Segment could not be found! Trying to substitute...", LogLevel.errors);
				}
				#endif

				targetSnippet = substituteSegment(themeId);

				if (targetSnippet == null)
				{
					#if !(PSAI_NOLOG)
					{
                        Logger.Instance.Log("failed to substitute Segment. Stopping music.", LogLevel.errors);
					}
					#endif

					StopMusic(true);
					return PsaiResult.essential_segment_missing;
				}
		    }

		    // ////////////////////////////////////////////////
		    // at this point, the target Segment has to be set! (internal error otherwise)
		    // ////////////////////////////////////////////////

		    m_holdIntensity = holdIntensity;
            
            // starting a Theme immediately when any other Theme is already playing should result in
            // fading out that other Theme.
            if (immediately && GetEffectiveSegment() != null)
            {
                m_scheduleFadeoutUponSnippetPlayback = true;
            }

		    if (targetSnippet != null)
		    {
                return PlaySegment(targetSnippet, immediately);	
		    }
		    else
		    {
			    #if !(PSAI_NOLOG)
			    {
                    Logger.Instance.Log("fatal internal error! entered code section in PlayTheme that is supposed to be unreachable!", LogLevel.errors);
			    }
			    #endif

			    return PsaiResult.internal_error;
		    }
	    }





        internal PsaiResult StopMusic(bool immediately)
	    {
		    //boost::recursive_mutex::scoped_lock blockieren(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
            {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("StopMusic(");
	                sb.Append(immediately);
	                sb.Append(") called");
	                Logger.Instance.Log(sb.ToString(), LogLevel.info);
                }
            }
		    #endif


            if (m_paused)
            {
                setPaused(false);
            }


		    // return immediately in menu mode
		    if (m_psaiPlayMode == PsaiPlayMode.menuMode)
		    {
			    #if !(PSAI_NOLOG)
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("StopMusic() ignored: Menu Mode is active", LogLevel.warnings);
                    }
			    #endif

			    return PsaiResult.commandIgnoredMenuModeActive;
		    }

		    // return immediately in cutscene mode
		    if (m_psaiPlayModeIntended == PsaiPlayMode.cutScene)
		    {
			    #if !(PSAI_NOLOG)
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("StopMusic() ignored: Cutscene is active", LogLevel.warnings);
                    }
			    #endif

			    return PsaiResult.commandIgnoredCutsceneActive;
		    }

		    if (m_initializationFailure)
		    {
			    #if !(PSAI_NOLOG)
			    {
                    Logger.Instance.Log(LOGMESSAGE_ABORTION_DUE_TO_INITIALIZATION_FAILURE, LogLevel.errors);
			    }
			    #endif

			    return PsaiResult.initialization_error;
		    }

       		if (m_psaiStateIntended == PsaiState.silence && !immediately)
		    {
			    #if !(PSAI_NOLOG)
			    {
				    if (LogLevel.warnings <= Logger.Instance.LogLevel)
				    {
				    	Logger.Instance.Log("StopMusic() ignored - psai is currently already transitioning to SILENCE mode", LogLevel.warnings);
				    }
			    }
			    #endif

			    return PsaiResult.commandIgnored;
		    }

            m_returnToLastBasicMoodFlag = false;
            m_holdIntensity = false;

		    //////////////////////////////////////////////////////////////////////////

		    switch (m_psaiState)
		    {
			    case PsaiState.playing:
			    case PsaiState.silence:		// SILENCE will be set if we are leaving the menu and were in SILENCE when we entered it
			    {
				    Segment effectiveSnippet = GetEffectiveSegment();

				    if (effectiveSnippet != null)
				    {				
					    bool validPathToEndSegmentExists = false;					
					    if (!immediately)
					    {
						    validPathToEndSegmentExists = CheckIfThereIsAPathToEndSegmentForEffectiveSegmentAndLogWarningIfThereIsnt();
					    }

					    if (immediately || !validPathToEndSegmentExists)
					    {
						    startFade(m_currentVoiceNumber, PSAI_FADEOUTMILLIS_STOPMUSIC, 0);
						    EnterSilenceMode();
					    }
					    else
					    {	
						    #if !(PSAI_NOLOG)
						    {
							    WriteLogWarningIfThereIsNoDirectPathForEffectiveSnippetToEndSnippet();                             
						    }
						    #endif

                            if (m_nonInterruptingTriggerOfHighestPriority.themeId != -1)
                            {
                                m_nonInterruptingTriggerOfHighestPriority.themeId = -1;

                                #if !(PSAI_NOLOG)
                                {
                                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                                    {
                                    	Logger.Instance.Log("cleared noninterrupting trigger", LogLevel.debug);
                                    }
                                }
                                #endif
                            }

						    m_psaiStateIntended = PsaiState.silence;
					    }
				    }
			    }
			    break;

			    case PsaiState.rest:
			    {
                    EnterSilenceMode();
			    }
			    break;
		    }

		    return PsaiResult.OK;
	    }



        void WriteLogWarningIfThereIsNoDirectPathForEffectiveSnippetToEndSnippet()
	    {
		    #if !(PSAI_NOLOG)

		    if (LogLevel.warnings <= Logger.Instance.LogLevel)
		    {
			    Segment effectiveSnippet = GetEffectiveSegment();
	
			    if (effectiveSnippet == null)
			    {
				    Logger.Instance.Log("INTERNAL WARNING: WriteLogWarningIfThereIsNoDirectPathForEffectiveSnippetToEndSnippet() effectiveSnippet is NULL.", LogLevel.debug);			
			    }
			    else if (effectiveSnippet.nextSnippetToShortestEndSequence == null)
			    {
	                Logger.Instance.Log("INTERNAL WARNING: WriteLogWarningIfThereIsNoDirectPathForEffectiveSnippetToEndSnippet() effectiveSnippet->nextSnippetToShortestEndSequence is NULL.", LogLevel.debug);			 
			    }
			    else if ((effectiveSnippet.nextSnippetToShortestEndSequence.SnippetTypeBitfield  & (int)SegmentSuitability.end) == 0)
			    {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("There is no direct path to an END-Snippet from the current Snippet, thus psai will play an indirection via Snippet: ");
	                sb.Append(effectiveSnippet.nextSnippetToShortestEndSequence.Name);
	                sb.Append(" Please add a direct compatible Transition to an END-Snippet for Snippet ");
	                sb.Append(effectiveSnippet.Name);
	                sb.Append(" to have psai stop the music more quickly.");
	                Logger.Instance.Log(sb.ToString(), LogLevel.warnings);
			    }
		    }
		    #endif
	    }



        bool CheckIfThereIsAPathToEndSegmentForEffectiveSegmentAndLogWarningIfThereIsnt()
	    {
		    Segment effectiveSegment = GetEffectiveSegment();
		    if (!effectiveSegment.IsUsableAs(SegmentSuitability.end) && effectiveSegment.nextSnippetToShortestEndSequence == null)
		    {
			    #if !(PSAI_NOLOG)
			    {
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("There is no direct or indirect path to an END-Segment from the Segment currently playing, thus psai has to fade out immediately. Please add a compatible transition to an END-Segment for Segment: ");
	                    sb.Append(effectiveSegment.Name);
	                    Logger.Instance.Log(sb.ToString(), LogLevel.warnings);
                    }
			    }
			    #endif

			    return false;
		    }		

		    return true;
	    }



        void updateFades()
	    {
		    //boost::recursive_mutex::scoped_lock blockieren(m_pnxLogicMutex);

		    //if (m_audioPlaybackLayer)
		    {
			    bool isTimerNecessary = false;

			    #if !(PSAI_NOLOG)
			    {
                    /*
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("updateFades() fadeVoices.Count=");
	                    sb.Append(m_fadeVoices.Count);
	                    Logger.Instance.Log(sb.ToString(), LogLevel.debug);				    
                    }
                     */
			    }
			    #endif


                int millisSinceLastUpdate = GetTimestampMillisElapsedSinceInitialisation() - m_timeStampOfLastFadeUpdate;

                m_timeStampOfLastFadeUpdate = GetTimestampMillisElapsedSinceInitialisation();

                //foreach (FadeData fadeData in m_fadeVoices)
                int fadeDataIndex = 0;
                while ( fadeDataIndex < m_fadeVoices.Count)
			    {
                    FadeData fadeData = m_fadeVoices[fadeDataIndex];

                    #if !(PSAI_NOLOG)
                    {
                        /*
                        if (LogLevel.debug <= Logger.Instance.LogLevel)
                        {
	                        StringBuilder sb = new StringBuilder();
	                        sb.Append("checking fadeData for voice ");
	                        sb.Append(fadeData.voiceNumber);
	                        sb.Append("  channelState=");
	                        sb.Append(m_playbackChannels[fadeData.voiceNumber].GetCurrentChannelState());
	                        Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                        }
                         */
                    }
                    #endif

				    // do we have a time offset ?
				    if (fadeData.delayMillis > 0 )
				    {
					    fadeData.delayMillis -= millisSinceLastUpdate;		
					    #if !(PSAI_NOLOG)
					    {
                            /*
                            if (LogLevel.debug <= Logger.Instance.LogLevel)
                            {
	                            StringBuilder sb = new StringBuilder();
	                            sb.Append("fadeout pending for voice ");
	                            sb.Append(fadeData.voiceNumber);
	                            sb.Append(", remaining delay ms=");
	                            sb.Append(fadeData.delayMillis);
	                            Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                            }
                             */
					    }
					    #endif

					    if (fadeData.delayMillis <= 0)
					    {
						    fadeData.delayMillis = 0;

						    #if !(PSAI_NOLOG)
						    {
                                /*
                                 if (LogLevel.debug <= Logger.Instance.LogLevel)
                                 {
                                 	Logger.Instance.Log("starting fadeout of voice " + fadeData.voiceNumber, LogLevel.debug);
                                 }
                                 */
						    }
						    #endif
					    }

					    isTimerNecessary = true;

                        fadeDataIndex++;
				    }
				    else		// if the time offset is zero, fade out this voice
				    {
					    float vol = fadeData.currentVolume - fadeData.fadeoutDeltaVolumePerUpdate;

					    if (vol > 0.0f)
					    {
						    isTimerNecessary = true;
						    fadeData.currentVolume = vol;	// write back the volume

                            m_playbackChannels[fadeData.voiceNumber].FadeOutVolume = vol;

                            
                            #if !(PSAI_NOLOG)
                            {
                                /*                                
                                if (LogLevel.debug <= Logger.Instance.LogLevel)
                                {
	                                StringBuilder sb = new StringBuilder();
	                                sb.Append("volume of voice ");
	                                sb.Append(fadeData.voiceNumber);
	                                sb.Append(" set to ");
	                                sb.Append(vol);
	                                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                                }
                                 */
                            }
                            #endif

						    fadeDataIndex++;
					    }
					    else
					    {
						    // voice can be switched off
						    int voiceNumber = fadeData.voiceNumber;
						    
                            if (m_playbackChannels[voiceNumber].IsPlaying())
						    {
                                m_playbackChannels[voiceNumber].StopChannel();
                                m_playbackChannels[voiceNumber].ReleaseSegment();
                                m_fadeVoices.RemoveAt(fadeDataIndex);
						    }
						    else
						    {
							    // channel is not playing, hence take it away
							    #if !(PSAI_NOLOG)
							    {
                                    /*
                                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                                    {
	                                    StringBuilder sb = new StringBuilder();
	                                    sb.Append("channelState=");
	                                    sb.Append(m_playbackChannels[voiceNumber].GetCurrentChannelState());
	                                    sb.Append("  ");
	                                    sb.Append("erasing voice ");
	                                    sb.Append(voiceNumber);
	                                    Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                                    }
                                    */
                                }
							    #endif

                                m_fadeVoices.RemoveAt(fadeDataIndex);
						    }
					    }
				    }

			    }

			    if (isTimerNecessary) 
			    {
                    m_timerFades.SetTimer(PSAI_FADING_UPDATE_INVERVAL_MILLIS, 0);
			    }
		    }
	    }


        internal PsaiResult HoldCurrentIntensity(bool hold)
	    {
		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("HoldCurrentIntensity() hold=");
	                sb.Append(hold);
	                Logger.Instance.Log(sb.ToString(), LogLevel.info);
                }
		    }
		    #endif

		    switch (m_psaiPlayModeIntended)
		    {
			    case PsaiPlayMode.cutScene:
			    {                    
				    return PsaiResult.commandIgnoredCutsceneActive;
			    }
			    //break;

			    case PsaiPlayMode.menuMode:
			    {
				    return PsaiResult.commandIgnoredMenuModeActive;
			    }
			    //break;

			    case PsaiPlayMode.regular:
			    {
				    if (hold && m_holdIntensity)
				    {
					    #if !(PSAI_NOLOG)
					    {
                            if (LogLevel.warnings <= Logger.Instance.LogLevel)
                            {
                            	Logger.Instance.Log("HoldCurrentIntensity(true) - ignored because the intensity is already being held", LogLevel.warnings);
                            }
					    }
					    #endif
					    return PsaiResult.commandIgnored;
				    }
				    else if (!hold && !m_holdIntensity)
				    {
					    #if !(PSAI_NOLOG)
					    {
                            if (LogLevel.warnings <= Logger.Instance.LogLevel)
                            {
                            	Logger.Instance.Log("HoldCurrentIntensity(false) - ignored because the intensity is already decreasing", LogLevel.warnings);
                            }
					    }
					    #endif
					    return PsaiResult.commandIgnored;
				    }
				    else
				    {
					    if (hold)
					    {
						    #if !(PSAI_NOLOG)
						    {
                                if (LogLevel.info <= Logger.Instance.LogLevel)
                                {
                                	Logger.Instance.Log("intensity for the current Theme is being held on a constant level", LogLevel.info);
                                }
						    }
						    #endif

                            m_remainingMusicDurationAtTimeOfHoldIntensity = GetRemainingMusicDurationSecondsOfCurrentTheme();


             		        #if !(PSAI_NOLOG)
						    {
                                if (LogLevel.debug <= Logger.Instance.LogLevel)
                                {
	                                StringBuilder sb = new StringBuilder();
	                                sb.Append("m_remainingMusicDurationAtTimeOfHoldIntensity=");
	                                sb.Append(m_remainingMusicDurationAtTimeOfHoldIntensity);
	                                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                                }
                            }
                            #endif

						    m_heldIntensity = getCurrentIntensity();						
						    m_holdIntensity = true;
					    }
					    else
					    {
						    #if !(PSAI_NOLOG)
						    {
                                if (LogLevel.info <= Logger.Instance.LogLevel)
                                {
	                                StringBuilder sb = new StringBuilder();
	                                sb.Append("automatic descrease of intensity is reactivated, remaining music duration=");
	                                sb.Append(m_remainingMusicDurationAtTimeOfHoldIntensity);
	                                Logger.Instance.Log(sb.ToString(), LogLevel.info);
                                }							    
						    }
						    #endif

                            SetCurrentIntensityAndMusicDuration(m_heldIntensity, m_remainingMusicDurationAtTimeOfHoldIntensity, false);
						    m_holdIntensity = false;
					    }
					    return PsaiResult.OK;
				    }
			    }
			    //break;

			    default:
				    return PsaiResult.internal_error;		// should never be reached
		    }
	    }


	    void SetCurrentIntensityAndMusicDuration(float intensity, int musicDuration, bool recalculateIntensitySlope)
	    {		
		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("SetCurrentIntensityAndMusicDuration()  intensity=");
	                sb.Append(intensity);
	                sb.Append("  musicDuration=");
	                sb.Append(musicDuration);
	                sb.Append("  recalculateIntensitySlope=");
	                sb.Append(recalculateIntensitySlope);
	
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
		    }
            #endif

            m_timeStampOfLastIntensitySetForCurrentTheme = GetTimestampMillisElapsedSinceInitialisation();
            m_lastMusicDuration = musicDuration;

            if (recalculateIntensitySlope)
            {
                m_currentIntensitySlope = intensity / musicDuration;
            }

            m_startOrRetriggerIntensityOfCurrentTheme = intensity;            
	    }


        void SegmentEndApproachingHandler()
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
				    sb.Append("--- SegmentEndApproachingHandler()  m_psaiStateIntended=");
	                sb.Append(m_psaiStateIntended);
	                sb.Append("  m_nonInterruptingTriggerOfHighestPriority.themeId=");
	                sb.Append(m_nonInterruptingTriggerOfHighestPriority.themeId);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
		    }
		    #endif


            // make sure we don't go to rest/sleep in case a non-interrupting trigger call
            // has been received 
            if (m_nonInterruptingTriggerOfHighestPriority.themeId != -1)
            {
                m_psaiState = PsaiState.playing;
                m_psaiStateIntended = PsaiState.playing;
            }

		    switch (m_psaiStateIntended)
		    {
			    case PsaiState.silence:
				    {
                        if (m_currentSegmentPlaying == null || m_currentSegmentPlaying.IsUsableAs(SegmentSuitability.end))
					    {
						    EnterSilenceMode();
					    }
					    else
					    {		
						    PlaySegment(m_currentSegmentPlaying.nextSnippetToShortestEndSequence, false);
					    }
				    }
				    break;

			    case PsaiState.rest:
				    {
                        if (m_currentSegmentPlaying == null || m_currentSegmentPlaying.IsUsableAs(SegmentSuitability.end))
                        {
                            if (m_psaiState != PsaiState.rest)  // m_psaiState will be PsaiState.rest if we already called GoToRest using a fadeout.
                            {
                                EnterRestMode(GetLastBasicMoodId(), getEffectiveThemeId());
                            }                            
                        }
                        else
                        {
                            PlaySegment(m_currentSegmentPlaying.nextSnippetToShortestEndSequence, false);
                        }
				    }
				    break;

			    default:
				    {
                        
					    if (m_returnToLastBasicMoodFlag)
					    {
						    #if !(PSAI_NOLOG)
						    {
                                if (LogLevel.debug <= Logger.Instance.LogLevel)
                                {
                                	Logger.Instance.Log("returnToLastBasicMoodFlag was set", LogLevel.debug);
                                }
						    }
						    #endif
					
						    // no End-Segment is playing and there is a valid transition to End - go on with the ending sequence.
						    if ( ((m_currentSegmentPlaying.SnippetTypeBitfield & (int)SegmentSuitability.end) == 0) &&
							     CheckIfThereIsAPathToEndSegmentForEffectiveSegmentAndLogWarningIfThereIsnt() == true
							    )
						    {
							    WriteLogWarningIfThereIsNoDirectPathForEffectiveSnippetToEndSnippet();

							    PlaySegment(m_currentSegmentPlaying.nextSnippetToShortestEndSequence, false);
							    return;
						    }
						    else
						    {
							    PlayThemeNowOrAtEndOfCurrentSegment(GetLastBasicMoodId(), m_lastBasicMood.intensityAfterRest, m_lastBasicMood.musicDurationGeneral, false, false);
							    m_returnToLastBasicMoodFlag = false;
							    return;
						    }
					    }


					    
                        // do we have received any trigger-Calls while playing? Then use the one with the 
                        // highest priority

                        if (m_psaiPlayMode == PsaiPlayMode.regular && m_nonInterruptingTriggerOfHighestPriority.themeId != -1)
                        {

                            Theme nonInterruptingTheme = m_soundtrack.getThemeById(m_nonInterruptingTriggerOfHighestPriority.themeId);

                            if (m_currentSegmentPlaying.CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(m_soundtrack, nonInterruptingTheme.id))
                            {
                                PlayThemeNowOrAtEndOfCurrentSegment(m_nonInterruptingTriggerOfHighestPriority.themeId, m_nonInterruptingTriggerOfHighestPriority.startIntensity, m_nonInterruptingTriggerOfHighestPriority.musicDuration, false, m_nonInterruptingTriggerOfHighestPriority.holdIntensity);
                                m_nonInterruptingTriggerOfHighestPriority.themeId = -1;
                            }
                            else
                            {
                                if (m_currentSegmentPlaying.MapOfNextTransitionSegmentToTheme.ContainsKey(nonInterruptingTheme.id))
                                {
                                    #if !(PSAI_NOLOG)
                                    {
                                        if (LogLevel.info <= Logger.Instance.LogLevel)
                                        {
	                                        StringBuilder sb = new StringBuilder();
	                                        sb.Append("No direct transition exists from Segment ");
	                                        sb.Append(m_currentSegmentPlaying.Name);
	                                        sb.Append(" to any MIDDLE or BRIDGE Segment of Theme ");
	                                        sb.Append(nonInterruptingTheme.Name);
	                                        sb.Append(", psai is therefore playing an indirect transition via the shortest path of compatible Segments. The next one will be ");
	                                        sb.Append(m_currentSegmentPlaying.MapOfNextTransitionSegmentToTheme[nonInterruptingTheme.id]);
	                                        Logger.Instance.Log(sb.ToString(), LogLevel.info);
                                        }
                                    }
                                    #endif

                                    PlaySegment(m_currentSegmentPlaying.MapOfNextTransitionSegmentToTheme[nonInterruptingTheme.id], false);
                                }
                                else
                                {
                                    #if !(PSAI_NOLOG)
                                    {
                                        if (LogLevel.warnings <= Logger.Instance.LogLevel)
                                        {
	                                        StringBuilder sb = new StringBuilder();
	                                        sb.Append("Could not perform any transition from Segment ");
	                                        sb.Append(m_currentSegmentPlaying.Name);
	                                        sb.Append(" to Theme ");
	                                        sb.Append(nonInterruptingTheme.Name);
	                                        sb.Append(" as no direct or indirect path of compatible Segments exists. Psai is therefore switching Themes by crossfading.");
	                                        Logger.Instance.Log(sb.ToString(), LogLevel.warnings);
                                        }
                                    }
                                    #endif

                                    PlayThemeNowOrAtEndOfCurrentSegment(m_nonInterruptingTriggerOfHighestPriority.themeId, m_nonInterruptingTriggerOfHighestPriority.startIntensity, m_nonInterruptingTriggerOfHighestPriority.musicDuration, true, m_nonInterruptingTriggerOfHighestPriority.holdIntensity);
                                    m_nonInterruptingTriggerOfHighestPriority.themeId = -1;
                                }
                            }
                        }
                        else
                        {
                            // business as usual: get the next snippet of the current theme
                            float currentIntensity = getCurrentIntensity();
                            if (currentIntensity > 0.0f)
                            {
                                m_nonInterruptingTriggerOfHighestPriority.themeId = -1;
                                PlaySegmentOfCurrentTheme(SegmentSuitability.middle);
                            }
                            else
                            {
                                IntensityZeroHandler();
                            }
                        }					    
				    }
				    break;
		    }
	    }


        PsaiResult HandleNonInterruptingTriggerCall(Theme argTheme, float intensity, int musicDuration)
        {
            #if !(PSAI_NOLOG)
            {            
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("HandleNonInterruptingTriggerCall() argTheme=");
	                sb.Append(argTheme.Name);
	                sb.Append("  intensity=");
	                sb.Append(intensity);
	                sb.Append("  musicDuration=");
	                sb.Append(musicDuration);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
            }
            #endif


            bool updateTqe = false;
            if (m_nonInterruptingTriggerOfHighestPriority.themeId != -1)
            {                
                m_nonInterruptingTriggerOfHighestPriority.themeId = -1;
                updateTqe = true;

                /*
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("previouslyTriggeredTheme was null");
	                    Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                    }
                }
                #endif
                */
            }
            else
            {
                // check if the trigger call differs from the last one
                if ( m_nonInterruptingTriggerOfHighestPriority.themeId != argTheme.id         ||
                    m_nonInterruptingTriggerOfHighestPriority.startIntensity != intensity
                    )
                {
                        #if !(PSAI_NOLOG)
                        {
                            /*
                            if (LogLevel.debug <= Logger.Instance.LogLevel)
                            {
	                            StringBuilder sb = new StringBuilder();
	                            sb.Append("differing trigger -> update");
	                            Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                            }
                             */
                        }
                        #endif

                    updateTqe = true;
                }
            }

            if (updateTqe)
            {
                m_nonInterruptingTriggerOfHighestPriority.themeId = argTheme.id;
                m_nonInterruptingTriggerOfHighestPriority.startIntensity = intensity;
                m_nonInterruptingTriggerOfHighestPriority.musicDuration = musicDuration;
                m_psaiStateIntended = PsaiState.playing;
                return PsaiResult.OK;
            }
            else
            {
                #if !(PSAI_NOLOG)
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("... no update necessary", LogLevel.debug);
                    }
                #endif

                return PsaiResult.OK;    
            }
        }


        void SegmentEndReachedHandler()
	    {		   
		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("SegmentEndReachedHandler() m_psaiStateIntended=");
	                sb.Append(m_psaiStateIntended);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }			    
		    }
		    #endif

		    if (m_targetSegment == null)
		    {
                m_currentSegmentPlaying = null;
		    }
	    }



        void InitiateTransitionToRestMode()
	    {

            #if !(PSAI_NOLOG)
            {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
                    Logger.Instance.Log("InitiateTransitionToRestMode()", LogLevel.debug);
                }
            }
            #endif

		    if (m_currentSegmentPlaying != null)
		    {
			    if ( m_currentSegmentPlaying.IsUsableAs(SegmentSuitability.end) )
			    {
                    EnterRestMode(GetLastBasicMoodId(), getEffectiveThemeId());
			    }
			    else if (CheckIfThereIsAPathToEndSegmentForEffectiveSegmentAndLogWarningIfThereIsnt() == false)
			    {
				    startFade(m_currentVoiceNumber, GetRemainingMillisecondsOfCurrentSegmentPlayback(), 0);
				    EnterRestMode(GetLastBasicMoodId(), getEffectiveThemeId());
			    }
			    else
			    {
				    WriteLogWarningIfThereIsNoDirectPathForEffectiveSnippetToEndSnippet();
				    PlaySegment(m_currentSegmentPlaying.nextSnippetToShortestEndSequence, false);
				    m_psaiStateIntended = PsaiState.rest;
			    }
		    }
	    }



        /* will be called whenever the intensity of the current theme has dropped down (or has been explicitly set) to zero.
	    */
	    void IntensityZeroHandler()
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);		

		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
                	Logger.Instance.Log("IntensityZeroHandler()", LogLevel.debug);
                }
		    }
		    #endif

            if (m_currentSegmentPlaying != null)
		    {
                Theme currentTheme = m_soundtrack.getThemeById(m_currentSegmentPlaying.ThemeId);

			    switch(currentTheme.themeType)
			    {
			    case ThemeType.basicMood:
				    {
                        InitiateTransitionToRestMode();		
				    }
				    break;

			    case ThemeType.basicMoodAlt:
			    case ThemeType.dramaticEvent:
			    case ThemeType.action:
			    case ThemeType.shock:
				    {				
					    ThemeQueueEntry themeQueueEntry = getFollowingThemeQueueEntry();
					    if (themeQueueEntry != null)
					    {

                            #if !(PSAI_NOLOG)
                            {
                                if (LogLevel.debug <= Logger.Instance.LogLevel)
                                {
	                                string themeName = "NOT FOUND";
	
	                                Theme followingTheme = m_soundtrack.getThemeById(themeQueueEntry.themeId);
	                                if (followingTheme != null)
	                                {
	                                    themeName = followingTheme.Name;
	                                }
	
	                                StringBuilder sb = new StringBuilder();
	                                sb.Append("found following Theme Queue Entry: themeId=");
	                                sb.Append(themeQueueEntry.themeId);
	                                sb.Append(" [");
	                                sb.Append(themeName);
	                                sb.Append("] ");
	                                sb.Append(" musicDuration=");
	                                sb.Append(themeQueueEntry.musicDuration);
	                                sb.Append(" startIntensity=");
	                                sb.Append(themeQueueEntry.startIntensity);
	                                sb.Append(" playMode=");
	                                sb.Append(themeQueueEntry.playmode);
	                                sb.Append(" restTimeMillis=");
	                                sb.Append(themeQueueEntry.restTimeMillis);
	                                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                                }
                            }
                            #endif


                            if (m_currentSegmentPlaying.CheckIfAnyDirectOrIndirectTransitionIsPossible(m_soundtrack, themeQueueEntry.themeId) == true)
						    {
							    PopAndPlayNextFollowingTheme(false);
						    }
						    else
						    {
							    #if !(PSAI_NOLOG)
							    {
                                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                                    {
	                                    StringBuilder sb = new StringBuilder();
	                                    sb.Append("no transition is possible from current Segment (");
	                                    sb.Append(m_currentSegmentPlaying.Id);
	                                    sb.Append(") to following theme (themeId=");
	                                    sb.Append(themeQueueEntry.themeId);
	                                    sb.Append(") ");
	                                    sb.Append(" -> Play an End-Segment and go to rest.");
	                                    Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                                    }
							    }
							    #endif

                                InitiateTransitionToRestMode();
						    }
					    }
					    else
					    {
						    #if !(PSAI_NOLOG)
						    {
                                if (LogLevel.debug <= Logger.Instance.LogLevel)
                                {
                                    Logger.Instance.Log("No Following Theme is set. Play an End-Segment and go to rest.", LogLevel.debug);
                                }							    
						    }
						    #endif

                            InitiateTransitionToRestMode();
					    }
				    }
				    break;
			    }
		    }
	    }


	    // returns true if a Theme can always transition into another theme, according to the following rules:
	    // 1. if the interruption-Behavior of (source, destination) is IMMEDIATLEY, NEVER or LAYER, return true;
	    // 2. if the interruption-behavior is AT_END_OF_SNIPPET: Every Snippet (except End-Snippets, which are ignored) has 
        //    to have a valid path to the targetTheme. In this case, return true;        
	    /*
        bool CheckIfThemeTransitionIsAlwaysPossible(Theme sourceTheme, Theme targetTheme)
	    {
		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("CheckIfThemeTransitionIsPossible() sourceTheme=");
	                sb.Append(sourceTheme.id);
	                sb.Append(" destinationThemeId=");
	                sb.Append(targetTheme.id);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
		    }
		    #endif

		    if (m_themeCompatibilityMatrix[sourceTheme.id] != null)
		    {
			    // quick lookup
                Dictionary<int, bool> innerMap =  m_themeCompatibilityMatrix[sourceTheme.id];
			    return innerMap[targetTheme.id];
		    }
		    else
		    {
			    ThemeInterruptionBehavior interruptionBehavior = Theme.GetThemeInterruptionBehavior(sourceTheme.themeType, targetTheme.themeType);

			    switch (interruptionBehavior)
			    {
                    case ThemeInterruptionBehavior.layer:
			        case ThemeInterruptionBehavior.at_end_of_current_snippet:
				    {
					    bool atLeastOneValidPathExistsInWholeTheme = false;

                        foreach(Snippet snippet in sourceTheme.m_snippets)
                        {
                            if ((snippet.SnippetTypeBitfield & (int)SnippetType.end) > 0)
                            {
                                continue;
                            }
						    else
						    {
							    if (snippet.CheckIfAtLeastOneDirectTransitionIsPossible(targetTheme.id) == false && snippet.MapOfNextTransitionSegmentToTheme.ContainsKey(targetTheme.id) == false)
							    {			

								    #if !(PSAI_NOLOG)
								    {
                                        if (LogLevel.info <= Logger.Instance.LogLevel)
                                        {
	                                        StringBuilder sb = new StringBuilder();
	                                        sb.Append("No compatible transition authored from snippet ");
	                                        sb.Append(snippet.id);
	                                        sb.Append("  '");
	                                        sb.Append(snippet.audioData.filename);
	                                        sb.Append("' (theme ");
	                                        sb.Append(snippet.themeId);
	                                        sb.Append(")  to theme ");
	                                        sb.Append(targetTheme.id);
	                                        Logger.Instance.Log(sb.ToString(), LogLevel.info);
                                        }									
								    }
								    #endif

                                    m_themeCompatibilityMatrix[sourceTheme.id][targetTheme.id] = false;
								    return false;
							    }
							    else
							    {
								    atLeastOneValidPathExistsInWholeTheme = true;
							    }
						    }
					    }

                        m_themeCompatibilityMatrix[sourceTheme.id][targetTheme.id] = atLeastOneValidPathExistsInWholeTheme;
					    return atLeastOneValidPathExistsInWholeTheme;
				    }

			    default:
                    m_themeCompatibilityMatrix[sourceTheme.id][targetTheme.id] = true;
				    return true;
			    }			
		    }
	    }
        */
        


        /*
	    // returns true if the theme with the given themeId contains at least one snippet,
	    // that would be a compatible follower / layer of the snippet with the given snippetId.
	    bool CheckIfAtLeastOneTransitionIsPossible(Snippet snippet, int themeId)
	    {
            foreach(Follower follower in snippet.followers)
            {
			    Snippet followerSnippet = GetSnippetById(follower.snippetId);
			    if (followerSnippet.themeId == themeId)
				    return true;
            }

		    return false;
	    }
        */

       
        public PsaiResult GoToRest(bool immediately, int fadeOutMilliSeconds)
        {
            return GoToRest(immediately, fadeOutMilliSeconds, -1, -1);

        }

        public PsaiResult GoToRest(bool immediately, int fadeOutMilliSeconds, int restSecondsOverrideMin, int restSecondsOverrideMax)
        {

            if (restSecondsOverrideMin > restSecondsOverrideMax)
            {
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
	                    Logger.Instance.Log("restSecondsOverrideMin needs to be greater or equal to restSecondsOverrideMax", LogLevel.errors);
                    }
                }
                #endif
                return PsaiResult.invalidParam;
            }

            if ( fadeOutMilliSeconds < 0)
            {
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
	                    Logger.Instance.Log("negative parameters are not allowed", LogLevel.errors);
                    }
                }
                #endif
                return PsaiResult.invalidParam;
            }

            ///////////////////////////////////////////////

            if (restSecondsOverrideMin >=0 && restSecondsOverrideMax >=0 )
            {
                m_restModeSecondsOverride = GetRandomInt(restSecondsOverrideMin, restSecondsOverrideMax);
            }
            else
            {
                m_restModeSecondsOverride = -1;
            }
            

            if (!immediately)
            {
                InitiateTransitionToRestMode();
            }
            else
            {
                startFade(m_currentVoiceNumber, fadeOutMilliSeconds, 0);
                EnterRestMode(GetLastBasicMoodId(), getEffectiveThemeId());
            }
            return PsaiResult.OK;
        }


        /* sets the psaiState to PSAISTATE_REST, where no music is played for a period defined in the themes.psai file.
	    * param themeId the theme that will affect the duration of the rest, and will be played automatically when the rest is over.
	    * param restMillis the milliseconds to rest. Pass 0 to have the resting millis calculated based on the authored data.
	    */
	    void EnterRestMode(int themeIdToWakeUpWith, int themeIdToUseForRestingTimeCalculation)
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
            {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("--- Entering rest mode. Will wake up with Theme ");
	                sb.Append(themeIdToWakeUpWith);
	                Logger.Instance.Log(sb.ToString(), LogLevel.info);
                }
            }
		    #endif

		    m_psaiState = PsaiState.rest;
            m_holdIntensity = false;
            m_timerStartSnippetPlayback.Stop();     // may be necessary if we have been fading out immediately
            m_timerSegmentEndApproaching.Stop();    // may be necessary if we have been fading out immediately
            m_timerWakeUpFromRest.Stop();

            m_effectiveTheme = m_soundtrack.getThemeById(themeIdToWakeUpWith);      // the effective Theme is also valid during Rest Mode

		    if (m_effectiveTheme != null)
		    {
                int millisTimerRest = 0;

                if (m_restModeSecondsOverride > 0)
                {

          		    #if !(PSAI_NOLOG)
                    {
                        if (LogLevel.info <= Logger.Instance.LogLevel)
                        {
	                        StringBuilder sb = new StringBuilder();
	                        sb.Append("--- resting time is based on override values.");
	                        Logger.Instance.Log(sb.ToString(), LogLevel.info);
                        }
                    }
		            #endif

                    millisTimerRest = m_restModeSecondsOverride;
                    m_restModeSecondsOverride = -1;
                }
                else
                {
                    Theme themeRest = m_soundtrack.getThemeById(themeIdToUseForRestingTimeCalculation);
                    if (themeRest != null)
                    {
                        #if !(PSAI_NOLOG)
                        {
                            if (LogLevel.info <= Logger.Instance.LogLevel)
                            {
	                            StringBuilder sb = new StringBuilder();
	                            sb.Append("--- resting time is based on Theme ");
                                sb.Append(themeRest.Name);
	                            Logger.Instance.Log(sb.ToString(), LogLevel.info);
                            }
                        }
                        #endif

                        millisTimerRest = GetRandomInt(themeRest.restSecondsMin, themeRest.restSecondsMax) * 1000;
                    }
                    else
                    {
                        #if !(PSAI_NOLOG)
                        {
                            if (LogLevel.warnings <= Logger.Instance.LogLevel)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append("--- resting time is based on Theme ");
                                sb.Append(m_effectiveTheme.Name);
                                sb.Append("(themeIdToUseForRestingTimeCalculation was not found: ");
                                sb.Append(themeIdToUseForRestingTimeCalculation);
                                sb.Append(" )");
                                Logger.Instance.Log(sb.ToString(), LogLevel.warnings);
                            }
                        }
                        #endif

                        millisTimerRest = GetRandomInt(m_effectiveTheme.restSecondsMin, m_effectiveTheme.restSecondsMax) * 1000;
                    }
                }


			    if (millisTimerRest > 0)
			    {
				    m_timeStampRestStart = GetTimestampMillisElapsedSinceInitialisation();

				    #if !(PSAI_NOLOG)
				    {
                        if (LogLevel.info <= Logger.Instance.LogLevel)
                        {
	                        StringBuilder sb = new StringBuilder();
	                        sb.Append("...resting for ");
						    sb.Append(millisTimerRest);
	                        sb.Append(" ms");
	                        Logger.Instance.Log(sb.ToString(), LogLevel.info);
                        }
				    }
				    #endif

                    m_timerWakeUpFromRest.SetTimer(millisTimerRest, 0);				    
			    }
			    else
			    {
				    #if !(PSAI_NOLOG)
				    {
                        if (LogLevel.info <= Logger.Instance.LogLevel)
                        {
                        	Logger.Instance.Log("resting time is zero, starting again immediately.", LogLevel.info);
                        }
				    }
				    #endif

				    WakeUpFromRestHandler();
			    }
		    }
		    else
		    {
			    #if !(PSAI_NOLOG)
			    {
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("can't go to rest because Theme wasn't found!", LogLevel.errors);
                    }
			    }
			    #endif
		    }
        }


        void WakeUpFromRestHandler()
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
                	Logger.Instance.Log("waking up from musical rest", LogLevel.info);
                }
		    }
		    #endif

		    if (m_effectiveTheme != null)
		    {
			    PlayThemeNowOrAtEndOfCurrentSegment(m_effectiveTheme.id, m_effectiveTheme.intensityAfterRest, m_effectiveTheme.musicDurationAfterRest, true, false);			
			    
			    m_psaiState = PsaiState.playing;
			    m_psaiStateIntended = PsaiState.playing;
		    }
	    }


        /** evaluates and returns the best compatible follow-up-Segment (or compatible HighlightLayer-Segment respectively) to the given Segment. 
	    * The evaluation is based on the list of registered followers-tiles to the predecessor Segment, the number of replays,
	    * the intensity and the desired suitabilities of the follower Segment
	    * @param sourceSegment  	the predecessor Segment
	    * @param intensity			the desired intensity of the follower Segment
	    * @param allowedSegmentSuitabilities		the allowed suitabilities of the follower Segment as a bitwise OR combination
	    * @return a pointer to the best follower Segment
	    */
	    Segment GetBestCompatibleSegment(Segment sourceSegment, int targetThemeId, float intensity, int allowedSegmentSuitabilities)
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
		    {
                /*
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("GetBestCompatibleSnippet() currentSnippetId=");
	                sb.Append(currentSnippetId);
	                sb.Append("  intensity=");
	                sb.Append(intensity);
				    sb.Append("  allowedSnippetTypes=");
	                sb.Append(Snippet.GetStringOfSnippetTypes(allowedSnippetTypes));
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
                */
            }
            #endif

		    float maxDeltaIntensity = 0.0f;
		    int minPlaycount = 0;
		    int maxPlaycount = 0;

		    //Snippet currentSnippet = GetSnippetById(currentSnippetId);     // TODO: overhead, lieber gleich m_currentTheme als Pointer übergeben

            if (sourceSegment == null)
		    {
                return null;
		    }

		    // Vergleichsliste aufbauen
		    List<Follower> snippetList = new List<Follower>();
            int nachfolgerCount = sourceSegment.Followers.Count;

		    for (int i=0; i < nachfolgerCount; i++)
		    {
                int id = sourceSegment.Followers[i].snippetId;

                Segment tempTeil = m_soundtrack.GetSegmentById(id);			// TODO: overhead. extend Nachfolger datastructure by snippet type ?
			
			    if (tempTeil != null)
			    {
				    if ((allowedSegmentSuitabilities & tempTeil.SnippetTypeBitfield) > 0 && tempTeil.ThemeId == targetThemeId)
				    {  
					    if (i==0)
					    {
						    minPlaycount = tempTeil.Playcount;
					    }
					    else
					    {
						    if (tempTeil.Playcount < minPlaycount) 
							    minPlaycount = tempTeil.Playcount;
					    }

					    if (tempTeil.Playcount > maxPlaycount) 
						    maxPlaycount = tempTeil.Playcount;

					    float tDeltaIntensity = intensity - tempTeil.Intensity;

					    if (tDeltaIntensity < 0.0f) 
						    tDeltaIntensity = tDeltaIntensity * -1.0f;

					    if (tDeltaIntensity > maxDeltaIntensity) 
						    maxDeltaIntensity = tDeltaIntensity;

                        snippetList.Add(sourceSegment.Followers[i]);
				    }
			    }
		    }

		    if (snippetList.Count == 0)
		    {
			
			    #if !(PSAI_NOLOG)
			    {
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("no Segment of type ");
	                    sb.Append(Segment.GetStringFromSegmentSuitabilities(allowedSegmentSuitabilities));
	                    sb.Append(" found for Theme ");
	                    sb.Append(targetThemeId);
	                    sb.Append( " , that would be a compatible follower/layer of Segment ");
	                    sb.Append(sourceSegment.Name);
	                    Logger.Instance.Log(sb.ToString(), LogLevel.errors);
                    }
			    }
			    #endif

                return null;
		    }
		    else
		    {
                Weighting weighting = null;
                Theme pTargetTheme = m_soundtrack.getThemeById(targetThemeId);

			    if (pTargetTheme != null)
			    {
				    weighting = pTargetTheme.weightings;
			    }

			    return ChooseBestSegmentFromList(snippetList, weighting, intensity, maxPlaycount, minPlaycount, maxDeltaIntensity );
		    }
	    }


        /** evaluates and returns the best snippet to start the given theme, depending on
	    *  intensity and replay count
	    */
	    Segment GetBestStartSegmentForTheme(int themeId, float intensity)
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
		    {
                /*
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("GetBestStartSegmentForTheme() themeId=");
	                sb.Append(themeId);
	                sb.Append("  intensity=");
	                sb.Append(intensity);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
                 */
		    }
		    #endif

            Theme theme = m_soundtrack.getThemeById(themeId);
		
		    if (theme == null)
		    {			    
			    return null;
		    }
		    else
		    {
			    Segment resultSnippet = null;
			    resultSnippet = GetBestStartSegmentForTheme_internal(theme, intensity);			
			    return resultSnippet;
		    }
	    }


        Segment GetBestStartSegmentForTheme_internal(Theme theme, float intensity)
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

		    float maxDeltaIntensity = 0.0f;
		    int minAbgespielt = 0;
		    int maxAbgespielt = 0;

		    // Vergleichsliste aufbauen
		    List<Follower> segmentList = new List<Follower>();

		    int teilstueckeCount = theme.m_segments.Count;

		    for (int i=0; i < teilstueckeCount; i++)
		    {
			    Segment tempTeil = theme.m_segments[i];

			    if (tempTeil != null)
			    {
				    if (  ((int)SegmentSuitability.start & tempTeil.SnippetTypeBitfield ) > 0 )
				    {
					    if (i==0)
					    {
						    minAbgespielt = tempTeil.Playcount;
					    }
					    else
					    {
						    if (tempTeil.Playcount < minAbgespielt) 
							    minAbgespielt = tempTeil.Playcount;
					    }

					    if (tempTeil.Playcount > maxAbgespielt) 
						    maxAbgespielt = tempTeil.Playcount;


					    float tDeltaIntensity = intensity - tempTeil.Intensity;

					    if (tDeltaIntensity < 0.0f) 
						    tDeltaIntensity = tDeltaIntensity * -1.0f;

					    if (tDeltaIntensity > maxDeltaIntensity) 
						    maxDeltaIntensity = tDeltaIntensity;

					    // kreiere eine snippetType Nachfolger
					    Follower vorfolger = new Follower();
					    vorfolger.snippetId = tempTeil.Id;
                        vorfolger.compatibility = 1.0f;                 // compatiblity is ignored for Start-Segments and therefore set to 1.0f
					    segmentList.Add(vorfolger); 
				    }
			    }
			    else
			    {
				    #if !(PSAI_NOLOG)
				    {
                        if (LogLevel.errors <= Logger.Instance.LogLevel)
                        {
	                        StringBuilder sb = new StringBuilder();
	                        sb.Append("INTERNAL ERROR ! GetBestStartSegmentForTheme_internal() - a Segment with id ");
	                        sb.Append(tempTeil.Id);
	                        sb.Append(" could not be found!");
	                        Logger.Instance.Log(sb.ToString(), LogLevel.errors);
                        }
				    }
				    #endif
			    }
		    }

		    Weighting weighting = theme.weightings;

		    return ChooseBestSegmentFromList(segmentList, weighting, intensity, maxAbgespielt, minAbgespielt, maxDeltaIntensity);
	    }





        // Evaluates the most appropriate Segment to follow (or to layer respectively, for Highlights) from the list.	    
	    // This score is calculated by adding three basic scores, each one being a value between 0.0f and 1.0f, and affected by an individual weighting
	    // as authored in the psai Editor.
	    //
	    // 1. compatibility score
	    // 2. intensity score
	    // 3. variety score
	    //
	    // variety itself consists of the Segment's playcount score a random score, each affected by an individual
	    // weighting. 	
	    //
	    // TODO: maxPlaycount, minPlaycount and maxDeltaIntensity should be evaluated within this method. 
	    Segment ChooseBestSegmentFromList(List<Follower> segmentList, Weighting weighting, float intensity, int maxPlaycount, int minPlaycount, float maxDeltaIntensity)
	    {		    
		    #if !(PSAI_NOLOG)
		    {       
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("ChooseBestSegmentFromList() snippetList.size()=");
	                sb.Append(segmentList.Count);
	                sb.Append(" intensity=");
	                sb.Append(intensity);
	                sb.Append("  maxPlaycount=");
	                sb.Append(maxPlaycount);
	                sb.Append("  minPlaycount=");
	                sb.Append(minPlaycount);
	                sb.Append("  maxDeltaIntensity=");
	                sb.Append(maxDeltaIntensity);
	                sb.Append("  weighting.switchGroups=");
	                sb.Append(weighting.switchGroups);
	                sb.Append("  weighting.intensityVsVariety=");
	                sb.Append( weighting.intensityVsVariety);
	                sb.Append("  weightingLowPlaycountVsRandom=");
	                sb.Append(weighting.lowPlaycountVsRandom);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                } 
		    }
		    #endif

		    Segment resultSnippet = null;

		    if (segmentList.Count == 0)
		    {
			    #if !(PSAI_NOLOG)
			    {
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("ChooseBestSegmentFromList() empty segment list ! returning null", LogLevel.debug);
                    }
			    }
			    #endif

			    return null;
		    }

		    float playcountNormalizationFactor = 1.0f;
		    float intensityNormalizationFactor = 1.0f;


		    int deltaPlaycount = maxPlaycount - minPlaycount;

		    if (deltaPlaycount > 0)
			    playcountNormalizationFactor = (1.0f / deltaPlaycount);
		    else
			    playcountNormalizationFactor = 0.0f;

		    if (maxDeltaIntensity > 0)
			    intensityNormalizationFactor = ( 1.0f / maxDeltaIntensity );
		    else
			    intensityNormalizationFactor = 0.0f;

		    float bestSegmentScore = 0.0f;


		    int versize = segmentList.Count;
		    for (int i=0; i < versize; i++)
		    {
			    if (segmentList[i] != null)
			    {
                    Segment tmpTeilstueck = m_soundtrack.GetSegmentById(segmentList[i].snippetId);		// TODO: unnecessary waste of CPU cycles ! pass the pointers directly or use a map / dictionary
				
				    //
				    // score calculation new
				    // 
				    float weightingGroupStickyness = 1.0f - weighting.switchGroups;
				    float weightingIntensity = 1.0f - weighting.intensityVsVariety;
				    float weightingVariety = weighting.intensityVsVariety;		// Variety consists of playcount and random
				    float weightingPlaycount = 1.0f - weighting.lowPlaycountVsRandom;
				    float weightingRandom = weighting.lowPlaycountVsRandom;				

				    // switch group 
				    float compatibilityScore = segmentList[i].compatibility * weightingGroupStickyness;		// float value between 0 and 1

				    // intensity
				    float deltaIntensity = intensity - tmpTeilstueck.Intensity;
				    if (deltaIntensity < 0 )
				    {
					    deltaIntensity *= -1.0f;
				    }
				    float intensityScore = ( 1.0f - (deltaIntensity * intensityNormalizationFactor)) * weightingIntensity;
				
		
				    // variety
				    float playCountOfSnippetInPercent = ((tmpTeilstueck.Playcount - minPlaycount) * playcountNormalizationFactor);
				    float playCountScore = (1.0f - playCountOfSnippetInPercent) * weightingPlaycount;		
				
				    float randomPercentValue = GetRandomFloat();		// returns a random percent float (between 0.0f and 0.99f)
				    float randomScore = randomPercentValue * weightingRandom;				
				    float varietyScore = (playCountScore + randomScore) * weightingVariety / 2.0f;

				    float segmentScore = compatibilityScore + intensityScore + varietyScore;

				    // Vergleichen
				    if ( (resultSnippet == null) || (segmentScore > bestSegmentScore))
				    {
					    resultSnippet = tmpTeilstueck;
					    bestSegmentScore = segmentScore;
				    }

				    #if !(PSAI_NOLOG)
				    {
                        /*
                        if (LogLevel.debug <= Logger.Instance.LogLevel)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("segmentList[");
                            sb.Append(i);
                            sb.Append("]  id=");
                            sb.Append(segmentList[i].snippetId);
                            sb.Append("  segmentScore=");
                            sb.Append(segmentScore);
                            sb.Append("  compatibilityScore=");
                            sb.Append(compatibilityScore);
                            sb.Append("  intensityScore=");
                            sb.Append(intensityScore);
                            sb.Append("  varietyScore=");
                            sb.Append(varietyScore);
                            sb.Append("   [ playCountScore=");
                            sb.Append(playCountScore);
                            sb.Append("  randomScore=");
                            sb.Append(randomScore);
                            sb.Append(" ]");
                            sb.Append("    segmentList[i]->compatibility=");
                            sb.Append(segmentList[i].compatibility);
                            Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                        }
                        */
				    }
                    #endif
                }
		    }

		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(" ...returning Segment id=");
                    sb.Append(resultSnippet.Id);
                    sb.Append("  themeId=");
                    sb.Append(resultSnippet.ThemeId);
                    sb.Append("  playbackCount=");
                    sb.Append(resultSnippet.Playcount);
                    sb.Append("  intensity=");
                    sb.Append(resultSnippet.Intensity);
                    sb.Append("  bestSegmentScore=");
                    sb.Append(bestSegmentScore);
                    Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
		    }
		    #endif

		    return resultSnippet;
	    }



        private void PlaySegmentOfCurrentTheme(SegmentSuitability snippetType)
	    {
		    #if !(PSAI_NOLOG)
		    {
			    if (LogLevel.debug <= Logger.Instance.LogLevel)
			    {
			    	Logger.Instance.Log("PlaySegmentOfCurrentTheme() " + snippetType, LogLevel.debug);
			    }
		    }
		    #endif

		    if (m_effectiveTheme != null)
		    {
			    Segment nextSnippet;
			    float currentIntensity = getCurrentIntensity();

                if (m_currentSegmentPlaying != null)
                {

                    nextSnippet = GetBestCompatibleSegment(m_currentSegmentPlaying, m_effectiveTheme.id, currentIntensity, (int)snippetType);

                    m_targetSegmentSuitabilitiesRequested = (int)snippetType;

                    if (nextSnippet == null)
                    {
                        #if !(PSAI_NOLOG)
                        {
                            if (LogLevel.warnings <= Logger.Instance.LogLevel)
                            {
	                            StringBuilder sb = new StringBuilder();
	                            sb.Append("No compatible Segment of suitability ");
	                            sb.Append(snippetType);
	                            sb.Append(" found for Segment ");
	                            sb.Append(m_currentSegmentPlaying.Name);
	                            sb.Append("  Trying to substitute..!");
	                            Logger.Instance.Log(sb.ToString(), LogLevel.warnings);
                            }
                        }
                        #endif

                        nextSnippet = substituteSegment(m_effectiveTheme.id);
                    }

                    if (nextSnippet != null)
                    {
                        PlaySegment(nextSnippet, false);
                    }
                }
                else
                {
                    #if !(PSAI_NOLOG)
                    {
                        if (LogLevel.errors <= Logger.Instance.LogLevel)
                        {
                        	Logger.Instance.Log("INTERNAL ERROR! PlayEndSegmentOfCurrentTheme() - m_currentSegmentPlaying was NULL", LogLevel.errors);
                        }
                    }
                    #endif
                }
		    }
	    }

        
        private Segment substituteSegment(int themeId)
	    {
		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("substituteSegment() themeId=");
	                sb.Append(themeId);
	                Logger.Instance.Log(sb.ToString(), LogLevel.info);
                }
		    }
		    #endif

		    Segment targetSnippet;
		    targetSnippet = null;

		    // try to play the first Segment of the currentTheme
            Theme targetTheme = m_soundtrack.getThemeById(themeId);
		    if (targetTheme != null && targetTheme.m_segments.Count > 0)
		    {
			    targetSnippet = targetTheme.m_segments[0];
		    }

		    // still no Segment ??? play some Segment from the last Basic Mood !
		    if (targetSnippet == null)
		    {
			    #if !(PSAI_NOLOG)
			    {
				    if (LogLevel.errors <= Logger.Instance.LogLevel)
				    {
				    	Logger.Instance.Log("Failed to substiture Segment - No Segment found within the same Theme!", LogLevel.errors);
				    }
			    }
			    #endif
		    }
            else
		    {
			    #if !(PSAI_NOLOG)
			    {
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("Segment was substituted by Segment ");
	                    sb.Append(targetSnippet.Id);
	                    Logger.Instance.Log(sb.ToString(), LogLevel.warnings);
                    }
			    }
			    #endif
		    }

		    return targetSnippet;
	    }


        private void EnterSilenceMode()
	    {
		    // enter silence mode
		   #if !(PSAI_NOLOG)
		    if (LogLevel.info <= Logger.Instance.LogLevel)
		    {
		    	Logger.Instance.Log("entering Silence Mode", LogLevel.info);
		    }
		   #endif
            
            m_timerStartSnippetPlayback.Stop();
            m_timerSegmentEndApproaching.Stop();

            m_targetSegment = null;
            m_effectiveTheme = null;
            m_scheduleFadeoutUponSnippetPlayback = false;

            m_psaiStateIntended = PsaiState.silence;
            m_psaiState = PsaiState.silence;        
	    }

       	internal bool menuModeIsActive()
	    {
		    return m_psaiPlayMode == PsaiPlayMode.menuMode;
	    }

	    internal bool cutSceneIsActive()
	    {
		    return m_psaiPlayModeIntended == PsaiPlayMode.cutScene;
	    }


        internal PsaiResult MenuModeEnter(int menuThemeId, float menuIntensity)
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("entering Menu Mode, menuTheme id=");
	                sb.Append(menuThemeId);
	                sb.Append("  , intensity=");
	                sb.Append(menuIntensity);
	                Logger.Instance.Log(sb.ToString(), LogLevel.info);
                }

                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb2 = new StringBuilder();
	                sb2.Append("MenuModeEnter()  m_themeQueue.size()=");
	                sb2.Append(m_themeQueue.Count);
	                Logger.Instance.Log(sb2.ToString(), LogLevel.debug);
                }
		    }
		    #endif

		    if (m_initializationFailure)
		    {
			    return PsaiResult.initialization_error;
		    }

		    //////////////////////////////////////////////////////////////////////////

            if (m_paused)
            {
                setPaused(false);
            }


		    if (m_psaiPlayMode != PsaiPlayMode.menuMode)
		    {

			    // special case: we were leaving a cutscene and the cutscene theme is still playing,
			    // so we don't want to return to the cutscene afterwards
			    if (m_psaiPlayMode == PsaiPlayMode.cutScene && m_psaiPlayModeIntended == PsaiPlayMode.regular)
			    {
				    #if !(PSAI_NOLOG)
				    {
                        if (LogLevel.debug <= Logger.Instance.LogLevel)
                        {
	                        StringBuilder sb = new StringBuilder();
	                        sb.Append("special case: Menu Mode entered when we were just returning from the cutscene.");
	                        sb.Append("  m_currentTheme->id=");
	                        sb.Append(m_effectiveTheme.id);
	                        sb.Append("  m_targetSegment=");
	                        sb.Append(m_targetSegment);
	                        Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                        }
				    }
				    #endif

				    PushEffectiveThemeToThemeQueue(m_psaiPlayModeIntended);
			    }
			    else
			    {
                    PushEffectiveThemeToThemeQueue(m_psaiPlayMode);
			    }

                Theme menuTheme = m_soundtrack.getThemeById(menuThemeId);
			    if (menuTheme != null)
			    {
				    PlayThemeNowOrAtEndOfCurrentSegment(menuThemeId, menuIntensity, 666, true, true);
			    }
			    else
			    {				
				    // TODO: kein Menu Thema wurde gesetzt, d.h. der Menu Mode soll ohne Musik gestartet werden. Hier muss die Musik pausiert und ergo der psai status irgendwie eingefroren werden. Timer killen, timestamps neu berechnen
				    PlayThemeNowOrAtEndOfCurrentSegment(m_lastBasicMood.id, menuIntensity, 666, true, true);  // kommt raus sobald die pause funzt
			    }

			    SetPlayMode(PsaiPlayMode.menuMode);					
			    m_psaiPlayModeIntended = PsaiPlayMode.menuMode;
			    return PsaiResult.OK;
		    }		
		    else
		    {
			    #if !(PSAI_NOLOG)
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("Menu Mode already active", LogLevel.warnings);
                    }
			    #endif

			    return PsaiResult.commandIgnoredMenuModeActive;
		    }
	    }

	    internal PsaiResult MenuModeLeave()
	    {
		   // boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
                	Logger.Instance.Log("MenuModeLeave", LogLevel.info);
                }

                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append(" m_themeQueue.size()=");
	                sb.Append(m_themeQueue.Count);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }			    
		    }
		    #endif

		    if (m_initializationFailure)
		    {
			    #if !(PSAI_NOLOG)
			    {
                    Logger.Instance.Log(LOGMESSAGE_ABORTION_DUE_TO_INITIALIZATION_FAILURE, LogLevel.errors);
			    }
			    #endif

			    return PsaiResult.initialization_error;
		    }

		    //////////////////////////////////////////////////////////////////////////

            if (m_paused)
            {
                setPaused(false);
            }

		    if (m_psaiPlayMode == PsaiPlayMode.menuMode)
		    {
			    if (getFollowingThemeQueueEntry() != null)
			    {
				    PopAndPlayNextFollowingTheme(true);
				    return PsaiResult.OK;
			    }
			    else
			    {
				    m_psaiStateIntended = PsaiState.silence;
				    m_psaiState = PsaiState.silence;
				    SetPlayMode(PsaiPlayMode.regular);
				    m_psaiPlayModeIntended = PsaiPlayMode.regular;

				    StopMusic(true);
				    return PsaiResult.OK;
			    }
		    }
		    else
		    {
			    #if !(PSAI_NOLOG)
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("MenuModeLeave() ignored - MenuMode wasn't active. Call MenuModeEnter() first !", LogLevel.warnings);
                    }
			    #endif

			    return PsaiResult.commandIgnored;
		    }
	    }


	    internal PsaiResult CutSceneEnter(int themeId, float intensity)
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("CutSceneEnter(), theme id=");
	                sb.Append(themeId);
	                sb.Append("  , intensity=");
	                sb.Append(intensity);
	                Logger.Instance.Log(sb.ToString(), LogLevel.info);
                }

                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb2 = new StringBuilder();
	                sb2.Append( "  m_themeQueue.size()=");
	                sb2.Append(m_themeQueue.Count);
	                Logger.Instance.Log(sb2.ToString(), LogLevel.debug);
                }
		    }
		    #endif

		    if (m_initializationFailure)
		    {
			    #if !(PSAI_NOLOG)
			    {				    
                    Logger.Instance.Log(LOGMESSAGE_ABORTION_DUE_TO_INITIALIZATION_FAILURE, LogLevel.errors);
			    }
			    #endif

			    return PsaiResult.initialization_error;
		    }

		    //////////////////////////////////////////////////////////////////////////


		    switch(m_psaiPlayModeIntended)		// we use 'intended' here and not m_psaiPlayMode, so immediate switches back to Cutscene after leaving will be ignored and not cause trouble
		    {
		    case PsaiPlayMode.cutScene:
			    {
				    #if !(PSAI_NOLOG)
                        if (LogLevel.warnings <= Logger.Instance.LogLevel)
                        {
                        	Logger.Instance.Log("Cutscene Mode already active - command ignored !", LogLevel.warnings);					    
                        }
				    #endif

				    return PsaiResult.commandIgnoredCutsceneActive;
			    }
			    //break;

		    case PsaiPlayMode.menuMode:
			    {
				    #if !(PSAI_NOLOG)
                        if (LogLevel.warnings <= Logger.Instance.LogLevel)
                        {
                        	Logger.Instance.Log("MenuMode active - command ignored !", LogLevel.warnings);
                        }
				    #endif

				    return PsaiResult.commandIgnoredMenuModeActive;
			    }
			    //break;

		    default:
			    {
				    PushEffectiveThemeToThemeQueue(PsaiPlayMode.regular);

                    Theme cutSceneTheme = m_soundtrack.getThemeById(themeId);
				    SetPlayMode(PsaiPlayMode.cutScene);
				    m_psaiPlayModeIntended = PsaiPlayMode.cutScene;
				    if (cutSceneTheme != null)
				    {
					    PlayThemeNowOrAtEndOfCurrentSegment(themeId, intensity, cutSceneTheme.musicDurationGeneral, true, true);
					    return PsaiResult.OK;
				    }
				    else
				    {									
					    PlayThemeNowOrAtEndOfCurrentSegment(m_lastBasicMood.id, intensity, m_lastBasicMood.musicDurationGeneral, true, true);
					    return PsaiResult.unknown_theme;
				    }				

			    }
			    //break;
		    }
	    }


	    internal PsaiResult CutSceneLeave(bool immediately, bool reset)
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.info <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("CutSceneLeave()  immediately=");
	                sb.Append(immediately);
	                sb.Append("  reset=");
	                sb.Append(reset);
	                Logger.Instance.Log(sb.ToString(), LogLevel.info);
                }

                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb2 = new StringBuilder();
	                sb2.Append("  m_themeQueue.size()=");
	                sb2.Append(m_themeQueue.Count);
	                Logger.Instance.Log(sb2.ToString(), LogLevel.debug);
                }
		    }
		    #endif

		    if (m_initializationFailure)
		    {
			    #if !(PSAI_NOLOG)
			    {
                    Logger.Instance.Log(LOGMESSAGE_ABORTION_DUE_TO_INITIALIZATION_FAILURE, LogLevel.errors);
			    }
			    #endif

			    return PsaiResult.initialization_error;
		    }

		    //////////////////////////////////////////////////////////////////////////

		    if (m_psaiPlayMode == PsaiPlayMode.cutScene && m_psaiPlayModeIntended == PsaiPlayMode.cutScene)
		    {

			    if (reset)
			    {
				    m_themeQueue.Clear();
				    #if !(PSAI_NOLOG)
				    {
                        if (LogLevel.debug <= Logger.Instance.LogLevel)
                        {
                        	Logger.Instance.Log("m_themeQueue cleared", LogLevel.debug);
                        }
				    }
				    #endif
			    }

			    if (getFollowingThemeQueueEntry() != null)
			    {
				    m_psaiPlayModeIntended = PsaiPlayMode.regular;
				    PopAndPlayNextFollowingTheme(immediately);
				    return PsaiResult.OK;
			    }
			    else
			    {
				    m_psaiStateIntended = PsaiState.silence;
				    m_psaiState = PsaiState.silence;
				    m_psaiPlayModeIntended = PsaiPlayMode.regular;
				    StopMusic(immediately);
				    return PsaiResult.OK;
			    }
		    }
		    else
		    {
			    #if !(PSAI_NOLOG)
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("CutSceneLeave() ignored - no CutScene was active. Call CutSceneEnter() first!", LogLevel.warnings);				    
                    }
			    #endif

			    return PsaiResult.commandIgnored;
		    }
	    }


       	ThemeQueueEntry getFollowingThemeQueueEntry()
	    {
		    if (m_themeQueue.Count > 0)
		    {
                return m_themeQueue[0];
		    }		
		    else
			    return null;
	    }


        void SetPlayMode(PsaiPlayMode playMode)
	    {
		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("SetPlayMode() ");
	                sb.Append(playMode);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
		    }
		    #endif

		    m_psaiPlayMode = playMode;
	    }


        // creates a themequeue-Entry for the theme currently playing and pushes it on the theme queue stack. 
	    // If a theme change was imminent, the upcoming theme is stored instead.
	    // Used internally for entering Mode Mode and Cut Scenes
	    void PushEffectiveThemeToThemeQueue(PsaiPlayMode playModeToReturnTo)
	    {
		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("PushEffectiveThemeToThemeQueue()  playModeToReturnTo=");
	                sb.Append(playModeToReturnTo);
	                sb.Append("  m_nonInterruptingTriggerCallOfHighestPriority.themeId=");
	                sb.Append(m_nonInterruptingTriggerOfHighestPriority.themeId);
	                sb.Append("  m_currentSegmentPlaying=");                
	                if (m_currentSegmentPlaying != null)
	                {
	                    sb.Append(m_currentSegmentPlaying.Name);
	                }
	                else
	                {
	                    sb.Append("null");
	                }
	                sb.Append("  m_targetSegment=");
	                if (m_targetSegment != null)
	                {
	                    sb.Append(m_targetSegment.Name);
	                }
	                else
	                {
	                    sb.Append("null");
	                }
	
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                } 
		    }
		    #endif


            // if we switch from Rest-Mode into MenuMode, we need to remember the remaining milliseconds until we wake up from rest again
		    if (m_psaiState == PsaiState.rest)
		    {
                int restModeRemainingMillis = GetTimestampMillisElapsedSinceInitialisation() - m_timeStampRestStart;
			    m_timerWakeUpFromRest.Stop();

                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log(".. we were in Rest Mode, restModeRemainingMillis=" + restModeRemainingMillis, LogLevel.debug);
                    }
                }
                #endif

                pushThemeToThemeQueue(m_lastBasicMood.id, m_lastBasicMood.intensityAfterRest, 0, true, restModeRemainingMillis, PsaiPlayMode.regular, false);
                return;
		    }



            if (m_nonInterruptingTriggerOfHighestPriority.themeId != -1)
            {
                // we have received a non-interrupting trigger, so we pack this on the themeQueue.
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("...m_nonInterruptingTrigger was set, themeId=");
	                    sb.Append(m_nonInterruptingTriggerOfHighestPriority.themeId);
	                    Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                    }
                }
                #endif

                Theme nonInterruptingTheme = m_soundtrack.getThemeById(m_nonInterruptingTriggerOfHighestPriority.themeId);
                pushThemeToThemeQueue(nonInterruptingTheme.id, m_nonInterruptingTriggerOfHighestPriority.startIntensity, m_nonInterruptingTriggerOfHighestPriority.musicDuration, false, 0, PsaiPlayMode.regular, false);

                // clear non-interrupting trigger call, as we would otherwise return this after the first Snippet of the interrupting Theme would have played
                m_nonInterruptingTriggerOfHighestPriority.themeId = -1;

                return;
            }



            // no trigger is pending, business as usual

            Segment effectiveSegment = GetEffectiveSegment();
            if (effectiveSegment != null)
            {

                // Special case: we have already targeted a Snippet of some other theme, or no snippet is playing right now.
                // This means we have to switch to the target Theme with the maximum music duration
                if (effectiveSegment == m_targetSegment && m_currentSegmentPlaying == null || (m_targetSegment != null && m_currentSegmentPlaying != null && m_targetSegment.ThemeId != m_currentSegmentPlaying.ThemeId))
                {
                    #if !(PSAI_NOLOG)
                    {
                        if (LogLevel.debug <= Logger.Instance.LogLevel)
                        {
	                        StringBuilder sb = new StringBuilder();
	                        sb.Append("setting targetSegment.themeId as the following Theme, storing full music duration of themeId=");
	                        sb.Append(effectiveSegment.ThemeId);
	                        Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                        }
                    }
                    #endif

                    Theme targetTheme = m_soundtrack.getThemeById(m_targetSegment.ThemeId);
                    pushThemeToThemeQueue(m_targetSegment.ThemeId, getUpcomingIntensity(), targetTheme.musicDurationGeneral, false, 0, playModeToReturnTo, m_holdIntensity);
                }
                else
                {
                    // business as usual
                    #if !(PSAI_NOLOG)
                    {
                        if (LogLevel.debug <= Logger.Instance.LogLevel)
                        {
	                        StringBuilder sb = new StringBuilder();
	                        sb.Append("pushing themeId of the Effective Segment to Theme Queue, themeId=");
	                        sb.Append(effectiveSegment.ThemeId);
	                        Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                        }
                    }
                    #endif

                    pushThemeToThemeQueue(effectiveSegment.ThemeId, getCurrentIntensity(), GetRemainingMusicDurationSecondsOfCurrentTheme(), false, 0, playModeToReturnTo, m_holdIntensity);
                }
            }
            else
            {
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("not pushing anything to themeQueue. effectiveSnippet=");
	                    sb.Append(GetEffectiveSegment());
	                    sb.Append("  m_targetSegment=");
	                    sb.Append(m_targetSegment);
	                    Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                    }
                }
                #endif
            }
	    }


        Segment GetEffectiveSegment()
	    {
		    if (m_targetSegment != null)
		    {
			    return m_targetSegment;
		    }
		    else if (m_currentSegmentPlaying != null)
		    {
			    return m_currentSegmentPlaying;
		    }

		    return null;
	    }


        internal int getEffectiveThemeId()
        {
            if (m_effectiveTheme != null)
                return m_effectiveTheme.id;
            else
                return -1;
        }

               
        int GetEffectiveSegmentSuitabilitiesRequested()
	    {
		    if (m_targetSegment != null)
		    {
			    return m_targetSegmentSuitabilitiesRequested;
		    }
		    else
		    {
			    return m_currentSnippetTypeRequested;
		    }		
	    }



        /** Pops and Plays the next entry in the followingThemeQueue, using the previously stored parameters
	    * for intensity, themeId and SnippetTypes. Removes the themeQueueEntry from the queue afterwards.
	    * @param immediately Play the Theme immediately
	    */ 
	    void PopAndPlayNextFollowingTheme(bool immediately)
	    {
		    //boost::recursive_mutex::scoped_lock block(m_pnxLogicMutex);

		    #if !(PSAI_NOLOG)
		    {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("PopAndPlayNextFollowingTheme()  m_themeQueue.size()=");
	                sb.Append(m_themeQueue.Count);
	                sb.Append("  immediately=");
	                sb.Append(immediately);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
		    }
		    #endif

		    if (getFollowingThemeQueueEntry() != null)
		    {
			    ThemeQueueEntry tqe = getFollowingThemeQueueEntry();
			    m_psaiPlayModeIntended = tqe.playmode;
                
			    switch (m_psaiPlayModeIntended)
			    {
			    case PsaiPlayMode.regular:
				    {
					    if (tqe.restTimeMillis == 0)
					    {
						    PlayThemeNowOrAtEndOfCurrentSegment(tqe, immediately);
					    }
					    else
					    {
						    EnterRestMode(tqe.themeId, tqe.themeId);
					    }
				    }
				    break;

			    case PsaiPlayMode.cutScene:
				    {
					    PlayThemeNowOrAtEndOfCurrentSegment(tqe.themeId, tqe.startIntensity, tqe.musicDuration, immediately, tqe.holdIntensity);
				    }
				    break;

			    case PsaiPlayMode.menuMode:
				    {
					    PlayThemeNowOrAtEndOfCurrentSegment(tqe.themeId, tqe.startIntensity, tqe.musicDuration, immediately, tqe.holdIntensity);
				    }
				    break;

			    default:
				    {
				    #if !(PSAI_NOLOG)
                        if (LogLevel.errors <= Logger.Instance.LogLevel)
                        {
                        	Logger.Instance.Log("unkown PSAIPLAYMODE !", LogLevel.errors);
                        }
				    #endif
				    }
				    break;
			    }
			    removeFirstFollowingThemeQueueEntry();
		    }
		    else
		    {
			    #if !(PSAI_NOLOG)
			    {
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
	                    StringBuilder sb = new StringBuilder();
	                    sb.Append("PopAndPlayNextFollowingTheme() - themeQueue is empty!");
	                    Logger.Instance.Log(sb.ToString(), LogLevel.errors);
                    }
                    //AbortWithFatalErrorMessage("FATAL INTERNAL ERROR: PopAndPlayNextFollowingTheme() - themeQueue is empty! !");
			    }
			    #endif
			
		    }
	    }

       	void removeFirstFollowingThemeQueueEntry()
	    {
		    if (m_themeQueue.Count > 0)
		    {
			    m_themeQueue.RemoveAt(0);
		    }
	    }


        internal PsaiInfo getPsaiInfo()
	    {
            PsaiInfo psaiInfo = new PsaiInfo();
		    psaiInfo.psaiState = m_psaiState;
            psaiInfo.upcomingPsaiState = m_psaiStateIntended;

            // lastBasicMoodThemeId
            if (m_lastBasicMood != null)
            {
                psaiInfo.lastBasicMoodThemeId = m_lastBasicMood.id;
            }
            else
            {
                psaiInfo.lastBasicMoodThemeId = -1;
            }
              
            psaiInfo.effectiveThemeId = getEffectiveThemeId();
            psaiInfo.upcomingThemeId = getUpcomingThemeId();
            psaiInfo.currentIntensity = getCurrentIntensity();
            psaiInfo.upcomingIntensity = getUpcomingIntensity();
            psaiInfo.intensityIsHeld = m_holdIntensity;
		    psaiInfo.themesQueued = m_themeQueue.Count;
            psaiInfo.returningToLastBasicMood = m_returnToLastBasicMoodFlag;
            psaiInfo.paused = m_paused;
            
            //targetSnippetId
		    if (m_targetSegment != null)
		    {
			    psaiInfo.targetSegmentId = m_targetSegment.Id;			    
		    }
		    else
		    {
			    psaiInfo.targetSegmentId = -1;
		    }

            // remainingMillisecondsInRestMode
            if (m_timerWakeUpFromRest.IsSet())
            {
                psaiInfo.remainingMillisecondsInRestMode = m_timerWakeUpFromRest.GetRemainingMillisToFireTime();
            }

            return psaiInfo;
	    }



        internal int getCurrentSnippetId()
        {
            if (m_currentSegmentPlaying != null)
            {
                return m_currentSegmentPlaying.Id;
            }
            else
            {
                return -1;
            }
        }


        // returns the remainingMillis of current snippet playback, or -1 if no snippet is playing
	    internal int GetRemainingMillisecondsOfCurrentSegmentPlayback()
	    {
		    if (m_currentSegmentPlaying != null)
		    {
			    return (int) (m_currentSegmentPlaying.audioData.GetFullLengthInMilliseconds() - GetMillisElapsedAfterCurrentSnippetPlaycall());
            }
		    else
			    return -1;
	    }


        // returns the remaining milliseconds until the next snippet will start, or -1 if not set
	    internal int GetRemainingMillisecondsUntilNextSegmentStart()
	    {
		    if (m_timerStartSnippetPlayback.IsSet())
            {
                if (m_timerStartSnippetPlayback.IsSet())
                {
                    return m_timerStartSnippetPlayback.GetRemainingMillisToFireTime();
                }                
            }
			return -1;
	    }


        internal bool CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(int sourceSegmentId, int targetThemeId)
        {
            Segment sourceSegment = m_soundtrack.GetSegmentById(sourceSegmentId);
            if (sourceSegment != null)
            {
                return sourceSegment.CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(m_soundtrack, targetThemeId);
            }
            return false;
        }

        internal void AddLoggerOutput(psai.net.LoggerOutput loggerOutput)
        {
            if (!Logger.Instance.LoggerOutputs.Contains(loggerOutput))
            {
                Logger.Instance.LoggerOutputs.Add(loggerOutput);
            }            
        }

    } // Logik
} // namespace
