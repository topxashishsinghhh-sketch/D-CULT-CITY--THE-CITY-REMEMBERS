using UnityEngine;
using UnityEngine.AI;

namespace DCultCity.Wildlife
{
    /// <summary>
    /// Spawns ambient wildlife (Deer in forest, Wolves in mountain foothills).
    /// </summary>
    public class WildlifeSpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] private int deerCount = 6;
        [SerializeField] private int wolfCount = 3;
        [SerializeField] private bool spawnOnAwake = true;

        private void Start()
        {
            if (spawnOnAwake)
            {
                SpawnFauna();
            }
        }

        [ContextMenu("Spawn Fauna")]
        public void SpawnFauna()
        {
            // Forest Deer spawn points (North-West forest zone)
            Vector3[] deerSpawns = {
                new Vector3(-60f, 0.5f, 60f), new Vector3(-68f, 0.5f, 72f),
                new Vector3(-50f, 0.5f, 78f), new Vector3(-75f, 0.5f, 55f),
                new Vector3(-55f, 0.5f, 85f), new Vector3(-82f, 0.5f, 68f)
            };

            for (int i = 0; i < deerCount && i < deerSpawns.Length; i++)
            {
                CreateAnimal(deerSpawns[i], AnimalSpecies.ForestDeer, $"Deer_{i + 1}");
            }

            // Mountain Wolves (North-East foothills)
            Vector3[] wolfSpawns = {
                new Vector3(40f, 0.5f, 70f), new Vector3(65f, 0.5f, 60f),
                new Vector3(75f, 0.5f, 35f)
            };

            for (int i = 0; i < wolfCount && i < wolfSpawns.Length; i++)
            {
                CreateAnimal(wolfSpawns[i], AnimalSpecies.MountainWolf, $"Wolf_{i + 1}");
            }

            Debug.Log("<color=#00FF88>[WildlifeSpawner]</color> Wildlife populated across Forest and Mountain foothills.");
        }

        private void CreateAnimal(Vector3 pos, AnimalSpecies species, string name)
        {
            Vector3 spawnPos = pos;
            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
            }

            GameObject go = new GameObject(name);
            go.transform.position = spawnPos;
            var animal = go.AddComponent<AnimalBase>();

            var field = typeof(AnimalBase).GetField("species", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(animal, species);
        }
    }
}
