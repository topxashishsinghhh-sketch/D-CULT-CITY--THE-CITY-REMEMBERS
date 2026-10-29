using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DCultCity.NPC
{
    /// <summary>
    /// Performance Optimizer: Distant NPC Culling & Pooling.
    /// Automatically deactivates visual renderers and halts NavMesh updates for NPCs
    /// outside the active player radius, saving massive CPU and GPU overhead on mobile and desktop.
    /// </summary>
    public class NPCDistanceCuller : MonoBehaviour
    {
        [Header("Culling Radii")]
        [SerializeField] private float cullDistance = 55f;
        [SerializeField] private float checkInterval = 0.35f;

        private Transform playerTf;
        private float timer = 0f;
        private readonly List<CullEntry> trackedNPCs = new List<CullEntry>();

        private struct CullEntry
        {
            public Transform transform;
            public Renderer[] renderers;
            public NavMeshAgent agent;
            public bool isCulled;
        }

        public void SetCullDistance(float dist)
        {
            cullDistance = dist;
        }

        private void Start()
        {
            var p = FindObjectOfType<DCultCity.Player.ThirdPersonController>();
            if (p != null) playerTf = p.transform;
            RefreshTrackedNPCs();
        }

        public void RefreshTrackedNPCs()
        {
            trackedNPCs.Clear();
            var all = FindObjectsOfType<NPCBase>();
            foreach (var npc in all)
            {
                trackedNPCs.Add(new CullEntry
                {
                    transform = npc.transform,
                    renderers = npc.GetComponentsInChildren<Renderer>(true),
                    agent = npc.GetComponent<NavMeshAgent>(),
                    isCulled = false
                });
            }
        }

        private void Update()
        {
            if (playerTf == null)
            {
                var p = FindObjectOfType<DCultCity.Player.ThirdPersonController>();
                if (p != null) playerTf = p.transform;
                return;
            }

            timer += Time.deltaTime;
            if (timer >= checkInterval)
            {
                timer = 0f;
                CullPass();
            }
        }

        private void CullPass()
        {
            Vector3 playerPos = playerTf.position;
            float sqrCull = cullDistance * cullDistance;

            for (int i = 0; i < trackedNPCs.Count; i++)
            {
                var entry = trackedNPCs[i];
                if (entry.transform == null) continue;

                float sqrDist = (entry.transform.position - playerPos).sqrMagnitude;
                bool shouldCull = sqrDist > sqrCull;

                if (shouldCull != entry.isCulled)
                {
                    entry.isCulled = shouldCull;
                    for (int r = 0; r < entry.renderers.Length; r++)
                    {
                        if (entry.renderers[r] != null)
                        {
                            entry.renderers[r].enabled = !shouldCull;
                        }
                    }

                    if (entry.agent != null)
                    {
                        entry.agent.isStopped = shouldCull;
                    }

                    trackedNPCs[i] = entry;
                }
            }
        }
    }
}
