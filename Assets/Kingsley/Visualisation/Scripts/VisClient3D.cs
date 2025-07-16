using KController;
using KCore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace KVisualisation
{
    public class VisClient3D : VisClient
    {
        [SerializeField]
        private GameObject objectPrefab;

        private Dictionary<string, GameObject> mMovingObjects = new Dictionary<string, GameObject>();
        private List<string> mAllObjectNames = new List<string>();

        public override void OnUpdate()
        {
            foreach (string name in mAllObjectNames)
            {
                var update = mVisSimController.GetSimObjectUpdate(name);

                GameObject obj;
                if (mMovingObjects.TryGetValue(name, out obj))
                {
                    if (update != null)
                    {
                        obj.transform.position = update.position;
                        obj.transform.rotation = update.orientation;
                    }
                    else
                    {
                        Destruct(name);
                    }
                }
                else if (update != null)
                {
                    Spawn(name, update);
                }
            }
        }


        public override void OnMovingObjectSpawn(string name)
        {
            mAllObjectNames.Add(name);

            var update = mVisSimController.GetSimObjectUpdate(name);
            if (update != null)
            {
                Spawn(name, update);
            }
        }

        public override void OnMovingObjectDestruct(string name)
        {
            Destruct(name);
        }

        private void Spawn(string name, VisSimController.MovingObjectUpdate update)
        {
            var instance = Instantiate(objectPrefab, transform);
            instance.name = name;
            instance.transform.position = update.position;
            instance.transform.rotation = update.orientation;
            mMovingObjects.Add(name, instance);
        }

        private void Destruct(string name)
        {
            GameObject obj;
            if (mMovingObjects.TryGetValue(name, out obj))
            {
                Destroy(obj);
                mMovingObjects.Remove(name);
            }
        }
    }
}
