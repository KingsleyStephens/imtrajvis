using KingsleyStephens.Core;
using System;
using UnityEngine;

namespace KingsleyStephens
{
    namespace Core
    {
        /**
         * Timer class
         */
        public class GameTimer : TimerBase
        {
            // PRIVATE

            // PUBLIC

            // PROTECTED

            protected override void DerivedStart()
            {
                mMinTime = TimeKeeper.Instance.Time.time;
            }

            protected override void DerivedMaxBound()
            {
                Stop();
            }

            /**
            * Update
            * 
            * @param time The current game time
            */
            protected override void DerivedUpdate(TimeSnapshot time)
            {
                CurrentTime = time.time;
            }
        }
    }
}