using UnityEngine;

namespace KingsleyStephens
{
    namespace CoreInternal
    {
        /**
         * Interface for StreamWriter
         */
        public interface IStreamWriter
        {
            // PUBLIC

            public bool AutoFlush { get; set; }

            /**
             * Write a message to the stream
             * 
             * @param message The message to write to the stream
             */
            public void WriteLine(string message);

            /**
             * Close the stream
             */
            public void Close();

            /**
             * Dispose of the stream
             */
            public void Dispose();

        }
    }
}
