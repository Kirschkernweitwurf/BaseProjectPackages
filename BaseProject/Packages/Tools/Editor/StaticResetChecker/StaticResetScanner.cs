using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Base.ToolsPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Pure text-based scanner for static fields that are not reset on Enter Play Mode.
    /// </summary>
    internal static class StaticResetScanner
    {
        // How far a reset method is followed into the helpers it calls. Each hop is another pass over
        // every body found so far, and a clearing helper five calls deep is not a real shape.
        private const int HelperExpansionDepth = 4;

        // Longest declaration line kept for the report. A generated or minified line can run for
        // thousands of characters, which the list cannot show and nobody would read anyway.
        private const int MaxSnippetLength = 200;

        private static readonly HashSet<string> Modifiers = new()
        {
            "readonly",
            "volatile",
            "unsafe",
            "extern",
            "event"
        };

        private static readonly HashSet<string> Keywords = new()
        {
            "static",
            "public",
            "private",
            "protected",
            "internal",
            "readonly",
            "volatile",
            "unsafe",
            "extern",
            "event",
            "new",
            "abstract",
            "virtual",
            "override",
            "sealed",
            "async",
            "partial",
            "const",
            "ref",
            "out",
            "in",
            "params",
            "this",
            "base",
            "return",
            "void",
            "var",
            "dynamic",
            "int",
            "uint",
            "long",
            "ulong",
            "short",
            "ushort",
            "byte",
            "sbyte",
            "float",
            "double",
            "decimal",
            "bool",
            "char",
            "string",
            "object",
            "nint",
            "nuint",
            "delegate",
            "enum",
            "struct",
            "class",
            "interface",
            "record",
            "namespace",
            "using",
            "if",
            "else",
            "for",
            "foreach",
            "while",
            "do",
            "switch",
            "case",
            "default",
            "break",
            "continue",
            "throw",
            "try",
            "catch",
            "finally",
            "lock",
            "fixed",
            "checked",
            "unchecked",
            "typeof",
            "sizeof",
            "nameof",
            "true",
            "false",
            "null",
            "operator",
            "implicit",
            "explicit",
            "where",
            "get",
            "set",
            "init",
            "value",
            "yield",
            "await",
            "global",
            "is",
            "as",
            "when",
            "stackalloc",
            "goto",
            "add",
            "remove"
        };

        /// <summary>
        /// Walks the project for static state that survives leaving play mode. Domain reload is
        /// disabled, so anything not cleared explicitly carries into the next session.
        /// </summary>
        /// <param name="options">The options the scan runs under.</param>
        /// <param name="filesScanned">
        /// How many files were read, so an empty result can be told from an empty scan.
        /// </param>
        /// <returns>One finding per static that is never reset.</returns>
        internal static List<Finding> Scan(ScanOptions options, out int filesScanned)
        {
            List<Finding> results = new();
            filesScanned = 0;

            DirectoryInfo dataDirectory = Directory.GetParent(Application.dataPath);
            if (dataDirectory == null)
                throw new DirectoryNotFoundException("Could not find project root from data path: "
                    + Application.dataPath);

            string projectRoot = dataDirectory.FullName;
            string absoluteRoot = Path.IsPathRooted(options.RootFolder)
                ? options.RootFolder
                : Path.Combine(projectRoot, options.RootFolder);

            if (!Directory.Exists(absoluteRoot))
                throw new DirectoryNotFoundException("Folder not found: " + absoluteRoot);

            PackageInfo[] packages = PackageInfo.GetAllRegisteredPackages();

            foreach (string path in Directory.GetFiles(absoluteRoot, "*.cs", SearchOption.AllDirectories))
            {
                string normalized = path.Replace('\\', '/');
                if (options.SkipEditorFolders
                    && normalized.IndexOf("/Editor/", StringComparison.OrdinalIgnoreCase) >= 0)
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
                ScanFile(source, ToAssetPath(path, packages), normalized, options, results);
            }

            return results
                .GroupBy(finding => finding.AssetPath + "|" + finding.Line + "|" + finding.Name)
                .Select(group => group.First())
                .ToList();
        }

        private static void ScanFile(string source, string assetPath, string absolutePath, ScanOptions options,
            List<Finding> results)
        {
            ScanContext context = new()
            {
                Cleaned = CleanSource(source),
                LineStarts = BuildLineStarts(source),
                Options = options
            };

            foreach (Match match in Regex.Matches(context.Cleaned, @"\bstatic\b"))
            {
                int position = match.Index;
                if (PrecededByWord(context.Cleaned, position, "using"))
                    continue;

                ProcessStatic(context, position);
            }

            if (context.Fields.Count == 0)
                return;

            string resetText = BuildResetSearchText(context);

            foreach (FieldHit hit in context.Fields)
            {
                bool reset = resetText.Length > 0 && Regex.IsMatch(resetText, $@"\b{Regex.Escape(hit.Name)}\b");
                if (reset)
                    continue;

                int line = LineFromIndex(context.LineStarts, hit.Index);
                string lineText = GetLineText(source, context.LineStarts, line);
                if (!string.IsNullOrEmpty(options.IgnoreMarker) && lineText.Contains(options.IgnoreMarker))
                    continue;

                results.Add(new Finding
                {
                    AssetPath = assetPath,
                    AbsolutePath = absolutePath,
                    Line = line,
                    Name = hit.Name,
                    Kind = hit.Kind,
                    Snippet = lineText.Trim()
                });
            }
        }

        private static string BuildResetSearchText(ScanContext context)
        {
            StringBuilder builder = new();
            foreach (string resetBody in context.ResetBodies)
                builder.Append('\n').Append(resetBody);

            if (!context.Options.ExpandHelpers || context.ResetBodies.Count <= 0)
                return builder.ToString();

            HashSet<string> seen = new();
            List<string> frontier = new(context.ResetBodies);
            for (int depth = 0; depth < HelperExpansionDepth && frontier.Count > 0; depth++)
            {
                List<string> next = new();
                foreach (string body in frontier)
                {
                    foreach (Match call in Regex.Matches(body, @"\b(\w+)\s*\("))
                    {
                        string name = call.Groups[1].Value;
                        if (!context.StaticMethods.TryGetValue(name, out string helperBody)
                            || !seen.Add(name))
                            continue;

                        builder.Append('\n').Append(helperBody);
                        next.Add(helperBody);
                    }
                }

                frontier = next;
            }

            return builder.ToString();
        }

        private static void ProcessStatic(ScanContext context, int position)
        {
            string cleaned = context.Cleaned;
            int length = cleaned.Length;
            int index = position + "static".Length;

            int angle = 0, bracket = 0, parentheses = 0;

            while (index < length)
            {
                char current = cleaned[index];

                switch (current)
                {
                    case '<':
                    {
                        angle++;
                        index++;
                        continue;
                    }
                    case '>':
                    {
                        if (angle > 0)
                            angle--;

                        index++;
                        continue;
                    }
                    case '[':
                    {
                        bracket++;
                        index++;
                        continue;
                    }
                    case ']':
                    {
                        if (bracket > 0)
                            bracket--;

                        index++;
                        continue;
                    }
                }

                if (angle > 0 || bracket > 0)
                {
                    index++;
                    continue;
                }

                switch (current)
                {
                    case '(' when parentheses == 0 && LooksLikeMethodParen(cleaned, index):
                    {
                        HandleMethod(context, position, index);
                        return;
                    }
                    case '(':
                    {
                        parentheses++;
                        index++;
                        continue;
                    }
                    case ')':
                    {
                        if (parentheses > 0)
                            parentheses--;

                        index++;
                        continue;
                    }
                }

                if (parentheses > 0)
                {
                    index++;
                    continue;
                }

                switch (current)
                {
                    case '{':
                    {
                        HandleBlockMember(context, position, index);
                        return;
                    }
                    case '=' when index + 1 < length && cleaned[index + 1] == '>':
                    {
                        return;
                    }
                    case '=':
                    {
                        int semicolon = FindTopLevelSemicolon(cleaned, index + 1);
                        EmitField(context, position, cleaned.Substring(position, semicolon - position));
                        return;
                    }
                    case ';':
                    {
                        EmitField(context, position, cleaned.Substring(position, index - position));
                        return;
                    }
                    default:
                    {
                        index++;
                        break;
                    }
                }
            }
        }

        private static bool LooksLikeMethodParen(string cleaned, int parenIndex)
        {
            char previous = PrevNonSpace(cleaned, parenIndex - 1);
            if (previous == '>')
                return true;

            string identifier = ReadIdentifierBefore(cleaned, parenIndex);
            return identifier != null && !IsKeyword(identifier);
        }

        private static void HandleMethod(ScanContext context, int position, int parenIndex)
        {
            string cleaned = context.Cleaned;
            int closeParen = MatchPair(cleaned, parenIndex, '(', ')');
            int length = cleaned.Length;

            int cursor = closeParen;
            while (cursor < length)
            {
                char current = cleaned[cursor];
                if (current == '{')
                    break;

                if (current == ';')
                    return;

                if (current == '='
                    && cursor + 1 < length
                    && cleaned[cursor + 1] == '>')
                    break;

                cursor++;
            }

            if (cursor >= length)
                return;

            string body;
            if (cleaned[cursor] == '{')
            {
                int bodyEnd = MatchPair(cleaned, cursor, '{', '}');
                body = cleaned.Substring(cursor, bodyEnd - cursor);
            }
            else
            {
                int semicolon = FindTopLevelSemicolon(cleaned, cursor + 2);
                body = cleaned.Substring(cursor + 2, Math.Max(0, semicolon - (cursor + 2)));
            }

            string name = ReadIdentifierBefore(cleaned, parenIndex);
            if (!string.IsNullOrEmpty(name))
                context.StaticMethods.TryAdd(name, body);

            if (IsResetMethod(context, position))
                context.ResetBodies.Add(body);
        }

        private static void HandleBlockMember(ScanContext context, int position, int braceIndex)
        {
            string cleaned = context.Cleaned;
            string head = cleaned.Substring(position, braceIndex - position);

            if (Regex.IsMatch(head, @"\b(class|struct|interface|enum|record|namespace)\b"))
                return;

            if (!context.Options.IncludeAutoProperties)
                return;

            int blockEnd = MatchPair(cleaned, braceIndex, '{', '}');
            string block = cleaned.Substring(braceIndex, blockEnd - braceIndex);

            bool isAuto = Regex.IsMatch(block, @"\b(get|set|init)\s*;");
            if (!isAuto)
                return;

            string name = LastIdentifier(head);
            if (string.IsNullOrEmpty(name) || IsKeyword(name))
                return;

            context.Fields.Add(new FieldHit
            {
                Index = AbsoluteNameIndex(position, head, name),
                Name = name,
                Kind = "static property"
            });
        }

        private static void EmitField(ScanContext context, int position, string declaration)
        {
            string body = declaration["static".Length..];

            body = StripLeadingModifiers(body, out bool isEvent, out bool isReadonly);
            if (isEvent && !context.Options.IncludeEvents)
                return;

            if (isReadonly && context.Options.IgnoreReadonly)
                return;

            List<string> declarators = SplitTopLevel(body, ',');
            for (int index = 0; index < declarators.Count; index++)
            {
                string declarator = declarators[index];
                int assign = IndexOfTopLevelAssign(declarator);
                string left = assign >= 0
                    ? declarator[..assign]
                    : declarator;

                string name = index == 0
                    ? LastIdentifier(left)
                    : FirstIdentifier(left);

                if (string.IsNullOrEmpty(name) || IsKeyword(name))
                    continue;

                context.Fields.Add(new FieldHit
                {
                    Index = AbsoluteNameIndex(position, declaration, name),
                    Name = name,
                    Kind = isEvent
                        ? "static event"
                        : "static field"
                });
            }
        }

        private static bool IsResetMethod(ScanContext context, int position)
        {
            string cleaned = context.Cleaned;
            int index = position - 1;
            while (index >= 0)
            {
                char current = cleaned[index];
                if (current is '}' or '{' or ';')
                    break;

                index--;
            }

            string prefix = cleaned.Substring(index + 1, position - (index + 1));
            foreach (string attribute in context.Options.ResetAttributes)
            {
                if (Regex.IsMatch(prefix, $@"\b{Regex.Escape(attribute)}\b"))
                    return true;
            }

            return false;
        }

        private static string CleanSource(string source)
        {
            StringBuilder builder = new(source.Length);
            int index = 0, length = source.Length;
            while (index < length)
            {
                char current = source[index];

                switch (current)
                {
                    case '/' when index + 1 < length && source[index + 1] == '/':
                    {
                        while (index < length && source[index] != '\n')
                        {
                            builder.Append(' ');
                            index++;
                        }

                        continue;
                    }
                    case '/' when index + 1 < length && source[index + 1] == '*':
                    {
                        builder.Append("  ");
                        index += 2;
                        while (index < length
                               && !(source[index] == '*' && index + 1 < length && source[index + 1] == '/'))
                        {
                            builder.Append(source[index] == '\n'
                                ? '\n'
                                : ' ');

                            index++;
                        }

                        if (index < length)
                        {
                            builder.Append("  ");
                            index += 2;
                        }

                        continue;
                    }
                    case '@':
                    case '$':
                    {
                        int prefixEnd = index;
                        bool verbatim = false;
                        while (prefixEnd < length && (source[prefixEnd] == '@' || source[prefixEnd] == '$'))
                        {
                            if (source[prefixEnd] == '@')
                                verbatim = true;

                            prefixEnd++;
                        }

                        if (prefixEnd < length && source[prefixEnd] == '"')
                        {
                            for (int prefixIndex = index; prefixIndex < prefixEnd; prefixIndex++)
                                builder.Append(' ');

                            index = BlankString(source, prefixEnd, verbatim, builder);
                            continue;
                        }

                        builder.Append(current);
                        index++;
                        continue;
                    }
                    case '"':
                        index = BlankString(source, index, false, builder);
                        continue;
                    case '\'':
                        index = BlankChar(source, index, builder);
                        continue;
                    default:
                        builder.Append(current);
                        index++;
                        break;
                }
            }

            return builder.ToString();
        }

        private static int BlankString(string source, int index, bool verbatim, StringBuilder builder)
        {
            int length = source.Length;
            builder.Append(' ');
            index++;
            while (index < length)
            {
                char current = source[index];
                if (verbatim)
                {
                    if (current == '"')
                    {
                        if (index + 1 < length && source[index + 1] == '"')
                        {
                            builder.Append("  ");
                            index += 2;
                            continue;
                        }

                        builder.Append(' ');
                        index++;
                        return index;
                    }

                    builder.Append(current == '\n'
                        ? '\n'
                        : ' ');

                    index++;
                }
                else
                {
                    switch (current)
                    {
                        case '\\' when index + 1 < length:
                        {
                            builder.Append("  ");
                            index += 2;
                            continue;
                        }
                        case '"':
                        {
                            builder.Append(' ');
                            index++;
                            return index;
                        }
                        case '\n':
                        {
                            builder.Append('\n');
                            index++;
                            return index;
                        }
                        default:
                        {
                            builder.Append(' ');
                            index++;
                            break;
                        }
                    }
                }
            }

            return index;
        }

        private static int BlankChar(string source, int index, StringBuilder builder)
        {
            int length = source.Length;
            builder.Append(' ');
            index++;
            while (index < length)
            {
                char current = source[index];
                switch (current)
                {
                    case '\\' when index + 1 < length:
                    {
                        builder.Append("  ");
                        index += 2;
                        continue;
                    }
                    case '\'':
                    {
                        builder.Append(' ');
                        index++;
                        return index;
                    }
                    case '\n':
                    {
                        builder.Append('\n');
                        index++;
                        return index;
                    }
                    default:
                    {
                        builder.Append(' ');
                        index++;
                        break;
                    }
                }
            }

            return index;
        }

        private static int MatchPair(string text, int openIndex, char open, char close)
        {
            int depth = 0;
            for (int index = openIndex; index < text.Length; index++)
            {
                if (text[index] == open)
                {
                    depth++;
                }
                else if (text[index] == close)
                {
                    depth--;
                    if (depth == 0)
                        return index + 1;
                }
            }

            return text.Length;
        }

        private static int FindTopLevelSemicolon(string text, int start)
        {
            int parenthesis = 0, bracket = 0, brace = 0;
            for (int index = start; index < text.Length; index++)
            {
                char current = text[index];
                switch (current)
                {
                    case '(':
                    {
                        parenthesis++;
                        break;
                    }
                    case ')':
                    {
                        if (parenthesis > 0)
                            parenthesis--;

                        break;
                    }
                    case '[':
                    {
                        bracket++;
                        break;
                    }
                    case ']':
                    {
                        if (bracket > 0)
                            bracket--;

                        break;
                    }
                    case '{':
                    {
                        brace++;
                        break;
                    }
                    case '}':
                    {
                        if (brace > 0)
                            brace--;

                        break;
                    }
                    case ';':
                    {
                        if (parenthesis == 0
                            && bracket == 0
                            && brace == 0)
                            return index;

                        break;
                    }
                }
            }

            return text.Length - 1;
        }

        private static List<string> SplitTopLevel(string text, char separator)
        {
            List<string> parts = new();
            int angle = 0, parenthesis = 0, bracket = 0, brace = 0, segmentStart = 0;
            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];
                switch (current)
                {
                    case '<':
                        angle++;
                        break;
                    case '>':
                        if (angle > 0)
                            angle--;

                        break;
                    case '(':
                        parenthesis++;
                        break;
                    case ')':
                        if (parenthesis > 0)
                            parenthesis--;

                        break;
                    case '[':
                        bracket++;
                        break;
                    case ']':
                        if (bracket > 0)
                            bracket--;

                        break;
                    case '{':
                        brace++;
                        break;
                    case '}':
                        if (brace > 0)
                            brace--;

                        break;
                }

                if (current != separator
                    || angle != 0
                    || parenthesis != 0
                    || bracket != 0
                    || brace != 0)
                    continue;

                parts.Add(text.Substring(segmentStart, index - segmentStart));
                segmentStart = index + 1;
            }

            parts.Add(text[segmentStart..]);
            return parts;
        }

        private static int IndexOfTopLevelAssign(string text)
        {
            int angle = 0, parenthesis = 0, bracket = 0, brace = 0;
            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];
                switch (current)
                {
                    case '<':
                    {
                        angle++;
                        break;
                    }
                    case '>':
                    {
                        if (angle > 0)
                            angle--;

                        break;
                    }
                    case '(':
                    {
                        parenthesis++;
                        break;
                    }
                    case ')':
                    {
                        if (parenthesis > 0)
                            parenthesis--;

                        break;
                    }
                    case '[':
                    {
                        bracket++;
                        break;
                    }
                    case ']':
                    {
                        if (bracket > 0)
                            bracket--;

                        break;
                    }
                    case '{':
                    {
                        brace++;
                        break;
                    }
                    case '}':
                    {
                        if (brace > 0)
                            brace--;

                        break;
                    }
                }

                if (current != '='
                    || angle != 0
                    || parenthesis != 0
                    || bracket != 0
                    || brace != 0)
                    continue;

                char next = index + 1 < text.Length
                    ? text[index + 1]
                    : '\0';

                char previous = index > 0
                    ? text[index - 1]
                    : '\0';

                if (next == '>'
                    || next == '='
                    || previous == '='
                    || previous == '!'
                    || previous == '<'
                    || previous == '>'
                    || previous == '+'
                    || previous == '-'
                    || previous == '*'
                    || previous == '/'
                    || previous == '%'
                    || previous == '&'
                    || previous == '|'
                    || previous == '^')
                    continue;

                return index;
            }

            return -1;
        }

        private static string StripLeadingModifiers(string text, out bool isEvent, out bool isReadonly)
        {
            isEvent = false;
            isReadonly = false;
            while (true)
            {
                int index = 0;
                while (index < text.Length && char.IsWhiteSpace(text[index]))
                    index++;

                int start = index;
                while (index < text.Length && (char.IsLetterOrDigit(text[index]) || text[index] == '_'))
                    index++;

                if (index == start)
                    break;

                string token = text.Substring(start, index - start);
                if (!Modifiers.Contains(token))
                    break;

                if (token == "event")
                    isEvent = true;

                if (token == "readonly")
                    isReadonly = true;

                text = text[index..];
            }

            return text;
        }

        private static string LastIdentifier(string text)
        {
            text = text.TrimEnd();
            int end = text.Length - 1;
            if (end < 0 || !(char.IsLetterOrDigit(text[end]) || text[end] == '_'))
                return null;

            int start = end;
            while (start >= 0 && (char.IsLetterOrDigit(text[start]) || text[start] == '_'))
                start--;

            return text.Substring(start + 1, end - start);
        }

        private static string FirstIdentifier(string text)
        {
            int index = 0;
            while (index < text.Length && !(char.IsLetterOrDigit(text[index]) || text[index] == '_'))
                index++;

            int start = index;
            while (index < text.Length && (char.IsLetterOrDigit(text[index]) || text[index] == '_'))
                index++;

            return index > start
                ? text.Substring(start, index - start)
                : null;
        }

        private static string ReadIdentifierBefore(string text, int index)
        {
            int cursor = index - 1;
            while (cursor >= 0 && char.IsWhiteSpace(text[cursor]))
                cursor--;

            if (cursor >= 0 && text[cursor] == '>')
            {
                int depth = 0;
                while (cursor >= 0)
                {
                    if (text[cursor] == '>')
                    {
                        depth++;
                    }
                    else if (text[cursor] == '<')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            cursor--;
                            break;
                        }
                    }

                    cursor--;
                }

                while (cursor >= 0 && char.IsWhiteSpace(text[cursor]))
                    cursor--;
            }

            int end = cursor + 1, start = cursor;
            while (start >= 0 && (char.IsLetterOrDigit(text[start]) || text[start] == '_'))
                start--;

            return end - (start + 1) > 0
                ? text.Substring(start + 1, end - (start + 1))
                : null;
        }

        private static char PrevNonSpace(string text, int index)
        {
            while (index >= 0 && char.IsWhiteSpace(text[index]))
                index--;

            return index >= 0
                ? text[index]
                : '\0';
        }

        private static bool PrecededByWord(string text, int position, string word)
        {
            int cursor = position - 1;
            while (cursor >= 0 && char.IsWhiteSpace(text[cursor]))
                cursor--;

            int end = cursor + 1, start = cursor;
            while (start >= 0 && (char.IsLetterOrDigit(text[start]) || text[start] == '_'))
                start--;

            return end - (start + 1) > 0 && text.Substring(start + 1, end - (start + 1)) == word;
        }

        private static int AbsoluteNameIndex(int position, string text, string name)
        {
            MatchCollection matches = Regex.Matches(text, $@"\b{Regex.Escape(name)}\b");
            return matches.Count > 0
                ? position + matches[^1].Index
                : position;
        }

        private static bool IsKeyword(string word) => Keywords.Contains(word);

        private static int[] BuildLineStarts(string source)
        {
            List<int> starts = new()
            {
                0
            };

            for (int index = 0; index < source.Length; index++)
            {
                if (source[index] == '\n')
                    starts.Add(index + 1);
            }

            return starts.ToArray();
        }

        private static int LineFromIndex(int[] lineStarts, int index)
        {
            int low = 0, high = lineStarts.Length - 1, found = 0;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (lineStarts[mid] <= index)
                {
                    found = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return found + 1;
        }

        private static string GetLineText(string source, int[] lineStarts, int lineNumber)
        {
            int index = lineNumber - 1;
            if (index < 0 || index >= lineStarts.Length)
                return string.Empty;

            int start = lineStarts[index];
            int end = index + 1 < lineStarts.Length
                ? lineStarts[index + 1]
                : source.Length;

            string text = source.Substring(start, end - start).TrimEnd('\r', '\n');
            return text.Length > MaxSnippetLength
                ? text[..MaxSnippetLength]
                : text;
        }

        private static string ToAssetPath(string absolute, PackageInfo[] packages)
        {
            string path = absolute.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/');
            if (path.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
                return "Assets" + path[dataPath.Length..];

            foreach (PackageInfo package in packages)
            {
                string resolved = package.resolvedPath.Replace('\\', '/').TrimEnd('/');
                if (resolved.Length > 0
                    && path.StartsWith(resolved + "/", StringComparison.OrdinalIgnoreCase))
                    return package.assetPath + path[resolved.Length..];
            }

            return path;
        }
    }
}