using System;
using System.IO;
using System.Text.RegularExpressions;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using Base.ToolPackage.Editor.CodebaseGraph.Scanning;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolPackage.Editor.CodebaseGraph.Editing
{
    /// <summary>
    /// Applies the three quick fixes the graph can offer. All of them are deliberately timid: the
    /// declaration is matched by member name in the declaring type's own file, the edit is refused
    /// unless exactly one line matches, and the match is then checked against the shape it claims to
    /// be. Anything ambiguous is left alone for a person to handle.
    /// </summary>
    public static class MemberSourceEditor
    {
        private const string AccessorPattern = @"\b(private|protected|internal)\s+(get|set|init|add|remove)\b";
        private const char BodyClose = '}';
        private const char BodyOpen = '{';
        private const string ExpressionBody = "=>";
        private const string InternalKeyword = "internal";
        private const char LineBreak = '\n';
        private const char ParameterOpen = '(';
        private const string PrivateKeyword = "private";
        private const string PublicKeyword = "public";
        private const string ReadOnlyKeyword = "readonly";
        private const char StatementEnd = ';';
        private const string WiderKeywords = "public|internal|protected";

        /// <summary>Rewrites a public member declaration to internal.</summary>
        /// <param name="type">Type that declares the member.</param>
        /// <param name="member">Member to demote.</param>
        /// <returns>True when the file was changed.</returns>
        public static bool DemoteToInternal(TypeNodeInfo type, MemberNodeInfo member)
        {
            Regex pattern = new($@"^(\s*){PublicKeyword}(\s+)(?=[^\r\n]*\b{Regex.Escape(member.Name)}\b)",
                RegexOptions.Multiline);

            return Rewrite(type, member, pattern, $"$1{InternalKeyword}$2", false);
        }

        /// <summary>Rewrites a member declaration to private, whatever it was before.</summary>
        /// <param name="type">Type that declares the member.</param>
        /// <param name="member">Member to demote.</param>
        /// <returns>True when the file was changed.</returns>
        public static bool DemoteToPrivate(TypeNodeInfo type, MemberNodeInfo member)
        {
            Regex pattern = new($@"^(\s*)(?:{WiderKeywords})(\s+)(?=[^\r\n]*\b{Regex.Escape(member.Name)}\b)",
                RegexOptions.Multiline);

            return Rewrite(type, member, pattern, $"$1{PrivateKeyword}$2", false);
        }

        /// <summary>Adds the readonly keyword to a field declaration.</summary>
        /// <param name="type">Type that declares the field.</param>
        /// <param name="member">Field to make readonly.</param>
        /// <returns>True when the file was changed.</returns>
        public static bool AddReadOnly(TypeNodeInfo type, MemberNodeInfo member)
        {
            Regex pattern = new(
                $@"^(\s*)((?:private|protected|internal|public)(?:\s+static)?\s+)"
                + $@"(?![^\r\n]*\b{ReadOnlyKeyword}\b)(?=[^\r\n]*\b{Regex.Escape(member.Name)}\b)",
                RegexOptions.Multiline);

            return Rewrite(type, member, pattern, $"$1$2{ReadOnlyKeyword} ", true);
        }

        /// <summary>Opens the script that declares the member and jumps to its declaration.</summary>
        /// <param name="type">Type that declares the member.</param>
        /// <param name="member">Member to jump to, or null to open the file at the top.</param>
        public static void OpenAtMember(TypeNodeInfo type, MemberNodeInfo member)
        {
            if (type == null || string.IsNullOrEmpty(type.ScriptPath))
                return;

            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(type.ScriptPath);
            if (script == null)
                return;

            AssetDatabase.OpenAsset(script, FindLine(type, member));
        }

        private static int FindLine(TypeNodeInfo type, MemberNodeInfo member)
            => SourceLineLocator.Find(SourceLineLocator.Split(type.ScriptPath),
                member,
                type.ShortName,
                type.Kind == ETypeKind.Interface);

        private static bool Rewrite(TypeNodeInfo type,
            MemberNodeInfo member,
            Regex pattern,
            string replacement,
            bool requiresField)
        {
            if (type == null || string.IsNullOrEmpty(type.ScriptPath))
            {
                CustomLogger.LogWarning($"No script file is known for {member?.Name}, so it was not changed.", null);
                return false;
            }

            // Packages installed from Git live under a virtual path with no writable file behind it.
            if (!File.Exists(type.ScriptPath))
            {
                CustomLogger.LogWarning($"{type.ScriptPath} is not a writable file in this project, "
                    + "so nothing was changed.",
                    null);

                return false;
            }

            // Read the raw bytes rather than the asset text, so the file keeps its own line endings.
            string source = File.ReadAllText(type.ScriptPath);
            if (string.IsNullOrEmpty(source))
                return false;

            MatchCollection matches = pattern.Matches(source);
            if (matches.Count != 1)
            {
                CustomLogger.LogWarning($"{member.Name} in {type.ShortName} matched {matches.Count} declarations, "
                    + "so nothing was changed. Edit it by hand.",
                    null);

                return false;
            }

            if (!IsSafeToRewrite(source, matches[0].Index, member, type, requiresField))
                return false;

            File.WriteAllText(type.ScriptPath, pattern.Replace(source, replacement, 1));
            AssetDatabase.ImportAsset(type.ScriptPath);
            return true;
        }

        private static bool IsSafeToRewrite(string source,
            int matchIndex,
            MemberNodeInfo member,
            TypeNodeInfo type,
            bool requiresField)
        {
            if (requiresField && !IsFieldDeclaration(source, matchIndex))
            {
                CustomLogger.LogWarning($"The line matched for {member.Name} in {type.ShortName} is not a "
                    + "plain field declaration, so nothing was changed. Edit it by hand.",
                    null);

                return false;
            }

            // Lowering the declaration below an accessor that is already narrower will not compile,
            // for example public with an internal set becoming internal with an internal set.
            if (!HasNarrowerAccessor(source, matchIndex))
                return true;

            CustomLogger.LogWarning($"{member.Name} in {type.ShortName} declares its own accessor "
                + "visibility, so lowering the declaration would not compile. Edit it by hand.",
                null);

            return false;
        }

        /// <summary>
        /// Looks for a narrower accessor across the whole member, not just its first line. A property
        /// written over several lines keeps its private set on a line of its own, and checking only as
        /// far as the next line break walks straight past it.
        /// </summary>
        private static bool HasNarrowerAccessor(string source, int matchIndex)
            => Regex.IsMatch(ReadMemberSpan(source, matchIndex), AccessorPattern);

        private static string ReadMemberSpan(string source, int matchIndex)
        {
            int lineEnd = source.IndexOf(LineBreak, matchIndex);
            int declarationEnd = lineEnd < 0
                ? source.Length
                : lineEnd;

            string firstLine = source[matchIndex..declarationEnd];

            // A declaration that ends on its own line has no body to walk into.
            if (firstLine.IndexOf(StatementEnd) >= 0 && firstLine.IndexOf(BodyOpen) < 0)
                return firstLine;

            int bodyStart = source.IndexOf(BodyOpen, matchIndex);
            if (bodyStart < 0)
                return firstLine;

            int depth = 0;

            for (int index = bodyStart; index < source.Length; index++)
            {
                if (source[index] == BodyOpen)
                    depth++;
                else if (source[index] == BodyClose)
                    depth--;

                if (depth == 0)
                    return source[matchIndex..(index + 1)];
            }

            return source[matchIndex..];
        }

        /// <summary>
        /// True when the matched line really is a plain field. An attribute written on the same line
        /// keeps the regex from matching the field it belongs to, and the single match it does find is
        /// then something else entirely.
        /// </summary>
        private static bool IsFieldDeclaration(string source, int matchIndex)
        {
            int lineEnd = source.IndexOf(LineBreak, matchIndex);
            string line = lineEnd < 0
                ? source[matchIndex..]
                : source[matchIndex..lineEnd];

            string trimmed = line.TrimEnd();

            if (trimmed.Length == 0 || trimmed[^1] != StatementEnd)
                return false;

            return trimmed.IndexOf(ParameterOpen) < 0
                && trimmed.IndexOf(ExpressionBody, StringComparison.Ordinal) < 0;
        }
    }
}
