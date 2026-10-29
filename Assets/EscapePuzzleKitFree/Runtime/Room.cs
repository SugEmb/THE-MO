using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>A trigger volume naming where the player is, for the save slots: "Boiler Room".</summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Room")]
    [RequireComponent(typeof(Collider))]
    public class Room : MonoBehaviour
    {
        [Tooltip("Empty: the GameObject's name")] public string title = "";

        public string Title => title != "" ? title : name;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            var player = Interactor.Current;
            if (player != null && (other.transform == player.Body || other.transform.IsChildOf(player.Body)))
                Run.Room = Title;
        }
    }
}
