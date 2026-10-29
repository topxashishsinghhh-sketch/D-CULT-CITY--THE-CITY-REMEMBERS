using System;
using System.Collections.Generic;
using System.IO;
using DCultCity.NPC;
using DCultCity.Player;
using DCultCity.Reputation;
using UnityEngine;

namespace DCultCity.SaveLoad
{
    /// <summary>
    /// Central save / load manager for D-CULT CITY.
    ///
    /// Architecture:
    ///   • 3 save slots stored as JSON files in Application.persistentDataPath.
    ///   • Captures: player position, character customization, all NPC trust scores
    ///     and full NPCMemory entries (including decay strength), faction reputation,
    ///     and in-game clock hour.
    ///   • JsonUtility used for speed and mobile compatibility.
    ///     Dictionary<K,V> is serialized as List<StringFloatPair>.
    ///   • AutoSave can be triggered on a timer (default every 2 minutes).
    ///   • Static singleton — accessible from any system via SaveLoadManager.Instance.
    /// </summary>
    public class SaveLoadManager : MonoBehaviour
    {
        // ------------------------------------------------------------------ //
        //  Singleton
        // ------------------------------------------------------------------ //
        public static SaveLoadManager Instance { get; private set; }

        // ------------------------------------------------------------------ //
        //  Configuration
        // ------------------------------------------------------------------ //
        [Header("Save Slots")]
        [SerializeField] private int totalSlots    = 3;
        [SerializeField] private int activeSlot    = 0;

        [Header("Auto-Save")]
        [SerializeField] private bool  autoSaveEnabled  = true;
        [SerializeField] private float autoSaveInterval = 120f;  // seconds

        // ------------------------------------------------------------------ //
        //  Internal state
        // ------------------------------------------------------------------ //
        private float autoSaveTimer = 0f;

        /// <summary>Fired after a successful save — slot index is passed.</summary>
        public event Action<int>         OnSaveCompleted;
        /// <summary>Fired after a successful load — loaded data is passed.</summary>
        public event Action<GameSaveData> OnLoadCompleted;

        // ------------------------------------------------------------------ //
        //  Lifecycle
        // ------------------------------------------------------------------ //
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Update()
        {
            if (!autoSaveEnabled) return;

            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                autoSaveTimer = 0f;
                Save(activeSlot);
                Debug.Log($"<color=#AAFFCC>[SaveLoadManager]</color> Auto-saved to slot {activeSlot}.");
            }
        }

        // ------------------------------------------------------------------ //
        //  File path helpers
        // ------------------------------------------------------------------ //
        private string GetSavePath(int slot)
        {
            string dir = Application.persistentDataPath;
            return Path.Combine(dir, $"dcult_save_slot{slot}.json");
        }

        public bool SlotExists(int slot) => File.Exists(GetSavePath(slot));

        // ------------------------------------------------------------------ //
        //  SAVE
        // ------------------------------------------------------------------ //
        public void Save(int slot)
        {
            try
            {
                var data = GameSaveData.CreateNew(slot);

                // --- Player ---
                CapturePlayer(ref data.player);

                // --- Factions ---
                CaptureFactions(ref data.factions);

                // --- NPC memories ---
                CaptureNPCs(ref data.npcs);

                // --- Game clock ---
                if (DCultCity.NPC.GameClock.Instance != null)
                    data.inGameHour = DCultCity.NPC.GameClock.Instance.CurrentHour;

                // --- Serialize ---
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(GetSavePath(slot), json);

                activeSlot = slot;
                OnSaveCompleted?.Invoke(slot);

                Debug.Log($"<color=#00FF88>[SaveLoadManager]</color> Game saved to slot {slot} → {GetSavePath(slot)}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadManager] Save FAILED: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // ------------------------------------------------------------------ //
        //  LOAD
        // ------------------------------------------------------------------ //
        public GameSaveData Load(int slot)
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[SaveLoadManager] No save file found for slot {slot} at: {path}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                var data    = JsonUtility.FromJson<GameSaveData>(json);

                // --- Restore player ---
                RestorePlayer(data.player);

                // --- Restore factions ---
                RestoreFactions(data.factions);

                // --- Restore NPC memories ---
                RestoreNPCs(data.npcs);

                // --- Restore game clock ---
                // GameClock doesn't expose a setter — we use a public method added below
                var clock = FindObjectOfType<DCultCity.NPC.GameClock>();
                if (clock != null) clock.SetHour(data.inGameHour);

                activeSlot = slot;
                OnLoadCompleted?.Invoke(data);

                Debug.Log($"<color=#00FFCC>[SaveLoadManager]</color> Loaded slot {slot} (saved {data.saveDateTime}).");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadManager] Load FAILED: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        // ------------------------------------------------------------------ //
        //  DELETE
        // ------------------------------------------------------------------ //
        public void DeleteSlot(int slot)
        {
            string path = GetSavePath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"<color=#FFAA44>[SaveLoadManager]</color> Deleted save slot {slot}.");
            }
        }

        // ------------------------------------------------------------------ //
        //  Slot metadata (for the UI to read without fully loading)
        // ------------------------------------------------------------------ //
        public GameSaveData PeekSlot(int slot)
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path)) return null;
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<GameSaveData>(json);
            }
            catch { return null; }
        }

        public int   TotalSlots  => totalSlots;
        public int   ActiveSlot  => activeSlot;
        public string SaveFolder => Application.persistentDataPath;

        // ================================================================== //
        //  CAPTURE helpers
        // ================================================================== //

        private void CapturePlayer(ref PlayerSaveData pd)
        {
            var ctrl = FindObjectOfType<DCultCity.Player.ThirdPersonController>();
            if (ctrl == null) return;

            Vector3 pos = ctrl.transform.position;
            pd.posX = pos.x;
            pd.posY = pos.y;
            pd.posZ = pos.z;
            pd.rotY = ctrl.transform.eulerAngles.y;

            var visual = ctrl.Visual;
            if (visual?.Data == null) return;
            var d = visual.Data;

            pd.characterName    = d.characterName;
            pd.gender           = (int)d.gender;
            pd.skinToneIndex    = d.skinToneIndex;
            pd.hairStyleIndex   = d.hairStyleIndex;
            pd.hairColorIndex   = d.hairColorIndex;
            pd.clothingColorIndex = d.clothingColorIndex;
            pd.pantsColorIndex  = d.pantsColorIndex;
        }

        private void CaptureFactions(ref FactionSaveData fd)
        {
            if (FactionReputation.Instance == null) return;
            fd.factionScores.Clear();
            foreach (var kv in FactionReputation.Instance.GetSnapshot())
                fd.factionScores.Add(new StringFloatPair(kv.Key, kv.Value));
        }

        private void CaptureNPCs(ref System.Collections.Generic.List<NPCSaveData> list)
        {
            list.Clear();
            var allNPCs = FindObjectsOfType<NPCBase>();
            foreach (var npc in allNPCs)
            {
                var snap = new NPCSaveData
                {
                    npcId        = npc.NPCId,
                    trustCurrent = npc.TrustTracker.Current,
                    memories     = npc.Memory.GetSerializableList()
                };
                list.Add(snap);
            }
        }

        // ================================================================== //
        //  RESTORE helpers
        // ================================================================== //

        private void RestorePlayer(PlayerSaveData pd)
        {
            var ctrl = FindObjectOfType<DCultCity.Player.ThirdPersonController>();
            if (ctrl == null) return;

            // Move player to saved position
            ctrl.GetComponent<CharacterController>()?.enabled = false;
            ctrl.transform.position = new Vector3(pd.posX, pd.posY, pd.posZ);
            ctrl.transform.rotation = Quaternion.Euler(0f, pd.rotY, 0f);
            ctrl.GetComponent<CharacterController>()?.enabled = true;

            // Restore appearance
            var data = new CharacterCustomizationData
            {
                characterName     = pd.characterName,
                gender            = (GenderType)pd.gender,
                skinToneIndex     = pd.skinToneIndex,
                hairStyleIndex    = pd.hairStyleIndex,
                hairColorIndex    = pd.hairColorIndex,
                clothingColorIndex = pd.clothingColorIndex,
                pantsColorIndex   = pd.pantsColorIndex
            };

            ctrl.Visual?.ApplyCustomization(data);
        }

        private void RestoreFactions(FactionSaveData fd)
        {
            if (FactionReputation.Instance == null) return;
            var dict = new System.Collections.Generic.Dictionary<string, float>();
            foreach (var pair in fd.factionScores)
                dict[pair.key] = pair.value;
            FactionReputation.Instance.LoadSnapshot(dict);
        }

        private void RestoreNPCs(System.Collections.Generic.List<NPCSaveData> list)
        {
            var allNPCs = FindObjectsOfType<NPCBase>();

            // Build lookup by id
            var npcMap = new System.Collections.Generic.Dictionary<string, NPCBase>();
            foreach (var npc in allNPCs)
                npcMap[npc.NPCId] = npc;

            foreach (var snap in list)
            {
                if (!npcMap.TryGetValue(snap.npcId, out NPCBase npc)) continue;

                // Restore full memory
                npc.Memory.LoadFromList(snap.memories);

                // Recalculate trust from the restored memories
                npc.TrustTracker.Recalculate(npc.Memory, "Player_01");
            }

            Debug.Log($"<color=#AAFFEE>[SaveLoadManager]</color> Restored {list.Count} NPC memory snapshots.");
        }
    }
}
