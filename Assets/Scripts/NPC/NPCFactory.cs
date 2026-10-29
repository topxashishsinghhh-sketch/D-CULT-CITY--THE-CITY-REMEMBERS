using System.Collections.Generic;
using DCultCity.NPC;
using UnityEngine;
using UnityEngine.AI;

namespace DCultCity.NPC
{
    /// <summary>
    /// Spawns 7 Story NPCs in the test city, each with:
    ///   • A unique identity, color, and starting position
    ///   • A 3-block daily schedule (morning / afternoon / evening)
    ///   • A trust-gated DialogueTree that reacts to 4 player actions
    ///
    /// Story NPCs:
    ///   1. Elena  – Florist in the park area
    ///   2. Marcus – Police Officer near the precinct
    ///   3. Victor – Casino Manager inside the casino block
    ///   4. Layla  – Street Food Vendor near the shops
    ///   5. Danny  – Gangster contact near the back alley
    ///   6. Rosa   – Park Jogger / Witness
    ///   7. Sam    – Mechanic / Car Enthusiast near the road
    /// </summary>
    public class NPCFactory : MonoBehaviour
    {
        [Header("NavMesh Bake")]
        [Tooltip("If true, the spawner will try to place NPCs on the nearest NavMesh point")]
        [SerializeField] private bool snapToNavMesh = true;

        private void Start()
        {
            SpawnAllNPCs();
        }

        [ContextMenu("Spawn All Story NPCs")]
        public void SpawnAllNPCs()
        {
            // 10 Story NPCs with full schedules, trust-gated dialogue and memory
            SpawnElena();
            SpawnMarcus();
            SpawnVictor();
            SpawnLayla();
            SpawnDanny();
            SpawnRosa();
            SpawnSam();
            SpawnHenderson();
            SpawnVance();
            SpawnAris();

            // 40 Ambient NPCs with wandering and reactions
            SpawnAmbientCrowd(40);

            Debug.Log("<color=#00FFCC>[NPCFactory]</color> All 50 NPCs spawned (10 Story + 40 Ambient)!");
        }

        private void SpawnHenderson()
        {
            var npc = CreateStoryNPC("NPC_Chief_Henderson", "Chief Henderson",
                new Vector3(-16f, 0.1f, 16f),
                new Color(0.12f, 0.18f, 0.35f));

            npc.schedule = new NPCDailySchedule(new List<ScheduleEntry>
            {
                new ScheduleEntry { startHour = 8f,  endHour = 17f, targetPosition = new Vector3(-16f, 0.1f, 16f), activityLabel = "supervising precinct operations" },
                new ScheduleEntry { startHour = 17f, endHour = 24f, targetPosition = new Vector3(-12f, 0.1f, 15f), activityLabel = "reviewing case files" }
            });

            npc.dialogueTree = new DialogueTree
            {
                lines = new List<DialogueLine>
                {
                    new DialogueLine
                    {
                        minimumTrust = TrustLevel.Hostile,
                        npcText = "I don't tolerate criminals in my jurisdiction. Get out of my sight before I book you.",
                        playerReplies = new[] { "Understood, Chief.", "You can't touch me." },
                        replyTrustDeltas = new[] { 0.05f, -0.25f }
                    },
                    new DialogueLine
                    {
                        minimumTrust = TrustLevel.Neutral,
                        npcText = "D-Cult City requires order. If you keep your nose clean, we won't have problems.",
                        playerReplies = new[] { "Yes, Chief.", "I'm working with Marcus.", "Just visiting." },
                        replyTrustDeltas = new[] { 0.08f, 0.15f, 0.05f }
                    },
                    new DialogueLine
                    {
                        minimumTrust = TrustLevel.Trusted,
                        requiresMemory = true,
                        npcText = "Marcus spoke highly of your work on the docks. I have authorized special police liaison status for you.",
                        playerReplies = new[] { "An honor, Chief.", "Happy to help the precinct." },
                        replyTrustDeltas = new[] { 0.20f, 0.15f }
                    }
                }
            };
        }

        private void SpawnVance()
        {
            var npc = CreateStoryNPC("NPC_Vance_Syndicate", "Vance",
                new Vector3(16f, 0.1f, 18f),
                new Color(0.35f, 0.12f, 0.12f));

            npc.schedule = new NPCDailySchedule(new List<ScheduleEntry>
            {
                new ScheduleEntry { startHour = 12f, endHour = 20f, targetPosition = new Vector3(16f, 0.1f, 18f), activityLabel = "holding court in VIP lounge" },
                new ScheduleEntry { startHour = 20f, endHour = 24f, targetPosition = new Vector3(22f, 0.1f, -18f), activityLabel = "inspecting the warehouse docks" }
            });

            npc.dialogueTree = new DialogueTree
            {
                lines = new List<DialogueLine>
                {
                    new DialogueLine
                    {
                        minimumTrust = TrustLevel.Hostile,
                        npcText = "You made a big mistake messing with my interests. Walk away while you still can.",
                        playerReplies = new[] { "We can negotiate.", "I'm not backing down." },
                        replyTrustDeltas = new[] { 0.10f, -0.30f }
                    },
                    new DialogueLine
                    {
                        minimumTrust = TrustLevel.Neutral,
                        npcText = "In D-Cult City, money and discretion speak louder than words. Which do you have?",
                        playerReplies = new[] { "I have skills you can use.", "Just observing.", "I deal with Danny." },
                        replyTrustDeltas = new[] { 0.15f, 0.05f, 0.18f }
                    },
                    new DialogueLine
                    {
                        minimumTrust = TrustLevel.Trusted,
                        requiresMemory = true,
                        npcText = "You proved your loyalty with that delivery. Welcome to the inner circle.",
                        playerReplies = new[] { "Ready for the big jobs, Vance.", "Let's run this town." },
                        replyTrustDeltas = new[] { 0.25f, 0.20f }
                    }
                }
            };
        }

        private void SpawnAris()
        {
            var npc = CreateStoryNPC("NPC_Dr_Aris", "Dr. Aris",
                new Vector3(-8f, 0.1f, -14f),
                new Color(0.20f, 0.60f, 0.50f));

            npc.schedule = new NPCDailySchedule(new List<ScheduleEntry>
            {
                new ScheduleEntry { startHour = 7f,  endHour = 15f, targetPosition = new Vector3(-8f, 0.1f, -14f), activityLabel = "botanical park study" },
                new ScheduleEntry { startHour = 15f, endHour = 21f, targetPosition = new Vector3(-45f, 0.1f, 45f), activityLabel = "forest perimeter research" }
            });

            npc.dialogueTree = new DialogueTree
            {
                lines = new List<DialogueLine>
                {
                    new DialogueLine
                    {
                        minimumTrust = TrustLevel.Hostile,
                        npcText = "Violence and destruction... I have nothing to say to someone who harms this city.",
                        playerReplies = new[] { "I regret my actions.", "Nature is harsh, Doctor." },
                        replyTrustDeltas = new[] { 0.15f, -0.10f }
                    },
                    new DialogueLine
                    {
                        minimumTrust = TrustLevel.Neutral,
                        npcText = "Greetings. The wildlife in the northern forest has been restless lately.",
                        playerReplies = new[] { "Have you seen any deer?", "What can I do to help?" },
                        replyTrustDeltas = new[] { 0.10f, 0.15f }
                    },
                    new DialogueLine
                    {
                        minimumTrust = TrustLevel.Trusted,
                        requiresMemory = true,
                        npcText = "Thank you for respecting our wildlife sanctuary. You have the heart of a true protector.",
                        playerReplies = new[] { "Nature deserves our protection.", "Glad to help." },
                        replyTrustDeltas = new[] { 0.20f, 0.15f }
                    }
                }
            };
        }

        private void SpawnAmbientCrowd(int totalCount)
        {
            Vector3[] zones = {
                new Vector3(  0f, 0.1f,   0f),  // Downtown Center
                new Vector3( 12f, 0.1f, -10f),  // Commercial Plaza
                new Vector3(-12f, 0.1f, -12f),  // Central Park Lawn
                new Vector3(-60f, 0.1f, -60f),  // Coastal Beach Boardwalk
                new Vector3( 15f, 0.1f,  12f)   // Casino Square
            };

            for (int i = 0; i < totalCount; i++)
            {
                Vector3 center = zones[i % zones.Length];
                Vector3 randOffset = new Vector3(Random.Range(-12f, 12f), 0f, Random.Range(-12f, 12f));
                Vector3 spawnPos = center + randOffset;

                if (snapToNavMesh && NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 8f, NavMesh.AllAreas))
                {
                    spawnPos = hit.position;
                }

                GameObject ambientObj = new GameObject($"Ambient_NPC_{i + 1:00}");
                ambientObj.transform.position = spawnPos;

                var ambient = ambientObj.AddComponent<AmbientNPC>();
                ambient.npcId = $"Ambient_{i + 1:00}";
                ambient.displayName = $"Citizen {i + 1}";
                ambient.isStoryNPC = false;
            }
        }

        // ================================================================== //
        //  Individual NPC Definitions
        // ================================================================== //

        private void SpawnElena()
        {
            var npc = CreateStoryNPC("NPC_Elena_Florist", "Elena",
                new Vector3(-10f, 0.1f, -10f),
                new Color(0.90f, 0.50f, 0.65f));

            npc.schedule = new NPCDailySchedule(new List<ScheduleEntry>
            {
                new ScheduleEntry { startHour = 7f,  endHour = 12f, targetPosition = new Vector3(-12f, 0.1f, -10f), activityLabel = "tending flowers" },
                new ScheduleEntry { startHour = 12f, endHour = 16f, targetPosition = new Vector3(-11f, 0.1f, -13f), activityLabel = "eating lunch in the park" },
                new ScheduleEntry { startHour = 16f, endHour = 20f, targetPosition = new Vector3(-10f, 0.1f, -10f), activityLabel = "closing the stall" },
                new ScheduleEntry { startHour = 20f, endHour = 24f, targetPosition = new Vector3(-20f, 0.1f, -20f), activityLabel = "walking home" }
            });

            npc.dialogueTree = BuildElenaDialogue();
        }

        private void SpawnMarcus()
        {
            var npc = CreateStoryNPC("NPC_Officer_Marcus", "Officer Marcus",
                new Vector3(-14f, 0.1f, 12f),
                new Color(0.15f, 0.25f, 0.55f));

            npc.schedule = new NPCDailySchedule(new List<ScheduleEntry>
            {
                new ScheduleEntry { startHour = 6f,  endHour = 14f, targetPosition = new Vector3(-14f, 0.1f, 14f), activityLabel = "on patrol near precinct" },
                new ScheduleEntry { startHour = 14f, endHour = 18f, targetPosition = new Vector3(-10f, 0.1f,  8f), activityLabel = "patrolling the district" },
                new ScheduleEntry { startHour = 18f, endHour = 24f, targetPosition = new Vector3(-15f, 0.1f, 16f), activityLabel = "end of shift briefing" }
            });

            npc.dialogueTree = BuildMarcusDialogue();
        }

        private void SpawnVictor()
        {
            var npc = CreateStoryNPC("NPC_Victor_Casino", "Victor",
                new Vector3(14f, 0.1f, 14f),
                new Color(0.75f, 0.55f, 0.10f));

            npc.schedule = new NPCDailySchedule(new List<ScheduleEntry>
            {
                new ScheduleEntry { startHour = 10f, endHour = 22f, targetPosition = new Vector3(14f, 0.1f, 14f), activityLabel = "overseeing the casino floor" },
                new ScheduleEntry { startHour = 22f, endHour = 24f, targetPosition = new Vector3(15f, 0.1f, 18f), activityLabel = "counting chips in the back" },
                new ScheduleEntry { startHour = 0f,  endHour = 10f, targetPosition = new Vector3(13f, 0.1f, 11f), activityLabel = "sleeping in the penthouse" }
            });

            npc.dialogueTree = BuildVictorDialogue();
        }

        private void SpawnLayla()
        {
            var npc = CreateStoryNPC("NPC_Layla_Vendor", "Layla",
                new Vector3(12f, 0.1f, -14f),
                new Color(0.85f, 0.40f, 0.20f));

            npc.schedule = new NPCDailySchedule(new List<ScheduleEntry>
            {
                new ScheduleEntry { startHour = 8f,  endHour = 14f, targetPosition = new Vector3(14f, 0.1f, -14f), activityLabel = "running the food cart" },
                new ScheduleEntry { startHour = 14f, endHour = 18f, targetPosition = new Vector3(12f, 0.1f, -12f), activityLabel = "chatting with neighbors" },
                new ScheduleEntry { startHour = 18f, endHour = 21f, targetPosition = new Vector3(13f, 0.1f, -14f), activityLabel = "evening service" },
                new ScheduleEntry { startHour = 21f, endHour = 24f, targetPosition = new Vector3(18f, 0.1f, -18f), activityLabel = "heading home" }
            });

            npc.dialogueTree = BuildLaylaDialogue();
        }

        private void SpawnDanny()
        {
            var npc = CreateStoryNPC("NPC_Danny_Gangster", "Danny",
                new Vector3(18f, 0.1f, -8f),
                new Color(0.18f, 0.18f, 0.18f));

            npc.schedule = new NPCDailySchedule(new List<ScheduleEntry>
            {
                new ScheduleEntry { startHour = 0f,  endHour = 12f, targetPosition = new Vector3(20f, 0.1f, -10f), activityLabel = "sleeping or hiding" },
                new ScheduleEntry { startHour = 12f, endHour = 18f, targetPosition = new Vector3(18f, 0.1f,  -8f), activityLabel = "watching the street" },
                new ScheduleEntry { startHour = 18f, endHour = 24f, targetPosition = new Vector3(16f, 0.1f,  -6f), activityLabel = "doing business in the alley" }
            });

            npc.dialogueTree = BuildDannyDialogue();
        }

        private void SpawnRosa()
        {
            var npc = CreateStoryNPC("NPC_Rosa_Jogger", "Rosa",
                new Vector3(-5f, 0.1f, -5f),
                new Color(0.60f, 0.80f, 0.40f));

            npc.schedule = new NPCDailySchedule(new List<ScheduleEntry>
            {
                new ScheduleEntry { startHour = 6f,  endHour = 9f,  targetPosition = new Vector3(-8f, 0.1f, -8f),  activityLabel = "morning jog in the park" },
                new ScheduleEntry { startHour = 9f,  endHour = 17f, targetPosition = new Vector3(-5f, 0.1f, -12f), activityLabel = "at work in the city" },
                new ScheduleEntry { startHour = 17f, endHour = 19f, targetPosition = new Vector3(-8f, 0.1f, -8f),  activityLabel = "evening walk" },
                new ScheduleEntry { startHour = 19f, endHour = 24f, targetPosition = new Vector3(-6f, 0.1f, -6f),  activityLabel = "sitting by the fountain" }
            });

            npc.dialogueTree = BuildRosaDialogue();
        }

        private void SpawnSam()
        {
            var npc = CreateStoryNPC("NPC_Sam_Mechanic", "Sam",
                new Vector3(8f, 0.1f, -4f),
                new Color(0.40f, 0.35f, 0.25f));

            npc.schedule = new NPCDailySchedule(new List<ScheduleEntry>
            {
                new ScheduleEntry { startHour = 8f,  endHour = 18f, targetPosition = new Vector3(8f, 0.1f, -3f),  activityLabel = "working in the garage" },
                new ScheduleEntry { startHour = 18f, endHour = 21f, targetPosition = new Vector3(6f, 0.1f, -2f),  activityLabel = "road-watching after work" },
                new ScheduleEntry { startHour = 21f, endHour = 24f, targetPosition = new Vector3(10f, 0.1f, -6f), activityLabel = "watching cars from the curb" }
            });

            npc.dialogueTree = BuildSamDialogue();
        }

        // ================================================================== //
        //  Dialogue Tree Builders
        // ================================================================== //

        private DialogueTree BuildElenaDialogue()
        {
            return new DialogueTree { lines = new List<DialogueLine>
            {
                // Hostile — she's scared after witnessing crime
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Hostile,
                    npcText        = "Don't come near me! I know what you've done. Stay away from my stall!",
                    playerReplies  = new[] { "I'm sorry, it won't happen again.", "Mind your own business." },
                    replyTrustDeltas = new[] { 0.15f, -0.20f },
                    replyRepDeltas   = new[] { 0.05f, -0.10f }
                },
                // Suspicious — cold greeting
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Suspicious,
                    npcText        = "Oh... it's you. Looking to buy flowers, or is something else on your mind?",
                    playerReplies  = new[] { "Just passing by. Nice flowers.", "I need your help with something." },
                    replyTrustDeltas = new[] { 0.10f, 0.05f },
                    replyRepDeltas   = new[] { 0.02f, 0.05f }
                },
                // Neutral
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Neutral,
                    npcText        = "Hello! Fresh roses today, just delivered from the highlands. Can I help you?",
                    playerReplies  = new[] { "I'll take some flowers.", "What's the gossip around here?", "I saw trouble earlier." },
                    replyTrustDeltas = new[] { 0.12f, 0.08f, 0.15f },
                    replyRepDeltas   = new[] { 0.05f, 0.02f, 0.08f }
                },
                // Trusted — opens mission hint
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Trusted,
                    requiresMemory = true,
                    npcText        = "You've been so kind to this neighborhood. I have to tell you something — a man's been watching my stall. He works for someone powerful...",
                    playerReplies  = new[] { "Tell me more. I can handle it.", "Be careful, Elena. I'll look into it." },
                    replyTrustDeltas = new[] { 0.20f, 0.18f },
                    replyRepDeltas   = new[] { 0.10f, 0.08f }
                }
            }};
        }

        private DialogueTree BuildMarcusDialogue()
        {
            return new DialogueTree { lines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Hostile,
                    npcText        = "Step back! You're on thin ice. One more incident and you're in cuffs, understood?",
                    playerReplies  = new[] { "I'll behave, officer.", "You can't prove anything." },
                    replyTrustDeltas = new[] { 0.10f, -0.25f },
                    replyRepDeltas   = new[] { 0.05f, -0.20f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Suspicious,
                    npcText        = "Citizen. We've had reports of some disturbances in the area. You wouldn't know anything about that, would you?",
                    playerReplies  = new[] { "Not at all, Officer.", "I might have seen something...", "I don't talk to cops." },
                    replyTrustDeltas = new[] { 0.05f, 0.15f, -0.15f },
                    replyRepDeltas   = new[] { 0.03f, 0.10f, -0.12f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Neutral,
                    npcText        = "Good afternoon. Keeping out of trouble I hope. This is a good neighborhood — let's keep it that way.",
                    playerReplies  = new[] { "Always, Officer.", "What's your beat around here?", "There's trouble near the casino." },
                    replyTrustDeltas = new[] { 0.08f, 0.05f, 0.18f },
                    replyRepDeltas   = new[] { 0.04f, 0.02f, 0.10f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Trusted,
                    requiresMemory = true,
                    npcText        = "I'm glad you're on our side. Between you and me, there's a gang moving product through the docks. If you find evidence, bring it to me — off the record.",
                    playerReplies  = new[] { "I'm on it, Marcus.", "What's in it for me?", "That's too risky for me." },
                    replyTrustDeltas = new[] { 0.25f, 0.05f, -0.05f },
                    replyRepDeltas   = new[] { 0.15f, 0.02f, -0.02f }
                }
            }};
        }

        private DialogueTree BuildVictorDialogue()
        {
            return new DialogueTree { lines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Hostile,
                    npcText        = "You're not welcome on my floor anymore. Security! Escort this person out.",
                    playerReplies  = new[] { "I'll go. My apologies.", "I'll be back." },
                    replyTrustDeltas = new[] { 0.08f, -0.30f },
                    replyRepDeltas   = new[] { 0.03f, -0.20f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Neutral,
                    npcText        = "Welcome to the Grand Lux. Tonight's jackpot is extraordinary. Feeling lucky?",
                    playerReplies  = new[] { "Deal me in.", "I just won big at roulette!", "Who runs this place, really?" },
                    replyTrustDeltas = new[] { 0.05f, 0.20f, 0.05f },
                    replyRepDeltas   = new[] { 0.05f, 0.15f, 0.02f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Trusted,
                    requiresMemory = true,
                    npcText        = "You've proven yourself here. I have a proposition — someone's been skimming my tables. Find out who, and I'll cut you in on next month's earnings.",
                    playerReplies  = new[] { "I'll find your thief, Victor.", "What do you pay?", "That's not my problem." },
                    replyTrustDeltas = new[] { 0.30f, 0.15f, -0.10f },
                    replyRepDeltas   = new[] { 0.10f, 0.05f, -0.05f }
                }
            }};
        }

        private DialogueTree BuildLaylaDialogue()
        {
            return new DialogueTree { lines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Hostile,
                    npcText        = "No food for trouble-makers. Come back when you've cleaned up your act.",
                    playerReplies  = new[] { "Fair enough.", "Just give me something to eat!" },
                    replyTrustDeltas = new[] { 0.08f, -0.12f },
                    replyRepDeltas   = new[] { 0.02f, -0.05f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Neutral,
                    npcText        = "Hey! Try my spiced rice bowl — best in the city! The whole block eats here.",
                    playerReplies  = new[] { "I'll take one, thanks!", "Heard anything interesting lately?", "Keep the change." },
                    replyTrustDeltas = new[] { 0.10f, 0.08f, 0.18f },
                    replyRepDeltas   = new[] { 0.04f, 0.03f, 0.08f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Trusted,
                    requiresMemory = true,
                    npcText        = "You're good people. Danny's boys have been shaking down the vendors this week. We need someone to stand up to them.",
                    playerReplies  = new[] { "Point them out to me.", "Stay safe, Layla.", "I don't want trouble." },
                    replyTrustDeltas = new[] { 0.25f, 0.15f, -0.05f },
                    replyRepDeltas   = new[] { 0.12f, 0.08f, -0.03f }
                }
            }};
        }

        private DialogueTree BuildDannyDialogue()
        {
            return new DialogueTree { lines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Hostile,
                    npcText        = "You really pushed your luck coming here. One more move against us and it's over for you in this city.",
                    playerReplies  = new[] { "Let's start fresh.", "I'm not afraid of you." },
                    replyTrustDeltas = new[] { 0.15f, -0.30f },
                    replyRepDeltas   = new[] { 0.10f, -0.20f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Suspicious,
                    npcText        = "I've heard things about you. Could be good, could be bad. Depends what side you're on.",
                    playerReplies  = new[] { "I'm on nobody's side.", "I'm looking for work.", "I know about the docks." },
                    replyTrustDeltas = new[] { 0.05f, 0.15f, 0.20f },
                    replyRepDeltas   = new[] { 0.02f, 0.12f, 0.10f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Neutral,
                    npcText        = "You've got nerve showing up here. I respect that. What do you want?",
                    playerReplies  = new[] { "I want in on the operation.", "What happened on the docks?", "I have a delivery for you." },
                    replyTrustDeltas = new[] { 0.20f, 0.10f, 0.25f },
                    replyRepDeltas   = new[] { 0.15f, 0.05f, 0.20f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Trusted,
                    requiresMemory = true,
                    npcText        = "Alright. You've proven yourself. There's a shipment coming Thursday — I need a reliable driver. $10K if you make the delivery clean.",
                    playerReplies  = new[] { "I'll do it.", "50-50 or no deal.", "Too risky. I'm out." },
                    replyTrustDeltas = new[] { 0.30f, 0.20f, -0.15f },
                    replyRepDeltas   = new[] { 0.25f, 0.15f, -0.10f }
                }
            }};
        }

        private DialogueTree BuildRosaDialogue()
        {
            return new DialogueTree { lines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Hostile,
                    npcText        = "I've seen what you're capable of. Please just leave me alone.",
                    playerReplies  = new[] { "I'm sorry for what happened.", "Don't be scared." },
                    replyTrustDeltas = new[] { 0.12f, 0.05f },
                    replyRepDeltas   = new[] { 0.05f, 0.02f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Neutral,
                    npcText        = "Oh hi! Beautiful morning for a run, right? This park is the best thing about this city.",
                    playerReplies  = new[] { "Agreed! Do you run here every day?", "I need some information.", "Have you seen anything odd?" },
                    replyTrustDeltas = new[] { 0.10f, 0.05f, 0.12f },
                    replyRepDeltas   = new[] { 0.03f, 0.02f, 0.06f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Trusted,
                    requiresMemory = true,
                    npcText        = "I'm so glad someone's paying attention! I jog past the casino every night. Something shady happens between 11 PM and midnight — men carrying crates.",
                    playerReplies  = new[] { "Can you document it? I'll act on it.", "I'll look into it tonight." },
                    replyTrustDeltas = new[] { 0.22f, 0.18f },
                    replyRepDeltas   = new[] { 0.10f, 0.08f }
                }
            }};
        }

        private DialogueTree BuildSamDialogue()
        {
            return new DialogueTree { lines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Hostile,
                    npcText        = "After what you did to that car — and those people — don't ask me for help.",
                    playerReplies  = new[] { "I'll make it right.", "It was an accident." },
                    replyTrustDeltas = new[] { 0.12f, 0.05f },
                    replyRepDeltas   = new[] { 0.04f, 0.02f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Neutral,
                    npcText        = "Hey. Nice wheels you've been seen driving. She's a beauty — is she yours?",
                    playerReplies  = new[] { "She's mine. I take good care of her.", "I borrowed it.", "I found it." },
                    replyTrustDeltas = new[] { 0.10f, 0.02f, -0.08f },
                    replyRepDeltas   = new[] { 0.03f, 0.01f, -0.05f }
                },
                new DialogueLine
                {
                    minimumTrust   = TrustLevel.Trusted,
                    requiresMemory = true,
                    npcText        = "You're alright. I know cars, and I know people. I've got a contact who can get you a fast set of wheels — legit, no questions. Interested?",
                    playerReplies  = new[] { "Absolutely. Hook me up.", "What's the catch?", "I'm good for now." },
                    replyTrustDeltas = new[] { 0.25f, 0.15f, 0.05f },
                    replyRepDeltas   = new[] { 0.08f, 0.04f, 0.01f }
                }
            }};
        }

        // ================================================================== //
        //  Helpers
        // ================================================================== //
        private StoryNPC CreateStoryNPC(string id, string displayName, Vector3 position, Color color)
        {
            GameObject go = new GameObject(id);

            // Snap to NavMesh
            if (snapToNavMesh && NavMesh.SamplePosition(position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                go.transform.position = hit.position;
            else
                go.transform.position = position;

            var npc           = go.AddComponent<StoryNPC>();
            npc.npcId         = id;
            npc.displayName   = displayName;
            npc.isStoryNPC    = true;

            // Expose body color via reflection-safe field
            var colorField = typeof(StoryNPC).GetField("bodyColor",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            colorField?.SetValue(npc, color);

            return npc;
        }
    }
}
