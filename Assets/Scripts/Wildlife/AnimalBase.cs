using UnityEngine;
using UnityEngine.AI;

namespace DCultCity.Wildlife
{
    public enum AnimalSpecies
    {
        ForestDeer,
        MountainWolf,
        WildRabbit
    }

    /// <summary>
    /// Autonomous animal behavior in D-CULT CITY.
    /// Handles grazing, idle animations, boundary wandering,
    /// and panicked fleeing when player or vehicles come too close.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class AnimalBase : MonoBehaviour
    {
        [Header("Species Identity")]
        [SerializeField] private AnimalSpecies species = AnimalSpecies.ForestDeer;
        [SerializeField] private float wanderRadius = 15f;
        [SerializeField] private float fleeDistance = 9f;
        [SerializeField] private float walkSpeed = 2.2f;
        [SerializeField] private float sprintSpeed = 6.5f;

        private NavMeshAgent agent;
        private Transform playerTf;
        private float stateTimer = 0f;
        private bool isFleeing = false;

        private Transform headBone;
        private Transform bodyBone;
        private float animCycle = 0f;

        public AnimalSpecies Species => species;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = walkSpeed;
            agent.stoppingDistance = 0.5f;
            BuildAnimalVisual();
        }

        private void Start()
        {
            var p = FindObjectOfType<DCultCity.Player.ThirdPersonController>();
            if (p != null) playerTf = p.transform;
            ChooseNewDestination();
        }

        private void Update()
        {
            if (playerTf == null)
            {
                var p = FindObjectOfType<DCultCity.Player.ThirdPersonController>();
                if (p != null) playerTf = p.transform;
                return;
            }

            float distToPlayer = Vector3.Distance(transform.position, playerTf.position);

            if (distToPlayer < fleeDistance)
            {
                // Panic flee
                if (!isFleeing)
                {
                    isFleeing = true;
                    agent.speed = sprintSpeed;
                }

                Vector3 fleeDir = (transform.position - playerTf.position).normalized;
                Vector3 fleeTarget = transform.position + fleeDir * 16f;

                if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, 16f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
            else if (isFleeing)
            {
                // Recovered from flee
                isFleeing = false;
                agent.speed = walkSpeed;
                stateTimer = 4f;
            }
            else
            {
                // Normal wander / graze
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    ChooseNewDestination();
                }
            }

            AnimateProcedural();
        }

        private void ChooseNewDestination()
        {
            stateTimer = Random.Range(4f, 9f);
            if (!agent.isOnNavMesh) return;

            Vector3 rand = Random.insideUnitSphere * wanderRadius;
            rand += transform.position;

            if (NavMesh.SamplePosition(rand, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        private void AnimateProcedural()
        {
            float speed = agent.velocity.magnitude;
            if (speed > 0.1f)
            {
                animCycle += Time.deltaTime * speed * 4f;
                if (headBone != null)
                {
                    headBone.localRotation = Quaternion.Euler(Mathf.Sin(animCycle) * 12f, 0f, 0f);
                }
            }
            else
            {
                // Grazing head-bob
                if (headBone != null)
                {
                    headBone.localRotation = Quaternion.Euler(25f + Mathf.Sin(Time.time * 1.5f) * 8f, 0f, 0f);
                }
            }
        }

        private void BuildAnimalVisual()
        {
            GameObject model = new GameObject("Model_Creature");
            model.transform.SetParent(transform, false);

            Color bodyCol = species switch
            {
                AnimalSpecies.MountainWolf => new Color(0.45f, 0.45f, 0.48f),
                AnimalSpecies.WildRabbit => new Color(0.85f, 0.85f, 0.85f),
                _ => new Color(0.55f, 0.35f, 0.20f) // Forest Deer
            };

            float scale = species == AnimalSpecies.WildRabbit ? 0.35f : (species == AnimalSpecies.MountainWolf ? 0.85f : 1.1f);

            // Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(model.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.65f * scale, 0f);
            body.transform.localScale = new Vector3(0.5f * scale, 0.5f * scale, 1.1f * scale);
            bodyBone = body.transform;
            SetColor(body, bodyCol);

            // Head / Neck
            GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neck.name = "Neck";
            neck.transform.SetParent(body.transform, false);
            neck.transform.localPosition = new Vector3(0f, 0.5f, 0.55f);
            neck.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);
            SetColor(neck, bodyCol);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(neck.transform, false);
            head.transform.localPosition = new Vector3(0f, 0.6f, 0.2f);
            head.transform.localScale = new Vector3(0.85f, 0.7f, 1.1f);
            headBone = head.transform;
            SetColor(head, bodyCol);

            // Antlers if Deer
            if (species == AnimalSpecies.ForestDeer)
            {
                GameObject antlerL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                antlerL.transform.SetParent(head.transform, false);
                antlerL.transform.localPosition = new Vector3(-0.35f, 0.6f, 0f);
                antlerL.transform.localScale = new Vector3(0.08f, 0.45f, 0.08f);
                antlerL.transform.localRotation = Quaternion.Euler(15f, 0f, -25f);
                SetColor(antlerL, new Color(0.75f, 0.70f, 0.60f));

                GameObject antlerR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                antlerR.transform.SetParent(head.transform, false);
                antlerR.transform.localPosition = new Vector3(0.35f, 0.6f, 0f);
                antlerR.transform.localScale = new Vector3(0.08f, 0.45f, 0.08f);
                antlerR.transform.localRotation = Quaternion.Euler(15f, 0f, 25f);
                SetColor(antlerR, new Color(0.75f, 0.70f, 0.60f));
            }
        }

        private void SetColor(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            Destroy(go.GetComponent<Collider>()); // Let NavMesh handle collision

            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Diffuse");

            Material m = new Material(s);
            m.color = color;
            r.sharedMaterial = m;
        }
    }
}
