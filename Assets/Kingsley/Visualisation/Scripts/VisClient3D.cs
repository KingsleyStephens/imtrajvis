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

        [Header("STC")]
        [SerializeField]
        private GameObject stcMapPlane;

        [SerializeField]
        private Color[] agentColours;

        [SerializeField]
        private Color weaponColour;

        [SerializeField]
        private GameObject densityMap;

        private DensityMap3D densityMap3D;

        private Dictionary<Vector2Int, GameObject> mMovingObjects = new Dictionary<Vector2Int, GameObject>();
        private List<Vector2Int> mAllObjectIds = new List<Vector2Int>();

        private Dictionary<Vector2Int, int> formationColours = new Dictionary<Vector2Int, int>();
        private int nextColour = 0;

        private float mLastDensityUpdateTime = 0.0f;

        private Transform mGhostParent;
        private Transform mLineParent;
        private Transform mTrailParent;
        private Transform mWeaponParent;

        public TextMeshProUGUI clientIdLabel;

        private List<Vector2Int> mFilterSelection = new List<Vector2Int>();
        private MovingObjectSettings.FilterType mFilterType = MovingObjectSettings.FilterType.None;
        private Dictionary<Vector2Int, bool> mFilterState= new Dictionary<Vector2Int, bool>();

        public float SimTime {  get { return mVisSimController.Players[mId].SimTime; } }
        public bool IsPaused { get { return mVisSimController.Players[mId].PlaybackSpeed == 0.0f; } }
        public Vector2 TimeExtents { get { return mVisSimController.Players[mId].TimeExtents; } }

        public bool IsSTC { get; set; }

        private int NumPlayers { get { return mVisSimController.Players.Count; } }

        public override void OnStart()
        {
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

            for (int i = 0; i < NumPlayers; i++)
            {
                int[] allIds = mVisSimController.ListSimObjects(i);
                if (allIds != null)
                {
                    foreach (int id in allIds)
                    {
                        mFilterState.Add(new Vector2Int(i, id), false);
                    }
                }
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

            for (int i = 0; i < NumPlayers; i++)
            {
                int[] allIds = mVisSimController.ListSimObjects(i);
                foreach (int id in allIds)
                {
                    mAllObjectIds.Add(new Vector2Int(i, id));
                    Spawn(i, id);
                }
            }
        }

        public GameObject[] GetAllOjectsOfFormation(int player, int formationId)
        {
            List<GameObject> allObjects = new List<GameObject>();
            foreach (var key in mMovingObjects.Keys)
            {
                GameObject movingObject = mMovingObjects[key];
                VisAgent data = movingObject.GetComponent<VisAgent>();
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
                    if (SimTime > mLastDensityUpdateTime + movingObjectSettings.densityUpdateFrequence)
                    {
                        updateDensity = true;
                        mLastDensityUpdateTime = SimTime;
                    }
                }

                foreach (Vector2Int id in mAllObjectIds)
                {
                    var update = mVisSimController.GetSimObjectUpdate(id.x, id.y);

                    GameObject obj;
                    if (mMovingObjects.TryGetValue(id, out obj))
                    {
                        if (update != null)
                        {
                            obj.transform.localPosition = update.position;
                            obj.transform.localRotation = update.orientation;

                            if (IsSTC)
                            {
                                MapSTC(obj.transform, SimTime, TimeExtents.y - TimeExtents.x);
                            }

                            if (updateDensity && !IsFiltered(id))
                            {
                                densityMap3D.UpdatePosition(obj.transform.position, SimTime);
                            }

                            if (movingObjectSettings.trail)
                            {
                                AdaptiveTrailRenderer adaptiveTrailRenderer = obj.GetComponentInChildren<AdaptiveTrailRenderer>();
                                if (adaptiveTrailRenderer != null)
                                {
                                    adaptiveTrailRenderer.AddPosition(obj.transform.position, SimTime);
                                }
                            }

                            VisAgent visAgent = obj.GetComponentInChildren<VisAgent>();
                            if (visAgent != null)
                            {
                                visAgent.SetTime(SimTime);
                            }
                        }
                        else
                        {
                            Destruct(id.x, id.y);
                        }
                    }
                    else if (update != null)
                    {
                        Spawn(id.x, id.y);
                    }
                }
            }
        }

        private void MapSTC(Transform movingObject, float time, float totalTime)
        {
            if (stcMapPlane == null)
            {
                return;
            }

            Vector3 localPlanePos = stcMapPlane.transform.InverseTransformPoint(movingObject.position);
            Vector3 localPlaneDir = stcMapPlane.transform.InverseTransformDirection(movingObject.forward);

            localPlanePos.y = (time / totalTime) * movingObjectSettings.stcTimeToZ;
            localPlaneDir.y = 0.0f;

            Vector3 pos = stcMapPlane.transform.TransformPoint(localPlanePos);
            Vector3 dir = stcMapPlane.transform.TransformDirection(localPlaneDir);

            movingObject.position = pos;
            movingObject.LookAt(movingObject.position + dir);
        }

        private void MapSTC(Vector3 position, Vector3 forward, float time, float totalTime, out Vector3 outPos, out Vector3 outDir)
        {
            if (stcMapPlane == null)
            {
                outPos = Vector3.zero;
                outDir = Vector3.zero;

                return;
            }

            Vector3 localPlanePos = stcMapPlane.transform.InverseTransformPoint(position);
            Vector3 localPlaneDir = stcMapPlane.transform.InverseTransformDirection(forward);

            localPlanePos.y = (time / totalTime) * movingObjectSettings.stcTimeToZ;
            localPlaneDir.y = 0.0f;

            Vector3 pos = stcMapPlane.transform.TransformPoint(localPlanePos);
            Vector3 dir = stcMapPlane.transform.TransformDirection(localPlaneDir);

            outPos = pos;
            outDir = dir;
        }


        public override void OnTimeScrub(int playerId, float fromTime, float toTime)
        {
            if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Dynamic)
            {
                foreach (Vector2Int id in mAllObjectIds)
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
                            }
                        }

                        if (movingObjectSettings.ghosting)
                        {
                            if (toTime < fromTime)
                            {
                                GhostSpawner ghostSpawner = obj.GetComponentInChildren<GhostSpawner>();
                                if (ghostSpawner != null)
                                {
                                    ghostSpawner.Reset();
                                }
                            }
                        }
                    }
                }

                if (toTime < fromTime)
                {
                    if (movingObjectSettings.density && densityMap3D != null)
                    {
                        mLastDensityUpdateTime = toTime;

                        densityMap3D.SpliceTime(toTime);
                    }

                    foreach (Transform child in mGhostParent)
                    {
                        GhostObject ghostObject = child.gameObject.GetComponent<GhostObject>();
                        if (ghostObject != null)
                        {
                            if (ghostObject.spawnTime > toTime)
                            {
                                Destroy(ghostObject.gameObject);
                            }
                        }
                    }
                }
            }
        }

        public override void OnMovingObjectSpawn(int playerId, int id)
        {
            mAllObjectIds.Add(new Vector2Int(playerId, id));

            Spawn(playerId, id);
        }

        public override void OnMovingObjectDestruct(int playerId, int id)
        {
            Destruct(playerId, id);
        }

        public override void OnSettingsUpdate()
        {
            ApplySettings();
        }

        public override void OnViewChange()
        {
            if (mGhostParent != null)
            {
                foreach (Transform child in mGhostParent)
                {
                    Destroy(child.gameObject);
                }
            }

            if (mLineParent != null)
            {
                foreach (Transform child in mLineParent)
                {
                    Destroy(child.gameObject);
                }
            }

            if (mTrailParent != null)
            {
                foreach (Transform child in mTrailParent)
                {
                    Destroy(child.gameObject);
                }
            }

            if (mWeaponParent != null)
            {
                foreach (Transform child in mWeaponParent)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public override void OnResize()
        {

        }

        private int GetNextAgentColour()
        {
            int colour = nextColour;
            nextColour++;
            nextColour %= agentColours.Length;
            return colour;
        }



        private void Spawn(int parent, int id)
        {
            if (mMovingObjects.ContainsKey(new Vector2Int(parent, id)))
            {
                return;
            }

            var data = mVisSimController.GetSimObjectData(parent, id);
            if (data != null)
            {
                VisAgent.AgentClass agentClass = System.Enum.Parse<VisAgent.AgentClass>(data.objectClass);
                if (agentClass == VisAgent.AgentClass.Weapon)
                {
                    return; // Ignore for now
                }

                GameObject instance = Instantiate(objectPrefab, transform);
                instance.name = data.name;
                instance.transform.localScale = Vector3.one;

                if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Dynamic)
                { 
                    var update = mVisSimController.GetSimObjectUpdate(parent, id);
                    if (update != null)
                    {
                        instance.transform.localPosition = update.position;
                        instance.transform.localRotation = update.orientation;
                    }
                }

                VisAgent agentData = instance.GetComponent<VisAgent>();
                if (agentData != null)
                {
                    agentData.VisClient3D = this;
                    agentData.playerId = parent;
                    agentData.id = data.id;
                    agentData.parentId = data.parentId;
                    agentData.agentClass = agentClass;

                    ApplySettingsToMovingObject(instance);

                    //if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Dynamic && agentClass == VisAgent.AgentClass.Weapon)
                    //{
                    //    EnsureWeaponParent();

                    //    Instantiate(weaponPrefab, mWeaponParent);
                    //}
                }

                mMovingObjects.Add(new Vector2Int(parent, id), instance);
            }
        }

        private void Destruct(int parent, int id)
        {
            GameObject obj;
            if (mMovingObjects.TryGetValue(new Vector2Int(parent, id), out obj))
            {
                VisAgent agentData = obj.GetComponent<VisAgent>();
                if (agentData != null)
                {
                    if (agentData.agentClass == VisAgent.AgentClass.MovingObject && movingObjectSettings.trail)
                    {
                        TrailRenderer tr = obj.GetComponentInChildren<TrailRenderer>();
                        if (tr != null)
                        {
                            tr.transform.SetParent(mTrailParent, true);
                        }
                        AdaptiveTrailRenderer atr = obj.GetComponentInChildren<AdaptiveTrailRenderer>();
                        if (atr != null)
                        {
                            atr.transform.SetParent(mTrailParent, true);

                            GhostObject ghostObject = atr.gameObject.GetComponent<GhostObject>();
                            if (ghostObject != null)
                            {
                                ghostObject.spawnTime = SimTime;
                            }
                        }
                    }
                }

                Destroy(obj);
                mMovingObjects.Remove(new Vector2Int(parent, id));
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

                    densityMap.transform.localScale = movingObjectSettings.objectScale;
                }
            }

            if (!movingObjectSettings.ghosting)
            {
                if (mGhostParent != null)
                {
                    foreach (Transform child in mGhostParent)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
            else
            {
                foreach (var k in mFilterState.Keys)
                {
                    if (mFilterState[k])
                    {
                        foreach (Transform child in mGhostParent)
                        {
                            GhostObject ghostObject = child.gameObject.GetComponent<GhostObject>();
                            if (ghostObject != null)
                            {
                                if (ghostObject.parentId == k.y)
                                {
                                    Destroy(ghostObject.gameObject);
                                }
                            }
                        }
                    }
                }
            }

            if (!movingObjectSettings.trail || movingObjectSettings.filter != MovingObjectSettings.FilterType.None)
            {
                if (mLineParent != null)
                {
                    foreach (Transform child in mLineParent)
                    {
                        Destroy(child.gameObject);
                    }
                }
                if (mTrailParent != null)
                {
                    foreach (Transform child in mTrailParent)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
            else
            {
                if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Static)
                {
                    if (mTrailParent != null)
                    {
                        foreach (Transform child in mTrailParent)
                        {
                            Destroy(child.gameObject);
                        }
                    }
                }
                else
                {
                    if (mLineParent != null)
                    {
                        foreach (Transform child in mLineParent)
                        {
                            Destroy(child.gameObject);
                        }
                    }
                }
            }

            // Weapons
            if (mWeaponParent != null)
            {
                foreach (Transform child in mWeaponParent)
                {
                    Destroy(child.gameObject);
                }
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

            IsSTC = movingObjectSettings.isSTC;
        }

        private bool IsFiltered(Vector2Int objectId)
        {
            return mFilterState.ContainsKey(objectId) && mFilterState[objectId];
        }

        private void ApplySettingsToMovingObject(GameObject movingObject)
        {
            foreach (Transform child in movingObject.transform)
            {
                Destroy(child.gameObject);
            }

            VisAgent agentData = movingObject.GetComponent<VisAgent>();
            if (agentData != null)
            {
                agentData.ClearUpdatables();

                if (IsFiltered(new Vector2Int(agentData.playerId, agentData.id)))
                {
                    return;
                }
                
                if (agentData.agentClass == VisAgent.AgentClass.MovingObject)
                {
                    movingObject.transform.localScale = movingObjectSettings.objectScale;

                    Color c = Color.white;
                    if (agentData.parentId != -1)
                    {
                        if (agentData.agentClass == VisAgent.AgentClass.MovingObject)
                        {
                            if (!formationColours.ContainsKey(new Vector2Int(agentData.playerId, agentData.parentId)))
                            {
                                formationColours[new Vector2Int(agentData.playerId, agentData.parentId)] = GetNextAgentColour();
                            }

                            c = agentColours[formationColours[new Vector2Int(agentData.playerId, agentData.parentId)]];
                        }
                        else
                        {
                            c = weaponColour;
                        }
                    }
                    else
                    {
                        if (!formationColours.ContainsKey(new Vector2Int(agentData.playerId, agentData.id)))
                        {
                            formationColours[new Vector2Int(agentData.playerId, agentData.id)] = GetNextAgentColour();
                        }

                        c = agentColours[formationColours[new Vector2Int(agentData.playerId, agentData.id)]];
                    }

                    float pathStartTime = 0.0f;
                    float pathTimeStep = 0.0f;
                    SimulationPlayer.MovingObjectUpdate[] path = null;
                    if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Static)
                    {
                        pathTimeStep = 0.5f;
                        float totalTime = 60.0f * 10.0f;
                        path = mVisSimController.Players[mId].SampleObjectPath(agentData.id, 0.0f, totalTime, pathTimeStep);
                        if (path != null && path.Length > 0)
                        {
                            if (IsSTC)
                            {
                                float time = 0.0f;
                                foreach (var p in path)
                                {
                                    Vector3 newPos;
                                    Vector3 newDir;
                                    MapSTC(p.position, p.orientation * Vector3.forward, time, totalTime, out newPos, out newDir);
                                    p.position = newPos;
                                    p.orientation = Quaternion.LookRotation(newDir);

                                    time += pathTimeStep;
                                }
                            }

                            movingObject.transform.localPosition = path[path.Length - 1].position;
                            movingObject.transform.localRotation = path[path.Length - 1].orientation;
                        }
                    }
                    else if (movingObjectSettings.trail)
                    {
                        var data = mVisSimController.Players[mId].GetSimObjectData(agentData.id);

                        pathStartTime = data.timePeriod.x;
                        pathTimeStep = 0.1f;
                        float totalTime = Mathf.Min(data.timePeriod.y, SimTime);
                        path = mVisSimController.Players[mId].SampleObjectPath(agentData.id, pathStartTime, totalTime, pathTimeStep);

                        if (IsSTC)
                        {
                            float time = data.timePeriod.x;
                            foreach (var p in path)
                            {
                                Vector3 newPos;
                                Vector3 newDir;
                                MapSTC(p.position, p.orientation * Vector3.forward, time, totalTime,out newPos, out newDir);
                                p.position = newPos;
                                p.orientation = Quaternion.LookRotation(newDir);

                                time += pathTimeStep;
                            }
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

                                    lineRenderer.widthMultiplier = transform.lossyScale.x * movingObjectSettings.objectScale.x;

                                    lineRenderer.positionCount = path.Length;
                                    lineRenderer.SetPositions(path.Select((a) => a.position).ToArray());
                                }
                            }
                        }
                        else
                        {
                            GameObject trailObject = Instantiate(trailPrefab, movingObject.transform);

                            AdaptiveTrailRenderer trail = trailObject.GetComponent<AdaptiveTrailRenderer>();
                            if (trail != null)
                            {
                                trail.Init(mTrailParent);

                                float t = pathStartTime;
                                foreach (var p in path)
                                {
                                    trail.AddPosition(movingObject.transform.parent.TransformPoint(p.position), t);
                                    t += pathTimeStep;
                                }

                                LineRenderer trailRenderer = trail.DelegateTrail.gameObject.GetComponent<LineRenderer>();
                                if (trailRenderer != null)
                                {
                                    var m = trailRenderer.material;
                                    m.SetColor("_BaseColor", c);
                                    trailRenderer.material = m;

                                    trailRenderer.widthMultiplier = transform.lossyScale.x * movingObjectSettings.objectScale.x;
                                }
                            }

                            GhostObject ghostObject = trailObject.GetComponent<GhostObject>();
                            if (ghostObject != null)
                            {
                                ghostObject.parentId = agentData.id;
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
                        ghostSpawner.parentPlayer = agentData.playerId;
                        ghostSpawner.parentId = agentData.id;
                        ghostSpawner.ghostPrefab = ghostPrefab;
                        ghostSpawner.ghostParent = mGhostParent;
                        ghostSpawner.ghostingDropType = movingObjectSettings.ghostingDrop;
                        ghostSpawner.ghostingValue = movingObjectSettings.ghostingDropValue;
                        ghostSpawner.colour = movingObjectSettings.overrideGhostColour ? movingObjectSettings.ghostColour : c;
                        //ghostSpawner.duration = movingObjectSettings.trailTime;
                        agentData.AddUpdatable(ghostSpawner);

                        var data = mVisSimController.Players[mId].GetSimObjectData(agentData.id);
                        float startTime = data.timePeriod.x;

                        if (movingObjectSettings.timeType == MovingObjectSettings.TimeType.Static)
                        {
                            SimulationPlayer.MovingObjectUpdate[] ghostPath = null;
                            if (ghostSpawner.ghostingDropType == MovingObjectSettings.GhostingDrop.Distance)
                            {
                                pathTimeStep = 0.5f;
                                ghostPath = mVisSimController.Players[mId].SampleObjectPathDistance(agentData.id, startTime, data.timePeriod.y, pathTimeStep, movingObjectSettings.ghostingDropValue);
                            }
                            else
                            {
                                pathTimeStep = movingObjectSettings.ghostingDropValue;
                                ghostPath = mVisSimController.Players[mId].SampleObjectPath(agentData.id, startTime, data.timePeriod.y, pathTimeStep);
                            }

                            if (ghostPath != null && ghostPath.Length > 0)
                            {
                                if (IsSTC)
                                {
                                    float time = data.timePeriod.x;
                                    foreach (var p in path)
                                    {
                                        Vector3 newPos;
                                        Vector3 newDir;
                                        MapSTC(p.position, p.orientation * Vector3.forward, time, data.timePeriod.y, out newPos, out newDir);
                                        p.position = newPos;
                                        p.orientation = Quaternion.LookRotation(newDir);

                                        time += pathTimeStep;
                                    }
                                }

                                foreach (var p in ghostPath)
                                {
                                    ghostSpawner.Spawn(p.position, p.orientation, movingObjectSettings.objectScale, p.time, -1.0f);
                                }
                            }
                        }
                        else
                        {
                            float totalTime = Mathf.Min(data.timePeriod.y, SimTime);
                            SimulationPlayer.MovingObjectUpdate[] ghostPath = null;
                            if (ghostSpawner.ghostingDropType == MovingObjectSettings.GhostingDrop.Distance)
                            {
                                pathTimeStep = 0.5f;
                                ghostPath = mVisSimController.Players[mId].SampleObjectPathDistance(agentData.id, startTime, totalTime, pathTimeStep, movingObjectSettings.ghostingDropValue);
                            }
                            else
                            {
                                pathTimeStep = movingObjectSettings.ghostingDropValue;
                                ghostPath = mVisSimController.Players[mId].SampleObjectPath(agentData.id, startTime, pathTimeStep, pathTimeStep);
                            }

                            if (ghostPath != null && ghostPath.Length > 0)
                            {
                                if (IsSTC)
                                {
                                    float time = data.timePeriod.x;
                                    foreach (var p in path)
                                    {
                                        Vector3 newPos;
                                        Vector3 newDir;
                                        MapSTC(p.position, p.orientation * Vector3.forward, time, pathTimeStep, out newPos, out newDir);
                                        p.position = newPos;
                                        p.orientation = Quaternion.LookRotation(newDir);

                                        time += pathTimeStep;
                                    }
                                }

                                foreach (var p in ghostPath)
                                {
                                    ghostSpawner.Spawn(p.position, p.orientation, movingObjectSettings.objectScale, p.time, -1.0f);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
