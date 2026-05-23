#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Base.ToolPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Pure text-based scanner for static fields that are not reset on Enter Play Mode.
    /// </summary>
    public static class StaticResetScanner
    {
        private static readonly HashSet<string> Modifiers = new()
            { "readonly", "volatile", "unsafe", "extern", "event" };

        private static readonly HashSet<string> Keywords = new()
        {
            "static", "public", "private", "protected", "internal", "readonly", "volatile", "unsafe",
            "extern", "event", "new", "abstract", "virtual", "override", "sealed", "async", "partial",
            "const", "ref", "out", "in", "params", "this", "base", "return", "void", "var", "dynamic",
            "int", "uint", "long", "ulong", "short", "ushort", "byte", "sbyte", "float", "double", "decimal",
            "bool", "char", "string", "object", "nint", "nuint", "delegate", "enum", "struct", "class",
            "interface", "record", "namespace", "using", "if", "else", "for", "foreach", "while", "do",
            "switch", "case", "default", "break", "continue", "throw", "try", "catch", "finally", "lock",
            "fixed", "checked", "unchecked", "typeof", "sizeof", "nameof", "true", "false", "null",
            "operator", "implicit", "explicit", "where", "get", "set", "init", "value", "yield", "await",
            "global", "is", "as", "when", "stackalloc", "goto", "add", "remove"
        };

        public static List<Finding> Scan(ScanOptions opt, out int filesScanned)
        {
            List<Finding> results = new();
            filesScanned = 0;

            DirectoryInfo dataDir = Directory.GetParent(Application.dataPath);
            if (dataDir == null)
                throw new DirectoryNotFoundException("Could not find project root from data path: " +
                                                     Application.dataPath);

            string projectRoot = dataDir.FullName;
            string absRoot = Path.IsPathRooted(opt.RootFolder)
                ? opt.RootFolder
                : Path.Combine(projectRoot, opt.RootFolder);

            if (!Directory.Exists(absRoot))
                throw new DirectoryNotFoundException("Folder not found: " + absRoot);

            foreach (string path in Directory.GetFiles(absRoot, "*.cs", SearchOption.AllDirectories))
            {
                string norm = path.Replace('\\', '/');
                if (opt.SkipEditorFolders && norm.IndexOf("/Editor/", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                string source;
                try
                {
                    source = File.ReadAllText(path);
                }
                catch
                {
                    continue;
                }

                if (source.IndexOf("static", StringComparison.Ordinal) < 0)
                    continue;

                filesScanned++;
                ScanFile(source, ToAssetPath(path), opt, results);
            }

            return results
                .GroupBy(f => f.AssetPath + "|" + f.Line + "|" + f.Name)
                .Select(g => g.First())
                .ToList();
        }

        private static void ScanFile(string source, string assetPath, ScanOptions opt, List<Finding> results)
        {
            Context ctx = new()
            {
                Cleaned = CleanSource(source),
                LineStarts = BuildLineStarts(source),
                Opt = opt
            };

            foreach (Match m in Regex.Matches(ctx.Cleaned, @"\bstatic\b"))
            {
                int pos = m.Index;
                if (PrecededByWord(ctx.Cleaned, pos, "using"))
                    continue;

                ProcessStatic(ctx, pos);
            }

            if (ctx.Fields.Count == 0)
                return;

            string resetText = BuildResetSearchText(ctx);

            foreach (FieldHit f in ctx.Fields)
            {
                bool reset = resetText.Length > 0 && Regex.IsMatch(resetText, $@"\b{Regex.Escape(f.Name)}\b");
                if (reset)
                    continue;

                int line = LineFromIndex(ctx.LineStarts, f.Index);
                string lineText = GetLineText(source, ctx.LineStarts, line);
                if (!string.IsNullOrEmpty(opt.IgnoreMarker) && lineText.Contains(opt.IgnoreMarker))
                    continue;

                results.Add(new Finding
                {
                    AssetPath = assetPath,
                    Line = line,
                    Name = f.Name,
                    Kind = f.Kind,
                    Snippet = lineText.Trim()
                });
            }
        }

        private static string BuildResetSearchText(Context ctx)
        {
            StringBuilder sb = new();
            foreach (string b in ctx.ResetBodies) sb.Append('\n').Append(b);

            if (!ctx.Opt.ExpandHelpers || ctx.ResetBodies.Count <= 0)
                return sb.ToString();

            HashSet<string> seen = new();
            List<string> frontier = new(ctx.ResetBodies);
            for (int depth = 0; depth < 4 && frontier.Count > 0; depth++)
            {
                List<string> next = new();
                foreach (string body in frontier)
                foreach (Match call in Regex.Matches(body, @"\b(\w+)\s*\("))
                {
                    string name = call.Groups[1].Value;
                    if (!ctx.StaticMethods.TryGetValue(name, out string item) || !seen.Add(name))
                        continue;

                    sb.Append('\n').Append(item);
                    next.Add(item);
                }

                frontier = next;
            }

            return sb.ToString();
        }

        private static void ProcessStatic(Context ctx, int pos)
        {
            string s = ctx.Cleaned;
            int n = s.Length;
            int i = pos + "static".Length;

            int angle = 0, bracket = 0, parentheses = 0;

            while (i < n)
            {
                char c = s[i];

                switch (c)
                {
                    case '<':
                    {
                        angle++;
                        i++;
                        continue;
                    }
                    case '>':
                    {
                        if (angle > 0)
                            angle--;

                        i++;
                        continue;
                    }
                    case '[':
                    {
                        bracket++;
                        i++;
                        continue;
                    }
                    case ']':
                    {
                        if (bracket > 0)
                            bracket--;

                        i++;
                        continue;
                    }
                }

                if (angle > 0 || bracket > 0)
                {
                    i++;
                    continue;
                }

                switch (c)
                {
                    case '(' when parentheses == 0 && LooksLikeMethodParen(s, i):
                    {
                        HandleMethod(ctx, pos, i);
                        return;
                    }
                    case '(':
                    {
                        parentheses++;
                        i++;
                        continue;
                    }
                    case ')':
                    {
                        if (parentheses > 0)
                            parentheses--;

                        i++;
                        continue;
                    }
                }

                if (parentheses > 0)
                {
                    i++;
                    continue;
                }

                switch (c)
                {
                    case '{':
                    {
                        HandleBlockMember(ctx, pos, i);
                        return;
                    }
                    case '=' when i + 1 < n && s[i + 1] == '>':
                    {
                        return;
                    }
                    case '=':
                    {
                        int sc = FindTopLevelSemicolon(s, i + 1);
                        EmitField(ctx, pos, s.Substring(pos, sc - pos));
                        return;
                    }
                    case ';':
                    {
                        EmitField(ctx, pos, s.Substring(pos, i - pos));
                        return;
                    }
                    default:
                    {
                        i++;
                        break;
                    }
                }
            }
        }

        private static bool LooksLikeMethodParen(string s, int parenIndex)
        {
            char prev = PrevNonSpace(s, parenIndex - 1);
            if (prev == '>')
                return true;

            string id = ReadIdentifierBefore(s, parenIndex);
            return id != null && !IsKeyword(id);
        }

        private static void HandleMethod(Context ctx, int pos, int parenIndex)
        {
            string s = ctx.Cleaned;
            int closeParen = MatchPair(s, parenIndex, '(', ')');
            int n = s.Length;

            int b = closeParen;
            while (b < n)
            {
                char c = s[b];
                if (c == '{')
                    break;

                if (c == ';')
                    return;

                if (c == '=' && b + 1 < n && s[b + 1] == '>')
                    break;

                b++;
            }

            if (b >= n)
                return;

            string body;
            if (s[b] == '{')
            {
                int end = MatchPair(s, b, '{', '}');
                body = s.Substring(b, end - b);
            }
            else
            {
                int sc = FindTopLevelSemicolon(s, b + 2);
                body = s.Substring(b + 2, Math.Max(0, sc - (b + 2)));
            }

            string name = ReadIdentifierBefore(s, parenIndex);
            if (!string.IsNullOrEmpty(name))
                ctx.StaticMethods.TryAdd(name, body);

            if (IsResetMethod(ctx, pos))
                ctx.ResetBodies.Add(body);
        }

        private static void HandleBlockMember(Context ctx, int pos, int braceIndex)
        {
            string s = ctx.Cleaned;
            string head = s.Substring(pos, braceIndex - pos);

            if (Regex.IsMatch(head, @"\b(class|struct|interface|enum|record|namespace)\b"))
                return;

            if (!ctx.Opt.IncludeAutoProperties)
                return;

            int end = MatchPair(s, braceIndex, '{', '}');
            string block = s.Substring(braceIndex, end - braceIndex);

            bool isAuto = Regex.IsMatch(block, @"\b(get|set|init)\s*;");
            if (!isAuto)
                return;

            string name = LastIdentifier(head);
            if (string.IsNullOrEmpty(name) || IsKeyword(name))
                return;

            ctx.Fields.Add(new FieldHit
            {
                Index = AbsoluteNameIndex(pos, head, name),
                Name = name,
                Kind = "static property"
            });
        }

        private static void EmitField(Context ctx, int pos, string full)
        {
            string body = full["static".Length..];

            body = StripLeadingModifiers(body, out bool isEvent);
            if (isEvent && !ctx.Opt.IncludeEvents)
                return;

            List<string> declarators = SplitTopLevel(body, ',');
            for (int d = 0; d < declarators.Count; d++)
            {
                string decl = declarators[d];
                int eq = IndexOfTopLevelAssign(decl);
                string left = eq >= 0 ? decl[..eq] : decl;

                string name = d == 0 ? LastIdentifier(left) : FirstIdentifier(left);
                if (string.IsNullOrEmpty(name) || IsKeyword(name))
                    continue;

                ctx.Fields.Add(new FieldHit
                {
                    Index = AbsoluteNameIndex(pos, full, name),
                    Name = name,
                    Kind = isEvent ? "static event" : "static field"
                });
            }
        }

        private static bool IsResetMethod(Context ctx, int pos)
        {
            string s = ctx.Cleaned;
            int j = pos - 1;
            while (j >= 0)
            {
                char c = s[j];
                if (c is '}' or '{' or ';')
                    break;
                j--;
            }

            string prefix = s.Substring(j + 1, pos - (j + 1));
            foreach (string attr in ctx.Opt.ResetAttributes)
                if (Regex.IsMatch(prefix, $@"\b{Regex.Escape(attr)}\b"))
                    return true;

            return false;
        }

        private static string CleanSource(string s)
        {
            StringBuilder sb = new(s.Length);
            int i = 0, n = s.Length;
            while (i < n)
            {
                char c = s[i];

                switch (c)
                {
                    case '/' when i + 1 < n && s[i + 1] == '/':
                    {
                        while (i < n && s[i] != '\n')
                        {
                            sb.Append(' ');
                            i++;
                        }

                        continue;
                    }
                    case '/' when i + 1 < n && s[i + 1] == '*':
                    {
                        sb.Append("  ");
                        i += 2;
                        while (i < n && !(s[i] == '*' && i + 1 < n && s[i + 1] == '/'))
                        {
                            sb.Append(s[i] == '\n' ? '\n' : ' ');
                            i++;
                        }

                        if (i < n)
                        {
                            sb.Append("  ");
                            i += 2;
                        }

                        continue;
                    }
                    case '@':
                    case '$':
                    {
                        int j = i;
                        bool verbatim = false;
                        while (j < n && (s[j] == '@' || s[j] == '$'))
                        {
                            if (s[j] == '@') verbatim = true;
                            j++;
                        }

                        if (j < n && s[j] == '"')
                        {
                            for (int k = i; k < j; k++) sb.Append(' ');
                            i = BlankString(s, j, verbatim, sb);
                            continue;
                        }

                        sb.Append(c);
                        i++;
                        continue;
                    }
                    case '"':
                        i = BlankString(s, i, false, sb);
                        continue;
                    case '\'':
                        i = BlankChar(s, i, sb);
                        continue;
                    default:
                        sb.Append(c);
                        i++;
                        break;
                }
            }

            return sb.ToString();
        }

        private static int BlankString(string s, int i, bool verbatim, StringBuilder sb)
        {
            int n = s.Length;
            sb.Append(' ');
            i++;
            while (i < n)
            {
                char c = s[i];
                if (verbatim)
                {
                    if (c == '"')
                    {
                        if (i + 1 < n && s[i + 1] == '"')
                        {
                            sb.Append("  ");
                            i += 2;
                            continue;
                        }

                        sb.Append(' ');
                        i++;
                        return i;
                    }

                    sb.Append(c == '\n' ? '\n' : ' ');
                    i++;
                }
                else
                {
                    switch (c)
                    {
                        case '\\' when i + 1 < n:
                        {
                            sb.Append("  ");
                            i += 2;
                            continue;
                        }
                        case '"':
                        {
                            sb.Append(' ');
                            i++;
                            return i;
                        }
                        case '\n':
                        {
                            sb.Append('\n');
                            i++;
                            return i;
                        }
                        default:
                        {
                            sb.Append(' ');
                            i++;
                            break;
                        }
                    }
                }
            }

            return i;
        }

        private static int BlankChar(string s, int i, StringBuilder sb)
        {
            int n = s.Length;
            sb.Append(' ');
            i++;
            while (i < n)
            {
                char c = s[i];
                switch (c)
                {
                    case '\\' when i + 1 < n:
                    {
                        sb.Append("  ");
                        i += 2;
                        continue;
                    }
                    case '\'':
                    {
                        sb.Append(' ');
                        i++;
                        return i;
                    }
                    case '\n':
                    {
                        sb.Append('\n');
                        i++;
                        return i;
                    }
                    default:
                    {
                        sb.Append(' ');
                        i++;
                        break;
                    }
                }
            }

            return i;
        }

        private static int MatchPair(string s, int openIdx, char open, char close)
        {
            int depth = 0;
            for (int i = openIdx; i < s.Length; i++)
            {
                if (s[i] == open)
                {
                    depth++;
                }
                else if (s[i] == close)
                {
                    depth--;
                    if (depth == 0)
                        return i + 1;
                }
            }

            return s.Length;
        }

        private static int FindTopLevelSemicolon(string s, int start)
        {
            int p = 0, b = 0, c = 0;
            for (int i = start; i < s.Length; i++)
            {
                char ch = s[i];
                switch (ch)
                {
                    case '(':
                    {
                        p++;
                        break;
                    }
                    case ')':
                    {
                        if (p > 0) p--;
                        break;
                    }
                    case '[':
                    {
                        b++;
                        break;
                    }
                    case ']':
                    {
                        if (b > 0) b--;
                        break;
                    }
                    case '{':
                    {
                        c++;
                        break;
                    }
                    case '}':
                    {
                        if (c > 0) c--;
                        break;
                    }
                    case ';':
                    {
                        if (p == 0 && b == 0 && c == 0)
                            return i;

                        break;
                    }
                }
            }

            return s.Length - 1;
        }

        private static List<string> SplitTopLevel(string s, char sep)
        {
            List<string> res = new();
            int a = 0, p = 0, b = 0, c = 0, last = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                switch (ch)
                {
                    case '<':
                        a++;
                        break;
                    case '>':
                        if (a > 0) a--;
                        break;
                    case '(':
                        p++;
                        break;
                    case ')':
                        if (p > 0) p--;
                        break;
                    case '[':
                        b++;
                        break;
                    case ']':
                        if (b > 0) b--;
                        break;
                    case '{':
                        c++;
                        break;
                    case '}':
                        if (c > 0) c--;
                        break;
                }

                if (ch != sep || a != 0 || p != 0 || b != 0 || c != 0)
                    continue;

                res.Add(s.Substring(last, i - last));
                last = i + 1;
            }

            res.Add(s[last..]);
            return res;
        }

        private static int IndexOfTopLevelAssign(string s)
        {
            int a = 0, p = 0, b = 0, c = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                switch (ch)
                {
                    case '<':
                    {
                        a++;
                        break;
                    }
                    case '>':
                    {
                        if (a > 0)
                            a--;
                        break;
                    }
                    case '(':
                    {
                        p++;
                        break;
                    }
                    case ')':
                    {
                        if (p > 0)
                            p--;
                        break;
                    }
                    case '[':
                    {
                        b++;
                        break;
                    }
                    case ']':
                    {
                        if (b > 0)
                            b--;
                        break;
                    }
                    case '{':
                    {
                        c++;
                        break;
                    }
                    case '}':
                    {
                        if (c > 0)
                            c--;
                        break;
                    }
                }

                if (ch != '=' || a != 0 || p != 0 || b != 0 || c != 0)
                    continue;

                char nx = i + 1 < s.Length ? s[i + 1] : '\0';
                char pv = i > 0 ? s[i - 1] : '\0';
                if (nx == '>' || nx == '=' ||
                    pv == '=' || pv == '!' || pv == '<' || pv == '>' ||
                    pv == '+' || pv == '-' || pv == '*' || pv == '/' ||
                    pv == '%' || pv == '&' || pv == '|' || pv == '^')
                    continue;
                return i;
            }

            return -1;
        }

        private static string StripLeadingModifiers(string s, out bool isEvent)
        {
            isEvent = false;
            while (true)
            {
                int i = 0;
                while (i < s.Length && char.IsWhiteSpace(s[i]))
                    i++;

                int start = i;
                while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_'))
                    i++;

                if (i == start)
                    break;

                string tok = s.Substring(start, i - start);
                if (!Modifiers.Contains(tok))
                    break;

                if (tok == "event")
                    isEvent = true;

                s = s[i..];
            }

            return s;
        }

        private static string LastIdentifier(string s)
        {
            s = s.TrimEnd();
            int j = s.Length - 1;
            if (j < 0 || !(char.IsLetterOrDigit(s[j]) || s[j] == '_'))
                return null;

            int k = j;
            while (k >= 0 && (char.IsLetterOrDigit(s[k]) || s[k] == '_'))
                k--;

            return s.Substring(k + 1, j - k);
        }

        private static string FirstIdentifier(string s)
        {
            int i = 0;
            while (i < s.Length && !(char.IsLetterOrDigit(s[i]) || s[i] == '_'))
                i++;

            int st = i;
            while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_'))
                i++;

            return i > st ? s.Substring(st, i - st) : null;
        }

        private static string ReadIdentifierBefore(string s, int index)
        {
            int i = index - 1;
            while (i >= 0 && char.IsWhiteSpace(s[i]))
                i--;

            if (i >= 0 && s[i] == '>')
            {
                int depth = 0;
                while (i >= 0)
                {
                    if (s[i] == '>')
                    {
                        depth++;
                    }
                    else if (s[i] == '<')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            i--;
                            break;
                        }
                    }

                    i--;
                }

                while (i >= 0 && char.IsWhiteSpace(s[i]))
                    i--;
            }

            int end = i + 1, k = i;
            while (k >= 0 && (char.IsLetterOrDigit(s[k]) || s[k] == '_'))
                k--;

            return end - (k + 1) > 0
                ? s.Substring(k + 1, end - (k + 1))
                : null;
        }

        private static char PrevNonSpace(string s, int idx)
        {
            while (idx >= 0 && char.IsWhiteSpace(s[idx]))
                idx--;

            return idx >= 0 ? s[idx] : '\0';
        }

        private static bool PrecededByWord(string s, int pos, string word)
        {
            int i = pos - 1;
            while (i >= 0 && char.IsWhiteSpace(s[i]))
                i--;

            int end = i + 1, k = i;
            while (k >= 0 && (char.IsLetterOrDigit(s[k]) || s[k] == '_'))
                k--;

            return end - (k + 1) > 0 && s.Substring(k + 1, end - (k + 1)) == word;
        }

        private static int AbsoluteNameIndex(int pos, string text, string name)
        {
            MatchCollection matches = Regex.Matches(text, $@"\b{Regex.Escape(name)}\b");
            return matches.Count > 0 ? pos + matches[^1].Index : pos;
        }

        private static bool IsKeyword(string s) => Keywords.Contains(s);

        private static int[] BuildLineStarts(string s)
        {
            List<int> list = new() { 0 };
            for (int i = 0; i < s.Length; i++)
                if (s[i] == '\n')
                    list.Add(i + 1);
            return list.ToArray();
        }

        private static int LineFromIndex(int[] lineStarts, int index)
        {
            int lo = 0, hi = lineStarts.Length - 1, ans = 0;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                if (lineStarts[mid] <= index)
                {
                    ans = mid;
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            return ans + 1;
        }

        private static string GetLineText(string source, int[] lineStarts, int line1Based)
        {
            int idx = line1Based - 1;
            if (idx < 0 || idx >= lineStarts.Length)
                return "";

            int start = lineStarts[idx];
            int end = idx + 1 < lineStarts.Length ? lineStarts[idx + 1] : source.Length;
            string text = source.Substring(start, end - start).TrimEnd('\r', '\n');
            return text.Length > 200 ? text[..200] : text;
        }

        private static string ToAssetPath(string absolute)
        {
            string abs = absolute.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/');
            if (abs.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
                return "Assets" + abs[dataPath.Length..];
            return abs;
        }
    }
}
#endif