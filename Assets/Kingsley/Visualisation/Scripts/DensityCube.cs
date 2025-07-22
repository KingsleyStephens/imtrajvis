using System.Net.Sockets;
using UnityEngine;

namespace KVisualisation
{
    public class DensityCube : MonoBehaviour
    {
        public Color[] colours = new Color[4] { Color.white, Color.green, Color.blue, Color.red };

        public float maxDensity = 10.0f;

        private float density = 0.0f;
        private MeshRenderer meshRenderer;

        public float Density
        {
            set { density = value; UpdateCube(density); }
            get { return density; }
        }

        private void Start()
        {
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
        }

        private void UpdateCube(float density)
        {
            if (meshRenderer != null)
            {
                Material mat = meshRenderer.material;

                Color c = GetColour(density / maxDensity);
                c.a = 0.1f;

                mat.SetColor("_BaseColor", c);

                meshRenderer.material = mat;
            }
        }

        private Color GetColour(float percent)
        {
            float step = 1.0f / colours.Length;
            int bucket = (int)(percent / step);
            float bucketPercent = percent % step * colours.Length;

            if (bucket >= colours.Length - 1)
            {
                return colours[colours.Length - 1];
            }

            Color start = colours[bucket];
            Color end = colours[bucket + 1];
            return Color.Lerp(start, end, bucketPercent);
        }
    }
}
