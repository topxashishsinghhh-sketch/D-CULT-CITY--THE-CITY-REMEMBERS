using System;
using DCultCity.Vehicles;
using UnityEngine;

namespace DCultCity.Core
{
    /// <summary>
    /// Self-contained Procedural Audio Engine for D-CULT CITY.
    /// Synthesizes procedural sound effects (engine hum, sirens, footsteps, casino chimes,
    /// crime stings, UI clicks) entirely in memory without requiring external audio asset files.
    /// Automatically reacts to EventBus signals.
    /// </summary>
    public class ProceduralAudioEngine : MonoBehaviour
    {
        public static ProceduralAudioEngine Instance { get; private set; }

        private AudioSource sfxSource;
        private AudioSource engineSource;
        private AudioSource sirenSource;

        private AudioClip footstepClip;
        private AudioClip casinoWinClip;
        private AudioClip crimeStingClip;
        private AudioClip clickClip;
        private AudioClip engineHumClip;
        private AudioClip sirenClip;

        private VehicleController activeVehicle;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            engineSource = gameObject.AddComponent<AudioSource>();
            engineSource.playOnAwake = false;
            engineSource.loop = true;
            engineSource.volume = 0.45f;

            sirenSource = gameObject.AddComponent<AudioSource>();
            sirenSource.playOnAwake = false;
            sirenSource.loop = true;
            sirenSource.volume = 0.55f;

            GenerateClips();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerActionEvent>(OnPlayerAction);
            EventBus.Subscribe<VehicleOccupancyEvent>(OnVehicleOccupancy);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerActionEvent>(OnPlayerAction);
            EventBus.Unsubscribe<VehicleOccupancyEvent>(OnVehicleOccupancy);
        }

        private void Update()
        {
            // Modulate engine audio pitch by vehicle speed
            if (activeVehicle != null && activeVehicle.IsOccupied)
            {
                if (!engineSource.isPlaying) engineSource.Play();
                float speedKmh = activeVehicle.SpeedKmh;
                engineSource.pitch = Mathf.Lerp(0.85f, 2.2f, speedKmh / 100f);
            }
            else if (engineSource.isPlaying)
            {
                engineSource.Stop();
            }
        }

        private void OnPlayerAction(PlayerActionEvent evt)
        {
            switch (evt.actionType)
            {
                case ActionType.WonCasino:
                    PlayClip(casinoWinClip, 0.8f);
                    break;
                case ActionType.StoleCar:
                case ActionType.HitPedestrian:
                case ActionType.AssaultedCitizen:
                    PlayClip(crimeStingClip, 0.9f);
                    break;
                default:
                    PlayClip(clickClip, 0.5f);
                    break;
            }
        }

        private void OnVehicleOccupancy(VehicleOccupancyEvent evt)
        {
            if (evt.change == VehicleOccupancyEvent.OccupancyChange.Entered)
            {
                activeVehicle = FindObjectOfType<VehicleController>();
                engineSource.clip = engineHumClip;
                engineSource.Play();
            }
            else
            {
                engineSource.Stop();
                activeVehicle = null;
            }
        }

        public void PlayFootstep() => PlayClip(footstepClip, 0.35f);
        public void PlayUIClick() => PlayClip(clickClip, 0.6f);

        public void SetSirenActive(bool active)
        {
            if (active && !sirenSource.isPlaying)
            {
                sirenSource.clip = sirenClip;
                sirenSource.Play();
            }
            else if (!active && sirenSource.isPlaying)
            {
                sirenSource.Stop();
            }
        }

        private void PlayClip(AudioClip clip, float volume = 1f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        // ================================================================== //
        //  Procedural Audio Synthesis
        // ================================================================== //
        private void GenerateClips()
        {
            footstepClip = CreatePulseClip("Footstep", 140f, 0.08f, true);
            clickClip = CreatePulseClip("UIClick", 880f, 0.04f, false);
            casinoWinClip = CreateTriadChime("CasinoWin");
            crimeStingClip = CreateCrimeSting("CrimeSting");
            engineHumClip = CreateEngineLoop("EngineHum");
            sirenClip = CreateSirenLoop("PoliceSiren");
        }

        private AudioClip CreatePulseClip(string name, float freq, float duration, bool lowPass)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f - ((float)i / totalSamples); // Decay
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                if (lowPass) wave = Mathf.Sign(wave) * 0.4f; // Softer thud
                data[i] = wave * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateTriadChime(string name)
        {
            // C major triad arpeggio: C5 (523Hz), E5 (659Hz), G5 (784Hz), C6 (1046Hz)
            int sampleRate = 44100;
            float duration = 0.5f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[totalSamples];
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f };

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float sample = 0f;
                int noteIndex = Mathf.Clamp(Mathf.FloorToInt(t / (duration / notes.Length)), 0, notes.Length - 1);
                float env = 1f - (t / duration);

                sample += Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * t) * env;
                data[i] = sample * 0.35f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateCrimeSting(string name)
        {
            int sampleRate = 44100;
            float duration = 0.6f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(340f, 110f, t / duration); // Descending dramatic sting
                float env = Mathf.Pow(1f - (t / duration), 1.5f);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.3f * Mathf.Sin(4f * Mathf.PI * freq * t);
                data[i] = wave * env * 0.45f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateEngineLoop(string name)
        {
            int sampleRate = 44100;
            float duration = 1.0f; // 1s seamless loop
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float wave1 = Mathf.Sin(2f * Mathf.PI * 65f * t);
                float wave2 = Mathf.Sin(2f * Mathf.PI * 130f * t) * 0.4f;
                float wave3 = Mathf.Sin(2f * Mathf.PI * 195f * t) * 0.2f;
                data[i] = (wave1 + wave2 + wave3) * 0.35f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateSirenLoop(string name)
        {
            int sampleRate = 44100;
            float duration = 1.4f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                // Modulate frequency between 650Hz and 1050Hz
                float freq = 850f + Mathf.Sin(2f * Mathf.PI * (1f / duration) * t) * 200f;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.3f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
