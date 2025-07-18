using KController;
using UnityEngine;

namespace KVisualisation
{
    public class VisClient : MonoBehaviour
    {
        [System.Serializable]
        public class MovingObjectSettings
        {
            public enum GhostingDrop
            {
                Time,
                Distance
            }

            public bool mesh = true;
            public bool trail = false;
            public bool ghosting = false;
            public bool density = false;

            public float trailTime = 30.0f;
            public float densityUpdateFrequence = 2.0f;

            public GhostingDrop ghostingDrop = GhostingDrop.Time;
            public float ghostingDropValue = 1.0f;
            public bool overrideGhostColour = false;
            public Color ghostColour = Color.white;
        }

        public MovingObjectSettings movingObjectSettings = new MovingObjectSettings();


        protected VisSimController mVisSimController;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (mVisSimController != null)
            {
                mVisSimController.StartSim();
            }

            OnStart();
        }

        public virtual void OnStart()
        { 
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

        public virtual void OnMovingObjectSpawn(int id)
        {

        }

        public virtual void OnMovingObjectDestruct(int id)
        {

        }
    }
}
