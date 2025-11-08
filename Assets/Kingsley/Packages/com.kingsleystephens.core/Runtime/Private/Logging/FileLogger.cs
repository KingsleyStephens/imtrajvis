using System;
using System.IO;
using UnityEngine;
using System.Text;

namespace KingsleyStephens
{
    namespace CoreInternal
    {
        /**
         * Class to write log messages to a log file
         */
        public class FileLogger : IDisposable
        {
            private readonly string cTimeStampFormat = "HH:mm:ss.fff";
            private IStreamWriter mWriter;

            /**
             * Opens or creates a log file at the given path
             * 
             * @param filePath The file path to open the log file at
             */
            public void Open(string filePath)
            {
                Close();

                try
                {
                    mWriter = StreamWriterFactory.New(filePath, append: true);
                    mWriter.AutoFlush = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to open log file {filePath}: {ex.Message}");
                }
            }

            /**
             * Writes a line to the log file prepended by a timestamp
             * 
             * @param message The message to write to the log file
             */
            public void Write(string message)
            {
                WriteFormattedMessage(message, null, true);
            }

            /**
             * Writes a line to the log file without prepending a timestamp
             * 
             * @param message The message to write to the log file
             */
            public void WriteRaw(string message)
            {
                WriteFormattedMessage(message);
            }

            /**
             * Writes a line to the log file.
             * 
             * @param message The message to write to the log file
             */
            public void WriteWarning(string message)
            {
                WriteFormattedMessage(message, "WARNING", true);
            }


            /**
             * Writes a line to the log file.
             * 
             * @param message The message to write to the log file
             */
            public void WriteError(string message)
            {
                WriteFormattedMessage(message, "ERROR", true);
            }

            /**
             * Closes the log file safely.
             */
            public void Close()
            {
                if (mWriter != null)
                {
                    mWriter.Close();
                    mWriter.Dispose();
                    mWriter = null;
                }
            }

            /**
             * Ensures cleanup if not explicitly closed
             */
            public void Dispose()
            {
                Close();
            }

            private string GetFormattedMessage(string message, string severity = null, bool includeTime = false)
            {
                StringBuilder sb = new StringBuilder();
                if (includeTime)
                {
                    sb.Append($"[{GetTimeStamp()}]");
                }

                if (severity != null)
                {
                    sb.Append($"[{severity}]");
                }

                if (includeTime || severity != null)
                {
                    sb.Append(' ');
                }

                sb.Append(message);

                return sb.ToString();
            }

            private void WriteFormattedMessage(string message, string severity = null, bool includeTime = false)
            {
                if (mWriter == null)
                {
                    Debug.LogWarning("Log file not open. Call Open() first.");
                    return;
                }

                mWriter.WriteLine(GetFormattedMessage(message, severity, includeTime));
            }

            private string GetTimeStamp()
            {
                return DateTime.Now.ToString(cTimeStampFormat);
            }
        }
    }
}

