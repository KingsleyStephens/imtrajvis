using KController;
using UnityEngine;

namespace KVisualisation
{
    public class VisClient : MonoBehaviour
    {
        protected VisSimController mVisSimController;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            OnUpdate();
        }


        public void SetController(VisSimController visSimController)
        {
            mVisSimController = visSimController;

            if (mVisSimController != null)
            {
                mVisSimController.Attach(OnMovingObjectSpawn, OnMovingObjectDestruct);
            }
        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnMovingObjectSpawn(string name)
        {

        }

        public virtual void OnMovingObjectDestruct(string name)
        {

        }
    }
}
