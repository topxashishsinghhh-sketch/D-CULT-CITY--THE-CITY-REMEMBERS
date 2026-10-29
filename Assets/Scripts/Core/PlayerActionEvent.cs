using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCultCity.Core
{
    /// <summary>
    /// Categories of player actions that impact memory, trust, and faction reputation.
    /// </summary>
    public enum ActionType
    {
        HelpedNPC,
        StoleCar,
        WonCasino,
        HitPedestrian,
        AssaultedCitizen,
        CompletedDelivery,
        BribedOfficer,
        RescuedAnimal,
        Custom
    }

    /// <summary>
    /// Core event representing an action in the world.
    /// Captures who did it, what was done, where, when, and who witnessed it.
    /// Used by NPCMemory, TrustSystem, and FactionReputation.
    /// </summary>
    [Serializable]
    public struct PlayerActionEvent : IEvent
    {
        public string actorId;          // Who performed the action (e.g. "Player")
        public ActionType actionType;   // What happened
        public string actionName;       // Custom name or description of action
        public Vector3 location;        // Where it occurred
        public float timestamp;         // In-game time when it happened
        public string targetId;         // Who was affected (e.g. "NPC_Marcus", "Sedan_01")
        public List<string> witnesses;  // NPCs or entities within line-of-sight / detection radius
        public float reputationDelta;   // Direct impact on faction/global rep (-1.0 to +1.0)
        public float trustDelta;        // Direct impact on target NPC trust (-1.0 to +1.0)

        public PlayerActionEvent(
            string actorId,
            ActionType actionType,
            Vector3 location,
            string targetId = "None",
            List<string> witnesses = null,
            float reputationDelta = 0f,
            float trustDelta = 0f,
            string actionName = null)
        {
            this.actorId = actorId;
            this.actionType = actionType;
            this.location = location;
            this.timestamp = Time.time;
            this.targetId = targetId ?? "None";
            this.witnesses = witnesses ?? new List<string>();
            this.reputationDelta = reputationDelta;
            this.trustDelta = trustDelta;
            this.actionName = string.IsNullOrEmpty(actionName) ? actionType.ToString() : actionName;
        }

        public override string ToString()
        {
            string witnessStr = witnesses != null && witnesses.Count > 0 ? string.Join(", ", witnesses) : "None";
            return $"[PlayerActionEvent] Time: {timestamp:F1}s | Actor: {actorId} | Action: {actionType} ({actionName}) | Target: {targetId} | Loc: {location} | Witnesses: [{witnessStr}] | Rep: {reputationDelta:+0.00;-0.00;0.00} | Trust: {trustDelta:+0.00;-0.00;0.00}";
        }
    }
}
