using KController;
using System.Collections.Generic;
using UnityEngine;

namespace KVisualisation
{
    public class DensityMap3D : MonoBehaviour
    {
        public GameObject cubePrefab;

        public float cubeSize = 10.0f;
        public float widthDistance = 1000.0f;

        public float maxDensity = 10.0f;
        public float duration = 30.0f;

        private Dictionary<int, Dictionary<int, Dictionary<int, GameObject>>> cubes = new Dictionary<int, Dictionary<int, Dictionary<int, GameObject>>>();

        //private int nextCubeId = 0;
        //private Dictionary<int, GameObject> cubeLookup = new Dictionary<int, GameObject>();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SpliceTime(float time)
        {
            foreach (var x in cubes.Keys)
            {
                foreach (var y in cubes[x].Keys)
                {
                    foreach (var z in cubes[x][y].Keys)
                    {
                        GameObject cubeObj = cubes[x][y][z];
                        if (cubeObj != null)
                        {
                            DensityCube dc = cubeObj.GetComponent<DensityCube>();
                            if (dc != null)
                            {
                                dc.SpliceDensity(time);
                            }
                        }
                    }
                }
            }
        }

        /**
         * Update an agent's position
         * 
         * @param position Position of the agent in world space
         */
        public void UpdatePosition(Vector3 position, float simTime)
        {
            Vector3 localPos = transform.InverseTransformPoint(position);

            float halfWidth = widthDistance * 0.5f;

            float scaledHalfWidth = halfWidth;// * transform.localScale.x;
            float scaledCubeSize = cubeSize;// * transform.localScale.x;

            int x = -1, y = -1, z = -1;

            if (localPos.x > -scaledHalfWidth && localPos.x < scaledHalfWidth)
            {
                float xRelative = localPos.x + scaledHalfWidth;

                x = (int)(xRelative / scaledCubeSize);
            }

            if (localPos.y > -scaledHalfWidth && localPos.y < scaledHalfWidth)
            {
                float yRelative = localPos.y + scaledHalfWidth;

                y = (int)(yRelative / scaledCubeSize);
            }

            if (localPos.z > -scaledHalfWidth && localPos.z < scaledHalfWidth)
            {
                float zRelative = localPos.z + scaledHalfWidth;

                z = (int)(zRelative / scaledCubeSize);
            }

            if (!cubes.ContainsKey(x))
            {
                cubes.Add(x, new Dictionary<int, Dictionary<int, GameObject>>());
            }

            if (!cubes[x].ContainsKey(y))
            {
                cubes[x].Add(y, new Dictionary<int, GameObject>());
            }

            if (!cubes[x][y].ContainsKey(z))
            {
                GameObject cube = Instantiate(cubePrefab, transform);
                DensityCube densityCube = cube.GetComponent<DensityCube>();
                if (densityCube != null)
                {
                    densityCube.maxDensity = maxDensity;
                }
                DeactivateTimer timer = cube.GetComponent<DeactivateTimer>();
                if (timer != null)
                {
                    timer.duration = duration;
                }
                cube.transform.localRotation = Quaternion.identity;
                cube.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);
                cube.transform.localPosition = new Vector3(-scaledHalfWidth + x * scaledCubeSize + scaledCubeSize * 0.5f,
                    -scaledHalfWidth + y * scaledCubeSize + scaledCubeSize * 0.5f ,
                    -scaledHalfWidth + z * scaledCubeSize + scaledCubeSize * 0.5f);
                cubes[x][y].Add(z, cube);
            }

            GameObject cubeObj = cubes[x][y][z];
            if (cubeObj != null)
            {
                if (!cubeObj.activeSelf)
                {
                    cubeObj.SetActive(true);
                }

                DensityCube dc = cubeObj.GetComponent<DensityCube>();
                if (dc != null)
                {
                    dc.UpdateDensity(simTime);
                }
            }
        }
    }

}