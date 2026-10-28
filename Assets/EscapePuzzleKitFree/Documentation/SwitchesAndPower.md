# Switches and power

A **switch** is used its own way until it switches on. Then it sets its `flag`, fires
`onSwitchedOn`, holds its on pose and can't be used again, unless it has `toggle`: then the
next use switches it back off (`onSwitchedOff`; the flag stays set).

| Switch | Used | On when |
|---|---|---|
| Lever | pulled | at once |
| PushButton | pressed, playing through and back | at once |
| SwitchPanel | each part `*_key_1`... flipped up or down | they read `code` ("1" up, "0" down) |
| Valve | turned a step a use | all its `steps` are turned |
| FuseBox | with its `item`: a fuse, fuel, a crank handle | the item is in; `fitted` shows |
| Keypad, CombinationLock, Padlock, CardReader | see [DoorsAndLocks.md](DoorsAndLocks.md) | solved |

## Power

Every switch and Powered has a `power` list, `needs` (a LogicGate's is `inputs`): drag in the
things it runs on. It is live while every one of them is. Until then a switch refuses with its
`noPowerText`, and a Powered machine stands still.

- **Powered**: a machine that runs while its power is live: a lamp, a beacon, a roller shutter.
  Its `motion` plays to its running pose, its `loop` clip runs, and its `lit` renderers and
  `lights` come on. Events: `onStarted`, `onStopped`, `onLive`.
- **Gauge**: shows its source's output, 0 to 1, as its motion's pose: a needle, or a gate that
  winds up as a Valve is turned.
- **LogicGate**: power from logic, with no model: All, Any, None, One, or Flag. `setsFlag`
  marks the moment it goes live. Events: `onTrue`, `onFalse`.

A chain from the demo:

- The fuse box (a FuseBox) powers the breakers (a SwitchPanel), and they power the lamps and
  the keypad.
- The generator (a Lever) needs the fuel tank (a FuseBox), and powers the card reader.
- A LogicGate (All of the generator and the reader) sets `exit_unlocked`, which unlocks the way
  out.

From code: `lever.SwitchOn()`, `lever.SwitchOff()`, `lever.IsOn`, `lever.Live`, and the
`Changed` event.
