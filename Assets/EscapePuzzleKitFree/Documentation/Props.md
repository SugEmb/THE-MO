# Props

Each prefab in `Prefabs/` is a model with its components set up: drop it in and it works. The
moving pieces are the model's named child parts, and how they move is a `Motion` on the
component.

## Motion

- **Clip**: one of the prop's own clips on its Legacy `Animation` (`open`, `pull`, `turn`). The
  kit samples clips itself, so each keeps its own time: a generator's lever stays thrown while
  its pulleys run.
- **Turn**: a `part` turned about its local `axis` by `angle` degrees, over `seconds`.
- **Slide**: a `part` moved by a local `offset`.
- **None**: nothing moves; the logic still works.

## Parts

Props with many controls find them by name: a keypad's keys `*_key_1` to `*_key_C`, a breaker
panel's `*_key_1`..., a lock's `*_wheel_1`.... Each part needs its own collider, so the look can
pick it, and no box of the body around it: keys set into a face need a Mesh Collider on the body,
as the kit's safe and PSX keypad have, and so does a desk with things on it or a vent with
something inside. A lamp named `*_lamp_<key>` lights with its key; `*_lamp` and `*_lamp_idle`
show a switch on and off.

## Swap a mesh

- **Keep the kit's parts:** give your model's parts the kit's names and put it in the prefab in
  place of the kit's model.
- **A model of your own:** add the component (a Door, say) to its root, set the motion to Turn,
  its `part` to the leaf, `axis` (0, 1, 0) and `angle` 100. Give it a collider, and it is a door.

## The look

The low poly models share one texture atlas (`Textures/atlas.png`, and `atlas_glow.png` for lit
lamps); each PSX look has its own, in `Textures/<look>/`. All draw with the kit's shader,
`EscapePuzzleKitFree/Stylized Lit`, on Built-in and URP alike.
