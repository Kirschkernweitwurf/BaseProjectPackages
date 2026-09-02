using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ToolsPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Covers the two things only the source text can tell us. First, the compiler inlines the value of
    /// a const or an enum member at every call site, so no instruction ever points back at the
    /// declaration. Second, an ignore marker in a comment is the escape hatch for a finding that is
    /// deliberate. Both are whole word text matches and therefore deliberately generous: they only ever
    /// silence a finding, never raise one.
    /// <br/><br/>
    /// Every script in the project is read, not only the ones whose type could be resolved from the file
    /// name. A generic class, a nested type or a file named differently from its type has no resolved
    /// script path, and skipping those files made consts used inside them look dead.
    /// </summary>
    internal static class SourceTextScanner
    {
        private const string BlockCommentEnd = "*/";
        private const char CharQuote = '\'';
        private const char CommentSlash = '/';
        private const char CommentStar = '*';
        private const string CommentStart = "//";
        private const char Escape = '\\';
        private const char HoleClose = '}';
        private const char HoleOpen = '{';
        private const string IgnoreMarker = "graph-ignore";
        private const char Interpolation = '$';
        private const char LineBreak = '\n';
        private const int MaximumStringPrefix = 2;
        private const int MinimumNameLength = 3;
        private const char Quote = '"';
        private const int SelfFileOccurrences = 2;
        private const char Underscore = '_';
        private const char Verbatim = '@';

        /// <summary>Marks inlined members that are used, and any member carrying the ignore marker.</summary>
        /// <param name="graph">Graph to annotate.</param>
        /// <param name="index">Index built while resolving script paths, holding the source already read.</param>
        internal static void Scan(CodebaseGraphData graph, ScriptIndex index)
        {
            Dictionary<string, List<MemberNodeInfo>> inlined = CollectInlinedMembers(graph);
            Dictionary<string, List<TypeNodeInfo>> typesByPath = MapTypesByPath(graph);

            foreach (KeyValuePair<string, string> pair in index.Sources)
            {
                typesByPath.TryGetValue(pair.Key, out List<TypeNodeInfo> declared);

                MarkInlinedUsage(pair.Value, inlined, declared);
                MarkSuppressed(graph, pair.Key, pair.Value, declared);
            }
        }

        /// <summary>
        /// Counts the identifiers in a file, ignoring anything that is not code. Walking the characters
        /// directly rather than running a regex is several times faster over every source file in the
        /// project, and it is also the only way to tell code from the text inside it.
        /// <br/><br/>
        /// Skipping strings and comments matters more than it looks. This count is the entire evidence
        /// for whether an inlined const is used, so a const called Speed mentioned once in a log message
        /// anywhere in the project would otherwise read as alive forever. Interpolation holes are still
        /// read, because the code inside them is code.
        /// </summary>
        internal static Dictionary<string, int> CountIdentifiers(string source)
        {
            Dictionary<string, int> counts = new(StringComparer.Ordinal);
            int index = 0;

            while (index < source.Length)
            {
                if (TrySkipComment(source, ref index) || TrySkipText(source, ref index, counts))
                    continue;

                if (!IsIdentifierStart(source[index]))
                {
                    index++;
                    continue;
                }

                ReadIdentifier(source, ref index, counts);
            }

            return counts;
        }

        private static Dictionary<string, List<MemberNodeInfo>> CollectInlinedMembers(CodebaseGraphData graph)
        {
            Dictionary<string, List<MemberNodeInfo>> byName = new(StringComparer.Ordinal);

            foreach (MemberNodeInfo member in graph.Members.Values)
            {
                if (member.Kind != EMemberKind.Const && member.Kind != EMemberKind.EnumMember)
                    continue;

                // Very short names match far too much to be worth reporting on.
                if (member.Name.Length < MinimumNameLength)
                {
                    member.HasTextUsage = true;
                    continue;
                }

                if (!byName.TryGetValue(member.Name, out List<MemberNodeInfo> list))
                {
                    list = new List<MemberNodeInfo>();
                    byName[member.Name] = list;
                }

                list.Add(member);
            }

            return byName;
        }

        private static Dictionary<string, List<TypeNodeInfo>> MapTypesByPath(CodebaseGraphData graph)
        {
            Dictionary<string, List<TypeNodeInfo>> byPath = new(StringComparer.Ordinal);

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                if (string.IsNullOrEmpty(type.ScriptPath))
                    continue;

                if (!byPath.TryGetValue(type.ScriptPath, out List<TypeNodeInfo> list))
                {
                    list = new List<TypeNodeInfo>();
                    byPath[type.ScriptPath] = list;
                }

                list.Add(type);
            }

            return byPath;
        }

        private static void MarkInlinedUsage(string source, Dictionary<string, List<MemberNodeInfo>> inlined,
            List<TypeNodeInfo> declared)
        {
            Dictionary<string, int> occurrences = CountIdentifiers(source);

            foreach (KeyValuePair<string, int> pair in occurrences)
            {
                if (!inlined.TryGetValue(pair.Key, out List<MemberNodeInfo> members))
                    continue;

                foreach (MemberNodeInfo member in members)
                {
                    // Inside its own file the declaration itself is one occurrence, so a second one is
                    // the first real use. In any other file a single occurrence already is a use.
                    int needed = IsDeclaredHere(member, declared)
                        ? SelfFileOccurrences
                        : 1;

                    if (pair.Value >= needed)
                        member.HasTextUsage = true;
                }
            }
        }

        private static bool IsDeclaredHere(MemberNodeInfo member, List<TypeNodeInfo> declared)
        {
            if (declared == null)
                return false;

            foreach (TypeNodeInfo type in declared)
            {
                if (member.DeclaringTypeKey.Equals(type.Key))
                    return true;
            }

            return false;
        }

        private static void MarkSuppressed(CodebaseGraphData graph,
            string path,
            string source,
            List<TypeNodeInfo> declared)
        {
            if (source.IndexOf(IgnoreMarker, StringComparison.Ordinal) < 0)
                return;

            string[] lines = source.Split(LineBreak);

            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                if (line.IndexOf(IgnoreMarker, StringComparison.Ordinal) < 0)
                    continue;

                if (!TryReadCodeBeforeMarker(line, out string code))
                    continue;

                if (!SuppressOnLine(declared, code))
                    graph.UnmatchedIgnoreMarkers.Add($"{path}:{index + 1}");
            }
        }

        /// <summary>
        /// Takes only the code in front of the comment. Matching the whole line would let a name
        /// mentioned inside the explanation silence a completely different member.
        /// </summary>
        private static bool TryReadCodeBeforeMarker(string line, out string code)
        {
            code = string.Empty;

            int marker = line.IndexOf(IgnoreMarker, StringComparison.Ordinal);
            int comment = line.LastIndexOf(CommentStart, marker, StringComparison.Ordinal);

            // Without a comment in front of it the word is not a marker at all, it is a string. The
            // declaration of the marker itself is the obvious case, and it is in this very file.
            if (comment < 0)
                return false;

            code = comment == 0
                ? string.Empty
                : line[..comment];

            return true;
        }

        private static bool SuppressOnLine(List<TypeNodeInfo> declared, string code)
        {
            if (declared == null || code.Length == 0)
                return false;

            bool matched = false;

            foreach (TypeNodeInfo type in declared)
            {
                foreach (MemberNodeInfo member in type.Members)
                {
                    // A plain containment test first, so the regex only runs on the few that could match.
                    if (code.IndexOf(member.Name, StringComparison.Ordinal) < 0)
                        continue;

                    if (!Regex.IsMatch(code, $@"\b{Regex.Escape(member.Name)}\b"))
                        continue;

                    member.IsSuppressed = true;
                    matched = true;
                }
            }

            return matched;
        }

        private static void ReadIdentifier(string source, ref int index, Dictionary<string, int> counts)
        {
            int start = index;

            while (index < source.Length && IsIdentifierPart(source[index]))
                index++;

            string identifier = source[start..index];
            counts.TryGetValue(identifier, out int count);
            counts[identifier] = count + 1;
        }

        private static bool TrySkipComment(string source, ref int index)
        {
            if (source[index] != CommentSlash || index + 1 >= source.Length)
                return false;

            if (source[index + 1] == CommentSlash)
            {
                int end = source.IndexOf(LineBreak, index);
                index = end < 0
                    ? source.Length
                    : end;

                return true;
            }

            if (source[index + 1] != CommentStar)
                return false;

            int close = source.IndexOf(BlockCommentEnd, index + 2, StringComparison.Ordinal);
            index = close < 0
                ? source.Length
                : close + BlockCommentEnd.Length;

            return true;
        }

        /// <summary>
        /// Steps over a string or character literal. An interpolated string is walked rather than
        /// skipped, so the identifiers inside its holes are still counted.
        /// </summary>
        private static bool TrySkipText(string source, ref int index, Dictionary<string, int> counts)
        {
            char value = source[index];

            if (value == CharQuote)
            {
                SkipCharLiteral(source, ref index);
                return true;
            }

            if (value != Quote)
                return false;

            bool isVerbatim = HasPrefix(source, index, Verbatim);
            bool isInterpolated = HasPrefix(source, index, Interpolation);

            index++;

            while (index < source.Length)
            {
                char current = source[index];

                if (!isVerbatim && current == Escape)
                {
                    index += 2;
                    continue;
                }

                if (isInterpolated && current == HoleOpen)
                {
                    // Two braces in a row are a literal brace rather than the start of a hole.
                    if (index + 1 < source.Length && source[index + 1] == HoleOpen)
                    {
                        index += 2;
                        continue;
                    }

                    ReadHole(source, ref index, counts);
                    continue;
                }

                if (current != Quote)
                {
                    index++;
                    continue;
                }

                // Two quotes in a row inside a verbatim string are one escaped quote.
                if (isVerbatim && index + 1 < source.Length && source[index + 1] == Quote)
                {
                    index += 2;
                    continue;
                }

                index++;
                return true;
            }

            return true;
        }

        private static void ReadHole(string source, ref int index, Dictionary<string, int> counts)
        {
            int depth = 0;

            while (index < source.Length)
            {
                char value = source[index];

                if (value == HoleOpen)
                {
                    depth++;
                    index++;
                    continue;
                }

                if (value == HoleClose)
                {
                    depth--;
                    index++;

                    if (depth <= 0)
                        return;

                    continue;
                }

                if (IsIdentifierStart(value))
                {
                    ReadIdentifier(source, ref index, counts);
                    continue;
                }

                index++;
            }
        }

        private static void SkipCharLiteral(string source, ref int index)
        {
            index++;

            while (index < source.Length)
            {
                if (source[index] == Escape)
                {
                    index += 2;
                    continue;
                }

                if (source[index] == CharQuote)
                {
                    index++;
                    return;
                }

                index++;
            }
        }

        private static bool HasPrefix(string source, int index, char prefix)
        {
            for (int back = 1; back <= MaximumStringPrefix && index - back >= 0; back++)
            {
                char value = source[index - back];

                if (value == prefix)
                    return true;

                if (value != Verbatim && value != Interpolation)
                    return false;
            }

            return false;
        }

        private static bool IsIdentifierStart(char value) => char.IsLetter(value) || value == Underscore;

        private static bool IsIdentifierPart(char value) => char.IsLetterOrDigit(value) || value == Underscore;
    }
}