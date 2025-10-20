using System;
using System.IO;
using UnityEngine;

namespace KCore
{
    public class FileLogger : IDisposable
    {
        private StreamWriter writer;
        private string filePath;
        private bool mIsOpen = false;

        /**
         * Opens or creates a log file at the given path
         */
        public void Open(string filePath)
        {
            try
            {
                writer = new StreamWriter(filePath, append: true);
                writer.AutoFlush = true;
                mIsOpen = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to open log file: {ex.Message}");
            }
        }

        /**
         * Writes a line to the log file.
         */
        public void Write(string message)
        {
            if (!mIsOpen)
            {
                Debug.LogWarning("Log file not open. Call Open() first.");
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            writer.WriteLine($"[{timestamp}] {message}");
        }

        /**
         * Writes a line to the log file.
         */
        public void WriteRaw(string message)
        {
            if (!mIsOpen)
            {
                Debug.LogWarning("Log file not open. Call Open() first.");
                return;
            }

            writer.WriteLine(message);
        }

        /**
         * Writes a line to the log file.
         */
        public void WriteWarning(string message)
        {
            if (!mIsOpen)
            {
                Debug.LogWarning("Log file not open. Call Open() first.");
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            writer.WriteLine($"[{timestamp}][WARNING] {message}");
        }


        /**
         * Writes a line to the log file.
         */
        public void WriteError(string message)
        {
            if (!mIsOpen)
            {
                Debug.LogWarning("Log file not open. Call Open() first.");
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            writer.WriteLine($"[{timestamp}][ERROR] {message}");
        }

        /**
         * Closes the log file safely.
         */
        public void Close()
        {
            if (mIsOpen)
            {
                writer.Close();
                writer.Dispose();
                mIsOpen = false;
            }
        }

        /**
         * Ensures cleanup if not explicitly closed
         */
        public void Dispose()
        {
            Close();
        }
    }
}

