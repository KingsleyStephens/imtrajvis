
namespace KingsleyStephens
{
    namespace Core
    {
        /**
         * Struct to pass around time information
         */
        public struct TimeSnapshot
        {
            public float time;              // A time instant
            public float deltaTime;         // A time increment

            /**
             * Constructor
             * 
             * @param time The time instant
             * @param delatTime The time increment
             */
            public TimeSnapshot(float time, float deltaTime)
            {
                this.time = time;
                this.deltaTime = deltaTime;
            }
        }
    }
}