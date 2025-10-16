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
        public event ButtonDelegate OnMenu;
        public event ButtonDelegate OnFPV;
        public event ToggleButtonDelegate OnPin;
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

        // PUBLIC

        public float Time { set { if (!mLockTimeSldier) OnTimeChange?.Invoke(value); } }
        public bool Pause { set { SetTimePlayButton(!value); } }
        public DesignSpace DesignSpace { get { return designSpace; } }
        public DesignInstanceJSON DesignInstance
        {
            get { return mDesignSpaceInstance; }
            set
            {
                mDesignSpaceInstance = value;
                if (designSpace != null)
                {
                    if (designSpace != null)
                    {
                        designSpace.DesignInstance = mDesignSpaceInstance;
                    }
                }

                OnStaticTimeChange?.Invoke(!mDesignSpaceInstance.Query("Space/Time", "Time", "+ 1D animation"));
            }
        }


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
                menuButton.OnActivateButton += () => OnMenu?.Invoke();
            }

            if (fpvButton != null)
            {
                fpvButton.OnActivateButton += () => OnFPV?.Invoke();
            }

            if (pinToggle != null)
            {
                pinToggle.OnToggleButton += (bool on) => OnPin?.Invoke(on);
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
                designSpace.OnTimeChange += (bool staticTime) => OnStaticTimeChange?.Invoke(staticTime);
            }
        }

        public void Register(ButtonDelegate onClose, ButtonDelegate onMenu, ButtonDelegate onFPV, ToggleButtonDelegate onPin, ToggleButtonDelegate onTimePlay, ToggleButtonDelegate onStaticTimeChange, ButtonDelegate onDesignSpaceChange, SliderDelegate onTimeChange)
        {
            OnClose += onClose;
            OnMenu += onMenu;
            OnFPV += onFPV;
            OnPin += onPin;
            OnTimePlay += onTimePlay;
            OnStaticTimeChange += onStaticTimeChange;
            OnDesignSpaceChange += onDesignSpaceChange;
            OnTimeChange += onTimeChange;
        }

        private void MenuChange()
        {
            if (designSpace != null)
            {
                designSpace.gameObject.SetActive(!designSpace.gameObject.activeSelf);
            }
        }

        private void DoTimeChange(bool staticTime)
        {
            if (timeSlider != null)
            {
                timeSlider.gameObject.SetActive(!staticTime);
            }

            if (timePlayButton != null)
            {
                timePlayButton.gameObject.SetActive(!staticTime);
            }
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