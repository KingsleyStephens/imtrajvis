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

        public delegate void OnSpawnMovingObject(int id);
        private event OnSpawnMovingObject onSpawned;

        public delegate void OnDestroyMovingObject(int id);
        private event OnDestroyMovingObject onDestroyed;

        [SerializeField]
        private string dbName = "simulation";

        [SerializeField]
        private string sessionName = "session";

        [SerializeField]
        private float startTime = 0.0f;

        [SerializeField]
        private float playbackSpeed = 1.0f;

        public float PlaybackSpeed { get; set; }

        private static SimulationPlayer instance;

        public static SimulationPlayer Instance { get { return instance; } }


        private SpatioTemporalDB mSimulationDB;

        private float mSimTime;
        private float mSimStartTime;


        public float SimTime { get { return mSimTime; } set { mSimTime = value; } }

        public bool Loaded { get; private set; }

        private void Awake()
        {
            instance = this;
            PlaybackSpeed = playbackSpeed;
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
            PlaybackSpeed = playbackSpeed;
            SetSimTime(startTime);
            SetUp();
        }

        public void OnStop()
        {
            TearDown();
        }

        private void SetUp()
        {
            int[] objs = ListSimObjects();
            foreach (int obj in objs)
            {
                onSpawned(obj);
            }
        }

        private void TearDown()
        {
            int[] objs = ListSimObjects();
            foreach (int obj in objs)
            {
                onDestroyed(obj);
            }
        }

        public void CheckSpawned(OnSpawnMovingObject onSpawn)
        {
            int[] objs = ListSimObjects();
            foreach (int obj in objs)
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
        }

        public int[] ListSimObjects()
        {
            return mSimulationDB.ListTable(sessionName);
        }

        public MovingObjectData GetSimObjectData(int id)
        {
            SpatioTemporalDB.MovingObjectData data = mSimulationDB.ReadTable(sessionName, id);
            if (data != null)
            {
                return new MovingObjectData(data.id, data.parentId, data.objectClass, data.name);
            }

            return null;
        }

        public MovingObjectUpdate GetSimObjectUpdate(int id)
        {
            SpatioTemporalDB.MovingObjectPositionUpdate update = mSimulationDB.ReadTable(sessionName, id, mSimTime);
            if (update != null)
            {
                return new MovingObjectUpdate(update.position, update.orientation, update.speed);
            }

            return null;
        }

        // Update is called once per frame
        void Update()
        {
            mSimTime += Time.deltaTime * PlaybackSpeed;
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