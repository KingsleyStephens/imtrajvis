using KSimulation;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace KController
{

    /**
     * The controller in the MVC architecture. Interface between the visualisation and the simulation.
     */
    public class VisSimController : MonoBehaviour
    {
        public class MovingObjectData
        {
            public MovingObjectData(int id, int parentId, string objectClass, string name, Vector2 timePeriod)
            {
                this.id = id;
                this.parentId = parentId;
                this.objectClass = objectClass;
                this.name = name;
                this.timePeriod = timePeriod;
            }

            public int id;
            public int parentId;
            public string objectClass;
            public string name;
            public Vector2 timePeriod;
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
        
        [SerializeField]
        private GameObject simPlayerPrefab;

        private List<SimulationPlayer> mSimPlayers = new List<SimulationPlayer>();

        public delegate void OnSpawn(int playerId, int id);
        private event OnSpawn onSpawned;

        public delegate void OnDestruct(int playerId, int id);
        private event OnDestruct onDestructed;

        public delegate void OnTimeScrub(int playerId, float fromTime, float toTime);
        private event OnTimeScrub onTimeScrubed;

        public bool Loaded { get; private set; }

        public List<SimulationPlayer> Players { get { return mSimPlayers; } }

        void Start()
        {
        }

        public void StartPlay()
        {
            foreach (var sp in mSimPlayers)
            {
                sp.OnStart();
            }
        }

        public void StopPlay()
        {
            foreach (var sp in mSimPlayers)
            {
                sp.OnStop();
            }

            onSpawned = null;
            onDestructed = null;
            onTimeScrubed = null;
        }

        /**
         * Load the simulation from the configured settings
         */
        public void LoadSim(string dataset = null, string[] session = null)
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
                            for (int i = 0; i < session.Length; ++i)
                            {
                                LoadSimPlayer(i, dataset, session[i]);
                            }
                            Loaded = true;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
        }

        public void UnloadSim()
        {
            foreach (var p in mSimPlayers)
            {
                Destroy(p.gameObject);
            }
            mSimPlayers.Clear();
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

        private void LoadSimPlayer(int id, string dataset = null, string session = null)
        {
            GameObject player = Instantiate(simPlayerPrefab);
            player.name = "SimPlayer" + id.ToString();
            SimulationPlayer simPlayer = player.GetComponent<SimulationPlayer>();
            if (simPlayer != null)
            {
                if (dataset != null && session != null)
                {
                    simPlayer.dbName = dataset;
                    simPlayer.sessionName = session;
                }

                simPlayer.Id = id;
                mSimPlayers.Add(simPlayer);
                simPlayer.Load();
                simPlayer.Attach(CallOnSpawned, CallOnDestructed, CallOnTimeScrubbed);
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
            SimulationPlayer simPlayer = GameObject.FindAnyObjectByType<SimulationPlayer>();
            if (simPlayer != null)
            {
                while (!simPlayer.Loaded)
                {
                    yield return null;
                }

                simPlayer.Attach(CallOnSpawned, CallOnDestructed, CallOnTimeScrubbed);
            }

            Loaded = true;
        }

        public void Attach(OnSpawn onSpawn, OnDestruct onDestruct, OnTimeScrub onScrub)
        {
            onSpawned += onSpawn;
            onDestructed += onDestruct;
            onTimeScrubed += onScrub;

            foreach (var p in mSimPlayers)
            {
                p.CheckSpawned((playerId, id) => onSpawn(playerId, id));
            }
        }

        public void Detach(OnSpawn onSpawn, OnDestruct onDestruct, OnTimeScrub onScrub)
        {
            onSpawned -= onSpawn;
            onDestructed -= onDestruct;
            onTimeScrubed -= onScrub;
        }

        private void CallOnSpawned(int playerId, int id)
        {
            onSpawned?.Invoke(playerId, id);
        }

        private void CallOnDestructed(int playerId, int id)
        {
            onDestructed?.Invoke(playerId,id);
        }

        private void CallOnTimeScrubbed(int playerId, float fromTime, float toTime)
        {
            onTimeScrubed?.Invoke(playerId, fromTime, toTime);
        }

        public int[] ListSimObjects(int playerId)
        {
            if (playerId >= 0 && playerId < mSimPlayers.Count)
            {
                return mSimPlayers[playerId].ListSimObjects();
            }

            return null;
        }

        public MovingObjectData GetSimObjectData(int playerId, int id)
        {
            if (playerId < mSimPlayers.Count)
            {
                SimulationPlayer.MovingObjectData data = mSimPlayers[playerId].GetSimObjectData(id);
                if (data != null)
                {
                    return new MovingObjectData(data.id, data.parentId, data.objectClass, data.name, data.timePeriod);
                }
            }

            return null;
        }

        public MovingObjectUpdate GetSimObjectUpdate(int playerId, int id)
        {
            if (playerId < mSimPlayers.Count)
            {
                SimulationPlayer.MovingObjectUpdate update = mSimPlayers[playerId].GetSimObjectUpdate(id);
                if (update != null)
                { 
                    return new MovingObjectUpdate(update.position, update.orientation, update.speed);
                }
            }

            return null;
        }


    }


}