using KVisualisation;
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




        //[SerializeField]
        //private Vector3 smScale = new Vector3(0.002f, 0.002f, 0.002f);
        //[SerializeField]
        //private Vector3 designScale = new Vector3(0.01f, 0.01f, 0.01f);
        //[SerializeField]
        //private Vector3 fpvScale = new Vector3(1.0f, 1.0f, 1.0f);






        // PUBLIC

        public WorkspaceScene Scene { get { return scene; } }
        public string Id { get; private set; }




        public bool Pinned { get; private set; }
        public bool IsFPV { get; private set; }
        public bool DesignActive { get { return mDesignActive; } }

        public DesignInstanceJSON DesignInstance
        {
            get { return ui?.DesignInstance; }
            set
            {
                if (ui != null)
                {
                    ui.DesignInstance = value;
                }
            }
        }

        public SceneFlowManager SceneFlowManager { get { return scene?.SceneFlowManager; } }



        // PRIVATE DATA

        private DesignInstanceJSON mDesignSpaceInstance;
        private bool mFPVActive = false;



        private bool mDesignActive = false;

        private bool mLockTimeEvents = false;
        private bool mIsStaticTime = false;





        private float mInitialTime = 0.0f;


        // PUBLIC


        /**
         * Initialise
         */
        public void Create(string id, DesignInstanceJSON designInstance, string dataset, string[] sessions, GameObject scenePrefab)
        {
            Id = id;
            scene?.Init(dataset, sessions, scenePrefab);
        }

        /**
         * Apply transform and view properties
         */
        public void Apply(Vector3 rotation, float zoom, float initialTime)
        {
            mInitialTime = initialTime;
            scene?.Transform(rotation, zoom);
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (ui != null)
            {
                ui.Register(DoClose, Menu, ToggleFPV, Pin, TimePlayToggle, DoTimeChange, OnDesignSpaceChange, OnTimeChange);

                SyncTime();
            }
        }

        private void OnDesignSpaceChange()
        {
            if (mSceneFlowManager != null)
            {
                mSceneFlowManager.ConfigDesignSpace(mDesignSpaceInstance);
            }

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
            mIsStaticTime = staticTime;

            if (!mIsStaticTime && mSceneFlowManager != null)
            {
                mSceneFlowManager.ScrubTime = 0.0f;
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

        public void Menu()
        {
            if (scene != null)
            {
                Transform vis = scene.Vis;
                if (vis != null)
                {
                    mDesignActive = !mDesignActive;

                    OnDesignMenu?.Invoke(this);

                    if (mDesignActive)
                    {
                        vis.localScale = designScale;
                        vis.localPosition = Vector3.zero;
                        mSceneFlowManager.SetViewScale(VisScaler.ScaleType.Medium);
                    }
                    else
                    {
                        vis.localScale = smScale;
                        vis.localPosition = Vector3.zero;
                        mSceneFlowManager.SetViewScale(VisScaler.ScaleType.Small);
                    }
                }
            }
        }

        public void Shrink()
        {
            if (mFPVActive)
            {
                ToggleFPV();

                mSceneFlowManager.Resize();
            }
        }

        public void Grow()
        {
            mSceneFlowManager.Resize();
        }

        public void ToggleFPV()
        {
            if (scene != null)
            {
                Transform vis = scene.Vis;
                if (vis != null)
                {
                    mFPVActive = !mFPVActive;

                    if (mFPVActive && designSpace != null)
                    {
                        designSpace.SetActive(false);
                    }
                    else if (!mFPVActive && mDesignActive && designSpace != null)
                    {
                        designSpace.SetActive(true);
                    }

                    if (mFPVActive)
                    {
                        vis.localScale = fpvScale;
                        mSceneFlowManager.SetViewScale(VisScaler.ScaleType.LifeSize);
                    }
                    else if (mDesignActive)
                    {
                        vis.localScale = designScale;
                        mSceneFlowManager.SetViewScale(VisScaler.ScaleType.Medium);
                    }
                    else
                    {
                        vis.localScale = smScale;
                        mSceneFlowManager.SetViewScale(VisScaler.ScaleType.Small);
                    }

                    vis.localPosition = Vector3.zero;

                    if (ui != null)
                    {
                        ui.SetActive(!mFPVActive);
                    }

                    IsFPV = mFPVActive;

                    if (mFPVActive)
                    {
                        OnGoFPV?.Invoke(this);
                    }
                }
            }
            
        }

        public void Pin(bool pinned)
        {
            Pinned = pinned;
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
