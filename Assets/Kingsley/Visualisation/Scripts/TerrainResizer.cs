using UnityEngine;

namespace KVisualisation
{
    public class TerrainResizer : MonoBehaviour
    {
        public float terrainSize = 1000.0f;
        public float terrainHeight = 600.0f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Terrain terrain = gameObject.GetComponent<Terrain>();
            if (terrain != null)
            {
                Vector3 globalScale = transform.lossyScale;

                Vector3 newSize = new Vector3(globalScale.x * terrainSize, globalScale.y * terrainHeight, globalScale.z * terrainSize);
                terrain.terrainData.size = newSize;
            }
        }
    }

}