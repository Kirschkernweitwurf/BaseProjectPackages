using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
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
    public static class SourceTextScanner
    {
        private const string CommentStart = "//";
        private const string IgnoreMarker = "graph-ignore";
        private const char LineBreak = '\n';
        private const int MinimumNameLength = 3;
        private const int SelfFileOccurrences = 2;
        private const char Underscore = '_';

        /// <summary>Marks inlined members that are used, and any member carrying the ignore marker.</summary>
        /// <param name="graph">Graph to annotate.</param>
        /// <param name="index">Index built while resolving script paths, holding the source already read.</param>
        public static void Scan(CodebaseGraphData graph, ScriptIndex index)
        {
            Dictionary<string, List<MemberNodeInfo>> inlined = CollectInlinedMembers(graph);
            Dictionary<string, List<TypeNodeInfo>> typesByPath = MapTypesByPath(graph);

            foreach (KeyValuePair<string, string> pair in index.Sources)
            {
                typesByPath.TryGetValue(pair.Key, out List<TypeNodeInfo> declared);

                MarkInlinedUsage(pair.Key, pair.Value, inlined, declared);
                MarkSuppressed(graph, pair.Key, pair.Value, declared);
            }
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

        private static void MarkInlinedUsage(string path,
            string source,
            Dictionary<string, List<MemberNodeInfo>> inlined,
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
                    int needed = IsDeclaredHere(member, path, declared)
                        ? SelfFileOccurrences
                        : 1;

                    if (pair.Value >= needed)
                        member.HasTextUsage = true;
                }
            }
        }

        private static bool IsDeclaredHere(MemberNodeInfo member, string path, List<TypeNodeInfo> declared)
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

                if (!SuppressOnLine(declared, ReadCodeBeforeMarker(line)))
                    graph.UnmatchedIgnoreMarkers.Add($"{path}:{index + 1}");
            }
        }

        /// <summary>
        /// Takes only the code in front of the comment. Matching the whole line would let a name
        /// mentioned inside the explanation silence a completely different member.
        /// </summary>
        private static string ReadCodeBeforeMarker(string line)
        {
            int marker = line.IndexOf(IgnoreMarker, StringComparison.Ordinal);
            int comment = line.LastIndexOf(CommentStart, marker, StringComparison.Ordinal);

            return comment <= 0
                ? string.Empty
                : line[..comment];
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

        /// <summary>
        /// Walks the characters directly rather than running a regex. This runs over every source file
        /// in the project, and a hand written scan of what is a very simple pattern is several times
        /// faster than the engine that would otherwise do it.
        /// </summary>
        private static Dictionary<string, int> CountIdentifiers(string source)
        {
            Dictionary<string, int> counts = new(StringComparer.Ordinal);
            int index = 0;

            while (index < source.Length)
            {
                if (!IsIdentifierStart(source[index]))
                {
                    index++;
                    continue;
                }

                int start = index;
                while (index < source.Length && IsIdentifierPart(source[index]))
                    index++;

                string identifier = source[start..index];
                counts.TryGetValue(identifier, out int count);
                counts[identifier] = count + 1;
            }

            return counts;
        }

        private static bool IsIdentifierStart(char value) => char.IsLetter(value) || value == Underscore;

        private static bool IsIdentifierPart(char value)
            => char.IsLetterOrDigit(value) || value == Underscore;
    }
}
