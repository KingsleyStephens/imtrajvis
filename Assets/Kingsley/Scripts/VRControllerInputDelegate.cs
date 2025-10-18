using UnityEngine;
using Oculus.Interaction.Locomotion;

namespace KVisualisation
{

    public class VRControllerInputDelegate : MonoBehaviour, IVRControllerInputs
    {
        private IVRControllerInputs controllerInputs;

        void Start()
        {
            VRFlyCamera flyCamera = GetComponent<VRFlyCamera>();
            if (flyCamera != null)
            {
                controllerInputs = flyCamera;
            }
            else
            {
                VROrbitCamera orbitCamera = GetComponent<VROrbitCamera>();
                if (orbitCamera != null)
                {
                    controllerInputs = orbitCamera;
                }
            }
        }

        public void LeftThumbClick()
        {
            controllerInputs?.LeftThumbClick();
        }

        public void LeftThumbUp(float stickValue)
        {
            controllerInputs?.LeftThumbUp(stickValue);
        }

        public void LeftThumbDown(float stickValue)
        {
            controllerInputs?.LeftThumbDown(stickValue);
        }

        public void LeftThumbRight(float stickValue)
        {
            controllerInputs?.LeftThumbRight(stickValue);
        }

        public void LeftThumbLeft(float stickValue)
        {
            controllerInputs?.LeftThumbLeft(stickValue);
        }

        public void LeftThumbNeutral()
        {
            controllerInputs?.LeftThumbNeutral();
        }

        public void LeftPrimaryButtonDown()
        {
            controllerInputs?.LeftPrimaryButtonDown();
        }

        public void LeftPrimaryButtonUp()
        {
            controllerInputs?.LeftPrimaryButtonUp();
        }

        public void LeftSecondaryButtonDown()
        {
            controllerInputs?.LeftSecondaryButtonDown();
        }
        public void LeftSecondaryButtonUp()
        {
            controllerInputs?.LeftSecondaryButtonUp();
        }
    }

}
