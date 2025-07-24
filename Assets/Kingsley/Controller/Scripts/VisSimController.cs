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
        public class MovingObjectData
        {
            public MovingObjectData(int id, int parentId, string objectClass, string name)
            {
                this.id = id;
                this.parentId = parentId;
                this.objectClass = objectClass;
                this.name = name;
            }

            public int id;
            public int parentId;
            public string objectClass;
            public string name;
        }

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

        public delegate void OnSpawn(int id);
        private event OnSpawn onSpawned;

        public delegate void OnDestruct(int id);
        private event OnDestruct onDestructed;

        public bool Loaded { get; private set; }

        public SimulationPlayer Player { get { return mSimPlayer; } private set { mSimPlayer = value; } }

        void Start()
        {
            LoadSim();
        }

        public void StartPlay()
        {
            if (mSimPlayer != null)
            {
                mSimPlayer.OnStart();
            }
        }

        public void StopPlay()
        {
            if (mSimPlayer != null)
            {
                mSimPlayer.OnStop();
            }

            onSpawned = null;
            onDestructed = null;
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

            mSimPlayer.CheckSpawned((id) => onSpawn(id));
        }

        public void Detach(OnSpawn onSpawn, OnDestruct onDestruct)
        {
            onSpawned -= onSpawn;
            onDestructed -= onDestruct;
        }

        private void CallOnSpawned(int id)
        {
            onSpawned?.Invoke(id);
        }

        private void CallOnDestructed(int id)
        {
            onDestructed?.Invoke(id);
        }

        public int[] ListSimObjects()
        {
            if (mSimPlayer != null)
            {
                return mSimPlayer.ListSimObjects();
            }

            return null;
        }

        public MovingObjectData GetSimObjectData(int id)
        {
            if (mSimPlayer != null)
            {
                SimulationPlayer.MovingObjectData data = mSimPlayer.GetSimObjectData(id);
                if (data != null)
                {
                    return new MovingObjectData(data.id, data.parentId, data.objectClass, data.name);
                }
            }

            return null;
        }

        public MovingObjectUpdate GetSimObjectUpdate(int id)
        {
            if (mSimPlayer != null)
            {
                SimulationPlayer.MovingObjectUpdate update = mSimPlayer.GetSimObjectUpdate(id);
                if (update != null)
                { 
                    return new MovingObjectUpdate(update.position, update.orientation, update.speed);
                }
            }

            return null;
        }


    }


}