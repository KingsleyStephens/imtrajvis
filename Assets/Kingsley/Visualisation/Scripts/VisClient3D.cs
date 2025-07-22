using KController;
using KCore;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static Oculus.Interaction.Context;

namespace KVisualisation
{
    public class VisClient3D : VisClient
    {
        [SerializeField]
        private GameObject objectPrefab;
        [SerializeField]
        private GameObject meshPrefab;
        [SerializeField]
        private GameObject ghostPrefab;
        [SerializeField]
        private GameObject trailPrefab;
        [SerializeField]
        private GameObject weaponPrefab;

        [SerializeField]
        private Color[] agentColours;

        [SerializeField]
        private Color weaponColour;

        [SerializeField]
        private GameObject densityMap;

        private DensityMap3D densityMap3D;

        private Dictionary<int, GameObject> mMovingObjects = new Dictionary<int, GameObject>();
        private List<int> mAllObjectIds = new List<int>();

        private Dictionary<int, int> formationColours = new Dictionary<int, int>();
        private int nextColour = 0;

        private float mLastDensityUpdateTime = 0.0f;

        private Transform mGhostParent;

        public override void OnStart()
        {
            GameObject ghostParent = new GameObject("GhostParent");
            ghostParent.transform.parent = transform;
            mGhostParent = ghostParent.transform;
            mGhostParent.localPosition = Vector3.zero;
            mGhostParent.localRotation = Quaternion.identity;

            ApplySettings();
        }


        public override void OnUpdate()
        {
            bool updateDensity = false;
            if (movingObjectSettings.density && densityMap3D != null)
            {
                if (Time.time > mLastDensityUpdateTime + movingObjectSettings.densityUpdateFrequence)
                {
                    updateDensity = true;
                    mLastDensityUpdateTime = Time.time;
                }
            }

            foreach (int id in mAllObjectIds)
            {
                var update = mVisSimController.GetSimObjectUpdate(id);

                GameObject obj;
                if (mMovingObjects.TryGetValue(id, out obj))
                {
                    if (update != null)
                    {
                        obj.transform.localPosition = update.position;
                        obj.transform.localRotation = update.orientation;

                        if (updateDensity)
                        {
                            densityMap3D.UpdatePosition(obj.transform.localPosition);
                        }
                    }
                    else
                    {
                        Destruct(id);
                    }
                }
                else if (update != null)
                {
                    Spawn(id);
                }
            }
        }


        public override void OnMovingObjectSpawn(int id)
        {
            mAllObjectIds.Add(id);

            Spawn(id);
        }

        public override void OnMovingObjectDestruct(int id)
        {
            Destruct(id);
        }

        public override void OnSettingsUpdate()
        {
            ApplySettings();
        }

        private int GetNextAgentColour()
        {
            int colour = nextColour;
            nextColour++;
            nextColour %= agentColours.Length;
            return colour;
        }

        private void Spawn(int id)
        {
            var data = mVisSimController.GetSimObjectData(id);
            var update = mVisSimController.GetSimObjectUpdate(id);
            if (data != null && update != null)
            {
                GameObject instance = Instantiate(objectPrefab, transform);
                instance.name = data.name;
                instance.transform.localPosition = update.position;
                instance.transform.localRotation = update.orientation;
                VisAgentData agentData = instance.GetComponent<VisAgentData>();
                if (agentData != null)
                {
                    VisAgentData.AgentClass agentClass = System.Enum.Parse<VisAgentData.AgentClass>(data.objectClass);

                    agentData.id = data.id;
                    agentData.parentId = data.parentId;
                    agentData.agentClass = agentClass;

                    ApplySettingsToMovingObject(instance);

                    if (agentClass == VisAgentData.AgentClass.Weapon)
                    {
                        Instantiate(weaponPrefab, instance.transform.parent);
                    }
                }

                mMovingObjects.Add(id, instance);
            }
        }

        private void Destruct(int id)
        {
            GameObject obj;
            if (mMovingObjects.TryGetValue(id, out obj))
            {
                VisAgentData agentData = obj.GetComponent<VisAgentData>();
                if (agentData != null)
                {
                    if (agentData.agentClass == VisAgentData.AgentClass.MovingObject && movingObjectSettings.trail)
                    {
                        TrailRenderer tr = obj.GetComponentInChildren<TrailRenderer>();
                        if (tr != null)
                        {
                            tr.transform.SetParent(obj.transform.parent, true);
                        }
                    }
                }

                Destroy(obj);
                mMovingObjects.Remove(id);
            }
        }

        private void ApplySettings()
        {
            if (densityMap != null)
            {
                densityMap.SetActive(movingObjectSettings.density);

                if (movingObjectSettings.density)
                {
                    densityMap3D = densityMap.GetComponent<DensityMap3D>();
                    densityMap3D.duration = movingObjectSettings.trailTime;
                }
            }

            if (!movingObjectSettings.ghosting)
            {
                foreach (Transform child in mGhostParent)
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (var id in mMovingObjects.Keys)
            {
                ApplySettingsToMovingObject(mMovingObjects[id]);
            }
        }

        private void ApplySettingsToMovingObject(GameObject movingObject)
        {
            foreach (Transform child in movingObject.transform)
            {
                Destroy(child.gameObject);
            }

            VisAgentData agentData = movingObject.GetComponent<VisAgentData>();
            if (agentData != null)
            {
                if (agentData.agentClass == VisAgentData.AgentClass.MovingObject)
                {
                    Color c = Color.white;
                    if (agentData.parentId != -1)
                    {
                        if (agentData.agentClass == VisAgentData.AgentClass.MovingObject)
                        {
                            if (!formationColours.ContainsKey(agentData.parentId))
                            {
                                formationColours[agentData.parentId] = GetNextAgentColour();
                            }

                            c = agentColours[formationColours[agentData.parentId]];
                        }
                        else
                        {
                            c = weaponColour;
                        }
                    }

                    if (movingObjectSettings.mesh)
                    {
                        GameObject meshObject = Instantiate(meshPrefab, movingObject.transform);
                        MeshRenderer meshRenderer = meshObject.GetComponent<MeshRenderer>();
                        if (meshRenderer != null)
                        {
                            var m = meshRenderer.material;
                            m.SetColor("_BaseColor", c);
                            m.SetColor("_WireColor", c);
                            meshRenderer.material = m;
                        }
                    }

                    if (movingObjectSettings.trail)
                    {
                        GameObject trailObject = Instantiate(trailPrefab, movingObject.transform);

                        TrailRenderer trailRenderer = trailObject.GetComponent<TrailRenderer>();
                        if (trailRenderer != null)
                        {
                            trailRenderer.time = movingObjectSettings.trailTime;

                            var m = trailRenderer.material;
                            m.SetColor("_BaseColor", c);
                            trailRenderer.material = m;
                        }
                    }

                    if (movingObjectSettings.ghosting)
                    {
                        GameObject ghostingObject = new GameObject("ghosting");
                        ghostingObject.transform.SetParent(movingObject.transform);
                        ghostingObject.transform.localPosition = Vector3.zero;
                        ghostingObject.transform.localRotation = Quaternion.identity;

                        GhostSpawner ghostSpawner = ghostingObject.AddComponent<GhostSpawner>();
                        ghostSpawner.ghostPrefab = ghostPrefab;
                        ghostSpawner.ghostParent = mGhostParent;
                        ghostSpawner.ghostingDropType = movingObjectSettings.ghostingDrop;
                        ghostSpawner.ghostingValue = movingObjectSettings.ghostingDropValue;
                        ghostSpawner.colour = movingObjectSettings.overrideGhostColour ? movingObjectSettings.ghostColour : c;
                        ghostSpawner.duration = movingObjectSettings.trailTime;
                    }
                }
            }
        }
    }
}
