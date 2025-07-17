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
            if (mVisSimController != null)
            {
                mVisSimController.StartSim();
            }
        }

        // Update is called once per frame
        void Update()
        {
            OnUpdate();
        }

        private void OnEnable()
        {
            if (mVisSimController != null)
            {
                mVisSimController.Attach(OnMovingObjectSpawn, OnMovingObjectDestruct);
            }
        }


        public void SetController(VisSimController visSimController)
        {
            mVisSimController = visSimController;
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
