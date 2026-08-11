using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Adds the entries of <see cref="CustomContextMenuAttribute"/> to the field's right-click menu.
    /// Runs early among the after-field handlers so the rect it listens on is still the field's own row.
    /// </summary>
    /// <remarks>
    /// Unity's own context menu is not extended here but replaced for that rect, because there is no
    /// public hook for adding to it from a plain handler. Copy and paste stay reachable from the label
    /// itself, which is outside the rect this claims.
    /// </remarks>
    public sealed class CustomContextMenuHandler : IAfterFieldHandler
    {
        private const int HandlerOrder = -180;

        public int Order => HandlerOrder;

        public void AfterField(in MemberContext context)
        {
            if (context.Field == null)
                return;

            if (Event.current.type != EventType.ContextClick)
                return;

            CustomContextMenuAttribute[] entries =
                (CustomContextMenuAttribute[])context.Field.GetCustomAttributes(
                    typeof(CustomContextMenuAttribute), true);

            if (entries.Length == 0)
                return;

            Rect row = GUILayoutUtility.GetLastRect();
            if (!row.Contains(Event.current.mousePosition))
                return;

            Show(context, entries);
            Event.current.Use();
        }

        private static void Show(in MemberContext context, CustomContextMenuAttribute[] entries)
        {
            GenericMenu menu = new();

            Type declaringType = context.DeclaringType;
            object declaringObject = context.DeclaringObject;
            SerializedObject serializedObject = context.Property.serializedObject;

            foreach (CustomContextMenuAttribute entry in entries)
            {
                MethodInfo method = ReflectionCache.GetMethod(declaringType, entry.Method);

                if (method == null || method.GetParameters().Length > 0 || declaringObject == null)
                {
                    menu.AddDisabledItem(new GUIContent(entry.Label));
                    continue;
                }

                menu.AddItem(new GUIContent(entry.Label), false, () =>
                {
                    // Pending inspector edits are written first, so the method sees the values the user
                    // is looking at rather than the ones from before this repaint.
                    serializedObject.ApplyModifiedProperties();
                    method.Invoke(declaringObject, null);
                    serializedObject.Update();
                });
            }

            menu.ShowAsContext();
        }
    }
}
