using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// A step of the story: a line said, an objective set, a sound, lights put out, a hint. Fires as
    /// the player enters its trigger, as the level starts, or when fired (by a pickup, a stash, an
    /// inspectable, or any UnityEvent). With <see cref="needsFlag"/> it waits for it.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Story Beat")]
    /// <summary>Anything a story beat's wakes can wake: an enemy asleep until then, say.</summary>
    public interface IWakeable
    {
        void Wake();
    }

    public class StoryBeat : MonoBehaviour, ISaved
    {
        public enum When { Fired, OnEnter, OnStart }

        public When when = When.OnEnter;
        [Tooltip("Fires once only")] public bool once = true;
        [Tooltip("Fires only once this flag is set")] public string needsFlag = "";
        public string setsFlag = "";
        [TextArea(1, 3)] public string subtitle = "";
        public string speaker = "";
        public string objective = "";
        public AudioClip sound;
        [Tooltip("Put out, and kept out once done")] public Light[] lightsOff = new Light[0];
        [Tooltip("Woken: every IWakeable on these (your enemies, say)")] public GameObject[] wakes = new GameObject[0];
        [Range(0f, 1f)] public float damage;
        [Tooltip("A hint: the action's key (F) and what it does (FLASHLIGHT)")] public string hintKey = "", hint = "";
        public UnityEvent onFired = new UnityEvent();

        public bool Done { get; private set; }

        void Start()
        {
            if (when == When.OnStart)
                Fire();
        }

        void OnTriggerEnter(Collider other)
        {
            var player = Interactor.Current;
            if (when == When.OnEnter && player != null && (other.transform == player.Body || other.transform.IsChildOf(player.Body)))
                Fire();
        }

        /// <summary>Fires it, unless done (once) or waiting on its flag.</summary>
        public void Fire()
        {
            if (!isActiveAndEnabled || once && Done || needsFlag != "" && !Run.HasFlag(needsFlag))
                return;
            Done = true;
            Run.SetFlag(setsFlag);
            Run.Say(subtitle, speaker);
            if (objective != "")
                Run.Objective = objective;
            if (sound != null)
                AudioSource.PlayClipAtPoint(sound, transform.position);
            LightsOut();
            foreach (var go in wakes)
                if (go != null)
                    foreach (var w in go.GetComponents<IWakeable>())
                        w.Wake();
            if (damage > 0f)
                Run.Health -= damage;
            Run.Hint(hintKey, hint);
            onFired.Invoke();
        }

        void LightsOut()
        {
            foreach (var l in lightsOff)
                if (l != null) l.enabled = false;
        }

        public void Save(Dictionary<string, object> state) => state["done"] = Done;

        public void Load(IDictionary<string, object> state)
        {
            Done = Json.Get(state, "done", Done);
            if (Done)
                LightsOut();
        }
    }
}
