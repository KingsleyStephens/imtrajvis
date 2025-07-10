using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SATestbed
{
    /**
     * Class to load and initialise the sim
     */
    public class SimulationLoader : MonoBehaviour
    {
        [SerializeField]
        private Logger.LogType logLevel = Logger.LogType.All;

        [SerializeField]
        private Simulation simulation;

        /** 
         * Start is called before the first frame update
         */
        void Start()
        {
            Logger.SetLogLevel(logLevel);

            gameObject.AddComponent<ExecutionManager>();

            if (simulation != null)
            {
                Logger.LogMessage("Add simulation!");
                ExecutionManager.Instance.Add(simulation);
            }
        }
    }
}