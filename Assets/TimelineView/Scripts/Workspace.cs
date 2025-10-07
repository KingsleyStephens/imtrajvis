using KVisualisation;
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
        private Transform viewRoot;
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

        public bool Pinned { get; private set; }
        public bool IsFPV { get; private set; }

        public string CellId { get; set; }

        public Transform SceneRoot {  get { return sceneRoot; } }
        public Transform ViewRoot { get { return viewRoot; } }

        private VisRoot mVisRoot;
        private bool mDesignActive = false;

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

                if (mVisRoot  != null)
                {
                    mVisRoot.OnResize();
                }
            }
        }

        public void Grow()
        {
            if (mVisRoot != null)
            {
                mVisRoot.OnResize();
            }
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
    }

}
