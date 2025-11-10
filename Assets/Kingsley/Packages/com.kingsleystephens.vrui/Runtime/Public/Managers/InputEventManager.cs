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

            // PRIVATE

            public Vector2 mLastDragPosition;

            private Button mCurrCloseButton;
            private CellView mCurrentlySelectedCellView;
            private Toggle mCurrToggleButton;
            private Slider mCurrSlider;
            private UIEventHandler mCurrEventHandler;
            private Vector2 mCurrControlPos = Vector2.zero;
                       
            private bool mSelectDown = false;

            // PUBLIC

            public bool SliderSelected { get { return mCurrSlider != null; } }

            public IInputInterface Input { get { return inputInterface; } }

            // PRIVATE

            /**
             * Update
             */
            void Update()
            {
                Raycast();
                TestSelect();
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
                    if (!mSelectDown)
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

                        mSelectDown = true;
                    }
                }

                if (inputInterface.GetSelectUp())
                {
                    mSelectDown = false;

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