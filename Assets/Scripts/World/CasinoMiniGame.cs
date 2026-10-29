using System.Collections;
using DCultCity.Core;
using UnityEngine;

namespace DCultCity.World
{
    /// <summary>
    /// Interactive Grand Lux Casino Roulette mini-game.
    /// Lets the player gamble chips on Red, Black, or Green Zero.
    /// Plays procedural wheel tick sounds and triggers EventBus PlayerActionEvent(WonCasino) on wins.
    /// </summary>
    public class CasinoMiniGame : MonoBehaviour
    {
        [Header("Casino Location")]
        [SerializeField] private Vector3 rouletteTablePos = new Vector3(15f, 0f, 15f);
        [SerializeField] private float interactRadius = 3.5f;

        [Header("Player Bankroll")]
        [SerializeField] private int playerCash = 5000;

        private bool isModalOpen = false;
        private bool isSpinning = false;
        private int selectedBetAmount = 500;
        private string selectedBetType = "RED"; // RED, BLACK, ZERO
        private string spinResultText = "";
        private int lastWinningNumber = -1;

        private GUIStyle boxStyle;
        private GUIStyle titleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle labelStyle;
        private bool stylesInit = false;

        public bool IsOpen => isModalOpen;

        private void Update()
        {
            var p = FindObjectOfType<DCultCity.Player.ThirdPersonController>();
            if (p == null) return;

            float dist = Vector3.Distance(p.transform.position, rouletteTablePos);

            if (dist < interactRadius && !isModalOpen)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    isModalOpen = true;
                    p.enabled = false;
                }
            }
        }

        private void InitStyles()
        {
            if (stylesInit) return;
            float sc = Mathf.Max(1f, Screen.height / 720f);

            boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(16, 16, 14, 14) };
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(22 * sc),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(14 * sc),
                fontStyle = FontStyle.Bold
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(13 * sc),
                alignment = TextAnchor.MiddleCenter
            };
            labelStyle.normal.textColor = Color.white;

            stylesInit = true;
        }

        private void OnGUI()
        {
            var p = FindObjectOfType<DCultCity.Player.ThirdPersonController>();
            if (p != null && !isModalOpen)
            {
                float dist = Vector3.Distance(p.transform.position, rouletteTablePos);
                if (dist < interactRadius)
                {
                    float sc = Mathf.Max(1f, Screen.height / 720f);
                    float w = 280f * sc;
                    float h = 42f * sc;
                    GUI.backgroundColor = new Color(0.12f, 0.12f, 0.16f, 0.92f);
                    GUI.Box(new Rect((Screen.width - w) * 0.5f, Screen.height * 0.7f, w, h),
                        "<b>[E] Play Roulette at Grand Lux</b>", GUI.skin.box);
                    GUI.backgroundColor = Color.white;
                }
            }

            if (!isModalOpen) return;
            InitStyles();

            float sc = Mathf.Max(1f, Screen.height / 720f);
            float screenW = Screen.width;
            float screenH = Screen.height;

            float panelW = Mathf.Min(520f * sc, screenW * 0.92f);
            float panelH = Mathf.Min(480f * sc, screenH * 0.9f);
            float panelX = (screenW - panelW) * 0.5f;
            float panelY = (screenH - panelH) * 0.5f;

            GUI.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 0.97f);
            GUILayout.BeginArea(new Rect(panelX, panelY, panelW, panelH), boxStyle);

            GUILayout.Label("GRAND LUX ROULETTE", titleStyle);
            GUILayout.Label($"Bankroll: <color=#00FFAA><b>${playerCash:N0}</b></color>", labelStyle);
            GUILayout.Space(12f * sc);

            // Bet amount selectors
            GUILayout.Label("Select Bet Chips:", labelStyle);
            GUILayout.BeginHorizontal();
            int[] betOptions = { 100, 500, 1000, 2500 };
            foreach (var b in betOptions)
            {
                GUI.backgroundColor = selectedBetAmount == b ? new Color(1f, 0.8f, 0.2f) : Color.gray;
                if (GUILayout.Button($"${b}", buttonStyle, GUILayout.Height(36f * sc)))
                {
                    selectedBetAmount = b;
                }
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            GUILayout.Space(12f * sc);

            // Bet Type
            GUILayout.Label("Place Your Bet On:", labelStyle);
            GUILayout.BeginHorizontal();

            GUI.backgroundColor = selectedBetType == "RED" ? new Color(0.95f, 0.25f, 0.25f) : new Color(0.45f, 0.15f, 0.15f);
            if (GUILayout.Button("RED (1:1)", buttonStyle, GUILayout.Height(42f * sc))) selectedBetType = "RED";

            GUI.backgroundColor = selectedBetType == "BLACK" ? new Color(0.35f, 0.35f, 0.4f) : new Color(0.18f, 0.18f, 0.2f);
            if (GUILayout.Button("BLACK (1:1)", buttonStyle, GUILayout.Height(42f * sc))) selectedBetType = "BLACK";

            GUI.backgroundColor = selectedBetType == "ZERO" ? new Color(0.2f, 0.85f, 0.4f) : new Color(0.15f, 0.4f, 0.2f);
            if (GUILayout.Button("ZERO 0 (35:1)", buttonStyle, GUILayout.Height(42f * sc))) selectedBetType = "ZERO";

            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            GUILayout.Space(16f * sc);

            // Spin Result display
            if (!string.IsNullOrEmpty(spinResultText))
            {
                GUILayout.Label(spinResultText, titleStyle);
            }
            else
            {
                GUILayout.Label(isSpinning ? "<i>Wheel is spinning...</i>" : "Place bets and spin!", labelStyle);
            }

            GUILayout.FlexibleSpace();

            // Spin button
            GUI.backgroundColor = isSpinning ? Color.gray : new Color(0.2f, 0.85f, 0.45f);
            GUI.enabled = !isSpinning && playerCash >= selectedBetAmount;
            if (GUILayout.Button(isSpinning ? "SPINNING..." : $"SPIN WHEEL (${selectedBetAmount})", buttonStyle, GUILayout.Height(48f * sc)))
            {
                StartCoroutine(SpinRoutine());
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;

            GUILayout.Space(8f * sc);

            // Leave button
            if (GUILayout.Button("LEAVE TABLE", buttonStyle, GUILayout.Height(36f * sc)))
            {
                isModalOpen = false;
                if (p != null) p.enabled = true;
            }

            GUILayout.EndArea();
        }

        private IEnumerator SpinRoutine()
        {
            isSpinning = true;
            playerCash -= selectedBetAmount;
            spinResultText = "";

            // Simulate wheel spin
            float duration = 2.0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                ProceduralAudioEngine.Instance?.PlayUIClick();
                yield return new WaitForSeconds(0.12f);
            }

            // Outcome (European roulette 0-36)
            lastWinningNumber = Random.Range(0, 37);
            bool isZero = lastWinningNumber == 0;
            bool isRed = !isZero && (lastWinningNumber % 2 == 1); // Odd is red, Even is black
            string colorName = isZero ? "GREEN" : (isRed ? "RED" : "BLACK");

            bool won = false;
            int payout = 0;

            if (selectedBetType == "ZERO" && isZero)
            {
                won = true;
                payout = selectedBetAmount * 36;
            }
            else if (selectedBetType == "RED" && isRed)
            {
                won = true;
                payout = selectedBetAmount * 2;
            }
            else if (selectedBetType == "BLACK" && !isRed && !isZero)
            {
                won = true;
                payout = selectedBetAmount * 2;
            }

            if (won)
            {
                playerCash += payout;
                spinResultText = $"<color=#00FFAA>WINNER! Number {lastWinningNumber} ({colorName}) +${payout:N0}</color>";

                // Publish EventBus action
                EventBus.Publish(new PlayerActionEvent("Player_01", ActionType.WonCasino,
                    rouletteTablePos, "Casino_Roulette", 0.08f, 0.05f, $"Won ${payout:N0} at roulette on {colorName} {lastWinningNumber}"));
            }
            else
            {
                spinResultText = $"<color=#FF4444>LOST! Number {lastWinningNumber} ({colorName})</color>";

                EventBus.Publish(new PlayerActionEvent("Player_01", ActionType.WonCasino,
                    rouletteTablePos, "Casino_Roulette", -0.01f, 0f, $"Lost ${selectedBetAmount} on roulette {colorName} {lastWinningNumber}"));
            }

            isSpinning = false;
        }
    }
}
