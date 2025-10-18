using TMPro;
using UnityEditor;
using UnityEngine;
using static KingsleyPhD.DesignSpace;
using static KingsleyPhD.Workspace;

namespace KingsleyPhD
{
    /**
     * Class to organise a workspace UI
     */
    public class WorkspaceUI : MonoBehaviour
    {
        // EVENTS

        public delegate void ButtonDelegate();
        public delegate void ToggleButtonDelegate(bool on);
        public delegate void SliderDelegate(float relativeValue);

        public event ButtonDelegate OnClose;
        public event ToggleButtonDelegate OnMenu;
        public event ToggleButtonDelegate OnFPV;
        public event ToggleButtonDelegate OnTimePlay;
        public event ToggleButtonDelegate OnStaticTimeChange;
        public event ButtonDelegate OnDesignSpaceChange;
        public event SliderDelegate OnTimeChange;

        // DATA

        [Header("Righthand button strip")]
        [SerializeField]
        private Button closeButton;
        [SerializeField]
        private Button menuButton;
        [SerializeField]
        private Button fpvButton;
        [SerializeField]
        private Toggle pinToggle;

        [Header("Time controls")]
        [SerializeField]
        private Slider timeSlider;
        [SerializeField]
        private Button timePlayButton;

        [Header("Design space")]
        [SerializeField]
        private DesignSpace designSpace;

        // PRIVATE

        private bool mLockTimeSldier = false;
        private bool mTimePaused = true;
        private DesignInstanceJSON mDesignSpaceInstance;
        private bool mIsPinned = false;
        private bool mIsStaticTime = false;
        private bool mDesignActive = false;
        private bool mFPVActive = false;

        // PUBLIC

        public float Time { set { if (!mLockTimeSldier) OnTimeChange?.Invoke(value); } }
        public bool Pause { set { SetTimePlayButton(!value); } }
        public DesignSpace DesignSpace { get { return designSpace; } }
        public DesignInstanceJSON DesignInstance { get { return mDesignSpaceInstance; } }
        public bool Pinned { get { return mIsPinned; } }
        public bool StaticTime { get { return mIsStaticTime; } }
        public bool DesignActive { get { return mDesignActive; } }
        public bool FPVActive { get { return mFPVActive; } set { mFPVActive = value; OnFPV?.Invoke(mFPVActive); } }


        public void SetTime(float time) 
        {
            if (timeSlider != null)
            {
                mLockTimeSldier = true;
                timeSlider.Value = time;
                mLockTimeSldier = false;
            }
         }

        // PRIVATE

        /**
         * Start
         */
        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.OnActivateButton += () => OnClose?.Invoke();
            }

            if (menuButton != null)
            {
                menuButton.OnActivateButton += MenuChange;
                menuButton.OnActivateButton += () => OnMenu?.Invoke(mDesignActive);
            }

            if (fpvButton != null)
            {
                fpvButton.OnActivateButton += () => FPVActive = !FPVActive;
            }

            if (pinToggle != null)
            {
                pinToggle.OnToggleButton += (bool on) => mIsPinned = on;
            }

            if (timePlayButton != null)
            {
                timePlayButton.OnActivateButton += () => { mTimePaused = !mTimePaused; SetTimePlayButton(!mTimePaused);  };
                timePlayButton.OnActivateButton += () => OnTimePlay?.Invoke(!mTimePaused);

                SetTimePlayButton(!mTimePaused);
            }

            if (designSpace != null)
            {
                designSpace.gameObject.SetActive(false);

                designSpace.OnChange += () => OnDesignSpaceChange?.Invoke();
                designSpace.OnTimeChange += DoTimeChange;
            }
        }

        public void Init(DesignInstanceJSON designInstance, Camera mainCamera)
        {
            mDesignSpaceInstance = designInstance;
            if (designSpace != null)
            {
                designSpace.DesignInstance = mDesignSpaceInstance;
            }

            mIsStaticTime = !mDesignSpaceInstance.Query("Space/Time", "Time", "+ 1D animation");
            DoTimeChange(mIsStaticTime);

            if (timeSlider != null)
            {
                Canvas c = timeSlider.GetComponent<Canvas>();
                if (c != null)
                {
                    c.worldCamera = mainCamera;
                }
            }
        }

        public void Register(ButtonDelegate onClose, ToggleButtonDelegate onMenu, ToggleButtonDelegate onFPV, ToggleButtonDelegate onTimePlay, ToggleButtonDelegate onStaticTimeChange, ButtonDelegate onDesignSpaceChange, SliderDelegate onTimeChange)
        {
            OnClose += onClose;
            OnMenu += onMenu;
            OnFPV += onFPV;
            OnTimePlay += onTimePlay;
            OnStaticTimeChange += onStaticTimeChange;
            OnDesignSpaceChange += onDesignSpaceChange;
            OnTimeChange += onTimeChange;
        }

        private void MenuChange()
        {
            mDesignActive = !mDesignActive;

            if (designSpace != null)
            {
                designSpace.gameObject.SetActive(mDesignActive);
            }
        }

        private void DoTimeChange(bool staticTime)
        {
            mIsStaticTime = staticTime;

            if (timeSlider != null)
            {
                timeSlider.gameObject.SetActive(!mIsStaticTime);
            }

            if (timePlayButton != null)
            {
                timePlayButton.gameObject.SetActive(!mIsStaticTime);
            }

            OnStaticTimeChange?.Invoke(mIsStaticTime);
        }

        private void SetTimePlayButton(bool playing)
        {
            if (timePlayButton != null)
            {
                TextMeshProUGUI text = timePlayButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = (!playing) ? ">" : "||";
                }
            }
        }
    }
}