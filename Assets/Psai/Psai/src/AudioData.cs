//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;

namespace psai.net
{
    public class AudioData
	{
        public string filePathRelativeToProjectDir;
        public int sampleCountTotal;
        public int sampleCountPreBeat;
        public int sampleCountPostBeat;
        public int sampleRateHz;				// in Hertz, not kHz
        public float bpm;


        public AudioData()
        {

        }
        
        public AudioData(psai.ProtoBuf_AudioData pbAudioData)
        {
            this.filePathRelativeToProjectDir = pbAudioData.filename;
            this.sampleRateHz = pbAudioData.sampleRate;
            this.sampleCountTotal = pbAudioData.sampleCountTotal;
            this.sampleCountPreBeat = pbAudioData.sampleCountPrebeat;
            this.sampleCountPostBeat = pbAudioData.sampleCountPostbeat;
            this.bpm = pbAudioData.bpm;
        }


        psai.ProtoBuf_AudioData CreateProtoBuf()
        {
            psai.ProtoBuf_AudioData pbAudio = new psai.ProtoBuf_AudioData();
            pbAudio.filename = this.filePathRelativeToProjectDir;
            pbAudio.sampleCountTotal = this.sampleCountTotal;
            pbAudio.sampleCountPrebeat = this.sampleCountPreBeat;
            pbAudio.sampleCountPostbeat = this.sampleCountPostBeat;
            pbAudio.sampleRate = this.sampleRateHz;
            return pbAudio;
        }

        public int GetFullLengthInMilliseconds()
        {
            return (int)((long)sampleCountTotal * 1000 / sampleRateHz);
        }

        public int GetPreBeatZoneInMilliseconds()
        {
            return (int)((long)sampleCountPreBeat * 1000 / sampleRateHz);
        }

        public int GetPostBeatZoneInMilliseconds()
        {
            return (int)((long)sampleCountPostBeat * 1000 / sampleRateHz);
        }
	};
}
