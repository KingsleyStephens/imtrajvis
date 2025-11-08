using UnityEngine;
using KingsleyStephens.Core;

namespace KingsleyStephens
{
    namespace CoreInternal
    {
        /**
         * StreamWriter factory class
         */
        public static class StreamWriterFactory
        {
            // PUBLIC
            public static bool Mock { get; set; }

            public static IStreamWriter New(string filePath, bool append)
            {
                if (Mock)
                {
                    return new MockStreamWriter(filePath, append);
                }
                else
                {
                    return new StreamWriterProxy(filePath, append);
                }

            }
        }
    }
}
