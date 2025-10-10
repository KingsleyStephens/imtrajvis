using KVisualisation;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using static KingsleyPhD.DesignSpace;

namespace KingsleyPhD
{
    /**
     * Small workspace
     */
    public class Workspace : MonoBehaviour
    {
        [SerializeField]
        private GameObject sceneFlowManagerPrefab;
        [SerializeField]
        private Transform sceneRoot;
        [SerializeField]
        private Transform viewRoot;
        [SerializeField]
        private GameObject ui;
        [SerializeField]
        private Button closeButton;
        [SerializeField]
        private Button menuButton;
        [SerializeField]
        private Button fpvButton;
        [SerializeField]
        private Toggle pinToggle;
        [SerializeField]
        private Slider timeSlider;
        [SerializeField]
        private Button timePlayButton;
        [SerializeField]
        private GameObject designSpace;
        [SerializeField]
        private Vector3 smScale = new Vector3(0.002f, 0.002f, 0.002f);
        [SerializeField]
        private Vector3 designScale = new Vector3(0.01f, 0.01f, 0.01f);
        [SerializeField]
        private Vector3 fpvScale = new Vector3(10.0f, 10.0f, 10.0f);


        private DesignInstanceJSON mDesignSpaceInstance;

        public delegate void Change();
        public event Change OnChange;

        public delegate void Close(Workspace workspace);
        public event Close OnClose;

        public delegate void GoFPV(Workspace workspace);
        public event GoFPV OnGoFPV;

        public delegate void GoDesignMenu(Workspace workspace);
        public event GoDesignMenu OnDesignMenu;

        private bool mFPVActive = false;

        private SceneFlowManager mSceneFlowManager;

        public bool Pinned { get; private set; }
        public bool IsFPV { get; private set; }
        public bool DesignActive { get { return mDesignActive; } }

        public string CellId { get; set; }

        public Transform SceneRoot { get { return sceneRoot; } }
        public Transform ViewRoot { get { return viewRoot; } }

        public SceneFlowManager SceneFlowManager { get { return mSceneFlowManager; } }

        private VisRoot mVisRoot;
        private bool mDesignActive = false;

        private bool mLockTimeEvents = false;
        private bool mIsStaticTime = false;
        private bool mTimePaused = true;
        public DesignInstanceJSON DesignInstance { 
            get { return mDesignSpaceInstance; }
            set {
                mDesignSpaceInstance = value;
                if (designSpace != null)
                {
                    DesignSpace ds = designSpace.GetComponent<DesignSpace>();
                    if (ds != null)
                    {
                        ds.DesignInstance = mDesignSpaceInstance;
                    }
                }

                DoTimeChange(!mDesignSpaceInstance.Query("Space/Time", "Time", "+ 1D animation"));
            }
        }

        public float Time { set { if (!mLockTimeEvents && mSceneFlowManager != null) { mSceneFlowManager.ScrubTime = value; } } }


        private float mInitialTime = 0.0f;


        public void Init(float time)
        {
            mInitialTime = time;
        }

        private void Awake()
        {
            if (designSpace != null)
            {
                DesignSpace ds = designSpace.GetComponent<DesignSpace>();
                if (ds != null)
                {
                    ds.OnChange += DoChange;
                    ds.OnTimeChange += DoTimeChange;
                }
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            viewRoot.localScale = smScale;
            viewRoot.localPosition = Vector3.zero;

            mVisRoot = viewRoot.GetComponent<VisRoot>();

            if (closeButton != null)
            {
                closeButton.OnActivateButton += DoClose;
            }

            if (menuButton != null)
            {
                menuButton.OnActivateButton += Menu;
            }

            if (fpvButton != null)
            {
                fpvButton.OnActivateButton += ToggleFPV;
            }

            if (pinToggle != null)
            {
                pinToggle.OnToggleButton += Pin;
            }

            if (timePlayButton != null)
            {
                timePlayButton.OnActivateButton += TimePlayToggle;
            }

            if (designSpace != null)
            {
                designSpace.SetActive(false);
            }

            GameObject sceneFlowGO = Instantiate(sceneFlowManagerPrefab);
            mSceneFlowManager = sceneFlowGO.GetComponent<SceneFlowManager>();
            if (mSceneFlowManager != null)
            {
                mSceneFlowManager.Launch(viewRoot);
                mSceneFlowManager.ConfigDesignSpace(mDesignSpaceInstance);
                mSceneFlowManager.SimTime = mInitialTime;
            }

            SyncTimeSlider();
            SetTimePlayButton(mTimePaused);
        }

        void DoChange()
        {
            if (mSceneFlowManager != null)
            {
                mSceneFlowManager.ConfigDesignSpace(mDesignSpaceInstance);
            }

            OnChange?.Invoke();
        }

        void DoTimeChange(bool staticTime)
        {
            if (timeSlider != null)
            {
                mIsStaticTime = staticTime;
                timeSlider.gameObject.SetActive(!mIsStaticTime);
                if (timePlayButton != null)
                {
                    timePlayButton.gameObject.SetActive(!mIsStaticTime);
                }

                if (!mIsStaticTime && mSceneFlowManager != null)
                {
                    mSceneFlowManager.ScrubTime = 0.0f;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!mIsStaticTime && !mTimePaused)
            {
                SyncTimeSlider();
            }
        }

        private void SyncTimeSlider()
        {
            if (timeSlider != null && mSceneFlowManager != null)
            {
                mLockTimeEvents = true;

                timeSlider.Value = mSceneFlowManager.SimRelativeTime;

                mLockTimeEvents = false;
            }
        }

        public void DoClose()
        {
            OnClose?.Invoke(this);

            Destroy(mSceneFlowManager.gameObject);
            mSceneFlowManager = null;
        }

        public void Menu()
        {
            if (designSpace != null)
            {
                mDesignActive = !mDesignActive;
                designSpace.SetActive(mDesignActive);

                OnDesignMenu?.Invoke(this);

                if (mDesignActive)
                {
                    viewRoot.localScale = designScale;
                    viewRoot.localPosition = Vector3.zero;
                }
                else
                {
                    viewRoot.localScale = smScale;
                    viewRoot.localPosition = Vector3.zero;
                }

                if (mVisRoot != null)
                {
                    mVisRoot.OnResize();
                }
            }
        }

        public void Shrink()
        {
            if (mFPVActive)
            {
                ToggleFPV();

                if (mVisRoot != null)
                {
                    mVisRoot.OnResize();
                }
                mSceneFlowManager.Resize();
            }
        }

        public void Grow()
        {
            if (mVisRoot != null)
            {
                mVisRoot.OnResize();
            }
            mSceneFlowManager.Resize();
        }

        public void ToggleFPV()
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
                viewRoot.localScale = fpvScale;
            }
            else if (mDesignActive)
            {
                viewRoot.localScale = designScale;
            }
            else
            {
                viewRoot.localScale = smScale;
            }

            viewRoot.localPosition = Vector3.zero;

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

        public void Pin(bool pinned)
        {
            Pinned = pinned;
        }

        public void TimePlayToggle()
        {
            mTimePaused = !mTimePaused;

            SetTimePlayButton(mTimePaused);
        }

        private void SetTimePlayButton(bool paused)
        {
            if (mSceneFlowManager != null)
            {
                mSceneFlowManager.TimePause(paused);
            }

            if (timePlayButton != null)
            {
                TextMeshProUGUI text = timePlayButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = (paused) ? ">" : "||";
                }
            }
        }
    }

}
