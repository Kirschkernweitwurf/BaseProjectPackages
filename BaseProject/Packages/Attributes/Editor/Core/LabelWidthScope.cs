using System;
using UnityEditor;

namespace Base.AttributesPackage.Editor.Core
{
    /// <summary>
    /// Widens the label column for one field when its label would otherwise be clipped.
    /// </summary>
    /// <remarks>
    /// Unity caps every prefix label at <see cref="EditorGUIUtility.labelWidth"/> and cuts whatever does
    /// not fit, which is fine while labels come from field names and are therefore short. A renamed label
    /// is written by hand and is regularly longer than that, and the cut is silent: the reader sees a
    /// sentence ending mid word with no hint that the inspector is the thing truncating it.
    /// <para>
    /// Only widened, never narrowed, and only for the one row. Narrowing would misalign the field against
    /// its neighbors, and widening every field would move the whole inspector for the sake of one label.
    /// </para>
    /// </remarks>
    internal readonly struct LabelWidthScope : IDisposable
    {
        private const float IndentStep = 15f;
        private const float Padding = 8f;

        private readonly float _previous;

        /// <summary>Widens the label column to the given width if it is not already that wide.</summary>
        /// <param name="width">The width the label needs, or zero to leave the column alone.</param>
        internal LabelWidthScope(float width)
        {
            _previous = EditorGUIUtility.labelWidth;

            if (width > _previous)
                EditorGUIUtility.labelWidth = width;
        }

        /// <summary>
        /// The width the given member needs, or zero when it can use the column as it is.
        /// </summary>
        /// <remarks>
        /// Measured only for a renamed label. A field named by its own name is short by construction, and
        /// measuring every one of them would mean a text measurement per field per repaint for a case
        /// that never comes up.
        /// </remarks>
        /// <param name="context">The member about to be drawn.</param>
        /// <returns>The required width, or zero.</returns>
        internal static float Required(in MemberContext context)
        {
            if (context.GetAttribute<LabelAttribute>() == null)
                return 0f;

            string text = context.DisplayName;

            if (string.IsNullOrEmpty(text))
                return 0f;

            return EditorStyles.label.CalcSize(ScratchContent.For(text)).x
                + EditorGUI.indentLevel * IndentStep
                + Padding;
        }

        /// <summary>Restores the label column to what it was.</summary>
        public void Dispose() => EditorGUIUtility.labelWidth = _previous;
    }
}