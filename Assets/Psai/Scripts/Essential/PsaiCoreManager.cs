//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using psai.net;

/// <summary>
/// Holds basic settings of the psai playback engine and is responsible for filtering and managing concurrent calls from Trigger Scripts that might otherwise interfere with each other.
/// </summary>
public class PsaiCoreManager : MonoBehaviour
{
    private static PsaiCoreManager _instance;
    public static PsaiCoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PsaiCoreManager>();
            }
            return _instance;
        }
    }


    /// <summary>
    /// Sets the amount of log information that psai writes to the Unity console.
    /// </summary>
    /// <remarks>
    /// Please stop your game before changing this setting. Changes at runtime will have no effect.
    /// </remarks>
    public psai.net.LogLevel LogLevel = psai.net.LogLevel.errors;

    public bool logTriggerScripts;
    private List<TriggerCall> _synchronizedOneShotTriggerCallsForNextTick = new List<TriggerCall>();

    public struct TriggerCall
    {
        public int themeId;
        public int musicDurationInSeconds;
        public float intensity;
        public bool forceImmediateStopBeforeTriggering;
        public PsaiSynchronizedTrigger senderBehaviour;

        public TriggerCall(PsaiSynchronizedTrigger senderTrigger, int themeId, float intensity, int musicDurationInSeconds)
        {
            this.senderBehaviour = senderTrigger;
            this.themeId = themeId;
            this.intensity = intensity;
            this.musicDurationInSeconds = musicDurationInSeconds;
            this.forceImmediateStopBeforeTriggering = false;
        }

        public override string ToString()
        {
            return "themeId: " + themeId + " intensity:" + intensity.ToString("F4") + " (" + this.senderBehaviour + ")";
        }
    }


    public enum PlaybackControlType
    {
        None = 0,
        StopMusic,
        GoToRest,
        ReturnToLastBasicMood
    }


    /** We cannot store the PsaiPlaybackControl behaviours to execute their calls later, as their gameObjects are likely to get destroyed
     * and thus be null when we access it in the next frame. An alternative would be to clone them, but we want to avoid memory allocation at runtime.
     * So we use a sinlge struct for all playback control calls (PsaiStopMusic, ReturnToLastBasicMood)
     */
    public struct PlaybackControlCall
    {
        public PlaybackControlType controlType;
        public bool immediately;
        public float fadeOutSeconds;
        public bool dontExecuteIfOtherTriggersAreFiring;
        public ThemeType restrictBlockToThisThemeType;
        public int overrideRestSecondsMin;
        public int overrideRestSecondsMax;

        public void Init()
        {
            controlType = PlaybackControlType.None;
            immediately = false;
            fadeOutSeconds = 0;
            dontExecuteIfOtherTriggersAreFiring = false;
            restrictBlockToThisThemeType = ThemeType.none;
        }
    }

    
    private PlaybackControlCall playbackControlCallFiringInThisTick;    

    /// <summary>
    /// The interval in seconds in which continuously firing Trigger Scripts will be evaluated.
    /// </summary>
    /// <remarks>
    /// Higher intervals will save some CPU cycles, while low intervals (e.g. 0.2 seconds) will make your soundtrack react as quickly as possible.
    /// </remarks>
    public float tickIntervalInSeconds = 0.2f;

    private float triggerTickIntervalCounter;
    private Dictionary<int, TriggerCall> mapThemeIdsToTriggerCalls = new Dictionary<int, TriggerCall>();
    private List<PsaiSynchronizedTrigger> continuousTriggersInScene = new List<PsaiSynchronizedTrigger>();

    public float Volume
    {
        get
        {
            return PsaiCore.Instance.GetVolume();
        }
        set
        {
            PsaiCore.Instance.SetVolume(value);
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // These are the default latency values (milliseconds) that psai will allow
    // the target system to buffer and play back compressed audio files.
    // Depending on your target system specifications you may choose to reduce
    // these values to make your soundtrack react quicker. Doing so may result
    // in audio stuttering on slower machines, so make sure to test your settings.
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// The time given by psai for buffering sounds on standalone builds (Windows, OSX, Linux)
    /// </summary>
    public int MaxBufferingLatencyMsStandalone = 100;

    /// <summary>
    /// The time given by psai to play back a buffered sound on standalone builds (Windows, OSX, Linux, Windows Store Apps)
    /// </summary>
    public int MaxPlaybackLatencyMsStandalone = 50;
    
    public int MaxBufferingLatencyMsXBox360 = 100;
    public int MaxPlaybackLatencyMsXBox360 = 50;

    public int MaxBufferingLatencyMsXBoxOne = 100;
    public int MaxPlaybackLatencyMsXBoxOne = 50;

    public int MaxBufferingLatencyMsPS3 = 100;
    public int MaxPlaybackLatencyMsPS3 = 50;

    public int MaxBufferingLatencyMsPS4 = 100;
    public int MaxPlaybackLatencyMsPS4 = 50;

    public int MaxBufferingLatencyMsWebplayer = 150;
    public int MaxPlaybackLatencyMsWebplayer = 50;

    public int MaxBufferingLatencyMsIOS = 200;
    public int MaxPlaybackLatencyMsIOS = 170;

    public int MaxBufferingLatencyMsAndroid = 200;
    public int MaxPlaybackLatencyMsAndroid = 170;

    public int MaxBufferingLatencyMsWindowsStoreApps = 200;
    public int MaxPlaybackLatencyMsWindowsStoreApps = 170;

    public int MaxBufferingLatencyMsWindowsPhone8 = 200;
    public int MaxPlaybackLatencyMsWindowsPhone8 = 170;


    //////////////////////////////////////////////////////////////////////////    


    void SetDefaultLatencyValuesForPlatform()
    {
        int bufferLatency = 100;
        int playbackLatency = 50;

#if (UNITY_STANDALONE || UNITY_WSA)
            bufferLatency = MaxBufferingLatencyMsStandalone;
            playbackLatency = MaxPlaybackLatencyMsStandalone;
#elif UNITY_PS3
            bufferLatency = MaxBufferingLatencyMsPS3;
            playbackLatency = MaxPlaybackLatencyMsPS3;
#elif UNITY_PS4
            bufferLatency = MaxBufferingLatencyMsPS4;
            playbackLatency = MaxPlaybackLatencyMsPS4;
#elif UNITY_XBOX360
            bufferLatency = MaxBufferingLatencyMsXBox360;
            playbackLatency = MaxPlaybackLatencyMsXBox360;
#elif UNITY_XBOXONE
            bufferLatency = MaxBufferingLatencyMsXBox360;
            playbackLatency = MaxPlaybackLatencyMsXBox360;            
#elif UNITY_WEBPLAYER
            bufferLatency = MaxBufferingLatencyMsWebplayer;
            playbackLatency = MaxPlaybackLatencyMsWebplayer;
#elif UNITY_IOS
            bufferLatency = MaxBufferingLatencyMsIOS;
            playbackLatency = MaxPlaybackLatencyMsIOS;
#elif UNITY_ANDROID
            bufferLatency = MaxBufferingLatencyMsAndroid;
            playbackLatency = MaxPlaybackLatencyMsAndroid;
#elif UNITY_WP8
            bufferLatency = MaxBufferingLatencyMsWindowsPhone8;
            playbackLatency = MaxPlaybackLatencyMsWindowsPhone8;
#endif

        PsaiCore.Instance.SetMaximumLatencyNeededByPlatformToBufferSounddata(bufferLatency);
        PsaiCore.Instance.SetMaximumLatencyNeededByPlatformToPlayBackBufferedSounddata(playbackLatency);      
    }



    void Awake()
    {
        if (PsaiCore.IsInstanceInitialized())
        {            
            PsaiCore.Instance.Release();
            PsaiCore.Instance = null;
        }

        PsaiCore.Instance.SetLogLevel(LogLevel);
    }

    void Start()
    {
        PsaiCore.Instance.SetVolume(1.0f);
        SetDefaultLatencyValuesForPlatform();
    }

    public void Update()
    {
        PsaiCore.Instance.Update();

        triggerTickIntervalCounter += Time.deltaTime;
        if (triggerTickIntervalCounter > tickIntervalInSeconds)
        {
            triggerTickIntervalCounter -= tickIntervalInSeconds;
            ProcessAllIncomingTriggerCallsForThisTick();
        }
    }


    public void OnApplicationPause(bool paused)
    {
        if (PsaiCore.IsInstanceInitialized())
        {
            PsaiCore.Instance.SetPaused(paused);
        }        
    }

    public void OnApplicationExit()
    {
        //PsaiCore.Instance.Release();
    }
    

    public bool RegisterContinuousTrigger(PsaiSynchronizedTrigger continuousTrigger)
    {
        if (!continuousTriggersInScene.Contains(continuousTrigger))
        {
            continuousTriggersInScene.Add(continuousTrigger);

            //Debug.Log("ContinuousTrigger registered successfully: " + continuousTrigger.ToString());
            return true;
        }
        return false;
    }

    public bool UnregisterContinuousTrigger(PsaiSynchronizedTrigger continuousTrigger)
    {
        if (continuousTriggersInScene.Contains(continuousTrigger))
        {
            continuousTriggersInScene.Remove(continuousTrigger);
            //Debug.Log("ContinuousTrigger unregistered successfully: " + continuousTrigger.ToString());
            return true;
        }
        return false;
    }

    public void RegisterOneShotTriggerCall(TriggerCall triggerCall)
    {
        _synchronizedOneShotTriggerCallsForNextTick.Add(triggerCall);
    }
       

    void ProcessAllIncomingTriggerCallsForThisTick()
    {
        mapThemeIdsToTriggerCalls.Clear();
        foreach (PsaiSynchronizedTrigger triggerBehaviour in continuousTriggersInScene)
        {
            if (triggerBehaviour.EvaluateAllTriggerConditions() == true)
            {
                float calculatedIntensity = triggerBehaviour.CalculateTriggerIntensity();
                TriggerCall triggerCall = new TriggerCall(triggerBehaviour, triggerBehaviour.themeId, calculatedIntensity, triggerBehaviour.musicDurationInSeconds);
                ProcessTriggerCall(triggerCall);
            }
        }

        // Now we check for synchronized One-Shot Triggers
        foreach (TriggerCall triggerCall in _synchronizedOneShotTriggerCallsForNextTick)
        {
            if (triggerCall.intensity > 0)
            {
                ProcessTriggerCall(triggerCall);
            }
        }
        _synchronizedOneShotTriggerCallsForNextTick.Clear();



        // Execute explicit calls to GoToRest, StopMusic,...
        if (playbackControlCallFiringInThisTick.controlType != PlaybackControlType.None)
        {
            bool executeControlCall = true;

            if (mapThemeIdsToTriggerCalls.Keys.Count > 0 && playbackControlCallFiringInThisTick.dontExecuteIfOtherTriggersAreFiring == true)
            {
                foreach(int themeId in mapThemeIdsToTriggerCalls.Keys)
                {
                    ThemeInfo themeInfo = PsaiCore.Instance.GetThemeInfo(themeId);
                    if (themeInfo.type == playbackControlCallFiringInThisTick.restrictBlockToThisThemeType)
                    {     
                        #if !(PSAI_NOLOG)
                        if (this.logTriggerScripts)
                        {
                            Debug.LogWarning("skipping " + playbackControlCallFiringInThisTick.controlType + " as there are other Triggers firing:" + themeInfo.type);
                        }
                        #endif

                        executeControlCall = false;
                        break;
                    }
                }
            }

            if (executeControlCall)
            {
                switch (playbackControlCallFiringInThisTick.controlType)
                {
                    case PlaybackControlType.StopMusic:
                        {
                            PsaiCore.Instance.StopMusic(playbackControlCallFiringInThisTick.immediately);
                        }
                        break;

                    case PlaybackControlType.GoToRest:
                        {
                            PsaiCore.Instance.GoToRest(playbackControlCallFiringInThisTick.immediately, 
                                                        playbackControlCallFiringInThisTick.fadeOutSeconds,
                                                        playbackControlCallFiringInThisTick.overrideRestSecondsMin * 1000,
                                                        playbackControlCallFiringInThisTick.overrideRestSecondsMax * 1000);
                        }
                        break;

                    case PlaybackControlType.ReturnToLastBasicMood:
                        {
                            PsaiCore.Instance.ReturnToLastBasicMood(playbackControlCallFiringInThisTick.immediately);
                        }
                        break;
                }
            }         
            playbackControlCallFiringInThisTick.Init();
        }

        foreach (TriggerCall triggerCall in mapThemeIdsToTriggerCalls.Values)
        {
            // adding up triggers may exceed the limit                
            float triggerIntensity = Mathf.Min(1.0f, triggerCall.intensity);

            if (logTriggerScripts)
            {
                Debug.Log("[" + Time.timeSinceLevelLoad + " ]PsaiCoreManager executing TriggerCall " + triggerCall.ToString());
            }

            if (triggerCall.forceImmediateStopBeforeTriggering)
            {
                PsaiCore.Instance.StopMusic(true);
            }
            if (triggerCall.senderBehaviour.overrideMusicDurationInSeconds && triggerCall.musicDurationInSeconds > 0)
            {
                PsaiCore.Instance.TriggerMusicTheme(triggerCall.themeId, triggerIntensity, triggerCall.musicDurationInSeconds);
            }
            else
            {
                PsaiCore.Instance.TriggerMusicTheme(triggerCall.themeId, triggerIntensity);
            }
        }
    }


    /// <summary>
    /// Checks if a TriggerCall for a given Theme has already been stored in the map for this tick, and updates the map accordingly.
    /// </summary>
    /// <param name="triggerCall"></param>
    private void ProcessTriggerCall(TriggerCall triggerCall)
    {
        bool storeNewTriggerCall = false;        
        if (mapThemeIdsToTriggerCalls.ContainsKey(triggerCall.themeId))
        {
            TriggerCall storedTriggerCall = mapThemeIdsToTriggerCalls[triggerCall.themeId];
            if (triggerCall.senderBehaviour.addUpIntensities)
            {
                triggerCall.intensity += storedTriggerCall.intensity;

                if (triggerCall.intensity > triggerCall.senderBehaviour.limitIntensitySum)
                {
                    triggerCall.intensity = triggerCall.senderBehaviour.limitIntensitySum;
                }
                storeNewTriggerCall = true;
            }
            else
            {
                if (storedTriggerCall.intensity < triggerCall.intensity)
                {
                    storeNewTriggerCall = true;
                }
            }
        }
        else
        {
            storeNewTriggerCall = true;
        }

        if (storeNewTriggerCall)
        {
            mapThemeIdsToTriggerCalls[triggerCall.themeId] = triggerCall;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////
    // for some reason the sender Behaviour is null in the next Update loop after calling these Synchronized...methods, 
    // so we fill a struct.

    public void SynchronizedReturnToLastBasicMood(PsaiReturnToLastBasicMood sender)
    {
        this.playbackControlCallFiringInThisTick.controlType = PlaybackControlType.ReturnToLastBasicMood;
        this.playbackControlCallFiringInThisTick.immediately = sender.immediately;
        this.playbackControlCallFiringInThisTick.fadeOutSeconds = sender.fadeoutSeconds;
        this.playbackControlCallFiringInThisTick.dontExecuteIfOtherTriggersAreFiring = sender.dontExecuteIfTriggersAreFiring;
        this.playbackControlCallFiringInThisTick.restrictBlockToThisThemeType = sender.restrictBlockToThisThemeType;            
    }

    public void SynchronizedGoToRest(PsaiStopMusic sender)
    {
        this.playbackControlCallFiringInThisTick.controlType = PlaybackControlType.GoToRest;
        this.playbackControlCallFiringInThisTick.immediately = sender.immediately;
        this.playbackControlCallFiringInThisTick.fadeOutSeconds = sender.fadeoutSeconds;
        this.playbackControlCallFiringInThisTick.dontExecuteIfOtherTriggersAreFiring = sender.dontExecuteIfTriggersAreFiring;
        this.playbackControlCallFiringInThisTick.restrictBlockToThisThemeType = sender.restrictBlockToThisThemeType;
        if (sender.overrideDefaultRestTime)
        {
            this.playbackControlCallFiringInThisTick.overrideRestSecondsMin = sender.restTimeOverrideSecondsMin;
            this.playbackControlCallFiringInThisTick.overrideRestSecondsMax = sender.restTimeOverrideSecondsMax;
        }
        else
        {
            this.playbackControlCallFiringInThisTick.overrideRestSecondsMin = -1;
            this.playbackControlCallFiringInThisTick.overrideRestSecondsMin = -1;
        }
    }

    public void SynchronizedStopMusic(PsaiStopMusic sender)
    {
        this.playbackControlCallFiringInThisTick.controlType = PlaybackControlType.StopMusic;
        this.playbackControlCallFiringInThisTick.immediately = sender.immediately;
        this.playbackControlCallFiringInThisTick.fadeOutSeconds = sender.fadeoutSeconds;
        this.playbackControlCallFiringInThisTick.dontExecuteIfOtherTriggersAreFiring = sender.dontExecuteIfTriggersAreFiring;
        this.playbackControlCallFiringInThisTick.restrictBlockToThisThemeType = sender.restrictBlockToThisThemeType;
    }

}
