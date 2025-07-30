using UnityEngine;

namespace KVisualisation
{

    public class VisRoot : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void OnResize()
        {
            TerrainResizer[] terrainResizers = GetComponentsInChildren<TerrainResizer>();
            foreach (var terrainResizer in terrainResizers)
            {
                terrainResizer.Resize();
            }
        }
    }
}
