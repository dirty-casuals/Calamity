//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if (!PSAI_STANDALONE)


using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using System.Collections;

namespace psai.net
{

    public class PsaiAsyncLoader : MonoBehaviour
    {
        public void LoadSegmentAsync(AudioPlaybackLayerChannelUnity audioPlaybackChannel)
        {
            #if (!PSAI_NOLOG)
            if (LogLevel.debug <= Logger.Instance.LogLevel)
            {
                Logger.Instance.Log("LoadSegmentAsync() pathToClip=" + audioPlaybackChannel.PathToClip + "   audioPlaybackChannel.GetHashCode()=" + audioPlaybackChannel.GetHashCode(), LogLevel.debug);
            }
            #endif
            StartCoroutine("LoadSegmentAsync_Coroutine", audioPlaybackChannel);
        }

        private IEnumerator LoadSegmentAsync_Coroutine(AudioPlaybackLayerChannelUnity audioPlaybackChannel)
        {           

            ResourceRequest request = Resources.LoadAsync(audioPlaybackChannel.PathToClip, typeof(AudioClip));
            yield return request;

            #if (!PSAI_NOLOG)
            if (LogLevel.debug <= Logger.Instance.LogLevel)
            {
                string logMessage = "LoadSegmentAsync_Coroutine complete, asset=" + request.asset.name;
                logMessage += "  PlaybackIsPending=" + audioPlaybackChannel.PlaybackIsPending + "   audioPlaybackChannel.GetHashCode()" + audioPlaybackChannel.GetHashCode();
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
                audioPlaybackChannel.AudioClip.LoadAudioData();                
            }


            if (audioPlaybackChannel.PlaybackIsPending)
            {
                while (!audioPlaybackChannel.IsReadyToPlay())
                {
                    yield return null;

                    #if (!PSAI_NOLOG)
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log("Playback is pending but AudioClip is not ready to play!", LogLevel.debug);
                    }
                    #endif
                }

                int delayMillis = audioPlaybackChannel.TargetPlaybackTimestamp - Logik.GetTimestampMillisElapsedSinceInitialisation();

                if (delayMillis < 0 && Time.timeSinceLevelLoad > 1.0f)
                {
                    #if (!PSAI_NOLOG)
                    if (LogLevel.warnings <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log(string.Format("playback timing problem detected! Missing milliseconds: {0} ", delayMillis), LogLevel.warnings);
                    }

                    if (clip.loadType == AudioClipLoadType.Streaming)
                    {
                        Logger.Instance.Log("Please increase the 'Max Playback Latency' in PsaiCoreManager for the current target platform.", LogLevel.warnings);                       
                    }
                    else
                    {
                        Logger.Instance.Log("We highly recommend setting the 'Load Type' of all psai Audio Clips to 'Streaming'.", LogLevel.warnings);
                    }

                    #endif
                }

                audioPlaybackChannel.PlayBufferedClip(delayMillis);
            }
        }
    }



    public class AudioPlaybackLayerChannelUnity : IAudioPlaybackLayerChannel
    {
        private AudioSource _audioSource = null;
        private Segment _segmentToLoad;
        private int _timeSamples;
        private bool _playbackHasBeenInterruptedByPause;
        private PsaiAsyncLoader _psaiAsyncLoader;


        /** The Logik.MillisElapsedSinceInitialization() when the scheduled Segment should is supposed to fire
         */
        public int TargetPlaybackTimestamp
        { 
            get;
            set;
        }

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


        public AudioPlaybackLayerChannelUnity()
        {
            AudioSource source = PlatformLayerUnity.PsaiGameObject.transform.Find(PlatformLayerUnity.NAME_OF_CHANNELS_CHILDNODE).gameObject.AddComponent<AudioSource>();
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


#if (!PSAI_BUILT_BY_VS)
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

            _audioSource.clip = null;       // we reset the clip to prevent the situation in ScheduleSegmentPlayback(), where the previous clip was reported as readyToPlay, causing problems.
            _psaiAsyncLoader.LoadSegmentAsync(this);
            return PsaiResult.OK;

            /*
            // Fallback: Load Clip directly. 
            if (AudioClip == null)
            {
                AudioClip = UnityEngine.Resources.Load(PathToClip) as AudioClip;
            }

            if (AudioClip == null)
            {
                #if (!PSAI_NOLOG)
                if (LogLevel.errors <= Logger.Instance.LogLevel)
                {
                    Logger.Instance.Log("Segment not found: " + PathToClip, LogLevel.errors);
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
             */
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
                readyToPlay = (_audioSource.clip.loadState == AudioDataLoadState.Loaded);
            }

            return readyToPlay;
        }


        public void PlayBufferedClip(int delayMillis)
        {
            #if (!PSAI_NOLOG)
            if (LogLevel.debug <= Logger.Instance.LogLevel)
            {
                string logMessage = string.Format("PlayBufferedClip()  _audioSource._clip: {0}  delayMillis: {1}", _audioSource.clip, delayMillis);
                logMessage += " IsReadToPlay=" + IsReadyToPlay().ToString();
                Logger.Instance.Log(logMessage, LogLevel.debug);
            }
            #endif

            if (delayMillis > 0)
            {
                _audioSource.PlayDelayed((uint)delayMillis * 0.001f);
            }
            else
            {
                _audioSource.Play();
            }            
            
            PlaybackIsPending = false;
        }

        PsaiResult IAudioPlaybackLayerChannel.ScheduleSegmentPlayback(Segment segment, int delayMilliseconds)
        {
            if (_segmentToLoad != null && _segmentToLoad.Id == segment.Id)
            {			
				bool readyToPlay = IsReadyToPlay();

                #if (!PSAI_NOLOG)
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
                    Logger.Instance.Log(string.Format("ScheduleSegmentPlayback() Segment: {0}  isReadyToPlay: {1}", segment.Name, readyToPlay), LogLevel.debug);
                }
                #endif
																
                // new method PlayDelayed introduced in Unity Version 4.1.0.                    
	            if (readyToPlay)
	            {
                    PlayBufferedClip(delayMilliseconds);

                    #if (!PSAI_NOLOG)
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log(string.Format("_audioSource.PlayDelayed() fired, delayInMs:{0}", delayMilliseconds), LogLevel.debug);
                    }
                    #endif

                    return PsaiResult.OK;
	            }                
	            else
	            {
                    TargetPlaybackTimestamp = Logik.GetTimestampMillisElapsedSinceInitialisation() + delayMilliseconds;
                    PlaybackIsPending = true;

                    #if (!PSAI_NOLOG)
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log("... play has not fired yet, PlaybackIsPending is now set to true.  TargetPlaybackTimestamp=" + TargetPlaybackTimestamp, LogLevel.debug);
                    }
                    #endif
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