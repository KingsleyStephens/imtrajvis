using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SATestbed
{

    public class VisSettings : MonoBehaviour
    {
        public GameObject scenePrefab;
        public bool fog = true;
        public Color fogColour = Color.black;
        public float fogDensity = 0.01f;
        public Material skyboxMaterial = null;

        public GameObject instance;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            ApplyFog();
            RenderSettings.skybox = skyboxMaterial;
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnEnable()
        {
            if (scenePrefab != null)
            {
                instance = Instantiate(scenePrefab, transform);
            }
        }

        private void OnDisable()
        {
            if (instance != null)
            {
                Destroy(instance);
                instance = null;
            }
        }

        private void ApplyFog()
        {
            RenderSettings.fog = fog;
            if (fog)
            {
                RenderSettings.fogColor = fogColour;
                RenderSettings.fogDensity = fogDensity;
            }
        }
    }

}