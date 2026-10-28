using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// The player's reach: on the camera of any first-person controller. Each frame it looks along
    /// the view's centre for an <see cref="Interactable"/> within <see cref="reach"/>, and interact
    /// uses the one it rests on. While a panel is open (the inventory, a code pad, the slots) it
    /// rests on nothing and <see cref="Paused"/> tells the controller to stand still and free the
    /// cursor. Its body (the controller it is under) is what is saved and placed.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Interactor")]
    [DisallowMultipleComponent]
    public class Interactor : MonoBehaviour
    {
        [Serializable] public class FocusEvent : UnityEvent<Interactable> { }

        [Tooltip("Metres from the view")] public float reach = 2f;
        [Tooltip("What the look can hit; walls block it")] public LayerMask layers = Physics.DefaultRaycastLayers;
        [Tooltip("Read E itself; off, call Interact() from your own input")] public bool readKeys = true;
        [Tooltip("The player's root: what is saved and placed. Empty: the CharacterController or Rigidbody above, else the topmost parent")]
        public Transform body;
        [Tooltip("The look rests on another Interactable, or on none (null)")] public FocusEvent onFocusChanged = new FocusEvent();

        /// <summary>The player's Interactor: the last one enabled.</summary>
        public static Interactor Current { get; private set; }

        /// <summary>Whether a panel holds the player: a controller stands still and frees the cursor.</summary>
        public static bool Paused => holds.Count > 0;

        public static event Action<bool> PausedChanged;

        public Interactable Focus { get; private set; }
        /// <summary>The collider the look is on: a keypad's key, a panel's switch.</summary>
        public Collider FocusPart { get; private set; }
        public Transform Body => body != null ? body : body = FindBody();

        static readonly HashSet<object> holds = new HashSet<object>();
        static int freed = -1;  // the frame the last hold let go

        /// <summary>Holds the player while <paramref name="by"/> is open; let go with on false.</summary>
        public static void Hold(object by, bool on)
        {
            var was = Paused;
            if (on) holds.Add(by); else holds.Remove(by);
            if (Paused == was)
                return;
            if (!Paused)
                freed = Time.frameCount;
            PausedChanged?.Invoke(Paused);
        }

        /// <summary>Uses what the look rests on, as E does.</summary>
        public void Interact()
        {
            if (Focus != null && !Paused && Time.frameCount != freed)  // E closing a panel doesn't also use
                Focus.Interact(this, FocusPart);
        }

        /// <summary>Where the player is looking from and along.</summary>
        public Ray View
        {
            get
            {
                var cam = GetComponent<Camera>();
                return cam != null ? cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)) : new Ray(transform.position, transform.forward);
            }
        }

        void OnEnable() => Current = this;

        void OnDisable()
        {
            if (Current == this)
                Current = null;
            SetFocus(null, null);
        }

        void Update()
        {
            if (Current == this)
                Run.PlayTime += Time.unscaledDeltaTime;
            Look();
            if (readKeys && Controls.Interact)
                Interact();
        }

        /// <summary>Finds what the view rests on now; Update does it each frame.</summary>
        public void Look()
        {
            Interactable found = null;
            Collider part = null;
            if (!Paused && Nearest(out var hit))
            {
                foreach (var i in hit.collider.GetComponentsInParent<Interactable>())  // the nearest that can be used: a shut safe's keypad, not its door
                    if (i.CanFocus)
                    {
                        found = i;
                        part = hit.collider;
                        break;
                    }
            }
            SetFocus(found, part);
        }

        readonly RaycastHit[] hits = new RaycastHit[16];

        /// <summary>The nearest hit along the view that isn't the player's own body: looking down, the view
        /// starts inside or above the controller's capsule.</summary>
        bool Nearest(out RaycastHit nearest)
        {
            nearest = default;
            var n = Physics.RaycastNonAlloc(View, hits, reach, layers, QueryTriggerInteraction.Ignore);
            var found = false;
            for (var i = 0; i < n; i++)
                if (!hits[i].collider.transform.IsChildOf(Body) && (!found || hits[i].distance < nearest.distance))
                {
                    nearest = hits[i];
                    found = true;
                }
            return found;
        }

        void SetFocus(Interactable found, Collider part)
        {
            FocusPart = part;
            if (found == Focus)
                return;
            Focus = found;
            onFocusChanged.Invoke(found);
        }

        Transform FindBody()
        {
            var t = transform;
            for (var p = transform; p != null; p = p.parent)
            {
                if (p.GetComponent<CharacterController>() != null || p.GetComponent<Rigidbody>() != null)
                    return p;
                t = p;
            }
            return t;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Fresh()
        {
            holds.Clear();
            PausedChanged = null;
            Current = null;
        }
    }
}
