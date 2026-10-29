using UnityEngine;

namespace DCultCity.Player
{
    /// <summary>
    /// Unified input reader handling Desktop (Keyboard & Mouse) and Mobile (Touch / On-Screen Joysticks).
    /// Decouples controller logic from hardware-specific input APIs.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        public static PlayerInputReader Instance { get; private set; }

        [Header("Desktop Configuration")]
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode customizeKey = KeyCode.C;

        // Internal values
        private Vector2 desktopMove;
        private Vector2 desktopLook;
        private bool desktopSprint;
        private bool desktopJump;

        // Mobile touch inputs set by VirtualJoystick / Mobile HUD
        private Vector2 mobileMove;
        private Vector2 mobileLook;
        private bool mobileSprint;
        private bool mobileJump;
        private bool mobileCustomizeTriggered;

        public Vector2 MoveInput => (desktopMove + mobileMove).sqrMagnitude > 1f ? (desktopMove + mobileMove).normalized : (desktopMove + mobileMove);
        public Vector2 LookInput => desktopLook + mobileLook;
        public bool IsSprinting => desktopSprint || mobileSprint;
        public bool JumpRequested => desktopJump || mobileJump;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Update()
        {
            // Desktop Keyboard
            desktopMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (desktopMove.sqrMagnitude > 1f) desktopMove.Normalize();

            desktopSprint = Input.GetKey(sprintKey);
            desktopJump = Input.GetKeyDown(jumpKey);

            // Desktop Mouse look (right mouse button drag or unlocked cursor)
            if (Input.GetMouseButton(1) || Cursor.lockState == CursorLockMode.Locked)
            {
                desktopLook = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }
            else
            {
                desktopLook = Vector2.zero;
            }

            // Keyboard shortcut to toggle customization
            if (Input.GetKeyDown(customizeKey))
            {
                mobileCustomizeTriggered = true;
            }
        }

        private void LateUpdate()
        {
            // Clear single-frame jump pulses
            mobileJump = false;
        }

        // --- Mobile Interface Methods ---
        public void SetMobileMove(Vector2 move) => mobileMove = move;
        public void SetMobileLook(Vector2 look) => mobileLook = look;
        public void SetMobileSprint(bool sprinting) => mobileSprint = sprinting;
        public void TriggerMobileJump() => mobileJump = true;
        
        public bool ConsumeCustomizeTrigger()
        {
            bool val = mobileCustomizeTriggered;
            mobileCustomizeTriggered = false;
            return val;
        }
    }
}
