using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingsleyStephens
{
    namespace Core
    {
        /**
         * Public API for logging
         */
        public static class Log
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

            private static Logger sDefaultLog = new Logger();
            private static Dictionary<int, Logger> sCustomLogs = new Dictionary<int, Logger>();
            private static int sNextLogId = 0;

            // PUBLIC

            /**
             * Open a log file at the given file path
             * 
             * @param filePath The filpath to save the log file
             */
            public static void Open(string filePath)
            {
                sDefaultLog.Open(filePath);
            }

            /**
             * Close the log file
             */
            public static void Close()
            {
                if (sDefaultLog != null)
                {
                    sDefaultLog.Close();
                }
            }

            /**
             * Create a custom log
             */
            public static int CreateCustom()
            {
                int id = sNextLogId++;
                sCustomLogs.Add(id, new Logger());
                return id;
            }

            /**
             * Close a custom log
             * 
             * @param id The ID of the log returned from CreateCustom
             */
            public static void CloseCustom(int id)
            {
                if (sCustomLogs.ContainsKey(id))
                {
                    sCustomLogs[id].Close();
                    sCustomLogs.Remove(id);
                }
            }

            /**
             * Get an instance of a custom log by ID
             * 
             * @param id The ID of the log created from CreateCustom
             */
            public static Logger Get(int id)
            {
                if (sCustomLogs.ContainsKey(id))
                {
                    return sCustomLogs[id];
                }

                return null;
            }

            /**
             * Set log level from type
             * 
             * @param level The level from type
             */
            public static void SetLogLevel(LogType level)
            {
                sDefaultLog.SetLogLevel((Logger.LogType)level);
            }

            /**
             * Add a custom log level
             * 
             * @param name The name of the level. Use this in the logger call.
             * @param level The level of the custom log
             */
            public static void AddCustomLogLevel(string name, uint level)
            {
                sDefaultLog.AddCustomLogLevel(name, level);
            }

            /**
             * Set log level from custom name
             * 
             * @param levelName The level custom name
             */
            public static void SetCustomLogLevel(string levelName)
            {
                sDefaultLog.SetCustomLogLevel(levelName);
            }

            /**
             * Write message helper
             * 
             * @param message The message to write to the log
             * @param args Parameters for the message
             */
            public static void LogMessage(string message, params object[] args)
            {
                sDefaultLog.LogMessage(message, args);
            }

            /**
             * Write warning helper
             * 
             * @param message The message to write to the log
             * @param args Parameters for the message
             */
            public static void LogWarning(string message, params object[] args)
            {
                sDefaultLog.LogWarning(message, args);
            }

            /**
             * Write warning helper
             *
             * @param message The message to write to the log
             * @param args Parameters for the message
            */
            public static void LogError(string message, params object[] args)
            {
                sDefaultLog.LogError(message, args);
            }

            /**
             * Write console log
             * 
             * @param logType The type of log output
             * @param message The message to write to the log
             * @param args Parameters for the message
             */
            public static void DebugLog(uint logType, string message, params object[] args)
            {
                sDefaultLog.DebugLog(logType, message, args);
            }
        }
    }
}