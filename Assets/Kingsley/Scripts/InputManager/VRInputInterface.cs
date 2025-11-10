using UnityEngine;

namespace KingsleyStephens
{
    namespace VRUI
    {
        /**
         * Desktop implmentation of an Input Interface
         */
        public class VRInputInterface : InputInterfaceMonoBehaviour
        {
            [SerializeField]
            private Transform rightController;

            // POINTING

            /**
             * Get the current pointing ray
             */
            public override Ray[] GetPointerRays()
            {
                if (rightController == null)
                {
                    return new Ray[0];
                }

                return new Ray[1] { new Ray(rightController.position, rightController.forward) };
            }

            // SELECTION

            /**
             * Has a select been triggered
             */
            public override bool GetSelectDown()
            {
                return OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
            }

            /**
             * Has a select been released
             */
            public override bool GetSelectUp()
            {
                return OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger);
            }

            // SCROLLING

            /**
             * Get the current scroll delta
             */
            public override float GetScroll()
            {
                return OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y * 0.5f;
            }
        }


    }
}
