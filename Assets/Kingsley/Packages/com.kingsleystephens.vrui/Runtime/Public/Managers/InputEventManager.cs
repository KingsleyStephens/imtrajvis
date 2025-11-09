using KingsleyStephens.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using KCore = KingsleyStephens.Core;

namespace KingsleyStephens
{
    namespace VRUI
    {

        /**
         * Manager class for input devices and UI
         */
        public class InputEventManager : SingletonMonoBehaviour<InputEventManager>
        {
            // FIELDS

            [SerializeField]
            private InputInterfaceMonoBehaviour inputInterface;

            //[SerializeField]
            //private Transform rightController;

            // PRIVATE

            private Vector2 mLastDragPosition;

            private Button mCurrCloseButton;
            private CellView mCurrentlySelectedCellView;
            private Toggle mCurrToggleButton;
            private Slider mCurrSlider;
            private UIEventHandler mCurrEventHandler;
            private Vector2 mCurrControlPos = Vector2.zero;

            // PUBLIC

            public bool SliderSelected { get { return mCurrSlider != null; } }

            public IInputInterface Input { get { return inputInterface; } }

            // PRIVATE

            private void Start()
            {
            }

            /**
             * Update
             */
            void Update()
            {
                Raycast();
                TestSelect();

                //if ((isVR && OVRInput.GetDown(OVRInput.Button.Start)) || (!isVR && Input.GetKeyDown(KeyCode.Escape)))
                //{
                //    OnEsc?.Invoke();

                //    KCore.Log.LogMessage("On ESCAPE");
                //}
                //else if (Input.GetKeyDown(KeyCode.RightArrow))
                //{
                //    OnTimeForward?.Invoke();

                //    KCore.Log.LogMessage("On Time Forward");
                //}
                //else if (Input.GetKeyDown(KeyCode.LeftArrow))
                //{
                //    OnTimeBackward?.Invoke();

                //    KCore.Log.LogMessage("On Time Backward");
                //}
                //else if (Input.GetKeyDown(KeyCode.RightBracket) || (isVR && OVRInput.GetDown(OVRInput.Button.One)))
                //{
                //    OnViewForward?.Invoke();

                //    KCore.Log.LogMessage("On View Forward");
                //}
                //else if (Input.GetKeyDown(KeyCode.LeftBracket) || (isVR && OVRInput.GetDown(OVRInput.Button.Two)))
                //{
                //    OnViewBackward?.Invoke();

                //    KCore.Log.LogMessage("On View Backward");
                //}
            }

            /**
             * Test for a select input
             */
            private void TestSelect()
            {
                if (inputInterface == null)
                {
                    return;
                }

                if (inputInterface.GetSelectDown())
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

                if (inputInterface.GetSelectUp())
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

            //public bool GetSelectDown()
            //{
            //   // return (isVR);// && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger)) || (!isVR && Input.GetMouseButtonDown(0));

            //    return inputInterface != null && inputInterface.GetSelectDown();
            //}

            //public bool GetSelectUp()
            //{
            //    // return (isVR);// && OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger)) || (!isVR && Input.GetMouseButtonUp(0));
            //    return inputInterface != null && inputInterface.GetSelectUp();
            //}

            //public Ray GetRay()
            //{
            //    return (isVR) ? new Ray(rightController.position, rightController.forward) : Camera.main.ScreenPointToRay(Input.mousePosition);
            //}

            //public float GetScroll()
            //{
            //    return 0.0f;// (isVR) ? OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y * 0.5f : Input.GetAxis("Mouse ScrollWheel");
            //}

            public void BeginDrag()
            {
                mLastDragPosition = mCurrControlPos;
            }

            public Vector2 GetDragDelta()
            {
                return mCurrControlPos - mLastDragPosition;
            }

            private void Raycast()
            {
                if (inputInterface == null)
                {
                    return;
                }

                Ray[] rays = inputInterface.GetPointerRays();
                RaycastHit hit;
                if (Physics.Raycast(rays[0], out hit, 100.0f, 1 << gameObject.layer))
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
}