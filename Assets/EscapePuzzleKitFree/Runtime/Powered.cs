using System;
using UnityEngine;
using UnityEngine.Events;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// A machine that runs while its power is live: a lamp, a fan, a lift, a blast door. Its motion
    /// plays to its running pose as power comes and back as it goes, its loop runs and its lit parts
    /// show while running. It powers others in turn.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Powered")]
    public class Powered : MonoBehaviour, ISource
    {
        public Power power = new Power();
        [Tooltip("To its running pose: a door opening, a lift rising")] public Motion motion = new Motion();
        [Tooltip("Loops while running")] public Loop loop = new Loop();
        [Tooltip("Shown while running")] public Renderer[] lit = new Renderer[0];
        [Tooltip("Turned on while running")] public Light[] lights = new Light[0];
        public AudioClip startSound, stopSound;
        public UnityEvent onStarted = new UnityEvent(), onStopped = new UnityEvent();
        public BoolEvent onLive = new BoolEvent();

        public event Action Changed;
        public bool Live { get; private set; }
        public float Output => Live ? 1f : 0f;

        bool awake;

        void Awake()
        {
            motion.Bind(this);
            loop.Bind(this);
        }

        void OnEnable()
        {
            power.Wire(Refresh);
            Refresh();
        }

        void OnDisable()
        {
            power.Unwire();
            Refresh();
        }

        void OnValidate() => power.Fix(this);

        void Update()
        {
            loop.Tick(Time.deltaTime);
            motion.Tick(Time.deltaTime);
            if (loop.Running && !motion.Moving)
                motion.Hold();
        }

        void Refresh()
        {
            var live = isActiveAndEnabled && power.Live;
            var first = !awake;
            awake = true;
            if (live == Live && !first)
                return;
            Live = live;
            loop.Running = live;
            foreach (var r in lit)
                if (r != null) r.enabled = live;
            foreach (var l in lights)
                if (l != null) l.enabled = live;
            if (first || SaveGame.Loading)
                motion.Set(live ? 1f : 0f);
            else
            {
                motion.Play(live ? 1f : 0f);
                Sound.Play(this, live ? startSound : stopSound);
                (live ? onStarted : onStopped).Invoke();
                onLive.Invoke(live);
            }
            Changed?.Invoke();
        }
    }
}
