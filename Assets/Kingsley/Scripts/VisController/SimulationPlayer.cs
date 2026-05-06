using System.Collections.Generic;
using KingsleyStephens.SpatioTemporalDatabase;
using System.IO;
using UnityEngine;

namespace KingsleyStephens
{
    namespace ImTrajVis
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
                public MovingObjectUpdate(Vector3 position, Quaternion orientation, float speed, float time)
                {
                    this.position = position;
                    this.orientation = orientation;
                    this.speed = speed;
                    this.time = time;
                }

                public Vector3 position;
                public Quaternion orientation;
                public float speed;
                public float time;
            }

            public delegate void OnSpawnMovingObject(int playerId, int id);
            private event OnSpawnMovingObject onSpawned;

            public delegate void OnDestroyMovingObject(int playerId, int id);
            private event OnDestroyMovingObject onDestroyed;

            public delegate void OnTimeScrub(int playerId, float fromTime, float toTime);
            private event OnTimeScrub onTimeScrubbed;


            public string dbName = "simulation";
            public string sessionName = "session";

            [SerializeField]
            private float startTime = 0.0f;

            [SerializeField]
            private float playbackSpeed = 1.0f;

            public int Id { get; set; }

            public float PlaybackSpeed { get; set; }

            private SpatioTemporalDB mSimulationDB;

            private float mSimTime;

            private Vector2 mTimeExtents = Vector2.zero;

            public float SimTime
            {
                get { return mSimTime; }
                set
                {
                    float oldValue = mSimTime;
                    mSimTime = value;
                    onTimeScrubbed(Id, oldValue, mSimTime);
                }
            }

            public Vector2 TimeExtents { get { return mTimeExtents; } }

            public bool Loaded { get; private set; }

            private void Awake()
            {
                PlaybackSpeed = playbackSpeed;
            }

            // Start is called once before the first execution of Update after the MonoBehaviour is created
            void Start()
            {

            }

            public void Load()
            {
                mSimulationDB = OpenDb();
                SetSimTime(startTime);

                int[] objs = ListSimObjects();
                Debug.Log("Simulation DB contains " + objs.Length + " moving objects for session " + sessionName);
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

                    onSpawned(Id, obj);
                }
            }

            private void TearDown()
            {
                int[] objs = ListSimObjects();
                foreach (int obj in objs)
                {
                    onDestroyed(Id, obj);
                }
            }

            public void CheckSpawned(OnSpawnMovingObject onSpawn)
            {
                int[] objs = ListSimObjects();
                foreach (int obj in objs)
                {
                    onSpawn(Id, obj);
                }
            }

            public void ScrubTime(float relativeTime)
            {
                float clampedTime = Mathf.Clamp01(relativeTime);
                SimTime = mTimeExtents.x + clampedTime * (mTimeExtents.y - mTimeExtents.x);
            }

            public float GetSimTimeAsRelativeTime()
            {
                return (SimTime - mTimeExtents.x) / (mTimeExtents.y - mTimeExtents.x);
            }

            private void SetSimTime(float time)
            {
                mSimTime = time;
            }

            public int[] ListSimObjects()
            {
                if (mSimulationDB != null)
                {
                    return mSimulationDB.ListTable(sessionName);
                }

                return new int[0];
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
                    return new MovingObjectUpdate(update.position, update.orientation, update.speed, mSimTime);
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
                        samples.Add(new MovingObjectUpdate(update.position, update.orientation, update.speed, t));
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
                            samples.Add(new MovingObjectUpdate(update.position, update.orientation, update.speed, t));
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
            }

            private JsonSpatioTemporalDB OpenDb()
            {
                string path = Path.Combine(Application.persistentDataPath, "TrajectoryData", dbName + ".json");

                Debug.Log("Loading simulation db: " + path);

                bool isTestData = dbName.Contains("TEST");
                JsonSpatioTemporalDB db = new JsonSpatioTemporalDB(path, isTestData, isTestData);
                if (db.CreateOrOpen())
                {
                    Debug.Log("Successfully Loaded simulation db: " + path);
                }
                return db;
            }
        }
    }
}