# Interaction

The player looks at something within reach and presses E. That is the whole loop, and every
other system is built on it.

- **Interactor**, on the player's camera: each frame it casts a ray from the view's centre,
  `reach` metres (2 by default), and rests on the nearest `Interactable` that can be used. E uses
  it. Its `body` is the player's root, what saves store and place; left empty, it is the
  CharacterController or Rigidbody above the camera.
- **Interactable** is the base of everything usable: doors, pickups, switches. Each has a `label`
  ("Office door"), a verb the prompt shows (OPEN, UNLOCK, TAKE) and a focus icon (hand, examine,
  door, key). Disabled, it can't be focused or used. Every Interactable fires `onInteracted`.
- **Controls** reads the keys on whichever input handling your project has (the Input System,
  the old Input Manager, or both): E use, F flashlight, Tab or I inventory, Esc or a right click
  cancel.

## Your own controller

The systems only need the Interactor. While a panel is open (the inventory, a code pad, a note)
`Interactor.Paused` is true: your controller should stand still and free the cursor. Listen to
`Interactor.PausedChanged`. A close-up of your own (a minigame) holds the player the same way with
`Interactor.Hold(this, true)`, and lets go with `false`; while it holds, the Hud leaves Esc and Tab to
it and draws no aim dot or prompt.

**Unity's Starter Assets (First Person).** Import them, drop in their `PlayerCapsule`, then:

1. Add an `Interactor` to `PlayerCapsule/PlayerCameraRoot`. It turns with the view; with no
   camera on it, the ray goes along its forward.
2. Add this to the PlayerCapsule, so panels stop the player and free the mouse:

```csharp
using HeyHeyThere.EscapePuzzleKitFree;
using StarterAssets;
using UnityEngine;

public class PauseStarterAssets : MonoBehaviour
{
    void OnEnable() => Interactor.PausedChanged += Pause;
    void OnDisable() => Interactor.PausedChanged -= Pause;

    void Pause(bool paused)
    {
        GetComponent<FirstPersonController>().enabled = !paused;
        var input = GetComponent<StarterAssetsInputs>();
        input.cursorLocked = input.cursorInputForLook = !paused;
        input.SetCursorState(!paused);
        input.move = input.look = Vector2.zero;
    }
}
```

**Your own keys.** Turn off `readKeys` on the Interactor and the Flashlight, and call
`Interactor.Current.Interact()`, `Flashlight.Current.Toggle()` and
`Hud.Current.ToggleInventory()` from your input.

## From code

```csharp
door.Interact(Interactor.Current);                 // as if the player pressed E on it
keypad.Interact(Interactor.Current, keyCollider);  // on one part: a keypad's key
```
