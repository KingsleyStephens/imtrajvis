using KSimulation;
using UnityEngine;
using UnityEngine.SceneManagement;
using static KSimulation.SimulationPlayer;

namespace Controller
{

    /**
     * The controller in the MVC architecture. Interface between the visualisation and the simulation.
     */
    public class VisSimController : MonoBehaviour
    {
        public class MovingObjectUpdate
        {
            public MovingObjectUpdate(Vector3 position, Quaternion orientation, float speed)
            {
                this.position = position;
                this.orientation = orientation;
                this.speed = speed;
            }

            public Vector3 position;
            public Quaternion orientation;
            public float speed;
        }

        [SerializeField]
        private SimConfig simConfig;


        private SimulationPlayer mSimPlayer;

        public delegate void OnSpawn(string name);
        private event OnSpawn onSpawned;

        public delegate void OnDestruct(string name);
        private event OnDestruct onDestructed;

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
                    case SimType.LocalRecord:
                        {
                            LoadScene((simConfig as LocalSim).scene);
                            break;
                        }
                    case SimType.LocalPlayback:
                        {
                            LoadScene((simConfig as LocalSim).scene);
                            mSimPlayer = GameObject.FindAnyObjectByType<SimulationPlayer>();
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
        private void LoadScene(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                SceneManager.LoadScene(name, LoadSceneMode.Additive);
            }
        }

        public void Attach(OnSpawn onSpawn, OnDestruct onDestruct)
        {
            onSpawned += onSpawn;
            onDestructed += onDestruct;

            mSimPlayer.Attach((n) => onSpawned(n), (n) => onDestructed(n));
        }


        public string[] ListSimObjects()
        {
            if (mSimPlayer != null)
            {
                return mSimPlayer.ListSimObjects();
            }

            return null;
        }

        public MovingObjectUpdate GetSimObjectUpdate(string name)
        {
            if (mSimPlayer != null)
            {
                SimulationPlayer.MovingObjectUpdate update = mSimPlayer.GetSimObjectUpdate(name);
                return new MovingObjectUpdate(update.position, update.orientation, update.speed);
            }

            return null;
        }


    }


}