using DCultCity.Core;
using UnityEngine;
using UnityEngine.AI;

namespace DCultCity.Reputation
{
    /// <summary>
    /// Manages real-time police pursuits when the player has a Wanted Level (1-5 stars).
    /// Features:
    /// - Spawns a Police Cruiser interceptor with alternating Red/Blue emergency flashing strobe lights
    /// - Chases player vehicle or on foot using NavMesh navigation
    /// - Activates procedural police siren audio
    /// - Evade mechanism: Hiding / staying > 28m away for 12s clears the wanted level ("EVADED!")
    /// - Busted mechanic: If police reach within 2.5m, player is arrested and fined.
    /// </summary>
    public class PolicePursuitManager : MonoBehaviour
    {
        public static PolicePursuitManager Instance { get; private set; }

        [Header("Pursuit Settings")]
        [SerializeField] private float evadeTimeRequired = 12f;
        [SerializeField] private float arrestDistance = 2.8f;
        [SerializeField] private float policeSpeed = 7.5f;

        private GameObject policeCruiserObj;
        private NavMeshAgent policeAgent;
        private Light redStrobe;
        private Light blueStrobe;

        private Transform playerTf;
        private float evadeTimer = 0f;
        private bool isEvading = false;
        private string pursuitStatusText = "";

        public string PursuitStatusText => pursuitStatusText;
        public float EvadeProgress => isEvading ? (evadeTimer / evadeTimeRequired) : 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            var p = FindObjectOfType<DCultCity.Player.ThirdPersonController>();
            if (p != null) playerTf = p.transform;
            BuildPoliceCruiser();
        }

        private void Update()
        {
            if (FactionReputation.Instance == null) return;
            if (playerTf == null)
            {
                var p = FindObjectOfType<DCultCity.Player.ThirdPersonController>();
                if (p != null) playerTf = p.transform;
                return;
            }

            WantedLevel wanted = FactionReputation.Instance.GetWantedLevel();

            if (wanted != WantedLevel.None)
            {
                RunPursuit(wanted);
            }
            else
            {
                StopPursuit();
            }
        }

        private void RunPursuit(WantedLevel wanted)
        {
            if (policeCruiserObj == null) BuildPoliceCruiser();

            policeCruiserObj.SetActive(true);
            ProceduralAudioEngine.Instance?.SetSirenActive(true);

            // Alternate emergency strobes (4 Hz flashing)
            float strobe = Mathf.Sin(Time.time * 24f);
            if (redStrobe != null) redStrobe.intensity = strobe > 0f ? 2.5f : 0f;
            if (blueStrobe != null) blueStrobe.intensity = strobe <= 0f ? 2.5f : 0f;

            // Target player position
            if (policeAgent != null && policeAgent.isOnNavMesh)
            {
                policeAgent.speed = policeSpeed + ((int)wanted * 0.8f);
                policeAgent.SetDestination(playerTf.position);
            }

            float dist = Vector3.Distance(policeCruiserObj.transform.position, playerTf.position);

            // Busted condition
            if (dist < arrestDistance)
            {
                OnPlayerArrested();
                return;
            }

            // Evade condition (player is far away or breaking chase)
            if (dist > 28f)
            {
                isEvading = true;
                evadeTimer += Time.deltaTime;
                pursuitStatusText = $"EVADING POLICE... ({Mathf.CeilToInt(evadeTimeRequired - evadeTimer)}s)";

                if (evadeTimer >= evadeTimeRequired)
                {
                    OnEvadeSuccessful();
                }
            }
            else
            {
                isEvading = false;
                evadeTimer = Mathf.Max(0f, evadeTimer - Time.deltaTime * 2f);
                pursuitStatusText = "POLICE IN PURSUIT!";
            }
        }

        private void StopPursuit()
        {
            if (policeCruiserObj != null && policeCruiserObj.activeSelf)
            {
                policeCruiserObj.SetActive(false);
            }
            if (redStrobe != null) redStrobe.intensity = 0f;
            if (blueStrobe != null) blueStrobe.intensity = 0f;

            ProceduralAudioEngine.Instance?.SetSirenActive(false);
            isEvading = false;
            evadeTimer = 0f;
            pursuitStatusText = "";
        }

        private void OnEvadeSuccessful()
        {
            // Reset police reputation back to neutral
            FactionReputation.Instance?.Apply("Police", 0.6f);
            StopPursuit();
            Debug.Log("<color=#00FFAA>[PolicePursuitManager]</color> Successfully EVADED the police! Wanted level cleared.");
        }

        private void OnPlayerArrested()
        {
            Debug.LogWarning("<color=#FF4444>[PolicePursuitManager]</color> BUSTED! Arrested by Precinct 4 Officers.");

            // Clear wanted level & fine
            FactionReputation.Instance?.Apply("Police", 0.75f);
            StopPursuit();

            // Respawn player outside precinct
            if (playerTf != null)
            {
                var cc = playerTf.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                playerTf.position = new Vector3(-14f, 0.5f, 10f); // In front of police station
                if (cc != null) cc.enabled = true;
            }

            EventBus.Publish(new PlayerActionEvent("Police_Precinct4", ActionType.Custom,
                playerTf.position, "Player_01", 0f, 0f, "Player booked and released at precinct"));
        }

        private void BuildPoliceCruiser()
        {
            policeCruiserObj = new GameObject("Police_Interceptor_Patrol");
            policeCruiserObj.transform.position = new Vector3(-14f, 0.4f, 16f); // Spawns at precinct

            // Chassis
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "CruiserBody";
            body.transform.SetParent(policeCruiserObj.transform, false);
            body.transform.localScale = new Vector3(1.8f, 0.6f, 4.2f);
            SetMaterial(body, new Color(0.12f, 0.12f, 0.15f)); // Police Black & White

            // White door panels
            GameObject doorW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorW.name = "DoorDecal";
            doorW.transform.SetParent(body.transform, false);
            doorW.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            doorW.transform.localScale = new Vector3(1.02f, 0.8f, 0.55f);
            SetMaterial(doorW, Color.white);

            // Lightbar on roof
            GameObject lightbar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lightbar.name = "Lightbar";
            lightbar.transform.SetParent(policeCruiserObj.transform, false);
            lightbar.transform.localPosition = new Vector3(0f, 0.75f, 0.1f);
            lightbar.transform.localScale = new Vector3(1.1f, 0.18f, 0.35f);
            SetMaterial(lightbar, new Color(0.1f, 0.1f, 0.1f));

            // Red Strobe
            GameObject redLightObj = new GameObject("RedStrobe");
            redLightObj.transform.SetParent(lightbar.transform, false);
            redLightObj.transform.localPosition = new Vector3(-0.35f, 0.3f, 0f);
            redStrobe = redLightObj.AddComponent<Light>();
            redStrobe.type = LightType.Point;
            redStrobe.color = Color.red;
            redStrobe.range = 14f;
            redStrobe.intensity = 0f;

            // Blue Strobe
            GameObject blueLightObj = new GameObject("BlueStrobe");
            blueLightObj.transform.SetParent(lightbar.transform, false);
            blueLightObj.transform.localPosition = new Vector3(0.35f, 0.3f, 0f);
            blueStrobe = blueLightObj.AddComponent<Light>();
            blueStrobe.type = LightType.Point;
            blueStrobe.color = Color.cyan;
            blueStrobe.range = 14f;
            blueStrobe.intensity = 0f;

            // NavMeshAgent for chase
            policeAgent = policeCruiserObj.AddComponent<NavMeshAgent>();
            policeAgent.speed = policeSpeed;
            policeAgent.angularSpeed = 240f;
            policeAgent.stoppingDistance = arrestDistance;

            policeCruiserObj.SetActive(false);
        }

        private void SetMaterial(GameObject go, Color col)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            Destroy(go.GetComponent<Collider>()); // Avoid physics snag during agent chase

            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Diffuse");

            Material m = new Material(s);
            m.color = col;
            r.sharedMaterial = m;
        }
    }
}
