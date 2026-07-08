using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Base inspector that renders foldout groups and inspector buttons for the attribute package.
    /// Derive concrete editors targeting MonoBehaviour and ScriptableObject.
    /// </summary>
    public abstract class AttributePackageEditor : UnityEditor.Editor
    {
        private const BindingFlags FieldFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        private const string KeySeparator = ".";

        private const BindingFlags MethodFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string ScriptPropertyPath = "m_Script";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawProperties();
            serializedObject.ApplyModifiedProperties();
            DrawButtons();
        }

        private static FieldInfo FindField(Type type, string name)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(name, FieldFlags);
                if (field != null)
                    return field;

                type = type.BaseType;
            }

            return null;
        }

        private static void DrawScriptField(SerializedProperty scriptProperty)
        {
            bool previousState = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptProperty, true);
            GUI.enabled = previousState;
        }

        private void DrawProperties()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            string activeFoldout = null;
            bool activeExpanded = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyPath == ScriptPropertyPath)
                {
                    DrawScriptField(iterator);
                    continue;
                }

                FoldoutAttribute foldout = GetFoldout(iterator.name);
                string foldoutName = foldout == null
                    ? null
                    : foldout.Name;

                if (foldoutName != activeFoldout)
                {
                    activeFoldout = foldoutName;
                    if (foldoutName != null)
                        activeExpanded = DrawFoldoutHeader(foldoutName);
                }

                if (foldoutName != null && !activeExpanded)
                    continue;

                if (foldoutName != null)
                    EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(iterator, true);

                if (foldoutName != null)
                    EditorGUI.indentLevel--;
            }
        }

        private bool DrawFoldoutHeader(string foldoutName)
        {
            string key = target.GetType().FullName + KeySeparator + foldoutName;
            bool expanded = EditorGUILayout.Foldout(EditorPrefs.GetBool(key, true), foldoutName, true);
            EditorPrefs.SetBool(key, expanded);
            return expanded;
        }

        private void DrawButtons()
        {
            MethodInfo[] methods = target.GetType().GetMethods(MethodFlags);
            foreach (MethodInfo method in methods)
            {
                ButtonAttribute button = method.GetCustomAttribute<ButtonAttribute>();
                if (button == null)
                    continue;

                if (method.GetParameters().Length > 0)
                    continue;

                string label = string.IsNullOrEmpty(button.Label)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : button.Label;

                if (GUILayout.Button(label))
                    InvokeOnTargets(method);
            }
        }

        private void InvokeOnTargets(MethodInfo method)
        {
            foreach (Object item in targets)
                method.Invoke(item, null);
        }

        private FoldoutAttribute GetFoldout(string fieldName)
        {
            FieldInfo field = FindField(target.GetType(), fieldName);
            return field == null
                ? null
                : field.GetCustomAttribute<FoldoutAttribute>();
        }
    }
}