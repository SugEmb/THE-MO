# Items and inventory

- **Item**, an asset (Assets > Create > Escape Puzzle Kit Free > Item): an `id` (snake_case, unique
  in the project: `office_key`), a title, a description, an icon and a `kind`. A Key opens
  things; Heal and Battery restore by `amount` when used from the inventory; a Document has
  `text` to read, a line of `---` starting a new page. `keep` leaves a key carried after it is
  used. `model` is what the examine view holds up. The kit has no weapons, but a Weapon item is
  equipped from the inventory (`Run.Equipped`, `Run.EquippedChanged`) and an Ammo item left to
  your gun, whose rounds `Run.Loaded` keeps.
- **Pickup**: something to take. It adds its item and hides itself. With `examine` it is held
  up to look at as it is taken: a diary read.
- **Stash**: an Openable that holds an item, a drawer or a locker. Its first opening gives it.
- **Inspectable**: looked at, not taken. It says its `text`, or with `hold` shows the object in
  the examine view with the text under it.
- **Hud**: Tab opens the inventory, each item with its icon; select one to examine, use or read
  it. Every item a save may hold must be on the Hud's `catalogue`. Its `ExamineView` first shows a
  model turned by `startYaw`: 200 shows a model facing +z (Unity's forward) from the front, 20 one
  facing -z.

What is carried lives in `Run`, across scenes:

```csharp
Run.Add("office_key");  Run.Has("office_key");  Run.Remove("office_key");
Run.ItemsChanged += Refresh;
```

## Add an item

1. Assets > Create > Escape Puzzle Kit Free > Item. Set the id, title, description and icon.
2. Add it to the Hud's `catalogue`.
3. Put it on a Pickup (or a Stash) where it is found, and on the Door, Padlock or FuseBox that
   takes it.

The icons in `Icons/` are renders of the props; any sprite will do.
