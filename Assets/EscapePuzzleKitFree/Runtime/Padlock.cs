using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>A lock on a chain, a hasp, a lid: its key unlocks it, and its motion opens the shackle.</summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Padlock")]
    public class Padlock : FuseBox
    {
        public override string Verb => "UNLOCK";
        public override Icon FocusIcon => Icon.Key;

        void Reset() => missingText = "It's padlocked.";
    }
}
