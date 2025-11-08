using System;
using System.Collections.Generic;
using UnityEngine;
using KingsleyStephens.Core;


namespace KingsleyStephens
{
    namespace ImTrajVis
    {
        /**
         * Class to handle logging
         */
        public static class Logger
        {
            // PUBLIC

            private static int sTelemetryLogId = -1;

            // PUBLIC

            public static void OpenTelemetryLog(string filePath)
            {
                sTelemetryLogId = Log.CreateCustom();
                Log.Get(sTelemetryLogId).Open(filePath, false);
            }

            public static void CloseTelemetryLog()
            {
                Log.CloseCustom(sTelemetryLogId);
            }

            public static void LogTelemetryHeader(string message, params object[] args)
            {
                Debug.Assert(sTelemetryLogId != -1);
                if (sTelemetryLogId == -1)
                {
                    Log.LogError("Need to open telemetry log before writting header to it");
                    return;
                }

                Log.Get(sTelemetryLogId).LogMessage(message, args);
            }

            public static void LogTelemetry(string time, float simTime, string id, Vector3 position, Vector3 rotation)
            {
                Debug.Assert(sTelemetryLogId != -1);
                if (sTelemetryLogId == -1)
                {
                    Log.LogError("Need to open telemetry log before writting to it");
                    return;
                }

                string message = "{0},{1},{2},{3},{4},{5},{6},{7},{8}";
                object[] args = new object[] { time, simTime, id, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z };

                Log.Get(sTelemetryLogId).LogMessage(message, args);
            }
        }
    }
}