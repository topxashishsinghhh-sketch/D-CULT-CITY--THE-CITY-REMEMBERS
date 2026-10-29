using UnityEngine;

namespace DCultCity.NPC
{
    /// <summary>
    /// Ambient NPC (40 total across city, beach, and parks).
    /// Inherits from NPCBase:
    /// - Shares full EventBus reaction and memory system
    /// - Reacts dynamically (stares, flees, calls police)
    /// - Simple trust tracking, but NO interactive dialogue
    /// </summary>
    public class AmbientNPC : NPCBase
    {
        [Header("Ambient Customization")]
        [SerializeField] private Color clothingColor;

        private void Start()
        {
            if (clothingColor == default)
            {
                Color[] palettes = {
                    new Color(0.2f, 0.4f, 0.6f),
                    new Color(0.6f, 0.25f, 0.25f),
                    new Color(0.3f, 0.55f, 0.35f),
                    new Color(0.7f, 0.6f, 0.2f),
                    new Color(0.45f, 0.45f, 0.5f)
                };
                clothingColor = palettes[Random.Range(0, palettes.Length)];
            }

            BuildVisual(clothingColor);
        }

        protected override void OnStateChanged(NPCState from, NPCState to)
        {
            base.OnStateChanged(from, to);

            // Audio / visual reaction cues
            if (to == NPCState.Flee)
            {
                agent.speed = 5.2f;
            }
            else if (to == NPCState.CallPolice)
            {
                agent.speed = 0f;
            }
            else
            {
                agent.speed = 2.0f;
            }
        }
    }
}
