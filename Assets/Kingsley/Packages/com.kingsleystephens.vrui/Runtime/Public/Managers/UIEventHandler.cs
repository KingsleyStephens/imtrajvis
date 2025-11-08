using UnityEngine;

namespace KingsleyStephens
{
    namespace VRUI
    {
        public class UIEventHandler : MonoBehaviour
        {
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
}