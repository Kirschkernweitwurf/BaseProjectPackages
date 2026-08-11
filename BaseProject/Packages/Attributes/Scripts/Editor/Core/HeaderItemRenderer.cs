using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the controls declared by <see cref="HeaderButtonAttribute"/>,
    /// <see cref="HeaderLabelAttribute"/> and <see cref="HeaderDrawAttribute"/> into the header of a
    /// component or asset. Registered with Unity's header by <see cref="HeaderItemInjector"/>, which is
    /// why the entry point is a private method found by reflection rather than a normal call.
    /// </summary>
    /// <remarks>
    /// Unity hands out one square, icon-sized slot per header item. These controls carry words rather
    /// than an icon, so they are drawn extending leftwards from that slot into the empty part of the
    /// header, and a single slot is reported as consumed no matter how many were drawn.
    /// </remarks>
    public static class HeaderItemRenderer
    {
        /// <summary>Name of the method the header hook binds to. Kept here so the hook needs no literal.</summary>
        public const string DrawMethodName = nameof(DrawHeaderItems);

        private const string CancelLabel = "Cancel";
        private const string ConfirmLabel = "Confirm";
        private const float Gap = 2f;

        private const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const float MinimumWidth = 40f;
        private const string UndoFormat = "Header button {0}";

        private static GUIStyle LabelStyle => _labelStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight
        };

        private static readonly Dictionary<Type, HeaderItem[]> Items = new();


        private static GUIStyle _labelStyle;

        /// <summary>Returns the header controls declared by the given type, cached after the first call.</summary>
        /// <param name="type">The component or asset type to inspect.</param>
        /// <returns>The controls in declaration order.</returns>
        private static HeaderItem[] GetItems(Type type)
        {
            if (Items.TryGetValue(type, out HeaderItem[] cached))
                return cached;

            List<HeaderItem> items = new();

            CollectMethods(type, items);
            CollectProperties(type, items);

            HeaderItem[] result = items.ToArray();
            Items[type] = result;
            return result;
        }

        private static void CollectMethods(Type type, List<HeaderItem> items)
        {
            foreach (MethodInfo method in type.GetMethods(MemberFlags))
            {
                HeaderButtonAttribute button = method.GetCustomAttribute<HeaderButtonAttribute>();
                if (button != null && method.GetParameters().Length == 0)
                {
                    string label = string.IsNullOrEmpty(button.Label)
                        ? ObjectNames.NicifyVariableName(method.Name)
                        : button.Label;

                    items.Add(new HeaderItem(method, EHeaderItemKind.Button, button, label, button.Width));
                    continue;
                }

                HeaderLabelAttribute methodLabel = method.GetCustomAttribute<HeaderLabelAttribute>();
                if (methodLabel != null
                    && method.GetParameters().Length == 0
                    && method.ReturnType != typeof(void))
                {
                    items.Add(new HeaderItem(method, EHeaderItemKind.Label, null, null, methodLabel.Width));
                    continue;
                }

                HeaderDrawAttribute draw = method.GetCustomAttribute<HeaderDrawAttribute>();
                if (draw != null && TakesRect(method))
                    items.Add(new HeaderItem(method, EHeaderItemKind.Draw, null, null, draw.Width));
            }
        }

        private static void CollectProperties(Type type, List<HeaderItem> items)
        {
            foreach (PropertyInfo property in type.GetProperties(MemberFlags))
            {
                HeaderLabelAttribute label = property.GetCustomAttribute<HeaderLabelAttribute>();

                if (label != null && property.CanRead)
                    items.Add(new HeaderItem(property, EHeaderItemKind.Label, null, null, label.Width));
            }
        }

        private static bool TakesRect(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            return parameters.Length == 1 && parameters[0].ParameterType == typeof(Rect);
        }

        // Bound by HeaderItemInjector. The signature has to match Unity's header item delegate exactly:
        // the rect is the next free slot, and returning true tells Unity that slot is taken.
        private static bool DrawHeaderItems(Rect rect, Object[] targets)
        {
            if (rect.x < 0f || targets == null || targets.Length == 0)
                return false;

            Object first = targets[0];
            if (first == null)
                return false;

            HeaderItem[] items = GetItems(first.GetType());
            if (items.Length == 0)
                return false;

            float x = rect.xMax;

            foreach (HeaderItem item in items)
            {
                float width = Mathf.Max(item.Width, MinimumWidth);
                x -= width;

                Draw(new Rect(x, rect.y, width, rect.height), item, targets, first);
                x -= Gap;
            }

            return true;
        }

        private static void Draw(Rect rect, in HeaderItem item, Object[] targets, Object first)
        {
            switch (item.Kind)
            {
                case EHeaderItemKind.Label:
                    GUI.Label(rect, ReadLabel(item, first), LabelStyle);
                    break;
                case EHeaderItemKind.Draw:
                    Invoke(item, first, new object[]
                    {
                        rect
                    });

                    break;
                default:
                    DrawButton(rect, item, targets);
                    break;
            }
        }

        private static void DrawButton(Rect rect, in HeaderItem item, Object[] targets)
        {
            GUIContent content = new(item.Label, Describe(item));

            using (new EditorGUI.DisabledScope(!IsEnabled(item.Button.Mode)))
            {
                if (GUI.Button(rect, content, EditorStyles.miniButton) && Confirm(item))
                    Run(targets, item);
            }
        }

        private static string ReadLabel(in HeaderItem item, Object target)
        {
            object value = item.Member switch
            {
                PropertyInfo property => property.GetValue(target, null),
                MethodInfo method => method.Invoke(target, null),
                _ => null
            };

            return value?.ToString() ?? string.Empty;
        }

        private static void Invoke(in HeaderItem item, Object target, object[] arguments)
        {
            if (item.Member is MethodInfo method)
                method.Invoke(target, arguments);
        }

        // A header button runs outside the inspector's own edit flow, so nothing marks the object dirty
        // or asks for a repaint afterward. Without this the method runs and the inspector keeps showing
        // the values from before the click, which reads exactly like a button that does nothing.
        private static void Run(Object[] targets, in HeaderItem item)
        {
            if (item.Member is not MethodInfo method)
                return;

            Type declaring = method.DeclaringType;
            if (declaring == null)
                return;

            List<Object> affected = new();

            // A multi-object selection can mix types, and only the ones declaring the button can run it.
            foreach (Object target in targets)
            {
                if (target != null && declaring.IsInstanceOfType(target))
                    affected.Add(target);
            }

            if (affected.Count == 0)
                return;

            Undo.RecordObjects(affected.ToArray(), string.Format(UndoFormat, item.Label));

            foreach (Object target in affected)
            {
                method.Invoke(target, null);
                EditorUtility.SetDirty(target);
            }

            InternalEditorUtility.RepaintAllViews();
        }

        private static string Describe(in HeaderItem item)
        {
            string name = item.Member.Name;

            return item.Label == name
                ? name
                : $"{name}()";
        }

        private static bool IsEnabled(EButtonMode mode)
        {
            switch (mode)
            {
                case EButtonMode.PlayMode:
                    return Application.isPlaying;
                case EButtonMode.EditMode:
                    return !Application.isPlaying;
                default:
                    return true;
            }
        }

        private static bool Confirm(in HeaderItem item)
        {
            if (string.IsNullOrEmpty(item.Button.Confirm))
                return true;

            return EditorUtility.DisplayDialog(item.Label, item.Button.Confirm, ConfirmLabel, CancelLabel);
        }
    }
}