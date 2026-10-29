using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// An Openable with a lock. Tried while locked, it unlocks (staying shut) when the player is on
    /// its <see cref="unlockSide"/>, carries its <see cref="key"/> or its <see cref="unlockFlag"/> is
    /// set, in that order; otherwise it says its locked text. Jammed, it only rattles. Unlocked, it
    /// opens and closes, and opening it can end the game or lead to another level. Its front is its
    /// local +Z. Anything can unlock it by <see cref="Unlock"/>: a switch's onSwitchedOn, say.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Door")]
    public class Door : Openable
    {
        public enum Lock { Open, Locked, Jammed }
        public enum Side { Neither, Front, Back }

        [Tooltip("Open: it opens. Locked: a key, a side or a flag unlocks it. Jammed: it never opens, unless Unlock() is called")]
        public Lock lockState = Lock.Open;
        [Tooltip("The item that unlocks it")] public Item key;
        [Tooltip("The key stays carried after; else it is used up")] public bool keepKey;
        [Tooltip("A bolt: from this side it unlocks without a key")] public Side unlockSide = Side.Neither;
        [Tooltip("Once this flag is set, trying it unlocks it")] public string unlockFlag = "";
        [TextArea(1, 3)] public string lockedText = "It's locked.";
        [TextArea(1, 3)] public string jammedText = "It's jammed.";
        [Tooltip("Set the first time it is found locked")] public string lockedObjective = "";
        [Tooltip("Opening it is the way out: the run ends, escaped")] public bool endsGame;
        [Tooltip("Opening it loads this level (its scene's name or path, in the Build Profiles)")] public string leadsTo = "";
        [Tooltip("The Entrance there to arrive at; empty for the level's start")] public string entrance = "";
        [Tooltip("Shown in this door's place once it is unlocked, this door hidden (a chained door pried open becomes a plain one). An object outside this door")]
        public GameObject unlockedPiece;
        public AudioClip unlockSound, lockedSound;
        public UnityEvent onUnlocked = new UnityEvent();
        [Tooltip("Tried while locked or jammed, and it stayed so")] public UnityEvent onRefused = new UnityEvent();

        public bool IsLocked => lockState != Lock.Open;
        public override string Verb => lockState == Lock.Locked && WouldUnlock(Interactor.Current) ? "UNLOCK" : base.Verb;
        public override Icon FocusIcon => lockState == Lock.Locked && WouldUnlock(Interactor.Current) ? Icon.Key : focusIcon;

        void Reset() => focusIcon = Icon.Door;

        /// <summary>Whether trying it now, as <paramref name="who"/>, would unlock it.</summary>
        public bool WouldUnlock(Interactor who) =>
            lockState == Lock.Locked && (OnUnlockSide(who) || key != null && Run.Has(key.Id) || Run.HasFlag(unlockFlag));

        public bool OnUnlockSide(Interactor who)
        {
            if (unlockSide == Side.Neither || who == null)
                return false;
            var front = Vector3.Dot(transform.forward, who.Body.position - transform.position) > 0f;
            return front == (unlockSide == Side.Front);
        }

        protected override void OnInteract(Interactor who, Collider part)
        {
            switch (lockState)
            {
                case Lock.Jammed:
                    Refuse(jammedText);
                    break;
                case Lock.Locked when OnUnlockSide(who) || Run.HasFlag(unlockFlag):
                    Unlock();
                    break;
                case Lock.Locked when key != null && Run.Has(key.Id):
                    if (!keepKey && !key.keep)
                        Run.Remove(key.Id);
                    Unlock();
                    break;
                case Lock.Locked:
                    Refuse(lockedText);
                    if (lockedObjective != "" && Run.Objective != lockedObjective)
                        Run.Objective = lockedObjective;
                    break;
                default:
                    base.OnInteract(who, part);
                    break;
            }
        }

        void Refuse(string text)
        {
            Say(text);
            Sound.Play(this, lockedSound);
            if (!open && !motion.Moving)
                motion.Play(0.04f, () => motion.Play(0f));  // a rattle
            onRefused.Invoke();
        }

        /// <summary>Unlocks it, shut, from locked or jammed.</summary>
        public void Unlock()
        {
            if (lockState == Lock.Open)
                return;
            lockState = Lock.Open;
            Sound.Play(this, unlockSound);
            onUnlocked.Invoke();
            Swap();
        }

        void Swap()
        {
            if (unlockedPiece == null || lockState != Lock.Open)
                return;
            unlockedPiece.SetActive(true);
            gameObject.SetActive(false);
        }

        /// <summary>Locks or jams it, shutting it if open.</summary>
        public void SetLock(Lock value)
        {
            lockState = value;
            if (value != Lock.Open)
                Close();
        }

        public override void Open()
        {
            if (lockState != Lock.Open || open)
                return;
            base.Open();
            if (endsGame)
                Run.Escape();
            else if (leadsTo != "")
                SaveGame.ChangeLevel(leadsTo, entrance);
        }

        public override void Save(Dictionary<string, object> state)
        {
            base.Save(state);
            state["lock"] = lockState;
        }

        public override void Load(IDictionary<string, object> state)
        {
            lockState = Json.Get(state, "lock", lockState);
            base.Load(state);
            Swap();
        }
    }
}
