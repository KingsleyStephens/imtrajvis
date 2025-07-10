using UnityEngine;

namespace Controller
{

    /**
     * Data class to load a local simulation
     */
    [CreateAssetMenu(fileName = "LocalSim", menuName = "Kingsley/Controller/LocalSim", order = 1)]
    public class LocalSim : SimConfig
    {
        public string scene;
    }

}
