using DCultCity.Vehicles;
using UnityEngine;

namespace DCultCity.World
{
    /// <summary>
    /// Extends the test city with a parked drivable vehicle.
    /// Spawns the vehicle, positions it on the roadway near the spawn point,
    /// and adds a VehicleMobileHUD to the city manager for mobile driving.
    /// </summary>
    public class TestCityVehicleSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Vector3 spawnPosition  = new Vector3(6f,   0.35f, -3f);
        [SerializeField] private Vector3 spawnRotation  = new Vector3(0f, -90f,    0f);
        [SerializeField] private bool    spawnOnAwake   = true;

        private GameObject spawnedVehicle;

        private void Awake()
        {
            if (spawnOnAwake) SpawnVehicle();
        }

        [ContextMenu("Spawn Test Vehicle")]
        public void SpawnVehicle()
        {
            if (spawnedVehicle != null)
            {
                if (Application.isPlaying) Destroy(spawnedVehicle);
                else DestroyImmediate(spawnedVehicle);
            }

            // Create vehicle root
            spawnedVehicle = new GameObject("DCultVehicle_RedSedan");
            spawnedVehicle.transform.position = spawnPosition;
            spawnedVehicle.transform.rotation = Quaternion.Euler(spawnRotation);

            // Attach components — VehicleController.Awake() chains visual + colliders
            var controller = spawnedVehicle.AddComponent<VehicleController>();
            controller.definition.vehicleId  = "Vehicle_RedSedan_01";
            controller.definition.bodyColor  = new Color(0.78f, 0.15f, 0.15f);
            controller.definition.vehicleClass = VehicleClass.Sedan;

            // Ensure mobile HUD component exists on this manager
            if (GetComponent<VehicleMobileHUD>() == null)
                gameObject.AddComponent<VehicleMobileHUD>();

            Debug.Log("<color=#FFCC00>[TestCityVehicleSpawner]</color> Red Sedan spawned! Walk up and press [F] to enter.");
        }
    }
}
