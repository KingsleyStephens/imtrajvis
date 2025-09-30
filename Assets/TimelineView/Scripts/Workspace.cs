using UnityEngine;

namespace KingsleyPhD
{
    /**
     * Small workspace
     */
    public class Workspace : MonoBehaviour
    {
        [SerializeField]
        private Transform sceneRoot;
        [SerializeField]
        private GameObject ui;
        [SerializeField]
        private CloseButton closeButton;
        [SerializeField]
        private CloseButton menuButton;
        [SerializeField]
        private CloseButton fpvButton;
        [SerializeField]
        private Toggle pinToggle;
        [SerializeField]
        private GameObject designSpace;
        [SerializeField]
        private Vector3 fpvScale = new Vector3(10.0f, 10.0f, 10.0f);


        private DesignInstanceJSON mDesignSpaceInstance;

        public delegate void Change();
        public event Change OnChange;

        public delegate void Close(Workspace workspace);
        public event Close OnClose;

        public delegate void GoFPV(Workspace workspace);
        public event GoFPV OnGoFPV;

        private Vector3 mNormalScale;
        private bool mFPVActive = false;

        public bool Pinned { get; private set; }
        public bool IsFPV { get; private set; }

        public string CellId { get; set; }

        public Transform SceneRoot {  get { return sceneRoot; } }

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
            }
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            mNormalScale = transform.localScale;


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
                fpvButton.OnActivateButton += FPV;
            }

            if (pinToggle != null)
            {
                pinToggle.OnToggleButton += Pin;
            }

            if (designSpace != null)
            {
                designSpace.SetActive(false);

                DesignSpace ds = designSpace.GetComponent<DesignSpace>();
                if (ds != null)
                {
                    ds.OnChange += DoChange;
                }
            }
        }

        void DoChange()
        {
            OnChange?.Invoke();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void DoClose()
        {
            OnClose?.Invoke(this);
        }

        public void Menu()
        {
            if (designSpace != null)
            {
                designSpace.SetActive(!designSpace.activeSelf);
            }
        }

        public void Shrink()
        {
            if (mFPVActive)
            {
                FPV();
            }
        }

        public void FPV()
        {
            if (designSpace != null)
            {
                designSpace.SetActive(false);
            }

            mFPVActive = !mFPVActive;

            if (mFPVActive)
            {
                transform.localScale = fpvScale;
            }
            else
            {
                transform.localScale = mNormalScale;
            }

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
    }

}
