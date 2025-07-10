using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace SATestbed
{
    /**
     * A 0D locationless environment
     */
    [CreateAssetMenu(fileName = "ThreeDEnvironment", menuName = "SATestbed/ThreeDEnvironment", order = 1)]
    public class ThreeDEnvironment : Environment
    {
        // PUBLIC

        // Fields
        public string[] scenes;


        /**
         * Init
         */
        public override void Init()
        {
            Logger.LogMessage("ThreeDEnvironment LOAD");
            Enabled = true;
        }

        /**
         * Begin
        */
        public override void BeginUpdating()
        {
            Logger.LogMessage("ThreeDEnvironment now exists");


            foreach (string s in scenes)
            {
                SceneManager.LoadSceneAsync(s, LoadSceneMode.Additive);
            }
        }

        /**
         * End
         */
        public override void EndUpdating()
        {
            foreach (string s in scenes)
            {
                SceneManager.UnloadSceneAsync(s);
            }
        }

        // PRIVATE





        
    }

}
