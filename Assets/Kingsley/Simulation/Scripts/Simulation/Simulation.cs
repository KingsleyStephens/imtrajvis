using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SATestbed
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
            Logger.LogMessage("Simulation LOAD");
            Enabled = true;
        }

        /**
         * Begin
        */
        public override void BeginUpdating()
        {
            Logger.LogMessage("Simulation BEGIN");
        }

        /**
         * End
         */
        public override void EndUpdating()
        {
            Logger.LogMessage("Simulation END");
        }

        protected override void DoUpdate(TimeSnapshot time) 
        {
            // Logger.LogMessage("Simulation update t:{0}, dt:{1}", time.time, time.deltaTime);
        }
    }

}