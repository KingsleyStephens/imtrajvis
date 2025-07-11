using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KCore;

namespace KSimulation
{

    /**
     * Base class for running a simulation
     */
//    [CreateAssetMenu(fileName = "Simulation", menuName = "SATestbed/Simulation", order = 1)]
    public class Simulation : Updateable
    {
        /**
         * Init
         */
        public override void Init()
        {
            KCore.Logger.LogMessage("Simulation LOAD");
            Enabled = true;
        }

        /**
         * Begin
        */
        public override void BeginUpdating()
        {
            KCore.Logger.LogMessage("Simulation BEGIN");
        }

        /**
         * End
         */
        public override void EndUpdating()
        {
            KCore.Logger.LogMessage("Simulation END");
        }

        protected override void DoUpdate(TimeSnapshot time) 
        {
            // KCore.Logger.LogMessage("Simulation update t:{0}, dt:{1}", time.time, time.deltaTime);
        }
    }

}