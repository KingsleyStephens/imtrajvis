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

        private int nextCubeId = 0;
        private Dictionary<int, GameObject> cubeLookup = new Dictionary<int, GameObject>();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdatePosition(Vector3 position)
        {
            float halfWidth = widthDistance * 0.5f;

            int x = -1, y = -1, z = -1;

            if (position.x > transform.position.x - halfWidth && position.x < transform.position.x + halfWidth)
            {
                float xRelative = position.x - (transform.position.x - halfWidth);

                x = (int)(xRelative / cubeSize);
            }

            if (position.y > transform.position.y - halfWidth && position.y < transform.position.y + halfWidth)
            {
                float yRelative = position.y - (transform.position.y - halfWidth);

                y = (int)(yRelative / cubeSize);
            }

            if (position.z > transform.position.z - halfWidth && position.z < transform.position.z + halfWidth)
            {
                float zRelative = position.z - (transform.position.z - halfWidth);

                z = (int)(zRelative / cubeSize);
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
                cube.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);
                cube.transform.localPosition = new Vector3(-halfWidth + x * cubeSize + cubeSize * 0.5f,
                    -halfWidth + y * cubeSize + cubeSize * 0.5f,
                    -halfWidth + z * cubeSize + cubeSize * 0.5f);
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
                    dc.Density = dc.Density + 1.0f;
                }
            }
        }
    }

}