using System.Collections.Generic;
using Base.ToolsPackage.Editor.CodebaseGraph.Model;
using UnityEngine;

namespace Base.ToolsPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// The single place that decides what a kind looks like and what a visibility is colored. Node
    /// drawing and the legend both read from here, so the legend cannot drift from what is on screen.
    /// </summary>
    internal static class GraphSymbols
    {
        private const string NamespaceGlyphText = "{}";

        /// <summary>Glyph shown on a namespace node.</summary>
        internal static string NamespaceGlyph => NamespaceGlyphText;

        /// <summary>
        /// Color of each visibility, used for the accent stripe and the member rows. Pastel like the
        /// rest, which also reads better than a saturated tint would against the dark node body.
        /// </summary>
        private static readonly Dictionary<EAccessLevel, Color> AccessColors = new()
        {
            [EAccessLevel.Public] = new Color(0.62f, 0.85f, 0.60f),
            [EAccessLevel.ProtectedInternal] = new Color(0.58f, 0.86f, 0.83f),
            [EAccessLevel.Protected] = new Color(0.94f, 0.86f, 0.62f),
            [EAccessLevel.Internal] = new Color(0.64f, 0.78f, 0.94f),
            [EAccessLevel.Private] = new Color(0.76f, 0.76f, 0.80f)
        };

        /// <summary>Letter shown for each kind of member.</summary>
        private static readonly Dictionary<EMemberKind, string> MemberGlyphs = new()
        {
            [EMemberKind.Constructor] = "C",
            [EMemberKind.Const] = "K",
            [EMemberKind.EnumMember] = "N",
            [EMemberKind.Event] = "E",
            [EMemberKind.Field] = "F",
            [EMemberKind.Method] = "M",
            [EMemberKind.Property] = "P",
            [EMemberKind.SerializedField] = "S"
        };

        /// <summary>Letter shown for each kind of type.</summary>
        private static readonly Dictionary<ETypeKind, string> TypeGlyphs = new()
        {
            [ETypeKind.Class] = "C",
            [ETypeKind.Delegate] = "D",
            [ETypeKind.Enum] = "E",
            [ETypeKind.Interface] = "I",
            [ETypeKind.Struct] = "S"
        };

        /// <summary>Returns the letter for a kind of type.</summary>
        /// <param name="kind">Kind to name.</param>
        /// <returns>The glyph.</returns>
        internal static string GetGlyph(ETypeKind kind) => TypeGlyphs.GetValueOrDefault(kind, NamespaceGlyphText);

        /// <summary>Returns the letter for a kind of member.</summary>
        /// <param name="kind">Kind to name.</param>
        /// <returns>The glyph.</returns>
        internal static string GetGlyph(EMemberKind kind) => MemberGlyphs.TryGetValue(kind, out string glyph)
            ? glyph
            : MemberGlyphs[EMemberKind.Field];

        /// <summary>Returns the color standing for a visibility.</summary>
        /// <param name="access">Visibility to color.</param>
        /// <returns>The color.</returns>
        internal static Color GetColor(EAccessLevel access) => AccessColors.TryGetValue(access, out Color color)
            ? color
            : AccessColors[EAccessLevel.Private];

        /// <summary>Lists the type glyphs in reading order, for the legend.</summary>
        /// <returns>Kind and glyph pairs.</returns>
        internal static IReadOnlyDictionary<ETypeKind, string> GetTypeGlyphs() => TypeGlyphs;

        /// <summary>Lists the member glyphs in reading order, for the legend.</summary>
        /// <returns>Kind and glyph pairs.</returns>
        internal static IReadOnlyDictionary<EMemberKind, string> GetMemberGlyphs() => MemberGlyphs;

        /// <summary>Lists the visibilities in reading order, for the legend.</summary>
        /// <returns>The visibilities, widest first.</returns>
        internal static IReadOnlyList<EAccessLevel> GetAccessOrder() => new[]
        {
            EAccessLevel.Public,
            EAccessLevel.ProtectedInternal,
            EAccessLevel.Protected,
            EAccessLevel.Internal,
            EAccessLevel.Private
        };
    }
}