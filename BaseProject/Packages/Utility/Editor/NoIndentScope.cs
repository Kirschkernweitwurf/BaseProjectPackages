using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor
{
    /// <summary>
    /// Suspends the ambient indent for the duration of the scope, then restores it.
    /// </summary>
    /// <remarks>
    /// Most EditorGUI calls run their rect through <c>EditorGUI.IndentedRect</c> before drawing, which
    /// shifts it right and shrinks its width by one step per indent level. That is what you want when
    /// the call is laying itself out, and exactly what you do not want when the caller already computed
    /// an absolute rect: the control lands one step too far right and loses that much width.
    /// <para>
    /// A class deriving from <see cref="GUI.Scope"/> rather than a struct. A struct always keeps its
    /// implicit parameterless constructor, so <c>new NoIndentScope()</c> would silently pick that one,
    /// skip the body entirely, and then restore a level of zero on disposal, flattening the indent for
    /// everything drawn afterwards. This is also the shape Unity's own scopes use.
    /// </para>
    /// </remarks>
    public sealed class NoIndentScope : GUI.Scope
    {
        private readonly int _indentLevel;

        /// <summary>Suspends the indent.</summary>
        public NoIndentScope()
        {
            _indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
        }

        /// <inheritdoc/>
        protected override void CloseScope() => EditorGUI.indentLevel = _indentLevel;
    }
}