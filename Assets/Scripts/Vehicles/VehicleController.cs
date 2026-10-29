using DCultCity.Core;
using DCultCity.Player;
using UnityEngine;

namespace DCultCity.Vehicles
{
    /// <summary>
    /// Core vehicle driving physics using Unity's Rigidbody + WheelColliders (4 total).
    /// Supports arcade-feel acceleration, speed-sensitive steering, braking, handbrake,
    /// gear-based reverse, and full enter/exit flow with EventBus integration.
    ///
    /// Architecture:
    ///   VehicleDefinition  → tuning data
    ///   VehicleController  → physics + input + enter/exit state machine
    ///   VehicleProceduralVisual → mesh, wheel spin, brake lights
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        // ------------------------------------------------------------------ //
        //  Inspector Fields
        // ------------------------------------------------------------------ //
        [Header("Vehicle Setup")]
        [SerializeField] public VehicleDefinition definition = new VehicleDefinition();

        [Header("Wheel Colliders - Assign in Inspector or auto-built at runtime")]
        [SerializeField] private WheelCollider wheelFL = null;
        [SerializeField] private WheelCollider wheelFR = null;
        [SerializeField] private WheelCollider wheelRL = null;
        [SerializeField] private WheelCollider wheelRR = null;

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

        private bool  wheelCollidersBuilt = false;

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
            rb.centerOfMass = new Vector3(0f, -0.4f, 0.1f); // Low CoM for stability
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            visual = GetComponent<VehicleProceduralVisual>();
            if (visual == null) visual = gameObject.AddComponent<VehicleProceduralVisual>();
            visual.def = definition;
            visual.BuildVehicle();

            // Add a simple chassis collider sized to the body
            if (GetComponent<BoxCollider>() == null)
            {
                BoxCollider chassis = gameObject.AddComponent<BoxCollider>();
                chassis.center = new Vector3(0f, 0.42f, 0f);
                chassis.size   = new Vector3(1.80f, 0.62f, 4.20f);
            }

            EnsureWheelColliders();
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

        private void EnsureWheelColliders()
        {
            if (wheelCollidersBuilt) return;

            // Wheel collider positions matching visual
            Vector3[] positions = {
                new Vector3(-0.95f, 0.25f,  1.30f),
                new Vector3( 0.95f, 0.25f,  1.30f),
                new Vector3(-0.95f, 0.25f, -1.30f),
                new Vector3( 0.95f, 0.25f, -1.30f),
            };

            WheelCollider[] built = new WheelCollider[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject wgo = new GameObject($"WheelCol_{i}");
                wgo.transform.SetParent(transform, false);
                wgo.transform.localPosition = positions[i];

                WheelCollider wc = wgo.AddComponent<WheelCollider>();
                wc.radius  = 0.32f;
                wc.mass    = 25f;
                wc.suspensionDistance = 0.18f;

                // Spring & damper suspension
                JointSpring spring = wc.suspensionSpring;
                spring.spring   = 38000f;
                spring.damper   = 4500f;
                spring.targetPosition = 0.45f;
                wc.suspensionSpring = spring;

                // Friction curves — arcade-tuned for fun
                WheelFrictionCurve fwd = wc.forwardFriction;
                fwd.stiffness = 1.6f;
                wc.forwardFriction = fwd;

                WheelFrictionCurve side = wc.sidewaysFriction;
                side.stiffness = 1.4f;
                wc.sidewaysFriction = side;

                built[i] = wc;
            }

            if (wheelFL == null) wheelFL = built[0];
            if (wheelFR == null) wheelFR = built[1];
            if (wheelRL == null) wheelRL = built[2];
            if (wheelRR == null) wheelRR = built[3];

            wheelCollidersBuilt = true;
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
                // Read driving inputs regardless of platform
                ReadDrivingInput();

                if (Input.GetKeyDown(enterExitKey))
                {
                    ExitVehicle();
                }

                // Snap player model to seat
                if (playerController != null)
                {
                    playerController.transform.position = driverSeatPoint.position;
                    playerController.transform.rotation = driverSeatPoint.rotation;
                }
            }

            // Animate visual
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

            // Mobile overrides set by VehicleMobileHUD (see below)
        }

        // ------------------------------------------------------------------ //
        //  Physics update
        // ------------------------------------------------------------------ //
        private void FixedUpdate()
        {
            if (!playerInside) return;

            ApplySteering();
            ApplyMotorAndBraking();
        }

        private void ApplySteering()
        {
            // Speed-sensitive steering: narrows at higher speeds for stability
            float speedFactor = Mathf.Clamp01(rb.velocity.magnitude * definition.steerSpeedRatio);
            float dynamicMaxSteer = Mathf.Lerp(definition.maxSteerAngle, definition.maxSteerAngle * 0.3f, speedFactor);

            float targetAngle = steerInput * dynamicMaxSteer;
            wheelFL.steerAngle = Mathf.Lerp(wheelFL.steerAngle, targetAngle, Time.fixedDeltaTime * 8f);
            wheelFR.steerAngle = wheelFL.steerAngle;
        }

        private void ApplyMotorAndBraking()
        {
            float fwdSpeed = Vector3.Dot(rb.velocity, transform.forward);
            bool atTopSpeed = Mathf.Abs(fwdSpeed) >= definition.topSpeed;

            // Torque
            float motor = 0f;
            if (!atTopSpeed)
            {
                if (reverseGear)
                    motor = throttleInput * definition.reverseForce;   // negative input × negative force
                else
                    motor = throttleInput * definition.motorForce;
            }

            // Apply motor to rear wheels (RWD)
            wheelRL.motorTorque = motor;
            wheelRR.motorTorque = motor;

            // Braking
            float brake = brakeInput * definition.brakeForce;

            // Handbrake: locks rear wheels (classic drift/stop)
            if (handbrakeInput)
            {
                wheelRL.brakeTorque = definition.brakeForce * 2.5f;
                wheelRR.brakeTorque = definition.brakeForce * 2.5f;
                wheelFL.brakeTorque = 0f;
                wheelFR.brakeTorque = 0f;
            }
            else
            {
                wheelFL.brakeTorque = brake;
                wheelFR.brakeTorque = brake;
                wheelRL.brakeTorque = brake;
                wheelRR.brakeTorque = brake;
            }

            // Drag brake: passive deceleration on no input
            if (Mathf.Abs(throttleInput) < 0.05f && !handbrakeInput)
            {
                wheelRL.brakeTorque = definition.dragBrakeForce;
                wheelRR.brakeTorque = definition.dragBrakeForce;
            }
        }

        // ------------------------------------------------------------------ //
        //  Enter / Exit state machine
        // ------------------------------------------------------------------ //
        private void EnterVehicle()
        {
            if (playerInside) return;
            playerInside = true;

            // Cache camera reference
            if (playerCamera == null)
                playerCamera = FindObjectOfType<ThirdPersonCamera>();

            // Hide player visual (model sits inside car)
            if (playerController != null)
            {
                playerController.enabled = false;

                // Hide character mesh children
                foreach (var r in playerController.GetComponentsInChildren<Renderer>())
                    r.enabled = false;
            }

            // Reassign camera to this vehicle
            if (playerCamera != null)
            {
                playerCamera.Target = transform;
            }

            // Publish event
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

            // Move player to exit offset (world-space)
            Vector3 exitWorld = transform.TransformPoint(definition.exitOffset);

            if (playerController != null)
            {
                playerController.transform.position = exitWorld + Vector3.up * 0.1f;
                playerController.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                playerController.enabled = true;

                // Restore character mesh
                foreach (var r in playerController.GetComponentsInChildren<Renderer>())
                    r.enabled = true;
            }

            // Return camera to player
            if (playerCamera != null && playerController != null)
                playerCamera.Target = playerController.transform;

            // Clear wheel torques so car doesn't roll away
            foreach (var wc in new[] { wheelFL, wheelFR, wheelRL, wheelRR })
            {
                if (wc != null)
                {
                    wc.motorTorque = 0f;
                    wc.brakeTorque = definition.brakeForce;
                }
            }
            throttleInput = 0f;
            steerInput    = 0f;

            // Publish event
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
            if (force < 120f) return; // Ignore trivial contact

            EventBus.Publish(new VehicleCollisionEvent(
                definition.vehicleId,
                PLAYER_ACTOR_ID,
                col.gameObject.tag,
                force,
                col.contacts[0].point));

            // Wrap as PlayerActionEvent so Milestone 1 listener also catches it
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
