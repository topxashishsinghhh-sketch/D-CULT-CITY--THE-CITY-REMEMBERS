using DCultCity.Core;
using DCultCity.Reputation;
using UnityEngine;

namespace DCultCity.Missions
{
    /// <summary>
    /// Gangster Storyline — Mission 1: "First Contact"
    /// Danny wants you to pick up a package and deliver it across town.
    /// Objectives:
    ///   1. Meet Danny in the alley
    ///   2. Drive to the pickup location
    ///   3. Pick up the package [E]
    ///   4. Deliver it to the drop point without police contact
    /// Reward: Gangster rep +0.35, Danny trust +0.30
    /// </summary>
    public class MissionGangster_FirstContact : MissionBase
    {
        public override string         MissionId   => "gangster_01_first_contact";
        public override string         Title       => "First Contact";
        public override string         Description => "Danny's got a job that pays well. " +
                                                      "Pick up a package and get it across town — clean.";
        public override MissionFaction  Faction    => MissionFaction.Gangster;

        private enum Step { GoToDanny, GoToPickup, PickupPackage, Deliver, Done }
        private Step step = Step.GoToDanny;

        private readonly Vector3 dannyPos    = new Vector3(18f, 0f, -8f);
        private readonly Vector3 pickupPos   = new Vector3(-20f, 0f, 10f);
        private readonly Vector3 dropoffPos  = new Vector3(20f, 0f, 20f);

        private bool hasPackage = false;
        private GameObject markerObj;
        private float policeProximityTimer = 0f; // accumulate time near police rep zone

        protected override void OnActivate()
        {
            objectives.AddRange(new[]
            {
                new MissionObjective("Meet Danny in the alley"),
                new MissionObjective("Drive to the pickup location"),
                new MissionObjective("Pick up the package [E]"),
                new MissionObjective("Deliver to the drop point — stay clear of police")
            });
            SpawnMarker(dannyPos, Color.red);
        }

        public override void Tick(float deltaTime)
        {
            if (Status != MissionStatus.Active) return;

            switch (step)
            {
                case Step.GoToDanny:
                    if (DistToPlayer(dannyPos) < 4f)
                    {
                        AdvanceObjective();
                        step = Step.GoToPickup;
                        MoveMarker(pickupPos, Color.yellow);
                        Debug.Log("[Mission] Danny: 'Grab the package from the north-west lockup. Don't get caught.'");
                    }
                    break;

                case Step.GoToPickup:
                    if (DistToPlayer(pickupPos) < 6f)
                    {
                        AdvanceObjective();
                        step = Step.PickupPackage;
                        MoveMarker(pickupPos, Color.white);
                    }
                    break;

                case Step.PickupPackage:
                    if (DistToPlayer(pickupPos) < 3f && Input.GetKeyDown(KeyCode.E))
                    {
                        hasPackage = true;
                        AdvanceObjective();
                        step = Step.Deliver;
                        MoveMarker(dropoffPos, new Color(1f, 0.5f, 0f));
                        Debug.Log("[Mission] Package secured. Get it to the casino district — fast.");
                    }
                    break;

                case Step.Deliver:
                    // Fail if police faction catches up (wanted level >= 2 stars)
                    if (FactionReputation.Instance != null &&
                        FactionReputation.Instance.GetWantedLevel() >= WantedLevel.TwoStars)
                    {
                        policeProximityTimer += deltaTime;
                        if (policeProximityTimer > 8f)
                        {
                            CleanupMarker();
                            Fail("Police intercepted the delivery. Danny's going to be furious.");
                            return;
                        }
                    }
                    else
                    {
                        policeProximityTimer = 0f;
                    }

                    if (DistToPlayer(dropoffPos) < 4f && hasPackage)
                    {
                        hasPackage = false;
                        AdvanceObjective();
                        step = Step.Done;
                        CleanupMarker();
                        FactionReputation.Instance?.Apply("Gangsters",  0.35f);
                        FactionReputation.Instance?.Apply("Civilians",  -0.05f);
                        EventBus.Publish(new PlayerActionEvent("Player_01", ActionType.CompletedDelivery,
                            dropoffPos, "NPC_Danny_Gangster", reputationDelta: 0.35f, trustDelta: 0.30f,
                            actionName: "Completed Danny's delivery"));
                        Complete();
                    }
                    break;
            }
        }

        private void SpawnMarker(Vector3 pos, Color col)
        {
            CleanupMarker();
            markerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            markerObj.name = "MissionMarker_Gangster";
            markerObj.transform.position = pos + Vector3.up * 0.6f;
            markerObj.transform.localScale = Vector3.one * 0.8f;
            Object.Destroy(markerObj.GetComponent<Collider>());
            var r = markerObj.GetComponent<Renderer>();
            if (r)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                r.sharedMaterial = new Material(s) { color = col };
            }
        }

        private void MoveMarker(Vector3 pos, Color col) => SpawnMarker(pos, col);
        private void CleanupMarker() { if (markerObj != null) Object.Destroy(markerObj); }
    }

    /// <summary>
    /// Gangster Storyline — Mission 2: "The Takeover"
    /// Danny wants control of the casino floor. Help him pressure Victor.
    /// Objectives:
    ///   1. Talk to Victor and deliver Danny's message (dialogue check: Trust >= Neutral)
    ///   2. Collect "insurance" from 2 businesses
    ///   3. Report back to Danny
    /// Reward: Gangster rep +0.50, Casino rep −0.20, Victor trust −0.30
    /// </summary>
    public class MissionGangster_TheTakeover : MissionBase
    {
        public override string         MissionId   => "gangster_02_the_takeover";
        public override string         Title       => "The Takeover";
        public override string         Description => "Danny's ready to move on the casino. " +
                                                      "Deliver his message to Victor and collect tribute from the shops.";
        public override MissionFaction  Faction    => MissionFaction.Gangster;

        private readonly Vector3 victorPos   = new Vector3(14f, 0f, 14f);
        private readonly Vector3 shop1Pos    = new Vector3(12f, 0f, -14f);  // Layla's stall
        private readonly Vector3 shop2Pos    = new Vector3(-12f, 0f, -10f); // Park vendor
        private readonly Vector3 dannyPos    = new Vector3(18f, 0f, -8f);

        private bool messagedVictor = false;
        private bool collectedShop1 = false;
        private bool collectedShop2 = false;
        private readonly System.Collections.Generic.List<GameObject> markers = new();

        protected override void OnActivate()
        {
            objectives.AddRange(new[]
            {
                new MissionObjective("Deliver Danny's message to Victor [E]"),
                new MissionObjective("Collect tribute from Shop 1 [E]"),
                new MissionObjective("Collect tribute from Shop 2 [E]"),
                new MissionObjective("Report back to Danny")
            });
            markers.Add(CreateMarker(victorPos, Color.yellow));
            markers.Add(CreateMarker(shop1Pos,  Color.red));
            markers.Add(CreateMarker(shop2Pos,  Color.red));
        }

        public override void Tick(float deltaTime)
        {
            if (Status != MissionStatus.Active) return;

            if (!messagedVictor && DistToPlayer(victorPos) < 3f && Input.GetKeyDown(KeyCode.E))
            {
                messagedVictor = true;
                AdvanceObjective();
                FactionReputation.Instance?.Apply("Casino", -0.10f);
                EventBus.Publish(new PlayerActionEvent("Player_01", ActionType.Custom,
                    victorPos, "NPC_Victor_Casino", -0.10f, -0.30f, "Delivered Danny's threat to Victor"));
                Debug.Log("[Mission] Victor looks shaken. 'Tell Danny... I'll think about it.'");
            }

            if (messagedVictor && !collectedShop1 && DistToPlayer(shop1Pos) < 3f && Input.GetKeyDown(KeyCode.E))
            {
                collectedShop1 = true;
                AdvanceObjective();
                EventBus.Publish(new PlayerActionEvent("Player_01", ActionType.AssaultedCitizen,
                    shop1Pos, "NPC_Layla_Vendor", -0.15f, -0.20f, "Extorted Layla's stall for Danny"));
            }

            if (messagedVictor && !collectedShop2 && DistToPlayer(shop2Pos) < 3f && Input.GetKeyDown(KeyCode.E))
            {
                collectedShop2 = true;
                AdvanceObjective();
            }

            if (messagedVictor && collectedShop1 && collectedShop2 && DistToPlayer(dannyPos) < 4f)
            {
                foreach (var m in markers) if (m) Object.Destroy(m);
                markers.Clear();
                FactionReputation.Instance?.Apply("Gangsters",  0.50f);
                FactionReputation.Instance?.Apply("Civilians",  -0.15f);
                FactionReputation.Instance?.Apply("Casino",     -0.20f);
                Complete();
            }
        }

        private GameObject CreateMarker(Vector3 pos, Color col)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "GangMarker";
            go.transform.position = pos + Vector3.up * 0.6f;
            go.transform.localScale = Vector3.one * 0.8f;
            Object.Destroy(go.GetComponent<Collider>());
            var r = go.GetComponent<Renderer>();
            if (r)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                r.sharedMaterial = new Material(s) { color = col };
            }
            return go;
        }
    }
}
