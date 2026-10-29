using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// The kit's screen, drawn with IMGUI so it needs no Canvas and works on any pipeline: the aim
    /// point and prompt, subtitles, objective and hints; and its panels: the inventory (Tab),
    /// the examine view, a keypad's code pad, the save slots, the pause menu (Esc) and the end.
    /// It only listens (to <see cref="Run"/>, <see cref="Panels"/> and the
    /// <see cref="Interactor"/>), so a UI of your own can take its place.
    /// </summary>
    [AddComponentMenu("Escape Puzzle Kit Free/Hud")]
    [RequireComponent(typeof(ExamineView))]
    public class Hud : MonoBehaviour
    {
        enum Panel { None, Inventory, Examine, CodePad, Slots, Pause, End }

        [Tooltip("Every item a save may hold, so a loaded save finds them")] public Item[] catalogue = new Item[0];
        [Tooltip("The aim point's icons: hand, examine, door, key")] public Texture2D[] focusIcons = new Texture2D[4];
        public Texture2D aimDot;
        public Color accent = new Color(0.96f, 0.78f, 0.36f);
        public Color panelColor = new Color(0.06f, 0.07f, 0.08f, 0.92f);
        [Tooltip("Text size, as a share of the screen's height")] public float textSize = 1f / 34f;
        [Tooltip("The pause menu offers Load and Restart")] public bool pauseMenu = true;
        [Tooltip("Show health and battery in the inventory")] public bool showBars = true;
        public AudioClip openSound, closeSound;

        public static Hud Current { get; private set; }

        Panel panel;
        Interactor interactor;
        ExamineView examine;
        Keypad pad;
        SavePoint savePoint;
        string padEntry = "";
        string examineTitle = "", examineText = "";
        string[] pages = new string[0];
        int page;
        string selected;
        float padClosesAt = -1f;
        bool fromInventory;
        int openedFrame;
        IDictionary<string, object>[] slots = new IDictionary<string, object>[SaveGame.Slots];

        string subtitle = "", speaker = "";
        float subtitleLeft;
        string objectiveShown = "";
        float objectiveLeft;
        string hintKey = "", hint = "";
        float hintLeft;

        GUIStyle label;
        float u;

        public bool IsOpen => panel != Panel.None;

        void Awake()
        {
            Current = this;
            examine = GetComponent<ExamineView>();  // the catalogue, referenced, is loaded: its items known by id
        }

        void OnEnable()
        {
            Run.Said += OnSaid;
            Run.ObjectiveChanged += OnObjective;
            Run.Hinted += OnHint;
            Run.Escaped += OnEscaped;
            Panels.CodePad += OpenCodePad;
            Panels.Examine += OpenExamine;
            Panels.Slots += OpenSlots;
        }

        void OnDisable()
        {
            Run.Said -= OnSaid;
            Run.ObjectiveChanged -= OnObjective;
            Run.Hinted -= OnHint;
            Run.Escaped -= OnEscaped;
            Panels.CodePad -= OpenCodePad;
            Panels.Examine -= OpenExamine;
            Panels.Slots -= OpenSlots;
            Interactor.Hold(this, false);
            if (Current == this)
                Current = null;
        }

        // --- what it hears -------------------------------------------------------------------------

        void OnSaid(string text, string who)
        {
            subtitle = text;
            speaker = who;
            subtitleLeft = 2f + text.Length * 0.06f;
        }

        void OnObjective(string text)
        {
            objectiveShown = text;
            objectiveLeft = text != "" ? 6f : 0f;
        }

        void OnHint(string key, string verb)
        {
            hintKey = key;
            hint = verb;
            hintLeft = 5f;
        }

        void OnEscaped() => Open(Panel.End);

        void OpenCodePad(Keypad keypad)
        {
            pad = keypad;
            padEntry = "";
            padClosesAt = -1f;
            Open(Panel.CodePad);
        }

        void OpenExamine(Item item, GameObject shown, string text)
        {
            fromInventory = panel == Panel.Inventory;
            examineTitle = item != null ? Item.Title(item.Id) : shown != null ? Pretty(shown.name) : "";
            examineText = text ?? "";
            pages = examineText.Replace("\r", "").Split(new[] { "\n---\n" }, StringSplitOptions.None).Select(p => p.Trim()).ToArray();
            page = 0;
            examine.Open(shown);
            Open(Panel.Examine);
        }

        void OpenSlots(SavePoint at)
        {
            savePoint = at;
            Open(Panel.Slots);
        }

        // --- panels --------------------------------------------------------------------------------

        void Open(Panel p)
        {
            if (panel == Panel.Examine && p != Panel.Examine)
                examine.Close();
            var was = panel;
            panel = p;
            openedFrame = Time.frameCount;
            if (p == Panel.Slots)
                for (var s = 0; s < slots.Length; s++)
                    slots[s] = SaveGame.Peek(s);
            Interactor.Hold(this, p != Panel.None);
            Time.timeScale = p == Panel.Pause || p == Panel.End ? 0f : 1f;
            if (was != p && interactor != null)
                Sound.Play(interactor, p == Panel.None ? closeSound : openSound, 0.5f);
        }

        /// <summary>Opens or closes the inventory, as Tab does.</summary>
        public void ToggleInventory() => Open(panel == Panel.Inventory ? Panel.None : panel == Panel.None ? Panel.Inventory : panel);

        public void Close()
        {
            if (panel == Panel.End)
                return;
            var back = panel == Panel.Examine && fromInventory ? Panel.Inventory : panel == Panel.Slots && savePoint == null ? Panel.Pause : Panel.None;
            savePoint = panel == Panel.Slots ? null : savePoint;
            Open(back);
        }

        void Update()
        {
            interactor = Interactor.Current;
            var dt = Time.unscaledDeltaTime;
            subtitleLeft -= dt;
            objectiveLeft -= dt;
            hintLeft -= dt;
            if (Time.frameCount == openedFrame)
                return;  // the key that opened it is not the one to close it
            switch (panel)
            {
                case Panel.None:
                    if (Interactor.Paused) break;  // another holder's close-up (a minigame's): its keys, not ours
                    if (Controls.Inventory) ToggleInventory();
                    else if (Controls.Cancel && pauseMenu) Open(Panel.Pause);
                    break;
                case Panel.Inventory:
                    if (Controls.Inventory || Controls.Cancel) Close();
                    break;
                case Panel.Examine:
                    examine.Tick(pages.Length <= 1);
                    if (pages.Length > 1)
                    {
                        var m = Controls.Move.x;
                        if (Controls.Confirm || Mathf.Abs(m) > 0.5f && Pressed(m)) page = Mathf.Clamp(page + (m < 0f ? -1 : 1), 0, pages.Length - 1);
                    }
                    if (Controls.Cancel || Controls.Interact || Controls.Inventory) Close();
                    break;
                case Panel.CodePad:
                    if (pad == null || !pad.isActiveAndEnabled) { Open(Panel.None); break; }
                    if (padClosesAt >= 0f)
                    {
                        if (Time.unscaledTime >= padClosesAt) Open(Panel.None);
                        break;
                    }
                    var d = Controls.Digit;
                    if (d >= 0) PadKey(d.ToString());
                    else if (Controls.Back) PadKey("C");
                    else if (Controls.Confirm) PadKey("E");
                    else if (Controls.Cancel) Close();
                    break;
                case Panel.Slots:
                case Panel.Pause:
                    if (Controls.Cancel) Close();
                    break;
            }
        }

        float lastPage;
        bool Pressed(float m)
        {
            if (Time.unscaledTime - lastPage < 0.25f)
                return false;
            lastPage = Time.unscaledTime;
            return true;
        }

        void PadKey(string key)
        {
            if (pad == null || padClosesAt >= 0f)
                return;
            pad.Press(key);  // its keys in the world move too
            padEntry = pad.IsOn ? "OPEN" : pad.Entry;
            if (pad.IsOn)
                padClosesAt = Time.unscaledTime + 0.8f;
        }

        // --- drawing -------------------------------------------------------------------------------

        void OnGUI()
        {
            u = Screen.height * textSize;
            label ??= new GUIStyle(GUI.skin.label) { wordWrap = true, richText = true };
            GUI.depth = -10;
            switch (panel)
            {
                case Panel.None: DrawPlay(); break;
                case Panel.Inventory: DrawInventory(); break;
                case Panel.Examine: DrawExamine(); break;
                case Panel.CodePad: DrawCodePad(); break;
                case Panel.Slots: DrawSlots(); break;
                case Panel.Pause: DrawPause(); break;
                case Panel.End: DrawEnd(); break;
            }
            if (panel != Panel.Pause && panel != Panel.End && subtitleLeft > 0f)
                DrawSubtitle();
        }

        void DrawPlay()
        {
            float w = Screen.width, h = Screen.height;
            var focus = interactor != null ? interactor.Focus : null;
            var icon = focus != null && focusIcons.Length > (int)focus.FocusIcon ? focusIcons[(int)focus.FocusIcon] : null;
            if (Interactor.Paused)  // another holder's close-up: no aim over it
                focus = null;
            else if (focus != null && icon != null)
                GUI.DrawTexture(new Rect(w / 2f - u, h / 2f - u, 2f * u, 2f * u), icon);
            else
            {
                var s = focus != null ? u * 0.45f : u * 0.22f;
                Fill(new Rect(w / 2f - s / 2f, h / 2f - s / 2f, s, s), focus != null ? accent : new Color(1f, 1f, 1f, 0.6f), aimDot);
            }
            if (focus != null)
            {
                var text = $"<color=#{ColorUtility.ToHtmlStringRGB(accent)}>[E]</color>  {focus.Verb}" + (focus.label != "" ? $"  <size={(int)(u * 0.8f)}>{focus.label}</size>" : "");
                Text(new Rect(0f, h / 2f + u * 1.6f, w, u * 1.5f), text, u, Color.white, TextAnchor.UpperCenter, true);
            }
            if (objectiveLeft > 0f && objectiveShown != "")
            {
                var a = Mathf.Clamp01(objectiveLeft);
                Text(new Rect(u, u, w * 0.5f, u), "OBJECTIVE", u * 0.7f, Alpha(accent, a), TextAnchor.UpperLeft);
                Text(new Rect(u, u * 1.8f, w * 0.5f, u * 3f), objectiveShown, u * 0.9f, new Color(1f, 1f, 1f, a), TextAnchor.UpperLeft, true);
            }
            if (hintLeft > 0f)
            {
                var a = Mathf.Clamp01(hintLeft);
                var r = new Rect(w - u * 12f, h - u * 3.2f, u * 11f, u * 1.8f);
                Fill(r, Alpha(panelColor, panelColor.a * a));
                Text(r, $"<color=#{ColorUtility.ToHtmlStringRGB(accent)}>[{hintKey}]</color>  {hint}", u * 0.85f, new Color(1f, 1f, 1f, a), TextAnchor.MiddleCenter);
            }
        }

        void DrawSubtitle()
        {
            float w = Screen.width, h = Screen.height;
            var a = Mathf.Clamp01(subtitleLeft * 2f);
            var text = (speaker != "" ? $"<color=#{ColorUtility.ToHtmlStringRGB(accent)}>{speaker}:</color> " : "") + subtitle;
            Text(new Rect(w * 0.15f + 2f, h * 0.8f + 2f, w * 0.7f, u * 4f), Strip(text), u, new Color(0f, 0f, 0f, a * 0.8f), TextAnchor.UpperCenter, true);
            Text(new Rect(w * 0.15f, h * 0.8f, w * 0.7f, u * 4f), text, u, new Color(1f, 1f, 1f, a), TextAnchor.UpperCenter, true);
        }

        void DrawInventory()
        {
            float w = Screen.width, h = Screen.height;
            Fill(new Rect(0, 0, w, h), new Color(0f, 0f, 0f, 0.55f));
            var box = new Rect(w / 2f - u * 18f, h / 2f - u * 12f, u * 36f, u * 24f);
            Fill(box, panelColor);
            Text(new Rect(box.x + u, box.y + u * 0.6f, box.width, u * 1.4f), "INVENTORY", u * 1.1f, accent, TextAnchor.UpperLeft);
            if (Run.Objective != "")
                Text(new Rect(box.x + u, box.y + u * 2.1f, box.width - 2f * u, u * 2f), "Objective: " + Run.Objective, u * 0.8f, new Color(1f, 1f, 1f, 0.75f), TextAnchor.UpperLeft, true);
            var items = Run.Items.ToList();
            if (selected != null && !Run.Has(selected))
                selected = null;
            var cell = u * 4.2f;
            for (var i = 0; i < items.Count; i++)
            {
                var r = new Rect(box.x + u + (i % 4) * (cell + u * 0.4f), box.y + u * 4.4f + (i / 4) * (cell + u * 0.4f), cell, cell);
                var id = items[i].Key;
                var item = Item.Find(id);
                Fill(r, selected == id ? Alpha(accent, 0.35f) : new Color(1f, 1f, 1f, 0.07f));
                if (item != null && item.icon != null)
                    GUI.DrawTexture(new Rect(r.x + u * 0.4f, r.y + u * 0.3f, r.width - u * 0.8f, r.height - u * 1.5f), item.icon.texture, ScaleMode.ScaleToFit);
                else
                    Text(new Rect(r.x + u * 0.2f, r.y, r.width - u * 0.4f, r.height - u), Item.Title(id), u * 0.7f, Color.white, TextAnchor.MiddleCenter, true);
                if (items[i].Value > 1)
                    Text(new Rect(r.x, r.yMax - u * 1.2f, r.width - u * 0.3f, u), "x" + items[i].Value, u * 0.75f, accent, TextAnchor.LowerRight);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none))
                    selected = id;
            }
            if (items.Count == 0)
                Text(new Rect(box.x + u, box.y + u * 4.4f, u * 18f, u * 2f), "Nothing carried.", u * 0.8f, new Color(1f, 1f, 1f, 0.5f), TextAnchor.UpperLeft);
            var side = new Rect(box.x + u * 20f, box.y + u * 4.4f, u * 15f, u * 14f);
            if (selected != null)
            {
                var item = Item.Find(selected);
                Text(new Rect(side.x, side.y, side.width, u * 1.4f), Item.Title(selected), u, accent, TextAnchor.UpperLeft);
                Text(new Rect(side.x, side.y + u * 1.6f, side.width, u * 7f), item != null ? item.description : "", u * 0.8f, Color.white, TextAnchor.UpperLeft, true);
                var bx = side.x;
                var usable = item != null && (item.kind == Item.Kind.Heal || item.kind == Item.Kind.Battery || item.kind == Item.Kind.Weapon);
                var verb = item == null || item.kind != Item.Kind.Weapon ? "USE" : Run.Equipped == selected ? "PUT AWAY" : "EQUIP";
                if (usable && Button(new Rect(bx, side.y + u * 9f, u * 6f, u * 1.6f), verb))
                {
                    if (!Run.Use(selected))
                        Run.Say(item.kind == Item.Kind.Heal ? "Health is full." : "The battery is full.");
                }
                if (usable)
                    bx += u * 6.5f;
                if (item != null && Button(new Rect(bx, side.y + u * 9f, u * 7f, u * 1.6f), item.kind == Item.Kind.Document ? "READ" : "EXAMINE"))
                    OpenExamine(item, item.kind == Item.Kind.Document && item.model == null ? null : item.model, item.kind == Item.Kind.Document ? item.text : item.description);
            }
            if (showBars)
            {
                Bar(new Rect(box.x + u, box.yMax - u * 2.6f, u * 12f, u * 0.5f), "HEALTH", Run.Health);
                if (Flashlight.Current != null)
                    Bar(new Rect(box.x + u * 14f, box.yMax - u * 2.6f, u * 12f, u * 0.5f), "BATTERY", Run.Battery);
            }
            Text(new Rect(box.x, box.yMax - u * 1.3f, box.width - u, u), "Tab or Esc to close", u * 0.65f, new Color(1f, 1f, 1f, 0.45f), TextAnchor.UpperRight);
        }

        void DrawExamine()
        {
            float w = Screen.width, h = Screen.height;
            Fill(new Rect(0, 0, w, h), new Color(0f, 0f, 0f, 0.82f));
            var text = pages.Length > 0 ? pages[Mathf.Clamp(page, 0, pages.Length - 1)] : "";
            var hasCopy = examine.Showing && examine.Picture != null;
            if (hasCopy)
                GUI.DrawTexture(new Rect(0, text != "" ? -h * 0.1f : 0f, w, h), examine.Picture, ScaleMode.StretchToFill, true);
            Text(new Rect(0, h * 0.05f, w, u * 2f), examineTitle, u * 1.4f, accent, TextAnchor.UpperCenter);
            var area = hasCopy ? new Rect(w * 0.15f, h * 0.7f, w * 0.7f, h * 0.2f) : new Rect(w * 0.2f, h * 0.15f, w * 0.6f, h * 0.68f);
            Text(area, text, u * (hasCopy ? 0.9f : 1f), Color.white, hasCopy ? TextAnchor.UpperCenter : TextAnchor.UpperLeft, true);
            var help = hasCopy ? "drag to turn, wheel to zoom" : "";
            if (pages.Length > 1)
            {
                var pr = new Rect(w / 2f - u * 6f, h * 0.9f, u * 12f, u * 1.6f);
                if (page > 0 && Button(new Rect(pr.x, pr.y, u * 2f, pr.height), "<")) page--;
                Text(pr, $"{page + 1} / {pages.Length}", u * 0.8f, Color.white, TextAnchor.MiddleCenter);
                if (page < pages.Length - 1 && Button(new Rect(pr.xMax - u * 2f, pr.y, u * 2f, pr.height), ">")) page++;
                help = "A and D or Enter to turn pages";
            }
            Text(new Rect(0, h - u * 1.4f, w, u), (help != "" ? help + ", " : "") + "Esc or E to put it away", u * 0.65f, new Color(1f, 1f, 1f, 0.5f), TextAnchor.UpperCenter);
        }

        void DrawCodePad()
        {
            float w = Screen.width, h = Screen.height;
            Fill(new Rect(0, 0, w, h), new Color(0f, 0f, 0f, 0.45f));
            var k = u * 3f;
            var box = new Rect(w / 2f - k * 1.5f - u, h / 2f - k * 2.5f - u * 2f, k * 3f + u * 2f, k * 5f + u * 4f);
            Fill(box, panelColor);
            var display = new Rect(box.x + u, box.y + u, box.width - 2f * u, k * 0.8f);
            Fill(display, new Color(0.02f, 0.05f, 0.03f));
            var shown = padEntry == "OPEN" ? "OPEN" : padEntry.PadRight(pad != null ? pad.code.Length : 4, '_');
            Text(display, string.Join(" ", shown.ToCharArray()), u * 1.3f, padEntry == "OPEN" ? new Color(0.4f, 1f, 0.5f) : accent, TextAnchor.MiddleCenter);
            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "C", "0", "E" };
            for (var i = 0; i < keys.Length; i++)
            {
                var r = new Rect(box.x + u + (i % 3) * k, box.y + u * 2f + k + (i / 3) * k, k - u * 0.3f, k - u * 0.3f);
                if (Button(r, keys[i] == "E" ? "OK" : keys[i]))
                    PadKey(keys[i]);
            }
            Text(new Rect(box.x, box.yMax - u * 1.2f, box.width, u), "digits, Enter, Backspace; Esc to leave", u * 0.55f, new Color(1f, 1f, 1f, 0.45f), TextAnchor.UpperCenter);
        }

        void DrawSlots()
        {
            float w = Screen.width, h = Screen.height;
            Fill(new Rect(0, 0, w, h), new Color(0f, 0f, 0f, 0.6f));
            var box = new Rect(w / 2f - u * 16f, h / 2f - u * 8f, u * 32f, u * 16f);
            Fill(box, panelColor);
            var saving = savePoint != null;
            Text(new Rect(box.x + u, box.y + u * 0.6f, box.width, u * 1.4f), saving ? "SAVE" : "LOAD", u * 1.1f, accent, TextAnchor.UpperLeft);
            for (var s = 0; s < SaveGame.Slots; s++)
            {
                var r = new Rect(box.x + u, box.y + u * 2.8f + s * u * 3.6f, box.width - 2f * u, u * 3.2f);
                var save = slots[s];
                var text = save == null ? $"Slot {s + 1}   empty"
                    : $"Slot {s + 1}   {Json.Get(save, "room", "")}\n<size={(int)(u * 0.7f)}>{Clock(Json.Get(save, "time", 0f))} played, {Json.Get(save, "date", "").Replace('T', ' ')}</size>";
                if (Button(r, text, TextAnchor.MiddleLeft) && (saving || save != null))
                {
                    if (saving)
                    {
                        if (savePoint.SaveTo(s))
                            Open(Panel.None);
                        savePoint = null;
                    }
                    else
                    {
                        Open(Panel.None);
                        SaveGame.Load(s);
                    }
                }
            }
            Text(new Rect(box.x, box.yMax - u * 1.3f, box.width - u, u), "Esc to go back", u * 0.65f, new Color(1f, 1f, 1f, 0.45f), TextAnchor.UpperRight);
        }

        void DrawPause()
        {
            float w = Screen.width, h = Screen.height;
            Fill(new Rect(0, 0, w, h), new Color(0f, 0f, 0f, 0.7f));
            Text(new Rect(0, h * 0.25f, w, u * 2f), "PAUSED", u * 1.6f, accent, TextAnchor.UpperCenter);
            if (Run.Objective != "")
                Text(new Rect(w * 0.2f, h * 0.25f + u * 2.4f, w * 0.6f, u * 2f), Run.Objective, u * 0.85f, Color.white, TextAnchor.UpperCenter, true);
            var x = w / 2f - u * 6f;
            var y = h * 0.45f;
            if (Button(new Rect(x, y, u * 12f, u * 1.8f), "RESUME")) Open(Panel.None);
            if (Button(new Rect(x, y + u * 2.2f, u * 12f, u * 1.8f), "LOAD")) { savePoint = null; Open(Panel.Slots); }
            if (Button(new Rect(x, y + u * 4.4f, u * 12f, u * 1.8f), "RESTART")) { Open(Panel.None); SaveGame.Restart(); }
            if (Button(new Rect(x, y + u * 6.6f, u * 12f, u * 1.8f), "QUIT")) Application.Quit();
        }

        void DrawEnd()
        {
            float w = Screen.width, h = Screen.height;
            Fill(new Rect(0, 0, w, h), new Color(0f, 0f, 0f, 0.85f));
            Text(new Rect(0, h * 0.3f, w, u * 3f), "ESCAPED", u * 2.4f, accent, TextAnchor.UpperCenter);
            Text(new Rect(0, h * 0.3f + u * 3.4f, w, u * 1.4f), $"in {Clock(Run.PlayTime)}", u, Color.white, TextAnchor.UpperCenter);
            if (Button(new Rect(w / 2f - u * 6f, h * 0.55f, u * 12f, u * 1.8f), "PLAY AGAIN"))
            {
                Open(Panel.None);
                SaveGame.Restart();
            }
        }

        // --- helpers -------------------------------------------------------------------------------

        static string Clock(float seconds)
        {
            var t = TimeSpan.FromSeconds(seconds);
            return t.TotalHours >= 1 ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}" : $"{t.Minutes}:{t.Seconds:00}";
        }

        static string Pretty(string name) => name.Replace('_', ' ').Trim();

        static string Strip(string rich) => System.Text.RegularExpressions.Regex.Replace(rich, "<[^>]+>", "");

        static Color Alpha(Color c, float a) => new Color(c.r, c.g, c.b, a);

        void Fill(Rect r, Color c, Texture2D tex = null)
        {
            var was = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, tex != null ? tex : Texture2D.whiteTexture);
            GUI.color = was;
        }

        void Text(Rect r, string text, float size, Color color, TextAnchor anchor, bool wrap = false)
        {
            label.fontSize = Mathf.Max(8, (int)size);
            label.alignment = anchor;
            label.wordWrap = wrap;
            label.normal.textColor = color;
            GUI.Label(r, text, label);
        }

        bool Button(Rect r, string text, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var hover = r.Contains(Event.current.mousePosition);
            Fill(r, hover ? Alpha(accent, 0.3f) : new Color(1f, 1f, 1f, 0.08f));
            Text(new Rect(r.x + u * 0.5f, r.y, r.width - u, r.height), text, u * 0.85f, hover ? accent : Color.white, anchor, true);
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        void Bar(Rect r, string name, float value)
        {
            Text(new Rect(r.x, r.y - u * 1.1f, r.width, u), name, u * 0.6f, new Color(1f, 1f, 1f, 0.6f), TextAnchor.LowerLeft);
            Fill(r, new Color(1f, 1f, 1f, 0.12f));
            Fill(new Rect(r.x, r.y, r.width * Mathf.Clamp01(value), r.height), value < 0.25f ? new Color(0.9f, 0.3f, 0.25f) : accent);
        }
    }
}
