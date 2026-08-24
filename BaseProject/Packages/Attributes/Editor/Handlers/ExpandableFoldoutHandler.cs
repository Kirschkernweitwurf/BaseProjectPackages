using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.Drawers;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Draws the foldout arrow in front of an <see cref="ExpandableAttribute"/> field. The arrow sits in
    /// the gutter <see cref="LeadingGutter"/> reserves for it, so an expandable reference reads like any
    /// other foldout rather than carrying a separate button.
    /// </summary>
    /// <remarks>
    /// The arrow is drawn over the row the field just occupied rather than into a reserved rect, because
    /// the pipeline only offers trailing widgets and a foldout belongs in front. That is also why this
    /// runs first among the after-field handlers: any handler drawing a row before it would move the
    /// rect out from under the arrow.
    /// </remarks>
    internal sealed class ExpandableFoldoutHandler : IAfterFieldHandler
    {
        private const int HandlerOrder = -200;

        public int Order => HandlerOrder;

        public void AfterField(in MemberContext context)
        {
            ExpandableAttribute attribute = context.GetAttribute<ExpandableAttribute>();
            if (attribute == null || !ExpandableState.NeedsArrow(context))
                return;

            // The rect from the layout pass is a placeholder, so there is nothing to draw over yet.
            if (Event.current.type == EventType.Layout)
                return;

            Rect row = GUILayoutUtility.GetLastRect();
            Rect arrow = LeadingGutter.RectFor(row, EditorGUI.indentLevel, EditorGUIUtility.singleLineHeight);

            bool stored = ExpandableState.IsExpanded(context, attribute);
            bool expanded;

            // The gutter rect already accounts for the indent, so the control must not apply it again.
            using (new NoIndentScope())
                expanded = EditorGUI.Foldout(arrow, stored, GUIContent.none, true);

            if (expanded != stored)
                ExpandableState.SetExpanded(context, expanded);
        }
    }
}