using UnityEngine;

namespace KingsleyPhD
{

    [RequireComponent(typeof(SphereCollider))]
    public class TrackballControl : MonoBehaviour
    {
        [Header("Rotation Settings")]
        public float rotationSpeed = 200.0f;
        public float damping = 5.0f;

        [Header("Zoom Settings")]
        public Camera targetCamera;
        public float zoomSpeed = 0.1f;
        public float minZoomDistance = 0.25f;
        public float maxZoomDistance = 2.0f;

        private bool isDragging = false;
        private Vector3 lastMousePosition;
        private Quaternion targetRotation;
        private Transform targetTransform;
        private float currentZoomDistance = 1.0f;

        private SphereCollider sphereCollider;
        private bool colliderHover = false;

        public float ZoomDistance { set {  currentZoomDistance = value; } }

        void Start()
        {
            targetTransform = transform;
            targetRotation = targetTransform.rotation;
            currentZoomDistance = transform.localScale.x;
            sphereCollider = GetComponent<SphereCollider>();

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        void Update()
        {
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                colliderHover = (hit.collider == sphereCollider);
            }
            else
            {
                colliderHover = false;
            }

            HandleRotation();
            HandleZoom();
        }

        void HandleRotation()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (colliderHover)
                {
                    isDragging = true;
                    lastMousePosition = Input.mousePosition;
                }
            } 
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                lastMousePosition = Input.mousePosition;

                // Calculate rotation based on mouse movement
                float rotX = delta.y * rotationSpeed * Time.deltaTime;
                float rotY = -delta.x * rotationSpeed * Time.deltaTime;

                Quaternion xQuat = Quaternion.AngleAxis(rotX, targetCamera.transform.right);
                Quaternion yQuat = Quaternion.AngleAxis(rotY, Vector3.up);

                targetRotation = yQuat * xQuat * targetRotation;
            }

            // Smooth rotation interpolation
            targetTransform.rotation = Quaternion.Slerp(targetTransform.rotation, targetRotation, Time.deltaTime * damping);
        }

        void HandleZoom()
        {
            if (colliderHover)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.0001f)
                {
                    currentZoomDistance -= scroll * zoomSpeed;
                    currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);
                }

                // Move camera along its forward axis to zoom
                //Vector3 direction = (targetCamera.transform.position - targetTransform.position).normalized;
                //targetCamera.transform.position = targetTransform.position + direction * currentZoomDistance;

                targetTransform.localScale = new Vector3(currentZoomDistance, currentZoomDistance, currentZoomDistance);
            }
        }
    }

}