//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using System.Diagnostics;


namespace psai.net
{
    /// <summary>
    /// Used to control the verbosity of the debug information that will be written to the output console and log file.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// switch off all log information
        /// </summary>
        off = 0,

        /// <summary>
        /// only log errors
        /// </summary>
        errors,

        /// <summary>
        /// log errors and warnings
        /// </summary>
        warnings,
        
        /// <summary>
        /// logs errors, warning, and general information about calls to psai's api
        /// </summary>
        info,
        
        /// <summary>
        /// logs everything, including internal debug information
        /// </summary>
        debug
    };

    public interface LoggerOutput
    {
        void WriteLog(string argMessage, LogLevel logLevel);
    }

    internal class LoggerOutputWindows : LoggerOutput
    {
        internal static readonly string PSAI_FILENAME_LOGFILE = "psai.log";

        internal LoggerOutputWindows()
        {
            string pathToAppDataDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string pathToAppDataPsaiSubdir = Path.Combine(pathToAppDataDir, "psai");
            if (!File.Exists(pathToAppDataPsaiSubdir))
            {
                System.IO.Directory.CreateDirectory(pathToAppDataPsaiSubdir);
            }

            // Create a file for output named TestFile.txt.
            Stream myFile = File.Create(Path.Combine(pathToAppDataPsaiSubdir, PSAI_FILENAME_LOGFILE));

            /* Create a new text writer using the output stream, and add it to
             * the trace listeners. */
            TextWriterTraceListener myTextListener = new TextWriterTraceListener(myFile);
            Trace.Listeners.Add(myTextListener);

            /*
            string fullPathToLogFile = Path.Combine(pathToAppDataPsaiSubdir, PSAI_FILENAME_LOGFILE);
            m_streamWriter = new System.IO.StreamWriter(fullPathToLogFile);
            m_streamWriter.AutoFlush = true;
             */

        }


        public void WriteLog(string argMessage, LogLevel logLevel)
        {

            // Write output to the file.
            Trace.WriteLine(argMessage);

            // Flush the output.
            Trace.Flush(); 
        }
    }

    internal class Logger
    {
        private static Logger s_instance;
        internal static Logger Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new Logger();

                }
                return s_instance;
            }

            set
            {
                s_instance = value;
            }
        }

        private StringBuilder m_lastErrorStringBuilder;
        private bool m_lastErrorNewMessageAvailable;

        private List<LoggerOutput> m_loggerOutputs = new List<LoggerOutput>();

        internal LogLevel LogLevel
        {
            get;
            set;
        }


        internal List<LoggerOutput> LoggerOutputs
        {
            get
            {

                if (m_loggerOutputs.Count == 0)
                {
                    #if PSAI_STANDALONE
                        m_loggerOutputs.Add(new LoggerOutputWindows());                    
                    #else
                        m_loggerOutputs.Add(new LoggerOutputUnity());                    
                    #endif
                }
                return m_loggerOutputs;
            }

            set
            {
                m_loggerOutputs = value;
            }
        }

        internal void Log(string argText, LogLevel logLevel)
        {
            if (logLevel <= LogLevel)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("[");
                sb.Append(Logik.GetTimestampMillisElapsedSinceInitialisation().ToString());
                sb.Append("]");

                switch (logLevel)
                {
                    case LogLevel.info:
                        sb.Append("[INFO]: ");
                        break;

                    case LogLevel.warnings:
                        sb.Append("[WARNING]: ");
                        break;

                    case LogLevel.errors:
                        sb.Append("[ERROR]:");
                        break;

                    case LogLevel.debug:
                        sb.Append("     [INTERNAL]: ");
                        break;
                }
                sb.Append(argText);


                WriteToLoggerOutputs(sb.ToString(), logLevel);


                if (logLevel == LogLevel.warnings || logLevel == LogLevel.errors)
                {
                    if (m_lastErrorStringBuilder == null || m_lastErrorNewMessageAvailable == false)
                    {
                        m_lastErrorStringBuilder = new StringBuilder();
                    }

                    m_lastErrorStringBuilder.AppendLine(argText);
                    m_lastErrorNewMessageAvailable = true;
                }
            }
        }

        internal void WriteToLoggerOutputs(string message, LogLevel logLevel)
        {
            foreach (LoggerOutput logOutput in LoggerOutputs)
            {
                logOutput.WriteLog(message, logLevel);
            }
        }

        internal string GetLastError()
        {
            if (m_lastErrorNewMessageAvailable)
            {
                m_lastErrorNewMessageAvailable = false;
                return m_lastErrorStringBuilder.ToString();
            }
            else
            {
                return null;
            }
        }
    }


}
