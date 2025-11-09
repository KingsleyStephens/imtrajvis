using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace KingsleyStephens
{
    namespace VRUI
    {
        /**
         * Class to bind input events to event handlers
         */
        public class InputEventBinder : MonoBehaviour
        {
            /**
             * Input event to Unity event binding
             */
            [Serializable]
            public class InputEvent
            {
                public InputAction action;
                public UnityEvent callback;
            }

            /**
             * List of event bindings
             */
            [SerializeField]
            private InputEvent[] inputEvents;

            /**
             * Start
             */
            void Start()
            {
                foreach (var e in inputEvents)
                {
                    e.action.performed += (ctx) => e.callback.Invoke();
                    e.action.Enable();
                }
            }
        }
    }
}
