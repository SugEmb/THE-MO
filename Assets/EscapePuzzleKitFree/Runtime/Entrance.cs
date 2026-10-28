using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>Where a door from another level arrives: the player is placed here, facing its forward.</summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Entrance")]
    public class Entrance : MonoBehaviour
    {
        [Tooltip("What a door's entrance names; empty: the GameObject's name")] public string id = "";

        public string Id => id != "" ? id : name;

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.9f, 0.4f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.9f, 0.3f);
            Gizmos.DrawLine(transform.position + Vector3.up * 0.9f, transform.position + Vector3.up * 0.9f + transform.forward * 0.8f);
        }
    }
}
