using DCultCity.Core;
using System.Collections.Generic;
using UnityEngine;

namespace DCultCity.NPC
{
    /// <summary>
    /// Story NPC — extends NPCBase with:
    ///   • NPCDailySchedule (moves to location-based waypoints per in-game hour)
    ///   • DialogueTree (trust-gated branching dialogue)
    ///   • Reaction lines when trust changes
    ///   • Proximity-triggered interaction prompt
    /// </summary>
    public class StoryNPC : NPCBase
    {
        // ------------------------------------------------------------------ //
        //  Inspector
        // ------------------------------------------------------------------ //
        [Header("Story NPC Configuration")]
        [SerializeField] private Color bodyColor = new Color(0.25f, 0.55f, 0.85f);

        [Header("Schedule (set at runtime by NPCFactory)")]
        public NPCDailySchedule schedule;

        [Header("Interaction")]
        [SerializeField] private float interactionRadius = 2.8f;
        [SerializeField] private KeyCode interactKey     = KeyCode.E;

        // Dialogue tree is populated by NPCFactory at spawn time
        [HideInInspector] public DialogueTree dialogueTree;

        // ------------------------------------------------------------------ //
        //  Runtime state
        // ------------------------------------------------------------------ //
        private bool  playerInRange    = false;
        private bool  dialogueOpen     = false;
        private int   selectedReply    = -1;

        private DialogueLine activeLine    = null;
        private string       reactionText  = "";    // Memory-driven reaction displayed above head
        private float        reactionTimer = 0f;

        private GUIStyle promptStyle;
        private GUIStyle dialogueBoxStyle;
        private GUIStyle npcTextStyle;
        private GUIStyle replyButtonStyle;
        private GUIStyle reactionStyle;
        private bool     stylesInit = false;

        // ------------------------------------------------------------------ //
        //  Lifecycle
        // ------------------------------------------------------------------ //
        protected override void Awake()
        {
            base.Awake();
            agent.speed = 1.8f;
        }

        private void Start()
        {
            BuildVisual(bodyColor);
        }

        protected override void Update()
        {
            base.Update();                     // Memory tick + state machine
            FollowSchedule();                  // Override wander with schedule
            CheckPlayerProximity();            // Interaction prompt
            TickReaction();                    // Clear reaction speech bubble timer
        }

        // ------------------------------------------------------------------ //
        //  Schedule
        // ------------------------------------------------------------------ //
        private void FollowSchedule()
        {
            if (schedule == null || !schedule.HasEntries) return;
            if (GameClock.Instance == null) return;

            var entry = schedule.GetActiveEntry(GameClock.Instance.CurrentHour);
            if (entry == null) return;

            // Only redirect if not in dialogue
            if (!dialogueOpen)
                SetScheduleDestination(entry.targetPosition);
        }

        // ------------------------------------------------------------------ //
        //  Proximity & Dialogue
        // ------------------------------------------------------------------ //
        private void CheckPlayerProximity()
        {
            var playerTf = FindPlayer();
            if (playerTf == null) { playerInRange = false; return; }

            float dist = Vector3.Distance(transform.position, playerTf.position);
            playerInRange = dist < interactionRadius;

            if (playerInRange && !dialogueOpen && Input.GetKeyDown(interactKey))
            {
                OpenDialogue();
            }
        }

        private void OpenDialogue()
        {
            if (dialogueTree == null) return;

            bool hasMemory = Memory.HasAnyFor("Player_01");
            activeLine    = dialogueTree.GetBestLine(TrustTracker.Level, hasMemory);

            // Inject memory-driven context into NPC text
            if (hasMemory && activeLine != null)
            {
                var vivid = Memory.GetMostVividFor("Player_01");
                if (vivid != null && vivid.strength > 0.3f)
                {
                    activeLine = new DialogueLine
                    {
                        minimumTrust   = activeLine.minimumTrust,
                        npcText        = $"{activeLine.npcText} ...\n<i>({vivid.ToNarrativeLine()})</i>",
                        playerReplies  = activeLine.playerReplies,
                        replyTrustDeltas = activeLine.replyTrustDeltas,
                        replyRepDeltas   = activeLine.replyRepDeltas,
                        requiresMemory = activeLine.requiresMemory
                    };
                }
            }

            dialogueOpen = true;
            selectedReply = -1;

            // Stop NPC moving while talking
            agent.SetDestination(transform.position);
            // Face player
            var player = FindPlayer();
            if (player != null)
                transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }

        private void CloseDialogue(int replyChoice)
        {
            if (activeLine == null) { dialogueOpen = false; return; }

            if (replyChoice >= 0 && replyChoice < (activeLine.replyTrustDeltas?.Length ?? 0))
            {
                float trustDelta = activeLine.replyTrustDeltas[replyChoice];
                float repDelta   = activeLine.replyRepDeltas  != null && replyChoice < activeLine.replyRepDeltas.Length
                                   ? activeLine.replyRepDeltas[replyChoice] : 0f;

                // Publish as a player action event so the entire memory pipeline runs
                EventBus.Publish(new PlayerActionEvent(
                    actorId:         "Player_01",
                    actionType:      trustDelta >= 0 ? ActionType.HelpedNPC : ActionType.AssaultedCitizen,
                    location:        transform.position,
                    targetId:        npcId,
                    reputationDelta: repDelta,
                    trustDelta:      trustDelta,
                    actionName:      $"Replied to {displayName}: '{activeLine.playerReplies[replyChoice]}'"));
            }

            dialogueOpen  = false;
            activeLine    = null;
            selectedReply = -1;
        }

        // ------------------------------------------------------------------ //
        //  Memory reaction override
        // ------------------------------------------------------------------ //
        protected override void OnMemoryStored(MemoryEntry entry, bool wasWitness)
        {
            // Show a floating reaction bubble above the NPC's head
            reactionText = TrustTracker.Level switch
            {
                TrustLevel.Hostile    => "😠 Stay away from me!",
                TrustLevel.Suspicious => "😟 I'm watching you...",
                TrustLevel.Neutral    => "😐 Hmm...",
                TrustLevel.Trusted    => "😊 Good to see you!",
                _                     => ""
            };

            if (entry.trustDelta < -0.5f)
                reactionText = "😱 What are you doing?!";
            else if (entry.trustDelta > 0.4f)
                reactionText = "🙏 Thank you!";

            reactionTimer = 3.5f;
        }

        private void TickReaction()
        {
            if (reactionTimer > 0f)
                reactionTimer -= Time.deltaTime;
            else
                reactionText = "";
        }

        // ------------------------------------------------------------------ //
        //  OnGUI — Interaction prompt + Dialogue box + Reaction bubble
        // ------------------------------------------------------------------ //
        private void InitStyles()
        {
            if (stylesInit) return;
            float scale = Mathf.Max(1f, Screen.height / 720f);

            promptStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize  = Mathf.RoundToInt(13 * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText  = true
            };
            promptStyle.normal.textColor = Color.white;

            dialogueBoxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(14, 14, 14, 14)
            };

            npcTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(14 * scale),
                wordWrap  = true,
                richText  = true
            };
            npcTextStyle.normal.textColor = new Color(0.9f, 0.95f, 1f);

            replyButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize  = Mathf.RoundToInt(13 * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            reactionStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize  = Mathf.RoundToInt(13 * scale),
                alignment = TextAnchor.MiddleCenter,
                richText  = true
            };
            reactionStyle.normal.textColor = Color.white;

            stylesInit = true;
        }

        private void OnGUI()
        {
            InitStyles();
            float scale = Mathf.Max(1f, Screen.height / 720f);

            // --- Reaction bubble (world→screen) ---
            if (!string.IsNullOrEmpty(reactionText) && !dialogueOpen)
            {
                Vector3 screenPos = Camera.main != null
                    ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.7f)
                    : Vector3.zero;

                if (screenPos.z > 0f)
                {
                    float bw = 220f * scale;
                    float bh = 36f  * scale;
                    GUI.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 0.88f);
                    GUI.Box(new Rect(screenPos.x - bw * 0.5f, Screen.height - screenPos.y - bh, bw, bh),
                            reactionText, reactionStyle);
                }
            }

            // --- Enter proximity prompt ---
            if (playerInRange && !dialogueOpen)
            {
                string trustStr = TrustTracker.ToDisplayString();
                string color    = TrustTracker.Level switch
                {
                    TrustLevel.Trusted    => "#00FF88",
                    TrustLevel.Suspicious => "#FFAA00",
                    TrustLevel.Hostile    => "#FF4444",
                    _                     => "#FFFFFF"
                };
                float pw = 300f * scale;
                float ph = 44f  * scale;
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.14f, 0.92f);
                GUI.Box(new Rect((Screen.width - pw) * 0.5f, Screen.height * 0.65f, pw, ph),
                        $"[E] Talk to <b>{displayName}</b>  <color={color}>{trustStr}</color>", promptStyle);
            }

            // --- Dialogue box ---
            if (!dialogueOpen || activeLine == null) return;

            float screenW = Screen.width;
            float screenH = Screen.height;
            float panelW  = Mathf.Min(500f * scale, screenW * 0.92f);
            float panelH  = Mathf.Min(300f * scale, screenH * 0.45f);
            float panelX  = (screenW - panelW) * 0.5f;
            float panelY  = screenH - panelH - 20f * scale;

            GUI.backgroundColor = new Color(0.08f, 0.10f, 0.14f, 0.96f);
            GUILayout.BeginArea(new Rect(panelX, panelY, panelW, panelH), dialogueBoxStyle);

            // NPC name header
            GUI.backgroundColor = Color.clear;
            GUILayout.Label($"<b><color=#FFDD44>{displayName}</color></b>  " +
                            $"<size={Mathf.RoundToInt(11 * scale)}><color=#888888>{TrustTracker.ToDisplayString()}</color></size>",
                            npcTextStyle);
            GUILayout.Space(6f * scale);

            // NPC speech
            GUILayout.Label(activeLine.npcText, npcTextStyle);
            GUILayout.Space(10f * scale);

            // Player reply options
            if (activeLine.playerReplies != null)
            {
                for (int i = 0; i < activeLine.playerReplies.Length; i++)
                {
                    float trustVal = (activeLine.replyTrustDeltas != null && i < activeLine.replyTrustDeltas.Length)
                                     ? activeLine.replyTrustDeltas[i] : 0f;
                    string replColor = trustVal > 0 ? "#88FF99" : (trustVal < 0 ? "#FF8888" : "#CCCCCC");
                    GUI.backgroundColor = new Color(0.15f, 0.18f, 0.22f);

                    if (GUILayout.Button($"<color={replColor}>{activeLine.playerReplies[i]}</color>",
                                         replyButtonStyle, GUILayout.Height(36f * scale)))
                    {
                        CloseDialogue(i);
                    }
                }
            }

            GUILayout.Space(6f * scale);
            GUI.backgroundColor = new Color(0.3f, 0.3f, 0.35f);
            if (GUILayout.Button("[X] End Conversation", replyButtonStyle, GUILayout.Height(30f * scale)))
                CloseDialogue(-1);

            GUI.backgroundColor = Color.white;
            GUILayout.EndArea();
        }
    }
}
