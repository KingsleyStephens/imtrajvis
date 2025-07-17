using System.Collections.Generic;
using SpatioTemporalDatabase;
using System.IO;
using UnityEngine;

namespace KSimulation
{

    /**
     * Manager class for recording simulation
     */
    public class SimulationRecorder : MonoBehaviour
    {
        [SerializeField]
        private string dbName = "simulation";

        [SerializeField]
        private string sessionName = "session";

        [SerializeField]
        private bool record = false;

        [SerializeField]
        private float recordingTimestep = 0.5f;

        private static SimulationRecorder instance;


        public static SimulationRecorder Instance { get { return instance; } }


        private SpatioTemporalDB mSimulationDB;

        private List<MoveableObject> mNewMoveableObjects = new List<MoveableObject>();
        private List<MoveableObject> mMoveableObjects = new List<MoveableObject>();

        private float lastRecordTime = -1000.0f;
        private float startRecordTime = 0.0f;


        private void Awake()
        {
            instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (record)
            {
                mSimulationDB = CreateDb();
                mSimulationDB.CreateTable(sessionName);
                startRecordTime = Time.time;
            }
        }

        private void OnDestroy()
        {
            if (record)
            {
                mSimulationDB.Save();
                mSimulationDB.Close();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (record)
            {
                float simTime = Time.time - startRecordTime;

                if (mNewMoveableObjects.Count > 0)
                {
                    foreach (MoveableObject obj in mNewMoveableObjects)
                    {
                        int objectId = -1;
                        int parentId = -1;
                        string objectClass = "";
                        AgentId agentId = obj.gameObject.GetComponent<AgentId>();
                        if (agentId != null)
                        {
                            objectId = agentId.id;
                            parentId = agentId.parentId;
                            objectClass = agentId.agentClass.ToString();
                        }

                        mSimulationDB.UpdateTable(sessionName, objectId, recordingTimestep);
                        mSimulationDB.UpdateTable(sessionName, objectId, new SpatioTemporalDB.MovingObjectData(objectId, parentId, objectClass, obj.name));
                        mSimulationDB.UpdateTable(sessionName, objectId, new SpatioTemporalDB.MovingObjectPositionUpdate(obj.Position, obj.Orientation, obj.Velocity.magnitude, simTime));
                        mMoveableObjects.Add(obj);
                    }
                    mNewMoveableObjects.Clear();
                }

                if (simTime - lastRecordTime > recordingTimestep)
                {
                    RecordMovingObjects(simTime);
                    lastRecordTime = simTime;
                }
            }
        }

        public void AddMovingObject(MoveableObject obj)
        {
            if (record)
            {
                mNewMoveableObjects.Add(obj);
            }
        }

        public void RemoveMovingObject(MoveableObject obj)
        {
            if (record)
            {
                mMoveableObjects.Remove(obj);
            }
        }

        private void RecordMovingObjects(float time)
        {
            foreach (MoveableObject obj in mMoveableObjects)
            {
                if (obj.gameObject.activeSelf)
                {
                    int objectId = -1;
                    AgentId agentId = obj.gameObject.GetComponent<AgentId>();
                    if (agentId != null)
                    {
                        objectId = agentId.id;
                    }

                    mSimulationDB.UpdateTable(sessionName, objectId, new SpatioTemporalDB.MovingObjectPositionUpdate(obj.Position, obj.Orientation, obj.Velocity.magnitude, time));
                }
            }
        }

        private JsonSpatioTemporalDB CreateDb()
        {
            string path = Path.Combine(Application.persistentDataPath, dbName + ".json");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            JsonSpatioTemporalDB db = new JsonSpatioTemporalDB(path);
            db.CreateOrOpen();
            return db;
        }
    }

}