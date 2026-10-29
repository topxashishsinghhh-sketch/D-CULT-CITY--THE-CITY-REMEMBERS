using DCultCity.Core;
using DCultCity.Reputation;
using UnityEngine;

namespace DCultCity.Missions
{
    /// <summary>Side quest — Street Race through the city circuit.</summary>
    public class SideQuest_StreetRace : MissionBase
    {
        public override string         MissionId   => "side_race_01";
        public override string         Title       => "Street Legend";
        public override string         Description => "Sam has laid out a race circuit through the city. " +
                                                      "Hit all 4 checkpoints before the 90-second timer runs out.";
        public override MissionFaction  Faction    => MissionFaction.Racing;

        private readonly Vector3[] checkpoints =
        {
            new Vector3(  5f, 0f,  5f),
            new Vector3( 20f, 0f, 10f),
            new Vector3( 18f, 0f,-20f),
            new Vector3( -5f, 0f,-15f)
        };
        private int   nextCheckpoint = 0;
        private float timeRemaining  = 90f;
        private readonly System.Collections.Generic.List<GameObject> markers = new();

        protected override void OnActivate()
        {
            objectives.Add(new MissionObjective("Complete race circuit in under 90 seconds"));
            foreach (var cp in checkpoints)
                markers.Add(CreateCheckpointMarker(cp));
        }

        public override void Tick(float deltaTime)
        {
            if (Status != MissionStatus.Active) return;

            timeRemaining -= deltaTime;
            if (timeRemaining <= 0f) { Fail("Time's up! Better luck next race."); return; }

            if (nextCheckpoint < checkpoints.Length &&
                DistToPlayer(checkpoints[nextCheckpoint]) < 5f)
            {
                if (markers.Count > nextCheckpoint && markers[nextCheckpoint] != null)
                    Object.Destroy(markers[nextCheckpoint]);

                nextCheckpoint++;
                NotifyObjectiveUpdated();
                Debug.Log($"[Race] Checkpoint {nextCheckpoint}/{checkpoints.Length}! {timeRemaining:F0}s remaining.");

                if (nextCheckpoint >= checkpoints.Length)
                {
                    FactionReputation.Instance?.Apply("Civilians", 0.10f);
                    EventBus.Publish(new PlayerActionEvent("Player_01", ActionType.Custom,
                        GetPlayer()?.position ?? Vector3.zero, "Sam_RaceOrganizer",
                        0.10f, 0.15f, $"Won street race in {90f - timeRemaining:F1}s"));
                    Complete();
                }
            }
        }

        public string GetTimerDisplay() => $"{Mathf.CeilToInt(timeRemaining)}s";

        private GameObject CreateCheckpointMarker(Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "CheckpointMarker";
            go.transform.position = pos;
            go.transform.localScale = new Vector3(3f, 3f, 3f);
            Object.Destroy(go.GetComponent<Collider>());
            var r = go.GetComponent<Renderer>();
            if (r)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                r.sharedMaterial = new Material(s) { color = new Color(0.2f, 1f, 0.6f, 0.55f) };
            }
            return go;
        }
    }

    /// <summary>Side quest — Urgent package delivery for Layla.</summary>
    public class SideQuest_Delivery : MissionBase
    {
        public override string         MissionId   => "side_delivery_01";
        public override string         Title       => "Hot Delivery";
        public override string         Description => "Layla needs supplies delivered to the casino kitchen " +
                                                      "before the evening rush. You have 3 minutes.";
        public override MissionFaction  Faction    => MissionFaction.Civilian;

        private readonly Vector3 pickupPos   = new Vector3(14f, 0f, -14f);
        private readonly Vector3 deliverPos  = new Vector3(16f, 0f,  16f);
        private bool   hasPackage = false;
        private float  timer      = 180f;
        private GameObject marker;

        protected override void OnActivate()
        {
            objectives.AddRange(new[]
            {
                new MissionObjective("Pick up supplies from Layla [E]"),
                new MissionObjective("Deliver to the casino kitchen within 3 minutes")
            });
            SpawnMarker(pickupPos, Color.yellow);
        }

        public override void Tick(float deltaTime)
        {
            if (Status != MissionStatus.Active) return;
            if (hasPackage) { timer -= deltaTime; if (timer <= 0f) { Fail("Delivery too late!"); return; } }

            if (!hasPackage && DistToPlayer(pickupPos) < 3f && Input.GetKeyDown(KeyCode.E))
            {
                hasPackage = true;
                AdvanceObjective();
                SpawnMarker(deliverPos, Color.green);
            }

            if (hasPackage && DistToPlayer(deliverPos) < 4f)
            {
                Object.Destroy(marker);
                FactionReputation.Instance?.Apply("Civilians", 0.15f);
                FactionReputation.Instance?.Apply("Casino",    0.10f);
                EventBus.Publish(new PlayerActionEvent("Player_01", ActionType.CompletedDelivery,
                    deliverPos, "NPC_Layla_Vendor", 0.15f, 0.20f, "Delivered Layla's supplies on time"));
                Complete();
            }
        }

        public string TimerDisplay => hasPackage ? $"{Mathf.CeilToInt(timer)}s left" : "";

        private void SpawnMarker(Vector3 pos, Color col)
        {
            if (marker != null) Object.Destroy(marker);
            marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.position = pos + Vector3.up * 0.6f;
            marker.transform.localScale = Vector3.one * 0.9f;
            Object.Destroy(marker.GetComponent<Collider>());
            var r = marker.GetComponent<Renderer>();
            if (r)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                r.sharedMaterial = new Material(s) { color = col };
            }
        }
    }

    /// <summary>Side quest — Rescue an injured animal in the forest.</summary>
    public class SideQuest_AnimalRescue : MissionBase
    {
        public override string         MissionId   => "side_animal_rescue_01";
        public override string         Title       => "Wild Heart";
        public override string         Description => "A deer has been spotted injured near the forest edge. " +
                                                      "Find it and guide it to the wildlife sanctuary.";
        public override MissionFaction  Faction    => MissionFaction.Wildlife;

        private readonly Vector3 forestPos    = new Vector3(-50f, 0f, 50f);
        private readonly Vector3 sanctuaryPos = new Vector3(-35f, 0f, 60f);
        private bool reachedDeer    = false;
        private bool deerFollowing  = false;
        private GameObject markerObj;

        protected override void OnActivate()
        {
            objectives.AddRange(new[]
            {
                new MissionObjective("Find the injured deer in the forest"),
                new MissionObjective("Stay close to guide the deer to the sanctuary")
            });
            SpawnMarker(forestPos, new Color(0.3f, 0.8f, 0.3f));
        }

        public override void Tick(float deltaTime)
        {
            if (Status != MissionStatus.Active) return;

            if (!reachedDeer && DistToPlayer(forestPos) < 6f)
            {
                reachedDeer = true;
                deerFollowing = true;
                AdvanceObjective();
                SpawnMarker(sanctuaryPos, Color.green);
                Debug.Log("[Mission] You find the deer. It looks scared but follows you slowly.");
            }

            if (deerFollowing && DistToPlayer(sanctuaryPos) < 6f)
            {
                Object.Destroy(markerObj);
                FactionReputation.Instance?.Apply("Civilians", 0.20f);
                EventBus.Publish(new PlayerActionEvent("Player_01", ActionType.RescuedAnimal,
                    sanctuaryPos, "ForestDeer_01", 0.20f, 0.10f, "Guided injured deer to sanctuary"));
                Complete();
            }
        }

        private void SpawnMarker(Vector3 pos, Color col)
        {
            if (markerObj != null) Object.Destroy(markerObj);
            markerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            markerObj.transform.position = pos + Vector3.up * 0.8f;
            markerObj.transform.localScale = Vector3.one * 1.2f;
            Object.Destroy(markerObj.GetComponent<Collider>());
            var r = markerObj.GetComponent<Renderer>();
            if (r)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                r.sharedMaterial = new Material(s) { color = col };
            }
        }
    }

    /// <summary>Side quest — High-stakes roulette challenge at the casino.</summary>
    public class SideQuest_CasinoChallenge : MissionBase
    {
        public override string         MissionId   => "side_casino_01";
        public override string         Title       => "High Roller";
        public override string         Description => "Victor's laid down a challenge: win $15,000 in a single " +
                                                      "roulette session at the Grand Lux. Three spins, winner takes all.";
        public override MissionFaction  Faction    => MissionFaction.Casino;

        private readonly Vector3 casinoPos = new Vector3(14f, 0f, 14f);
        private int   spinsRemaining = 3;
        private float totalWinnings  = 0f;
        private bool  atTable        = false;
        private float spinCooldown   = 0f;

        protected override void OnActivate()
        {
            objectives.Add(new MissionObjective($"Win \u241115,000 across 3 spins (0/3 spins taken)"));
            SpawnMarker();
        }

        public override void Tick(float deltaTime)
        {
            if (Status != MissionStatus.Active) return;

            spinCooldown -= deltaTime;

            if (!atTable && DistToPlayer(casinoPos) < 4f)
            {
                atTable = true;
                if (markerObj != null) Object.Destroy(markerObj);
                Debug.Log("[Casino] Step up to the roulette table. Press [E] to spin.");
            }

            if (atTable && spinsRemaining > 0 && spinCooldown <= 0f && Input.GetKeyDown(KeyCode.E))
            {
                float spin = SimulateSpin();
                totalWinnings += spin;
                spinsRemaining--;
                spinCooldown = 1.5f;

                objectives[0].description = $"Win \u241115,000 across 3 spins " +
                                            $"(\u2741{totalWinnings:F0} / 3 spins, {3 - spinsRemaining} taken)";
                NotifyObjectiveUpdated();

                EventBus.Publish(new PlayerActionEvent("Player_01", ActionType.WonCasino,
                    casinoPos, "Casino_Roulette", spin > 0 ? 0.05f : -0.02f, 0f,
                    spin > 0 ? $"Won \u2741{spin:F0} on roulette" : $"Lost \u2741{Mathf.Abs(spin):F0} on roulette"));

                Debug.Log($"[Casino] Spin result: {(spin >= 0 ? "+" : "")}{spin:F0}. Total: {totalWinnings:F0}");

                if (spinsRemaining <= 0)
                {
                    if (totalWinnings >= 15000f)
                    {
                        FactionReputation.Instance?.Apply("Casino", 0.20f);
                        Complete();
                    }
                    else
                    {
                        Fail($"Only won \u2741{totalWinnings:F0} — needed \u241115,000. The house wins.");
                    }
                }
            }
        }

        private float SimulateSpin()
        {
            // 45% win / 55% loss, variable payout
            return Random.value > 0.55f
                ? Random.Range(2000f, 12000f)
                : -Random.Range(500f, 3000f);
        }

        private GameObject markerObj;
        private void SpawnMarker()
        {
            markerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            markerObj.transform.position = casinoPos + Vector3.up * 0.8f;
            markerObj.transform.localScale = Vector3.one * 1f;
            Object.Destroy(markerObj.GetComponent<Collider>());
            var r = markerObj.GetComponent<Renderer>();
            if (r)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                r.sharedMaterial = new Material(s) { color = new Color(0.9f, 0.75f, 0.1f) };
            }
        }
    }
}
