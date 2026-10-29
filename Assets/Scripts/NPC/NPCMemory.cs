using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCultCity.NPC
{
    /// <summary>
    /// A single remembered event: who did what, when, where, trust/rep impact, and how well the NPC remembers it.
    /// </summary>
    [Serializable]
    public class MemoryEntry
    {
        public string    actorId;
        public string    actionName;
        public string    targetId;
        public Vector3   location;
        public float     gameTimestamp;      // Time.time when the event occurred
        public float     trustDelta;
        public float     reputationDelta;
        public float     strength;           // 1.0 = vivid, 0.0 = forgotten
        public bool      wasWitness;         // Did this NPC directly witness it?
        public bool      heardViaGossip;

        public MemoryEntry(string actorId, string actionName, string targetId, Vector3 location,
                           float gameTimestamp, float trustDelta, float reputationDelta,
                           float initialStrength, bool wasWitness)
        {
            this.actorId          = actorId;
            this.actionName       = actionName;
            this.targetId         = targetId;
            this.location         = location;
            this.gameTimestamp    = gameTimestamp;
            this.trustDelta       = trustDelta;
            this.reputationDelta  = reputationDelta;
            this.strength         = Mathf.Clamp01(initialStrength);
            this.wasWitness       = wasWitness;
            this.heardViaGossip   = false;
        }

        /// <summary>
        /// Returns a narrative sentence the NPC could say referencing this memory.
        /// </summary>
        public string ToNarrativeLine()
        {
            string prefix = heardViaGossip ? "I heard" : (wasWitness ? "I saw" : "I know");
            string ago    = FormatAgo();
            return $"{prefix} you {actionName} {ago}.";
        }

        private string FormatAgo()
        {
            float elapsed = Time.time - gameTimestamp;
            if (elapsed < 60f)  return "just a moment ago";
            if (elapsed < 300f) return "a few minutes ago";
            if (elapsed < 900f) return "earlier today";
            return "some time ago";
        }
    }

    /// <summary>
    /// Stores, fades, and queries all remembered events for a single NPC.
    /// Events fade from strength 1→0 over time. Strong/vivid events (|delta|>0.5)
    /// fade 3× slower. Gossip events start at half strength.
    /// </summary>
    [Serializable]
    public class NPCMemory
    {
        [SerializeField] private List<MemoryEntry> entries = new List<MemoryEntry>();

        // How fast memories decay (strength units per real second)
        private const float BASE_DECAY_RATE    = 0.005f;
        private const float VIVID_DECAY_FACTOR = 0.33f;   // Strong events decay at 1/3 speed
        private const float GOSSIP_START_MULT  = 0.55f;   // Gossip starts weaker

        // Maximum entries before oldest weak memories are pruned
        private const int MAX_ENTRIES = 30;

        public IReadOnlyList<MemoryEntry> Entries => entries;

        // ------------------------------------------------------------------ //
        //  Store
        // ------------------------------------------------------------------ //
        public void Store(MemoryEntry entry)
        {
            // If we already have an entry for this action+actor, reinforce it
            var existing = entries.Find(e => e.actorId == entry.actorId && e.actionName == entry.actionName);
            if (existing != null)
            {
                existing.strength    = Mathf.Min(1f, existing.strength + entry.strength * 0.5f);
                existing.gameTimestamp = entry.gameTimestamp;
                return;
            }

            entries.Add(entry);
            Prune();
        }

        public void StoreGossip(MemoryEntry sourceEntry)
        {
            var gossip      = new MemoryEntry(
                sourceEntry.actorId, sourceEntry.actionName, sourceEntry.targetId,
                sourceEntry.location, sourceEntry.gameTimestamp,
                sourceEntry.trustDelta, sourceEntry.reputationDelta,
                sourceEntry.strength * GOSSIP_START_MULT, wasWitness: false);
            gossip.heardViaGossip = true;
            Store(gossip);
        }

        // ------------------------------------------------------------------ //
        //  Decay — call from MonoBehaviour Update or coroutine
        // ------------------------------------------------------------------ //
        public void Tick(float deltaTime)
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var e    = entries[i];
                float decay = BASE_DECAY_RATE;
                if (Mathf.Abs(e.trustDelta) > 0.5f || Mathf.Abs(e.reputationDelta) > 0.5f)
                    decay *= VIVID_DECAY_FACTOR;

                e.strength -= decay * deltaTime;
                if (e.strength <= 0f)
                    entries.RemoveAt(i);
            }
        }

        // ------------------------------------------------------------------ //
        //  Query helpers
        // ------------------------------------------------------------------ //

        /// <summary>Net remembered trust delta for a given actor (weighted by memory strength).</summary>
        public float GetNetTrustFor(string actorId)
        {
            float total = 0f;
            foreach (var e in entries)
                if (e.actorId == actorId)
                    total += e.trustDelta * e.strength;
            return Mathf.Clamp(total, -1f, 1f);
        }

        /// <summary>Net remembered reputation delta for a given actor (all entries).</summary>
        public float GetNetReputationFor(string actorId)
        {
            float total = 0f;
            foreach (var e in entries)
                if (e.actorId == actorId)
                    total += e.reputationDelta * e.strength;
            return Mathf.Clamp(total, -1f, 1f);
        }

        /// <summary>Returns strongest vivid memory for this actor (for dialogue).</summary>
        public MemoryEntry GetMostVividFor(string actorId)
        {
            MemoryEntry best = null;
            foreach (var e in entries)
                if (e.actorId == actorId && (best == null || e.strength > best.strength))
                    best = e;
            return best;
        }

        /// <summary>Returns all memories for a specific actor.</summary>
        public List<MemoryEntry> GetAllFor(string actorId)
        {
            var result = new List<MemoryEntry>();
            foreach (var e in entries)
                if (e.actorId == actorId)
                    result.Add(e);
            return result;
        }

        public bool HasAnyFor(string actorId) => entries.Exists(e => e.actorId == actorId);

        private void Prune()
        {
            if (entries.Count <= MAX_ENTRIES) return;
            // Remove the weakest entry that was via gossip first, otherwise weakest overall
            entries.Sort((a, b) => a.strength.CompareTo(b.strength));
            entries.RemoveAt(0);
        }

        // ------------------------------------------------------------------ //
        //  Serialization support (Milestone 5)
        // ------------------------------------------------------------------ //
        public List<MemoryEntry> GetSerializableList() => new List<MemoryEntry>(entries);

        public void LoadFromList(List<MemoryEntry> saved)
        {
            entries.Clear();
            entries.AddRange(saved);
        }
    }
}
