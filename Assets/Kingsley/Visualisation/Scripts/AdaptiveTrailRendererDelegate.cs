using System.Collections.Generic;
using UnityEngine;

namespace KVisualisation
{

    public class AdaptiveTrailRendererDelegate : MonoBehaviour
    {
        private LineRenderer lineRenderer;

        private Dictionary<int, float> positionTimes = new Dictionary<int, float>();

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public float MaxTime { get { return positionTimes.Keys.Count > 0 && lineRenderer != null ? positionTimes[lineRenderer.positionCount - 1] : 0.0f; } }

        public void AddPosition(Vector3 position, float time)
        {
            Vector3 localPos = transform.InverseTransformPoint(position);

            if (lineRenderer != null)
            {
                lineRenderer.positionCount = lineRenderer.positionCount + 1;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, localPos);
                positionTimes[lineRenderer.positionCount - 1] = time;
            }
        }

        public void CullOnTime(float time)
        {
            if (lineRenderer != null)
            {
                int i = lineRenderer.positionCount - 1;
                for (; i > 0; i--)
                {
                    if (positionTimes[i] <= time)
                    {
                        break;
                    }

                    positionTimes.Remove(i);
                }
                lineRenderer.positionCount = i + 1;
            }
        }

    }

}
