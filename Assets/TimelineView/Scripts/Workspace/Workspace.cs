using KVisualisation;
using System.Collections;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using static KingsleyPhD.DesignSpace;
using static KingsleyPhD.TimelineView;

namespace KingsleyPhD
{
    /**
     * Small Multiple Workspace
     */
    public class Workspace : MonoBehaviour
    {
        // EVENTS

        public delegate void Change();
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

        // PUBLIC

        public WorkspaceScene Scene { get { return scene; } }
        public string Id { get; private set; }
        public bool Pinned { get { return (ui != null) ? ui.Pinned : false; } }
        public bool DesignActive { get { return (ui != null) ? ui.DesignActive : false; } }
        public bool IsFPV { get { return (ui != null) ? ui.FPVActive : false; } }
        public DesignInstanceJSON DesignInstance { get { return ui?.DesignInstance; } }
        public SceneFlowManager SceneFlowManager { get { return scene?.SceneFlowManager; } }

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
            OnChange?.Invoke();
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
