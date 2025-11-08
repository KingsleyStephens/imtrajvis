using System.IO;
using UnityEngine;

namespace KingsleyStephens
{
    namespace CoreInternal
    {
        /**
         * A proxy class for StreamWriter
         */
        public class StreamWriterProxy : IStreamWriter
        {
            private StreamWriter mStreamWriter;

            // PUBLIC

            /**
             * Constructor
             * 
             * @param filePath The path of the file to write to
             * @param append Whether to append or overwrite
             */
            public StreamWriterProxy(string filePath, bool append) 
            {
                mStreamWriter = new StreamWriter(filePath, append);
            }

            public bool AutoFlush { 
                get 
                {
                    return mStreamWriter.AutoFlush; 
                } 
                set 
                {
                    mStreamWriter.AutoFlush = value; 
                } 
            }

            public void Close()
            {
                mStreamWriter.Close();
            }

            public void Dispose()
            {
                mStreamWriter.Dispose();
            }

            public void WriteLine(string message)
            {
                mStreamWriter.WriteLine(message);
            }
        }
    }
}
