using System.Data;
using UnityEditor.Graphs;
using UnityEngine;
using static KingsleyPhD.Workspace;
using KingsleyStephens.VRUI;

namespace KingsleyPhD
{
    /**
     * Class to organise a workspace scene
     */
    public class WorkspaceScene : MonoBehaviour
    {
        // DATA

        [Header("Visualisation prefab")]
        [SerializeField]
        private GameObject sceneFlowManagerPrefab;      // Prefab to instantiate the visualisation

        [Header("Runtime vis instance")]
        [SerializeField]
        private Transform vis;              // The transform to attach the visualisation to

        // PUBLIC

        public Transform Vis { get { return vis; } }
        public float RelativeTime { get { return (mSceneFlowManager != null) ? mSceneFlowManager.SimRelativeTime : 0.0f; } }
        public float ScrubTime{ set { if (mSceneFlowManager != null) mSceneFlowManager.ScrubTime = value; } }
        public bool Pause { set { if (mSceneFlowManager != null) mSceneFlowManager.TimePause(value); } }
        public SceneFlowManager SceneFlowManager { get { return mSceneFlowManager; } }

        // PRIVATE DATA

        private SceneFlowManager mSceneFlowManager;
        private TrackballControl mTrackballControl;

        // PRIVATE

        private void Awake()
        {
            mTrackballControl = GetComponent<TrackballControl>();
        }

        /**
         * Init
         */
        public void Init(string dataset, string[] sessions, GameObject scenePrefab)
        {
            if (vis != null)
            {
                GameObject sceneFlowManagerGameObject = Instantiate(sceneFlowManagerPrefab);
                mSceneFlowManager = sceneFlowManagerGameObject.GetComponent<SceneFlowManager>();
                if (mSceneFlowManager != null)
                {
                    mSceneFlowManager.Launch(vis, dataset, sessions, scenePrefab);
                    mSceneFlowManager.SimTime = 0.0f;
                    Pause = true;
                }
            }
        }

        /**
         * Shutdown
         */
        public void Shutdown()
        {
            if (mSceneFlowManager != null)
            {
                Destroy(mSceneFlowManager.gameObject);
                mSceneFlowManager = null;
            }
        }

        public void Transform(Vector3 rotation, float zoom)
        {
            if (mTrackballControl != null)
            {
                mTrackballControl.Set(rotation, zoom);
            }
        }

        public void Configure(DesignInstanceJSON designInstance)
        {
            if (mSceneFlowManager != null)
            {
                mSceneFlowManager.ConfigDesignSpace(designInstance, vis.localScale.x);
            }
        }

        public void Resize()
        {
            if (vis != null)
            {
                SphereCollider sphereCollider = vis.parent.GetComponent<SphereCollider>();
                if (sphereCollider != null)
                {
                    float r = sphereCollider.radius * 2.0f;

                    SceneTag sceneTag = vis.GetComponentInChildren<SceneTag>();
                    if (sceneTag != null)
                    {
                        BoxCollider sceneCollider = sceneTag.gameObject.GetComponent<BoxCollider>();
                        if (sceneCollider != null)
                        {
                            float halfWidth = sceneCollider.size.magnitude;
                            halfWidth *= sceneCollider.transform.localScale.x;
                            float ratio = r / halfWidth;
                            vis.localScale = new Vector3(ratio, ratio, ratio);
                        }
                    }
                }
                else
                {
                    vis.localScale = Vector3.one;   // Standard size for FPV
                }
            }

            if (mSceneFlowManager != null)
            {
                mSceneFlowManager.Resize();
            }
        }

    }

}