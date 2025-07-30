using KController;
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

        public VisSimController controller;

        public int instanceId = -1;

        private bool mActive = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
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

        // Update is called once per frame
        void Update()
        {
            if (!mActive && controller != null && controller.Loaded)
            {
                ActivateVisualisation(visualisationType);
                mActive = true;
            }
        }

        public void ApplyConfig(VisClient.MovingObjectSettings settings)
        {
            GameObject[] dimensions = new GameObject[] { oneD, twoD, twoPointFiveD, threeD };
            for (int i = 0; i < dimensions.Length; i++)
            {
                if (dimensions[i] != null && dimensions[i].activeSelf)
                {
                    VisClient visClient = dimensions[i].GetComponent<VisClient>();
                    if (visClient != null)
                    {
                        visClient.ApplySettings(settings);
                    }
                }
            }
        }
    }

}
