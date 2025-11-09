using UnityEngine;

namespace KingsleyStephens
{
    namespace VRUI
    {
        /**
         * Desktop implmentation of an Input Interface
         */
        public abstract class InputInterfaceMonoBehaviour : MonoBehaviour, IInputInterface
        {
            // POINTING

            /**
             * Get the current pointing ray
             */
            public abstract Ray[] GetPointerRays();

            // SELECTION

            /**
             * Has a select been triggered
             */
            public abstract bool GetSelectDown();

            /**
             * Has a select been released
             */
            public abstract bool GetSelectUp();

            // SCROLLING

            /**
             * Get the current scroll delta
             */
            public abstract float GetScroll();
        }


    }
}