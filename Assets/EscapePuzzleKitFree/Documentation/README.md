# Escape Puzzle Kit Free

The free sampler of Escape Puzzle Kit: its interaction, items and inventory, and doors and locks
systems for first-person games in Unity 6, Built-in or URP, with the story beats and saves they
use. Each system is a component, wired to the others in the Inspector with UnityEvents. They work
with any first-person controller and need no other package.

## Start here

1. Open `Demo/Storeroom.unity` and press Play. You wake in a dark storeroom: find the way out
   (WASD, mouse, E to use, F the flashlight, Tab the inventory, Esc the pause menu).
2. Open `Demo/Demo.unity` for the props, each working, side by side.
3. Drop any prefab from `Prefabs/` into your scene. Each works as it is: a door opens, a keypad
   takes its code, a drawer gives what it holds.

In your own scene, put an `Interactor` on your first-person camera (Add Component > Escape Puzzle
Kit Free > Interactor) and add an empty GameObject with a `Hud`, with your Items on its catalogue.
[Interaction.md](Interaction.md) has the rest.

## What is in it

| System | Components | Read |
|---|---|---|
| Interaction | Interactor, Interactable, Controls | [Interaction.md](Interaction.md) |
| Items and inventory | Item, Pickup, Stash, Inspectable, Hud | [Items.md](Items.md) |
| Doors and locks | Openable, Door, Padlock, Keypad | [DoorsAndLocks.md](DoorsAndLocks.md) |
| Story and saves, which they use | StoryBeat, Room, SaveGame, SavePoint, Entrance | [Story.md](Story.md), [Saves.md](Saves.md) |

The pages are the full kit's: where they name CombinationLock, CardReader, Lever, PushButton,
SwitchPanel, Valve, Gauge or LogicGate, those come with the full kit. The menus here read "Escape
Puzzle Kit Free", and the scripts are in `HeyHeyThere.EscapePuzzleKitFree`, so the sampler and the
full kit can share a project.

## Sounds

The props, the Hud and the flashlight come with their sounds, from PSX Horror SFX by
heyheythere, in `Sounds/` (see `Third-Party Notices.txt`).
Each is a field on its
component (a Door's `openSound`, a Keypad's `keySound`): put your own in, or clear it.

## The full kit

Escape Puzzle Kit adds switches and power (levers, buttons, breaker panels, valves, fuse boxes,
gauges, logic gates), combination locks and card readers, five close-up minigames (lockpicking, a
combination safe, wiring, pipe turning and a terminal hack), working props in two looks (low poly
and PSX), and two escape levels: Night Shift, of ten steps, and Pump Station, escaped in five games.

## Support

spyridwn.pd@gmail.com. Say which Unity version and pipeline you are on.

Changes from version to version: [CHANGELOG.md](CHANGELOG.md).
