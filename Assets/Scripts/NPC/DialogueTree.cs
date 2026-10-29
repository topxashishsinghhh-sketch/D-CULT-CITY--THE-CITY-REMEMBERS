using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCultCity.NPC
{
    /// <summary>
    /// A single line of dialogue plus trust-level requirement and optional memory reference.
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
        public TrustLevel minimumTrust;          // Only shown if NPC trust meets this level
        public string npcText;                   // What the NPC says
        public string[] playerReplies;           // Available player reply options
        public float[] replyTrustDeltas;         // Trust change for each reply choice
        public float[] replyRepDeltas;           // Reputation change for each reply choice
        public bool     requiresMemory;          // Only shown if NPC has a memory of the player
    }

    /// <summary>
    /// A full dialogue tree for one NPC. Contains multiple dialogue lines
    /// selected dynamically based on the NPC's current trust level and memory.
    /// </summary>
    [Serializable]
    public class DialogueTree
    {
        public List<DialogueLine> lines = new List<DialogueLine>();

        /// <summary>
        /// Select the most appropriate opening line for the current trust level and memory state.
        /// </summary>
        public DialogueLine GetBestLine(TrustLevel trust, bool hasMemory)
        {
            DialogueLine best = null;
            foreach (var line in lines)
            {
                if (line.requiresMemory && !hasMemory) continue;
                if (trust < line.minimumTrust) continue;

                // Pick the line whose minimum trust is closest to (but not above) current trust
                if (best == null || line.minimumTrust > best.minimumTrust)
                    best = line;
            }
            return best ?? (lines.Count > 0 ? lines[0] : null);
        }
    }
}
