using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingsleyPhD
{
    public class Slider : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Slider slider;

        public float Value { set { if (slider != null) { slider.value = value; } } }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnActivate(float xValue)
        {
            if (slider != null)
            {
                slider.value = xValue;
            }
        }
    }

}