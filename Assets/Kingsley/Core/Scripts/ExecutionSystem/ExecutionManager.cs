using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KCore
{
    /**
     * Monobehviour singleton class to manage execution of objects
     */
    public class ExecutionManager : MonoBehaviour
    {
        // PUBLIC

        // Singleton access
        public static ExecutionManager Instance { get { Debug.Assert(sInstance != null); return sInstance; } private set { sInstance = value; } }


        /**
         * Add a new updateable to the manager
         * 
         * @param u An Updateable object
         */
        public void Add(Updateable u)
        {
            mUpdateablesToAdd.Add(u);
        }

        /**
         * Remove an updateable from the manager
         * 
         * @param u An Updateable object
         */
        public void Remove(Updateable u)
        {
            mUpdateablesToRemove.Add(u);
        }



        // PRIVATE

        /**
         * Object awake
         */
        void Awake()
        {
            ExecutionManager em = GameObject.FindAnyObjectByType<ExecutionManager>();

            if (em != null && em != this)
            {
                Logger.LogError("ExecutionManager already in scene!");
            }
            else
            {
                Instance = this;
            }
        }

        /** 
         * Start is called before the first frame update
         */
        void Start()
        {
            Logger.LogMessage("Starting KCore::ExecutionManager");
        }

        /**
         * Update is called once per frame
         */
        void Update()
        {
            // Add
            if (mUpdateablesToAdd.Count > 0)
            {
                while (mUpdateablesToAdd.Count > 0)
                { 
                    Updateable u = mUpdateablesToAdd[0];
                    mUpdateablesToAdd.RemoveAt(0);
                    mUpdateables.Add(u);
                    u.BeginUpdating();      // May add sub-updateables
                }
                // TODO: Optimise by inserting at right place and remove this sort
                mUpdateables.Sort((Updateable a, Updateable b) => a.Priority - b.Priority);
            }

            // Update
            TimeSnapshot timeSnapshot = new TimeSnapshot(Time.time, Time.deltaTime);
            foreach (var u in mUpdateables)
            {
                u.RunUpdate(timeSnapshot);
            }

            // Remove
            if (mUpdateablesToRemove.Count > 0)
            {
                while (mUpdateablesToRemove.Count > 0)
                {
                    Updateable u = mUpdateablesToRemove[0];
                    mUpdateablesToRemove.RemoveAt(0);
                    u.EndUpdating();        // May add updateables to be removed
                    mUpdateables.Remove(u);
                }
            }
        }


        // PRIVATE

        // Singleton object
        private static ExecutionManager sInstance;

        // List of updateables to update
        private List<Updateable> mUpdateables = new List<Updateable>();
        private List<Updateable> mUpdateablesToAdd = new List<Updateable>();
        private List<Updateable> mUpdateablesToRemove = new List<Updateable>();
    }
}