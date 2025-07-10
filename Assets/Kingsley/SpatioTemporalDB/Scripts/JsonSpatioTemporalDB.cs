using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace SpatioTemporalDatabase
{

    /**
     * Spatio tempral database manager with CRUD-L operations
     */
    public class JsonSpatioTemporalDB : SpatioTemporalDB
    {
        [System.Serializable]
        public class MovingObjectPosition
        {
            public Vector3 position;
            public Vector3 velocity;
            public float time;
        }

        [System.Serializable]
        private class MovingObjectTrajectory
        {
            public string movingObjectId;
            public List<MovingObjectPosition> trajectory;
        }

        [System.Serializable]
        private class JsonTable
        {
            public string name;
            public List<MovingObjectTrajectory> trajectories;
        }


        [System.Serializable]
        private class JsonDB
        {
            public List<JsonTable> tables = new List<JsonTable>();
        }


        private string mJsonFilePath;
        private JsonDB mDB;

        /**
         * Constructor
         * 
         * @param path The name of the JSON file to save DB to
         */
        public JsonSpatioTemporalDB(string path) { mJsonFilePath = path; }


        // DB OPERATIONS

        public override bool CreateOrOpen() { 
            if (!string.IsNullOrEmpty(mJsonFilePath))
            {
                if (File.Exists(mJsonFilePath))
                {
                    try
                    {
                        mDB = JsonUtility.FromJson<JsonDB>(File.ReadAllText(mJsonFilePath));
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
                else
                {
                    mDB = new JsonDB();
                }
            }

            return true;
        }
        public override bool Save()
        {
            if (!string.IsNullOrEmpty(mJsonFilePath))
            {
                try
                {
                    File.WriteAllText(mJsonFilePath, JsonUtility.ToJson(mDB));
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Close() { return true; }


        // TABLE OPERATIONS

        /**
         * Create a new table in the database
         */
        public override bool CreateTable(string name) 
        { 
            mDB.tables.Add(new JsonTable() { name = name });

            return true;
        }

        /**
         * Delete a table from the database
         */
        public override bool DeleteTable(string name) { 
            
            if (mDB.tables.Find(t => t.name == name) != null)
            {
                mDB.tables.RemoveAll(t => t.name == name);
                return true;
            }
            return false; 
        }

        /**
         * Perform an update operation on a table
         */
        public override bool UpdateTable(string name, string movingObjectId, MovingObjectPositionUpdate positionUpdate) 
        {
            JsonTable table = mDB.tables.Find(t => t.name == name);
            if (table != null)
            {
                MovingObjectTrajectory movingObject = table.trajectories.Find(t => t.movingObjectId == movingObjectId);
                if (movingObject == null)
                {
                    movingObject = new MovingObjectTrajectory();
                    movingObject.movingObjectId = movingObjectId;
                    movingObject.trajectory = new List<MovingObjectPosition>();
                    table.trajectories.Add(movingObject);
                }

                movingObject.trajectory.Add(new MovingObjectPosition() { position = positionUpdate.position, velocity = positionUpdate.velocity, time = positionUpdate.time });
            }

            return false; 
        }

        /**
         * Read from a table
         */
        public override MovingObjectPositionUpdate[] ReadTable(string name, string movingObjectId, float time, float timeRange)
        {
            JsonTable table = mDB.tables.Find(t => t.name == name);
            if (table != null)
            {
                MovingObjectTrajectory movingObject = table.trajectories.Find(t => t.movingObjectId == movingObjectId);
                if (movingObject != null)
                {
                    List<MovingObjectPosition> positions = movingObject.trajectory.FindAll(t => t.time >= time && t.time <= time + timeRange);
                    return positions.Select(p => new MovingObjectPositionUpdate() { position = p.position, velocity = p.velocity, time = p.time }).ToArray();
                }
            }

            return null;
        }

        /**
         * List the contents of a table
         */
        public override string[] ListTable(string name) 
        {
            JsonTable table = mDB.tables.Find(t => t.name == name);
            if (table != null)
            {
                return table.trajectories.Select(t => t.movingObjectId).ToArray();
            }
            
            return null; 
        }

    }


}