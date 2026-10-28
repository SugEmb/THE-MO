using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// Opens the save slots. With <see cref="needsItem"/> (a tape, an ink ribbon) each save uses one.
    /// Without a slots panel it saves straight to the first slot.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Save Point")]
    public class SavePoint : Interactable
    {
        public Item needsItem;
        [TextArea(1, 3)] public string missingText = "You need something to save with.";
        [TextArea(1, 3)] public string savedText = "Saved.";

        public override string Verb => "SAVE";

        protected override void OnInteract(Interactor who, Collider part)
        {
            if (needsItem != null && !Run.Has(needsItem.Id))
                Say(missingText);
            else if (!Panels.OpenSlots(this))
                SaveTo(0);
        }

        /// <summary>Saves to a slot, using up one of its item; whether it did.</summary>
        public bool SaveTo(int slot)
        {
            if (needsItem != null && !Run.Remove(needsItem.Id))
            {
                Say(missingText);
                return false;
            }
            SaveGame.Save(slot);
            Say(savedText);
            return true;
        }
    }
}
