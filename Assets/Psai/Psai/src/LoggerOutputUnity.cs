//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;

namespace psai.net
{
    internal class LoggerOutputUnity : LoggerOutput
    {
        internal LoggerOutputUnity()
        {

        }

        ~LoggerOutputUnity()
        {

        }

        public void WriteLog(string argMessage, LogLevel logLevel)
        {
            string fullText = "PSAI " + argMessage;

            if (logLevel == LogLevel.errors)
            {
                UnityEngine.Debug.LogError(fullText);
            }
            else if (logLevel == LogLevel.warnings)
            {
                UnityEngine.Debug.LogWarning(fullText);
            }
            else
            {
                UnityEngine.Debug.Log(fullText);
            }            
        }
    }
}
