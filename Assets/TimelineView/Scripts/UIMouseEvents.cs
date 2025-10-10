using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static KVisualisation.DemoKeys;

namespace KingsleyPhD
{
    public class UIMouseEvents : MonoBehaviour
    {
        private Button mCurrCloseButton;
        private CellView mCurrentlySelectedCellView;
        private Toggle mCurrToggleButton;
        private Slider mCurrSlider;
        private UIEventHandler mCurrEventHandler;
        private Vector2 mCurrControlPos = Vector2.zero;

        public UnityEvent OnEsc;
        public UnityEvent OnTimeForward;
        public UnityEvent OnTimeBackward;
        public UnityEvent OnViewForward;
        public UnityEvent OnViewBackward;

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
                OnEsc?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                OnTimeForward?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                OnTimeBackward?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                OnViewForward?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                OnViewBackward?.Invoke();
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
                else if (mCurrEventHandler != null)
                {
                    mCurrEventHandler.Click();
                }
                else if (mCurrSlider != null)
                {
                    mCurrSlider.OnActivate(mCurrControlPos.x);
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
                    mCurrControlPos = hit.transform.InverseTransformPoint(hit.point);
                    mCurrControlPos.x = mCurrControlPos.x / box.size.x + 0.5f;
                    mCurrControlPos.y = mCurrControlPos.y / box.size.y + 0.5f;

                    Button closeButton = hit.transform.GetComponentInParent<Button>();
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
                            Slider hitSlider = hit.transform.GetComponentInParent<Slider>();
                            if (hitSlider != null)
                            {
                                mCurrSlider = hitSlider;
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
                                else
                                {
                                    UIEventHandler handler = hit.transform.GetComponent<UIEventHandler>();
                                    if (handler != null)
                                    {
                                        mCurrEventHandler = handler;
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

                if (mCurrEventHandler != null)
                {
                    mCurrEventHandler = null;
                }
            }
        }
    }

}