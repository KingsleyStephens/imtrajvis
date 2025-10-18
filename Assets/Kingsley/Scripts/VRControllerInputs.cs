using UnityEngine;

namespace KVisualisation
{

    public class VRControllerInputs : MonoBehaviour, IVRControllerInputs
    {
        [SerializeField]
        private VRCameraSwitcher cameraSwitcher;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        public void LeftThumbClick()
        {
            GetDelegate()?.LeftThumbClick();
        }

        public void LeftThumbUp(float stickValue)
        {
            GetDelegate()?.LeftThumbUp(stickValue);
        }

        public void LeftThumbDown(float stickValue)
        {
            GetDelegate()?.LeftThumbDown(stickValue);
        }

        public void LeftThumbRight(float stickValue)
        {
            GetDelegate()?.LeftThumbRight(stickValue);
        }

        public void LeftThumbLeft(float stickValue)
        {
            GetDelegate()?.LeftThumbLeft(stickValue);
        }

        public void LeftThumbNeutral()
        {
            GetDelegate()?.LeftThumbNeutral();
        }

        public void LeftPrimaryButtonDown()
        {
            GetDelegate()?.LeftPrimaryButtonDown();
        }

        public void LeftPrimaryButtonUp()
        {
            GetDelegate()?.LeftPrimaryButtonUp();
        }

        public void LeftSecondaryButtonDown()
        {
            GetDelegate()?.LeftSecondaryButtonDown();
        }

        public void LeftSecondaryButtonUp()
        {
            GetDelegate()?.LeftSecondaryButtonUp();
        }

        private VRControllerInputDelegate GetDelegate()
        {
            if (cameraSwitcher.CurrentCamera != null)
            {
                return cameraSwitcher.CurrentCamera.GetComponent<VRControllerInputDelegate>();
            }

            return null;
        }

    }

}
