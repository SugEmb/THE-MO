using HeyHeyThere.EscapePuzzleKitFree;
using StarterAssets;
using UnityEngine;
 
namespace Echo
{
    /// <summary>
    /// Put this on PlayerCapsule (the same object that has FirstPersonController and StarterAssetsInputs).
    /// Whenever a panel opens in the Escape Puzzle Kit (pause menu, code pad, inventory, our ECHO chat),
    /// the kit raises Interactor.PausedChanged. This script stops the player and frees the mouse,
    /// then locks the mouse and lets the player walk again when the panel closes.
    /// </summary>
    public class EchoPausePlayer : MonoBehaviour
    {
        FirstPersonController controller;
        StarterAssetsInputs input;
 
        void Awake()
        {
            controller = GetComponent<FirstPersonController>();
            input = GetComponent<StarterAssetsInputs>();
        }
 
        void OnEnable() => Interactor.PausedChanged += OnPaused;
        void OnDisable() => Interactor.PausedChanged -= OnPaused;
 
        void OnPaused(bool paused)
        {
            if (controller) controller.enabled = !paused;   // stop walking and looking around
 
            if (input)
            {
                input.cursorLocked = !paused;
                input.cursorInputForLook = !paused;
                input.move = Vector2.zero;                  // clear any key still held
                input.look = Vector2.zero;
            }
 
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;                        // show the mouse so buttons can be clicked
        }
    }
}
