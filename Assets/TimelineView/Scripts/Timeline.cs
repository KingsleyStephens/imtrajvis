using KVisualisation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using static KingsleyPhD.TimelineView;

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
        private GameObject workspacePrototype;
        [SerializeField]
        private Transform[] workspaceRoots;
        [SerializeField]
        private Transform fullscreenRoot;
        [SerializeField]
        private Transform designRoot;
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
        private SnapshotCamera snapshotCamera;
        [SerializeField]
        private SceneFlowManager sceneFlowManager;


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

        // PUBLIC

        public string WorkspaceRoot { get; set; } = string.Empty;
        public string Workspace { get; set; } = string.Empty;

        private string GetLastDirectory(string path)
        {
            return Path.GetFileName(Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        public string[] WorkspaceRoots
        {
            get
            {
                return Directory.GetDirectories(Application.persistentDataPath).Select(d => GetLastDirectory(d)).ToArray();
            }
        }

        public string[] Workspaces
        {
            get
            {
                return Directory.GetDirectories(Path.Combine(Application.persistentDataPath, WorkspaceRoot)).Select(d => GetLastDirectory(d)).ToArray();
            }
        }


        public void NewWorkspace()
        {
            WorkspaceRoot = DateTime.Today.ToString("yyyy-MM-dd");
            string workspaceRoot = Path.Combine(Application.persistentDataPath, WorkspaceRoot);

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
                WorkspaceRoot = WorkspaceRoots[value];
                SelectWorkspace = 0;
            }
        }

        public int SelectWorkspace
        {
            set
            {
                Workspace = Workspaces[value];

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
            mFPVWorkspace = workspace;

            UserStart us = workspace.ViewRoot.GetComponentInChildren<UserStart>();
            Debug.Log(us);
            if (us != null && cameraRig != null)
            {
                cameraRig.transform.position = us.transform.position;
            }

            workspace.ViewRoot.SetParent(fullscreenRoot);
            workspace.ViewRoot.localPosition = Vector3.zero;
            workspace.Grow();

            Hide();
        }

        public void Shrink()
        {
            if (mFPVWorkspace != null)
            {
                if (mDesignWorkspace != null)
                {
                    mFPVWorkspace.ViewRoot.SetParent(designRoot);
                }
                else
                {
                    mFPVWorkspace.ViewRoot.SetParent(mFPVWorkspace.SceneRoot);
                }

                mFPVWorkspace.ViewRoot.localPosition = Vector3.zero;
                mFPVWorkspace.Shrink();

                if (cameraRig != null)
                {
                    cameraRig.transform.position = mStartPos;
                }

                mFPVWorkspace = null;
            }

            Show();
        }

        // PRIVATE


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
        }

        private void LoadConfig()
        {
            if (mModel != null)
            {
                mModel.Reset();

                string timelineConfig = Path.Combine(Application.persistentDataPath, WorkspaceRoot, Workspace, mTimelineConfigFile);
                if (File.Exists(timelineConfig))
                {
                    mTimelineJSON = TimelineJSON.Load(timelineConfig);

                    foreach (var slot in mTimelineJSON.slots)
                    {
                        string imageFile = Path.Combine(Application.persistentDataPath, WorkspaceRoot, Workspace, slot.id + ".png");

                        mModel.AddCell(slot.id, TimelineModel.Cell.CellType.Button, "", false, imageFile);
                    }
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
        }

        /**
         * Update
         */
        void Update()
        {
            mController.Update();
        }

        void WorkspaceChange()
        {
            if (snapshotCamera != null && mCurrCellData != null)
            {
                snapshotCamera.TakeSnapshot(Path.Combine(Application.persistentDataPath, WorkspaceRoot, Workspace, mCurrCellData.image));
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

            Workspace ws = mWorkspaces.Find(x => x.CellId == cellData.id);
            if (ws == null)
            {
                if (cellButtonIndex < workspaceRoots.Length)
                {
                    Transform wsRoot = workspaceRoots[cellButtonIndex];

                    DesignInstanceJSON instance = ActivateCell(cellData.id);

                    if (wsRoot != null)
                    {
                        GameObject wsObj = Instantiate(workspacePrototype, wsRoot, true);
                        wsObj.transform.localPosition = Vector3.zero;
                        wsObj.transform.localScale = Vector3.one;

                        Workspace workspace = wsObj.GetComponent<Workspace>();
                        if (workspace != null)
                        {
                            workspace.CellId = cellData.id;
                            workspace.DesignInstance = instance;
                            workspace.OnChange += WorkspaceChange;
                            workspace.OnClose += WorkspaceClose;
                            workspace.OnGoFPV += Grow;
                            workspace.OnDesignMenu += OnDesignMenu;
                            mWorkspaces.Add(workspace);

                            if (sceneFlowManager != null)
                            {
                                sceneFlowManager.Launch(workspace.ViewRoot);
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

                mDesignWorkspace.ViewRoot.SetParent(designRoot);
                mDesignWorkspace.ViewRoot.localPosition = Vector3.zero;

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

                mDesignWorkspace.ViewRoot.SetParent(mDesignWorkspace.SceneRoot);
                mDesignWorkspace.ViewRoot.localPosition = Vector3.zero;
                mDesignWorkspace = null;

                transform.parent.gameObject.SetActive(true);
            }
        }

        private void CellClose(TimelineView.CellData cellData)
        {
            if (mCurrCellData != null && mCurrCellData.id == cellData.id)
            {
                Workspace ws = mWorkspaces.Find(x => x.CellId == cellData.id);
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
            Workspace ws = mWorkspaces.Find(x => x.CellId == workspace.CellId);
            if (ws != null)
            {
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
                string workspace = Path.Combine(Application.persistentDataPath, WorkspaceRoot, Workspace);
                if (!Directory.Exists(workspace))
                {
                    NewWorkspace();
                }

                string fileName = Path.Combine(Application.persistentDataPath, WorkspaceRoot, Workspace, id);
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

    }
}
