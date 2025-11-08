using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using KingsleyStephens.Math;

namespace KingsleyStephens
{
    namespace SpatioTemporalDatabase
    {

        /**
         * Spatio tempral database manager with CRUD-L operations
         */
        public class JsonSpatioTemporalDB : SpatioTemporalDB
        {
            [System.Serializable]
            public class PositionUpdate
            {
                public Vector3 position;
                public Quaternion orientation;
                public float speed;
                public float time;
                
                public PositionUpdate()
                {
                }

                public PositionUpdate(PositionUpdate other)
                {
                    position = other.position;
                    orientation = other.orientation;
                    speed = other.speed;
                    time = other.time;
                }
            }

            [System.Serializable]
            private class MovingObject
            {
                public int id;
                public int parentId;
                public string objectClass;
                public string name;
                public float trajectoryDelta;
                public List<PositionUpdate> trajectory = new List<PositionUpdate>();
            }

            [System.Serializable]
            private class JsonTable
            {
                public string name;
                public List<MovingObject> movingObjects = new List<MovingObject>();
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

            public override bool CreateOrOpen()
            {
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
                    return true;
                }

                return false;
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
            public override bool DeleteTable(string name)
            {

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
            public override bool UpdateTable(string name, int movingObjectId, MovingObjectPositionUpdate positionUpdate)
            {
                JsonTable table = mDB.tables.Find(t => t.name == name);
                if (table != null)
                {
                    MovingObject movingObject = table.movingObjects.Find(t => t.id == movingObjectId);
                    if (movingObject == null)
                    {
                        movingObject = new MovingObject();
                        movingObject.id = movingObjectId;
                        table.movingObjects.Add(movingObject);
                    }

                    movingObject.trajectory.Add(new PositionUpdate() { position = positionUpdate.position, orientation = positionUpdate.orientation, speed = positionUpdate.speed, time = positionUpdate.time });
                    return true;
                }

                return false;
            }

            /**
             * Perform an update operation on a table
             */
            public override bool UpdateTable(string name, int movingObjectId, float trajectoryDelta)
            {
                JsonTable table = mDB.tables.Find(t => t.name == name);
                if (table != null)
                {
                    MovingObject movingObject = table.movingObjects.Find(t => t.id == movingObjectId);
                    if (movingObject == null)
                    {
                        movingObject = new MovingObject();
                        movingObject.id = movingObjectId;
                        table.movingObjects.Add(movingObject);
                    }

                    movingObject.trajectoryDelta = trajectoryDelta;
                    return true;
                }

                return false;
            }

            public override bool UpdateTable(string name, int movingObjectId, MovingObjectData movingObjectData)
            {
                JsonTable table = mDB.tables.Find(t => t.name == name);
                if (table != null)
                {
                    MovingObject movingObject = table.movingObjects.Find(t => t.id == movingObjectId);
                    if (movingObject == null)
                    {
                        movingObject = new MovingObject();
                        movingObject.id = movingObjectId;
                        table.movingObjects.Add(movingObject);
                    }

                    movingObject.parentId = movingObjectData.parentId;
                    movingObject.objectClass = movingObjectData.objectClass;
                    movingObject.name = movingObjectData.name;
                    return true;
                }

                return false;
            }

            public override MovingObjectData ReadTable(string name, int movingObjectId)
            {
                JsonTable table = mDB.tables.Find(t => t.name == name);
                if (table != null)
                {
                    MovingObject movingObject = table.movingObjects.Find(t => t.id == movingObjectId);
                    if (movingObject != null)
                    {
                        return new MovingObjectData(movingObject.id, movingObject.parentId, movingObject.objectClass, movingObject.name);
                    }
                }

                return null;
            }


            /**
             * Read from a table
             */
            public override MovingObjectPositionUpdate ReadTable(string name, int movingObjectId, float time)
            {
                JsonTable table = mDB.tables.Find(t => t.name == name);
                if (table != null)
                {
                    MovingObject movingObject = table.movingObjects.Find(t => t.id == movingObjectId);
                    if (movingObject != null)
                    {
                        float halfTimeRange = movingObject.trajectoryDelta * 2.5f; // Take a sample of positions
                        List<PositionUpdate> positions = movingObject.trajectory.FindAll(t => t.time >= time - halfTimeRange && t.time <= time + halfTimeRange);

                        PositionUpdate positionUpdate = InterpolateUpdate(positions, time);
                        if (positionUpdate != null)
                        {
                            return new MovingObjectPositionUpdate(positionUpdate.position, positionUpdate.orientation, positionUpdate.speed, positionUpdate.time);
                        }
                    }
                }

                return null;
            }

            public override Vector2 ReadTableTimePeriod(string name, int movingObjectId)
            {
                JsonTable table = mDB.tables.Find(t => t.name == name);
                if (table != null)
                {
                    MovingObject movingObject = table.movingObjects.Find(t => t.id == movingObjectId);
                    if (movingObject != null)
                    {
                        if (movingObject.trajectory.Count > 0)
                        {
                            return new Vector2(movingObject.trajectory[0].time, movingObject.trajectory[movingObject.trajectory.Count - 1].time);
                        }
                    }
                }

                return Vector2.zero;
            }

            public override bool TableExsits(string name)
            {
                return mDB.tables.Find(t => t.name == name) != null;
            }


            /**
             * List the contents of a table
             */
            public override string[] ListTables()
            {
                return mDB.tables.Select(t => t.name).ToArray();
            }

            /**
             * List the contents of a table
             */
            public override int[] ListTable(string name)
            {
                JsonTable table = mDB.tables.Find(t => t.name == name);
                if (table != null)
                {
                    return table.movingObjects.Select(t => t.id).ToArray();
                }

                return new int[0];
            }


            private PositionUpdate InterpolateUpdate(List<PositionUpdate> positions, float time)
            {
                if (positions.Count == 0)
                {
                    return null;
                }

                int indexA = -1;
                int indexB = positions.Count;
                while (indexA + 1 < indexB)
                {
                    PositionUpdate left = positions[indexA + 1];
                    PositionUpdate right = positions[indexB - 1];

                    if (Mathf.Approximately(time, left.time))
                    {
                        return new PositionUpdate(left);
                    }
                    if (Mathf.Approximately(time, right.time))
                    {
                        return new PositionUpdate(right);
                    }

                    if (time > left.time)
                    {
                        indexA++;
                    }

                    if (time < right.time)
                    {
                        indexB--;
                    }
                }

                // Time in range of segments
                if (indexA >= 0 && indexB < positions.Count)
                {
                    PositionUpdate positionUpdate = new PositionUpdate() { time = time };

                    float t = (time - positions[indexA].time) / (positions[indexB].time - positions[indexA].time);
                    positionUpdate.orientation = Quaternion.Slerp(positions[indexA].orientation, positions[indexB].orientation, t);
                    positionUpdate.speed = Mathf.Lerp(positions[indexA].speed, positions[indexB].speed, t);

                    // Time first or last segment otherwise at least three segments
                    if (indexA == 0 || indexB == positions.Count - 1)
                    {
                        positionUpdate.position = Vector3.Lerp(positions[indexA].position, positions[indexB].position, t);
                    }
                    else
                    {
                        // Spline interpolate position
                        positionUpdate.position = CatmullRomSpline.Interpolate(positions[indexA - 1].position, positions[indexA].position, positions[indexB].position, positions[indexB + 1].position, t);
                    }

                    return positionUpdate;
                }

                return null;
            }
        }
    }
}