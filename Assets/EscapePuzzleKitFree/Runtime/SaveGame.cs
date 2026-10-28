using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// Three save slots, each a JSON file in the player's data folder. A save holds the run and the
    /// world: each saved object's state (every ISaved component on it: doors, switches, pickups,
    /// beats) under its object key, "&lt;level id&gt;:&lt;path&gt;", the level id being the scene's path.
    /// Changing level files the level left first, so coming back finds it as left. Renaming or
    /// moving an object loses its saved state, and it starts as built.
    ///     SaveGame.Save(0);   SaveGame.Load(0);   SaveGame.ChangeLevel("Cellar", "FromKitchen");
    /// </summary>
    public static class SaveGame
    {
        public const int Slots = 3;
        public const int Version = 1;

        /// <summary>Where the slots are written; a folder in Application.persistentDataPath.</summary>
        public static string Folder = "";

        /// <summary>True while saved states are being given back: nothing should fire its events.</summary>
        public static bool Loading { get; private set; }

        public static event Action<int> Saved, Loaded;

        static Dictionary<string, object> placeAt;  // the player as saved, for the level being loaded
        static string entranceAt;

        static string Dir => Folder != "" ? Folder : Path.Combine(Application.persistentDataPath, "saves");

        public static string FileOf(int slot) => Path.Combine(Dir, $"slot_{slot + 1}.json");

        public static bool Exists(int slot) => File.Exists(FileOf(slot));

        /// <summary>The level id: the scene's path, which is unique.</summary>
        public static string LevelOf(Scene scene) => scene.path != "" ? scene.path : scene.name;

        public static string Level => LevelOf(SceneManager.GetActiveScene());

        /// <summary>The object's path below its scene's root: "Doors/OfficeDoor".</summary>
        public static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;

        public static string KeyOf(GameObject go) => LevelOf(go.scene) + ":" + PathOf(go.transform);

        /// <summary>A slot's file as written, or null where it is empty or unreadable: for a slots panel's room, time and date.</summary>
        public static IDictionary<string, object> Peek(int slot)
        {
            try
            {
                return Exists(slot) ? Json.Read(File.ReadAllText(FileOf(slot))) as IDictionary<string, object> : null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Escape Puzzle Kit Free: slot {slot + 1} can't be read: {e.Message}");
                return null;
            }
        }

        public static void Save(int slot)
        {
            FileLevel(SceneManager.GetActiveScene());
            Run.Saves++;
            var save = new Dictionary<string, object>
            {
                ["version"] = Version,
                ["level"] = Level,
                ["room"] = Run.Room,
                ["objective"] = Run.Objective,
                ["time"] = Run.PlayTime,
                ["saves"] = Run.Saves,
                ["date"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                ["health"] = Run.Health,
                ["battery"] = Run.Battery,
                ["items"] = Run.Items.ToDictionary(kv => kv.Key, kv => (object)kv.Value),
                ["equipped"] = Run.Equipped,
                ["loaded"] = Run.Loaded.ToDictionary(kv => kv.Key, kv => (object)kv.Value),
                ["flags"] = Run.Flags.ToDictionary(f => f, f => (object)true),
                ["player"] = Player(),
                ["world"] = new Dictionary<string, object>(Run.World),
            };
            Directory.CreateDirectory(Dir);
            var file = FileOf(slot);
            File.WriteAllText(file + ".tmp", Json.Write(save));
            if (File.Exists(file))
                File.Delete(file);
            File.Move(file + ".tmp", file);
            Saved?.Invoke(slot);
        }

        /// <summary>Restores the slot's run and loads its level, each object given its saved state; false if the slot is empty or unreadable.</summary>
        public static bool Load(int slot)
        {
            var save = Peek(slot);
            if (save == null)
                return false;
            Run.Load(save);
            placeAt = save.TryGetValue("player", out var p) ? p as Dictionary<string, object> : null;
            entranceAt = null;
            var level = Json.Get(save, "level", Level);
            LoadLevel(level);
            Loaded?.Invoke(slot);
            return true;
        }

        public static void Delete(int slot)
        {
            if (Exists(slot))
                File.Delete(FileOf(slot));
        }

        /// <summary>Files this level into the world and loads another, the player arriving at its Entrance of that id (or its start).</summary>
        public static void ChangeLevel(string level, string entrance = "")
        {
            FileLevel(SceneManager.GetActiveScene());
            placeAt = null;
            entranceAt = entrance ?? "";
            LoadLevel(level);
        }

        /// <summary>Back to the start: a new run, the level loaded as built.</summary>
        public static void Restart(string level = null)
        {
            Run.Reset();
            placeAt = null;
            entranceAt = null;
            LoadLevel(level ?? Level);
        }

        /// <summary>Every saved object of the scene into the run's world, by its key.</summary>
        public static void FileLevel(Scene scene)
        {
            foreach (var (go, saved) in SavedIn(scene))
            {
                var state = new Dictionary<string, object>();
                foreach (var s in saved)
                    s.Save(state);
                Run.World[KeyOf(go)] = state;
            }
        }

        /// <summary>Gives each saved object of the scene its state from the run's world; one without any stays as built.</summary>
        public static void Apply(Scene scene)
        {
            Loading = true;
            try
            {
                foreach (var (go, saved) in SavedIn(scene))
                    if (Run.World.TryGetValue(KeyOf(go), out var s) && s is IDictionary<string, object> state)
                        foreach (var c in saved)
                            c.Load(state);
            }
            finally
            {
                Loading = false;
            }
        }

        /// <summary>Loads a level by its scene's name or path. In the Editor a scene not in the Build Profiles loads
        /// from its path all the same, so a demo plays, restarts and loads where it is opened.</summary>
        static void LoadLevel(string level)
        {
#if UNITY_EDITOR
            if (level.EndsWith(".unity") && SceneUtility.GetBuildIndexByScenePath(level) < 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(level, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            SceneManager.LoadScene(level);
        }

        static IEnumerable<(GameObject, List<ISaved>)> SavedIn(Scene scene) =>
#if UNITY_6000_6_OR_NEWER  // the sort mode is obsolete from 6.6, the overload without it new
            UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
#else
            UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
#endif
                .Where(m => m is ISaved && m.gameObject.scene == scene)
                .GroupBy(m => m.gameObject)
                .Select(g => (g.Key, g.Cast<ISaved>().ToList()))
                .ToList();

        static Dictionary<string, object> Player()
        {
            var who = Interactor.Current;
            if (who == null)
                return new Dictionary<string, object>();
            var p = who.Body.position;
            return new Dictionary<string, object>
            {
                ["position"] = new List<object> { p.x, p.y, p.z },
                ["yaw"] = who.Body.eulerAngles.y,
                ["pitch"] = Pitch(who),
                ["flashlight"] = Flashlight.Current != null && Flashlight.Current.On,
            };
        }

        static float Pitch(Interactor who)
        {
            var x = who.transform.eulerAngles.x;
            return x > 180f ? x - 360f : x;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Apply(scene);
            var who = Interactor.Current;
            if (who == null)
                return;
            if (placeAt != null)
            {
                var p = Json.Get<List<object>>(placeAt, "position", null);
                if (p != null && p.Count == 3)
                    Place(who, new Vector3(Convert.ToSingle(p[0]), Convert.ToSingle(p[1]), Convert.ToSingle(p[2])),
                          Json.Get(placeAt, "yaw", 0f), Json.Get(placeAt, "pitch", 0f));
                if (Flashlight.Current != null)
                    Flashlight.Current.On = Json.Get(placeAt, "flashlight", false);
            }
            else if (!string.IsNullOrEmpty(entranceAt))
            {
#if UNITY_6000_6_OR_NEWER
                var at = UnityEngine.Object.FindObjectsByType<Entrance>().FirstOrDefault(e => e.Id == entranceAt);
#else
                var at = UnityEngine.Object.FindObjectsByType<Entrance>(FindObjectsSortMode.None).FirstOrDefault(e => e.Id == entranceAt);
#endif
                if (at != null)
                    Place(who, at.transform.position, at.transform.eulerAngles.y, 0f);
            }
            placeAt = null;
            entranceAt = null;
        }

        /// <summary>Puts the player there, facing yaw and looking at pitch: through its body's IPlayerBody if it has one, else moved directly.</summary>
        public static void Place(Interactor who, Vector3 position, float yaw, float pitch)
        {
            var body = who.Body;
            var mover = body.GetComponent<IPlayerBody>();
            if (mover != null)
            {
                mover.Place(position, yaw, pitch);
                return;
            }
            var cc = body.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            body.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            if (who.transform != body)
                who.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            if (cc != null) cc.enabled = true;
            Physics.SyncTransforms();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            placeAt = null;
            entranceAt = null;
            Loading = false;
            Saved = Loaded = null;
        }
    }

    /// <summary>A controller that places itself: what a save or a level change calls instead of moving its transform.</summary>
    public interface IPlayerBody
    {
        void Place(Vector3 position, float yaw, float pitch);
    }
}
