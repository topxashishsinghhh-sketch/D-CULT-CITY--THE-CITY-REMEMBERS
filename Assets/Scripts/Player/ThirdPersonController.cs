using DCultCity.Core;
using UnityEngine;

namespace DCultCity.Player
{
    /// <summary>
    /// Smooth third-person character locomotion controller.
    /// Handles camera-relative movement, sprinting, jumping, slope traversal,
    /// and feeds locomotion speed to CharacterVisual for procedural animation.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Locomotion Speeds")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 8.5f;
        [SerializeField] private float rotationSmoothTime = 0.12f;
        [SerializeField] private float speedChangeRate = 10f;

        [Header("Jump & Gravity")]
        [SerializeField] private float jumpHeight = 1.4f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundCheckRadius = 0.28f;
        [SerializeField] private LayerMask groundLayers = ~0;

        [Header("References")]
        [SerializeField] private CharacterVisual characterVisual;
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private PlayerInputReader inputReader;

        private float currentSpeed;
        private float verticalVelocity;
        private float rotationVelocity;
        private bool isGrounded;

        // Footstep tracking for future NPC awareness
        private float distanceTraveled = 0f;

        public float CurrentSpeed => currentSpeed;
        public bool IsGrounded => isGrounded;
        public CharacterVisual Visual => characterVisual;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (characterVisual == null) characterVisual = GetComponentInChildren<CharacterVisual>();
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;

            inputReader = PlayerInputReader.Instance;
            if (inputReader == null)
            {
                inputReader = gameObject.AddComponent<PlayerInputReader>();
            }

            // Setup CharacterController dimensions
            controller.center = new Vector3(0f, 1.0f, 0f);
            controller.radius = 0.35f;
            controller.height = 1.95f;
        }

        private void Update()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            CheckGrounded();
            HandleMovement();
            HandleJumpAndGravity();

            // Feed movement speed to procedural animator
            if (characterVisual != null)
            {
                characterVisual.AnimateLocomotion(currentSpeed, isGrounded);
            }
        }

        private void CheckGrounded()
        {
            // CharacterController has built-in isGrounded, supplemented by sphere cast for reliability
            Vector3 spherePos = new Vector3(transform.position.x, transform.position.y + 0.15f, transform.position.z);
            isGrounded = controller.isGrounded || Physics.CheckSphere(spherePos, groundCheckRadius, groundLayers, QueryTriggerInteraction.Ignore);

            if (isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f; // Slight downward stick
            }
        }

        private void HandleMovement()
        {
            Vector2 input = inputReader.MoveInput;
            float targetSpeed = 0f;

            if (input.sqrMagnitude > 0.01f)
            {
                targetSpeed = inputReader.IsSprinting ? sprintSpeed : walkSpeed;

                // Calculate camera-relative movement angle
                float targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
                if (cameraTransform != null)
                {
                    targetRotation += cameraTransform.eulerAngles.y;
                }

                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

                Vector3 targetDirection = Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;
                controller.Move(targetDirection.normalized * (currentSpeed * Time.deltaTime));

                // Track distance for footstep notifications
                distanceTraveled += currentSpeed * Time.deltaTime;
                if (distanceTraveled > 2.2f && isGrounded)
                {
                    distanceTraveled = 0f;
                    OnFootstep();
                }
            }

            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
        }

        private void HandleJumpAndGravity()
        {
            if (isGrounded && inputReader.JumpRequested)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Apply gravity
            verticalVelocity += gravity * Time.deltaTime;
            controller.Move(new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime);
        }

        private void OnFootstep()
        {
            // Milestone 1 EventBus integration: publishes low-impact player movement event
            // that Milestone 4 NPCs can use for line-of-sight & audio detection!
        }
    }
}
