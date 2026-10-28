using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>
    /// JSON to and from plain values: Dictionary&lt;string, object&gt;, List&lt;object&gt;, string, double, bool
    /// and null (any other number is written as one). What a save is made of; JsonUtility takes no
    /// dictionaries.
    /// </summary>
    public static class Json
    {
        public static string Write(object value)
        {
            var sb = new StringBuilder();
            Write(sb, value, 0);
            return sb.ToString();
        }

        public static object Read(string text)
        {
            var at = 0;
            var value = Value(text, ref at);
            Space(text, ref at);
            if (at != text.Length)
                throw new FormatException($"JSON: more after the value at {at}");
            return value;
        }

        /// <summary>A saved value read back as T (numbers come back as double, lists as List&lt;object&gt;), or the fallback.</summary>
        public static T Get<T>(IDictionary<string, object> state, string key, T fallback)
        {
            if (state == null || !state.TryGetValue(key, out var v) || v == null)
                return fallback;
            if (v is T t)
                return t;
            if (typeof(T).IsEnum && v is string s)
                return Enum.TryParse(typeof(T), s, true, out var e) ? (T)e : fallback;
            try { return (T)Convert.ChangeType(v, typeof(T), CultureInfo.InvariantCulture); }
            catch (Exception) { return fallback; }
        }

        static void Write(StringBuilder sb, object v, int depth)
        {
            switch (v)
            {
                case null: sb.Append("null"); break;
                case string s: Quote(sb, s); break;
                case bool b: sb.Append(b ? "true" : "false"); break;
                case Enum e: Quote(sb, e.ToString().ToLowerInvariant()); break;
                case IDictionary d:
                {
                    if (d.Count == 0) { sb.Append("{}"); break; }
                    sb.Append("{\n");
                    var first = true;
                    foreach (DictionaryEntry entry in d)
                    {
                        if (!first) sb.Append(",\n");
                        first = false;
                        sb.Append(' ', 2 * depth + 2);
                        Quote(sb, entry.Key.ToString());
                        sb.Append(": ");
                        Write(sb, entry.Value, depth + 1);
                    }
                    sb.Append('\n').Append(' ', 2 * depth).Append('}');
                    break;
                }
                case IEnumerable list:
                {
                    sb.Append('[');
                    var first = true;
                    foreach (var x in list)
                    {
                        if (!first) sb.Append(", ");
                        first = false;
                        Write(sb, x, depth + 1);
                    }
                    sb.Append(']');
                    break;
                }
                case float f: sb.Append(f.ToString("R", CultureInfo.InvariantCulture)); break;
                case IFormattable n: sb.Append(n.ToString(null, CultureInfo.InvariantCulture)); break;
                default: Quote(sb, v.ToString()); break;
            }
        }

        static void Quote(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (var c in s)
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ') sb.Append("\\u").Append(((int)c).ToString("x4")); else sb.Append(c);
                        break;
                }
            sb.Append('"');
        }

        static void Space(string t, ref int i)
        {
            while (i < t.Length && char.IsWhiteSpace(t[i])) i++;
        }

        static object Value(string t, ref int i)
        {
            Space(t, ref i);
            if (i >= t.Length)
                throw new FormatException("JSON: ends early");
            switch (t[i])
            {
                case '{':
                {
                    var d = new Dictionary<string, object>();
                    i++;
                    Space(t, ref i);
                    if (i < t.Length && t[i] == '}') { i++; return d; }
                    while (true)
                    {
                        Space(t, ref i);
                        var key = String(t, ref i);
                        Space(t, ref i);
                        Expect(t, ref i, ':');
                        d[key] = Value(t, ref i);
                        Space(t, ref i);
                        if (i < t.Length && t[i] == ',') { i++; continue; }
                        Expect(t, ref i, '}');
                        return d;
                    }
                }
                case '[':
                {
                    var l = new List<object>();
                    i++;
                    Space(t, ref i);
                    if (i < t.Length && t[i] == ']') { i++; return l; }
                    while (true)
                    {
                        l.Add(Value(t, ref i));
                        Space(t, ref i);
                        if (i < t.Length && t[i] == ',') { i++; continue; }
                        Expect(t, ref i, ']');
                        return l;
                    }
                }
                case '"': return String(t, ref i);
                case 't': Word(t, ref i, "true"); return true;
                case 'f': Word(t, ref i, "false"); return false;
                case 'n': Word(t, ref i, "null"); return null;
                default:
                {
                    var start = i;
                    while (i < t.Length && "+-0123456789.eE".IndexOf(t[i]) >= 0) i++;
                    if (start == i)
                        throw new FormatException($"JSON: unexpected '{t[i]}' at {i}");
                    return double.Parse(t.Substring(start, i - start), CultureInfo.InvariantCulture);
                }
            }
        }

        static string String(string t, ref int i)
        {
            Expect(t, ref i, '"');
            var sb = new StringBuilder();
            while (i < t.Length && t[i] != '"')
            {
                var c = t[i++];
                if (c != '\\') { sb.Append(c); continue; }
                c = t[i++];
                switch (c)
                {
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u': sb.Append((char)Convert.ToInt32(t.Substring(i, 4), 16)); i += 4; break;
                    default: sb.Append(c); break;
                }
            }
            Expect(t, ref i, '"');
            return sb.ToString();
        }

        static void Expect(string t, ref int i, char c)
        {
            if (i >= t.Length || t[i] != c)
                throw new FormatException($"JSON: '{c}' expected at {i}");
            i++;
        }

        static void Word(string t, ref int i, string w)
        {
            if (string.CompareOrdinal(t, i, w, 0, w.Length) != 0)
                throw new FormatException($"JSON: '{w}' expected at {i}");
            i += w.Length;
        }
    }
}
