using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingsleyStephens
{
    namespace VRUI
    {
        /**
         * A cell view
         */
        public class CellView : MonoBehaviour
        {
            // PRIVATE

            private TimelineView.CellData mCellData;
            private Color mPanelColour;

            // PUBLIC

            public TimelineView.CellData CellData { get { return mCellData; } set { mCellData = value; UpdateView(); } }

            public int CellIndex { get; set; }

            [SerializeField]
            private TextMeshProUGUI label;

            [SerializeField]
            private Image panel;
            [SerializeField]
            private RawImage image;
            [SerializeField]
            private Color selectColor = new Color(1.0f, 1.0f, 1.0f, 0.8f);
            [SerializeField]
            private Color activateColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);

            [SerializeField]
            private GameObject toggle;
            [SerializeField]
            private GameObject radio;
            [SerializeField]
            private Image iconImage;

            public delegate void ActivateCell(int index, TimelineView.CellData cellData, bool selected);
            public event ActivateCell OnActivateCell;

            public delegate void CloseCell(TimelineView.CellData cellData);
            public event CloseCell OnCloseCell;



            private bool IsLabel()
            {
                return mCellData.type == TimelineView.CellData.CellType.Label || mCellData.type == TimelineView.CellData.CellType.IconLabel;
            }


            // PUBLIC

            public void OnSelect()
            {
                if (!IsLabel())
                {
                    if (panel != null)
                    {
                        panel.color = selectColor;
                    }
                }
            }

            public void OnActivate()
            {
                if (!IsLabel())
                {
                    if (panel != null)
                    {
                        bool selected = false;
                        switch (mCellData.type)
                        {
                            case TimelineView.CellData.CellType.Radio:
                            case TimelineView.CellData.CellType.Toggle:
                                {
                                    panel.color = activateColor;

                                    if (mCellData.type == TimelineView.CellData.CellType.Toggle && toggle != null)
                                    {
                                        UnityEngine.UI.Toggle t = toggle.GetComponentInChildren<UnityEngine.UI.Toggle>();
                                        if (t != null)
                                        {
                                            t.isOn = !t.isOn;
                                            selected = t.isOn;
                                        }
                                    }

                                    if (mCellData.type == TimelineView.CellData.CellType.Radio && radio != null)
                                    {
                                        UnityEngine.UI.Toggle t = radio.GetComponentInChildren<UnityEngine.UI.Toggle>();
                                        if (t != null)
                                        {
                                            t.isOn = !t.isOn;
                                            selected = t.isOn;
                                        }
                                    }

                                    break;
                                }
                            case TimelineView.CellData.CellType.Data:
                                {
                                    panel.color = activateColor;
                                    break;
                                }
                        }

                        OnActivateCell?.Invoke(CellIndex, CellData, selected);
                    }
                }
            }

            public void OnClose()
            {
                OnCloseCell?.Invoke(CellData);
            }


            public void ResetState()
            {
                if (!IsLabel())
                {
                    if (panel != null)
                    {
                        panel.color = mPanelColour;
                    }
                }
            }

            // PRIVATE

            /**
             * Start
             */
            void Start()
            {
                if (panel != null)
                {
                    mPanelColour = panel.color;
                }

                Button closeButton = gameObject.GetComponentInChildren<Button>();
                if (closeButton != null)
                {
                    if (CellData != null && CellData.type == TimelineView.CellData.CellType.NewCellButton)
                    {
                        closeButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        closeButton.OnActivateButton += OnClose;
                    }
                }
            }

            /**
             * Update the UI
             */
            private void UpdateView()
            {
                if (mCellData != null)
                {
                    switch (mCellData.type)
                    {
                        case TimelineView.CellData.CellType.Data:
                        case TimelineView.CellData.CellType.Label:
                        case TimelineView.CellData.CellType.IconLabel:
                        case TimelineView.CellData.CellType.Radio:
                        case TimelineView.CellData.CellType.Toggle:
                            {
                                if (label != null)
                                {
                                    label.text = mCellData.name;
                                }

                                if (mCellData.type == TimelineView.CellData.CellType.Data || IsLabel())
                                {
                                    if (toggle != null)
                                    {
                                        toggle.gameObject.SetActive(false);
                                    }

                                    if (radio != null)
                                    {
                                        radio.gameObject.SetActive(false);
                                    }

                                    if (iconImage != null)
                                    {
                                        if (mCellData.type == TimelineView.CellData.CellType.IconLabel)
                                        {
                                            iconImage.sprite = mCellData.icon;
                                        }
                                        else
                                        {
                                            iconImage.gameObject.SetActive(false);
                                        }
                                    }
                                }
                                else if (mCellData.type == TimelineView.CellData.CellType.Radio)
                                {
                                    if (radio != null)
                                    {
                                        UnityEngine.UI.Toggle t = radio.GetComponentInChildren<UnityEngine.UI.Toggle>();
                                        if (t != null)
                                        {
                                            t.isOn = mCellData.selected;
                                        }

                                        if (toggle != null)
                                        {
                                            toggle.gameObject.SetActive(false);
                                        }

                                        if (iconImage != null)
                                        {
                                            iconImage.gameObject.SetActive(false);
                                        }
                                    }
                                }
                                else if (mCellData.type == TimelineView.CellData.CellType.Toggle)
                                {
                                    UnityEngine.UI.Toggle t = toggle.GetComponentInChildren<UnityEngine.UI.Toggle>();
                                    if (t != null)
                                    {
                                        t.isOn = mCellData.selected;
                                    }

                                    if (radio != null)
                                    {
                                        radio.gameObject.SetActive(false);
                                    }
                                    if (iconImage != null)
                                    {
                                        iconImage.gameObject.SetActive(false);
                                    }
                                }

                                if (image != null)
                                {
                                    if (!string.IsNullOrEmpty(mCellData.image) && File.Exists(mCellData.image))
                                    {
                                        byte[] fileData = File.ReadAllBytes(mCellData.image);
                                        Texture2D tex = new Texture2D(2, 2); // size doesn’t matter, will be replaced
                                        tex.LoadImage(fileData);             // auto-resizes the texture dimensions
                                        image.texture = tex;

                                        image.gameObject.SetActive(true);
                                    }
                                    else
                                    {
                                        image.gameObject.SetActive(false);
                                    }
                                }

                                break;
                            }
                        case TimelineView.CellData.CellType.NewCellButton:
                            {
                                if (label != null)
                                {
                                    label.text = "+";
                                }
                                break;
                            }
                    }
                }
            }
        }

    }
}