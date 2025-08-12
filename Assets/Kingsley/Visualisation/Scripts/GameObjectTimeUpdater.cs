using UnityEngine;

namespace KVisualisation
{

    public class GameObjectTimeUpdater : MonoBehaviour, ITimeUpdater
    {
        private float mSimTime = 0.0f;
        private float mSpawnTime = 0.0f;

        public void SetTime(float time)
        {
            mSimTime = time;
        }

        public void Reset()
        {
            
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
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