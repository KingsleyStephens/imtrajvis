using UnityEngine;
using UnityEngine.SceneManagement;

namespace Controller
{

    /**
     * The controller in the MVC architecture. Interface between the visualisation and the simulation.
     */
    public class VisSimController : MonoBehaviour
    {
        [SerializeField]
        private SimConfig simConfig;


        void Start()
        {
            LoadSim();
        }

        /**
         * Load the simulation from the configured settings
         */
        public void LoadSim()
        {
            if (simConfig != null)
            {
                switch (simConfig.simType)
                {
                    case SimType.Local:
                        {
                            LoadLocalSim((simConfig as LocalSim).scene);
                            break;
                        }
                    case SimType.Remote:
                        {

                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
        }

        /**
         * Load a local simulation
         * 
         * @param name The scene name to additively load
         */
        private void LoadLocalSim(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                SceneManager.LoadScene(name, LoadSceneMode.Additive);
            }
        }

    }


}