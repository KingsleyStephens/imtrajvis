using KController;
using Oculus.Interaction.DebugTree;
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

            public enum TimeType
            {
                Dynamic,
                Static
            }

            public enum FilterType
            {
                None,
                InverseSelection,
                Selection,
                Clear
            }

            public bool mesh = true;
            public bool trail = false;
            public bool ghosting = false;
            public bool density = false;

            public FilterType filter = FilterType.None;

            public TimeType timeType = TimeType.Dynamic;

            public float trailTime = 30.0f;
            public float densityUpdateFrequence = 2.0f;

            public GhostingDrop ghostingDrop = GhostingDrop.Distance;
            public float ghostingDropValue = 20.0f;
            public bool overrideGhostColour = false;
            public Color ghostColour = Color.white;

            public Vector3 objectScale = Vector3.one;


            public MovingObjectSettings() { }
            public MovingObjectSettings(MovingObjectSettings other)
            {
                mesh = other.mesh;
                trail = other.trail;
                ghosting = other.ghosting;
                density = other.density;
                filter = other.filter;
                timeType = other.timeType;
                trailTime = other.trailTime;
                densityUpdateFrequence = other.densityUpdateFrequence;
                ghostingDrop = other.ghostingDrop;
                ghostingDropValue= other.ghostingDropValue;
                overrideGhostColour = other.overrideGhostColour;
                ghostColour = other.ghostColour;
            }
        }

        [SerializeField]
        protected MovingObjectSettings movingObjectSettings = new MovingObjectSettings();


        protected VisSimController mVisSimController;

        protected int mId = -1;
        public int Id { set { mId = value; } }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        public virtual void OnStart()
        { 
        }

        public virtual void SpawnAll()
        { 
        }

        public void ApplySettings(int id, MovingObjectSettings settings)
        {
            mId = id;
            movingObjectSettings = new MovingObjectSettings(settings);
                
            OnSettingsUpdate();
        }

        // Update is called once per frame
        void Update()
        {
            OnUpdate();
        }

        private void OnEnable()
        {

        }


        public void SetController(VisSimController visSimController)
        {
            mVisSimController = visSimController;
            if (mVisSimController != null)
            {
                mVisSimController.Attach(OnMovingObjectSpawn, OnMovingObjectDestruct, OnTimeScrub);
            }
        }

        public virtual void OnSettingsUpdate()
        {

        }

        public virtual void OnViewChange()
        {

        }

        public virtual void OnResize()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnMovingObjectSpawn(int playerId, int id)
        {

        }

        public virtual void OnMovingObjectDestruct(int playerId, int id)
        {

        }

        public virtual void OnTimeScrub(int playerId, float fromTime, float toTime)
        {

        }
    }
}
