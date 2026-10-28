using System.Collections.Generic;
using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// Takes an item to switch on: a fuse into a fuse box, fuel into a generator, a gear onto its
    /// shaft. Without it, it says so. The item is used up unless <see cref="keepItem"/> or the item
    /// keeps; <see cref="fitted"/> shows once it is in.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Fuse Box")]
    public class FuseBox : Switch
    {
        public Item item;
        [Tooltip("The item stays carried: a card swiped, a key turned and taken back")] public bool keepItem;
        [TextArea(1, 3)] public string missingText = "Something is missing.";
        [Tooltip("Shown once the item is in")] public GameObject[] fitted = new GameObject[0];

        public override string Verb => "USE";

        protected override void Awake()
        {
            base.Awake();
            Fit(on && !keepItem);
        }

        protected override void OnInteract(Interactor who, Collider part)
        {
            if (!CheckPower())
                return;
            if (on)  // toggle: the item comes back out
            {
                if (!keepItem && item != null)
                    Run.Add(item.Id);
                Fit(false);
                Flip(false);
                return;
            }
            if (item != null && !Run.Has(item.Id))
            {
                Refuse("item", missingText);
                return;
            }
            if (item != null && !keepItem && !item.keep)
                Run.Remove(item.Id);
            Fit(!keepItem);
            Flip(true);
        }

        protected void Fit(bool value)
        {
            foreach (var f in fitted)
                if (f != null) f.SetActive(value);
        }

        public override void Load(IDictionary<string, object> state)
        {
            base.Load(state);
            Fit(on && !keepItem);
        }
    }
}
