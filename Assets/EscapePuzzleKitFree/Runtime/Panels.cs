using System;
using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// The panels the systems ask for, whoever draws them: the kit's <see cref="Hud"/> listens here,
    /// and a UI of your own can instead. Each Open says whether anyone answered.
    /// </summary>
    public static class Panels
    {
        /// <summary>A keypad's code to enter.</summary>
        public static event Action<Keypad> CodePad;
        /// <summary>Something to hold up and turn: the item it is (or null), the object to show (or null), and a text to show under it.</summary>
        public static event Action<Item, GameObject, string> Examine;
        /// <summary>The save slots, at a save point.</summary>
        public static event Action<SavePoint> Slots;

        public static bool OpenCodePad(Keypad keypad)
        {
            CodePad?.Invoke(keypad);
            return CodePad != null;
        }

        public static bool OpenExamine(Item item, GameObject shown, string text)
        {
            Examine?.Invoke(item, shown, text);
            return Examine != null;
        }

        public static bool OpenSlots(SavePoint at)
        {
            Slots?.Invoke(at);
            return Slots != null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Fresh()
        {
            CodePad = null;
            Examine = null;
            Slots = null;
        }
    }
}
