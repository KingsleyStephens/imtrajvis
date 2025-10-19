using KVisualisation;
using SpatioTemporalDatabase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor.Graphs;
using UnityEngine;
using static KingsleyPhD.TimelineView;
using static System.Collections.Specialized.BitVector32;

namespace KingsleyPhD
{
    /**
     * User facing class for adding a timeline to a scene
     */
    public class Timeline : MonoBehaviour
    {
        // PUBLIC FIELDS
        [SerializeField]
        private GameObject cameraRig;
        [SerializeField]
        private VRCameraSwitcher cameraSwitcher;
        [SerializeField]
        private Camera mainCamera;
        [SerializeField]
        private GameObject workspacePrototype;
        [SerializeField]
        private Transform[] workspaceRoots;
        [SerializeField]
        private Transform fullscreenRoot;
        [SerializeField]
        private Transform designRoot;
        [SerializeField]
        private GameObject scenePrefab;
        [SerializeField]
        private GameObject underwaterPrefab;
        [SerializeField]
        private GameObject cellPrototype;
        [SerializeField]
        private GameObject lineBatcherPrototype;
        [SerializeField]
        private TimelineView.CellConfig cellConfig;
        [SerializeField]
        private TimelineView.LayoutType layoutType = TimelineView.LayoutType.HorizontalLine;
        [SerializeField]
        private int numCells = 1;
        [SerializeField]
        private string designSpaceFile;
        [SerializeField]
        private TMP_Dropdown workspaceRootDropdown;
        [SerializeField]
        private TMP_Dropdown workspaceDropdown;
        [SerializeField]
        private TMP_Dropdown datasetDropdown;
        [SerializeField]
        private TMP_Dropdown sessionDropdown;
        [SerializeField]
        private SnapshotCamera snapshotCamera;
        [SerializeField]
        private UIMouseEvents inputEvents;

        // PRIVATE DATA

        private string mTimelineConfigFile = "config.json";

        private TimelineModel mModel;
        private TimelineView mView;
        private TimelineController mController;

        private List<Workspace> mWorkspaces = new List<Workspace>();

        private TimelineView.CellData mCurrCellData;

        private Dictionary<string, DesignInstanceJSON> mDesignInstances = new Dictionary<string, DesignInstanceJSON>();

        private TimelineJSON mTimelineJSON;

        private Workspace mFPVWorkspace;
        private Workspace mDesignWorkspace;

        private Vector3 mStartPos = Vector3.zero;

        private bool mLockLoadConfig = false;

        private bool mSwitchingScene = false;

        private static Timeline instance;

        // PUBLIC

        public static Timeline Instance {  get { return instance; } }

        public VisClient3D VisClient3D { get {
                if (mFPVWorkspace != null)
                {
                    return mFPVWorkspace.SceneFlowManager.VisClient3D;
                }
                return null;
            } 
        }

        public string WorkspaceRoot { get; set; } = string.Empty;
        public string Workspace { get; set; } = string.Empty;
        public string Dataset { get; set; } = string.Empty;
        public string[] Session { get; set; } = new string[0];


        private string GetLastDirectory(string path)
        {
            return Path.GetFileName(Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        public string[] WorkspaceRoots
        {
            get
            {
                string dir = Path.Combine(Application.persistentDataPath, "Workspaces");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return Directory.GetDirectories(dir).Select(d => GetLastDirectory(d)).ToArray();
            }
        }

        public string[] Workspaces
        {
            get
            {
                string dir = Path.Combine(Application.persistentDataPath, "Workspaces", WorkspaceRoot);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return Directory.GetDirectories(dir).Select(d => GetLastDirectory(d)).ToArray();
            }
        }

        public string[] Datasets
        {
            get
            {
                string dir = Path.Combine(Application.persistentDataPath, "TrajectoryData");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return Directory.GetFiles(dir).Select(f => Path.GetFileNameWithoutExtension(f)).ToArray();
            }
        }

        public string[] Sessions
        {
            get
            {
                if (Dataset != null)
                {
                    string dir = Path.Combine(Application.persistentDataPath, "TrajectoryData");
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    string dbName = Path.Combine(dir, Dataset + ".json");
                    JsonSpatioTemporalDB db = new JsonSpatioTemporalDB(dbName);
                    if (db.CreateOrOpen())
                    {
                        return db.ListTables();
                    }
                }

                return new string[0];
            }
        }

        public void NewWorkspace()
        {
            WorkspaceRoot = DateTime.Today.ToString("yyyy-MM-dd");
            string workspaceRoot = Path.Combine(Application.persistentDataPath, "Workspaces", WorkspaceRoot);

            if (!Directory.Exists(workspaceRoot))
            {
                Directory.CreateDirectory(workspaceRoot);
            }

            int newWorkspace = 1;
            string[] workspaces = Workspaces;
            if (workspaces.Length > 0)
            {
                newWorkspace = int.Parse(workspaces[workspaces.Length - 1]) + 1;
            }

            Workspace = newWorkspace.ToString();

            string workspace = Path.Combine(workspaceRoot, Workspace);
            if (!Directory.Exists(workspace))
            {
                Directory.CreateDirectory(workspace);
            }

            UpdateWorkspaceDropdowns();

            string timelineConfig = Path.Combine(workspaceRoot, Workspace, mTimelineConfigFile);
            if (!File.Exists(timelineConfig))
            {
                mTimelineJSON = new TimelineJSON(timelineConfig);
                mTimelineJSON.Save();
            }
        }

        public int SelectWorkspaceRoot
        {
            set 
            {
                if (WorkspaceRoots.Length > 0)
                {
                    WorkspaceRoot = WorkspaceRoots[value];
                    SelectWorkspace = 0;
                }
            }
        }

        public int SelectWorkspace
        {
            set
            {
                if (Workspaces.Length > 0)
                {
                    Workspace = Workspaces[value];
                    SelectDataset = 0;
                }
            }
        }

        public int SelectDataset
        {
            set
            {
                if (Datasets.Length > 0)
                {
                    Dataset = Datasets[value];
                    
                    UpdateSessionsDropdown();
                    SelectSession = 0;
                }
            }
        }

        public int SelectSession
        {
            set
            {
                if (value == 0) // all
                {
                    Session = Sessions;
                }
                else
                {
                    Session = new string[1] { Sessions[value - 1] };
                }

                mTimelineJSON?.UpdateSession(Dataset, value);

                Reset();
            }
        }

        public void Hide()
        {
            foreach (var ws in mWorkspaces)
            {
                if (!ws.IsFPV)
                {
                    ws.gameObject.SetActive(false);
                }
            }

            transform.parent.gameObject.SetActive(false);
        }

        public void Show()
        {
            foreach (var ws in mWorkspaces)
            {
                ws.gameObject.SetActive(true);
            }

            transform.parent.gameObject.SetActive(true);
        }

        public void Grow(Workspace workspace)
        {
            if (mDesignWorkspace != null)
            {
                if (mTimelineJSON != null)
                {
                    mTimelineJSON.UpdateSlot(workspace.Id, designRoot.localRotation.eulerAngles, designRoot.localScale.x, workspace.SceneFlowManager.SimTime);
                }
            }
            else
            {
                if (mTimelineJSON != null && workspace.Scene != null)
                {
                    mTimelineJSON.UpdateSlot(workspace.Id, workspace.Scene.transform.localRotation.eulerAngles, workspace.Scene.transform.localScale.x, workspace.SceneFlowManager.SimTime);
                }
            }

            mFPVWorkspace = workspace;

            workspace.Scene.Vis.SetParent(fullscreenRoot);
            workspace.Scene.Vis.localPosition = Vector3.zero;
            workspace.Scene.Vis.localScale = Vector3.one;
            workspace.Scene.Vis.localRotation = Quaternion.identity;

            if (!mSwitchingScene)
            {
                UserStart us = workspace.Scene.Vis.GetComponentInChildren<UserStart>();
                if (us != null && cameraRig != null)
                {
                    cameraRig.transform.position = us.transform.position;
                }
            }

            if (cameraSwitcher != null)
            {
                VRCameraSwitcher.CameraType camType = (workspace.SceneFlowManager.FullScale) ? VRCameraSwitcher.CameraType.WorldScale : VRCameraSwitcher.CameraType.RoomScale;
                VRCameraSwitcher.CameraCollection.CameraModeType camMode = (workspace.SceneFlowManager.FreeFly) ? VRCameraSwitcher.CameraCollection.CameraModeType.FreeFly : VRCameraSwitcher.CameraCollection.CameraModeType.Orbit;
                cameraSwitcher.ActivateCamera(camType, camMode);
            }

            workspace.Grow();

            Hide();
        }

        public void Shrink()
        {
            if (mFPVWorkspace != null)
            {
                if (mDesignWorkspace != null)
                {
                    if (designRoot != null)
                    {
                        mFPVWorkspace.Scene.Vis.SetParent(designRoot);
                        designRoot.gameObject.SetActive(true);

                        if (mTimelineJSON != null)
                        {
                            TimelineSlot slot = mTimelineJSON.GetSlot(mDesignWorkspace.Id);
                            if (slot != null)
                            {
                                designRoot.localRotation = Quaternion.Euler(slot.orientation);
                                TrackballControl tb = designRoot.GetComponent<TrackballControl>();
                                if (tb != null)
                                {
                                    tb.ZoomDistance = slot.zoom;
                                }
                            }
                        }
                    }
                }
                else
                {
                    mFPVWorkspace.Scene.Vis.SetParent(mFPVWorkspace.Scene.transform);

                    if (mTimelineJSON != null)
                    {
                        TimelineSlot slot = mTimelineJSON.GetSlot(mFPVWorkspace.Id);
                        if (slot != null)
                        {
                            mFPVWorkspace.Scene.transform.localRotation = Quaternion.Euler(slot.orientation);
                            TrackballControl tb = mFPVWorkspace.Scene.GetComponent<TrackballControl>();
                            if (tb != null)
                            {
                                tb.ZoomDistance = slot.zoom;
                            }
                        }
                    }
                }

                mFPVWorkspace.Scene.Vis.localPosition = Vector3.zero;
                mFPVWorkspace.Scene.Vis.localRotation = Quaternion.identity;
                mFPVWorkspace.Shrink();

                if (!mSwitchingScene)
                {
                    if (cameraRig != null)
                    {
                        cameraRig.transform.position = mStartPos;
                    }

                    if (cameraSwitcher != null)
                    {
                        cameraSwitcher.ActivateCamera(VRCameraSwitcher.CameraType.Menu, VRCameraSwitcher.CameraCollection.CameraModeType.Static);
                    }
                }

                mFPVWorkspace = null;
            }

            if (mDesignWorkspace == null)
            {
                Show();
            }
        }


        public void TimeForward()
        {
            if (mFPVWorkspace != null && mFPVWorkspace.SceneFlowManager != null)
            {
                mFPVWorkspace.SceneFlowManager.DecreaseTime = false;
                mFPVWorkspace.SceneFlowManager.IncreaseTime = true;
            }
        }

        public void TimeBackward()
        {
            if (mFPVWorkspace != null && mFPVWorkspace.SceneFlowManager != null)
            {
                mFPVWorkspace.SceneFlowManager.DecreaseTime = true;
                mFPVWorkspace.SceneFlowManager.IncreaseTime = false;
            }
        }

        public void TimeControlReset()
        {
            if (mFPVWorkspace != null && mFPVWorkspace.SceneFlowManager != null)
            {
                mFPVWorkspace.SceneFlowManager.DecreaseTime = false;
                mFPVWorkspace.SceneFlowManager.IncreaseTime = false;
            }
        }

        public void FilterIsolate()
        {
            if (mFPVWorkspace != null && mFPVWorkspace.SceneFlowManager != null)
            {
                mFPVWorkspace.SceneFlowManager.ApplyFilter(VisClient.MovingObjectSettings.FilterType.InverseSelection);
            }
        }

        public void FilterRemove()
        {
            if (mFPVWorkspace != null && mFPVWorkspace.SceneFlowManager != null)
            {
                mFPVWorkspace.SceneFlowManager.ApplyFilter(VisClient.MovingObjectSettings.FilterType.Selection);
            }
        }

        public void FilterShowAll()
        {
            if (mFPVWorkspace != null && mFPVWorkspace.SceneFlowManager != null)
            {
                mFPVWorkspace.SceneFlowManager.ApplyFilter(VisClient.MovingObjectSettings.FilterType.Clear);
            }
        }

        // PRIVATE

        private void Awake()
        {
            instance = this;
        }

        /**
         * Start
         */
        void Start()
        {
            mModel = new TimelineModel();

            UpdateWorkspaceDropdowns();

            LoadConfig();

            mView = new TimelineView(cellPrototype, lineBatcherPrototype, transform, cellConfig);
            mView.SetLayout(layoutType, numCells);
            mController = new TimelineController(mModel, mView);
            mController.OnActivateCell += CellActivation;
            mController.OnCloseCell += CellClose;
            mController.OnNewCell += CellNew;

            if (cameraRig != null)
            {
                mStartPos = cameraRig.transform.position;
            }

            if (cameraSwitcher != null)
            {
                cameraSwitcher.ActivateCamera(VRCameraSwitcher.CameraType.Menu, VRCameraSwitcher.CameraCollection.CameraModeType.Static);
            }
        }

        private void LoadConfig()
        {
            if (mLockLoadConfig)
            {
                return;
            }

            mLockLoadConfig = true;

            if (mModel != null)
            {
                mModel.Reset();

                string timelineConfig = Path.Combine(Application.persistentDataPath, "Workspaces", WorkspaceRoot, Workspace, mTimelineConfigFile);
                if (File.Exists(timelineConfig))
                {
                    mTimelineJSON = TimelineJSON.Load(timelineConfig);

                    foreach (var slot in mTimelineJSON.slots)
                    {
                        string imageFile = Path.Combine(Application.persistentDataPath, "Workspaces", WorkspaceRoot, Workspace, slot.id + ".png");

                        mModel.AddCell(slot.id, TimelineModel.Cell.CellType.Button, "", false, imageFile);
                    }

                    int index = new List<string>(Datasets).IndexOf(mTimelineJSON.datasetName);
                    datasetDropdown.value = index;
                    sessionDropdown.value = mTimelineJSON.sessionValue;
                }
                else
                {
                    mTimelineJSON = new TimelineJSON(timelineConfig);
                    mTimelineJSON.Save();
                }
            }

            if (mController != null)
            {
                mController.Dirty = true;
            }

            mLockLoadConfig = false;
        }

        private void CellNew(string id)
        {
            if (mTimelineJSON != null)
            {
                mTimelineJSON.AddSlot(id);

                LoadConfig();
            }
        }

        private void UpdateWorkspaceDropdowns()
        {
            string[] roots = WorkspaceRoots;
            if (workspaceRootDropdown != null)
            {
                workspaceRootDropdown.ClearOptions();
                workspaceRootDropdown.AddOptions(new List<string>(roots));

                if (roots.Length > 0)
                {
                    WorkspaceRoot = roots[roots.Length - 1];
                    workspaceRootDropdown.value = roots.Length - 1;
                }
            }

            if (workspaceDropdown != null)
            {
                workspaceDropdown.ClearOptions();

                List<string> ws = (WorkspaceRoot != string.Empty) ? new List<string>(Workspaces) : new List<string>();
                workspaceDropdown.AddOptions(ws);

                if (ws.Count > 0)
                {
                    Workspace = ws[ws.Count - 1];
                    workspaceDropdown.value = ws.Count - 1;
                }
            }

            if (datasetDropdown != null)
            {
                datasetDropdown.ClearOptions();

                List<string> ds = new List<string>(Datasets);
                datasetDropdown.AddOptions(ds);

                //if (ds.Count > 0)
                //{
                //    Dataset = ds[0];
                //    datasetDropdown.value = 0;
                //}
            }

            UpdateSessionsDropdown();
        }

        private void UpdateSessionsDropdown()
        {
            if (sessionDropdown != null)
            {
                sessionDropdown.ClearOptions();
                sessionDropdown.AddOptions(new List<string>() { "All" });

                List<string> ds = new List<string>(Sessions);
                sessionDropdown.AddOptions(ds);

                //if (ds.Count > 0)
                //{
                //    Session = Sessions.ToArray();   // Default to all
                //    sessionDropdown.value = 0;
                //}
            }
        }

        /**
         * Update
         * 
         * Not active in FPV
         */
        void Update()
        {
            if (mController != null)
            {
                mController.Update();
            }
        }

        void WorkspaceChange(Workspace w)
        {
            if (snapshotCamera != null && mCurrCellData != null)
            {
                snapshotCamera.transform.position = w.SnapshotAnchor.position;
                snapshotCamera.transform.rotation = w.SnapshotAnchor.rotation;

                snapshotCamera.TakeSnapshot(Path.Combine(Application.persistentDataPath, "Workspaces", WorkspaceRoot, Workspace, mCurrCellData.image));
                if (mController != null)
                {
                    mController.Dirty = true;
                }
            }
        }

        private void CellActivation(int cellButtonIndex, TimelineView.CellData cellData, bool selected)
        {
            int i = 0;
            while (i < mWorkspaces.Count)
            {
                if (!mWorkspaces[i].Pinned)
                {
                    Destroy(mWorkspaces[i].gameObject);
                    mWorkspaces.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }

            Workspace ws = mWorkspaces.Find(x => x.Id == cellData.id);
            if (ws == null)
            {
                if (cellButtonIndex < workspaceRoots.Length)
                {
                    Transform wsRoot = workspaceRoots[cellButtonIndex];

                    DesignInstanceJSON instance = ActivateCell(cellData.id);

                    if (wsRoot != null)
                    {
                        GameObject workspaceGameObject = Instantiate(workspacePrototype, wsRoot);
                        Workspace workspace = workspaceGameObject.GetComponent<Workspace>();
                        if (workspace != null)
                        {
                            mWorkspaces.Add(workspace);

                            workspace.OnChange += WorkspaceChange;
                            workspace.OnClose += WorkspaceClose;
                            workspace.OnGoFPV += Grow;
                            workspace.OnDesignMenu += OnDesignMenu;

                            workspace.Create(cellData.id, instance, Dataset, Session, Dataset == "simulation" ? underwaterPrefab : scenePrefab, mainCamera);

                            workspace.Scene.transform.GetComponent<TrackballControl>().inputEvents = inputEvents;

                            if (mTimelineJSON != null)
                            {
                                TimelineSlot slot = mTimelineJSON.GetSlot(cellData.id);
                                if (slot != null)
                                {
                                    workspace.Apply(slot.orientation, slot.zoom, slot.time);
                                }
                            }
                        }

                        mCurrCellData = cellData;
                    }
                }
            }
        }


        private void OnDesignMenu(Workspace workspace)
        {
            if (mDesignWorkspace == null)
            {
                mDesignWorkspace = workspace;

                foreach (var ws in mWorkspaces)
                {
                    if (ws != mDesignWorkspace)
                    {
                        ws.gameObject.SetActive(false);
                    }
                }

                if (designRoot != null)
                {
                    mDesignWorkspace.Scene.Vis.SetParent(designRoot);
                    mDesignWorkspace.Scene.Vis.localPosition = Vector3.zero;
                    designRoot.position = mDesignWorkspace.DesignViewAnchor.position;
                    designRoot.rotation = mDesignWorkspace.DesignViewAnchor.rotation;
                    designRoot.gameObject.SetActive(true);
                }

                transform.parent.gameObject.SetActive(false);
            }
            else
            {
                foreach (var ws in mWorkspaces)
                {
                    if (ws != mDesignWorkspace)
                    {
                        ws.gameObject.SetActive(true);
                    }
                }

                mDesignWorkspace.Scene.Vis.SetParent(mDesignWorkspace.Scene.transform);
                mDesignWorkspace.Scene.Vis.localPosition = Vector3.zero;
                mDesignWorkspace = null;

                if (designRoot != null)
                {
                    designRoot.gameObject.SetActive(false);
                }

                transform.parent.gameObject.SetActive(true);
            }
        }

        private void CellClose(TimelineView.CellData cellData)
        {
            if (mCurrCellData != null && mCurrCellData.id == cellData.id)
            {
                Workspace ws = mWorkspaces.Find(x => x.Id == cellData.id);
                if (ws != null)
                {
                    int index = mWorkspaces.IndexOf(ws);
                    Destroy(ws.gameObject);
                    mWorkspaces.RemoveAt(index);
                }

                if (mDesignInstances.ContainsKey(mCurrCellData.id))
                {
                    string fileName = mDesignInstances[mCurrCellData.id].FileName;
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }

                    mDesignInstances.Remove(mCurrCellData.id);
                }

                if (mTimelineJSON != null)
                {
                    mTimelineJSON.RemoveSlot(mCurrCellData.id);
                }

                mCurrCellData = null;
            }
        }

        private void Reset()
        {
            while (mWorkspaces.Count > 0)
            {
                Destroy(mWorkspaces[0].gameObject);
                mWorkspaces.RemoveAt(0);
            }

            mDesignInstances.Clear();

            mCurrCellData = null;

            LoadConfig();
        }

        private void WorkspaceClose(Workspace workspace)
        {
            Workspace ws = mWorkspaces.Find(x => x.Id == workspace.Id);
            if (ws != null)
            {
                if (mTimelineJSON != null)
                {
                    mTimelineJSON.UpdateSlot(ws.Id, ws.Scene.transform.localRotation.eulerAngles, ws.Scene.transform.localScale.x, ws.SceneFlowManager.SimTime);
                }

                if (mDesignWorkspace != null)
                {
                    OnDesignMenu(mDesignWorkspace);
                }

                int index = mWorkspaces.IndexOf(ws);
                Destroy(ws.gameObject);
                mWorkspaces.RemoveAt(index);
            }
        }

        private DesignInstanceJSON ActivateCell(string id)
        {
            if (!mDesignInstances.ContainsKey(id))
            {
                string workspace = Path.Combine(Application.persistentDataPath, "Workspaces",WorkspaceRoot, Workspace);
                if (!Directory.Exists(workspace))
                {
                    NewWorkspace();
                }

                string fileName = Path.Combine(Application.persistentDataPath, "Workspaces",WorkspaceRoot, Workspace, id);
                if (File.Exists(fileName))
                {
                    DesignInstanceJSON instance = DesignInstanceJSON.Load(fileName);
                    if (instance != null)
                    {
                        mDesignInstances.Add(id, instance);
                    }
                }
                else
                {
                    DesignSpaceJSON designSpaceJSON = DesignSpaceJSON.Load(Path.Combine(Application.streamingAssetsPath, designSpaceFile));
                    if (designSpaceJSON != null)
                    {
                        DesignInstanceJSON instance = new DesignInstanceJSON(fileName, designSpaceJSON);
                        instance.Save();
                        mDesignInstances.Add(id, instance);
                    }
                }
            }

            if (mDesignInstances.ContainsKey(id))
            {
                return mDesignInstances[id];
            }
            return null;
        }

        public void NextWorkspace()
        {
            if (mFPVWorkspace != null)
            {
                mSwitchingScene = true;
                int index = mWorkspaces.IndexOf(mFPVWorkspace);
                Shrink(); // Current

                index++;
                if (index >= mWorkspaces.Count)
                {
                    index = 0;
                }

                mWorkspaces[index].UI.FPVActive = true;
                mSwitchingScene = false;
            }
        }

        public void PrevWorkspace()
        {
            if (mFPVWorkspace != null)
            {
                mSwitchingScene = true;
                int index = mWorkspaces.IndexOf(mFPVWorkspace);
                Shrink();       // Current

                index--;
                if (index < 0)
                {
                    index = mWorkspaces.Count - 1;
                }

                mWorkspaces[index].UI.FPVActive = true;
                mSwitchingScene = false;
            }
        }

        public void TimeToggle()
        {
            if (mFPVWorkspace != null)
            {
                mFPVWorkspace.Scene.SceneFlowManager.TimeControlToggle();
            }
        }
    }
}
