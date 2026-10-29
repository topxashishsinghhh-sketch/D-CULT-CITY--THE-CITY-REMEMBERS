using System;
using UnityEngine;

namespace DCultCity.Player
{
    public enum GenderType
    {
        Male,
        Female
    }

    /// <summary>
    /// Stores customized character appearance and identity data.
    /// Can be serialized to JSON for save/load systems.
    /// </summary>
    [Serializable]
    public class CharacterCustomizationData
    {
        public string characterName = "Citizen_01";
        public GenderType gender = GenderType.Male;
        public int skinToneIndex = 1;
        public int hairStyleIndex = 0;
        public int hairColorIndex = 0;
        public int clothingColorIndex = 0;
        public int pantsColorIndex = 0;

        // Color palettes for quick customization
        public static readonly Color[] SkinTones = new Color[]
        {
            new Color(0.98f, 0.85f, 0.74f), // Light
            new Color(0.89f, 0.71f, 0.55f), // Tan
            new Color(0.72f, 0.52f, 0.38f), // Olive / Caramel
            new Color(0.53f, 0.36f, 0.24f), // Brown
            new Color(0.33f, 0.22f, 0.15f)  // Deep Espresso
        };

        public static readonly Color[] HairColors = new Color[]
        {
            new Color(0.12f, 0.10f, 0.10f), // Midnight Black
            new Color(0.35f, 0.23f, 0.15f), // Chestnut Brown
            new Color(0.85f, 0.72f, 0.45f), // Golden Blonde
            new Color(0.70f, 0.25f, 0.15f), // Auburn Red
            new Color(0.80f, 0.80f, 0.85f), // Platinum Silver
            new Color(0.15f, 0.85f, 0.75f)  // Cyber Cyan
        };

        public static readonly Color[] ClothingColors = new Color[]
        {
            new Color(0.15f, 0.15f, 0.18f), // Stealth Black
            new Color(0.85f, 0.20f, 0.20f), // Crimson Jacket
            new Color(0.18f, 0.40f, 0.75f), // Royal Blue
            new Color(0.25f, 0.55f, 0.30f), // Olive Green
            new Color(0.92f, 0.92f, 0.94f), // Crisp White
            new Color(0.75f, 0.45f, 0.15f)  // Leather Amber
        };

        public static readonly Color[] PantsColors = new Color[]
        {
            new Color(0.15f, 0.22f, 0.35f), // Denim Blue
            new Color(0.12f, 0.12f, 0.12f), // Charcoal Dark
            new Color(0.60f, 0.52f, 0.42f), // Khaki Tan
            new Color(0.20f, 0.20f, 0.20f)  // Black Cargo
        };

        public Color GetCurrentSkinColor() => SkinTones[Mathf.Clamp(skinToneIndex, 0, SkinTones.Length - 1)];
        public Color GetCurrentHairColor() => HairColors[Mathf.Clamp(hairColorIndex, 0, HairColors.Length - 1)];
        public Color GetCurrentClothingColor() => ClothingColors[Mathf.Clamp(clothingColorIndex, 0, ClothingColors.Length - 1)];
        public Color GetCurrentPantsColor() => PantsColors[Mathf.Clamp(pantsColorIndex, 0, PantsColors.Length - 1)];

        public CharacterCustomizationData Clone()
        {
            return new CharacterCustomizationData
            {
                characterName = this.characterName,
                gender = this.gender,
                skinToneIndex = this.skinToneIndex,
                hairStyleIndex = this.hairStyleIndex,
                hairColorIndex = this.hairColorIndex,
                clothingColorIndex = this.clothingColorIndex,
                pantsColorIndex = this.pantsColorIndex
            };
        }
    }
}
