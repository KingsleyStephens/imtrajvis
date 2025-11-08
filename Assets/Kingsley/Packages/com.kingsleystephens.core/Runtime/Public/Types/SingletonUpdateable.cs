using UnityEngine;

namespace KingsleyStephens
{
    namespace Core
    {
        /**
         * Singleton pattern for Updateable objects
         */
        public class SingletonUpdateable<T> : Updateable where T : Updateable
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
                if (sInstance == null && ExecutionManager.Instance != null)
                {
                    sInstance = ExecutionManager.Instance.NewUpdateable<T>();
                }

                return sInstance;
            }

            /**
             * Destroy the singleton
             */
            public static void DestroySingleton()
            {
                if (sInstance != null && ExecutionManager.Instance != null)
                {
                    ExecutionManager.Instance.DestroyUpdateable(sInstance);
                    sInstance = null;
                }
            }
        }
    }
}