using UnityEngine;

namespace DCultCity.World
{
    /// <summary>
    /// Lightweight camera controller for Milestone 1 inspection.
    /// Supports desktop WASD + Mouse look and mobile touch gestures (drag to orbit, pinch to zoom).
    /// </summary>
    public class SimpleFreeCam : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 15f;
        [SerializeField] private float sprintMultiplier = 2.0f;
        [SerializeField] private float lookSensitivity = 2.5f;

        [Header("Mobile Touch Settings")]
        [SerializeField] private float touchRotationSpeed = 0.2f;
        [SerializeField] private float touchZoomSpeed = 0.05f;

        private float pitch = 25f;
        private float yaw = -35f;

        private void Start()
        {
            // Initial angle facing the test city
            transform.position = new Vector3(28f, 22f, -28f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void Update()
        {
            HandleDesktopInput();
            HandleMobileTouchInput();
        }

        private void HandleDesktopInput()
        {
            // Mouse look when holding right-click or middle-click
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * lookSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
                pitch = Mathf.Clamp(pitch, -85f, 85f);
                transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }

            // WASD Navigation
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            float elevation = 0f;

            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space)) elevation += 1f;
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl)) elevation -= 1f;

            float currentSpeed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
            Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal + Vector3.up * elevation).normalized;
            transform.position += moveDirection * (currentSpeed * Time.deltaTime);

            // Scroll wheel zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                transform.position += transform.forward * (scroll * 15f);
            }
        }

        private void HandleMobileTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                // Only rotate if not touching UI buttons (left 60% or middle of screen)
                if (touch.phase == TouchPhase.Moved && touch.position.y > Screen.height * 0.35f)
                {
                    yaw += touch.deltaPosition.x * touchRotationSpeed;
                    pitch -= touch.deltaPosition.y * touchRotationSpeed;
                    pitch = Mathf.Clamp(pitch, -85f, 85f);
                    transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                transform.position -= transform.forward * (deltaMagnitudeDiff * touchZoomSpeed);
            }
        }
    }
}
