using System;
using System.Collections.Generic;
using DCultCity.NPC;
using DCultCity.Player;
using UnityEngine;

namespace DCultCity.SaveLoad
{
    // ================================================================== //
    //  Serialization-safe wrappers
    //  Unity's JsonUtility cannot serialize Dictionary<K,V> directly.
    //  We use flat lists of key-value pair structs instead.
    // ================================================================== //

    [Serializable]
    public struct StringFloatPair
    {
        public string key;
        public float  value;
        public StringFloatPair(string k, float v) { key = k; value = v; }
    }

    // ================================================================== //
    //  Per-NPC snapshot
    // ================================================================== //

    [Serializable]
    public class NPCSaveData
    {
        public string             npcId;
        public float              trustCurrent;    // Raw trust score
        public float              trustBase;       // Designer-set base
        public List<MemoryEntry>  memories;        // Full memory list with strength values

        public NPCSaveData() { memories = new List<MemoryEntry>(); }
    }

    // ================================================================== //
    //  Faction reputation snapshot
    // ================================================================== //

    [Serializable]
    public class FactionSaveData
    {
        public List<StringFloatPair> factionScores = new List<StringFloatPair>();
    }

    // ================================================================== //
    //  Player / character snapshot
    // ================================================================== //

    [Serializable]
    public class PlayerSaveData
    {
        public string characterName;
        public int    gender;              // 0 = Male, 1 = Female (enum → int)
        public int    skinToneIndex;
        public int    hairStyleIndex;
        public int    hairColorIndex;
        public int    clothingColorIndex;
        public int    pantsColorIndex;

        // World position/rotation
        public float  posX, posY, posZ;
        public float  rotY;
    }

    // ================================================================== //
    //  Root save document
    // ================================================================== //

    [Serializable]
    public class GameSaveData
    {
        public int               saveSlot       = 0;
        public string            saveDateTime;       // Human-readable timestamp
        public float             gameTimeSeconds;    // Time.time at save
        public float             inGameHour;         // GameClock.CurrentHour

        public PlayerSaveData    player         = new PlayerSaveData();
        public FactionSaveData   factions       = new FactionSaveData();
        public List<NPCSaveData> npcs           = new List<NPCSaveData>();

        /// <summary>Convenience factory — sets metadata fields.</summary>
        public static GameSaveData CreateNew(int slot)
        {
            return new GameSaveData
            {
                saveSlot       = slot,
                saveDateTime   = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                gameTimeSeconds = Time.time
            };
        }
    }
}
