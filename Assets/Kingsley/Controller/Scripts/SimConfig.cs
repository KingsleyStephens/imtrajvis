using UnityEngine;

namespace KController
{


    /**
     * The type of simulation to load
     */
    public enum SimType
    {
        LocalRecord,
        LocalPlayback
    }

    /**
     * Base class for simulation configuration files
     */
    public class SimConfig : ScriptableObject
    {
        public SimType simType = SimType.LocalRecord;
    }

}
