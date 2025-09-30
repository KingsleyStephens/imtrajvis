using UnityEngine;
using UnityEngine.Events;

namespace KingsleyPhD
{
    public class UIMouseEvents : MonoBehaviour
    {
        private CloseButton mCurrCloseButton;
        private CellView mCurrentlySelectedCellView;
        private Toggle mCurrToggleButton;

        public UnityEvent OnEsc;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            MouseRaycast();
            MouseClick();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnEsc.Invoke();
            }
        }

        private void MouseClick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (mCurrCloseButton != null)
                {
                    mCurrCloseButton.OnActivate();
                } 
                else if (mCurrentlySelectedCellView != null)
                {
                    mCurrentlySelectedCellView.OnActivate();
                }
                else if (mCurrToggleButton != null)
                {
                    mCurrToggleButton.OnActivate();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (mCurrCloseButton != null)
                {
                    mCurrCloseButton.OnSelect();
                } 
                else if (mCurrentlySelectedCellView != null)
                {
                    mCurrentlySelectedCellView.OnSelect();
                }
            }
        }

        private void MouseRaycast()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100.0f, 1 << LayerMask.NameToLayer("UI")))
            {
                // Check if the object has a BoxCollider
                BoxCollider box = hit.collider as BoxCollider;
                if (box != null)
                {
                    CloseButton closeButton = hit.transform.GetComponentInParent<CloseButton>();
                    if (closeButton != null)
                    {
                        if (mCurrCloseButton == null)
                        {
                            mCurrCloseButton = closeButton;
                            mCurrCloseButton.OnSelect(hit.transform);
                        }
                        else if (mCurrCloseButton != closeButton)
                        {
                            mCurrCloseButton.ResetState();
                            mCurrCloseButton = closeButton;
                            mCurrCloseButton.OnSelect(hit.transform);
                        }
                        else
                        {
                            mCurrCloseButton.OnSelect(hit.transform);
                        }
                    }
                    else
                    {
                        Toggle toggleButton = hit.transform.GetComponentInParent<Toggle>();
                        if (toggleButton != null)
                        {
                            mCurrToggleButton = toggleButton;
                        }
                        else
                        {
                            CellView cellView = hit.transform.GetComponentInParent<CellView>();
                            if (cellView != null)
                            {
                                if (mCurrentlySelectedCellView == null)
                                {
                                    cellView.OnSelect();
                                    mCurrentlySelectedCellView = cellView;
                                }
                                else
                                {
                                    if (mCurrentlySelectedCellView != cellView)
                                    {
                                        mCurrentlySelectedCellView.ResetState();
                                        cellView.OnSelect();
                                        mCurrentlySelectedCellView = cellView;
                                    }
                                }
                            }
                        }
                    }

                }
            }
            else
            {
                if (mCurrCloseButton != null)
                {
                    mCurrCloseButton.ResetState();
                    mCurrCloseButton = null;
                }

                if (mCurrToggleButton != null)
                {
                    mCurrToggleButton = null;
                }

                if (mCurrentlySelectedCellView != null)
                {
                    mCurrentlySelectedCellView.ResetState();
                    mCurrentlySelectedCellView = null;
                }
            }
        }
    }

}