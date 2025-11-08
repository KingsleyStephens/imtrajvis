using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KingsleyStephens.CoreInternal;

namespace KingsleyStephens
{
    namespace Core
    {
        /**
         * Base class for something that can be updated
         */
        public class Updateable : ScriptableObject
        {
            // PRIVATE

            private float mLastUpdateTime;

            // PUBLIC

            // If the Updateable is enabled or not. If disabled then update calls won't execute
            public bool Enabled { get; set; } = true;

            // Order of execution. < 0 is higher priority, > 0 is lower in priority
            public int Priority { get; set; }

            // Time in seconds to tick
            public float UpdateInterval { get; set; }

            /**
             * Called on load
             */
            public void OnEnable()
            {
                mLastUpdateTime = -1.0f;
                Init();
            }

            /**
             * Called on going out of scope
             */
            public void OnDisable()
            {
                Shutdown();
            }

            /**
             * Initialise
             */
            public virtual void Init() { }

            /**
             * Initialise
             */
            public virtual void Shutdown() { }


            /**
             * Initialise
             */
            public virtual void BeginUpdating() { }

            /**
             * Shutdown
             */
            public virtual void EndUpdating() { }

            /**
             * Run the update
             * 
             * @param time A time snapshot
             */
            public void RunUpdate(TimeSnapshot time)
            {
                if (Enabled && time.time > mLastUpdateTime + UpdateInterval)
                {
                    DoUpdate(time);

                    mLastUpdateTime = time.time;
                }
            }


            // PROTECTED

            protected virtual void DoUpdate(TimeSnapshot time) { }

        }
    }
}