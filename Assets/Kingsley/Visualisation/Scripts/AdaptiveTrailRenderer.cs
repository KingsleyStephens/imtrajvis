using System.Collections.Generic;
using UnityEngine;

namespace KVisualisation
{

    public class AdaptiveTrailRenderer : MonoBehaviour
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

        public float MaxTime { get { return positionTimes.Keys.Count > 0 ? positionTimes[lineRenderer.positionCount - 1] : 0.0f; } }

        public void AddPosition(Vector3 position, float time)
        {
            lineRenderer.positionCount = lineRenderer.positionCount + 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
            positionTimes[lineRenderer.positionCount - 1] = time;
        }

        public void CullOnTime(float time)
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
