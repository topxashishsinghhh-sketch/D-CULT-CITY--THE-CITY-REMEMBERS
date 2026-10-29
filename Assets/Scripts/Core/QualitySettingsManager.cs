using DCultCity.NPC;
using UnityEngine;

namespace DCultCity.Core
{
    public enum GameQualityPreset
    {
        Low,       // High FPS, battery-friendly for mobile
        Medium,    // Balanced performance and visuals
        High       // Full draw distance and shadows for desktop
    }

    /// <summary>
    /// Manages graphic performance presets (Low / Medium / High).
    /// Adjusts shadow distance, target framerate, LOD bias, and NPC culling range dynamically.
    /// </summary>
    public class QualitySettingsManager : MonoBehaviour
    {
        public static QualitySettingsManager Instance { get; private set; }

        [SerializeField] private GameQualityPreset currentPreset = GameQualityPreset.Medium;

        public GameQualityPreset CurrentPreset => currentPreset;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            // Detect mobile platform for sensible default
            if (Application.isMobilePlatform)
            {
                currentPreset = GameQualityPreset.Medium;
            }
            ApplyPreset(currentPreset);
        }

        public void ApplyPreset(GameQualityPreset preset)
        {
            currentPreset = preset;

            switch (preset)
            {
                case GameQualityPreset.Low:
                    QualitySettings.shadowDistance = 22f;
                    QualitySettings.lodBias = 0.65f;
                    Application.targetFrameRate = 30;
                    QualitySettings.vSyncCount = 0;
                    UpdateCullerDistance(35f);
                    break;

                case GameQualityPreset.Medium:
                    QualitySettings.shadowDistance = 50f;
                    QualitySettings.lodBias = 1.0f;
                    Application.targetFrameRate = 60;
                    QualitySettings.vSyncCount = 1;
                    UpdateCullerDistance(55f);
                    break;

                case GameQualityPreset.High:
                    QualitySettings.shadowDistance = 90f;
                    QualitySettings.lodBias = 1.5f;
                    Application.targetFrameRate = 120;
                    QualitySettings.vSyncCount = 1;
                    UpdateCullerDistance(85f);
                    break;
            }

            Debug.Log($"<color=#33FF88>[QualitySettingsManager]</color> Applied Graphic Preset: {preset}");
        }

        private void UpdateCullerDistance(float dist)
        {
            var culler = FindObjectOfType<NPCDistanceCuller>();
            if (culler != null)
            {
                culler.SetCullDistance(dist);
            }
        }
    }
}
