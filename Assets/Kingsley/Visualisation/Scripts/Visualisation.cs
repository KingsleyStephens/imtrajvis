using KController;
using KingsleyPhD;
using KVisualisation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;



namespace SATestbed
{
    

    public class Visualisation : MonoBehaviour
    {
        public enum VisualisationType
        {
            OneD,
            TwoD,
            TwoPointFiveD,
            ThreeD
        }

        [SerializeField]
        private VisualisationType visualisationType = VisualisationType.ThreeD;

        [SerializeField]
        private GameObject oneD;

        [SerializeField]
        private GameObject twoD;

        [SerializeField]
        private GameObject twoPointFiveD;

        [SerializeField]
        private GameObject threeD;

        public int instanceId = -1;

        private bool mActive = false;

        public VisClient3D VisClient3D { get { return threeD.GetComponent<VisClient3D>(); } }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }


        public void StartVis()
        {
            GameObject[] dimensions = new GameObject[] { oneD, twoD, twoPointFiveD, threeD };
            for (int i = 0; i < dimensions.Length; i++)
            {
                if (dimensions[i] != null && dimensions[i].activeSelf)
                {
                    VisClient visClient = dimensions[i].GetComponent<VisClient>();
                    if (visClient != null)
                    {
                        visClient.OnStart();
                    }
                }
            }
        }

        public void SetViewScale(VisScaler.ScaleType scale)
        {
            if (threeD != null)
            {
                VisSettings visSettings = threeD.GetComponentInChildren<VisSettings>();
                if (visSettings != null && visSettings.instance != null)
                {
                    VisScaler scaler = visSettings.instance.GetComponent<VisScaler>();
                    if (scaler != null)
                    {
                        scaler.Scale = scale;
                    }
                }
            }
        }

        public void Configure(VisSimController controller, int clientId, GameObject scenePrefab)
        {
            instanceId = clientId;

            if (threeD != null && scenePrefab != null)
            {
                VisSettings visSettings = threeD.GetComponentInChildren<VisSettings>();
                if (visSettings != null)
                {
                    visSettings.scenePrefab = scenePrefab;
                }
            }

            if (controller != null)
            {
                GameObject[] dimensions = new GameObject[] { oneD, twoD, twoPointFiveD, threeD };
                for (int i = 0; i < dimensions.Length; i++)
                {
                    if (dimensions[i] != null)
                    {
                        VisClient visClient = dimensions[i].GetComponent<VisClient>();
                        if (visClient != null)
                        {
                            visClient.Id = clientId;
                            visClient.SetController(controller);
                        }
                    }
                }
            }
        }

        public void ActivateVisualisation(VisualisationType visType)
        {
            GameObject[] dimensions = new GameObject[] { oneD, twoD, twoPointFiveD, threeD };
            for (int i = 0; i < dimensions.Length; i++)
            {
                if (dimensions[i] != null)
                {
                    dimensions[i].SetActive(i == (int)visType);
                }
            }
        }

        public void ActivateVis()
        {
            ActivateVisualisation(visualisationType);
        }

        // Update is called once per frame
        void Update()
        {
            //if (!mActive && controller != null && controller.Loaded)
            //{
            //    ActivateVisualisation(visualisationType);
            //    mActive = true;
            //}
        }

        public void ApplyConfig(VisClient.MovingObjectSettings settings, bool reset = false)
        {
            GameObject[] dimensions = new GameObject[] { oneD, twoD, twoPointFiveD, threeD };
            for (int i = 0; i < dimensions.Length; i++)
            {
                if (dimensions[i] != null && dimensions[i].activeSelf)
                {
                    VisClient visClient = dimensions[i].GetComponent<VisClient>();
                    if (visClient != null)
                    {
                        visClient.ApplySettings(instanceId, settings, reset);
                    }
                }
            }
        }

        public void ApplyViewChange()
        {
            GameObject[] dimensions = new GameObject[] { oneD, twoD, twoPointFiveD, threeD };
            for (int i = 0; i < dimensions.Length; i++)
            {
                if (dimensions[i] != null && dimensions[i].activeSelf)
                {
                    VisClient visClient = dimensions[i].GetComponent<VisClient>();
                    if (visClient != null)
                    {
                        visClient.OnViewChange();
                    }
                }
            }
        }
    }

}
