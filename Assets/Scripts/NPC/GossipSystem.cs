using System.Collections.Generic;
using DCultCity.NPC;
using UnityEngine;

namespace DCultCity.NPC
{
    /// <summary>
    /// Spreads reputation knowledge between nearby NPCs at regular intervals.
    /// Gossip transfers MemoryEntries from NPC to NPC within hearing radius.
    /// This makes the city feel "alive" — if the player helped someone near the shops,
    /// NPCs in the park might hear about it within minutes.
    /// </summary>
    public class GossipSystem : MonoBehaviour
    {
        public static GossipSystem Instance { get; private set; }

        [Header("Gossip Settings")]
        [SerializeField] private float gossipRadius    = 12f;   // How far gossip travels
        [SerializeField] private float gossipInterval  = 8f;    // Seconds between gossip ticks
        [SerializeField] private float gossipChance    = 0.55f; // Probability an NPC spreads to each neighbour

        private float nextGossipTime = 0f;
        private readonly List<NPCBase> registeredNPCs = new List<NPCBase>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (Time.time >= nextGossipTime)
            {
                nextGossipTime = Time.time + gossipInterval;
                RunGossipCycle();
            }
        }

        public void Register(NPCBase npc)
        {
            if (!registeredNPCs.Contains(npc))
                registeredNPCs.Add(npc);
        }

        public void Unregister(NPCBase npc) => registeredNPCs.Remove(npc);

        private void RunGossipCycle()
        {
            // Clean up nulls
            registeredNPCs.RemoveAll(n => n == null);

            foreach (var speaker in registeredNPCs)
            {
                if (speaker.Memory.Entries.Count == 0) continue;

                foreach (var listener in registeredNPCs)
                {
                    if (listener == speaker) continue;
                    if (Vector3.Distance(speaker.transform.position, listener.transform.position) > gossipRadius) continue;
                    if (Random.value > gossipChance) continue;

                    // Share the most vivid memory the speaker has about the player
                    var vivid = speaker.Memory.GetMostVividFor("Player_01");
                    if (vivid != null && vivid.strength > 0.25f)
                    {
                        listener.Memory.StoreGossip(vivid);
                        listener.TrustTracker.Recalculate(listener.Memory, "Player_01");
                        // Debug.Log($"[Gossip] {speaker.NPCId} → {listener.NPCId}: '{vivid.actionName}'");
                    }
                }
            }
        }
    }
}
