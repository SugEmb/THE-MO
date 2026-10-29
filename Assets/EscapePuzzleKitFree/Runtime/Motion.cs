using System;
using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// How a piece moves between its rest (0) and its end (1): one of the kit's clips on the prop's
    /// Legacy Animation, sampled by hand so each clip keeps its own time (a lever stays thrown while
    /// an engine runs), or, for a model of your own, a part turned about an axis or slid by an offset.
    /// Its owner calls <see cref="Tick"/> each frame.
    /// </summary>
    [Serializable]
    public class Motion
    {
        public enum Mode { None, Clip, Turn, Slide }

        public Mode mode = Mode.None;
        [Tooltip("Clip: the clip on the prop's Animation")] public string clip = "";
        [Tooltip("Turn and Slide: the part that moves (a door's leaf, a drawer)")] public Transform part;
        [Tooltip("Turn: about this local axis, through the part's pivot")] public Vector3 axis = Vector3.up;
        [Tooltip("Turn: degrees at the end")] public float angle = 90f;
        [Tooltip("Slide: local offset at the end, metres")] public Vector3 offset = new Vector3(0f, 0f, 0.4f);
        [Tooltip("Turn and Slide: seconds from rest to end")] public float seconds = 1f;

        Animation anim;
        Vector3 restPosition;
        Quaternion restRotation;
        float at, to;
        Action then;
        bool bound, returns;

        /// <summary>Where it is, 0 at rest to 1 at its end.</summary>
        public float Position => at;
        public bool Moving { get; private set; }
        public float Length => mode == Mode.Clip ? (HasClip ? anim[clip].length : 0f) : mode == Mode.None ? 0f : Mathf.Max(0.0001f, seconds);
        bool HasClip => anim != null && !string.IsNullOrEmpty(clip) && anim[clip] != null;

        public static Motion OfClip(string clip) => new Motion { mode = Mode.Clip, clip = clip };

        /// <summary>Finds its Animation (a clip's) or notes its part's rest; the owner's Awake calls it.</summary>
        public void Bind(Component owner)
        {
            anim = Clips(owner);
            returns = mode == Mode.Clip && HasClip && Returns(anim, clip);
            if (part != null && !bound)
            {
                restPosition = part.localPosition;
                restRotation = part.localRotation;
            }
            bound = true;
        }

        /// <summary>At t at once (0 rest, 1 end), anything it was on its way to dropped.</summary>
        public void Set(float t)
        {
            at = to = Mathf.Clamp01(t);
            Moving = false;
            then = null;
            Apply();
        }

        /// <summary>On its way to t at its own pace; then, once there (at once with nothing to move).</summary>
        public void Play(float t, Action then = null)
        {
            to = Mathf.Clamp01(t);
            this.then = then;
            Moving = Length > 0f && !Mathf.Approximately(at, to);
            if (!Moving)
                Arrive();
        }

        /// <summary>From where it is now to the end and back to rest: a button pressed, a key swiped. A clip
        /// that ends where it starts (a key's press and release) plays through once.</summary>
        public void Pulse(Action then = null)
        {
            if (returns)
                Play(1f, () => { Set(0f); then?.Invoke(); });
            else
                Play(1f, () => Play(0f, then));
        }

        /// <summary>Anything moving where it is going, at once.</summary>
        public void Settle()
        {
            if (Moving)
                Arrive();
        }

        /// <summary>Puts its part back where it is: after a loop sampled on the same Animation, whose clip may
        /// hold the part at rest.</summary>
        public void Hold() => Apply();

        public void Tick(float dt)
        {
            if (!Moving)
                return;
            at = Mathf.MoveTowards(at, to, dt / Length);
            Apply();
            if (Mathf.Approximately(at, to))
                Arrive();
        }

        void Arrive()
        {
            at = to;
            Moving = false;
            Apply();
            var next = then;
            then = null;
            next?.Invoke();
        }

        void Apply()
        {
            switch (mode)
            {
                case Mode.Clip:
                    if (HasClip)
                        Sample(anim, clip, at * anim[clip].length);
                    break;
                case Mode.Turn:
                    if (part != null)
                        part.localRotation = restRotation * Quaternion.AngleAxis(angle * at, axis);
                    break;
                case Mode.Slide:
                    if (part != null)
                        part.localPosition = restPosition + offset * at;
                    break;
            }
        }

        /// <summary>The Animation holding a prop's clips: on the owner or under it, else above it (a drawer of a cabinet).</summary>
        public static Animation Clips(Component owner) =>
            owner.GetComponentInChildren<Animation>(true) ?? owner.GetComponentInParent<Animation>(true);

        /// <summary>Whether a clip ends in the pose it starts in; the parts are left as they were.</summary>
        static bool Returns(Animation anim, string clip)
        {
            var parts = anim.GetComponentsInChildren<Transform>(true);
            var was = new (Vector3, Quaternion)[parts.Length];
            for (var i = 0; i < parts.Length; i++)
                was[i] = (parts[i].localPosition, parts[i].localRotation);
            Sample(anim, clip, 0f);
            var start = new (Vector3, Quaternion)[parts.Length];
            for (var i = 0; i < parts.Length; i++)
                start[i] = (parts[i].localPosition, parts[i].localRotation);
            Sample(anim, clip, anim[clip].length);
            var same = true;
            for (var i = 0; i < parts.Length; i++)
            {
                same &= (parts[i].localPosition - start[i].Item1).sqrMagnitude < 1e-8f && Quaternion.Angle(parts[i].localRotation, start[i].Item2) < 0.1f;
                (parts[i].localPosition, parts[i].localRotation) = was[i];
            }
            return same;
        }

        /// <summary>A clip's curves at `time` seconds, sampled alone onto the parts it moves.</summary>
        public static void Sample(Animation anim, string clip, float time)
        {
            var s = anim[clip];
            s.enabled = true;
            s.weight = 1f;
            s.time = time;
            anim.Sample();
            s.enabled = false;
        }
    }

    /// <summary>A clip that loops while it runs (an engine, a fan, a beacon), sampled by hand as a Motion's is.</summary>
    [Serializable]
    public class Loop
    {
        public string clip = "";
        Animation anim;
        float time;

        public bool Running { get; set; }

        public void Bind(Component owner) => anim = Motion.Clips(owner);

        public void Tick(float dt)
        {
            if (!Running || anim == null || string.IsNullOrEmpty(clip) || anim[clip] == null)
                return;
            var length = Mathf.Max(0.0001f, anim[clip].length);
            time = (time + dt) % length;
            Motion.Sample(anim, clip, time);
        }
    }
}
