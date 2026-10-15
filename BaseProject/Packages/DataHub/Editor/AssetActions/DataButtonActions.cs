using System;
using System.Collections.Generic;
using System.Reflection;
using Base.AttributesPackage;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.AssetActions
{
    /// <summary>
    /// The <see cref="ButtonAttribute"/> methods of the selected assets, offered as Actions in the toolbar
    /// and on right click. An action runs on every selected row in one undo step, so a designer can run
    /// "Shuffle" or "Fill from folder" on ten decks at once instead of opening each one.
    /// </summary>
    internal static class DataButtonActions
    {
        private const string CancelLabel = "Cancel";
        private const string FailedFormat = "{0} failed on {1}: {2}";
        private const BindingFlags MethodFlags = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly;
        private const string RunLabel = "Run";

        /// <summary>
        /// Appends every button method all <paramref name="targets"/> share and that can run right now.
        /// Methods with parameters are left out, there is nothing to fill them with.
        /// </summary>
        /// <param name="targets">The selected assets.</param>
        /// <param name="methods">The list the methods are appended to.</param>
        internal static void Collect(Object[] targets, List<MethodInfo> methods)
        {
            Type shared = SharedType(targets);

            if (shared == null)
                return;

            // Declared methods per type, because a private button on a base class is not returned for
            // the derived type otherwise.
            for (Type type = shared; type != null && !DataSerializedFields.IsUnityType(type); type = type.BaseType)
            {
                foreach (MethodInfo method in type.GetMethods(MethodFlags))
                {
                    ButtonAttribute button = method.GetCustomAttribute<ButtonAttribute>(true);

                    if (button == null
                        || method.GetParameters().Length > 0
                        || !CanRunNow(button.Mode))
                        continue;

                    methods.Add(method);
                }
            }
        }

        /// <summary>The menu label of an action: the label of its button, or its nicified name.</summary>
        /// <param name="method">The button method.</param>
        /// <returns>The label.</returns>
        internal static string Label(MethodInfo method)
        {
            string label = method.GetCustomAttribute<ButtonAttribute>(true)?.Label;

            return string.IsNullOrEmpty(label)
                ? ObjectNames.NicifyVariableName(method.Name)
                : label;
        }

        /// <summary>
        /// Runs <paramref name="method"/> on every target, after its confirmation if it asks for one.
        /// </summary>
        /// <param name="method">The button method.</param>
        /// <param name="targets">The assets to run it on.</param>
        internal static void Run(MethodInfo method, Object[] targets)
        {
            string label = Label(method);
            string confirm = method.GetCustomAttribute<ButtonAttribute>(true)?.Confirm;

            if (!string.IsNullOrEmpty(confirm)
                && !EditorUtility.DisplayDialog(label, confirm, RunLabel, CancelLabel))
                return;

            int group = Undo.GetCurrentGroup();
            Undo.RecordObjects(targets, label);

            foreach (Object target in targets)
                Invoke(method, target, label);

            Undo.CollapseUndoOperations(group);
        }

        private static void Invoke(MethodInfo method, Object target, string label)
        {
            if (target == null)
                return;

            try
            {
                method.Invoke(target, null);
                EditorUtility.SetDirty(target);
            }
            catch (TargetInvocationException exception)
            {
                Exception cause = exception.InnerException ?? exception;
                CustomLogger.LogError(string.Format(FailedFormat, label, target.name, cause.Message), target);
            }
        }

        // The most derived type every target is an instance of, so an action only shows when it exists
        // on all of them.
        private static Type SharedType(Object[] targets)
        {
            if (targets.Length == 0
                || targets[0] == null)
                return null;

            Type shared = targets[0].GetType();

            foreach (Object target in targets)
            {
                if (target == null)
                    return null;

                while (!shared.IsInstanceOfType(target))
                    shared = shared.BaseType;
            }

            return shared;
        }

        private static bool CanRunNow(EButtonMode mode) => mode switch
        {
            EButtonMode.PlayMode => EditorApplication.isPlaying,
            EButtonMode.EditMode => !EditorApplication.isPlaying,
            _ => true
        };
    }
}