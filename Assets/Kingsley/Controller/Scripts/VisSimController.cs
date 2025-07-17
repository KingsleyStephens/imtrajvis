using KSimulation;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace KController
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

        public bool Loaded { get; private set; }

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
                StartCoroutine(LoadSceneCoroutine(name));
            }
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Post-load logic
            mSimPlayer = GameObject.FindAnyObjectByType<SimulationPlayer>();
            if (mSimPlayer != null)
            {

                while (!mSimPlayer.Loaded)
                {
                    yield return null;
                }

                mSimPlayer.Attach(CallOnSpawned, CallOnDestructed);
            }

            Loaded = true;
        }

        public void Attach(OnSpawn onSpawn, OnDestruct onDestruct)
        {
            onSpawned += onSpawn;
            onDestructed += onDestruct;

            mSimPlayer.CheckSpawned((n) => onSpawn(n));
        }

        public void StartSim()
        {
            if (mSimPlayer != null)
            {
                mSimPlayer.OnStart();
            }
        }

        public void Detach(OnSpawn onSpawn, OnDestruct onDestruct)
        {
            onSpawned -= onSpawn;
            onDestructed -= onDestruct;
        }

        private void CallOnSpawned(string name)
        {
            onSpawned?.Invoke(name);
        }

        private void CallOnDestructed(string name)
        {
            onDestructed?.Invoke(name);
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
                if (update != null)
                { 
                    return new MovingObjectUpdate(update.position, update.orientation, update.speed);
                }
            }

            return null;
        }


    }


}