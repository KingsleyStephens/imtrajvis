using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using UnityEngine;

namespace KingsleyPhD
{
    public class UIEventHandler : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }


        public void Click()
        {
            UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick?.Invoke();
            }
        }
    }
}