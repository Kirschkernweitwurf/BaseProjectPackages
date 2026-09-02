using System;
using Base.ToolsPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Builds and reads the ids that survive a recompile. Metadata tokens do not, so anything written to
    /// disk is keyed on names instead. The shapes nest by construction, which is what lets a namespace
    /// be dismissed or restored together with everything inside it.
    /// </summary>
    internal static class GraphIdentity
    {
        private const char FindingBoundary = '|';
        private const char MemberBoundary = '#';
        private const string MemberPrefix = "member:";
        private const string MemberSeparator = "#";
        private const char NamespaceBoundary = '.';
        private const string NamespacePrefix = "namespace:";
        private const string TypePrefix = "type:";

        /// <summary>
        /// Narrows an id to one finding. Without this a dismissal covers the whole entry, so setting
        /// aside a size warning on a type also silences a future dead type or cycle on that same type.
        /// The tool hides a real problem because of a decision made about a different one.
        /// </summary>
        /// <param name="id">ID of the entry.</param>
        /// <param name="finding">Finding to narrow it to.</param>
        /// <returns>The id of that one finding on that entry.</returns>
        internal static string ForFinding(string id, EFinding finding) => $"{id}{FindingBoundary}{finding}";

        /// <summary>Splits a stored id back into the entry it names and the finding, if it carries one.</summary>
        /// <param name="id">ID to read.</param>
        /// <param name="finding">The finding, or none when the id covers the whole entry.</param>
        /// <returns>The id with any finding removed.</returns>
        internal static string ReadEntry(string id, out EFinding finding)
        {
            finding = EFinding.None;

            if (string.IsNullOrEmpty(id))
                return id;

            int boundary = id.LastIndexOf(FindingBoundary);
            if (boundary < 0)
                return id;

            return Enum.TryParse(id[(boundary + 1)..], out finding)
                ? id[..boundary]
                : id;
        }

        /// <summary>Checks that a string looks like an id this tool produced.</summary>
        /// <param name="id">Text to test.</param>
        /// <returns>True when the id carries a known prefix.</returns>
        internal static bool IsValid(string id) => TryRead(id, out EDismissalKind _, out string _);

        /// <summary>Splits an id into what it points at and the name inside it.</summary>
        /// <param name="id">ID to read.</param>
        /// <param name="kind">What the id points at.</param>
        /// <param name="qualifiedName">The id with its prefix removed.</param>
        /// <returns>True when the id could be read.</returns>
        internal static bool TryRead(string id, out EDismissalKind kind, out string qualifiedName)
        {
            kind = default(EDismissalKind);
            qualifiedName = null;

            if (string.IsNullOrEmpty(id))
                return false;

            id = ReadEntry(id, out EFinding _);

            if (id.StartsWith(NamespacePrefix, StringComparison.Ordinal))
            {
                kind = EDismissalKind.Namespace;
                qualifiedName = id[NamespacePrefix.Length..];
                return true;
            }

            if (id.StartsWith(TypePrefix, StringComparison.Ordinal))
            {
                kind = EDismissalKind.Type;
                qualifiedName = id[TypePrefix.Length..];
                return true;
            }

            if (!id.StartsWith(MemberPrefix, StringComparison.Ordinal))
                return false;

            kind = EDismissalKind.Member;
            qualifiedName = id[MemberPrefix.Length..];
            return true;
        }

        /// <summary>
        /// True when one id sits inside another. A type lives under its namespace and a member under its
        /// type, and both boundaries are visible in the name itself.
        /// </summary>
        /// <param name="outerId">ID of the containing entry.</param>
        /// <param name="innerId">ID that may sit inside it.</param>
        /// <returns>True when the inner id is contained by the outer one.</returns>
        internal static bool IsNested(string outerId, string innerId)
        {
            if (!TryRead(outerId, out EDismissalKind outerKind, out string outer))
                return false;

            if (outerKind == EDismissalKind.Member)
                return false;

            if (!TryRead(innerId, out EDismissalKind _, out string inner))
                return false;

            if (inner.Length <= outer.Length || !inner.StartsWith(outer, StringComparison.Ordinal))
                return false;

            char boundary = inner[outer.Length];
            return boundary == NamespaceBoundary || boundary == MemberBoundary;
        }

        /// <summary>Builds the stable id of a namespace.</summary>
        /// <param name="name">Full namespace name.</param>
        /// <returns>The id.</returns>
        internal static string ForNamespace(string name) => NamespacePrefix + name;

        /// <summary>Builds the stable id of a type.</summary>
        /// <param name="type">Type to identify.</param>
        /// <returns>The id.</returns>
        internal static string ForType(TypeNodeInfo type) => TypePrefix + type.FullName;

        /// <summary>Builds the stable id of a member.</summary>
        /// <param name="declaring">Type the member is declared on.</param>
        /// <param name="member">Member to identify.</param>
        /// <returns>The id.</returns>
        internal static string ForMember(TypeNodeInfo declaring, MemberNodeInfo member)
            => $"{MemberPrefix}{declaring.FullName}{MemberSeparator}{member.Signature}";
    }
}