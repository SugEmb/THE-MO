using System.Collections.Generic;
using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// An Openable that holds something: a drawer, a locker, a crate. Its first opening gives what it
    /// holds and fires its beat; searched, it is an ordinary Openable.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Stash")]
    public class Stash : Openable
    {
        public Item item;
        [Min(1)] public int count = 1;
        [Tooltip("Fired as it is searched")] public StoryBeat beat;
        [Tooltip("Shown inside until taken: the item's model in the drawer")] public GameObject shown;

        public bool Holds => item != null;
        public override string Verb => Holds && !open ? "SEARCH" : base.Verb;

        protected override void Awake()
        {
            base.Awake();
            if (shown != null)
                shown.SetActive(Holds);
        }

        public override void Open()
        {
            if (open)
                return;
            base.Open();
            if (!Holds)
                return;
            Run.Add(item.Id, count);
            Say(count > 1 ? $"Found {count} x {Item.Title(item.Id)}." : $"Found the {Item.Title(item.Id)}.");
            item = null;
            if (shown != null)
                shown.SetActive(false);
            if (beat != null)
                beat.Fire();
        }

        public override void Save(Dictionary<string, object> state)
        {
            base.Save(state);
            state["item"] = item != null ? item.Id : "";
        }

        public override void Load(IDictionary<string, object> state)
        {
            base.Load(state);
            var id = Json.Get(state, "item", item != null ? item.Id : "");
            if (id == "")
                item = null;
            else if (item == null || item.Id != id)
                item = Item.Find(id);
            if (shown != null)
                shown.SetActive(Holds);
        }
    }
}
