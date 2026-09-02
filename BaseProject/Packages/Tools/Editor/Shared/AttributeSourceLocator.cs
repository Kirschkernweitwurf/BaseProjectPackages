using System;
using System.Text.RegularExpressions;
using Base.UtilityPackage;
using UnityEditor;

namespace Base.ToolsPackage.Editor.Shared
{
    /// <summary>
    /// Finds the source line an attribute sits on, so a window can open a script with the cursor
    /// already on the value the user clicked rather than at the top of the file.
    /// </summary>
    /// <remarks>
    /// A text scan rather than a syntax tree, because the answer only has to be good enough to place
    /// a cursor. The first line carrying the attribute wins; when there is none, the declaration the
    /// caller describes is used instead, which still lands the user in the right place.
    /// </remarks>
    internal static class AttributeSourceLocator
    {
        /// <summary>The capture group an argument pattern has to put the value in.</summary>
        private const int ArgumentGroup = 1;

        private const int LineStart = 0;
        private const int NotFound = -1;
        private const char OpenParenthesis = '(';

        private static readonly string[] LineSeparators =
        {
            "\r\n",
            "\n"
        };

        /// <summary>Returned when the script has nothing to point at.</summary>
        private static readonly (int Line, int Column) NoLocation = (0, 0);

        /// <summary>
        /// A pattern matching the declaration of a class, for use as the fallback line.
        /// </summary>
        /// <param name="typeName">
        /// The type name. A generic arity suffix is stripped, so <c>Pool`1</c> matches <c>Pool</c>.
        /// </param>
        /// <returns>The declaration pattern, or null when there is no name to match on.</returns>
        internal static Regex ClassDeclaration(string typeName)
        {
            string name = TypeNameUtility.TrimArity(typeName);

            // Without a name the pattern would be a bare "class" and match the first type in the
            // file, which is worse than having no fallback at all.
            if (string.IsNullOrEmpty(name))
                return null;

            return new Regex($@"\bclass\s+{Regex.Escape(name)}\b");
        }

        /// <summary>
        /// A pattern matching the declaration of a method, for use as the fallback line.
        /// </summary>
        /// <remarks>
        /// A call to the method matches this too. The first match wins, so a method called before it
        /// is declared points the cursor at the call instead. That is still inside the right file and
        /// close enough for the one job this has.
        /// </remarks>
        /// <param name="memberName">The method name.</param>
        /// <returns>The declaration pattern, or null when there is no name to match on.</returns>
        internal static Regex MemberDeclaration(string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
                return null;

            return new Regex($@"\b{Regex.Escape(memberName)}\s*\(");
        }

        /// <summary>
        /// Locates the attribute in a script.
        /// </summary>
        /// <param name="script">The script to scan.</param>
        /// <param name="attributeToken">
        /// The attribute name as it is written in source, without brackets, for example
        /// <c>DefaultExecutionOrder</c>.
        /// </param>
        /// <param name="declaration">
        /// The line to fall back to when the attribute is not found. Pass null for no fallback.
        /// </param>
        /// <param name="argument">
        /// Pattern picking the argument the cursor should land on, with the value in capture group
        /// one. Pass null to land just inside the parentheses instead.
        /// </param>
        /// <param name="requiredOnLine">
        /// Extra text the attribute line has to contain as well, for telling several uses of the same
        /// attribute in one file apart. Pass null when the attribute alone identifies the line.
        /// </param>
        /// <returns>
        /// A one-based line and a zero-based column, or zeros when nothing could be located.
        /// </returns>
        internal static (int Line, int Column) Find(MonoScript script, string attributeToken,
            Regex declaration, Regex argument = null, string requiredOnLine = null)
        {
            if (script == null || string.IsNullOrEmpty(attributeToken))
                return NoLocation;

            string source = script.text;

            if (string.IsNullOrEmpty(source))
                return NoLocation;

            string[] lines = source.Split(LineSeparators, StringSplitOptions.None);
            int declarationLine = NotFound;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int tokenIndex = line.IndexOf(attributeToken, StringComparison.Ordinal);

                if (tokenIndex >= 0 && IsOnLine(line, requiredOnLine))
                    return (i + 1, ColumnFor(line, tokenIndex, attributeToken, argument));

                if (declarationLine < 0 && declaration != null && declaration.IsMatch(line))
                    declarationLine = i + 1;
            }

            return declarationLine > 0
                ? (declarationLine, LineStart)
                : NoLocation;
        }

        private static bool IsOnLine(string line, string required) => string.IsNullOrEmpty(required)
            || line.IndexOf(required, StringComparison.Ordinal) >= 0;

        private static int ColumnFor(string line, int tokenIndex, string attributeToken, Regex argument)
        {
            if (argument != null)
            {
                Match match = argument.Match(line);

                if (match.Success)
                    return match.Groups[ArgumentGroup].Index;
            }

            int parenthesisIndex = line.IndexOf(OpenParenthesis, tokenIndex + attributeToken.Length);

            // One past the parenthesis is the first character of the argument list. An attribute with
            // no parentheses has nothing to point into, so the cursor goes on its name instead.
            return parenthesisIndex >= 0
                ? parenthesisIndex + 1
                : tokenIndex;
        }
    }
}