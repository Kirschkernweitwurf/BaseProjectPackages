using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Finds the line a member is declared on. A plain first match is nowhere near good enough. Every
    /// overload of a name collapses onto the first one, an operator never matches at all because its
    /// metadata name is op_Equality, and an attribute using nameof above the declaration wins over the
    /// declaration itself. So the search looks for a declaration, disambiguates overloads on their first
    /// parameter type, translates operator names back into source, and falls back to the type's own
    /// declaration rather than line one.
    /// </summary>
    public static class SourceLineLocator
    {
        private const string CommentPattern = @"^\s*(///|//|\*|/\*)";
        private const string DeclarationPattern = @"\b(public|private|protected|internal|static|const|"
            + @"readonly|abstract|virtual|override|sealed|partial|event|delegate|class|struct|interface|"
            + @"enum|record|operator|implicit|explicit)\b";

        private const int FirstLine = 1;
        private const string InterfaceMemberPattern = @"\s*[\w<>\[\],\.\?]+\s+";
        private const string NameOfMarker = "nameof(";
        private const string ParameterClose = ")";
        private const string ParameterOpen = "(";
        private const string TypeDeclarationPattern = @"\b(class|struct|interface|enum|record)\s+";

        private static readonly Regex CommentRegex = new(CommentPattern, RegexOptions.Compiled);
        private static readonly Regex DeclarationRegex = new(DeclarationPattern, RegexOptions.Compiled);

        /// <summary>Source spelling of each operator method name.</summary>
        private static readonly Dictionary<string, string> OperatorSpellings = new(StringComparer.Ordinal)
        {
            ["op_Addition"] = "operator +",
            ["op_BitwiseAnd"] = "operator &",
            ["op_BitwiseOr"] = "operator |",
            ["op_Decrement"] = "operator --",
            ["op_Division"] = "operator /",
            ["op_Equality"] = "operator ==",
            ["op_ExclusiveOr"] = "operator ^",
            ["op_Explicit"] = "explicit operator",
            ["op_False"] = "operator false",
            ["op_GreaterThan"] = "operator >",
            ["op_GreaterThanOrEqual"] = "operator >=",
            ["op_Implicit"] = "implicit operator",
            ["op_Increment"] = "operator ++",
            ["op_Inequality"] = "operator !=",
            ["op_LessThan"] = "operator <",
            ["op_LessThanOrEqual"] = "operator <=",
            ["op_LogicalNot"] = "operator !",
            ["op_Modulus"] = "operator %",
            ["op_Multiply"] = "operator *",
            ["op_OnesComplement"] = "operator ~",
            ["op_Subtraction"] = "operator -",
            ["op_True"] = "operator true",
            ["op_UnaryNegation"] = "operator -",
            ["op_UnaryPlus"] = "operator +"
        };

        /// <summary>Finds the one based line a member is declared on.</summary>
        /// <param name="lines">Source split into lines.</param>
        /// <param name="member">Member to locate, or null to locate the type.</param>
        /// <param name="typeName">Plain name of the declaring type, used as the fallback.</param>
        /// <param name="isInterface">
        /// Whether the declaring type is an interface. An interface member has neither an access
        /// modifier nor the abstract keyword, so the usual declaration test never matches one.
        /// </param>
        /// <returns>The line number, or the type's own line when the member cannot be pinned down.</returns>
        public static int Find(string[] lines, MemberNodeInfo member, string typeName, bool isInterface)
        {
            if (lines == null || lines.Length == 0)
                return FirstLine;

            if (member == null)
                return FindTypeLine(lines, typeName);

            int found = OperatorSpellings.TryGetValue(member.Name, out string spelling)
                ? FindToken(lines, spelling)
                : FindMemberLine(lines, member, isInterface);

            return found > 0
                ? found
                : FindTypeLine(lines, typeName);
        }

        /// <summary>Reads a script and splits it into lines.</summary>
        /// <param name="assetPath">Asset path of the script.</param>
        /// <returns>The lines, or an empty array.</returns>
        public static string[] Split(string assetPath)
        {
            string source = ScriptSourceReader.Read(assetPath);

            return string.IsNullOrEmpty(source)
                ? Array.Empty<string>()
                : source.Split('\n');
        }

        private static int FindMemberLine(string[] lines, MemberNodeInfo member, bool isInterface)
        {
            string firstParameter = ReadFirstParameterType(member.Signature);
            int loose = 0;

            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];

                if (!IsDeclarationOf(line, member.Name, isInterface))
                    continue;

                // With overloads the parameter type is the only thing that tells them apart.
                if (string.IsNullOrEmpty(firstParameter))
                    return index + 1;

                if (line.IndexOf(firstParameter, StringComparison.Ordinal) >= 0)
                    return index + 1;

                if (loose == 0)
                    loose = index + 1;
            }

            return loose;
        }

        private static bool IsDeclarationOf(string line, string name, bool isInterface)
        {
            if (line.IndexOf(name, StringComparison.Ordinal) < 0)
                return false;

            // An attribute that names the member, or a doc comment, is not where it is declared.
            if (CommentRegex.IsMatch(line) || line.IndexOf(NameOfMarker, StringComparison.Ordinal) >= 0)
                return false;

            if (!Regex.IsMatch(line, $@"\b{Regex.Escape(name)}\b"))
                return false;

            if (DeclarationRegex.IsMatch(line))
                return true;

            return isInterface
                && Regex.IsMatch(line, InterfaceMemberPattern + Regex.Escape(name) + @"\s*[<({]");
        }

        private static int FindToken(string[] lines, string token)
        {
            for (int index = 0; index < lines.Length; index++)
            {
                if (lines[index].IndexOf(token, StringComparison.Ordinal) >= 0)
                    return index + 1;
            }

            return 0;
        }

        private static int FindTypeLine(string[] lines, string typeName)
        {
            if (lines == null || lines.Length == 0 || string.IsNullOrEmpty(typeName))
                return FirstLine;

            Regex pattern = new(TypeDeclarationPattern + Regex.Escape(typeName) + @"\b");

            for (int index = 0; index < lines.Length; index++)
            {
                if (pattern.IsMatch(lines[index]))
                    return index + 1;
            }

            return FirstLine;
        }

        private static string ReadFirstParameterType(string signature)
        {
            if (string.IsNullOrEmpty(signature))
                return string.Empty;

            int open = signature.IndexOf(ParameterOpen, StringComparison.Ordinal);
            if (open < 0)
                return string.Empty;

            int close = signature.IndexOf(ParameterClose, open, StringComparison.Ordinal);
            if (close <= open + 1)
                return string.Empty;

            string parameters = signature[(open + 1)..close];
            int comma = parameters.IndexOf(',');

            return comma < 0
                ? parameters.Trim()
                : parameters[..comma].Trim();
        }
    }
}
