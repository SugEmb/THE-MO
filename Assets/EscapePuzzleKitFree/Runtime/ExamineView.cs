using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// Holds a copy of an object up close over the dimmed game, drawn by a camera of its own on
    /// <see cref="layer"/>, far off at <see cref="away"/>: turned by dragging (or the arrow keys),
    /// zoomed with the wheel. The <see cref="Hud"/> draws its title and text over the
    /// <see cref="Picture"/>.
    /// </summary>
    public class ExamineView : MonoBehaviour
    {
        [Tooltip("A layer nothing else uses; the game's cameras stop drawing it")] public int layer = 31;
        public Vector3 away = new Vector3(0f, -1000f, 0f);
        [Tooltip("Degrees a second on the keys")] public float turnSpeed = 115f;
        [Tooltip("The turn a model is first shown at: 20 for a front facing -z, 200 for one facing +z (Unity's forward)")] public float startYaw = 200f;

        public bool Showing { get; private set; }
        public RenderTexture Picture { get; private set; }
        public float yaw, pitch, zoom = 1f;

        Camera cam;
        Transform pivot;
        GameObject copy;
        Light lamp;
        float dist = 1f;

        /// <summary>Holds up a copy of <paramref name="source"/>, its scripts, colliders and sounds left out.</summary>
        public void Open(GameObject source)
        {
            Close();
            if (source == null)
                return;
            if (pivot == null)
            {
                pivot = new GameObject("Examine Pivot").transform;
                cam = new GameObject("Examine Camera").AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
                cam.fieldOfView = 30f;
                cam.enabled = false;
                lamp = new GameObject("Examine Light").AddComponent<Light>();
                lamp.type = LightType.Directional;
                lamp.cullingMask = 1 << layer;
                lamp.intensity = 1.1f;
                lamp.enabled = false;  // on only while its camera draws: no light of it reaches the level
                lamp.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
                foreach (var t in new[] { pivot, cam.transform, lamp.transform })
                    DontDestroyOnLoad(t.gameObject);
            }
            pivot.position = away;
            pivot.gameObject.SetActive(false);  // the copy's scripts don't wake
            copy = Instantiate(source, pivot);
            foreach (var m in copy.GetComponentsInChildren<MonoBehaviour>(true))
                DestroyImmediate(m);
            foreach (var c in copy.GetComponentsInChildren<Collider>(true))
                DestroyImmediate(c);
            foreach (var a in copy.GetComponentsInChildren<AudioSource>(true))
                DestroyImmediate(a);
            foreach (var t in copy.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = layer;
            copy.SetActive(true);
            pivot.gameObject.SetActive(true);
            copy.transform.localPosition = Vector3.zero;
            copy.transform.localRotation = Quaternion.identity;
            copy.transform.localScale = source.transform.lossyScale;

            var bounds = new Bounds(pivot.position, Vector3.zero);
            var first = true;
            foreach (var r in copy.GetComponentsInChildren<Renderer>())
            {
                if (first) bounds = r.bounds; else bounds.Encapsulate(r.bounds);
                first = false;
            }
            copy.transform.position -= bounds.center - pivot.position;
            dist = Mathf.Max(0.05f, bounds.extents.magnitude) / Mathf.Sin(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.25f;
            var flat = bounds.size.y < 0.35f * Mathf.Max(bounds.size.x, bounds.size.z);
            yaw = startYaw;
            pitch = flat ? 55f : 15f;  // what lies flat is shown from above, as it was found
            zoom = 1f;
            cam.cullingMask = 1 << layer;
            foreach (var c in Camera.allCameras)
                if (c != cam)
                    c.cullingMask &= ~(1 << layer);
            Showing = true;
        }

        public void Close()
        {
            if (copy != null)
                Destroy(copy);
            copy = null;
            Showing = false;
        }

        /// <summary>Turns and zooms by the mouse and keys, and draws it; the Hud calls it each frame while showing.</summary>
        public void Tick(bool keysTurn)
        {
            if (!Showing)
                return;
            var keys = keysTurn ? Controls.Move : Vector2.zero;
            var drag = Controls.Held ? Controls.Look * 5f : Vector2.zero;
            yaw -= keys.x * turnSpeed * Time.unscaledDeltaTime + drag.x;
            pitch = Mathf.Clamp(pitch - keys.y * turnSpeed * Time.unscaledDeltaTime + drag.y, -85f, 85f);
            zoom = Mathf.Clamp(zoom * Mathf.Pow(1.1f, Controls.Wheel), 0.5f, 2.5f);
            pivot.rotation = Quaternion.Euler(pitch, 0f, 0f) * Quaternion.Euler(0f, yaw, 0f);
            cam.transform.SetPositionAndRotation(away + new Vector3(0f, 0f, -dist / zoom), Quaternion.identity);
            cam.nearClipPlane = dist * 0.02f;
            cam.farClipPlane = dist * 10f;
            if (Picture == null || Picture.width != Screen.width || Picture.height != Screen.height)
            {
                if (Picture != null) Picture.Release();
                Picture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            }
            cam.targetTexture = Picture;
            lamp.enabled = true;
            cam.Render();
            lamp.enabled = false;
        }

        void OnDestroy()
        {
            if (Picture != null)
                Picture.Release();
            foreach (var c in new Component[] { pivot, cam, lamp })
                if (c != null)
                    Destroy(c.gameObject);
        }
    }
}
