//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace psai.net
{

    // this is used by PlaybackChannel as an interface to directly inform some
    // AudioLayer to load and play back Snippets.
    internal interface IAudioPlaybackLayerChannel
    {         
        PsaiResult LoadSegment(Segment segment);
        PsaiResult ReleaseSegment();
        PsaiResult ScheduleSegmentPlayback(Segment segment, int delayMilliseconds);
        PsaiResult StopChannel();
        PsaiResult SetVolume(float volume);
        PsaiResult SetPaused(bool paused);

        /// <summary>
        /// Platform-specific cleanup when the application is closed.
        /// </summary>
        void Release();
    }
}
