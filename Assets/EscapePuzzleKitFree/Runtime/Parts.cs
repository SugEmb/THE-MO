using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// A prop's named parts, each its own collider: a keypad's keys (*_key_1, *_key_C), a panel's
    /// switches, a lock's wheels. A part's name after the marker is its key. Each part moves by the
    /// prop's clip of its own name where there is one, and lights its lamp, the part named
    /// *_lamp_&lt;key&gt;.
    /// </summary>
    public class Parts
    {
        readonly Dictionary<Collider, string> byCollider = new Dictionary<Collider, string>();
        readonly Dictionary<string, Motion> moves = new Dictionary<string, Motion>();
        readonly Dictionary<string, Renderer> lamps = new Dictionary<string, Renderer>();

        /// <summary>The keys, in their parts' order by name: "1", "2", "3".</summary>
        public readonly List<string> Keys = new List<string>();

        public Parts(Component owner, string marker)
        {
            var anim = Motion.Clips(owner);
            foreach (var c in owner.GetComponentsInChildren<Collider>(true))
            {
                var key = KeyOf(c.name, marker);
                if (key == null)
                    continue;
                byCollider[c] = key;
                if (Keys.Contains(key))
                    continue;
                Keys.Add(key);
                if (anim != null && anim[c.name] != null)
                {
                    var m = Motion.OfClip(c.name);
                    m.Bind(owner);
                    moves[key] = m;
                }
            }
            Keys.Sort(StringComparer.Ordinal);
            foreach (var r in owner.GetComponentsInChildren<Renderer>(true))
            {
                var key = KeyOf(r.name, "_lamp_");
                if (key != null)
                    lamps[key] = r;
            }
        }

        public static string KeyOf(string name, string marker)
        {
            var k = name.LastIndexOf(marker, StringComparison.Ordinal);
            return k < 0 ? null : name.Substring(k + marker.Length);
        }

        /// <summary>The key of the part this collider is, or null.</summary>
        public string Key(Collider part) => part != null && byCollider.TryGetValue(part, out var key) ? key : null;

        public Collider Collider(string key) => byCollider.FirstOrDefault(kv => kv.Value == key).Key;

        public void Play(string key, float t)
        {
            if (moves.TryGetValue(key, out var m)) m.Play(t);
        }

        public void Set(string key, float t)
        {
            if (moves.TryGetValue(key, out var m)) m.Set(t);
        }

        public void Pulse(string key)
        {
            if (moves.TryGetValue(key, out var m)) m.Pulse();
        }

        public void Light(string key, bool on)
        {
            if (lamps.TryGetValue(key, out var r)) r.enabled = on;
        }

        public void Tick(float dt)
        {
            foreach (var m in moves.Values)
                m.Tick(dt);
        }
    }
}
