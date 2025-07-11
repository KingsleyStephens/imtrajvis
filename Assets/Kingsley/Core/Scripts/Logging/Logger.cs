using System.Collections;
using System.Collections.Generic;
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
            All
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
                            break;
                        }
                    case (uint)LogType.Error:
                        {
                            Debug.LogErrorFormat(message, args);
                            break;
                        }
                    default:
                        {
                            Debug.LogFormat(message, args);
                            break;
                        }
                }
            }
        }

        // PRIVATE
        private static uint LogLevel { get { return (sLogLevel == (uint)LogType.All) ? sHighestLevel : sLogLevel; } }

        private static Dictionary<string, uint> sCustomLoglevels = new Dictionary<string, uint>();
        private static uint sHighestLevel = (uint)LogType.All;
        private static uint sLogLevel = (uint)LogType.All;

    }
}