using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// What a puzzle is made of: used its way (pulled, fed a fuse, keyed a code), it switches on,
    /// sets its <see cref="flag"/> and fires onSwitchedOn. On, it is done: no longer usable, left in
    /// its on pose; with <see cref="toggle"/> it switches off again, its flag staying set. With power
    /// needs it can't be used until they are live, and it powers others while on and powered.
    /// </summary>
    public abstract class Switch : Interactable, ISource
    {
        [Tooltip("On now; as built, before any save")] [SerializeField] protected bool on;
        [Tooltip("Set as it switches on; doors and beats wait on it")] public string flag = "";
        [Tooltip("Can be switched off again; its flag stays set")] public bool toggle;
        [Tooltip("To its on pose and back")] public Motion motion = new Motion();
        public Power power = new Power();
        [Tooltip("Shown while live (on and powered)")] public Renderer[] lit = new Renderer[0];
        [Tooltip("Shown while powered and off")] public Renderer[] idle = new Renderer[0];
        [Tooltip("Loops while live: an engine, a fan")] public Loop loop = new Loop();
        public AudioClip onSound, offSound, refusedSound;
        [TextArea(1, 3)] public string noPowerText = "There's no power.";

        public UnityEvent onSwitchedOn = new UnityEvent(), onSwitchedOff = new UnityEvent();
        [Tooltip("Live (on and powered) or not")] public BoolEvent onLive = new BoolEvent();
        [Tooltip("Refused a use: \"power\", \"item\" or \"code\"")] public StringEvent onRefused = new StringEvent();

        public event Action Changed;

        public bool IsOn => on;
        public bool Powered => power.Live;
        public bool Live { get; private set; }
        public virtual float Output => Live ? 1f : 0f;
        public override bool CanFocus => base.CanFocus && (!on || toggle);

        float shown = -1f;

        protected virtual void Awake()
        {
            motion.Bind(this);
            motion.Set(on ? 1f : 0f);
            loop.Bind(this);
        }

        protected virtual void OnEnable()
        {
            power.Wire(Refresh);
            Refresh();
        }

        protected virtual void OnDisable() => power.Unwire();

        protected virtual void OnValidate() => power.Fix(this);

        protected virtual void Update()
        {
            loop.Tick(Time.deltaTime);
            motion.Tick(Time.deltaTime);
            if (loop.Running && !motion.Moving)
                motion.Hold();
        }

        /// <summary>Switches it on as a use would (a UnityEvent can call it): pose, sound, flag, events.</summary>
        public void SwitchOn() => Flip(true);

        public void SwitchOff() => Flip(false);

        /// <summary>Whether its power lets it be used; saying so, and refusing, when it doesn't.</summary>
        protected bool CheckPower()
        {
            if (power.Live)
                return true;
            Refuse("power", noPowerText);
            return false;
        }

        protected void Refuse(string why, string text)
        {
            Say(text);
            Sound.Play(this, refusedSound);
            onRefused.Invoke(why);
        }

        protected virtual void Flip(bool value)
        {
            if (value == on)
                return;
            on = value;
            motion.Play(on ? 1f : 0f);
            Sound.Play(this, on ? onSound : offSound);
            if (on)
            {
                Run.SetFlag(flag);
                onSwitchedOn.Invoke();
            }
            else
                onSwitchedOff.Invoke();
            Refresh();
        }

        /// <summary>Lamps, loop and what it powers, after its state or its power moved.</summary>
        protected void Refresh()
        {
            var was = Live;
            Live = on && power.Live;
            loop.Running = Live;
            foreach (var r in lit)
                if (r != null) r.enabled = Live;
            foreach (var r in idle)
                if (r != null) r.enabled = !on && power.Live;
            if (Live != was && !SaveGame.Loading)
                onLive.Invoke(Live);
            if (Live != was || !Mathf.Approximately(Output, shown))
            {
                shown = Output;
                Changed?.Invoke();
            }
            OnRefresh();
        }

        /// <summary>After each refresh: a subclass's own lamps.</summary>
        protected virtual void OnRefresh() { }

        public override void Save(Dictionary<string, object> state)
        {
            base.Save(state);
            state["on"] = on;
        }

        public override void Load(IDictionary<string, object> state)
        {
            base.Load(state);
            on = Json.Get(state, "on", on);
            motion.Set(on ? 1f : 0f);
            Refresh();
        }
    }
}
