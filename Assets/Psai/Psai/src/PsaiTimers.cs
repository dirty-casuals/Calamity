//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;


namespace psai.net
{

    /*
    /* Unlike the PsaiTimers used in psai native, this PsaiTimer class does not fire callbacks at a precise point of time,
     * but is based on polling. 
    */

    internal class PsaiTimer
    {
        bool m_isSet;
        bool m_isPaused;
        int m_estimatedThresholdReachedTime;
        int m_estimatedFireTime;
        int m_timerPausedTimestamp;

        internal PsaiTimer()
        {
            m_isSet = false;
        }

        /** Sets the timer.
        /* @param delayMillis - the milliseconds from now, when the timer should report fired.
         * @param remainingThresholdMilliseconds - the milliseconds backwards from the firing time,
         *  when the method ThresholdHasBeenReached() shall reports true. Pass 0 if not needed.
        */
        internal void SetTimer(int delayMillis, int remainingThresholdMilliseconds)
        {           
            m_estimatedFireTime = Logik.GetTimestampMillisElapsedSinceInitialisation() + delayMillis;
            m_estimatedThresholdReachedTime = m_estimatedFireTime - remainingThresholdMilliseconds;

            m_isSet = true;

            #if !(PSAI_NOLOG)
            {
                /*
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("SetTimer() delay=");
	                sb.Append(delayMillis);
	                sb.Append("  estimatedFireTime=");
	                sb.Append(m_estimatedFireTime);
	                Logger.Instance.Log(sb.ToString(), psaiCoreDotNet.LogLevel.debug);
                }
                 */
            }
            #endif
        }


        internal bool IsSet()
        {
            return m_isSet;
        }

        internal void Stop()
        {
            m_isSet = false;
        }

        internal void SetPaused(bool setPaused)
        {
            if (m_isSet)
            {

                if (setPaused)
                {
                    if (!m_isPaused)
                    {
                        m_isPaused = true;
                        m_timerPausedTimestamp = Logik.GetTimestampMillisElapsedSinceInitialisation();
                    }
                }
                else
                {
                    if (m_isPaused)
                    {
                        m_isPaused = false;

                        int timePausedPeriod = Logik.GetTimestampMillisElapsedSinceInitialisation() - m_timerPausedTimestamp;
                        m_estimatedFireTime += timePausedPeriod;
                        m_estimatedThresholdReachedTime += timePausedPeriod;


#if !(PSAI_NOLOG)                 
                        if (LogLevel.debug <= Logger.Instance.LogLevel)
                        {
                                StringBuilder sb = new StringBuilder();
                                sb.Append("timePausedPeriod=");
                                sb.Append(timePausedPeriod);
                                sb.Append("   m_estimatedFireTime=");
                                sb.Append(m_estimatedFireTime);
                                sb.Append("   estimatedThresholdReachedTime=");
                                sb.Append(m_estimatedThresholdReachedTime);

                                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                        }
#endif
                    }
                }
            }
        }

        /* returns the milliseconds remaining until the timer fires,
         * and deactivates itself if the value <= 0.
         */
        internal int GetRemainingMillisToFireTime()
        {
            if (m_isSet && !m_isPaused)
            {
                return (m_estimatedFireTime - Logik.GetTimestampMillisElapsedSinceInitialisation());
            }
            else
            {
                return 999999;
            }
        }

        internal int GetEstimatedFireTime()
        {
            if (m_isSet && !m_isPaused)
            {
                return m_estimatedFireTime;
            }
            else
            {
                return 999999;
            }
        }

        // returns true if the timer is Set and the Threshold has been reached, false otherwise.
        internal bool ThresholdHasBeenReached()
        {
            if (m_isSet && !m_isPaused)
            {
                return (Logik.GetTimestampMillisElapsedSinceInitialisation() >= m_estimatedThresholdReachedTime);
            }
            else
            {
                return false;
            }            
        }
    }
}
