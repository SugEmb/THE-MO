# Story

- **Flags**: snake_case names, set once and never cleared (`generator_on`). Switches, beats and
  gates set them; doors, beats and gates wait on them. `Run.SetFlag("x")`, `Run.HasFlag("x")`,
  and the `Run.FlagSet` event.
- **StoryBeat**: a step of the story. It fires `when` the player enters its trigger (OnEnter),
  as the level starts (OnStart), or when something fires it (Fired: a Pickup's, Stash's or
  Inspectable's `beat`, or any UnityEvent calling `Fire()`). Firing, it can:
  - say a `subtitle`, with a `speaker`;
  - set the `objective`;
  - play a `sound`;
  - put its `lightsOff` out;
  - wake its `wakes`: every `IWakeable` on them (your enemies);
  - deal `damage`;
  - show a `hint` with its key (F, FLASHLIGHT);
  - set a flag, and fire `onFired`.

  `needsFlag` holds it back until that flag is set; `once` fires it the first time only.
- **Room**: a trigger naming where the player is ("Office"), shown on the save slots.
- **The objective** is `Run.Objective`, shown by the Hud as it changes. A Door's
  `lockedObjective` sets one the first time the door is found locked.

Health is `Run.Health`, 0 to 1: a beat's damage takes it, a Heal item restores it. What 0 means
is your game's: listen to `Run.HealthChanged`. `Run.Escape()` ends the run escaped, with the
Hud's end screen; a Door with `endsGame` calls it.
