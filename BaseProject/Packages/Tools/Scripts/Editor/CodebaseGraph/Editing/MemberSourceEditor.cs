using System.IO;
using System.Text.RegularExpressions;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using Base.ToolPackage.Editor.CodebaseGraph.Scanning;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolPackage.Editor.CodebaseGraph.Editing
{
    /// <summary>
    /// Applies the two quick fixes the graph can offer. Both are deliberately timid: the declaration is
    /// matched by member name in the declaring type's own file, and the edit is refused unless exactly
    /// one line matches. Anything ambiguous is left alone for the person to handle.
    /// </summary>
    public static class MemberSourceEditor
    {
        private const string AccessorPattern = @"\b(private|protected|internal)\s+(get|set|init|add|remove)\b";
        private const string InternalKeyword = "internal";
        private const char LineBreak = '\n';
        private const string PrivateKeyword = "private";
        private const string PublicKeyword = "public";
        private const string ReadOnlyKeyword = "readonly";
        private const string WiderKeywords = "public|internal|protected";

        /// <summary>Rewrites a public member declaration to internal.</summary>
        /// <param name="type">Type that declares the member.</param>
        /// <param name="member">Member to demote.</param>
        /// <returns>True when the file was changed.</returns>
        public static bool DemoteToInternal(TypeNodeInfo type, MemberNodeInfo member)
        {
            Regex pattern = new($@"^(\s*){PublicKeyword}(\s+)(?=[^\r\n]*\b{Regex.Escape(member.Name)}\b)",
                RegexOptions.Multiline);

            return Rewrite(type, member, pattern, $"$1{InternalKeyword}$2");
        }

        /// <summary>Rewrites a member declaration to private, whatever it was before.</summary>
        /// <param name="type">Type that declares the member.</param>
        /// <param name="member">Member to demote.</param>
        /// <returns>True when the file was changed.</returns>
        public static bool DemoteToPrivate(TypeNodeInfo type, MemberNodeInfo member)
        {
            Regex pattern = new($@"^(\s*)(?:{WiderKeywords})(\s+)(?=[^\r\n]*\b{Regex.Escape(member.Name)}\b)",
                RegexOptions.Multiline);

            return Rewrite(type, member, pattern, $"$1{PrivateKeyword}$2");
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

            return Rewrite(type, member, pattern, $"$1$2{ReadOnlyKeyword} ");
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

        private static bool Rewrite(TypeNodeInfo type, MemberNodeInfo member, Regex pattern, string replacement)
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

            // Lowering the declaration below an accessor that is already narrower will not compile,
            // for example public with an internal set becoming internal with an internal set.
            if (HasNarrowerAccessor(source, matches[0].Index))
            {
                CustomLogger.LogWarning($"{member.Name} in {type.ShortName} declares its own accessor "
                    + "visibility, so lowering the declaration would not compile. Edit it by hand.",
                    null);

                return false;
            }

            File.WriteAllText(type.ScriptPath, pattern.Replace(source, replacement, 1));
            AssetDatabase.ImportAsset(type.ScriptPath);
            return true;
        }

        private static bool HasNarrowerAccessor(string source, int matchIndex)
        {
            int lineEnd = source.IndexOf(LineBreak, matchIndex);
            string line = lineEnd < 0
                ? source[matchIndex..]
                : source[matchIndex..lineEnd];

            return Regex.IsMatch(line, AccessorPattern);
        }
    }
}
