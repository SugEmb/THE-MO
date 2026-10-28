using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// The player's flashlight: F switches its Light; on, it drains the run's battery, going out
    /// when it is flat (a battery item restores it). With <see cref="item"/> it works once that is
    /// carried (the flashlight found on a bench). Saved with the player.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Flashlight")]
    [RequireComponent(typeof(Light))]
    public class Flashlight : MonoBehaviour
    {
        [Tooltip("Battery used a minute on, 0 to 1; 0 never runs down")] public float drain = 0.05f;
        [Tooltip("Read F itself; off, call Toggle() from your own input")] public bool readKeys = true;
        [Tooltip("Works only while this is carried; empty: always")] public Item item;
        public AudioClip clickSound;

        public static Flashlight Current { get; private set; }

        Light lamp;

        public bool On
        {
            get => Lamp.enabled;
            set => Lamp.enabled = value && Run.Battery > 0f && (item == null || Run.Has(item.Id));
        }

        Light Lamp => lamp != null ? lamp : lamp = GetComponent<Light>();

        void OnEnable() => Current = this;

        void OnDisable()
        {
            if (Current == this)
                Current = null;
        }

        public void Toggle()
        {
            On = !On;
            Sound.Play(this, clickSound, 0.5f);
        }

        void Update()
        {
            if (readKeys && !Interactor.Paused && Controls.Flashlight)
                Toggle();
            if (On && item != null && !Run.Has(item.Id))
                On = false;
            if (On && drain > 0f)
            {
                Run.Battery -= drain * Time.deltaTime / 60f;
                if (Run.Battery <= 0f)
                    On = false;
            }
            Lamp.intensity = full * (Run.Battery < 0.1f ? 0.4f + 0.6f * Mathf.PerlinNoise(Time.time * 8f, 0f) : 1f);  // flickers when nearly flat
        }

        float full;

        void Awake() => full = Lamp.intensity;
    }
}
