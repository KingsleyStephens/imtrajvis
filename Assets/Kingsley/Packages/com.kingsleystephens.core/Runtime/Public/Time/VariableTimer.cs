using KingsleyStephens.Core;
using UnityEngine;

namespace KingsleyStephens
{
    namespace Core
    {
        /**
           * Timer class
           */
        public class VariableTimer : TimerBase
        {
            // PRIVATE

            private float mSpeed = 1.0f;

            // PUBLIC

            public float Speed { get { return mSpeed; } set { mSpeed = value; } }
 
            // PROTECTED

            protected override void DerivedSetTime(float time)
            {
                CurrentTime = time;
            }

            protected override void DerivedMinBound()
            {
                Pause(true);
            }

            protected override void DerivedMaxBound()
            {
                Pause(true);
            }

            /**
             * Update
             * 
             * @param deltaTime The current game time delta
             */
            protected override void DerivedUpdate(TimeSnapshot time)
            {
                CurrentTime += mSpeed * time.deltaTime;
            }
        }
    }
}