using UnityEngine;

namespace KingsleyPhD
{

    public class VisScaler : MonoBehaviour
    {
        public enum ScaleType
        {
            Small,
            Medium,
            LifeSize
        }

        public ScaleType scale = ScaleType.Small;

        public Vector3 smallSizeScale = new Vector3(0.01f, 0.01f, 0.01f);
        public Vector3 mediumSizeScale = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 lifeSizeScale = new Vector3(1.0f, 1.0f, 1.0f);

        [SerializeField]
        private Transform scene;

        public ScaleType Scale
        {
            set
            {
                switch (value)
                {
                    case VisScaler.ScaleType.Small:
                        {
                            if (scene != null)
                            {
                                scene.transform.localScale = smallSizeScale;
                            }
                            break;
                        }
                    case VisScaler.ScaleType.Medium:
                        {
                            if (scene != null)
                            {
                                scene.transform.localScale = mediumSizeScale;
                            }
                            break;
                        }
                    case VisScaler.ScaleType.LifeSize:
                        {
                            if (scene != null)
                            {
                                scene.transform.localScale = lifeSizeScale;
                            }
                            break;
                        }
                }
            }
        }


        public void Reset()
        {
            Scale = scale;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Reset();
        }

    }

}
