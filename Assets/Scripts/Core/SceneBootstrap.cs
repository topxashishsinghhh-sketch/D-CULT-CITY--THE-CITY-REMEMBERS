using DCultCity.Core;
using DCultCity.Missions;
using DCultCity.NPC;
using DCultCity.Player;
using DCultCity.Reputation;
using DCultCity.SaveLoad;
using DCultCity.UI;
using DCultCity.Vehicles;
using DCultCity.Wildlife;
using DCultCity.World;
using UnityEngine;

namespace DCultCity.Core
{
    /// <summary>
    /// Initializes Milestone 1 & 2 scene components automatically at runtime.
    /// Ensures zero-setup testing: spawns player, camera, mobile controls, and character creator.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitializeScene()
        {
            // Check if player already exists
            var existingPlayer = FindObjectOfType<ThirdPersonController>();
            if (existingPlayer != null)
            {
                return; // Scene already manually configured
            }

            Debug.Log("<color=#00FFAA>[SceneBootstrap]</color> Auto-configuring Milestone 2 Player & Systems...");

            // 1. Setup City Environment
            GameObject cityManager = GameObject.Find("=== DCULT_CITY_SYSTEM ===");
            if (cityManager == null)
            {
                cityManager = new GameObject("=== DCULT_CITY_SYSTEM ===");
                cityManager.AddComponent<TestCityBuilder>();
                cityManager.AddComponent<EventBusTestListener>();
                cityManager.AddComponent<EventBusTestPublisher>();
                cityManager.AddComponent<Milestone1HUD>();
            }

            // 2. Spawn Third-Person Player
            GameObject playerObj = new GameObject("Player_Citizen");
            playerObj.transform.position = new Vector3(0f, 0.5f, 0f);

            var charController = playerObj.AddComponent<CharacterController>();
            var charVisual = playerObj.AddComponent<CharacterVisual>();
            var inputReader = playerObj.AddComponent<PlayerInputReader>();
            var tpController = playerObj.AddComponent<ThirdPersonController>();

            // 3. Setup Directional Sun Light if needed
            Light sunLight = FindObjectOfType<Light>();
            if (sunLight == null)
            {
                GameObject lightObj = new GameObject("Directional Light");
                sunLight = lightObj.AddComponent<Light>();
                sunLight.type = LightType.Directional;
                sunLight.color = new Color(1f, 0.96f, 0.85f);
                sunLight.intensity = 1.25f;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // 4. Setup Third-Person Camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }

            // Replace SimpleFreeCam with ThirdPersonCamera
            var freeCam = mainCam.GetComponent<SimpleFreeCam>();
            if (freeCam != null) Destroy(freeCam);

            var tpCam = mainCam.GetComponent<ThirdPersonCamera>();
            if (tpCam == null)
            {
                tpCam = mainCam.gameObject.AddComponent<ThirdPersonCamera>();
            }
            tpCam.Target = playerObj.transform;

            // 5. Setup UI Overlays (Mobile Controls & Character Creator)
            cityManager.AddComponent<MobileControlsOverlay>();
            cityManager.AddComponent<CharacterCreatorUI>();

            // 6. Spawn drivable vehicle (Milestone 3)
            cityManager.AddComponent<TestCityVehicleSpawner>();

            // 7. Milestone 4: NPC systems — GossipSystem, GameClock, FactionReputation, NPCFactory
            // GossipSystem & GameClock as persistent singletons
            if (FindObjectOfType<GossipSystem>() == null)
            {
                GameObject npcSystems = new GameObject("=== NPC_SYSTEMS ===");
                npcSystems.AddComponent<GossipSystem>();
                npcSystems.AddComponent<GameClock>();
                npcSystems.AddComponent<FactionReputation>();
            }
            // Bake NavMesh so NPCFactory agents can navigate (fires before NPCFactory.Start())
            cityManager.AddComponent<RuntimeNavMeshBaker>();

            // NPCFactory spawns 7 Story NPCs into the city
            cityManager.AddComponent<NPCFactory>();

            // 8. NPC Debug HUD — clock, faction rep bars, nearest NPC trust
            cityManager.AddComponent<NPCDebugHUD>();

            // 9. Milestone 5 — Save / Load system
            if (FindObjectOfType<SaveLoadManager>() == null)
                cityManager.AddComponent<SaveLoadManager>();
            cityManager.AddComponent<SaveLoadUI>();
            cityManager.AddComponent<QuickSaveHotkeys>();

            // 10. Milestone 6 — Open World Biomes, Wildlife, Optimization & Flow
            cityManager.AddComponent<FullWorldBuilder>();
            cityManager.AddComponent<WildlifeSpawner>();
            cityManager.AddComponent<NPCDistanceCuller>();
            cityManager.AddComponent<QualitySettingsManager>();

            if (FindObjectOfType<MissionManager>() == null)
                cityManager.AddComponent<MissionManager>();
            cityManager.AddComponent<MissionUI>();

            if (FindObjectOfType<GameFlowManager>() == null)
                cityManager.AddComponent<GameFlowManager>();
            cityManager.AddComponent<MainMenuUI>();

            // 11. Atmosphere, Sound & Pursuit Systems
            cityManager.AddComponent<DayNightCycle>();
            cityManager.AddComponent<ProceduralAudioEngine>();
            cityManager.AddComponent<PolicePursuitManager>();
            cityManager.AddComponent<CasinoMiniGame>();

            Debug.Log("<color=#00FFCC>[SceneBootstrap]</color> Milestone 6 Complete! Full World, 50 NPCs, Missions, Wildlife, Day/Night, Audio & Police Pursuits Active.");
        }
    }
}
