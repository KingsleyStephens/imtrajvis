using UnityEngine;

namespace KingsleyStephens
{
    namespace VRUI
    {
        /**
         * Desktop implmentation of an Input Interface
         */
        public class DesktopInputInterface : InputInterfaceMonoBehaviour
        {
            // POINTING

            /**
             * Get the current pointing ray
             */
            public override Ray[] GetPointerRays()
            {
                return new Ray[1] { Camera.main.ScreenPointToRay(Input.mousePosition) };
            }

            // SELECTION

            /**
             * Has a select been triggered
             */
            public override bool GetSelectDown()
            {
                return Input.GetMouseButtonDown(0);
            }

            /**
             * Has a select been released
             */
            public override bool GetSelectUp()
            {
                return Input.GetMouseButtonUp(0);
            }

            // SCROLLING

            /**
             * Get the current scroll delta
             */
            public override float GetScroll()
            {
                return Input.GetAxis("Mouse ScrollWheel");
            }
        }


    }
}