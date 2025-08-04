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

        public delegate void OnSpawnMovingObject(int id);
        private event OnSpawnMovingObject onSpawned;

        public delegate void OnDestroyMovingObject(int id);
        private event OnDestroyMovingObject onDestroyed;

        public delegate void OnTimeScrub(float fromTime, float toTime);
        private event OnTimeScrub onTimeScrubbed;

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

        private Vector2 mTimeExtents = Vector2.zero;

        public float SimTime { get { return mSimTime; } set {
                float oldValue = mSimTime;
                mSimTime = value;
                onTimeScrubbed(oldValue, mSimTime);
            } }

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

            int[] objs = ListSimObjects();
            foreach (int obj in objs)
            {
                UpdateTimeExtents(mSimulationDB.ReadTableTimePeriod(sessionName, obj));
            }

            Loaded = true;
        }

        private void UpdateTimeExtents(Vector2 objPeriod)
        {
            if (mTimeExtents.x > objPeriod.x)
            {
                mTimeExtents.x = objPeriod.x;
            }
            if (mTimeExtents.y < objPeriod.y)
            {
                mTimeExtents.y = objPeriod.y;
            }
        }

        private void OnDestroy()
        {
            mSimulationDB.Close();
        }

        public void Attach(OnSpawnMovingObject onSpawn, OnDestroyMovingObject onDestroy, OnTimeScrub onScrub)
        {
            onSpawned += onSpawn;
            onDestroyed += onDestroy;
            onTimeScrubbed += onScrub;
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
                UpdateTimeExtents(mSimulationDB.ReadTableTimePeriod(sessionName, obj));

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
            if (time >= mTimeExtents.x && time <= mTimeExtents.y)
            {
                TearDown();

                SetSimTime(time);

                SetUp();
            }
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
                Vector2 timePeriod = mSimulationDB.ReadTableTimePeriod(sessionName, id);

                return new MovingObjectData(data.id, data.parentId, data.objectClass, data.name, timePeriod);
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

        public MovingObjectUpdate[] SampleObjectPath(int id, float startTime, float endTime, float timeStep)
        {
            List<MovingObjectUpdate> samples = new List<MovingObjectUpdate>();

            if (endTime < 0.0f)
            {
                float t = startTime;
                SpatioTemporalDB.MovingObjectPositionUpdate update = mSimulationDB.ReadTable(sessionName, id, t);
                while (update != null)
                {
                    samples.Add(new MovingObjectUpdate(update.position, update.orientation, update.speed));
                    t += timeStep;
                    update = mSimulationDB.ReadTable(sessionName, id, t);
                }
            }
            else
            {
                for (float t = startTime; t <= endTime; t += timeStep)
                {
                    SpatioTemporalDB.MovingObjectPositionUpdate update = mSimulationDB.ReadTable(sessionName, id, t);
                    if (update != null)
                    {
                        samples.Add(new MovingObjectUpdate(update.position, update.orientation, update.speed));
                    }
                }
            }

            return samples.ToArray();
        }

        public MovingObjectUpdate[] SampleObjectPathDistance(int id, float startTime, float endTime, float timeStep, float distanceStep)
        {
            List<MovingObjectUpdate> samples = new List<MovingObjectUpdate>();

            MovingObjectUpdate[] timePath = SampleObjectPath(id, startTime, endTime, timeStep);
            if (timePath != null && timePath.Length > 0)
            {
                Vector3 lastPos = timePath[0].position;
                samples.Add(timePath[0]);

                for (int i = 1; i < timePath.Length; i++)
                {
                    var update = timePath[i];

                    if (Vector3.Distance(lastPos, update.position) >= distanceStep)
                    {
                        samples.Add(timePath[i]);
                        lastPos = update.position;
                    }
                }
            }

            return samples.ToArray();
        }

        // Update is called once per frame
        void Update()
        {
            mSimTime += Time.deltaTime * PlaybackSpeed;
            mSimTime = Mathf.Clamp(mSimTime, mTimeExtents.x, mTimeExtents.y);

            Debug.Log("K3 UPDATE " + mTimeExtents.ToString());
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