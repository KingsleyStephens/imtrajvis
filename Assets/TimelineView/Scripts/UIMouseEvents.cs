using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static KVisualisation.DemoKeys;

namespace KingsleyPhD
{
    public class UIMouseEvents : MonoBehaviour
    {
        [SerializeField]
        private bool isVR = false;
        [SerializeField]
        private Transform rightController;

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

        private Vector2 mLastDragPosition;

        public bool SliderSelected { get { return mCurrSlider != null; } }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }
    
        // Update is called once per frame
        void Update()
        {
            MouseRaycast();
            MouseClick();

            if ((isVR && OVRInput.GetDown(OVRInput.Button.Start)) || (!isVR && Input.GetKeyDown(KeyCode.Escape)))
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
            else if (Input.GetKeyDown(KeyCode.RightBracket) || (isVR && OVRInput.GetDown(OVRInput.Button.One)))
            {
                OnViewForward?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.LeftBracket) || (isVR && OVRInput.GetDown(OVRInput.Button.Two)))
            {
                OnViewBackward?.Invoke();
            }
        }

        private void MouseClick()
        {
            if (GetSelectDown())
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
                else if (mCurrSlider != null && !isVR)
                {
                    mCurrSlider.OnActivate(mCurrControlPos.x);
                }
            }

            if (GetSelectUp())
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

        public bool GetSelectDown()
        {
            return (isVR && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger)) || (!isVR && Input.GetMouseButtonDown(0));
        }

        public bool GetSelectUp()
        {
            return (isVR && OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger)) || (!isVR && Input.GetMouseButtonUp(0));
        }

        public Ray GetRay()
        {
            return (isVR) ? new Ray(rightController.position, rightController.forward) : Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        public float GetScroll()
        {
            return (isVR) ? OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y * 0.5f : Input.GetAxis("Mouse ScrollWheel");
        }

        public void BeginDrag()
        {
            mLastDragPosition = mCurrControlPos;
        }

        public Vector2 GetDragDelta()
        {
            return mCurrControlPos - mLastDragPosition;
        }

        private void MouseRaycast()
        {
            Ray ray = GetRay();
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100.0f, 1 << gameObject.layer))
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

                if (mCurrSlider != null)
                {
                    mCurrSlider = null;
                }
            }
        }
    }

}