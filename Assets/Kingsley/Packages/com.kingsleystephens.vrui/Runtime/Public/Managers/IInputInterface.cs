using UnityEngine;

namespace KingsleyStephens
{
    namespace VRUI
    {
        /**
         * Interface for input devices
         */
        public interface IInputInterface
        {
            // POINTING

            /**
             * Get the current pointing ray
             */
            public Ray[] GetPointerRays();

            // SELECTION

            /**
             * Has a select been triggered
             */
            public bool GetSelectDown();

            /**
             * Has a select been released
             */
            public bool GetSelectUp();

            // SCROLLING

            /**
             * Get the current scroll delta
             */
            public float GetScroll();
        }
    }
}