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
        private GameObject weaponPrefab;

        [SerializeField]
        private Color[] agentColours;

        [SerializeField]
        private Color weaponColour;

        private Dictionary<int, GameObject> mMovingObjects = new Dictionary<int, GameObject>();
        private List<int> mAllObjectIds = new List<int>();

        private Dictionary<int, int> formationColours = new Dictionary<int, int>();
        private int nextColour = 0;


        public override void OnUpdate()
        {
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
                VisAgentData.AgentClass agentClass = System.Enum.Parse<VisAgentData.AgentClass>(data.objectClass);
                GameObject pf = (agentClass == VisAgentData.AgentClass.MovingObject) ? objectPrefab : weaponPrefab;

                GameObject instance = Instantiate(pf, transform);
                instance.name = data.name;
                instance.transform.position = update.position;
                instance.transform.rotation = update.orientation;
                VisAgentData agentData = instance.GetComponent<VisAgentData>();
                if (agentData != null)
                {
                    agentData.id = data.id;
                    agentData.parentId = data.parentId;
                    agentData.agentClass = agentClass;

                    if (data.parentId != -1)
                    {
                        Color c = Color.white;
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
                        
                        MeshRenderer meshRenderer = instance.GetComponent<MeshRenderer>();
                        if (meshRenderer != null)
                        {
                            KCore.Logger.LogMessage("Set colour " + c.ToString());

                            var m = meshRenderer.material;
                            m.SetColor("_BaseColor", c);
                            m.SetColor("_WireColor", c);
                            meshRenderer.material = m;
                        }

                        TrailRenderer trailRenderer = instance.GetComponentInChildren<TrailRenderer>();
                        if (trailRenderer != null)
                        {
                            var m = trailRenderer.material;
                            m.SetColor("_BaseColor", c);
                            trailRenderer.material = m;
                        }
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
                Destroy(obj);
                mMovingObjects.Remove(id);
            }
        }
    }
}
