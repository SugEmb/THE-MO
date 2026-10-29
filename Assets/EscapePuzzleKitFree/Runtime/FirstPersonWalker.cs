using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// A plain first-person controller for the demo and for trying the kit: WASD or the arrows and
    /// the mouse, Shift runs. It stands still and frees the cursor while a panel is open. Any other
    /// controller works as well: the kit only needs an <see cref="Interactor"/> on its camera.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/First Person Walker")]
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonWalker : MonoBehaviour, IPlayerBody
    {
        [Tooltip("The camera, a child at eye height")] public Transform view;
        public float walk = 2.2f, run = 4f, mouse = 2f;
        [Tooltip("Lock and hide the cursor while playing")] public bool lockCursor = true;

        CharacterController body;
        float pitch, fall;
        bool free;  // Esc freed the cursor, with no Hud to pause

        void Awake()
        {
            body = GetComponent<CharacterController>();
            if (Application.isBatchMode || System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-no-cursor-lock") >= 0)
                lockCursor = false;  // tests and captures leave the cursor alone
            if (view == null && GetComponentInChildren<Camera>() != null)
                view = GetComponentInChildren<Camera>().transform;
        }

        void Update()
        {
            var paused = Interactor.Paused;
            if (Hud.Current == null && Controls.Cancel)
                free = true;
            else if (free && Controls.Click)
                free = false;
            if (lockCursor)
            {
                var locked = !paused && !free && Application.isFocused;
                Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !locked;
            }
            if (paused)
                return;
            if (!free)
            {
                var d = Controls.Look * mouse;
                transform.Rotate(0f, d.x, 0f);
                pitch = Mathf.Clamp(pitch - d.y, -80f, 80f);
                if (view != null)
                    view.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
            var m = Vector2.ClampMagnitude(Controls.Move, 1f) * (Controls.Running ? run : walk);
            fall = body.isGrounded ? -1f : fall - 9.8f * Time.deltaTime;
            body.Move((transform.right * m.x + transform.forward * m.y + Vector3.up * fall) * Time.deltaTime);
        }

        public void Place(Vector3 position, float yaw, float look)
        {
            body.enabled = false;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            body.enabled = true;
            pitch = look;
            fall = 0f;
            if (view != null)
                view.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            Physics.SyncTransforms();
        }

        void OnDisable()
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
