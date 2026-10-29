using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// Opens and closes: a door, a drawer, a locker, a hatch. Its <see cref="motion"/> plays to open
    /// and back to close; a load sets it at once, silently.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Openable")]
    public class Openable : Interactable
    {
        [Tooltip("Open now; as built, before any save")] [SerializeField] protected bool open;
        [Tooltip("How it opens: a clip of the prop's, or a part turned or slid")] public Motion motion = new Motion();
        public AudioClip openSound, closeSound;
        public UnityEvent onOpened = new UnityEvent(), onClosed = new UnityEvent();

        public bool IsOpen => open;
        public override string Verb => open ? "CLOSE" : "OPEN";

        protected virtual void Awake()
        {
            motion.Bind(this);
            motion.Set(open ? 1f : 0f);
        }

        protected virtual void Update() => motion.Tick(Time.deltaTime);

        protected override void OnInteract(Interactor who, Collider part)
        {
            if (open) Close(); else Open();
        }

        public virtual void Open()
        {
            if (open)
                return;
            open = true;
            motion.Play(1f);
            Sound.Play(this, openSound);
            onOpened.Invoke();
        }

        public virtual void Close()
        {
            if (!open)
                return;
            open = false;
            motion.Play(0f);
            Sound.Play(this, closeSound);
            onClosed.Invoke();
        }

        public override void Save(Dictionary<string, object> state)
        {
            base.Save(state);
            state["open"] = open;
        }

        public override void Load(IDictionary<string, object> state)
        {
            base.Load(state);
            open = Json.Get(state, "open", open);
            motion.Set(open ? 1f : 0f);
        }
    }
}
