# Saves

Three slots, each a JSON file in the player's data folder (`SaveGame.Folder` sets another). A
save holds:

- the run: items, flags, health, battery, the objective, the play time, the weapon equipped and
  each gun's rounds loaded;
- the player's place and view;
- every saved object in every level visited: each door's lock and whether it is open, each
  switch, each pickup taken, each beat fired.

The parts:

- **SavePoint**: opens the save slots, a typewriter. With `needsItem`, each save uses one: an
  ink ribbon.
- **The pause menu** (Esc) offers Load and Restart.
- **Entrance**: where a door from another level arrives. A Door with `leadsTo` (the scene's name
  or path, in the Build Profiles) and an `entrance` loads that level and places the player
  there. Coming back finds the level as it was left.

```csharp
SaveGame.Save(0);  SaveGame.Load(0);  SaveGame.ChangeLevel("Cellar", "FromKitchen");
SaveGame.Saved += slot => Debug.Log($"saved to {slot}");
```

An object's state is filed under its scene and its path in the hierarchy (`Level/OfficeDoor`).
Renaming or moving an object after a save loses its state, and it starts as built. A component
of your own can be saved too: implement `ISaved`, whose `Save` writes into a dictionary and
`Load` reads it back.
