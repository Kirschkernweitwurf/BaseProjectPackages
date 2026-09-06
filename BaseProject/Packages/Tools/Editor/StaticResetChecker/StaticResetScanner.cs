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

        /// <summary>
        /// Reads one file's text and adds every static it finds no reset for. This is the whole of the
        /// analysis: <see cref="Scan"/> only walks the disk and hands the text over, so pointing this
        /// at a source string covers the rules without a project having to be arranged around them.
        /// </summary>
        /// <param name="source">The full text of the file.</param>
        /// <param name="assetPath">Project relative path, shown in the window.</param>
        /// <param name="absolutePath">Path on disk, used to open the file at the line.</param>
        /// <param name="options">What counts as a reset and what is passed over.</param>
        /// <param name="results">Receives one finding per unreset static.</param>
        internal static void ScanFile(string source, string assetPath, string absolutePath, ScanOptions options,
            List<Finding> results)
        {
            ScanContext context = new()
            {
                Cleaned = SourceCleaner.Clean(source),
                LineStarts = SourceLines.BuildLineStarts(source),
                Options = options
            };

            foreach (Match match in Regex.Matches(context.Cleaned, @"\bstatic\b"))
            {
                int position = match.Index;
                if (DeclarationReader.PrecededByWord(context.Cleaned, position, "using"))
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

                int line = SourceLines.LineFromIndex(context.LineStarts, hit.Index);
                string lineText = SourceLines.GetLineText(source, context.LineStarts, line);
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
                        int semicolon = SourceNavigator.FindTopLevelSemicolon(cleaned, index + 1);
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
            char previous = SourceNavigator.PrevNonSpace(cleaned, parenIndex - 1);
            if (previous == '>')
                return true;

            string identifier = DeclarationReader.ReadIdentifierBefore(cleaned, parenIndex);
            return identifier != null && !DeclarationReader.IsKeyword(identifier);
        }

        private static void HandleMethod(ScanContext context, int position, int parenIndex)
        {
            string cleaned = context.Cleaned;
            int closeParen = SourceNavigator.MatchPair(cleaned, parenIndex, '(', ')');
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
                int bodyEnd = SourceNavigator.MatchPair(cleaned, cursor, '{', '}');
                body = cleaned.Substring(cursor, bodyEnd - cursor);
            }
            else
            {
                int semicolon = SourceNavigator.FindTopLevelSemicolon(cleaned, cursor + 2);
                body = cleaned.Substring(cursor + 2, Math.Max(0, semicolon - (cursor + 2)));
            }

            string name = DeclarationReader.ReadIdentifierBefore(cleaned, parenIndex);
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

            int blockEnd = SourceNavigator.MatchPair(cleaned, braceIndex, '{', '}');
            string block = cleaned.Substring(braceIndex, blockEnd - braceIndex);

            bool isAuto = Regex.IsMatch(block, @"\b(get|set|init)\s*;");
            if (!isAuto)
                return;

            string name = DeclarationReader.LastIdentifier(head);
            if (string.IsNullOrEmpty(name) || DeclarationReader.IsKeyword(name))
                return;

            context.Fields.Add(new FieldHit
            {
                Index = DeclarationReader.AbsoluteNameIndex(position, head, name),
                Name = name,
                Kind = "static property"
            });
        }

        private static void EmitField(ScanContext context, int position, string declaration)
        {
            string body = declaration["static".Length..];

            body = DeclarationReader.StripLeadingModifiers(body, out bool isEvent, out bool isReadonly);
            if (isEvent && !context.Options.IncludeEvents)
                return;

            if (isReadonly && context.Options.IgnoreReadonly)
                return;

            List<string> declarators = SourceNavigator.SplitTopLevel(body, ',');
            for (int index = 0; index < declarators.Count; index++)
            {
                string declarator = declarators[index];
                int assign = SourceNavigator.IndexOfTopLevelAssign(declarator);
                string left = assign >= 0
                    ? declarator[..assign]
                    : declarator;

                string name = index == 0
                    ? DeclarationReader.LastIdentifier(left)
                    : DeclarationReader.FirstIdentifier(left);

                if (string.IsNullOrEmpty(name) || DeclarationReader.IsKeyword(name))
                    continue;

                context.Fields.Add(new FieldHit
                {
                    Index = DeclarationReader.AbsoluteNameIndex(position, declaration, name),
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