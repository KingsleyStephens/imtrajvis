using System;
using System.Collections.Generic;
using UnityEngine;
using KingsleyStephens.CoreInternal;

namespace KingsleyStephens
{
    namespace Core
    {
        /**
         * Class to handle logging to console and to an optional file
         */
        public class Logger
        {
            // PUBLIC

            // Types of logging
            public enum LogType
            {
                None,
                Message,
                Warning,
                Error,
                All
            }

            // PRIVATE

            private uint LogLevel { get { return (mLogLevel == (uint)LogType.All) ? mHighestLevel : mLogLevel; } }

            private Dictionary<string, uint> mCustomLoglevels = new Dictionary<string, uint>();
            private uint mHighestLevel = (uint)LogType.All;
            private uint mLogLevel = (uint)LogType.All;

            private FileLogger mFileLog;
            private bool mMessageTimeStamps = false;

            // PUBLIC

            /**
             * Open a log file at the given file path
             * 
             * @param filePath The filpath to save the log file
             * @param messageTimeStamps Write message time stamps to file or not
             */
            public void Open(string filePath, bool messageTimeStamps = true)
            {
                mFileLog = new FileLogger();
                mFileLog.Open(filePath);
                mMessageTimeStamps = messageTimeStamps;
            }

            /**
             * Close the log file
             */
            public void Close()
            {
                if (mFileLog != null)
                {
                    mFileLog.Close();
                }
            }

            /**
             * Set log level from type
             * 
             * @param level The level from type
             */
            public void SetLogLevel(LogType level)
            {
                mLogLevel = (uint)level;
            }

            /**
             * Add a custom log level
             * 
             * @param name The name of the level. Use this in the logger call.
             * @param level The level of the custom log
             */
            public void AddCustomLogLevel(string name, uint level)
            {
                mCustomLoglevels[name] = level;

                if (level > mHighestLevel)
                {
                    mHighestLevel = level;
                }
            }

            /**
             * Set log level from custom name
             * 
             * @param levelName The level custom name
             */
            public void SetCustomLogLevel(string levelName)
            {
                uint level;
                if (mCustomLoglevels.TryGetValue(levelName, out level))
                {
                    mLogLevel = level;
                }
            }

            /**
             * Write message helper
             * 
             * @param message The message to write to the log
             * @param args Parameters for the message
             */
            public void LogMessage(string message, params object[] args)
            {
                DebugLog((uint)LogType.Message, message, args);
            }

            /**
             * Write warning helper
             * 
             * @param message The message to write to the log
             * @param args Parameters for the message
             */
            public void LogWarning(string message, params object[] args)
            {
                DebugLog((uint)LogType.Warning, message, args);
            }

            /**
             * Write warning helper
             *
             * @param message The message to write to the log
             * @param args Parameters for the message
            */
            public void LogError(string message, params object[] args)
            {
                DebugLog((uint)LogType.Error, message, args);
            }

            /**
             * Write console log
             * 
             * @param logType The type of log output
             * @param message The message to write to the log
             * @param args Parameters for the message
             */
            public void DebugLog(uint logType, string message, params object[] args)
            {
                if (logType != (uint)LogType.None && logType <= LogLevel)
                {
                    switch (logType)
                    {
                        case (uint)LogType.Warning:
                            {
                                Debug.LogWarningFormat(message, args);
                                if (mFileLog != null)
                                {
                                    mFileLog.WriteWarning(String.Format(message, args));
                                }
                                break;
                            }
                        case (uint)LogType.Error:
                            {
                                Debug.LogErrorFormat(message, args);
                                if (mFileLog != null)
                                {
                                    mFileLog.WriteError(String.Format(message, args));
                                }
                                break;
                            }
                        default:
                            {
                                Debug.LogFormat(message, args);
                                if (mFileLog != null)
                                {
                                    if (mMessageTimeStamps)
                                    {
                                        mFileLog.Write(String.Format(message, args));
                                    }
                                    else
                                    {
                                        mFileLog.WriteRaw(String.Format(message, args));
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }
    }
}