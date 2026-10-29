using System;
using DCultCity.Player;

namespace DCultCity.Core
{
    /// <summary>
    /// Published when the player updates or confirms their character appearance.
    /// Informs the world, UI, and save system of the player's customized identity.
    /// </summary>
    [Serializable]
    public struct CharacterCustomizedEvent : IEvent
    {
        public string characterName;
        public GenderType gender;
        public int skinToneIndex;
        public int hairStyleIndex;
        public int hairColorIndex;
        public int clothingColorIndex;

        public CharacterCustomizedEvent(CharacterCustomizationData data)
        {
            this.characterName = data.characterName;
            this.gender = data.gender;
            this.skinToneIndex = data.skinToneIndex;
            this.hairStyleIndex = data.hairStyleIndex;
            this.hairColorIndex = data.hairColorIndex;
            this.clothingColorIndex = data.clothingColorIndex;
        }

        public override string ToString()
        {
            return $"[CharacterCustomizedEvent] Name: {characterName} | Gender: {gender} | SkinIndex: {skinToneIndex} | HairStyle: {hairStyleIndex} | HairColor: {hairColorIndex} | ClothColor: {clothingColorIndex}";
        }
    }
}
