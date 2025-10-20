using System.Collections.Generic;
using UnityEngine;

namespace KVisualisation
{

    public class GhostSpawner : MonoBehaviour, ITimeUpdater
    {
        public GameObject ghostPrefab;
        public Color colour = Color.white;

        public Transform ghostParent;

        public VisClient.MovingObjectSettings.GhostingDrop ghostingDropType = VisClient.MovingObjectSettings.GhostingDrop.Distance;
        public float ghostingValue = 1.0f;
        public float duration = 0.0f;

        public int parentPlayer = -1;
        public int parentId = -1;

        private float lastTime = -1000.0f;
        private Vector3 lastPosition = Vector3.zero;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void SetTime(float time)
        {
            if (ghostingDropType == VisClient.MovingObjectSettings.GhostingDrop.Distance)
            {
                if (Vector3.Distance(transform.parent.localPosition, lastPosition) > ghostingValue * transform.parent.localScale.x)
                {
                    Spawn(transform.parent.position, transform.parent.rotation, transform.parent.localScale, time, duration);
                    lastPosition = transform.parent.localPosition;
                }
            }
            else if (ghostingDropType != VisClient.MovingObjectSettings.GhostingDrop.Time)
            {
                if (time - lastTime > ghostingValue)
                {
                    Spawn(transform.parent.position, transform.parent.rotation, transform.parent.localScale, time, duration);
                    lastTime = time;
                }
            }
        }

        public void Reset()
        {
            lastTime = -1000.0f;
            lastPosition = Vector3.zero;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Spawn(Vector3 pos, Quaternion rot, Vector3 scale, float time, float destructTime)
        {
            GameObject ghostObj = Instantiate(ghostPrefab, ghostParent, true);
            ghostObj.transform.position = pos;
            ghostObj.transform.rotation  = rot;
            ghostObj.transform.localScale = scale;

            if (destructTime > 0.0f)
            {
                DestructTimer destruct = ghostObj.AddComponent<DestructTimer>();
                destruct.duration = destructTime;
            }

            MeshRenderer meshRenderer = ghostObj.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
            {
                var m = meshRenderer.material;
                m.SetColor("_BaseColor", colour);
                m.SetColor("_WireColor", colour);
                meshRenderer.material = m;
            }

            GhostObject go = ghostObj.GetComponent<GhostObject>();
            {
                if (go != null)
                {
                    go.parentPlayer = parentPlayer;
                    go.parentId = parentId;
                    go.spawnTime = time;
                }
            }
        }
    }

}
