using System.Collections.Generic;
using SpatioTemporalDatabase;
using System.IO;
using UnityEngine;

namespace KSimulation
{

    /**
     * Manager class for recording simulation
     */
    public class SimulationPlayer : MonoBehaviour
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

        public delegate void OnSpawnMovingObject(string name);
        private event OnSpawnMovingObject onSpawned;

        public delegate void OnDestroyMovingObject(string name);
        private event OnDestroyMovingObject onDestroyed;

        [SerializeField]
        private string dbName = "simulation";

        [SerializeField]
        private string sessionName = "session";

        [SerializeField]
        private float startTime = 0.0f;

        [SerializeField]
        private float playbackSpeed = 1.0f;


        private static SimulationPlayer instance;

        public static SimulationPlayer Instance { get { return instance; } }


        private SpatioTemporalDB mSimulationDB;

        private float mSimTime;
        private float mSimStartTime;
        private float mStartTimeReference;


        public float SimTime { get { return mSimTime; } }

        public bool Loaded { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            mSimulationDB = OpenDb();
            SetSimTime(startTime);

            Loaded = true;
        }

        private void OnDestroy()
        {
            mSimulationDB.Close();
        }

        public void Attach(OnSpawnMovingObject onSpawn, OnDestroyMovingObject onDestroy)
        {
            onSpawned += onSpawn;
            onDestroyed += onDestroy;

        }

        public void OnStart()
        {
            SetUp();
        }

        private void SetUp()
        {
            string[] objs = ListSimObjects();
            foreach (string obj in objs)
            {
                onSpawned(obj);
            }
        }

        private void TearDown()
        {
            string[] objs = ListSimObjects();
            foreach (string obj in objs)
            {
                onDestroyed(obj);
            }
        }

        public void CheckSpawned(OnSpawnMovingObject onSpawn)
        {
            string[] objs = ListSimObjects();
            foreach (string obj in objs)
            {
                onSpawn(obj);
            }
        }

        public void ScrubTime(float time)
        {
            TearDown();

            SetSimTime(time);

            SetUp();
        }

        private void SetSimTime(float time)
        {
            mSimTime = mSimStartTime = time;
            mStartTimeReference = Time.time;
        }

        public string[] ListSimObjects()
        {
            return mSimulationDB.ListTable(sessionName);
        }

        public MovingObjectUpdate GetSimObjectUpdate(string name)
        {
            SpatioTemporalDB.MovingObjectPositionUpdate update = mSimulationDB.ReadTable(sessionName, name, mSimTime);
            if (update != null)
            {
                return new MovingObjectUpdate(update.position, update.orientation, update.speed);
            }

            return null;
        }

        // Update is called once per frame
        void Update()
        {
            mSimTime = mSimStartTime + Time.time * playbackSpeed - mStartTimeReference;
        }

        private JsonSpatioTemporalDB OpenDb()
        {
            string path = Path.Combine(Application.persistentDataPath, dbName + ".json");

            JsonSpatioTemporalDB db = new JsonSpatioTemporalDB(path);
            db.CreateOrOpen();
            return db;
        }
    }

}