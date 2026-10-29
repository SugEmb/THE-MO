using UnityEngine;
#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE  // "Both" defines the first without the package
using UnityEngine.InputSystem;
#endif

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// The keys the kit reads, on whichever input handling the project has (the Input System, the
    /// old Input Manager, or both): E interacts, F the flashlight, Tab or I opens the
    /// inventory, Esc or a right click closes or cancels. Your own input can skip it: call <see cref="Interactor.Interact"/> and the Hud's
    /// <see cref="Hud.ToggleInventory"/> from it and set <see cref="Interactor.readKeys"/> off.
    /// </summary>
    public static class Controls
    {
#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE
        static Keyboard K => Keyboard.current;
        static Mouse M => Mouse.current;
        public static bool Interact => K != null && K.eKey.wasPressedThisFrame;
        public static bool Flashlight => K != null && K.fKey.wasPressedThisFrame;
        public static bool Inventory => K != null && (K.tabKey.wasPressedThisFrame || K.iKey.wasPressedThisFrame);
        public static bool Cancel => K != null && K.escapeKey.wasPressedThisFrame || M != null && M.rightButton.wasPressedThisFrame;
        public static bool Confirm => K != null && (K.enterKey.wasPressedThisFrame || K.numpadEnterKey.wasPressedThisFrame || K.spaceKey.wasPressedThisFrame);
        public static bool Click => M != null && M.leftButton.wasPressedThisFrame;
        public static bool Held => M != null && M.leftButton.isPressed;
        public static Vector2 Look => M == null ? Vector2.zero : M.delta.ReadValue() * 0.1f;  // as the old Mouse X and Y
        public static float Wheel => M == null || M.scroll.ReadValue().y == 0f ? 0f : Mathf.Sign(M.scroll.ReadValue().y);
        public static bool Running => K != null && K.shiftKey.isPressed;
        public static bool Jump => K != null && K.spaceKey.wasPressedThisFrame;

        public static Vector2 Move
        {
            get
            {
                if (K == null) return Vector2.zero;
                float Down(UnityEngine.InputSystem.Controls.KeyControl a, UnityEngine.InputSystem.Controls.KeyControl b) => a.isPressed || b.isPressed ? 1f : 0f;
                return new Vector2(Down(K.dKey, K.rightArrowKey) - Down(K.aKey, K.leftArrowKey), Down(K.wKey, K.upArrowKey) - Down(K.sKey, K.downArrowKey));
            }
        }

        /// <summary>A digit key pressed this frame (top row or keypad), or -1.</summary>
        public static int Digit
        {
            get
            {
                if (K == null) return -1;
                for (var d = 0; d <= 9; d++)
                    if (K[(Key)((int)Key.Digit0 + d)].wasPressedThisFrame || K[(Key)((int)Key.Numpad0 + d)].wasPressedThisFrame)
                        return d;
                return -1;
            }
        }

        public static bool Back => K != null && (K.backspaceKey.wasPressedThisFrame || K.deleteKey.wasPressedThisFrame);
#else
        public static bool Interact => Input.GetKeyDown(KeyCode.E);
        public static bool Flashlight => Input.GetKeyDown(KeyCode.F);
        public static bool Inventory => Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I);
        public static bool Cancel => Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1);
        public static bool Confirm => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space);
        public static bool Click => Input.GetMouseButtonDown(0);
        public static bool Held => Input.GetMouseButton(0);
        public static Vector2 Look => new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        public static float Wheel => Input.mouseScrollDelta.y == 0f ? 0f : Mathf.Sign(Input.mouseScrollDelta.y);
        public static bool Running => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        public static bool Jump => Input.GetKeyDown(KeyCode.Space);
        public static Vector2 Move => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        public static bool Back => Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete);

        public static int Digit
        {
            get
            {
                for (var d = 0; d <= 9; d++)
                    if (Input.GetKeyDown(KeyCode.Alpha0 + d) || Input.GetKeyDown(KeyCode.Keypad0 + d))
                        return d;
                return -1;
            }
        }
#endif
    }
}
