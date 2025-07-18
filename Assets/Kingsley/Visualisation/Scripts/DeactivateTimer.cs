using UnityEngine;


namespace KVisualisation
{

    public class DeactivateTimer : MonoBehaviour
    {
        public float duration = 30.0f;

        private float startTime;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            startTime = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time > startTime + duration)
            {
                gameObject.SetActive(false);
            }
        }
    }

}
