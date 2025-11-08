using KVisualisation;
using System;
using System.Collections;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using static KingsleyPhD.DesignSpace;
using KingsleyStephens.ImTrajVis;
using static KingsleyStephens.VRUI.TimelineView;

namespace KingsleyPhD
{
    /**
     * Small Multiple Workspace
     */
    public class Workspace : MonoBehaviour
    {
        // EVENTS

        public delegate void Change(Workspace workspace);
        public event Change OnChange;

        public delegate void Close(Workspace workspace);
        public event Close OnClose;

        public delegate void GoFPV(Workspace workspace);
        public event GoFPV OnGoFPV;

        public delegate void GoDesignMenu(Workspace workspace);
        public event GoDesignMenu OnDesignMenu;

        // PUBLIC DATA

        [Header("Workspace transforms")]
        [SerializeField]
        private WorkspaceScene scene;       // The scene for the vis

        [SerializeField]
        private WorkspaceUI ui;             // The UI

        [SerializeField]
        private Transform designViewAnchor;

        [SerializeField]
        private Transform snapshotAnchor;

        [SerializeField]
        private float telemetryLogDelta = 0.1f;

        public Camera mainCamera;
        public Transform leftController;
        public Transform rightController;

        // PUBLIC

        public WorkspaceScene Scene { get { return scene; } }
        public WorkspaceUI UI { get { return ui; } }
        public string Id { get; private set; }
        public bool Pinned { get { return (ui != null) ? ui.Pinned : false; } }
        public bool DesignActive { get { return (ui != null) ? ui.DesignActive : false; } }
        public bool IsFPV { get { return (ui != null) ? ui.FPVActive : false; } }
        public DesignInstanceJSON DesignInstance { get { return ui?.DesignInstance; } }
        public SceneFlowManager SceneFlowManager { get { return scene?.SceneFlowManager; } }
        public Transform DesignViewAnchor { get { return designViewAnchor; } }
        public Transform SnapshotAnchor { get { return snapshotAnchor; } }

        // PRIVATE

        private float mLastTelemetryLog = 0.0f;

        // PUBLIC

        /**
         * Initialise
         */
        public void Create(string id, DesignInstanceJSON designInstance, string dataset, string[] sessions, GameObject scenePrefab, Camera mainCamera)
        {
            Id = id;

            ui?.Init(designInstance, mainCamera);
            scene?.Init(dataset, sessions, scenePrefab);

            OnDesignSpaceChange();
        }

        /**
         * Apply transform and view properties
         */
        public void Apply(Vector3 rotation, float zoom, float initialTime)
        {
            scene?.Transform(rotation, zoom);
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (ui != null)
            {
                ui.Register(DoClose, Menu, ToggleFPV, TimePlayToggle, DoTimeChange, OnDesignSpaceChange, OnTimeChange);

                SyncTime();

                scene?.Resize();
            }
        }

        private void OnDesignSpaceChange()
        {
            scene?.Configure(ui?.DesignInstance);
            OnChange?.Invoke(this);
        }

        public void OnTimeChange(float time)
        { 
            if (scene != null)
            {
                scene.ScrubTime = time;
            }
        } 

        void DoTimeChange(bool staticTime)
        {
            // Changing back to dynamic time, do a reset
            if (!staticTime && scene != null)
            {
                scene.ScrubTime = 0.0f;
            }
        }

        // Update is called once per frame
        void Update()
        {
            SyncTime();

            if (IsFPV)
            {
                float time = Time.time;
                if (time - mLastTelemetryLog > telemetryLogDelta)
                {
                    string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    KingsleyStephens.ImTrajVis.Logger.LogTelemetry(timestamp, SceneFlowManager.SimTime, mainCamera.name, mainCamera.transform.position, mainCamera.transform.rotation.eulerAngles);
                    KingsleyStephens.ImTrajVis.Logger.LogTelemetry(timestamp, SceneFlowManager.SimTime, leftController.gameObject.name, leftController.position, leftController.rotation.eulerAngles);
                    KingsleyStephens.ImTrajVis.Logger.LogTelemetry(timestamp, SceneFlowManager.SimTime, rightController.gameObject.name, rightController.position, rightController.rotation.eulerAngles);

                    mLastTelemetryLog = time;
                }
            }
        }

        private void SyncTime()
        {
            if (ui != null && scene != null)
            {
                ui.SetTime(scene.RelativeTime);
            }
        }

        public void DoClose()
        {
            OnClose?.Invoke(this);

            if (scene != null)
            {
                scene.Shutdown();
            }
        }

        public void Menu(bool active)
        {
            OnDesignMenu?.Invoke(this);

            scene?.Resize();
        }

        public void Shrink()
        {
            if (ui != null && ui.FPVActive)
            {
                ui.FPVActive = false;
                ToggleFPV(ui.FPVActive);
                scene?.Resize();
            }
        }

        public void Grow()
        {
            scene?.Resize();
        }

        public void ToggleFPV(bool fpvActive)
        {
            if (scene != null && ui != null)
            {
                if (ui != null)
                {
                    ui.gameObject.SetActive(!fpvActive);
                }

                scene?.Resize();

                if (fpvActive)
                {
                    OnGoFPV?.Invoke(this);
                }
            }
        }

        public void TimePlayToggle(bool playing)
        {
            if (scene != null)
            {
                scene.Pause = !playing;
            }
        }
    }

}
