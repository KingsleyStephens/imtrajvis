using Controller;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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

        VisSimController mController;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GameObject[] dimensions = new GameObject[] { oneD, twoD, twoPointFiveD, threeD };
            for (int i = 0; i < dimensions.Length; i++) {
                if (dimensions[i] != null) {
                    dimensions[i].SetActive(i == (int)visualisationType);
                }
            }

            mController = GameObject.FindAnyObjectByType<VisSimController>();
            if (mController != null)
            {
                mController.Attach(OnMovingObjectSpawn, OnMovingObjectDestruct);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnMovingObjectSpawn(string name)
        {

        }

        private void OnMovingObjectDestruct(string name)
        {

        }
    }

}
