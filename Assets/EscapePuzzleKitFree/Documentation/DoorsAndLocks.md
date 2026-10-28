# Doors and locks

- **Openable**: opens and closes, a drawer, a hatch. Its `motion` plays to open and back to
  close. Events: `onOpened`, `onClosed`.
- **Door**: an Openable with a lock (`lockState`: Open, Locked or Jammed). Tried while locked,
  it unlocks (staying shut) if one of these holds, checked in order:
  - the player is on its `unlockSide`: a door bolted from the other side. Its front is its local
    +Z;
  - its `unlockFlag` is set: a keypad elsewhere sets it;
  - the player carries its `key`, used up unless `keepKey` is set or the item keeps.

  Otherwise it says its `lockedText` and rattles. `endsGame` makes it the way out; `leadsTo`
  loads another level (see [Saves.md](Saves.md)). An `unlockedPiece` takes the door's place once
  it unlocks (a chained door pried open becomes a plain one). Anything can call `Unlock()`. Events:
  `onUnlocked`, `onRefused`.

The locks below are switches ([SwitchesAndPower.md](SwitchesAndPower.md)). Once solved they
switch on, fire `onSwitchedOn` and set their `flag`: wire either to what they open.

- **Padlock**: takes its key item, and its motion opens the shackle.
- **Keypad**: the right `code` switches it on. Its keys are pressed in the world (parts named
  `*_key_0` to `*_key_9`; `*_key_C` clears, `*_key_E` enters); used anywhere else, it opens the
  Hud's code pad. An entry is checked as soon as it is as long as the code. Event: `onKeyed`.
- **CombinationLock**: number wheels (parts `*_wheel_1`, `*_wheel_2`...). Each use rolls the
  wheel looked at one digit about its `axis`; on when they read `code`.
- **CardReader**: takes a card and keeps it, and plays its swipe.

## Wire a lock to what it opens

The chest in the demo is padlocked: its Stash starts disabled, and the padlock enables it.

1. Uncheck the chest's Stash.
2. On the Padlock's `onSwitchedOn`, add the chest's Stash, `enabled`, checked.

A door works the same way through a flag: set the Keypad's `flag` to `store_door_code` and the
Door's `unlockFlag` to the same. Flags are saved, and other levels can read them.
