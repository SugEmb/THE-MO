using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// A code entry: the right <see cref="code"/> switches it on, a wrong one says so. Its keys are
    /// pressed in the world (parts named *_key_0 to *_key_9, *_key_C clearing, *_key_E entering),
    /// or, used anywhere else, it opens the Hud's code pad. An entry is checked once it is as long
    /// as the code.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Keypad")]
    public class Keypad : Switch
    {
        [Tooltip("Digits")] public string code = "1234";
        [Tooltip("Used anywhere but a key, it opens the Hud's code pad")] public bool usePad = true;
        [TextArea(1, 3)] public string wrongText = "Wrong code.";
        public AudioClip keySound;
        [Tooltip("A key pressed: its key")] public StringEvent onKeyed = new StringEvent();

        public string Entry { get; private set; } = "";
        public override string Verb => "USE";

        Parts parts;

        protected override void Awake()
        {
            base.Awake();
            parts = new Parts(this, "_key_");
        }

        protected override void Update()
        {
            base.Update();
            parts.Tick(Time.deltaTime);
        }

        protected override void OnInteract(Interactor who, Collider part)
        {
            if (!CheckPower())
                return;
            var key = parts.Key(part);
            if (key != null)
                Press(key);
            else if (usePad && !on)
                Panels.OpenCodePad(this);
        }

        /// <summary>Presses a key: a digit, "C" to clear, "E" to enter.</summary>
        public void Press(string key)
        {
            if (on || !power.Live)
                return;
            parts.Pulse(key);
            Sound.Play(this, keySound, 0.6f);
            onKeyed.Invoke(key);
            if (key == "C")
                Entry = "";
            else if (key == "E")
                Check();
            else
            {
                Entry += key;
                if (Entry.Length >= code.Length)
                    Check();
            }
        }

        /// <summary>Enters a whole code at once, as a pad's enter does; whether it was right.</summary>
        public bool Enter(string entry)
        {
            if (on || !CheckPower())
                return on;
            Entry = entry ?? "";
            Check();
            return on;
        }

        void Check()
        {
            if (Entry == code)
                Flip(true);
            else
                Refuse("code", wrongText);
            Entry = "";
        }
    }
}
