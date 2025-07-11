using UnityEngine;

namespace SpatioTemporalDatabase
{

    /**
     * Spatio tempral database manager with CRUD-L operations
     */
    public abstract class SpatioTemporalDB
    {
        public class MovingObjectPositionUpdate
        {
            public Vector3 position;
            public Vector3 orientation;
            public float speed;
            public float time;
        }

        public SpatioTemporalDB() { }


        // DB OPERATIONS

        public abstract bool CreateOrOpen();
        public abstract bool Save();
        public abstract bool Close();


        // TABLE OPERATIONS

        /**
         * Create a new table in the database
         */
        public abstract bool CreateTable(string name);

        /**
         * Delete a table from the database
         */
        public abstract bool DeleteTable(string name);

        /**
         * Perform an update operation on a table
         */
        public abstract bool UpdateTable(string name, string movingObjectId, MovingObjectPositionUpdate positionUpdate);

        /**
         * Read from a table
         */
        public abstract MovingObjectPositionUpdate[] ReadTable(string name, string movingObjectId, float time, float timeRange);

        /**
         * List all the tables
         */
        public abstract string[] ListTables();

        /**
         * List the contents of a table
         */
        public abstract string[] ListTable(string name);

    }


}