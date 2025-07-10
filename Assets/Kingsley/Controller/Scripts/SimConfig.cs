using UnityEngine;

namespace Controller
{


    /**
     * The type of simulation to load
     */
    public enum SimType
    {
        Local,
        Remote
    }

    /**
     * Base class for simulation configuration files
     */
    public class SimConfig : ScriptableObject
    {
        public SimType simType = SimType.Local;
    }

}
