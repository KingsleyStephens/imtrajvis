using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KCore
{
    /**
     * Class to handle logging
     */
    public static class Logger
    {
        // PUBLIC

        // Types of logging
        public enum LogType
        {
            None,
            Message,
            Warning,
            Error,
            Telemetry,
            All
        }

        // PRIVATE

        private static uint LogLevel { get { return (sLogLevel == (uint)LogType.All) ? sHighestLevel : sLogLevel; } }

        private static Dictionary<string, uint> sCustomLoglevels = new Dictionary<string, uint>();
        private static uint sHighestLevel = (uint)LogType.All;
        private static uint sLogLevel = (uint)LogType.All;

        private static FileLogger sEventLog;
        private static FileLogger sTelemtryLog;

        // PUBLIC

        public static void OpenEventLog(string filePath)
        {
            sEventLog = new FileLogger();
            sEventLog.Open(filePath);
        }

        public static void CloseEventLog()
        {
            if (sEventLog != null)
            {
                sEventLog.Close();
            }
        }

        public static void OpenTelemetryLog(string filePath)
        {
            sTelemtryLog = new FileLogger();
            sTelemtryLog.Open(filePath);
        }

        public static void CloseTelemetryLog()
        {
            if (sTelemtryLog != null)
            {
                sTelemtryLog.Close();
            }
        }

        /**
         * Set log level from type
         * 
         * @param level The level from type
         */
        public static void SetLogLevel(LogType level)
        {
            sLogLevel = (uint)level;
        }

        /**
         * Add a custom log level
         * 
         * @param name The name of the level. Use this in the logger call.
         * @param level The level of the custom log
         */
        public static void AddCustomLogLevel(string name, uint level)
        {
            sCustomLoglevels[name] = level;

            if (level > sHighestLevel)
            {
                sHighestLevel = level;
            }
        }

        /**
         * Set log level from custom name
         * 
         * @param levelName The level custom name
         */
        public static void SetCustomLogLevel(string levelName)
        {
            uint level;
            if (sCustomLoglevels.TryGetValue(levelName, out level))
            {
                sLogLevel = level;
            }
        }

        public static void LogTelemetryHeader(string message, params object[] args)
        {
            DebugLog((uint)LogType.Telemetry, message, args);
        }

        public static void LogTelemetry(string time, float simTime, string id, Vector3 position, Vector3 rotation)
        {
            string message = "{0},{1},{2},{3},{4},{5},{6},{7},{8}";
            object[] args = new object[] { time, simTime, id, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z };

            DebugLog((uint)LogType.Telemetry, message, args);
        }

        /**
         * Write message helper
         */
        public static void LogMessage(string message, params object[] args)
        {
            DebugLog((uint)LogType.Message, message, args);
        }

        /**
         * Write warning helper
         */
        public static void LogWarning(string message, params object[] args)
        {
            DebugLog((uint)LogType.Warning, message, args);
        }

        /**
         * Write warning helper
        */
        public static void LogError(string message, params object[] args)
        {
            DebugLog((uint)LogType.Error, message, args);
        }

        /**
         * Write console log
         */
        public static void DebugLog(uint logType, string message, params object[] args)
        {
            if (logType != (uint)LogType.None && logType <= LogLevel)
            {
                switch (logType)
                {
                    case (uint)LogType.Warning:
                        {
                            Debug.LogWarningFormat(message, args);
                            if (sEventLog != null)
                            {
                                sEventLog.WriteWarning(String.Format(message, args));
                            }
                            break;
                        }
                    case (uint)LogType.Error:
                        {
                            Debug.LogErrorFormat(message, args);
                            if (sEventLog != null)
                            {
                                sEventLog.WriteError(String.Format(message, args));
                            }
                            break;
                        }
                    case (uint)LogType.Telemetry:
                        {
                            Debug.LogFormat(message, args);
                            if (sTelemtryLog != null)
                            {
                                sTelemtryLog.WriteRaw(String.Format(message, args));
                            }
                            break;
                        }
                    default:
                        {
                            Debug.LogFormat(message, args);
                            if (sEventLog != null)
                            {
                                sEventLog.Write(String.Format(message, args));
                            }
                            break;
                        }
                }
            }
        }
    }
}