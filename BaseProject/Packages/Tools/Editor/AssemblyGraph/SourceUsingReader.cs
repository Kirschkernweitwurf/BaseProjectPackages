using System;
using System.Collections.Generic;
using System.IO;

namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>
    /// Collects the namespaces a compilation names in its using directives.
    /// <br/><br/>
    /// A using is the one signal that survives everything the compiler folds away. A constant, a
    /// <c>nameof</c> and an enum value all become literals with no trace of where they came from,
    /// yet the file cannot compile without the assembly that declares them. Reading the directives
    /// back out of the source catches exactly that, because it is the same text the compiler acts on.
    /// </summary>
    internal static class SourceUsingReader
    {
        private const char AliasSeparator = '=';
        private const string CommentMarker = "//";
        private const char GenericMarker = '<';
        private const string GlobalPrefix = "global ";
        private const char NamespaceSeparator = '.';
        private const char OpenParenthesis = '(';
        private const string StaticPrefix = "static ";
        private const char StatementEnd = ';';
        private const string UsingPrefix = "using ";

        /// <summary>Reads every using directive in the given source files.</summary>
        /// <param name="sourceFiles">Paths relative to the project root.</param>
        /// <returns>The namespaces those directives name.</returns>
        internal static HashSet<string> Read(IEnumerable<string> sourceFiles)
        {
            HashSet<string> namespaces = new(StringComparer.Ordinal);
            if (sourceFiles == null)
                return namespaces;

            foreach (string sourceFile in sourceFiles)
                ReadFile(sourceFile, namespaces);

            return namespaces;
        }

        /// <summary>Reads the namespaces one line names, if it is a using directive at all.</summary>
        /// <param name="line">A single line of source.</param>
        /// <param name="namespaces">Receives what the line names.</param>
        internal static void ReadLine(string line, HashSet<string> namespaces)
        {
            string directive = ExtractDirective(line);
            if (directive == null)
                return;

            if (directive.StartsWith(StaticPrefix, StringComparison.Ordinal))
            {
                AddParentNamespace(directive.Substring(StaticPrefix.Length).Trim(), namespaces);
                return;
            }

            int aliasIndex = directive.IndexOf(AliasSeparator);
            if (aliasIndex < 0)
            {
                Add(directive, namespaces);
                return;
            }

            AddFromAlias(directive, aliasIndex, namespaces);
        }

        private static void ReadFile(string sourceFile, HashSet<string> namespaces)
        {
            if (string.IsNullOrEmpty(sourceFile))
                return;

            string fullPath = ProjectPaths.ToAbsolute(sourceFile);
            if (!File.Exists(fullPath))
                return;

            try
            {
                foreach (string line in File.ReadAllLines(fullPath))
                    ReadLine(line, namespaces);
            }
            catch (IOException)
            {
                // A file that will not open credits nothing, which costs a candidate to check by hand.
            }
        }

        /// <summary>Returns the part behind the keyword, or null when the line is not a directive.</summary>
        private static string ExtractDirective(string line)
        {
            string trimmed = StripComment(line).Trim();

            if (trimmed.StartsWith(GlobalPrefix, StringComparison.Ordinal))
                trimmed = trimmed.Substring(GlobalPrefix.Length).TrimStart();

            if (!trimmed.StartsWith(UsingPrefix, StringComparison.Ordinal))
                return null;

            // A using statement or a using declaration is not a directive, and both carry a call.
            if (trimmed.IndexOf(OpenParenthesis) >= 0)
                return null;

            if (trimmed[trimmed.Length - 1] != StatementEnd)
                return null;

            return trimmed.Substring(UsingPrefix.Length, trimmed.Length - UsingPrefix.Length - 1).Trim();
        }

        private static string StripComment(string line)
        {
            int marker = line.IndexOf(CommentMarker, StringComparison.Ordinal);

            return marker < 0
                ? line
                : line.Substring(0, marker);
        }

        private static void AddFromAlias(string directive, int aliasIndex, HashSet<string> namespaces)
        {
            // "using StreamReader reader = ..." is a declaration rather than an alias, and an alias
            // name is a single identifier, so a space on the left is what tells the two apart.
            string alias = directive.Substring(0, aliasIndex).Trim();
            if (alias.IndexOf(' ') >= 0)
                return;

            string target = directive.Substring(aliasIndex + 1).Trim();
            int generic = target.IndexOf(GenericMarker);
            if (generic >= 0)
                target = target.Substring(0, generic).Trim();

            // An alias may name a namespace or a type and the text does not say which, so both
            // readings are credited. Crediting too much costs a candidate, too little costs a build.
            Add(target, namespaces);
            AddParentNamespace(target, namespaces);
        }

        private static void AddParentNamespace(string typeName, HashSet<string> namespaces)
        {
            int lastSeparator = typeName.LastIndexOf(NamespaceSeparator);
            if (lastSeparator <= 0)
                return;

            Add(typeName.Substring(0, lastSeparator), namespaces);
        }

        private static void Add(string namespaceName, HashSet<string> namespaces)
        {
            if (string.IsNullOrEmpty(namespaceName))
                return;

            namespaces.Add(namespaceName);
        }
    }
}