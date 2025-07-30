using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace KVisualisation
{

    public class TrailTimeUpdater : MonoBehaviour, ITimeUpdater
    {
        private float mSimTime = 0.0f;
        private float mSpawnTime = 0.0f;

        private TrailRenderer mTrail;

        public void SetTime(float time)
        {
            mSimTime = time;
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            mTrail = GetComponent<TrailRenderer>();
            mSpawnTime = mSimTime;
        }

        // Update is called once per frame
        void Update()
        {
            if (mSimTime < mSpawnTime)
            {
                Destroy(gameObject);
            }
        }
    }

}