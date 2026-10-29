using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// What powers others: a switch while on, a <see cref="Powered"/> machine while running, a
    /// LogicGate. Live is whether it powers; Output what a Gauge reads
    /// (a valve's share of its turns). Changed fires as either moves.
    /// </summary>
    public interface ISource
    {
        bool Live { get; }
        float Output { get; }
        event Action Changed;
    }

    [Serializable] public class BoolEvent : UnityEvent<bool> { }
    [Serializable] public class StringEvent : UnityEvent<string> { }

    /// <summary>
    /// The sources a thing needs: it is powered while every one is live, and always with none. Drop
    /// any switch, Powered or LogicGate in (its GameObject will do).
    /// </summary>
    [Serializable]
    public class Power
    {
        [Tooltip("Powered while every one is live; with none, always. Switches, Powered, LogicGates")]
        public MonoBehaviour[] needs = new MonoBehaviour[0];

        Action changed;

        public bool Live => needs.OfType<ISource>().All(s => s.Live);

        /// <summary>The first source's output, what a gauge reads; 1 with none.</summary>
        public float Output => needs.OfType<ISource>().Select(s => s.Output).DefaultIfEmpty(1f).First();

        public bool Any => needs.OfType<ISource>().Any();

        /// <summary>Calls <paramref name="onChange"/> whenever a source moves; the owner's OnEnable.</summary>
        public void Wire(Action onChange)
        {
            Unwire();
            changed = onChange;
            foreach (var s in needs.OfType<ISource>())
                s.Changed += changed;
        }

        /// <summary>Sets the needs from code, rewired at once: power.Set(generator, breaker).</summary>
        public void Set(params MonoBehaviour[] sources)
        {
            var c = changed;
            Unwire();
            needs = sources ?? new MonoBehaviour[0];
            if (c == null)
                return;
            Wire(c);
            c();
        }

        public void Unwire()
        {
            if (changed == null)
                return;
            foreach (var s in needs.OfType<ISource>())
                s.Changed -= changed;
            changed = null;
        }

        /// <summary>Puts each entry's source in its place: a GameObject dropped in picks its first ISource. The owner's OnValidate.</summary>
        public void Fix(MonoBehaviour owner)
        {
            for (var i = 0; i < needs.Length; i++)
                if (needs[i] != null && !(needs[i] is ISource))
                    needs[i] = needs[i].GetComponents<MonoBehaviour>().FirstOrDefault(m => m is ISource && m != owner);
        }
    }
}
