using DCultCity.Core;
using DCultCity.Player;
using UnityEngine;

namespace DCultCity.Vehicles
{
    /// <summary>
    /// Core vehicle driving physics using Unity's Rigidbody and arcade forces.
    /// Eliminates external module dependencies (zero WheelCollider requirement).
    /// Supports responsive acceleration, speed-sensitive steering, lateral tire grip,
    /// braking, handbrake drifting, reverse gear, and full enter/exit flow with EventBus.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        // ------------------------------------------------------------------ //
        //  Inspector Fields
        // ------------------------------------------------------------------ //
        [Header("Vehicle Setup")]
        [SerializeField] public VehicleDefinition definition = new VehicleDefinition();

        [Header("Seat / Enter Point")]
        [SerializeField] private Transform driverSeatPoint = null;

        [Header("Enter Prompt")]
        [SerializeField] private KeyCode enterExitKey = KeyCode.F;

        [Header("State (read-only)")]
        [SerializeField] private bool playerInside = false;
        [SerializeField] private float currentSpeedKmh = 0f;

        // ------------------------------------------------------------------ //
        //  Private state
        // ------------------------------------------------------------------ //
        private Rigidbody rb;
        private VehicleProceduralVisual visual;
        private ThirdPersonController playerController;
        private ThirdPersonCamera playerCamera;

        private float throttleInput  = 0f;
        private float steerInput     = 0f;
        private float brakeInput     = 0f;
        private bool  handbrakeInput = false;
        private bool  reverseGear    = false;
        private bool  isGrounded     = true;

        // For on-screen prompt display
        private bool playerNearby = false;
        private float distToPlayer = float.MaxValue;

        // Cached actor id for EventBus events
        private const string PLAYER_ACTOR_ID = "Player_01";

        // ------------------------------------------------------------------ //
        //  Initialisation
        // ------------------------------------------------------------------ //
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = 1400f;
            rb.centerOfMass = new Vector3(0f, -0.4f, 0.1f); // Low center of mass for stability
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            visual = GetComponent<VehicleProceduralVisual>();
            if (visual == null) visual = gameObject.AddComponent<VehicleProceduralVisual>();
            visual.def = definition;
            visual.BuildVehicle();

            // Add a chassis collider sized to the car body
            if (GetComponent<BoxCollider>() == null)
            {
                BoxCollider chassis = gameObject.AddComponent<BoxCollider>();
                chassis.center = new Vector3(0f, 0.42f, 0f);
                chassis.size   = new Vector3(1.80f, 0.62f, 4.20f);
            }

            EnsureDriverSeat();
        }

        private void EnsureDriverSeat()
        {
            if (driverSeatPoint != null) return;
            GameObject seat = new GameObject("DriverSeat");
            seat.transform.SetParent(transform, false);
            seat.transform.localPosition = new Vector3(-0.40f, 0.75f, 0.60f);
            driverSeatPoint = seat.transform;
        }

        // ------------------------------------------------------------------ //
        //  Per-frame update
        // ------------------------------------------------------------------ //
        private void Update()
        {
            currentSpeedKmh = rb.velocity.magnitude * 3.6f;

            // Check proximity to player
            if (playerController == null)
                playerController = FindObjectOfType<ThirdPersonController>();

            if (playerController != null && !playerInside)
            {
                distToPlayer = Vector3.Distance(transform.position, playerController.transform.position);
                playerNearby = distToPlayer < definition.enterRadius;

                if (playerNearby && Input.GetKeyDown(enterExitKey))
                {
                    EnterVehicle();
                }
            }

            if (playerInside)
            {
                ReadDrivingInput();

                if (Input.GetKeyDown(enterExitKey))
                {
                    ExitVehicle();
                }

                // Snap player model to driver seat
                if (playerController != null && driverSeatPoint != null)
                {
                    playerController.transform.position = driverSeatPoint.position;
                    playerController.transform.rotation = driverSeatPoint.rotation;
                }
            }

            // Animate visual (wheels spin, front wheel steer, brake lights)
            float visualSpeed = Vector3.Dot(rb.velocity, transform.forward);
            bool isBraking = brakeInput > 0.05f || (handbrakeInput && visualSpeed > 0.5f);
            if (visual != null)
                visual.AnimateWheels(visualSpeed, steerInput * definition.maxSteerAngle, isBraking);
        }

        private void ReadDrivingInput()
        {
            // Throttle
            throttleInput = Input.GetAxis("Vertical");

            // Steer
            steerInput = Input.GetAxis("Horizontal");

            // Auto-detect reverse intention
            float fwdSpeed = Vector3.Dot(rb.velocity, transform.forward);
            reverseGear = throttleInput < -0.05f && fwdSpeed < 1.5f;

            // Brake (Space) or reverse-throttle brakes when going forward
            handbrakeInput = Input.GetKey(KeyCode.Space);
            brakeInput = handbrakeInput ? 1f : 0f;
            if (throttleInput < -0.05f && fwdSpeed > 1.5f) brakeInput = Mathf.Abs(throttleInput);
        }

        // ------------------------------------------------------------------ //
        //  Physics update (Arcade Rigidbody Model)
        // ------------------------------------------------------------------ //
        private void FixedUpdate()
        {
            // Ground check via downward raycast
            isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.6f, Vector3.down, 1.4f);

            if (!playerInside || !isGrounded) return;

            ApplySteering();
            ApplyMotorAndBraking();
            ApplyLateralFriction();
            ApplyUprightStabilization();
        }

        private void ApplySteering()
        {
            float fwdSpeed = Vector3.Dot(rb.velocity, transform.forward);
            float speedFactor = Mathf.Clamp01(Mathf.Abs(fwdSpeed) / Mathf.Max(1f, definition.topSpeed));
            float dynamicMaxSteer = Mathf.Lerp(definition.maxSteerAngle, definition.maxSteerAngle * 0.35f, speedFactor);
            float steerSign = fwdSpeed >= -0.5f ? 1f : -1f;

            float turnTorque = steerInput * dynamicMaxSteer * steerSign * 90f;
            rb.AddTorque(transform.up * turnTorque, ForceMode.Force);
        }

        private void ApplyMotorAndBraking()
        {
            float fwdSpeed = Vector3.Dot(rb.velocity, transform.forward);
            bool atTopSpeed = Mathf.Abs(fwdSpeed) >= definition.topSpeed;

            // Motor force
            if (!atTopSpeed && Mathf.Abs(throttleInput) > 0.05f)
            {
                float force = reverseGear
                    ? throttleInput * definition.reverseForce * 3.5f
                    : throttleInput * definition.motorForce * 3.5f;
                rb.AddForce(transform.forward * force, ForceMode.Force);
            }

            // Braking force
            if (brakeInput > 0.05f)
            {
                Vector3 forwardVel = transform.forward * Vector3.Dot(rb.velocity, transform.forward);
                rb.AddForce(-forwardVel * (definition.brakeForce * brakeInput * 1.5f), ForceMode.Force);
            }
            // Passive drag deceleration when coasting
            else if (Mathf.Abs(throttleInput) < 0.05f && !handbrakeInput)
            {
                Vector3 forwardVel = transform.forward * Vector3.Dot(rb.velocity, transform.forward);
                rb.AddForce(-forwardVel * (definition.dragBrakeForce * 1.5f), ForceMode.Force);
            }
        }

        private void ApplyLateralFriction()
        {
            // Cancel sideways sliding unless drifting with handbrake
            Vector3 sideVelocity = transform.right * Vector3.Dot(rb.velocity, transform.right);
            float grip = handbrakeInput ? 0.35f : 0.95f;
            rb.AddForce(-sideVelocity * (rb.mass * 8.5f * grip), ForceMode.Force);
        }

        private void ApplyUprightStabilization()
        {
            // Subtle torque to prevent the car from rolling onto its roof on rough terrain
            Vector3 predictedUp = Quaternion.AngleAxis(rb.angularVelocity.magnitude * Mathf.Rad2Deg * 0.1f / 8f, rb.angularVelocity) * transform.up;
            Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
            rb.AddTorque(torqueVector * (rb.mass * 10f), ForceMode.Force);
        }

        // ------------------------------------------------------------------ //
        //  Enter / Exit state machine
        // ------------------------------------------------------------------ //
        private void EnterVehicle()
        {
            if (playerInside) return;
            playerInside = true;

            if (playerCamera == null)
                playerCamera = FindObjectOfType<ThirdPersonCamera>();

            // Disable player control while driving
            if (playerController != null)
            {
                playerController.enabled = false;

                foreach (var r in playerController.GetComponentsInChildren<Renderer>())
                    r.enabled = false;
            }

            // Camera tracks vehicle
            if (playerCamera != null)
            {
                playerCamera.Target = transform;
            }

            EventBus.Publish(new VehicleOccupancyEvent(
                definition.vehicleId,
                VehicleOccupancyEvent.OccupancyChange.Entered,
                PLAYER_ACTOR_ID,
                transform.position));

            Debug.Log($"<color=#FFAA00>[VehicleController]</color> Player entered '{definition.vehicleId}'. Speed: {currentSpeedKmh:F0} km/h");
        }

        private void ExitVehicle()
        {
            if (!playerInside) return;
            playerInside = false;

            Vector3 exitWorld = transform.TransformPoint(definition.exitOffset);

            if (playerController != null)
            {
                playerController.transform.position = exitWorld + Vector3.up * 0.1f;
                playerController.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                playerController.enabled = true;

                foreach (var r in playerController.GetComponentsInChildren<Renderer>())
                    r.enabled = true;
            }

            if (playerCamera != null && playerController != null)
                playerCamera.Target = playerController.transform;

            throttleInput = 0f;
            steerInput    = 0f;

            EventBus.Publish(new VehicleOccupancyEvent(
                definition.vehicleId,
                VehicleOccupancyEvent.OccupancyChange.Exited,
                PLAYER_ACTOR_ID,
                transform.position));

            Debug.Log($"<color=#00FFCC>[VehicleController]</color> Player exited '{definition.vehicleId}'.");
        }

        // ------------------------------------------------------------------ //
        //  Collision events → EventBus
        // ------------------------------------------------------------------ //
        private void OnCollisionEnter(Collision col)
        {
            if (!playerInside) return;

            float force = col.impulse.magnitude;
            if (force < 120f) return;

            EventBus.Publish(new VehicleCollisionEvent(
                definition.vehicleId,
                PLAYER_ACTOR_ID,
                col.gameObject.tag,
                force,
                col.contacts[0].point));

            if (col.gameObject.CompareTag("NPC") || col.gameObject.CompareTag("Pedestrian"))
            {
                EventBus.Publish(new PlayerActionEvent(
                    actorId:          PLAYER_ACTOR_ID,
                    actionType:       ActionType.HitPedestrian,
                    location:         col.contacts[0].point,
                    targetId:         col.gameObject.name,
                    reputationDelta:  -0.65f,
                    trustDelta:       -0.80f,
                    actionName:       "Vehicular impact with pedestrian"));
            }
        }

        // ------------------------------------------------------------------ //
        //  Mobile input overrides (called by VehicleMobileHUD)
        // ------------------------------------------------------------------ //
        public void SetMobileThrottle(float val) { if (playerInside) throttleInput  = val; }
        public void SetMobileSteer  (float val) { if (playerInside) steerInput     = val; }
        public void SetMobileBrake  (bool  val) { if (playerInside) handbrakeInput = val; }

        // ------------------------------------------------------------------ //
        //  Accessors
        // ------------------------------------------------------------------ //
        public bool  IsOccupied     => playerInside;
        public float SpeedKmh       => currentSpeedKmh;
        public bool  IsPlayerNearby => playerNearby;
        public float DistToPlayer   => distToPlayer;

        // ------------------------------------------------------------------ //
        //  On-screen enter prompt (legacy OnGUI for zero-setup)
        // ------------------------------------------------------------------ //
        private GUIStyle promptStyle;
        private void OnGUI()
        {
            if (!playerNearby || playerInside) return;

            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = Mathf.RoundToInt(14 * Mathf.Max(1f, Screen.height / 720f)),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                promptStyle.normal.textColor = Color.white;
            }

            float scale = Mathf.Max(1f, Screen.height / 720f);
            float w = 260f * scale;
            float h = 42f  * scale;
            GUI.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 0.92f);
            GUI.Box(new Rect((Screen.width - w) * 0.5f, Screen.height * 0.68f, w, h),
                    $"[F] Enter {definition.vehicleId}", promptStyle);
            GUI.backgroundColor = Color.white;
        }
    }
}
