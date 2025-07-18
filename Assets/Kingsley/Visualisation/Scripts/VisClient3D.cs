using KController;
using KCore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

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

        public override void OnStart()
        {
            if (movingObjectSettings.density)
            {
                if (densityMap != null)
                {
                    densityMap.SetActive(true);
                    densityMap3D = densityMap.GetComponent<DensityMap3D>();
                    densityMap3D.duration = movingObjectSettings.trailTime;
                }
            }
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
                        obj.transform.position = update.position;
                        obj.transform.rotation = update.orientation;

                        if (updateDensity)
                        {
                            densityMap3D.UpdatePosition(obj.transform.position);
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
                instance.transform.position = update.position;
                instance.transform.rotation = update.orientation;
                VisAgentData agentData = instance.GetComponent<VisAgentData>();
                if (agentData != null)
                {
                    VisAgentData.AgentClass agentClass = System.Enum.Parse<VisAgentData.AgentClass>(data.objectClass);

                    agentData.id = data.id;
                    agentData.parentId = data.parentId;
                    agentData.agentClass = agentClass;

                    if (agentClass == VisAgentData.AgentClass.MovingObject)
                    {
                        Color c = Color.white;
                        if (data.parentId != -1)
                        {
                            if (agentClass == VisAgentData.AgentClass.MovingObject)
                            {
                                if (!formationColours.ContainsKey(data.parentId))
                                {
                                    formationColours[data.parentId] = GetNextAgentColour();
                                }

                                c = agentColours[formationColours[data.parentId]];
                            }
                            else
                            {
                                c = weaponColour;
                            }
                        }

                        if (movingObjectSettings.mesh)
                        {
                            GameObject meshObject = Instantiate(meshPrefab, instance.transform);
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
                            GameObject trailObject = Instantiate(trailPrefab, instance.transform);

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
                            GhostSpawner ghostSpawner = instance.AddComponent<GhostSpawner>();
                            ghostSpawner.ghostPrefab = ghostPrefab;
                            ghostSpawner.ghostingDropType = movingObjectSettings.ghostingDrop;
                            ghostSpawner.ghostingValue = movingObjectSettings.ghostingDropValue;
                            ghostSpawner.colour = movingObjectSettings.overrideGhostColour ? movingObjectSettings.ghostColour : c;
                            ghostSpawner.duration = movingObjectSettings.trailTime;
                        }
                    }
                    else if (agentClass == VisAgentData.AgentClass.Weapon)
                    {
                        Instantiate(weaponPrefab, instance.transform);
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
    }
}
