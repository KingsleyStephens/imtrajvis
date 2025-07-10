using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace SATestbed
{

    public class AgentVisualiser : MonoBehaviour
    {
        private struct PosData
        {
            public float time;
            public Vector3 pos;
            public Vector3 dir;
            public float energy;
        }

        private struct AgentData
        {
            public GameObject agent;
            public List<PosData> posData;
        }

        [SerializeField]
        private GameObject agentRepProto;

        [SerializeField]
        private GameObject mapPlane;
        private Vector3 mapPlanePos;
        private Vector3 mapPlaneRotation;
        private bool mapPlaneChange = false;


        private Dictionary<int, AgentData> agentReps = new Dictionary<int, AgentData>();

        [SerializeField]
        private float timeToZ = 10.0f;

        [SerializeField]
        private float energyToZ = 0.01f;

        private bool registeredForEvents = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            mapPlanePos = mapPlane.transform.position;
            mapPlaneRotation = mapPlane.transform.rotation.eulerAngles;
        }

        // Update is called once per frame
        void Update()
        {
            if (!registeredForEvents && EventManager.Instance != null)
            {
                EventManager.Instance.OnMovementEvent += OnMovementEvent;
                registeredForEvents = true;
            }

            Vector3 newPos = mapPlane.transform.position;
            if (Vector3.Distance(newPos, mapPlanePos) > 0.01f)
            {
                mapPlanePos = newPos;
                mapPlaneChange = true;
            }

            Vector3 newRot = mapPlane.transform.rotation.eulerAngles;
            if (Math.Abs(newRot.x - mapPlaneRotation.x) > 0.01f || Math.Abs(newRot.y - mapPlaneRotation.y) > 0.01f || Math.Abs(newRot.z - mapPlaneRotation.z) > 0.01f)
            {
                mapPlaneRotation = newRot;
                mapPlaneChange = true;
            }

            if (mapPlaneChange)
            {
                foreach (AgentData ad in agentReps.Values)
                {
                    TrailRenderer tr = ad.agent.GetComponent<TrailRenderer>();
                    if (tr != null)
                    {
                        tr.Clear();

                        if (ad.posData.Count > 1) {
                            foreach (PosData pd in ad.posData)
                            {
                                Vector3 pos, dir;
                                MapPos(pd, out pos, out dir);
                                tr.AddPosition(pos);
                            }

                            PosData lastPd = ad.posData[ad.posData.Count - 1];
                            Vector3 lastPos, lastDir;
                            MapPos(lastPd, out lastPos, out lastDir);

                            ad.agent.transform.position = lastPos;
                            ad.agent.transform.LookAt(lastPos + lastDir);
                        }
                    }
                }

                mapPlaneChange = false;
            }
        }

        private void OnMovementEvent(EventManager.Event e)
        {
            int agentId = e.agentId;

            if (!agentReps.ContainsKey(agentId))
            {
                GameObject ap = Instantiate(agentRepProto);
                agentReps.Add(agentId, new AgentData() { agent = ap, posData = new List<PosData>()});

                TrailRenderer tr = ap.GetComponent<TrailRenderer>();
                if (tr != null)
                {
                    tr.material.color = UnityEngine.Random.ColorHSV();
                }
            }

            GameObject agent = agentReps[agentId].agent;
            PosData pd = new PosData() { time = e.time, pos = e.position, dir = e.direction, energy = e.extraValue };
            agentReps[agentId].posData.Add(pd);

            Vector3 pos, dir;
            MapPos(pd, out pos, out dir);

            agent.transform.position = pos;
            agent.transform.LookAt(pos + dir);

        }

        private void MapPos(PosData posData, out Vector3 pos, out Vector3 dir)
        {
            Vector3 localPlanePos = mapPlane.transform.InverseTransformPoint(posData.pos);
            Vector3 localPlaneDir = mapPlane.transform.InverseTransformDirection(posData.dir);

            localPlanePos.y = 0.0f;
            localPlaneDir.y = 0.0f;

            pos = mapPlane.transform.TransformPoint(localPlanePos);
            dir = mapPlane.transform.TransformDirection(localPlaneDir);

            pos.y = mapPlane.transform.position.y + posData.time * timeToZ;     // World space

            Vector3 forward = mapPlane.transform.up;
            forward.y = 0.0f;
            forward.Normalize();

            pos += forward * (energyToZ * (100.0f - posData.energy));
        }
    }
}