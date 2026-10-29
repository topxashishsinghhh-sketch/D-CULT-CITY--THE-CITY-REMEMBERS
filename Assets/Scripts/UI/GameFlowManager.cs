using DCultCity.SaveLoad;
using UnityEngine;

namespace DCultCity.UI
{
    public enum GameFlowState
    {
        Splash,
        Login,
        MainMenu,
        Settings,
        CharacterSelect,
        InGame
    }

    /// <summary>
    /// Orchestrates the entire game flow:
    /// Splash → Login (Email or Guest) → Main Menu (New Game, Continue, Settings, Quit) → Character Select → In-Game.
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        public static GameFlowManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private GameFlowState currentState = GameFlowState.InGame; // Starts InGame for immediate testing, or toggled to Splash

        [Header("Player Credentials")]
        public string loggedInUser = "Guest_Player";
        public bool isGuest = true;

        public GameFlowState CurrentState => currentState;

        public event System.Action<GameFlowState> OnStateChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        public void TransitionTo(GameFlowState newState)
        {
            currentState = newState;
            Debug.Log($"<color=#FF88FF>[GameFlowManager]</color> State transitioned to: {newState}");

            // Handle subsystem enabling/disabling
            var charCreator = FindObjectOfType<CharacterCreatorUI>();
            var player = FindObjectOfType<DCultCity.Player.ThirdPersonController>();

            if (newState == GameFlowState.CharacterSelect)
            {
                if (charCreator != null && !charCreator.IsOpen)
                {
                    charCreator.ToggleCreatorView();
                }
            }
            else if (newState == GameFlowState.InGame)
            {
                if (player != null) player.enabled = true;
            }
            else
            {
                if (player != null) player.enabled = false;
            }

            OnStateChanged?.Invoke(newState);
        }

        public void StartNewGame()
        {
            TransitionTo(GameFlowState.CharacterSelect);
        }

        public void ContinueGame()
        {
            if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SlotExists(0))
            {
                SaveLoadManager.Instance.Load(0);
                TransitionTo(GameFlowState.InGame);
            }
            else
            {
                // Fallback to new game if no save exists
                StartNewGame();
            }
        }
    }
}
