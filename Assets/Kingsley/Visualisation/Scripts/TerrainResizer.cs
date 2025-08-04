using UnityEngine;

namespace KVisualisation
{
    public class TerrainResizer : MonoBehaviour
    {
        public float terrainSize = 1000.0f;
        public float terrainHeight = 600.0f;
        public float textureScale = 10.0f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Resize();
        }

        public void Resize()
        {
            Terrain terrain = gameObject.GetComponent<Terrain>();
            if (terrain != null)
            {
                Vector3 globalScale = transform.lossyScale;

                Vector3 newSize = new Vector3(globalScale.x * terrainSize, globalScale.y * terrainHeight, globalScale.z * terrainSize);
                terrain.terrainData.size = newSize;

                // Access the first terrain layer
                TerrainLayer[] layers = terrain.terrainData.terrainLayers;
                if (layers == null || layers.Length == 0)
                {
                    Debug.LogWarning("No terrain layers found.");
                    return;
                }

                TerrainLayer layer = layers[0];

                // Change tile size and offset
                float scale = textureScale * globalScale.x;

                layer.tileSize = new Vector2(scale, scale);
                layer.tileOffset = new Vector2(0f, 0f);

                // Re-assign the modified layer to the terrain
                terrain.terrainData.terrainLayers[0] = layer;
            }
        }
    }

}