using SATestbed;
using UnityEngine;

namespace KVisualisation
{
    public class MenuActivator : MonoBehaviour
    {
        public SceneFlowManager manager;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void Activate()
        {
            Collider[] collisions = Physics.OverlapSphere(transform.position, 1.0f);
            if (collisions.Length > 0)
            {
                Visualisation vis = collisions[0].gameObject.GetComponent<Visualisation>();
                if (vis != null && manager != null)
                {
                    manager.ShowConfigScreen(vis.instanceId);
                }
            }
        }
    }
}
