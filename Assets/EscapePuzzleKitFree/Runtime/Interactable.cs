using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>What <see cref="SaveGame"/> files under an object's key: each saved component on it writes into one state.</summary>
    public interface ISaved
    {
        void Save(Dictionary<string, object> state);
        void Load(IDictionary<string, object> state);
    }

    /// <summary>
    /// Anything the player uses by looking at it within reach and pressing interact (an
    /// <see cref="Interactor"/> on their camera). Its colliders, or its children's, are what the
    /// look finds. The component's enabled is the spec's: off, it can't be focused or used.
    /// Subclass it for your own: override <see cref="Verb"/> and <see cref="OnInteract"/>.
    /// </summary>
    public abstract class Interactable : MonoBehaviour, ISaved
    {
        public enum Icon { Hand, Examine, Door, Key }

        [Tooltip("Named in the prompt after its verb: OPEN Office door. Empty for the verb alone")] public string label = "";
        [Tooltip("The aim point's icon while looked at")] public Icon focusIcon = Icon.Hand;
        [Tooltip("Any use, whatever it did")] public UnityEvent onInteracted = new UnityEvent();

        /// <summary>What the prompt says, in capitals, as it is now: OPEN, UNLOCK, TAKE, PULL...</summary>
        public virtual string Verb => "USE";
        public virtual Icon FocusIcon => focusIcon;
        /// <summary>Whether the look can rest on it now (enabled, and not done).</summary>
        public virtual bool CanFocus => isActiveAndEnabled;

        /// <summary>Uses it: <paramref name="part"/> is the collider looked at (a keypad's key), null for the whole.</summary>
        public void Interact(Interactor who, Collider part = null)
        {
            if (!CanFocus)
                return;
            OnInteract(who, part);
            onInteracted.Invoke();
        }

        protected abstract void OnInteract(Interactor who, Collider part);

        public virtual void Save(Dictionary<string, object> state) => state["enabled"] = enabled;

        public virtual void Load(IDictionary<string, object> state) => enabled = Json.Get(state, "enabled", enabled);

        protected static void Say(string text) => Run.Say(text);
    }
}
