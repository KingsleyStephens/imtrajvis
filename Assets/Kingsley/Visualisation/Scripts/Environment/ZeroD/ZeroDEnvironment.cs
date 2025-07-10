using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SATestbed
{
    /**
     * A 0D locationless environment
     */
    [CreateAssetMenu(fileName = "ZeroDEnvironment", menuName = "SATestbed/ZeroDEnvironment", order = 1)]
    public class ZeroDEnvironment : Environment
    {
        // PUBLIC

        // UI
        public GameObject uiPrefab;

        /**
         * Init
         */
        public override void Init()
        {
            Logger.LogMessage("ZeroDEnvironment LOAD");
            Enabled = true;
        }

        /**
         * Begin
        */
        public override void BeginUpdating()
        {
            Logger.LogMessage("ZeroDEnvironment now exists");

            mUI = Instantiate(uiPrefab);
            UIDocument doc = mUI.GetComponent<UIDocument>();
            if (doc != null)
            {
                Label l = doc.rootVisualElement.Q("title") as Label;
                l.text = "Hello ZeroD!";
            }

        }

        /**
         * End
         */
        public override void EndUpdating()
        {
            Destroy(mUI);
        }

        // PRIVATE

        private GameObject mUI;
    }

}
