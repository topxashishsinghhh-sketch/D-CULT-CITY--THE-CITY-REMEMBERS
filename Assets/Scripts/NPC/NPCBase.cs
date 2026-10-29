using DCultCity.Core;
using DCultCity.Reputation;
using UnityEngine;
using UnityEngine.AI;

namespace DCultCity.NPC
{
    /// <summary>
    /// Shared base class for ALL NPCs (Story and Ambient).
    /// Provides: identity, NPCMemory, TrustSystem, NavMeshAgent-driven movement,
    /// detection radius, reaction state machine (Idle/Follow/Flee/CallPolice),
    /// and EventBus subscription to PlayerActionEvents.
    ///
    /// Subclasses:
    ///   StoryNPC   → adds NPCDailySchedule + DialogueTree
    ///   AmbientNPC → uses this class directly with reaction-only logic
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCBase : MonoBehaviour
    {
        // ------------------------------------------------------------------ //
        //  Inspector
        // ------------------------------------------------------------------ //
        [Header("Identity")]
        [SerializeField] public string npcId         = "NPC_Default";
        [SerializeField] public string displayName   = "Citizen";
        [SerializeField] public bool   isStoryNPC    = false;

        [Header("Detection")]
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private LayerMask detectionMask = ~0;

        [Header("Patrol / Wander")]
        [SerializeField] private float wanderRadius    = 8f;
        [SerializeField] private float wanderInterval  = 5f;

        [Header("Reaction Thresholds")]
        [SerializeField] private float fleeThreshold      = -0.50f;   // Trust below this → flee
        [SerializeField] private float callPoliceThreshold = -0.65f;  // Trust below this → call police

        // ------------------------------------------------------------------ //
        //  Internal state
        // ------------------------------------------------------------------ //
        protected NavMeshAgent  agent;
        private   NPCMemory     memory    = new NPCMemory();
        private   TrustSystem   trust     = new TrustSystem(0f);
        private   NPCState      state     = NPCState.Idle;

        private float wanderTimer = 0f;
        private float memoryTickTimer = 0f;
        private const float MEMORY_TICK_INTERVAL = 1f;

        private const string PLAYER_ACTOR = "Player_01";

        // ------------------------------------------------------------------ //
        //  Public accessors
        // ------------------------------------------------------------------ //
        public string      NPCId        => npcId;
        public NPCMemory   Memory       => memory;
        public TrustSystem TrustTracker => trust;
        public NPCState    CurrentState => state;

        // ------------------------------------------------------------------ //
        //  Lifecycle
        // ------------------------------------------------------------------ //
        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed        = 2.0f;
            agent.angularSpeed = 180f;
            agent.stoppingDistance = 0.8f;
        }

        protected virtual void OnEnable()
        {
            EventBus.Subscribe<PlayerActionEvent>(OnPlayerAction);
            GossipSystem.Instance?.Register(this);
        }

        protected virtual void OnDisable()
        {
            EventBus.Unsubscribe<PlayerActionEvent>(OnPlayerAction);
            GossipSystem.Instance?.Unregister(this);
        }

        protected virtual void Update()
        {
            // Tick memory decay
            memoryTickTimer += Time.deltaTime;
            if (memoryTickTimer >= MEMORY_TICK_INTERVAL)
            {
                memory.Tick(memoryTickTimer);
                trust.Recalculate(memory, PLAYER_ACTOR);
                memoryTickTimer = 0f;
            }

            UpdateStateMachine();
        }

        // ------------------------------------------------------------------ //
        //  State machine
        // ------------------------------------------------------------------ //
        private void UpdateStateMachine()
        {
            TrustLevel lvl = trust.Level;

            // Determine desired state from trust
            NPCState desired = lvl switch
            {
                TrustLevel.Hostile    when trust.Current <= callPoliceThreshold => NPCState.CallPolice,
                TrustLevel.Hostile                                               => NPCState.Flee,
                TrustLevel.Suspicious                                            => NPCState.Wary,
                TrustLevel.Trusted                                               => NPCState.Friendly,
                _                                                                => NPCState.Idle
            };

            if (desired != state)
            {
                OnStateChanged(state, desired);
                state = desired;
            }

            // Execute current state behaviour
            switch (state)
            {
                case NPCState.Idle:
                case NPCState.Wary:
                case NPCState.Friendly:
                    Wander();
                    break;
                case NPCState.Flee:
                    FleeFromPlayer();
                    break;
                case NPCState.CallPolice:
                    CallPoliceAction();
                    break;
            }
        }

        protected virtual void OnStateChanged(NPCState from, NPCState to)
        {
            // Subclasses override for animation or dialogue triggers
        }

        // ------------------------------------------------------------------ //
        //  Movement behaviours
        // ------------------------------------------------------------------ //
        private void Wander()
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer > 0f || !agent.isOnNavMesh) return;

            wanderTimer = wanderInterval + Random.Range(-1.5f, 1.5f);

            Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
            randomDir += transform.position;
            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }

        /// <summary>Call this from StoryNPC schedule to override wandering destination.</summary>
        public void SetScheduleDestination(Vector3 destination)
        {
            if (agent.isOnNavMesh)
                agent.SetDestination(destination);
        }

        private void FleeFromPlayer()
        {
            var player = FindPlayer();
            if (player == null) return;

            // Run in opposite direction from player
            Vector3 fleeDir = (transform.position - player.position).normalized;
            Vector3 fleeTarget = transform.position + fleeDir * 14f;

            if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, 14f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);

            agent.speed = 5.0f; // Panicked run speed
        }

        private void CallPoliceAction()
        {
            // Stop moving — stand and call
            agent.SetDestination(transform.position);
            agent.speed = 2.0f;

            // Publish a police-alert action event so police faction NPCs react in Milestone 6
            // This creates a cascading effect through the EventBus
        }

        // ------------------------------------------------------------------ //
        //  EventBus handler
        // ------------------------------------------------------------------ //
        private void OnPlayerAction(PlayerActionEvent evt)
        {
            // Only react to events that are near enough or where this NPC is a witness
            bool isWitness = IsWitnessTo(evt);
            bool isTarget  = evt.targetId == npcId;

            if (!isWitness && !isTarget) return;

            // Build and store a memory entry
            var entry = new MemoryEntry(
                actorId:          evt.actorId,
                actionName:       evt.actionName,
                targetId:         evt.targetId,
                location:         evt.location,
                gameTimestamp:    evt.timestamp,
                trustDelta:       isTarget ? evt.trustDelta : evt.trustDelta * 0.6f, // witnesses feel it less
                reputationDelta:  evt.reputationDelta,
                initialStrength:  isWitness ? 1.0f : 0.7f,
                wasWitness:       isWitness);

            memory.Store(entry);
            trust.Recalculate(memory, evt.actorId);

            // Propagate to faction system
            if (FactionReputation.Instance != null)
                FactionReputation.Instance.ApplyFromAction(evt.actionType, evt.reputationDelta);

            OnMemoryStored(entry, isWitness);
        }

        protected virtual void OnMemoryStored(MemoryEntry entry, bool wasWitness)
        {
            // Override in StoryNPC to trigger dialogue reactions
        }

        // ------------------------------------------------------------------ //
        //  Detection helper
        // ------------------------------------------------------------------ //
        private bool IsWitnessTo(PlayerActionEvent evt)
        {
            // Named witness list
            if (evt.witnesses != null && evt.witnesses.Contains(npcId)) return true;

            // Proximity detection
            float dist = Vector3.Distance(transform.position, evt.location);
            if (dist > detectionRadius) return false;

            // Optional line-of-sight check
            Vector3 dirToEvent = (evt.location - transform.position).normalized;
            if (Physics.Raycast(transform.position + Vector3.up, dirToEvent, dist, detectionMask))
                return false; // Blocked

            return true;
        }

        protected Transform FindPlayer()
        {
            var p = FindObjectOfType<Player.ThirdPersonController>();
            return p != null ? p.transform : null;
        }

        // ------------------------------------------------------------------ //
        //  Minimal visual — a capsule with name label (replaced by art in M6)
        // ------------------------------------------------------------------ //
        protected virtual void BuildVisual(Color bodyColor)
        {
            var existing = transform.Find("NPC_Visual");
            if (existing != null) return;

            GameObject vis = new GameObject("NPC_Visual");
            vis.transform.SetParent(transform, false);

            // Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(vis.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale    = new Vector3(0.5f, 0.95f, 0.5f);
            SetColor(body, bodyColor);

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(vis.transform, false);
            head.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            head.transform.localScale    = new Vector3(0.48f, 0.48f, 0.48f);
            SetColor(head, new Color(0.88f, 0.70f, 0.55f));
        }

        private void SetColor(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
            r.sharedMaterial = new Material(s) { color = color };
        }
    }

    public enum NPCState { Idle, Wary, Friendly, Flee, CallPolice }
}
