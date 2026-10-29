using UnityEngine;

namespace DCultCity.NPC
{
    /// <summary>
    /// Per-NPC trust score for a specific actor (usually the player).
    /// Trust starts at Neutral (0) and is clamped to [-1, +1].
    ///   > +0.5  = Trusted Friend
    ///   0 to +0.5 = Acquaintance / Neutral
    ///  -0.5 to 0  = Suspicious / Cold
    ///  < -0.5  = Hostile / Will call police / Flee
    /// Trust is derived from NPCMemory weighted deltas plus an explicit
    /// base adjustment so designers can set a starting disposition.
    /// </summary>
    [System.Serializable]
    public class TrustSystem
    {
        [Header("Trust Configuration")]
        [SerializeField] private float baseTrust     = 0f;    // Designer-set starting bias
        [SerializeField] private float currentTrust  = 0f;    // Computed + base

        // Boundaries
        private const float TRUSTED   =  0.50f;
        private const float HOSTILE   = -0.50f;

        public float Current => currentTrust;

        public TrustLevel Level => currentTrust switch
        {
            >= TRUSTED   => TrustLevel.Trusted,
            >= 0f        => TrustLevel.Neutral,
            >= HOSTILE   => TrustLevel.Suspicious,
            _            => TrustLevel.Hostile
        };

        public TrustSystem(float baseTrust = 0f)
        {
            this.baseTrust    = baseTrust;
            this.currentTrust = baseTrust;
        }

        /// <summary>
        /// Recompute trust from memory entries + base.
        /// Call this whenever NPCMemory changes.
        /// </summary>
        public void Recalculate(NPCMemory memory, string actorId)
        {
            float memoryContribution = memory.GetNetTrustFor(actorId);
            currentTrust = Mathf.Clamp(baseTrust + memoryContribution, -1f, 1f);
        }

        /// <summary>Directly bump trust (e.g. from scripted cutscene).</summary>
        public void ApplyDelta(float delta)
        {
            currentTrust = Mathf.Clamp(currentTrust + delta, -1f, 1f);
        }

        public void SetBase(float val) { baseTrust = Mathf.Clamp(val, -1f, 1f); }

        public string ToDisplayString() =>
            $"{Level} ({currentTrust:+0.00;-0.00;0.00})";
    }

    public enum TrustLevel
    {
        Trusted,
        Neutral,
        Suspicious,
        Hostile
    }
}
