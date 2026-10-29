using System.Collections.Generic;
using UnityEngine;

namespace DCultCity.Core
{
    /// <summary>
    /// Test harness component to publish sample player actions into the EventBus.
    /// Supports desktop keyboard shortcuts, mobile touch buttons, and Unity Inspector menus.
    /// </summary>
    public class EventBusTestPublisher : MonoBehaviour
    {
        [Header("Default Actor")]
        [SerializeField] private string defaultActorId = "Player_01";

        [Header("Desktop Keybinds")]
        [SerializeField] private KeyCode keyHelpedNPC = KeyCode.Alpha1;
        [SerializeField] private KeyCode keyStoleCar = KeyCode.Alpha2;
        [SerializeField] private KeyCode keyWonCasino = KeyCode.Alpha3;
        [SerializeField] private KeyCode keyHitPedestrian = KeyCode.Alpha4;

        private void Update()
        {
            // Desktop hotkeys for rapid testing
            if (Input.GetKeyDown(keyHelpedNPC))
            {
                TriggerHelpedNPC();
            }
            else if (Input.GetKeyDown(keyStoleCar))
            {
                TriggerStoleCar();
            }
            else if (Input.GetKeyDown(keyWonCasino))
            {
                TriggerWonCasino();
            }
            else if (Input.GetKeyDown(keyHitPedestrian))
            {
                TriggerHitPedestrian();
            }
        }

        [ContextMenu("Trigger: Helped NPC (+Trust)")]
        public void TriggerHelpedNPC()
        {
            var evt = new PlayerActionEvent(
                actorId: defaultActorId,
                actionType: ActionType.HelpedNPC,
                location: transform.position,
                targetId: "NPC_Elena_Florist",
                witnesses: new List<string> { "NPC_Shopkeeper_Sam", "NPC_Pedestrian_01" },
                reputationDelta: 0.15f,
                trustDelta: 0.30f,
                actionName: "Recovered stolen inventory for Elena"
            );

            EventBus.Publish(evt);
        }

        [ContextMenu("Trigger: Stole Car (-Reputation)")]
        public void TriggerStoleCar()
        {
            var evt = new PlayerActionEvent(
                actorId: defaultActorId,
                actionType: ActionType.StoleCar,
                location: transform.position,
                targetId: "Vehicle_Sedan_Red",
                witnesses: new List<string> { "Officer_Dave", "NPC_Bystander_08" },
                reputationDelta: -0.40f,
                trustDelta: -0.50f,
                actionName: "Hotwired red civilian sedan"
            );

            EventBus.Publish(evt);
        }

        [ContextMenu("Trigger: Won at Casino")]
        public void TriggerWonCasino()
        {
            var evt = new PlayerActionEvent(
                actorId: defaultActorId,
                actionType: ActionType.WonCasino,
                location: new Vector3(25f, 0f, 15f),
                targetId: "Casino_Roulette_HighStakes",
                witnesses: new List<string> { "Casino_Dealer_Jack", "PitBoss_Vance" },
                reputationDelta: 0.05f,
                trustDelta: 0.0f,
                actionName: "Won $25,000 jackpot on roulette"
            );

            EventBus.Publish(evt);
        }

        [ContextMenu("Trigger: Hit Pedestrian (Crime)")]
        public void TriggerHitPedestrian()
        {
            var evt = new PlayerActionEvent(
                actorId: defaultActorId,
                actionType: ActionType.HitPedestrian,
                location: transform.position + transform.forward * 3f,
                targetId: "NPC_Civilian_23",
                witnesses: new List<string> { "Officer_Dave", "NPC_Vendor_Leo", "NPC_Bystander_02" },
                reputationDelta: -0.65f,
                trustDelta: -0.80f,
                actionName: "Reckless vehicular impact with civilian"
            );

            EventBus.Publish(evt);
        }

        /// <summary>
        /// Allows publishing custom dynamic actions.
        /// </summary>
        public void TriggerCustomAction(ActionType actionType, string targetId, float repDelta, float trustDelta, string notes)
        {
            var evt = new PlayerActionEvent(
                actorId: defaultActorId,
                actionType: actionType,
                location: transform.position,
                targetId: targetId,
                witnesses: new List<string> { "Witness_Local" },
                reputationDelta: repDelta,
                trustDelta: trustDelta,
                actionName: notes
            );

            EventBus.Publish(evt);
        }
    }
}
