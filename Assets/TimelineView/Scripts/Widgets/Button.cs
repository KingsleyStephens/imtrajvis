using UnityEngine;
using UnityEngine.UI;
using static KingsleyPhD.TimelineView;

namespace KingsleyPhD
{
    public class Button : MonoBehaviour
    {
        [SerializeField]
        private Image panel;
        [SerializeField]
        private Color selectColor = new Color(1.0f, 1.0f, 1.0f, 0.8f);
        [SerializeField]
        private Color activateColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);

        [SerializeField]
        private GameObject confirmDialog;

        private Color mPanelColour;

        public delegate void ActivateButton();
        public event ActivateButton OnActivateButton;

        private Transform currentHit;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (panel != null)
            {
                mPanelColour = panel.color;
            }

            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnSelect(Transform hit = null)
        {
            if (panel != null)
            {
                panel.color = selectColor;
            }

            if (hit != null)
            {
                currentHit = hit;
            }
        }

        public void OnActivate()
        {
            if (confirmDialog != null)
            {
                if (currentHit.gameObject.name == "CloseButton")
                {
                    if (panel != null)
                    {
                        panel.color = activateColor;

                        if (confirmDialog != null && !confirmDialog.activeSelf)
                        {
                            confirmDialog.SetActive(true);
                        }
                    }
                }
                else if (currentHit.gameObject.name == "Confirm")
                {
                    OnConfirm();
                }
                else
                {
                    ResetState();
                }
            }
            else
            {
                if (panel != null)
                {
                    panel.color = activateColor;
                }

                OnActivateButton?.Invoke();
            }
        }

        public void OnConfirm()
        {
            OnActivateButton?.Invoke();

            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
        }

        public void ResetState()
        {
            if (panel != null)
            {
                panel.color = mPanelColour;
            }

            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
        }
    }

}