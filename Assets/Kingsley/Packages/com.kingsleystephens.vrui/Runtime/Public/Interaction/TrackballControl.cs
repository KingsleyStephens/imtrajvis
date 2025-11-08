using UnityEngine;
using UnityEngine.UIElements;

namespace KingsleyStephens
{
    namespace VRUI
    {

        [RequireComponent(typeof(SphereCollider))]
        public class TrackballControl : MonoBehaviour
        {
            public InputEventManager inputEvents;

            [Header("Rotation Settings")]
            public float rotationSpeed = 200.0f;
            public float damping = 5.0f;

            [Header("Zoom Settings")]
            public Camera targetCamera;
            public float zoomSpeed = 0.1f;
            public float minZoomDistance = 0.25f;
            public float maxZoomDistance = 2.0f;

            private bool isDragging = false;
            private Quaternion targetRotation;
            private float currentZoomDistance = 1.0f;

            private SphereCollider sphereCollider;
            private bool colliderHover = false;
            private Vector3 mCurrControlPos = Vector3.zero;
            private Vector3 mLastControlPos = Vector3.zero;

            public float ZoomDistance { set { currentZoomDistance = value; } }

            public void Set(Vector3 rotation, float zoom)
            {
                targetRotation = Quaternion.Euler(rotation);
                ApplyRotation(targetRotation);

                currentZoomDistance = zoom;
                ApplyZoom(currentZoomDistance);
            }


            void Start()
            {
                targetRotation = transform.rotation;
                currentZoomDistance = transform.localScale.x;
                sphereCollider = GetComponent<SphereCollider>();

                if (targetCamera == null)
                {
                    targetCamera = Camera.main;
                }
            }

            void Update()
            {
                if (inputEvents != null && !inputEvents.SliderSelected)
                {
                    Ray ray = inputEvents.GetRay();
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100.0f, 1 << gameObject.layer))
                    {
                        colliderHover = (hit.collider == sphereCollider);

                        mCurrControlPos = ray.origin;
                    }
                    else
                    {
                        colliderHover = false;
                    }

                    HandleRotation();
                    HandleZoom();
                }
            }

            void HandleRotation()
            {
                if (inputEvents != null)
                {
                    if (inputEvents.GetSelectDown())
                    {
                        if (colliderHover)
                        {
                            isDragging = true;
                            mLastControlPos = mCurrControlPos;
                        }
                    }
                    else if (inputEvents.GetSelectUp())
                    {
                        isDragging = false;
                    }

                    if (isDragging)
                    {
                        Vector3 delta = (mCurrControlPos - mLastControlPos);
                        mLastControlPos = mCurrControlPos;

                        // Calculate rotation based on movement
                        float rotX = -delta.y * rotationSpeed * Time.deltaTime;
                        float rotY = -delta.x * rotationSpeed * Time.deltaTime;

                        Ray ray = inputEvents.GetRay();

                        Quaternion xQuat = Quaternion.AngleAxis(rotX, Vector3.Cross(ray.direction, Vector3.up));
                        Quaternion yQuat = Quaternion.AngleAxis(rotY, Vector3.up);

                        targetRotation = yQuat * xQuat * targetRotation;
                    }

                    // Smooth rotation interpolation
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * damping);
                }
            }

            void HandleZoom()
            {
                if (inputEvents != null)
                {
                    if (colliderHover)
                    {
                        float scroll = inputEvents.GetScroll();
                        if (Mathf.Abs(scroll) > 0.01f)
                        {
                            currentZoomDistance -= scroll * zoomSpeed;
                            currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);
                        }

                        // Move camera along its forward axis to zoom
                        //Vector3 direction = (targetCamera.transform.position - targetTransform.position).normalized;
                        //targetCamera.transform.position = targetTransform.position + direction * currentZoomDistance;

                        ApplyZoom(currentZoomDistance);
                    }
                }
            }

            private void ApplyZoom(float zoom)
            {
                transform.localScale = new Vector3(zoom, zoom, zoom);
            }

            private void ApplyRotation(Quaternion rotation)
            {
                transform.rotation = rotation;
            }
        }

    }
}