using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KVisualisation
{
    public class DensityCube : MonoBehaviour
    {
        public Color[] colours = new Color[4] { Color.white, Color.green, Color.blue, Color.red };

        public float maxDensity = 10.0f;

        private float density = 0.0f;
        private MeshRenderer meshRenderer;

        public delegate bool IsFilteredFunction(Vector2Int objectId);
        
        public IsFilteredFunction IsFilteredCallback { get; set; }

        public class DensityData
        {
            public Vector2Int objectId;
            public float time;
        }

        private List<DensityData> mDensityTimestamps = new List<DensityData>();

        public float Density
        {
            set { density = value; UpdateCube(density); }
            get { return density; }
        }

        public void SpliceDensity(float time)
        {
            mDensityTimestamps = mDensityTimestamps.Where(x => x.time <= time).ToList();
            CalculateDensity();
        }

        public void UpdateDensity(Vector2Int objectId, float time)
        {
            if (mDensityTimestamps.Count == 0 || (mDensityTimestamps.Count > 0 && time > mDensityTimestamps[mDensityTimestamps.Count - 1].time))
            {
                mDensityTimestamps.Add(new DensityData() { objectId = objectId, time = time });
                CalculateDensity();
            }
        }

        public void CalculateDensity()
        {
            if (IsFilteredCallback != null)
            {

                List<DensityData> notFiltered = mDensityTimestamps.Where(x => !IsFilteredCallback(x.objectId)).ToList();
                Density = notFiltered.Count;
            }
        }

        private void UpdateCube(float density)
        {
            if (density == 0.0f)
            {
                gameObject.SetActive(false);
            }
            else
            {
                if (meshRenderer == null)
                {
                    meshRenderer = gameObject.GetComponent<MeshRenderer>();
                }

                if (meshRenderer != null)
                {
                    Material mat = meshRenderer.material;

                    Color c = GetColour(density / maxDensity);
                    c.a = 0.1f;

                    mat.SetColor("_BaseColor", c);

                    meshRenderer.material = mat;
                }
            }
        }

        private Color GetColour(float percent)
        {
            float step = 1.0f / colours.Length;
            int bucket = (int)(percent / step);
            float bucketPercent = percent % step;

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
