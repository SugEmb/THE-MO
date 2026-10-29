using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// Something to take: taking it adds its item to the run, hides it and fires its beat. With
    /// <see cref="examine"/> it is held up to look at as it is taken (a note read, a key turned over).
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Pickup")]
    public class Pickup : Interactable
    {
        public Item item;
        [Min(1)] public int count = 1;
        [Tooltip("Fired as it is taken")] public StoryBeat beat;
        [Tooltip("Held up in the examine view as it is taken")] public bool examine;
        public AudioClip takeSound;
        public UnityEvent onTaken = new UnityEvent();

        public override string Verb => "TAKE";

        protected override void OnInteract(Interactor who, Collider part) => Take();

        public void Take()
        {
            if (item == null)
                return;
            Run.Add(item.Id, count);
            if (takeSound != null)
                AudioSource.PlayClipAtPoint(takeSound, transform.position);
            if (!examine || !Panels.OpenExamine(item, item.model != null ? item.model : gameObject, item.kind == Item.Kind.Document ? item.text : item.description))
                Say(count > 1 ? $"Picked up {count} x {Item.Title(item.Id)}." : $"Picked up the {Item.Title(item.Id)}.");
            enabled = false;
            gameObject.SetActive(false);
            onTaken.Invoke();
            if (beat != null)
                beat.Fire();
        }

        public override void Load(IDictionary<string, object> state)
        {
            base.Load(state);
            gameObject.SetActive(enabled);
        }
    }
}
