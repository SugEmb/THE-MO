using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// Looked at closely: says its <see cref="text"/> (a scrawl on the wall, a code on a note pinned
    /// up) and fires its beat. With <see cref="hold"/> the object is held up in the examine view, its
    /// text under it.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Inspectable")]
    public class Inspectable : Interactable
    {
        [TextArea(2, 6)] public string text = "";
        [Tooltip("Fired on looking")] public StoryBeat beat;
        [Tooltip("Held up in the examine view; else its text is said")] public bool hold;

        public override string Verb => "EXAMINE";

        void Reset() => focusIcon = Icon.Examine;

        protected override void OnInteract(Interactor who, Collider part)
        {
            if (!hold || !Panels.OpenExamine(null, gameObject, text))
                Say(text);
            if (beat != null)
                beat.Fire();
        }
    }
}
