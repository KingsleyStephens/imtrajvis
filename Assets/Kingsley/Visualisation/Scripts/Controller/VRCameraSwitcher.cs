using System;
using System.Collections.Generic;
using UnityEngine;

namespace KVisualisation
{
    public class VRCameraSwitcher : MonoBehaviour
    {
        public enum CameraType
        {
            Menu,
            RoomScale,
            WorldScale
        }


        [Serializable]
        public class CameraCollection
        {
            public enum CameraModeType
            {
                Static,
                FreeFly,
                Orbit
            }

            [SerializeField]
            private GameObject staticCamera;
            [SerializeField]
            private GameObject freefly;
            [SerializeField]
            private GameObject orbit;

            private GameObject[] mAllCameras;

            public void DeactivateAll()
            {
                var cams = GetAllCameras();
                foreach (var c in cams)
                {
                    if (c != null)
                    {
                        c.SetActive(false);
                    }
                }
            }

            public void ActivateMode(CameraModeType mode)
            {
                var cams = GetAllCameras();
                int index = (int)mode;
                if (index < cams.Length)
                {
                    if (cams[index] != null)
                    {
                        cams[index].SetActive(true);
                    }
                    else
                    {
                        Debug.LogWarning("[VRCameraSwitcher] ActivateMode can't find camera mode type object for " + mode.ToString());
                    }
                    
                }
            }

            private GameObject[] GetAllCameras()
            {
                if (mAllCameras == null)
                {
                    mAllCameras = new GameObject[] { staticCamera, freefly, orbit };
                }

                return mAllCameras;
            }
        }

        [Header("Default camera")]
        [SerializeField]
        private CameraType cameraType = CameraType.Menu;
        [SerializeField]
        private CameraCollection.CameraModeType cameraMode = CameraCollection.CameraModeType.Static;

        [Header("Camera Collections")]
        [SerializeField]
        private CameraCollection menuCameras;
        [SerializeField]
        private CameraCollection roomScaleCameras;
        [SerializeField]
        private CameraCollection worldScaleCameras;

        // PRIVATE

        private CameraCollection[] mAllCameraCollections;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            ActivateCamera(cameraType, cameraMode);
        }

        // PUBLIC

        public void ActivateCamera(CameraType cameraType, CameraCollection.CameraModeType mode)
        {
            DeactivateAll();

            var cols = GetAllCameraCollections();
            int index = (int)cameraType;

            Debug.Log("cols.length " + cols.Length.ToString() + " index " + index.ToString());

            if (index < cols.Length)
            {
                cols[index].ActivateMode(mode);
            }
        }

        // PRIVATE

        private void DeactivateAll()
        {
            var cols = GetAllCameraCollections();
            foreach (var c in cols)
            {
                c.DeactivateAll();
            }
        }

        private CameraCollection[] GetAllCameraCollections()
        {
            if (mAllCameraCollections == null)
            {
                mAllCameraCollections = new CameraCollection[] { menuCameras, roomScaleCameras, worldScaleCameras };
            }

            return mAllCameraCollections;
        }
    }

}