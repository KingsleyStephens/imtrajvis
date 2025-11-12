using UnityEngine;
using UnityEngine.Events;


namespace KingsleyStephens
{
    namespace GameObjectHelpers
    {
        /**
         * Raise an event on destroy
         */
        public class OnDestroyEvent : MonoBehaviour
        {
            // Event to call on destroy
            public UnityEvent<GameObject> onDestory;

            /**
             * On Destroy
             */
            private void OnDestroy()
            {
                onDestory.Invoke(gameObject);
            }
        }
    }
}
