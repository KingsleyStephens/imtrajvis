using System.Collections.Generic;
using UnityEngine;

namespace KVisualisation
{

    public class AdaptiveTrailRenderer : MonoBehaviour
    {
        [SerializeField]
        private GameObject delegateTrailPrefab;

        private AdaptiveTrailRendererDelegate delegateTrail;

        public AdaptiveTrailRendererDelegate DelegateTrail { get { return delegateTrail; } }

        private void Awake()
        {
            
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void Init(Transform parentTransform)
        {
            GameObject go = Instantiate(delegateTrailPrefab, parentTransform);
            delegateTrail = go.GetComponent<AdaptiveTrailRendererDelegate>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public float MaxTime { get { return delegateTrail != null ? delegateTrail.MaxTime : 0.0f; } }

        public void AddPosition(Vector3 position, float time)
        {
            if (delegateTrail != null)
            {
                delegateTrail.AddPosition(position, time);
            }
        }

        public void CullOnTime(float time)
        {
            if (delegateTrail != null)
            {
                delegateTrail.CullOnTime(time);
            }
        }

    }

}
