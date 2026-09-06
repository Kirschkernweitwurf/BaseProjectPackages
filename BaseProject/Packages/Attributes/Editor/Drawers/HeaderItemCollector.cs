using System;
using System.Collections.Generic;
using System.Reflection;
using Base.AttributesPackage.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Drawers
{
    /// <summary>
    /// Reads the header controls a type declares, and the two things about a control that do not
    /// depend on drawing it: what its tooltip says and whether it is currently enabled.
    /// <para>
    /// Held apart from the drawing because every rule here fails silently. A method with the attribute
    /// but the wrong signature is skipped rather than reported, so a control that does not appear looks
    /// exactly like one that was never declared.
    /// </para>
    /// </summary>
    internal static class HeaderItemCollector
    {
        private const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>Reads the header controls the given type declares, methods first then properties.</summary>
        /// <param name="type">The component or asset type to inspect.</param>
        /// <returns>The controls in declaration order.</returns>
        internal static HeaderItem[] Collect(Type type)
        {
            List<HeaderItem> items = new();

            CollectMethods(type, items);
            CollectProperties(type, items);

            return items.ToArray();
        }

        /// <summary>
        /// The tooltip a control carries. Naming the member is the point, so a label that already says
        /// it is left alone rather than repeated with parentheses after it.
        /// </summary>
        /// <param name="item">The control to describe.</param>
        /// <returns>The tooltip text.</returns>
        internal static string Describe(in HeaderItem item)
        {
            string name = item.Member.Name;

            return item.Label == name
                ? name
                : $"{name}()";
        }

        /// <summary>Whether a button in the given mode can be pressed right now.</summary>
        /// <param name="mode">The editor state the button was declared for.</param>
        /// <returns>True while the editor is in that state.</returns>
        internal static bool IsEnabled(EButtonMode mode)
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
    }
}