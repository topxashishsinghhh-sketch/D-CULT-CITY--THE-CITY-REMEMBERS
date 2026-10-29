using DCultCity.NPC;
using DCultCity.Vehicles;
using UnityEngine;

namespace DCultCity.World
{
    /// <summary>
    /// Smooth dynamic Day/Night lighting and atmospheric cycle.
    /// Synchronized with GameClock.
    /// Automatically manages:
    /// - Sun rotation and color temperature (dawn golden, noon bright, dusk amber, night moon)
    /// - Streetlights and building illumination toggled between 18:00 and 06:00
    /// - Vehicle headlights activation at dusk
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        public static DayNightCycle Instance { get; private set; }

        [Header("Lighting References")]
        [SerializeField] private Light sunLight;

        [Header("Color Palettes")]
        [SerializeField] private Color dayColor = new Color(1f, 0.95f, 0.86f);
        [SerializeField] private Color duskColor = new Color(1f, 0.55f, 0.25f);
        [SerializeField] private Color nightColor = new Color(0.18f, 0.25f, 0.42f);

        private bool isNight = false;
        private Light[] cachedStreetlights;

        public bool IsNight => isNight;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            if (sunLight == null)
            {
                var lights = FindObjectsOfType<Light>();
                foreach (var l in lights)
                {
                    if (l.type == LightType.Directional)
                    {
                        sunLight = l;
                        break;
                    }
                }
            }
        }

        private void Start()
        {
            CacheStreetlights();
        }

        public void CacheStreetlights()
        {
            // Find all point lights attached to streetlights
            var allLights = FindObjectsOfType<Light>();
            var list = new System.Collections.Generic.List<Light>();
            foreach (var l in allLights)
            {
                if (l.type == LightType.Point && l.name.Contains("LightSource"))
                {
                    list.Add(l);
                }
            }
            cachedStreetlights = list.ToArray();
        }

        private void Update()
        {
            if (GameClock.Instance == null || sunLight == null) return;

            float hour = GameClock.Instance.CurrentHour; // 0 to 24

            // Calculate sun angle: 06:00 is sunrise (angle 0), 12:00 is noon (angle 90), 18:00 is sunset (angle 180)
            float sunAngle = ((hour - 6f) / 24f) * 360f;
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);

            // Time of day color blending
            bool wasNight = isNight;
            if (hour >= 6f && hour < 17f)
            {
                // Day
                isNight = false;
                sunLight.color = dayColor;
                sunLight.intensity = Mathf.Lerp(0.8f, 1.25f, Mathf.Sin((hour - 6f) / 11f * Mathf.PI));
                RenderSettings.ambientSkyColor = new Color(0.55f, 0.65f, 0.75f);
            }
            else if (hour >= 17f && hour < 20f)
            {
                // Sunset / Dusk
                isNight = hour >= 18.5f;
                float t = (hour - 17f) / 3f;
                sunLight.color = Color.Lerp(dayColor, duskColor, t);
                sunLight.intensity = Mathf.Lerp(1.0f, 0.35f, t);
                RenderSettings.ambientSkyColor = Color.Lerp(new Color(0.55f, 0.65f, 0.75f), new Color(0.4f, 0.25f, 0.2f), t);
            }
            else
            {
                // Night / Moonlight
                isNight = true;
                sunLight.color = nightColor;
                sunLight.intensity = 0.22f;
                RenderSettings.ambientSkyColor = new Color(0.10f, 0.12f, 0.18f);
            }

            if (wasNight != isNight)
            {
                ToggleNightLights(isNight);
            }
        }

        private void ToggleNightLights(bool enable)
        {
            if (cachedStreetlights == null || cachedStreetlights.Length == 0)
            {
                CacheStreetlights();
            }

            if (cachedStreetlights != null)
            {
                foreach (var sl in cachedStreetlights)
                {
                    if (sl != null) sl.enabled = enable;
                }
            }

            // Toggle vehicle headlights
            var vehicles = FindObjectsOfType<VehicleProceduralVisual>();
            foreach (var v in vehicles)
            {
                if (v.headLights != null)
                {
                    foreach (var hl in v.headLights)
                    {
                        if (hl != null) hl.enabled = enable;
                    }
                }
            }

            Debug.Log($"<color=#FFCC00>[DayNightCycle]</color> {(enable ? "Night lights enabled (Dusk)" : "Night lights extinguished (Dawn)")}.");
        }
    }
}
