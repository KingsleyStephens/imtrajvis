using UnityEngine;

namespace KingsleyStephens
{
    namespace Core
    {
        /**
         * Singleton pattern for MonoBehaviour objects
         */
        public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
        {
            // PRIVATE

            // Singleton instance
            private static T sInstance;

            // PUBLIC

            // Singleton
            public static T Instance { get { return sInstance ?? InitSingleton(); } }

            /**
             * Initialise
             * 
             * Can be triggered manually or else singleton will lazy initialise
             */
            public static T InitSingleton()
            {
                if (sInstance == null)
                { 
                    T[] t = GameObject.FindObjectsByType<T>(FindObjectsSortMode.None);
                    if (t.Length == 0)
                    {
                        GameObject go = new GameObject("Singleton " + typeof(T).Name);
                        sInstance = go.AddComponent<T>();
                    }
                    else if (t.Length > 1)
                    {
                        Debug.LogError("Multiple singleton objects found " + typeof(T));
                    }
                    else
                    {
                        sInstance = t[0];
                    }
                }

                return sInstance;
            }

            /**
             * Destroy the singleton
             */
            public static void DestroySingleton()
            {
                if (sInstance != null)
                {
                    GameObject.DestroyImmediate(sInstance);
                    sInstance = null;
                }
            }
        }
    }
}