using UnityEngine;
using KingsleyStephens.Core;

namespace KingsleyStephens
{
    namespace CoreInternal
    {
        /**
         * A mock class for StreamWriter
         */
        public class MockStreamWriter : IStreamWriter
        {

            // PUBLIC

            /**
             * Constructor
             * 
             * @param filePath The path of the file to write to
             * @param append Whether to append or overwrite
             */
            public MockStreamWriter(string filePath, bool append) 
            {
                Debug.Log($"MockStreamWriter({filePath}, {append})");

                if (filePath == "EXCEPTION")
                {
                    throw new System.Exception("MOCK");
                }
            }

            public bool AutoFlush { get { return false; } set { Debug.Log($"AutoFlush = {value}");  } }

            public void Close()
            {
                Debug.Log($"Close()");
            }

            public void Dispose()
            {
                Debug.Log($"Dispose()");
            }

            public void WriteLine(string message)
            {
                Debug.Log($"WriteLine({message})");
            }
        }
    }
}
