using System.Collections.Generic;
using UnityEngine;

namespace KVisualisation
{

    public class GhostSpawner : MonoBehaviour
    {
        public GameObject ghostPrefab;
        public Color colour = Color.white;

        public Transform ghostParent;

        public VisClient.MovingObjectSettings.GhostingDrop ghostingDropType = VisClient.MovingObjectSettings.GhostingDrop.Distance;
        public float ghostingValue = 1.0f;
        public float duration = 30.0f;

        private float lastTime = -1000.0f;
        private Vector3 lastPosition = Vector3.zero;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (ghostingDropType == VisClient.MovingObjectSettings.GhostingDrop.Distance)
            {
                if (Vector3.Distance(transform.parent.localPosition, lastPosition) > ghostingValue)
                {
                    Spawn(transform.parent.localPosition, transform.parent.localRotation, duration);
                    lastPosition = transform.parent.localPosition;
                }
            }
            else if (ghostingDropType != VisClient.MovingObjectSettings.GhostingDrop.Time)
            {
                if (Time.time - lastTime > ghostingValue)
                {
                    Spawn(transform.parent.localPosition, transform.parent.localRotation, duration);
                    lastTime = Time.time;
                }
            }
        }

        public void Spawn(Vector3 pos, Quaternion rot, float t)
        {
            GameObject ghostObj = Instantiate(ghostPrefab, ghostParent);
            ghostObj.transform.localPosition = pos;
            ghostObj.transform.localRotation  = rot;

            if (t >= 0.0f)
            {
                DestructTimer destruct = ghostObj.AddComponent<DestructTimer>();
                destruct.duration = t;
            }

            MeshRenderer meshRenderer = ghostObj.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                var m = meshRenderer.material;
                m.SetColor("_BaseColor", colour);
                m.SetColor("_WireColor", colour);
                meshRenderer.material = m;
            }
        }
    }

}
