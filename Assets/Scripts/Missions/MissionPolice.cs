using System.Collections.Generic;
using DCultCity.Core;
using DCultCity.Reputation;
using UnityEngine;

namespace DCultCity.Missions
{
    /// <summary>
    /// Police Storyline — Mission 1: "Thin Blue Line"
    /// Officer Marcus briefs you on gang activity at the docks.
    /// Objectives:
    ///   1. Meet Marcus at the precinct (approach within 4 m)
    ///   2. Drive to the docks marker (south-east zone)
    ///   3. Photograph evidence (press E at the crate marker)
    ///   4. Return evidence to Marcus
    /// Reward: Police rep +0.35, unlock Mission 2
    /// </summary>
    public class MissionPolice_ThinBlueLine : MissionBase
    {
        public override string        MissionId   => "police_01_thin_blue_line";
        public override string        Title       => "Thin Blue Line";
        public override string        Description => "Officer Marcus has a job for you. " +
                                                     "Investigate gang activity at the docks and bring back proof.";
        public override MissionFaction Faction    => MissionFaction.Police;

        private enum Step { GoToMarcus, GoToDocks, CollectEvidence, ReturnToMarcus, Done }
        private Step currentStep = Step.GoToMarcus;

        // World positions (aligned with test city layout)
        private readonly Vector3 precinct   = new Vector3(-14f, 0f, 14f);
        private readonly Vector3 docksMarker = new Vector3(22f, 0f, -22f);
        private readonly Vector3 crateMarker = new Vector3(24f, 0f, -24f);

        private bool evidenceCollected = false;
        private GameObject markerObj;

        protected override void OnActivate()
        {
            objectives.AddRange(new[]
            {
                new MissionObjective("Meet Officer Marcus at the precinct"),
                new MissionObjective("Drive to the docks"),
                new MissionObjective("Collect photographic evidence [E] at the crates"),
                new MissionObjective("Return evidence to Officer Marcus")
            });
            SpawnMarker(precinct, "Go to Marcus", Color.blue);
        }

        public override void Tick(float deltaTime)
        {
            if (Status != MissionStatus.Active) return;

            switch (currentStep)
            {
                case Step.GoToMarcus:
                    if (DistToPlayer(precinct) < 5f)
                    {
                        AdvanceObjective();
                        currentStep = Step.GoToDocks;
                        MoveMarker(docksMarker, "Go to the Docks", Color.cyan);
                        Debug.Log("[Mission] Marcus: 'Head to the south docks. Be careful.'");
                    }
                    break;

                case Step.GoToDocks:
                    if (DistToPlayer(docksMarker) < 8f)
                    {
                        AdvanceObjective();
                        currentStep = Step.CollectEvidence;
                        MoveMarker(crateMarker, "Examine the Crates [E]", Color.yellow);
                    }
                    break;

                case Step.CollectEvidence:
                    if (!evidenceCollected && DistToPlayer(crateMarker) < 3f && Input.GetKeyDown(KeyCode.E))
                    {
                        evidenceCollected = true;
                        AdvanceObjective();
                        currentStep = Step.ReturnToMarcus;
                        MoveMarker(precinct, "Return to Marcus", Color.blue);
                        Debug.Log("[Mission] You photograph the crates. This is enough evidence.");
                    }
                    break;

                case Step.ReturnToMarcus:
                    if (DistToPlayer(precinct) < 5f && evidenceCollected)
                    {
                        AdvanceObjective();
                        currentStep = Step.Done;
                        CleanupMarker();
                        FactionReputation.Instance?.Apply("Police", 0.35f);
                        FactionReputation.Instance?.Apply("Civilians", 0.10f);
                        EventBus.Publish(new PlayerActionEvent("Player_01", ActionType.Custom,
                            precinct, "NPC_Officer_Marcus", reputationDelta: 0.35f, trustDelta: 0.25f,
                            actionName: "Delivered evidence to Marcus"));
                        Complete();
                    }
                    break;
            }
        }

        private void SpawnMarker(Vector3 pos, string label, Color col)
        {
            CleanupMarker();
            markerObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            markerObj.name = $"MissionMarker_{label}";
            markerObj.transform.position = pos + Vector3.up * 0.5f;
            markerObj.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
            SetMarkerColor(col);
            Object.Destroy(markerObj.GetComponent<Collider>());
        }

        private void MoveMarker(Vector3 pos, string label, Color col)
        {
            SpawnMarker(pos, label, col);
        }

        private void CleanupMarker()
        {
            if (markerObj != null) Object.Destroy(markerObj);
        }

        private void SetMarkerColor(Color col)
        {
            if (markerObj == null) return;
            var r = markerObj.GetComponent<Renderer>();
            if (r == null) return;
            Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(s) { color = new Color(col.r, col.g, col.b, 0.75f) };
            r.sharedMaterial = mat;
        }
    }

    /// <summary>
    /// Police Storyline — Mission 2: "Operation Lockdown"
    /// Uses intelligence from M1 to bust the gang leadership.
    /// Objectives:
    ///   1. Drive to the warehouse district
    ///   2. Disable 3 gang lookouts (approach within 3 m each — no combat, just shadow them)
    ///   3. Signal Marcus
    /// Reward: Police rep +0.50, Gangster rep −0.30
    /// </summary>
    public class MissionPolice_OperationLockdown : MissionBase
    {
        public override string         MissionId   => "police_02_operation_lockdown";
        public override string         Title       => "Operation Lockdown";
        public override string         Description => "With evidence in hand, Marcus is ready to move. " +
                                                      "Help him shut down the gang's warehouse operation.";
        public override MissionFaction  Faction    => MissionFaction.Police;

        private readonly Vector3[] lookoutPositions =
        {
            new Vector3( 20f, 0f, -15f),
            new Vector3( 24f, 0f, -18f),
            new Vector3( 22f, 0f, -12f)
        };
        private bool[]   lookoutsNeutralized;
        private int      neutralizedCount = 0;
        private readonly Vector3 signalPos = new Vector3(-14f, 0f, 14f);
        private bool warehouseReached = false;
        private readonly List<GameObject> markerObjs = new List<GameObject>();

        protected override void OnActivate()
        {
            lookoutsNeutralized = new bool[3];
            objectives.AddRange(new[]
            {
                new MissionObjective("Drive to the warehouse district"),
                new MissionObjective("Neutralize lookout 1 of 3"),
                new MissionObjective("Neutralize lookout 2 of 3"),
                new MissionObjective("Neutralize lookout 3 of 3"),
                new MissionObjective("Signal Officer Marcus")
            });
            SpawnWarehouseMarker();
        }

        public override void Tick(float deltaTime)
        {
            if (Status != MissionStatus.Active) return;

            if (!warehouseReached)
            {
                if (DistToPlayer(lookoutPositions[0]) < 12f)
                {
                    warehouseReached = true;
                    AdvanceObjective();
                    SpawnLookoutMarkers();
                }
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                if (!lookoutsNeutralized[i] && DistToPlayer(lookoutPositions[i]) < 3f)
                {
                    lookoutsNeutralized[i] = true;
                    neutralizedCount++;
                    AdvanceObjective();
                    Debug.Log($"[Mission] Lookout {i + 1} neutralized. ({neutralizedCount}/3)");

                    if (neutralizedCount == 3)
                    {
                        ClearLookoutMarkers();
                        SpawnMarkerAt(signalPos, Color.blue);
                    }
                }
            }

            if (neutralizedCount == 3 && DistToPlayer(signalPos) < 5f)
            {
                AdvanceObjective();
                ClearAll();
                FactionReputation.Instance?.Apply("Police",    0.50f);
                FactionReputation.Instance?.Apply("Gangsters", -0.30f);
                FactionReputation.Instance?.Apply("Civilians",  0.15f);
                Complete();
            }
        }

        private void SpawnWarehouseMarker()
        {
            markerObjs.Add(CreateMarker(lookoutPositions[0] + Vector3.back * 4f, Color.blue));
        }

        private void SpawnLookoutMarkers()
        {
            foreach (var pos in lookoutPositions)
                markerObjs.Add(CreateMarker(pos, Color.red));
        }

        private void SpawnMarkerAt(Vector3 pos, Color col) => markerObjs.Add(CreateMarker(pos, col));

        private void ClearLookoutMarkers() { /* markers are cleared in ClearAll */ }

        private void ClearAll()
        {
            foreach (var m in markerObjs) if (m != null) Object.Destroy(m);
            markerObjs.Clear();
        }

        private GameObject CreateMarker(Vector3 pos, Color col)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "MissionMarker";
            go.transform.position = pos + Vector3.up * 0.5f;
            go.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
            Object.Destroy(go.GetComponent<Collider>());
            var r = go.GetComponent<Renderer>();
            if (r)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                r.sharedMaterial = new Material(s) { color = new Color(col.r, col.g, col.b, 0.75f) };
            }
            return go;
        }
    }
}
