using System.Collections.Generic;
using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// One kind of thing the player carries, found by its id (snake_case, unique in the project; the
    /// asset's name when left empty). An Item is known once loaded: referenced by anything in a loaded
    /// scene, or listed on the <see cref="Hud"/>'s catalogue, which is how a save's items find theirs.
    /// Create one with Assets > Create > Escape Puzzle Kit Free > Item.
    /// </summary>
    [CreateAssetMenu(menuName = "Escape Puzzle Kit Free/Item", fileName = "new_item")]
    public class Item : ScriptableObject
    {
        public enum Kind { Key, Heal, Battery, Document, Other, Weapon, Ammo }

        [Tooltip("snake_case, unique in the project; the asset's name when empty")]
        [SerializeField] string id = "";
        public string title = "";
        [TextArea(2, 4)] public string description = "";
        public Sprite icon;
        public Kind kind = Kind.Other;
        [Tooltip("Health or battery restored, 0 to 1")] [Range(0f, 1f)] public float amount;
        [Tooltip("A document's text; a line of --- starts a page")] [TextArea(4, 12)] public string text = "";
        [Tooltip("Not used up: a key that opens more than one door")] public bool keep;
        [Tooltip("Held up in the examine view; the item's pickup prop, say")] public GameObject model;

        static readonly Dictionary<string, Item> known = new Dictionary<string, Item>();

        public string Id => string.IsNullOrEmpty(id) ? name : id;

        /// <summary>The item with this id, among those loaded; null if none is.</summary>
        public static Item Find(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            if (known.TryGetValue(id, out var item) && item != null && item.Id == id)
                return item;
            known.Clear();  // renamed or made since: look again at every one loaded
            foreach (var each in Resources.FindObjectsOfTypeAll<Item>())
                known[each.Id] = each;
            return known.TryGetValue(id, out item) ? item : null;
        }

        /// <summary>Its title, or the id made readable where it has none or isn't loaded.</summary>
        public static string Title(string id)
        {
            var item = Find(id);
            return item != null && item.title != "" ? item.title : (id ?? "").Replace('_', ' ').ToUpperInvariant();
        }

        void OnEnable() => known[Id] = this;

        void OnValidate()
        {
            if (!string.IsNullOrEmpty(name))
                known[Id] = this;
        }
    }
}
