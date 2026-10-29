using DCultCity.SaveLoad;
using UnityEngine;

namespace DCultCity.Core
{
    /// <summary>
    /// Provides keyboard quick-save (F5) and quick-load (F9) shortcuts.
    /// Always saves/loads to slot 0. Works in Editor and builds.
    /// Also adds a diagnostic context-menu method to print save file location.
    /// </summary>
    public class QuickSaveHotkeys : MonoBehaviour
    {
        [Header("Hotkeys")]
        [SerializeField] private KeyCode quickSaveKey  = KeyCode.F5;
        [SerializeField] private KeyCode quickLoadKey  = KeyCode.F9;
        [SerializeField] private int     quickSaveSlot = 0;

        private void Update()
        {
            if (Input.GetKeyDown(quickSaveKey))
            {
                SaveLoadManager.Instance?.Save(quickSaveSlot);
                Debug.Log($"<color=#00FF88>[QuickSave]</color> F5 → Saved to slot {quickSaveSlot + 1}.");
            }

            if (Input.GetKeyDown(quickLoadKey))
            {
                var data = SaveLoadManager.Instance?.Load(quickSaveSlot);
                if (data != null)
                    Debug.Log($"<color=#00FFCC>[QuickLoad]</color> F9 → Loaded slot {quickSaveSlot + 1} " +
                              $"(saved {data.saveDateTime}, {data.npcs.Count} NPCs restored).");
                else
                    Debug.LogWarning($"[QuickLoad] No save found in slot {quickSaveSlot + 1}.");
            }
        }

        [ContextMenu("Print Save File Paths")]
        private void PrintPaths()
        {
            for (int i = 0; i < 3; i++)
            {
                bool ex = SaveLoadManager.Instance?.SlotExists(i) ?? false;
                Debug.Log($"Slot {i + 1}: {Application.persistentDataPath}/dcult_save_slot{i}.json  [exists={ex}]");
            }
        }
    }
}
