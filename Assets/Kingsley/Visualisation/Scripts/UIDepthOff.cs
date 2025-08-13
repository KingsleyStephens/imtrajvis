using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;

namespace KVisualisation
{
    public class UIDepthOff : MonoBehaviour
    {
        void Awake()
        {
            // Make sure this runs once at start
            ApplyDepthSettings();
        }

        void LateUpdate()
        {
            ApplyDepthSettings();
        }

        void ApplyDepthSettings()
        {
            // Handle standard UI graphics (Images, RawImages, etc.)
            var graphics = GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                Debug.Log("Graphics " + g.ToString());
                SetDepthOff(g.materialForRendering);
            }

            // Handle TextMeshPro objects
            var tmps = GetComponentsInChildren<TMP_Text>(true);
            foreach (var tmp in tmps)
            {
                SetDepthOff(tmp.fontMaterial);
            }
        }

        void SetDepthOff(Material mat)
        {
            if (!mat) return;

            Debug.Log("Graphics Has material " + mat.ToString());


            //if (mat.HasProperty("_ZWrite"))
            //{
            //    Debug.Log("Graphics ZWrite off");

            //    mat.SetInt("_ZWrite", 0); // Don't write to depth
            //}

           // if (mat.HasProperty("unity_GUIZTestMode"))
            {
                Debug.Log("Graphics unity_GUIZTestMode");

                mat.SetInt("unity_GUIZTestMode", (int)CompareFunction.Always); // Always pass depth test
            }
        }
    }

}
