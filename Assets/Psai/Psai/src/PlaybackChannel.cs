//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;

namespace psai.net
{

    internal enum ChannelState
    {
        stopped,
        load,
        playing,
        paused
    }


    public class PlaybackChannel
    {

        private int m_timeStampOfPlaybackStart;      // timestamps set by Logik.GetTimeMillisElapsedSinceInitialisaion()
        private int m_timeStampOfSnippetLoad; 
        private bool m_playbackIsScheduled;        // is false when LoadSnippet() was called and the time of playback is not known yet
        private bool m_stoppedExplicitly;

        private IAudioPlaybackLayerChannel m_audioPlaybackLayerChannel;     // this may be NULL
        private float m_masterVolume = 1.0f;
        private float m_fadeOutVolume = 1.0f;

        private bool m_paused = false;

        internal Segment Segment
        {
            get;
            private set;
        }


        internal PlaybackChannel()
        {
            #if PSAI_STANDALONE
                m_audioPlaybackLayerChannel = new AudioPlaybackLayerChannelOpenTK();            
            #else
                m_audioPlaybackLayerChannel = new AudioPlaybackLayerChannelUnity();                
            #endif
        }

        internal void Release()
        {
            m_audioPlaybackLayerChannel.Release();
        }


        internal bool Paused
        {
            get
            {
                return m_paused;
            }

            set
            {
                m_paused = value;
                m_audioPlaybackLayerChannel.SetPaused(value);
            }
        }


        internal ChannelState GetCurrentChannelState()
        {
            if (Segment == null || m_stoppedExplicitly)
            {
                return ChannelState.stopped;
            }
            else
            {
                float countdownMillis = GetCountdownToPlaybackInMilliseconds();
                if (m_playbackIsScheduled == true && countdownMillis > 0)
                {
                    return ChannelState.load;
                }
                else
                {
                    if (countdownMillis * -1 > Segment.audioData.GetFullLengthInMilliseconds())
                    {
                        return ChannelState.stopped;
                    }
                    else
                    {
                        return ChannelState.playing;
                    }
                }
            }
        }


        internal bool IsPlaying()
        {
            return (GetCurrentChannelState() == ChannelState.playing);
        }


        internal void LoadSegment(Segment snippet)
        {
            Segment = snippet;
            m_timeStampOfSnippetLoad = Logik.GetTimestampMillisElapsedSinceInitialisation();
            m_playbackIsScheduled = false;
            m_stoppedExplicitly = false;

            if (m_audioPlaybackLayerChannel != null)
            {
                m_audioPlaybackLayerChannel.LoadSegment(snippet);
            }
        }


        internal bool CheckIfSegmentHadEnoughTimeToLoad()
        {
            return (GetMillisecondsSinceSegmentLoad() >= Logik.s_audioLayerMaximumLatencyForBufferingSounds);
        }


        internal int GetMillisecondsSinceSegmentLoad()
        {
            return Logik.GetTimestampMillisElapsedSinceInitialisation() - m_timeStampOfSnippetLoad;
        }

        internal int GetMillisecondsUntilLoadingWillHaveFinished()
        {
            return System.Math.Max(0, (Logik.s_audioLayerMaximumLatencyForBufferingSounds - GetMillisecondsSinceSegmentLoad()));
        }



        internal void StopChannel()
        {
            m_stoppedExplicitly = true;

            #if !(PSAI_NOLOG)
            {
                /*
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("StopChannel()");
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
                */
            }            
            #endif


            if (m_audioPlaybackLayerChannel != null)
            {
                m_audioPlaybackLayerChannel.StopChannel();
            }
        }


        internal void ReleaseSegment()
        {
            Segment = null;

            if (m_audioPlaybackLayerChannel != null)
            {
                m_audioPlaybackLayerChannel.ReleaseSegment();
            }
        }

        // returns the remaining milliseconds until playback start
        internal int GetCountdownToPlaybackInMilliseconds()
        {
            return m_timeStampOfPlaybackStart - Logik.GetTimestampMillisElapsedSinceInitialisation();
        }


        // this will play back the requested Snippet in delayInMilliseconds. 
        // Playback of a scheduled Snippet cannot be canceled.
        internal void ScheduleSegmentPlayback(Segment snippet, int delayInMilliseconds)
        {            
            #if !(PSAI_NOLOG)
            {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("ScheduleSegmentPlayback() Segment=");
	                sb.Append(snippet.Name);
	                sb.Append("  delayInMilliseconds=");
	                sb.Append(delayInMilliseconds);
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }             
            }
            #endif


            if (delayInMilliseconds < 0)
            {
                #if !(PSAI_NOLOG)
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("delayInMilliseconds was negative, thus set to 0" , LogLevel.debug);
                    }
                #endif
                delayInMilliseconds = 0;
            }

            if (snippet != Segment)
            {
                LoadSegment(snippet);
            }

            m_stoppedExplicitly = false;
            m_playbackIsScheduled = true;
            m_timeStampOfPlaybackStart = Logik.GetTimestampMillisElapsedSinceInitialisation() + delayInMilliseconds;


            if (m_audioPlaybackLayerChannel != null)
            {
                m_audioPlaybackLayerChannel.ScheduleSegmentPlayback(snippet, delayInMilliseconds);
            }
            else
            {
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log("m_audioPlaybackLayerChannel is null!", LogLevel.errors);
                    }
                }
                #endif
            }
        }


        // the cached m_psaiMasterVolume, between 0.0f and 1.0f
        internal float MasterVolume
        {
            get { return m_masterVolume; }
            set
            {
                if (value >= 0 && value <= 1.0f)
                {
                    m_masterVolume = value;
                    UpdateVolume();
                }
            }
        }

        // the relative volume of the channel, used for fades. Values between 0.0f and 1.0f
        internal float FadeOutVolume
        {
            get { return m_fadeOutVolume; }
            set
            {
                if (value >= 0 && value <= 1.0f)
                {
                    m_fadeOutVolume = value;
                    UpdateVolume();
                }
            }
        }

        private void UpdateVolume()
        {
            if (m_audioPlaybackLayerChannel != null)
            {
                float volume = MasterVolume * FadeOutVolume;

                /*
                #if !(PSAI_NOLOG)
                {                    
                    if (Logger.Instance.LogLevel == LogLevel.debug)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("UpdateVolume() volume=");
                        sb.Append(volume);
                        sb.Append("  Mastervolume=");
                        sb.Append(MasterVolume);
                        sb.Append("  ChannelVolume=");
                        sb.Append(ChannelVolume);
                        Logger.Instance.Log(sb.ToString() , LogLevel.debug);
                    }                 
                }
                #endif
                */

                m_audioPlaybackLayerChannel.SetVolume(volume);
            }
            else
            {
                #if !(PSAI_NOLOG)
                {
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
                    	Logger.Instance.Log("m_audioPlaybackLayerChannel is null!" , LogLevel.errors);
                    }
                }
                #endif
            }
        }
    }

}
