using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KCore;

namespace KSimulation
{
    /**
     * Class to load and initialise the sim
     */
    public class SimulationLoader : MonoBehaviour
    {
        [SerializeField]
        private KCore.Logger.LogType logLevel = KCore.Logger.LogType.All;

        [SerializeField]
        private Simulation simulation;

        /** 
         * Start is called before the first frame update
         */
        void Start()
        {
            KCore.Logger.SetLogLevel(logLevel);

            gameObject.AddComponent<ExecutionManager>();

            if (simulation != null)
            {
                KCore.Logger.LogMessage("Add simulation!");
                ExecutionManager.Instance.Add(simulation);
            }
        }
    }
}