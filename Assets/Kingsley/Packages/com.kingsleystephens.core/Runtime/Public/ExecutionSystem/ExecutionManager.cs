using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KingsleyStephens.CoreInternal;

namespace KingsleyStephens
{
    namespace Core
    {
        /**
         * Monobehviour singleton class to manage execution of objects
         */
        public class ExecutionManager : SingletonMonoBehaviour<ExecutionManager>
        {
            // PRIVATE

            // List of updateables to update
            private List<Updateable> mUpdateables = new List<Updateable>();
            private List<Updateable> mUpdateablesToAdd = new List<Updateable>();
            private List<Updateable> mUpdateablesToRemove = new List<Updateable>();
            private List<Updateable> mUpdateablesToDestroy = new List<Updateable>();

            // PUBLIC

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
             * Add a new updateable
             */
            public T NewUpdateable<T>() where T : Updateable
            {
                T t = ScriptableObject.CreateInstance<T>();
                Add(t);

                return t;
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

            /**
             * Remove game timer
             * 
             * @param timer The GameTimer object previously created
             */
            public void DestroyUpdateable<T>(T t) where T : Updateable
            {
                if (!mUpdateablesToDestroy.Contains(t))
                {
                    Remove(t);
                    mUpdateablesToDestroy.Add(t);
                }
            }

            // PRIVATE

            /** 
                * Start is called before the first frame update
                */
            void Start()
            {
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

                // Remove
                if (mUpdateablesToRemove.Count > 0)
                {
                    while (mUpdateablesToRemove.Count > 0)
                    {
                        Updateable u = mUpdateablesToRemove[0];
                        mUpdateablesToRemove.RemoveAt(0);
                        u.EndUpdating();        // May add updateables to be removed
                        if (mUpdateables.Contains(u))
                        {
                            mUpdateables.Remove(u);
                        }
                    }
                }

                // Destroy
                if (mUpdateablesToDestroy.Count > 0)
                {
                    foreach (var u in mUpdateablesToDestroy)
                    {
                        Destroy(u);
                    }
                    mUpdateablesToDestroy.Clear();
                }

                // Update
                TimeSnapshot timeSnapshot = new TimeSnapshot(Time.time, Time.deltaTime);
                foreach (var u in mUpdateables)
                {
                    u.RunUpdate(timeSnapshot);
                }
            }
        }
    }
}