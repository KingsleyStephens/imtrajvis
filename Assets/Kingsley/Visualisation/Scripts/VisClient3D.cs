using KController;
using KSimulation;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static KVisualisation.VisClient;
using static Oculus.Interaction.Context;
using static OVRPlugin;


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
        private GameObject linePrefab;
        [SerializeField]
        private GameObject weaponPrefab;

        [SerializeField]
        private Color[] agentColours;

        [SerializeField]
        private Color weaponColour;

        [SerializeField]
        private GameObject densityMap;

        private static VisClient3D instance;

        public static VisClient3D Instance { get { return instance; } }


        private DensityMap3D densityMap3D;

        private Dictionary<int, GameObject> mMovingObjects = new Dictionary<int, GameObject>();
        private List<int> mAllObjectIds = new List<int>();

        private Dictionary<int, int> formationColours = new Dictionary<int, int>();
        private int nextColour = 0;

        private float mLastDensityUpdateTime = 0.0f;

        private Transform mGhostParent;
        private Transform mLineParent;
        private Transform mTrailParent;
        private Transform mWeaponParent;

        public TextMeshProUGUI clientIdLabel;

        private List<int> mFilterSelection = new List<int>();
        private MovingObjectSettings.FilterType mFilterType = MovingObjectSettings.FilterType.None;
        private Dictionary<int, bool> mFilterState= new Dictionary<int, bool>();


        public override void OnStart()
        {
            instance = this;

            GameObject ghostParent = new GameObject("GhostParent");
            ghostParent.transform.parent = transform;
            mGhostParent = ghostParent.transform;
            mGhostParent.localPosition = Vector3.zero;
            mGhostParent.localRotation = Quaternion.identity;
            mGhostParent.localScale = Vector3.one;

            GameObject lineParent = new GameObject("LineParent");
            lineParent.transform.parent = transform;
            mLineParent = lineParent.transform;
            mLineParent.localPosition = Vector3.zero;
            mLineParent.localRotation = Quaternion.identity;
            mLineParent.localScale = Vector3.one;

            GameObject trailParent = new GameObject("TrailParent");
            trailParent.transform.parent = transform;
            mTrailParent = trailParent.transform;
            mTrailParent.localPosition = Vector3.zero;
            mTrailParent.localRotation = Quaternion.identity;
            mTrailParent.localScale = Vector3.one;

            EnsureWeaponParent();

            int[] allIds = mVisSimController.ListSimObjects(mId);
            foreach (int id in allIds)
            {
                mFilterState.Add(id, false);
            }

            ApplySettings();
        }

        private void EnsureWeaponParent()
        {
            if (mWeaponParent == null)
            {
                GameObject weaponParent = new GameObject("WeaponParent");
                weaponParent.transform.parent = transform;
                mWeaponParent = weaponParent.transform;
                mWeaponParent.localPosition = Vector3.zero;
                mWeaponParent.localRotation = Quaternion.identity;
                mWeaponParent.localScale = Vector3.one;
            }
        }

        public override void SpawnAll()
        {
            mAllObjectIds.Clear();
            mMovingObjects.Clear();

            int[] allIds = mVisSimController.ListSimObjects(mId);
            foreach (int id in allIds)
            {
                mAllObjectIds.Add(id);
                Spawn(id);
            }
        }

        public GameObject[] GetAllOjectsOfFormation(int formationId)
        {
            List<GameObject> allObjects = new List<GameObject>();
            foreach (var key in mMovingObjects.Keys)
            {
                GameObject movingObject = mMovingObjects[key];
                VisAgentData data = movingObject.GetComponent<VisAgentData>();
                if (data != null)
                {
                    if (data.parentId == formationId)
                    {
                        allObjects.Add(movingObject);
                    }
                }
            }
            return allObjects.ToArray();
        }

        public override void OnUpdate()
        {
            if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Dynamic)
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
                    var update = mVisSimController.GetSimObjectUpdate(mId, id);

                    GameObject obj;
                    if (mMovingObjects.TryGetValue(id, out obj))
                    {
                        if (update != null)
                        {
                            obj.transform.localPosition = update.position;
                            obj.transform.localRotation = update.orientation;

                            if (updateDensity && !IsFiltered(id))
                            {
                                densityMap3D.UpdatePosition(obj.transform.localPosition);
                            }

                            if (movingObjectSettings.trail)
                            {
                                AdaptiveTrailRenderer adaptiveTrailRenderer = obj.GetComponentInChildren<AdaptiveTrailRenderer>();
                                if (adaptiveTrailRenderer != null)
                                {
                                    adaptiveTrailRenderer.AddPosition(obj.transform.position, mVisSimController.Players[mId].SimTime);
                                }
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
        }

        public override void OnTimeScrub(int playerId, float fromTime, float toTime)
        {
            if (playerId != mId)
            {
                return;
            }

            if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Dynamic)
            {
                foreach (int id in mAllObjectIds)
                {
                    GameObject obj;
                    if (mMovingObjects.TryGetValue(id, out obj))
                    {
                        if (movingObjectSettings.trail)
                        {
                            AdaptiveTrailRenderer adaptiveTrailRenderer = obj.GetComponentInChildren<AdaptiveTrailRenderer>();
                            if (adaptiveTrailRenderer != null)
                            {
                                if (toTime < fromTime)
                                {
                                    adaptiveTrailRenderer.CullOnTime(toTime);
                                }
                                //else
                                //{
                                //    float sampleDelta = 0.1f;
                                //    SimulationPlayer.MovingObjectUpdate[] path = mVisSimController.Player.SampleObjectPath(id, fromTime, toTime, sampleDelta);
                                //    if (path != null)
                                //    {
                                //        float t = fromTime;
                                //        foreach (var p in path)
                                //        {
                                //            adaptiveTrailRenderer.AddPosition(obj.transform.TransformPoint(p.position), t);
                                //            t += sampleDelta;
                                //        }
                                //    }
                                //}
                            }
                        }
                    }
                }
            }
        }

        public override void OnMovingObjectSpawn(int playerId, int id)
        {
            if (playerId != mId)
            {
                return;
            }

            mAllObjectIds.Add(id);

            Spawn(id);
        }

        public override void OnMovingObjectDestruct(int playerId, int id)
        {
            if (playerId != mId)
            {
                return;
            }

            Destruct(id);
        }

        public override void OnSettingsUpdate()
        {
            ApplySettings();
        }

        public override void OnViewChange()
        {
            foreach (Transform child in mGhostParent)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in mLineParent)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in mTrailParent)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in mWeaponParent)
            {
                Destroy(child.gameObject);
            }
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
            var data = mVisSimController.GetSimObjectData(mId, id);
            if (data != null)
            {
                GameObject instance = Instantiate(objectPrefab, transform);
                instance.name = data.name;

                if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Dynamic)
                { 
                    var update = mVisSimController.GetSimObjectUpdate(mId, id);
                    if (update != null)
                    {
                        instance.transform.localPosition = update.position;
                        instance.transform.localRotation = update.orientation;
                    }
                }

                VisAgentData agentData = instance.GetComponent<VisAgentData>();
                if (agentData != null)
                {
                    VisAgentData.AgentClass agentClass = System.Enum.Parse<VisAgentData.AgentClass>(data.objectClass);

                    agentData.id = data.id;
                    agentData.parentId = data.parentId;
                    agentData.agentClass = agentClass;

                    ApplySettingsToMovingObject(instance);

                    if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Dynamic && agentClass == VisAgentData.AgentClass.Weapon)
                    {
                        EnsureWeaponParent();

                        Instantiate(weaponPrefab, mWeaponParent);
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
                            tr.transform.SetParent(mTrailParent, true);
                        }
                    }
                }

                Destroy(obj);
                mMovingObjects.Remove(id);
            }
        }

        private void ApplySettings()
        {
            if (movingObjectSettings.filter != MovingObjectSettings.FilterType.None)
            {
                mFilterType = movingObjectSettings.filter;

                switch (mFilterType)
                {
                    case MovingObjectSettings.FilterType.Clear:
                        {
                            mFilterSelection.Clear();
                            var keys = mFilterState.Keys.ToArray();
                            foreach (var k in keys)
                            {
                                mFilterState[k] = false;
                            }
                            break;
                        }
                    case MovingObjectSettings.FilterType.Selection:
                    case MovingObjectSettings.FilterType.InverseSelection:
                        {
                            mFilterSelection.Clear();

                            foreach (var id in mMovingObjects.Keys)
                            {
                                GameObject movingObject = mMovingObjects[id];
                                bool isSelected = false;

                                SelectableObject selectable = movingObject.GetComponent<SelectableObject>();
                                if (selectable != null)
                                {
                                    isSelected = selectable.State == SelectableObject.StateType.Selected;
                                    selectable.State = SelectableObject.StateType.None;
                                }

                                if (isSelected)
                                {
                                    mFilterSelection.Add(id);
                                }
                            }

                            var keys = mFilterState.Keys.ToArray();
                            foreach (var k in keys)
                            {
                                if (mFilterType == MovingObjectSettings.FilterType.Selection)
                                {
                                    mFilterState[k] = mFilterState[k] || mFilterSelection.Contains(k);
                                }
                                else if (mFilterType == MovingObjectSettings.FilterType.InverseSelection)
                                {
                                    mFilterState[k] = mFilterState[k] || !mFilterSelection.Contains(k);
                                }
                            }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            
            if (densityMap != null)
            {
                densityMap.SetActive(movingObjectSettings.density);

                if (movingObjectSettings.density)
                {
                    densityMap3D = densityMap.GetComponent<DensityMap3D>();
                    //densityMap3D.duration = movingObjectSettings.trailTime;
                }
            }

            if (!movingObjectSettings.ghosting)
            {
                foreach (Transform child in mGhostParent)
                {
                    Destroy(child.gameObject);
                }
            }

            if (!movingObjectSettings.trail || movingObjectSettings.filter != MovingObjectSettings.FilterType.None)
            {
                foreach (Transform child in mLineParent)
                {
                    Destroy(child.gameObject);
                }
                foreach (Transform child in mTrailParent)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Static)
                {
                    foreach (Transform child in mTrailParent)
                    {
                        Destroy(child.gameObject);
                    }
                }
                else
                {
                    foreach (Transform child in mLineParent)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            // Weapons
            foreach (Transform child in mWeaponParent)
            {
                Destroy(child.gameObject);
            }


            if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Static)
            {
                foreach (var id in mMovingObjects.Keys)
                {
                    GameObject go = mMovingObjects[id];
                    Destroy(go);
                }
                mMovingObjects.Clear();

                SpawnAll();
            }
            else
            {
                foreach (var id in mMovingObjects.Keys)
                {
                    ApplySettingsToMovingObject(mMovingObjects[id]);
                }
            }

            if (clientIdLabel != null)
            {
                clientIdLabel.text = (mId + 1).ToString();
            }
        }

        private bool IsFiltered(int objectId)
        {
            return mFilterState.ContainsKey(objectId) && mFilterState[objectId];
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
                if (IsFiltered(agentData.id))
                {
                    return;
                }
                
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

                    float pathStartTime = 0.0f;
                    float pathTimeStep = 0.0f;
                    SimulationPlayer.MovingObjectUpdate[] path = null;
                    if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Static)
                    {
                        pathTimeStep = 0.5f;
                        path = mVisSimController.Players[mId].SampleObjectPath(agentData.id, 0.0f, 60.0f * 10.0f, pathTimeStep);
                        if (path != null && path.Length > 0)
                        {
                            movingObject.transform.localPosition = path[path.Length - 1].position;
                            movingObject.transform.localRotation = path[path.Length - 1].orientation;
                        }
                    }
                    else if (movingObjectSettings.trail)
                    {
                        var data = mVisSimController.Players[mId].GetSimObjectData(agentData.id);

                        pathStartTime = data.timePeriod.x;
                        pathTimeStep = 0.1f;
                        path = mVisSimController.Players[mId].SampleObjectPath(agentData.id, pathStartTime, Mathf.Min(data.timePeriod.y, mVisSimController.Players[mId].SimTime), pathTimeStep);
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
                        if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Static)
                        {
                            if (path != null)
                            {
                                GameObject lineObject = Instantiate(linePrefab, mLineParent);

                                LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
                                if (lineRenderer != null)
                                {
                                    var m = lineRenderer.material;
                                    m.SetColor("_BaseColor", c);
                                    lineRenderer.material = m;

                                    lineRenderer.widthMultiplier = transform.lossyScale.x;

                                    lineRenderer.positionCount = path.Length;
                                    lineRenderer.SetPositions(path.Select((a) => a.position).ToArray());
                                }
                            }
                        }
                        else
                        {
                            GameObject trailObject = Instantiate(trailPrefab, movingObject.transform);

                            LineRenderer trailRenderer = trailObject.GetComponent<LineRenderer>();
                            if (trailRenderer != null)
                            {
                                var m = trailRenderer.material;
                                m.SetColor("_BaseColor", c);
                                trailRenderer.material = m;

                                trailRenderer.widthMultiplier = transform.lossyScale.x;

                                AdaptiveTrailRenderer trail = trailObject.GetComponent<AdaptiveTrailRenderer>();
                                if (trail != null)
                                {
                                    float t = pathStartTime;
                                    foreach (var p in path)
                                    {
                                        trail.AddPosition(movingObject.transform.parent.TransformPoint(p.position), t);
                                        t += pathTimeStep;
                                    }
                                }

                                //trailRenderer.time = movingObjectSettings.trailTime;
                            }
                        }
                    }

                    if (movingObjectSettings.ghosting)
                    {
                        GameObject ghostingObject = new GameObject("ghosting");
                        ghostingObject.transform.SetParent(movingObject.transform);
                        ghostingObject.transform.localPosition = Vector3.zero;
                        ghostingObject.transform.localRotation = Quaternion.identity;
                        ghostingObject.transform.localScale = Vector3.one;

                        GhostSpawner ghostSpawner = ghostingObject.AddComponent<GhostSpawner>();
                        ghostSpawner.ghostPrefab = ghostPrefab;
                        ghostSpawner.ghostParent = mGhostParent;
                        ghostSpawner.ghostingDropType = movingObjectSettings.ghostingDrop;
                        ghostSpawner.ghostingValue = movingObjectSettings.ghostingDropValue;
                        ghostSpawner.colour = movingObjectSettings.overrideGhostColour ? movingObjectSettings.ghostColour : c;
                        //ghostSpawner.duration = movingObjectSettings.trailTime;

                        if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Static)
                        {
                            SimulationPlayer.MovingObjectUpdate[] ghostPath = null;
                            if (ghostSpawner.ghostingDropType == MovingObjectSettings.GhostingDrop.Distance)
                            {
                                ghostPath = mVisSimController.Players[mId].SampleObjectPathDistance(agentData.id, 0.0f, 60.0f * 10.0f, 0.5f, movingObjectSettings.ghostingDropValue);
                            }
                            else
                            {
                                ghostPath = mVisSimController.Players[mId].SampleObjectPath(agentData.id, 0.0f, 60.0f * 10.0f, movingObjectSettings.ghostingDropValue);
                            }

                            if (ghostPath != null && ghostPath.Length > 0)
                            { 
                                foreach (var p in ghostPath)
                                {
                                    ghostSpawner.Spawn(p.position, p.orientation, -1.0f);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
