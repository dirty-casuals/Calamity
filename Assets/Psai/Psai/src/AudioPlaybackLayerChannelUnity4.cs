//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if (false)

#if (!PSAI_STANDALONE)

//#undef PSAI_NOLOG
//#define UNITY_4_3


using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using System.Collections;

namespace psai.net
{

    public class PsaiAsyncLoader : MonoBehaviour
    {

#if UNITY_PRO_LICENSE

        public void LoadSegmentAsync(AudioPlaybackLayerChannelUnity audioPlaybackChannel)
        {
            #if (!PSAI_NOLOG)
            if (LogLevel.debug <= Logger.Instance.LogLevel)
            {
                Logger.Instance.Log("LoadSegmentAsync() pathToClip=" + audioPlaybackChannel.PathToClip+ "  tryToUseWrappers=" + AudioPlaybackLayerChannelUnity._tryToUseWrappers + "   audioPlaybackChannel.GetHashCode()=" + audioPlaybackChannel.GetHashCode(), LogLevel.debug);
            }
            #endif
            StartCoroutine("LoadSegmentAsync_Coroutine", audioPlaybackChannel);
        }

        private IEnumerator LoadSegmentAsync_Coroutine(AudioPlaybackLayerChannelUnity audioPlaybackChannel)
        {           
            if (AudioPlaybackLayerChannelUnity._tryToUseWrappers)
            {
                ResourceRequest request = Resources.LoadAsync(audioPlaybackChannel.PathToClipWrapper, typeof(GameObject));
                yield return request;
                
#if (!PSAI_NOLOG)
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                        string logMessage = "LoadSegmentAsync_Coroutine [Wrappers] complete, asset=" + request.asset.name;
                        logMessage += "  ImmediatePlaybackIsPending=" + audioPlaybackChannel.PlaybackIsPending;
                        Logger.Instance.Log(logMessage, LogLevel.debug);
                    }
#endif

                GameObject wrapperGameObject = request.asset as GameObject;

                if (wrapperGameObject == null)
                {
#if (!PSAI_NOLOG)
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log("The Wrapper prefab '" + audioPlaybackChannel.PathToClipWrapper + "' was not found! Please run the psaiMultiAudioObjectEditor on your soundtrack folder, and make sure 'create Wrappers' is enabled.", LogLevel.errors);
                    }
#endif
                }
                else
                {
                    PsaiAudioClipWrapper wrapperComponent = wrapperGameObject.GetComponent<PsaiAudioClipWrapper>();
                    if (wrapperComponent == null || wrapperComponent._audioClip == null)
                    {
                        Debug.LogError("a Wrapper prefab for AudioClip '" + audioPlaybackChannel.PathToClip + "' was found, but it was invalid. Please re-run the psaiMultiAudioObjectEditor on your soundtrack folder, and make sure 'create Wrappers' is enabled.");
                    }
                    else
                    {
                        audioPlaybackChannel.AudioClip = wrapperComponent._audioClip;
                    }
                }
            }
            else
            {
                ResourceRequest request = Resources.LoadAsync(audioPlaybackChannel.PathToClip, typeof(AudioClip));
                yield return request;

#if (!PSAI_NOLOG)
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {

                    string logMessage = "LoadSegmentAsync_Coroutine [No Wrappers] complete, asset=" + request.asset.name;
                    logMessage += "  ImmediatePlaybackIsPending=" + audioPlaybackChannel.PlaybackIsPending + "   audioPlaybackChannel.GetHashCode()" + audioPlaybackChannel.GetHashCode();
                    Logger.Instance.Log(logMessage, LogLevel.debug);
                }
#endif

                AudioClip clip = request.asset as AudioClip;

                if (clip == null)
                {
#if (!PSAI_NOLOG)
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log("The AudioClip '" + audioPlaybackChannel.PathToClip + "' was not found!", LogLevel.errors);
                    }
#endif
                }
                else
                {
                    audioPlaybackChannel.AudioClip = clip;
                }
            }

            if (audioPlaybackChannel.PlaybackIsPending)
            {
                while (!audioPlaybackChannel.IsReadyToPlay())
                {
                    yield return null;

#if (!PSAI_NOLOG)
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log("ImmediatePlayback is pending but AudioClip is not ready to play!", LogLevel.warnings);
                    }
#endif
                }

                audioPlaybackChannel.PlayBufferedClipImmediately();
            }
        }
#endif
    }



    public class AudioPlaybackLayerChannelUnity : IAudioPlaybackLayerChannel
    {

#if (UNITY_4_3 || UNITY_4_4 ||UNITY_4_5 || UNITY_4_6)
        public static bool _tryToUseWrappers = true;
#else
        public static bool _tryToUseWrappers = false;
#endif

#if (!PSAI_NOLOG && !UNITY_PRO_LICENSE)        //TODO: implement warning for UNITY_PRO
        private static bool _psaiWrapperWarningHasBeenShown = false;
#endif

        private AudioSource _audioSource = null;
        private Segment _segmentToLoad;
        private int _timeSamples;
        private bool _playbackHasBeenInterruptedByPause;
        private PsaiAsyncLoader _psaiAsyncLoader;

        public bool PlaybackIsPending
        {
            get;
            set;
        }


        public AudioClip AudioClip
        {
            get
            {
                if (_audioSource != null)
                {
                    return _audioSource.clip;
                }
                return null;
            }

            set
            {
                if (_audioSource != null)
                {
                    _audioSource.clip = value;
                }                
            }
        }


        public string PathToClip
        {
            get;
            set;
        }

        public string PathToClipWrapper
        {
            get { return PathToClip + "_go"; }
        }

        public AudioPlaybackLayerChannelUnity()
        {
            AudioSource source = PlatformLayerUnity.PsaiGameObject.transform.FindChild(PlatformLayerUnity.NAME_OF_CHANNELS_CHILDNODE).gameObject.AddComponent<AudioSource>();
            source.loop = false;
            source.ignoreListenerVolume = true;
            _audioSource = source;
        }


        public void Release()
        {
            if (_audioSource != null)
            {
                GameObject.DestroyImmediate(_audioSource);
            }
        }

        PsaiResult IAudioPlaybackLayerChannel.LoadSegment(Segment segment)
        {
            _segmentToLoad = segment;
            AudioClip = null;

#if UNITY_PRO_LICENSE
            if (_psaiAsyncLoader == null)
            {
                GameObject psaiObject = PsaiCoreManager.Instance.gameObject;

                if (psaiObject == null)
                {
                    #if !(PSAI_NOLOG)
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
                            Logger.Instance.Log("No 'Psai' object found in the Scene! Please make sure to add the Psai.prefab from the Psai.unitypackage to your Scene", LogLevel.errors);
                    }
                    #endif
                    return PsaiResult.initialization_error;
                }
                _psaiAsyncLoader = psaiObject.AddComponent<PsaiAsyncLoader>();
            }
#endif


            // careful! Using Path.Combine for the subfolders does not work for the Resources subfolders,
            // neither does "\\" double backslashes. So leave it like this, it works for WebPlayer and Standalone.
            // not checked yet for iOS and Android. If in doubt, leave out the subfolders.

            //string pathToClip = null;
            string psaiBinaryDirectoryName = Logik.Instance.m_psaiCoreBinaryDirectoryName;
            if (psaiBinaryDirectoryName.Length > 0)
            {
                PathToClip = psaiBinaryDirectoryName + "/" + segment.audioData.filePathRelativeToProjectDir;
            }
            else
            {
                PathToClip = segment.audioData.filePathRelativeToProjectDir;
            }


#if UNITY_PRO_LICENSE
            {
                _audioSource.clip = null;       // we reset the clip to prevent the situation in ScheduleSegmentPlayback(), where the previous clip was reported as readyToPlay, causing problems.
                _psaiAsyncLoader.LoadSegmentAsync(this);               
            }
            return PsaiResult.OK;
#else


            if (_tryToUseWrappers)
            {
                GameObject wrapperGameObject = (GameObject)UnityEngine.Resources.Load(PathToClipWrapper, typeof(GameObject));
                if (wrapperGameObject != null)
                {
                    PsaiAudioClipWrapper wrapperComponent = wrapperGameObject.GetComponent<PsaiAudioClipWrapper>();
                    if (wrapperComponent == null || wrapperComponent._audioClip == null)
                    {
                        Debug.LogError("a Wrapper prefab for AudioClip '" + _segmentToLoad.audioData.filePathRelativeToProjectDir + "' was found, but it was invalid. Please re-run the psaiMultiAudioObjectEditor on your soundtrack folder, and make sure 'create Wrappers' is enabled.");
                    }
                    else
                    {
                        AudioClip = wrapperComponent._audioClip;

                        #if (!PSAI_NOLOG)
                        if (LogLevel.debug <= Logger.Instance.LogLevel)
                        {
                            Logger.Instance.Log("success: audioClip loaded from Wrapper (synchronous)", LogLevel.debug);
                        }
                        #endif
                    }
                }
            }
            
            // Fallback: Load Clip directly. 
            if (AudioClip == null)
            {

                AudioClip = UnityEngine.Resources.Load(PathToClip) as AudioClip;

#if (!PSAI_NOLOG)

                if (_tryToUseWrappers && !_psaiWrapperWarningHasBeenShown && LogLevel.warnings <= Logger.Instance.LogLevel)
                {
                    Logger.Instance.Log("Due to an issue in Unity 4.x AudioClips will in some cases not be streamed from disk, even if their import settings are set accordingly. This may cause framerate drops whenever a new Segment is loaded. Psai provides a workaround for this, by wrapping each AudioClip in a dedicated GameObject. To have the Wrappers created, please right-click on your soundtrack folder in the Project window, and run the 'psai Multi Audio Object Editor'. Make sure 'use Wrappers' is enabled.", LogLevel.warnings);
                    _psaiWrapperWarningHasBeenShown = true;
                }
#endif             
            }


            /* final check to return PsaiResult and write Log */
            if (AudioClip == null)
            {
#if (!PSAI_NOLOG)
                if (LogLevel.errors <= Logger.Instance.LogLevel)
                {
                    Logger.Instance.Log("Segment not found: " + PathToClipWrapper, LogLevel.errors);
                }
#endif
                return PsaiResult.file_notFound;
            }
            else
            {

#if (!PSAI_NOLOG)
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
                    Logger.Instance.Log("LoadSegment() OK - segment.Name:" + segment.Name  + " _audioSource.clip: " + _audioSource.clip +  " PathToClip:"  + PathToClip, LogLevel.debug);
                }
#endif
                return PsaiResult.OK;
            }
#endif
        }

        PsaiResult IAudioPlaybackLayerChannel.ReleaseSegment()
        {
            if (_audioSource.clip != null)
            {
                /* this only has an effect after calling Resources.UnloadUnusedAssets().
                 * Only calling Resources.UnloadUnusedAssets() is also possible, but it will free the chached clips
                 * more seldomly */
                Resources.UnloadAsset(_audioSource.clip);

                _audioSource.clip = null;
                _segmentToLoad = null;
            }

            return PsaiResult.OK;
        }


        public bool IsReadyToPlay()
        {
            bool readyToPlay = false;
            if (_audioSource.clip != null)
            {

#if (UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 ||UNITY_3_3 || UNTIY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6)
					readyToPlay = _audioSource.clip.isReadyToPlay;
#else
                readyToPlay = (_audioSource.clip.loadState == AudioDataLoadState.Loaded);
#endif
            }

            return readyToPlay;
        }


        public void PlayBufferedClipImmediately()
        {
#if (!PSAI_NOLOG)
            if (LogLevel.debug <= Logger.Instance.LogLevel)
            {
                string logMessage = "PlayBufferedClipImmediately()  _audioSource._clip=" + _audioSource.clip;
                logMessage += " IsReadToPlay=" + IsReadyToPlay().ToString();
                Logger.Instance.Log(logMessage, LogLevel.debug);
            }
#endif
            _audioSource.Play();
            PlaybackIsPending = false;
        }

        PsaiResult IAudioPlaybackLayerChannel.ScheduleSegmentPlayback(Segment snippet, int delayMilliseconds)
        {
            if (_segmentToLoad != null && _segmentToLoad.Id == snippet.Id)
            {			
				bool readyToPlay = IsReadyToPlay();

#if (!PSAI_NOLOG)
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
                    Logger.Instance.Log("ScheduleSegmentPlayback() Segment:" + snippet.Name + "  isReadyToPlay:" + readyToPlay, LogLevel.debug);
                }
#endif
																
                // new method PlayDelayed introduced in Unity Version 4.1.0.                    
	            if (readyToPlay)
	            {
	                _audioSource.PlayDelayed((uint)delayMilliseconds / 1000.0f);
	                PlaybackIsPending = false;

#if (!PSAI_NOLOG)
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log("_audioSource.PlayDelayed() fired, delayInMs=" + delayMilliseconds, LogLevel.debug);
                    }
#endif
	            }
	            else
	            {

#if (!PSAI_NOLOG)
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log("... play has not fired yet, ImmediatePlaybackIsPending is now set to true.   this.GetHashCode()=" + this.GetHashCode(), LogLevel.debug);
                    }
#endif

	                PlaybackIsPending = true;
	            }                


                return PsaiResult.OK;
            }
            else
            {
                Logger.Instance.Log("ScheduleSegmentPlayback(): COULD NOT PLAY! No Segment loaded, or Segment Id to play did not match! Segment loaded: " + _segmentToLoad, LogLevel.errors);
            }

            return PsaiResult.notReady;
        }

        PsaiResult IAudioPlaybackLayerChannel.StopChannel()
        {
            _audioSource.volume = 0;
            _audioSource.Stop();

            return PsaiResult.OK;
        }

        PsaiResult IAudioPlaybackLayerChannel.SetVolume(float volume)
        {

            if (_audioSource != null)
            {
                _audioSource.volume = volume;
                return PsaiResult.OK;
            }
            else
            {
#if (!PSAI_NOLOG)
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("SetVolume() failed, _audioSource is NULL!", LogLevel.errors);
                    }
#endif

                return PsaiResult.notReady;
            }
        }

        PsaiResult IAudioPlaybackLayerChannel.SetPaused(bool paused)
        {
            if (paused)
            {
                if (_audioSource.isPlaying)
                {
                    _playbackHasBeenInterruptedByPause = true;
                    _audioSource.Pause();
                    _timeSamples = _audioSource.timeSamples;
                }
            }
            else
            {
                if (_playbackHasBeenInterruptedByPause)
                {
                    _audioSource.Play();
                    _audioSource.time = _timeSamples;

                    _playbackHasBeenInterruptedByPause = false;
                }
            }

            return PsaiResult.OK;
        }
    }
}

#endif

#endif