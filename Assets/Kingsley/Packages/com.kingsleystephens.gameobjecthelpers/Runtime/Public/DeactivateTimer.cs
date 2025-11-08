using UnityEngine;


namespace KingsleyStephens
{
    namespace GameObjectHelpers
    {
        /**
         * Deactivate a GameObject after a set time
         */
        public class DeactivateTimer : MonoBehaviour
        {
            public float duration = 30.0f;

            private float startTime;

            /**
             * Start
             */
            void Start()
            {
                startTime = Time.time;
            }

            /**
             * Update
             */ 
            void Update()
            {
                if (Time.time > startTime + duration)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
