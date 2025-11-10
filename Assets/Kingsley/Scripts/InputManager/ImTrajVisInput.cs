using UnityEngine;
using UnityEngine.Events;
using KingsleyStephens.Core;
using System;
using UnityEngine.InputSystem;

namespace KingsleyStephens
{
    namespace ImTrajVis
    {
        /**
         * Class to handle input events for ImTrajVis
         */
        public class ImTrajVisInput : MonoBehaviour
        {
            // EVENTS

            public UnityEvent OnBack;
            public UnityEvent OnTimeForward;
            public UnityEvent OnTimeBackward;
            public UnityEvent OnViewForward;
            public UnityEvent OnViewBackward;
            public UnityEvent OnNewWorkspace;
            public UnityEvent<int> OnSelectDataset;

            /**
             * Update
             */
            void Update()
            {
                if (OVRInput.GetDown(OVRInput.Button.Start) || Input.GetKeyDown(KeyCode.Escape))
                {
                    Log.LogMessage("On Back");

                    OnBack?.Invoke();
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    Log.LogMessage("On Time Forward");

                    OnTimeForward?.Invoke();
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    Log.LogMessage("On Time Backward");

                    OnTimeBackward?.Invoke();
                }
                else if (Input.GetKeyDown(KeyCode.RightBracket) || OVRInput.GetDown(OVRInput.Button.One))
                {
                    Log.LogMessage("On View Forward");

                    OnViewForward?.Invoke();
                }
                else if (Input.GetKeyDown(KeyCode.LeftBracket) || OVRInput.GetDown(OVRInput.Button.Two))
                {
                    Log.LogMessage("On View Backward");

                    OnViewBackward?.Invoke();
                }
                else if (Input.GetKeyDown(KeyCode.N))
                {
                    Log.LogMessage("On new Workspace");

                    OnNewWorkspace?.Invoke();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    Log.LogMessage("On select dataset 1");

                    OnSelectDataset?.Invoke(0);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    Log.LogMessage("On select dataset 2");

                    OnSelectDataset?.Invoke(1);
                }
            }
        }
    }
}