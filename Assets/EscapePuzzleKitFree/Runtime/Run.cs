using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// The run: what is carried, the flags set, health and battery, the objective and the room, and
    /// what each saved object has become (<see cref="World"/>, by object key). It lives across scene
    /// loads; <see cref="SaveGame"/> writes it to a slot and reads it back. Nothing here needs a scene.
    ///     Run.Add("office_key");          Run.Has("office_key");
    ///     Run.SetFlag("gym_power_on");    Run.HasFlag("gym_power_on");
    /// </summary>
    public static class Run
    {
        static readonly List<string> order = new List<string>();  // item ids, in the order first taken
        static readonly Dictionary<string, int> counts = new Dictionary<string, int>();
        static readonly HashSet<string> flags = new HashSet<string>();
        static float health = 1f, battery = 1f;
        static string objective = "", room = "", equipped = "";

        /// <summary>Each saved object's state by its object key, "&lt;level id&gt;:&lt;path&gt;", every level visited.</summary>
        public static readonly Dictionary<string, object> World = new Dictionary<string, object>();
        /// <summary>A gun's rounds in its magazine, by item id: yours to keep, saved as "loaded".</summary>
        public static readonly Dictionary<string, int> Loaded = new Dictionary<string, int>();
        public static float PlayTime;
        public static int Saves;

        public static event Action ItemsChanged;
        /// <summary>The weapon in hand changed: its item id, or empty.</summary>
        public static event Action<string> EquippedChanged;
        public static event Action<string> FlagSet;
        public static event Action<float> HealthChanged, BatteryChanged;
        public static event Action<string> ObjectiveChanged;
        /// <summary>A line to show: its text and speaker (empty for none).</summary>
        public static event Action<string, string> Said;
        /// <summary>A hint to show: an action's key and what it does ("F", "FLASHLIGHT").</summary>
        public static event Action<string, string> Hinted;
        /// <summary>The way out was taken: the run is over.</summary>
        public static event Action Escaped;

        public static bool HasEscaped { get; private set; }

        /// <summary>Back to a new run: nothing carried, no flags, full health and battery, no saved world.</summary>
        public static void Reset()
        {
            order.Clear();
            counts.Clear();
            flags.Clear();
            World.Clear();
            Loaded.Clear();
            health = battery = 1f;
            objective = room = equipped = "";
            PlayTime = 0f;
            Saves = 0;
            HasEscaped = false;
            ItemsChanged?.Invoke();
        }

        // --- items ---------------------------------------------------------------------------------

        /// <summary>What is carried, item id to count, in the order first taken.</summary>
        public static IEnumerable<KeyValuePair<string, int>> Items => order.Select(id => new KeyValuePair<string, int>(id, counts[id]));

        public static bool Has(string id, int count = 1) => !string.IsNullOrEmpty(id) && counts.TryGetValue(id, out var n) && n >= count;

        public static int Count(string id) => id != null && counts.TryGetValue(id, out var n) ? n : 0;

        public static void Add(string id, int count = 1)
        {
            if (string.IsNullOrEmpty(id) || count <= 0)
                return;
            if (!counts.ContainsKey(id))
            {
                order.Add(id);
                counts[id] = 0;
            }
            counts[id] += count;
            ItemsChanged?.Invoke();
        }

        /// <summary>Takes count of the item away; false, taking nothing, when fewer are carried.</summary>
        public static bool Remove(string id, int count = 1)
        {
            if (!Has(id, count))
                return false;
            counts[id] -= count;
            if (counts[id] == 0)
            {
                counts.Remove(id);
                order.Remove(id);
                if (equipped == id)
                    Equipped = "";
            }
            ItemsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Uses a carried item as the inventory does: a heal restores health and a battery the
        /// flashlight's, failing when already full; used up unless it keeps. A weapon is taken in hand,
        /// or put away. Keys, documents and ammo are used by what they open, by reading and by your
        /// gun, so they fail here.
        /// </summary>
        public static bool Use(string id)
        {
            var item = Item.Find(id);
            if (item == null || !Has(id))
                return false;
            switch (item.kind)
            {
                case Item.Kind.Heal when Health < 1f:
                    Health += item.amount;
                    break;
                case Item.Kind.Battery when Battery < 1f:
                    Battery += item.amount;
                    break;
                case Item.Kind.Weapon:
                    Equipped = Equipped == id ? "" : id;
                    return true;
                default:
                    return false;
            }
            if (!item.keep)
                Remove(id);
            return true;
        }

        // --- flags ---------------------------------------------------------------------------------

        /// <summary>The flags set, which are only ever set, never cleared.</summary>
        public static IEnumerable<string> Flags => flags;

        public static bool HasFlag(string flag) => !string.IsNullOrEmpty(flag) && flags.Contains(flag);

        public static void SetFlag(string flag)
        {
            if (string.IsNullOrEmpty(flag) || !flags.Add(flag))
                return;
            FlagSet?.Invoke(flag);
        }

        // --- the rest ------------------------------------------------------------------------------

        public static float Health
        {
            get => health;
            set { health = Mathf.Clamp01(value); HealthChanged?.Invoke(health); }
        }

        public static float Battery
        {
            get => battery;
            set { battery = Mathf.Clamp01(value); BatteryChanged?.Invoke(battery); }
        }

        public static string Objective
        {
            get => objective;
            set { objective = value ?? ""; ObjectiveChanged?.Invoke(objective); }
        }

        /// <summary>Where the player is, named by the last <see cref="Room"/> entered.</summary>
        public static string Room
        {
            get => room;
            set => room = value ?? "";
        }

        /// <summary>The weapon item in hand, or empty: the kit has no weapons, but keeps yours in its saves.</summary>
        public static string Equipped
        {
            get => equipped;
            set
            {
                value ??= "";
                if (value == equipped || value != "" && !Has(value))
                    return;
                equipped = value;
                EquippedChanged?.Invoke(value);
            }
        }

        public static void Say(string text, string speaker = "")
        {
            if (!string.IsNullOrEmpty(text))
                Said?.Invoke(text, speaker ?? "");
        }

        public static void Hint(string key, string verb)
        {
            if (!string.IsNullOrEmpty(verb))
                Hinted?.Invoke(key ?? "", verb);
        }

        /// <summary>Ends the run, escaped: a door with ends game opening, or your own way out.</summary>
        public static void Escape()
        {
            if (HasEscaped)
                return;
            HasEscaped = true;
            Escaped?.Invoke();
        }

        internal static void Load(IDictionary<string, object> save)
        {
            order.Clear();
            counts.Clear();
            flags.Clear();
            World.Clear();
            Loaded.Clear();
            if (save.TryGetValue("items", out var items) && items is IDictionary<string, object> carried)
                foreach (var kv in carried)
                {
                    order.Add(kv.Key);
                    counts[kv.Key] = Convert.ToInt32(kv.Value);
                }
            if (save.TryGetValue("flags", out var f) && f is IDictionary<string, object> set)
                foreach (var kv in set)
                    flags.Add(kv.Key);
            if (save.TryGetValue("loaded", out var l) && l is IDictionary<string, object> rounds)
                foreach (var kv in rounds)
                    Loaded[kv.Key] = Convert.ToInt32(kv.Value);
            if (save.TryGetValue("world", out var w) && w is IDictionary<string, object> world)
                foreach (var kv in world)
                    World[kv.Key] = kv.Value;
            health = Json.Get(save, "health", 1f);
            battery = Json.Get(save, "battery", 1f);
            objective = Json.Get(save, "objective", "");
            room = Json.Get(save, "room", "");
            equipped = Json.Get(save, "equipped", "");
            PlayTime = Json.Get(save, "time", 0f);
            Saves = Json.Get(save, "saves", 0);
            HasEscaped = false;
            ItemsChanged?.Invoke();
            HealthChanged?.Invoke(health);
            BatteryChanged?.Invoke(battery);
            ObjectiveChanged?.Invoke(objective);
            EquippedChanged?.Invoke(equipped);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Fresh()  // a new run each play, with domain reload off too
        {
            Reset();
            ItemsChanged = null;
            EquippedChanged = null;
            FlagSet = null;
            HealthChanged = BatteryChanged = null;
            ObjectiveChanged = null;
            Said = null;
            Hinted = null;
            Escaped = null;
        }
    }
}
