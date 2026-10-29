using UnityEngine;

namespace DCultCity.Vehicles
{
    /// <summary>
    /// Enum-driven vehicle configuration so a single VehicleController handles
    /// sedans, SUVs, sports cars, and police cruisers without subclassing.
    /// </summary>
    public enum VehicleClass
    {
        Sedan,
        Sports,
        SUV,
        PoliceCruiser
    }

    /// <summary>
    /// Stores all tuning parameters for a D-CULT CITY vehicle.
    /// Shared between VehicleController and VehicleProceduralVisual.
    /// </summary>
    [System.Serializable]
    public class VehicleDefinition
    {
        [Header("Identity")]
        public string vehicleId = "Vehicle_Sedan_01";
        public VehicleClass vehicleClass = VehicleClass.Sedan;

        [Header("Driving Tuning")]
        [Tooltip("Forward acceleration force in Newtons")]
        public float motorForce        = 2200f;
        [Tooltip("Reverse acceleration force")]
        public float reverseForce      = 1100f;
        [Tooltip("Maximum steer angle at slow speed (degrees)")]
        public float maxSteerAngle     = 32f;
        [Tooltip("Ackermann speed ratio — reduces steer angle at high speed")]
        public float steerSpeedRatio   = 0.05f;
        [Tooltip("Braking force applied when brake input or coasting")]
        public float brakeForce        = 3500f;
        [Tooltip("Passive drag torque on wheels when no throttle input")]
        public float dragBrakeForce    = 400f;
        [Tooltip("Top speed in m/s before motor force is cut")]
        public float topSpeed          = 28f;

        [Header("Enter / Exit")]
        [Tooltip("Radius within which the player can prompt to enter")]
        public float enterRadius       = 3.2f;
        [Tooltip("Offset from the vehicle origin where the player stands after exit")]
        public Vector3 exitOffset      = new Vector3(1.8f, 0f, 0f);

        [Header("Appearance")]
        public Color bodyColor         = new Color(0.72f, 0.18f, 0.18f);
        public Color roofColor         = new Color(0.12f, 0.12f, 0.14f);
        public Color windowColor       = new Color(0.45f, 0.62f, 0.80f, 0.55f);
        public Color wheelColor        = new Color(0.10f, 0.10f, 0.10f);
        public Color lightColor        = new Color(1.00f, 0.95f, 0.80f);
    }
}
