using UnityEngine;

namespace DCultCity.Player
{
    /// <summary>
    /// Smooth third-person follow camera with obstacle collision avoidance.
    /// Supports desktop mouse orbiting and mobile touch drag navigation.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.4f, 0f);

        [Header("Distance & Limits")]
        [SerializeField] private float defaultDistance = 5.0f;
        [SerializeField] private float minDistance = 1.5f;
        [SerializeField] private float maxDistance = 10.0f;
        [SerializeField] private float minPitch = -20f;
        [SerializeField] private float maxPitch = 70f;

        [Header("Sensitivity & Smoothing")]
        [SerializeField] private float mouseSensitivity = 2.5f;
        [SerializeField] private float touchSensitivity = 0.25f;
        [SerializeField] private float positionSmoothTime = 0.08f;
        [SerializeField] private float collisionRadius = 0.25f;
        [SerializeField] private LayerMask collisionLayers = ~0;

        private float yaw = 0f;
        private float pitch = 18f;
        private float currentDistance;
        private Vector3 currentVelocity;

        public Transform Target { get => target; set => target = value; }

        private void Start()
        {
            currentDistance = defaultDistance;
            if (target != null)
            {
                yaw = target.eulerAngles.y;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                // Auto-find player if not assigned
                var player = FindObjectOfType<ThirdPersonController>();
                if (player != null) target = player.transform;
                else return;
            }

            HandleInput();
            UpdateCameraPosition();
        }

        private void HandleInput()
        {
            // Desktop mouse orbit when right-clicking or moving
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            }

            // Desktop scroll zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                defaultDistance = Mathf.Clamp(defaultDistance - scroll * 4f, minDistance, maxDistance);
            }

            // Mobile touch orbit handled if touch is in right half of screen
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch t = Input.GetTouch(i);
                    // Right half of screen and not touching bottom UI buttons
                    if (t.position.x > Screen.width * 0.45f && t.position.y > Screen.height * 0.25f && t.phase == TouchPhase.Moved)
                    {
                        yaw += t.deltaPosition.x * touchSensitivity;
                        pitch -= t.deltaPosition.y * touchSensitivity;
                    }
                }
            }

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private void UpdateCameraPosition()
        {
            Vector3 focusPoint = target.position + targetOffset;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

            // Obstacle collision avoidance (prevents clipping through city buildings)
            Vector3 desiredPosition = focusPoint - (rotation * Vector3.forward * defaultDistance);
            Vector3 rayDirection = (desiredPosition - focusPoint).normalized;

            float targetDistance = defaultDistance;
            if (Physics.SphereCast(focusPoint, collisionRadius, rayDirection, out RaycastHit hit, defaultDistance, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                targetDistance = Mathf.Clamp(hit.distance - collisionRadius, minDistance, defaultDistance);
            }

            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 15f);
            Vector3 finalPosition = focusPoint - (rotation * Vector3.forward * currentDistance);

            transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref currentVelocity, positionSmoothTime);
            transform.LookAt(focusPoint);
        }
    }
}
