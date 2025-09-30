using UnityEngine;
using UnityEngine.UI;
using static KingsleyPhD.TimelineView;

namespace KingsleyPhD
{
    public class Toggle : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Toggle toggle;

        public delegate void ToggleButton(bool isOn);
        public event ToggleButton OnToggleButton;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnActivate()
        {
            if (toggle != null)
            {
                toggle.isOn = !toggle.isOn;
            }

            OnToggleButton?.Invoke(toggle.isOn);
        }
    }

}