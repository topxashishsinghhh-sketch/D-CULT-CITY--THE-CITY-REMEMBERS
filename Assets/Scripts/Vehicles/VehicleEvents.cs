using System;
using DCultCity.Core;
using UnityEngine;

namespace DCultCity.Vehicles
{
    /// <summary>
    /// Published when the player enters or exits a vehicle.
    /// Downstream systems (NPC memory, police faction, audio) subscribe to this.
    /// </summary>
    [Serializable]
    public struct VehicleOccupancyEvent : IEvent
    {
        public enum OccupancyChange { Entered, Exited }

        public string vehicleId;
        public OccupancyChange change;
        public string actorId;
        public Vector3 location;
        public float timestamp;

        public VehicleOccupancyEvent(string vehicleId, OccupancyChange change, string actorId, Vector3 location)
        {
            this.vehicleId = vehicleId;
            this.change = change;
            this.actorId = actorId;
            this.location = location;
            this.timestamp = Time.time;
        }

        public override string ToString() =>
            $"[VehicleOccupancyEvent] {actorId} {change} '{vehicleId}' at {location} (t={timestamp:F1}s)";
    }

    /// <summary>
    /// Published on every collision the active vehicle has with another Rigidbody or collider.
    /// NPCMemory uses this to record pedestrian-hit events.
    /// </summary>
    [Serializable]
    public struct VehicleCollisionEvent : IEvent
    {
        public string vehicleId;
        public string actorId;
        public string hitObjectTag;
        public float impactForce;
        public Vector3 location;
        public float timestamp;

        public VehicleCollisionEvent(string vehicleId, string actorId, string hitObjectTag, float impactForce, Vector3 location)
        {
            this.vehicleId = vehicleId;
            this.actorId = actorId;
            this.hitObjectTag = hitObjectTag;
            this.impactForce = impactForce;
            this.location = location;
            this.timestamp = Time.time;
        }
    }
}
