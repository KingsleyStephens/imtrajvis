using System.Collections.Generic;
using UnityEngine;
using KingsleyStephens.CoreInternal;

namespace KingsleyStephens
{
    namespace Core
    {
        /**
         * Time manager
         */
        public class TimeKeeper : SingletonUpdateable<TimeKeeper>
        {
            // PRIVATE

            private TimeSnapshot mGameTime;

            // PUBLIC

            // Game time and frame delta
            public TimeSnapshot Time { get { return mGameTime; } }

            /**
             * Add a new timer
             */
            public T NewTimer<T>() where T : TimerBase
            {
                if (ExecutionManager.Instance != null)
                {
                    T t = ExecutionManager.Instance.NewUpdateable<T>();
                    t.Priority = Priority + 1;  // Make slightly less priority than main game timer
                    return t;
                }

                return default(T);
            }

            /**
             * Remove game timer
             * 
             * @param timer The GameTimer object previously created
             */
            public void DestroyTimer<T>(T t) where T : TimerBase
            {
                if (ExecutionManager.Instance != null)
                {
                    ExecutionManager.Instance.DestroyUpdateable(t);
                }
            }

            // PRIVATE

            public override void Init()
            {
                Priority = -10000;   // Place high priority in execution order so game time is recorded first
            }

            /**
             * Update
             */
            protected override void DoUpdate(TimeSnapshot time) 
            {
                mGameTime = time;
            }

        }
    }
}