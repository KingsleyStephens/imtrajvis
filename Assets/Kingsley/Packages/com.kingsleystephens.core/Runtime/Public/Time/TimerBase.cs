using UnityEngine;
using static KingsleyStephens.Core.GameTimer;

namespace KingsleyStephens
{
    namespace Core
    {
        /**
         * Base class for timers
         */
        public class TimerBase : Updateable
        {
            // PRIVATE

            private bool mEnabled;
            private bool mPaused = true;
            private float mCurrentTime; // Relative time

            protected float mMinTime = 0.0f;
            protected float mDuration = -1.0f; // Infinite

            // PUBLIC

            public delegate void TimeDelegate(float time);

            public event TimeDelegate OnStart = delegate { };
            public event TimeDelegate OnStop = delegate { };
            public event TimeDelegate OnUpdate = delegate { };
            public event TimeDelegate OnMinTimeBound = delegate { };
            public event TimeDelegate OnMaxTimeBound = delegate { };
            public event TimeDelegate OnPause = delegate { };
            public event TimeDelegate OnUnPause = delegate { };

            public bool IsEnabled { get { return mEnabled; } }
            public bool IsPaused { get { return mPaused; } }

            public float Time { get { return mCurrentTime; } set { DerivedSetTime(value); } }
            public float TimeInterval { get { return mCurrentTime - mMinTime; } set { mCurrentTime = mMinTime + value; } }
            public float NormalisedTime { get { return (mDuration >= 0.0f) ? (mCurrentTime - mMinTime) / mDuration : -1.0f; } set { mCurrentTime = (mDuration >= 0.0f) ? mMinTime + value * mDuration : 0.0f; } }
            public float MinTime { get { return mMinTime; } }
            public float MaxTime { get { return (mDuration >= 0.0f) ? mMinTime + mDuration : -1.0f; } }
            public float Duration { get { return mDuration; } set { mDuration = value; } } // -ve if infinite

            protected float CurrentTime { get { return mCurrentTime; } set { mCurrentTime = value; } }

            /**
             * Start the timer
             */
            public void Start()
            {
                mCurrentTime = mMinTime;
                mEnabled = true;
                mPaused = false;

                DerivedStart();
                OnStart(Time);
            }

            /**
             * Stop the timer
             */
            public void Stop()
            {
                mEnabled = false;
                mPaused = true;

                OnStop(Time);
            }

            /**
             * Pause the timer
             * 
             * @param paused To pause or unpause
             */
            public void Pause(bool paused)
            {
                mPaused = paused;

                if (mPaused)
                {
                    OnPause(mCurrentTime);
                }
                else
                {
                    OnUnPause(mCurrentTime);
                }
            }

            // DERIVABLE INTERFACE

            protected virtual void DerivedStart()
            {

            }

            protected virtual void DerivedSetTime(float time)
            {
                Debug.LogError("Cannot set time manually for this timer");
            }

            protected virtual void DerivedUpdate(TimeSnapshot time)
            {

            }

            protected virtual void DerivedMinBound()
            {

            }

            protected virtual void DerivedMaxBound()
            {

            }

            // UPDATEABLE

            /**
            * Update
            * 
            * @param time The current game time
            */
            protected override void DoUpdate(TimeSnapshot time)
            {
                if (mEnabled && !mPaused)
                {
                    DerivedUpdate(time);
                    UpdateTime();
                }
            }

            /**
             * Make sure time is within the bounds
             */
            private void UpdateTime()
            {
                if (mDuration > 0.0f && mCurrentTime >= MaxTime)
                {
                    mCurrentTime = MaxTime;

                    OnUpdate(Time);

                    OnMaxTimeBound(Time);
                    DerivedMaxBound();
                }
                else if (mCurrentTime <= MinTime)
                {
                    mCurrentTime = MinTime;

                    OnUpdate(Time);

                    OnMinTimeBound(Time);
                    DerivedMinBound();
                }
                else
                {
                    OnUpdate(Time);
                }
            }

        }
    }
}
