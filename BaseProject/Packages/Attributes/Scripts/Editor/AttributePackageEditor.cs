using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Base inspector that renders foldout groups, tab groups, indentation, conditional
    /// visibility, validation, read-only native members and inspector buttons for the
    /// attribute package. Derive concrete editors targeting MonoBehaviour and ScriptableObject.
    /// </summary>
    public abstract class AttributePackageEditor : UnityEditor.Editor
    {
        private const BindingFlags FieldFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private const BindingFlags MethodFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const string KeySeparator = ".";
        private const string TabKeyPrefix = "TAB";
        private const string ScriptPropertyPath = "m_Script";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawProperties();
            serializedObject.ApplyModifiedProperties();
            DrawNativeMembers();
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

        private static PropertyInfo FindProperty(Type type, string name)
        {
            while (type != null)
            {
                foreach (PropertyInfo property in type.GetProperties(FieldFlags))
                    if (property.Name == name)
                        return property;

                type = type.BaseType;
            }

            return null;
        }

        private static MethodInfo FindMethod(Type type, string name)
        {
            while (type != null)
            {
                foreach (MethodInfo method in type.GetMethods(FieldFlags))
                    if (method.Name == name)
                        return method;

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
            List<SerializedProperty> properties = new List<SerializedProperty>();
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyPath == ScriptPropertyPath)
                {
                    DrawScriptField(iterator);
                    continue;
                }

                properties.Add(iterator.Copy());
            }

            int index = 0;
            string activeFoldout = null;
            bool activeExpanded = true;

            while (index < properties.Count)
            {
                SerializedProperty property = properties[index];

                TabAttribute tab = GetMemberAttribute<TabAttribute>(property.name);
                if (tab != null)
                {
                    activeFoldout = null;
                    index = DrawTabGroup(properties, index);
                    continue;
                }

                FoldoutAttribute foldout = GetMemberAttribute<FoldoutAttribute>(property.name);
                string foldoutName = foldout?.Name;

                if (foldoutName != activeFoldout)
                {
                    activeFoldout = foldoutName;
                    if (foldoutName != null)
                        activeExpanded = DrawFoldoutHeader(foldoutName);
                }

                if (foldoutName != null && !activeExpanded)
                {
                    index++;
                    continue;
                }

                if (foldoutName != null)
                    EditorGUI.indentLevel++;

                DrawMemberProperty(property);

                if (foldoutName != null)
                    EditorGUI.indentLevel--;

                index++;
            }
        }

        private int DrawTabGroup(List<SerializedProperty> properties, int startIndex)
        {
            TabAttribute first = GetMemberAttribute<TabAttribute>(properties[startIndex].name);
            string group = first.Group;

            List<SerializedProperty> members = new List<SerializedProperty>();
            List<string> memberTabs = new List<string>();
            List<string> tabOrder = new List<string>();

            int index = startIndex;
            while (index < properties.Count)
            {
                TabAttribute tab = GetMemberAttribute<TabAttribute>(properties[index].name);
                if (tab == null || tab.Group != group)
                    break;

                members.Add(properties[index]);
                memberTabs.Add(tab.Name);
                if (!tabOrder.Contains(tab.Name))
                    tabOrder.Add(tab.Name);

                index++;
            }

            string key = target.GetType().FullName + KeySeparator + TabKeyPrefix + KeySeparator + group;
            int stored = Mathf.Clamp(EditorPrefs.GetInt(key, 0), 0, tabOrder.Count - 1);

            int selected = GUILayout.Toolbar(stored, tabOrder.ToArray());
            if (selected != stored)
                EditorPrefs.SetInt(key, selected);

            string selectedTab = tabOrder[selected];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = 0; i < members.Count; i++)
            {
                if (memberTabs[i] != selectedTab)
                    continue;

                DrawMemberProperty(members[i]);
            }
            EditorGUILayout.EndVertical();

            return index;
        }

        private void DrawMemberProperty(SerializedProperty property)
        {
            if (!IsVisible(property))
                return;

            bool enabled = IsEnabled(property);

            IndentAttribute indent = GetMemberAttribute<IndentAttribute>(property.name);
            int amount = indent?.Amount ?? 0;

            bool isObjectRef = property.propertyType == SerializedPropertyType.ObjectReference;
            Object before = isObjectRef ? property.objectReferenceValue : null;

            EditorGUI.indentLevel += amount;
            using (new EditorGUI.DisabledScope(!enabled))
                EditorGUILayout.PropertyField(property, true);
            EditorGUI.indentLevel -= amount;

            if (isObjectRef)
                EnforceObjectConstraint(property, before);

            ApplyMinMax(property);
            DrawValidationMessages(property);
        }

        private bool IsVisible(SerializedProperty property)
        {
            ShowIfAttribute showIf = GetMemberAttribute<ShowIfAttribute>(property.name);
            if (showIf != null && !ResolveBool(showIf.Member))
                return false;

            HideIfAttribute hideIf = GetMemberAttribute<HideIfAttribute>(property.name);
            if (hideIf != null && ResolveBool(hideIf.Member))
                return false;

            ShowIfEnumAttribute showEnum = GetMemberAttribute<ShowIfEnumAttribute>(property.name);
            if (showEnum != null && !EnumMatches(showEnum))
                return false;

            return true;
        }

        private bool IsEnabled(SerializedProperty property)
        {
            EnableIfAttribute enableIf = GetMemberAttribute<EnableIfAttribute>(property.name);
            if (enableIf != null && !ResolveBool(enableIf.Member))
                return false;

            DisableIfAttribute disableIf = GetMemberAttribute<DisableIfAttribute>(property.name);
            if (disableIf != null && ResolveBool(disableIf.Member))
                return false;

            return true;
        }

        private bool ResolveBool(string member)
        {
            SerializedProperty property = serializedObject.FindProperty(member);
            if (property != null && property.propertyType == SerializedPropertyType.Boolean)
                return property.boolValue;

            Type type = target.GetType();

            FieldInfo field = FindField(type, member);
            if (field != null && field.FieldType == typeof(bool))
                return (bool)field.GetValue(target);

            PropertyInfo info = FindProperty(type, member);
            if (info != null && info.CanRead && info.PropertyType == typeof(bool))
                return (bool)info.GetValue(target, null);

            MethodInfo method = FindMethod(type, member);
            if (method != null && method.ReturnType == typeof(bool) && method.GetParameters().Length == 0)
                return (bool)method.Invoke(target, null);

            return true;
        }

        private bool EnumMatches(ShowIfEnumAttribute attribute)
        {
            object current = ResolveEnumValue(attribute.Member);
            if (current == null)
                return true;

            foreach (object value in attribute.Values)
                if (Equals(current, value))
                    return true;

            return false;
        }

        private object ResolveEnumValue(string member)
        {
            Type type = target.GetType();

            FieldInfo field = FindField(type, member);
            if (field != null)
                return field.GetValue(target);

            PropertyInfo property = FindProperty(type, member);
            if (property != null && property.CanRead)
                return property.GetValue(target, null);

            return null;
        }

        private void ApplyMinMax(SerializedProperty property)
        {
            MinMaxAttribute minMax = GetMemberAttribute<MinMaxAttribute>(property.name);
            if (minMax == null)
                return;

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int min = Mathf.RoundToInt(minMax.Min);
                int max = Mathf.RoundToInt(minMax.Max);
                int clamped = Mathf.Clamp(property.intValue, min, max);
                if (clamped != property.intValue)
                    property.intValue = clamped;
            }
            else if (property.propertyType == SerializedPropertyType.Float)
            {
                float clamped = Mathf.Clamp(property.floatValue, minMax.Min, minMax.Max);
                if (!Mathf.Approximately(clamped, property.floatValue))
                    property.floatValue = clamped;
            }
        }

        private void EnforceObjectConstraint(SerializedProperty property, Object before)
        {
            Object current = property.objectReferenceValue;
            if (current == null || current == before)
                return;

            bool assetOnly = GetMemberAttribute<AssetOnlyAttribute>(property.name) != null;
            bool sceneOnly = GetMemberAttribute<SceneObjectOnlyAttribute>(property.name) != null;

            if (assetOnly && !IsAsset(current))
                property.objectReferenceValue = before;
            else if (sceneOnly && !IsSceneObject(current))
                property.objectReferenceValue = before;
        }

        private void DrawValidationMessages(SerializedProperty property)
        {
            RequiredAttribute required = GetMemberAttribute<RequiredAttribute>(property.name);
            if (required != null && IsMissingReference(property))
                EditorGUILayout.HelpBox(
                    required.Message ?? ObjectNames.NicifyVariableName(property.name) + " is required.",
                    MessageType.Error);

            NotNullOrEmptyAttribute notEmpty = GetMemberAttribute<NotNullOrEmptyAttribute>(property.name);
            if (notEmpty != null && IsNullOrEmpty(property))
                EditorGUILayout.HelpBox(
                    notEmpty.Message ?? ObjectNames.NicifyVariableName(property.name) + " must not be empty.",
                    MessageType.Error);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                Object current = property.objectReferenceValue;
                if (current != null)
                {
                    if (GetMemberAttribute<AssetOnlyAttribute>(property.name) != null && !IsAsset(current))
                        EditorGUILayout.HelpBox("Only project assets are allowed here.", MessageType.Error);
                    else if (GetMemberAttribute<SceneObjectOnlyAttribute>(property.name) != null && !IsSceneObject(current))
                        EditorGUILayout.HelpBox("Only scene objects are allowed here.", MessageType.Error);
                }
            }

            ValidateInputAttribute validate = GetMemberAttribute<ValidateInputAttribute>(property.name);
            if (validate == null)
                return;

            MethodInfo method = FindMethod(target.GetType(), validate.MethodName);
            if (method == null)
                EditorGUILayout.HelpBox("Validation method not found: " + validate.MethodName, MessageType.Warning);
            else if (!InvokeValidate(method, property))
                EditorGUILayout.HelpBox(
                    validate.Message ?? "Validation failed: " + validate.MethodName,
                    MessageType.Error);
        }

        private bool InvokeValidate(MethodInfo method, SerializedProperty property)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] arguments;

            if (parameters.Length == 0)
            {
                arguments = null;
            }
            else if (parameters.Length == 1)
            {
                FieldInfo field = FindField(target.GetType(), property.name);
                arguments = new object[] { field?.GetValue(target) };
            }
            else
            {
                return true;
            }

            try
            {
                object result = method.Invoke(target, arguments);
                return !(result is bool valid) || valid;
            }
            catch
            {
                return true;
            }
        }

        private static bool IsMissingReference(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.ObjectReference
               && property.objectReferenceValue == null;

        private static bool IsNullOrEmpty(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String)
                return string.IsNullOrEmpty(property.stringValue);

            if (property.isArray)
                return property.arraySize == 0;

            return false;
        }

        private static bool IsAsset(Object value)
            => value != null && EditorUtility.IsPersistent(value);

        private static bool IsSceneObject(Object value)
            => value != null && !EditorUtility.IsPersistent(value) && (value is GameObject || value is Component);

        private bool DrawFoldoutHeader(string foldoutName)
        {
            string key = target.GetType().FullName + KeySeparator + foldoutName;
            bool expanded = EditorGUILayout.Foldout(EditorPrefs.GetBool(key, true), foldoutName, true);
            EditorPrefs.SetBool(key, expanded);
            return expanded;
        }

        private void DrawNativeMembers()
        {
            Type type = target.GetType();

            foreach (FieldInfo field in GetNonSerializedFields(type))
            {
                object value = field.GetValue(target);
                DrawReadOnlyValue(ObjectNames.NicifyVariableName(field.Name), value);
            }

            foreach (PropertyInfo property in GetNativeProperties(type))
            {
                object value;
                try
                {
                    value = property.GetValue(target, null);
                }
                catch (Exception exception)
                {
                    value = exception.Message;
                }

                DrawReadOnlyValue(ObjectNames.NicifyVariableName(property.Name), value);
            }
        }

        private static IEnumerable<FieldInfo> GetNonSerializedFields(Type type)
        {
            while (type != null)
            {
                FieldInfo[] fields = type.GetFields(FieldFlags);
                foreach (FieldInfo field in fields)
                    if (field.GetCustomAttribute<ShowNonSerializedAttribute>() != null)
                        yield return field;

                type = type.BaseType;
            }
        }

        private static IEnumerable<PropertyInfo> GetNativeProperties(Type type)
        {
            while (type != null)
            {
                PropertyInfo[] properties = type.GetProperties(FieldFlags);
                foreach (PropertyInfo property in properties)
                    if (property.CanRead && property.GetCustomAttribute<ShowNativePropertyAttribute>() != null)
                        yield return property;

                type = type.BaseType;
            }
        }

        private static void DrawReadOnlyValue(string label, object value)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                switch (value)
                {
                    case null:
                        EditorGUILayout.TextField(label, "null");
                        break;
                    case int intValue:
                        EditorGUILayout.IntField(label, intValue);
                        break;
                    case float floatValue:
                        EditorGUILayout.FloatField(label, floatValue);
                        break;
                    case bool boolValue:
                        EditorGUILayout.Toggle(label, boolValue);
                        break;
                    case string stringValue:
                        EditorGUILayout.TextField(label, stringValue);
                        break;
                    case Vector2 vector2Value:
                        EditorGUILayout.Vector2Field(label, vector2Value);
                        break;
                    case Vector3 vector3Value:
                        EditorGUILayout.Vector3Field(label, vector3Value);
                        break;
                    case Color colorValue:
                        EditorGUILayout.ColorField(label, colorValue);
                        break;
                    case Enum enumValue:
                        EditorGUILayout.TextField(label, enumValue.ToString());
                        break;
                    case Object objectValue:
                        EditorGUILayout.ObjectField(label, objectValue, objectValue.GetType(), true);
                        break;
                    default:
                        EditorGUILayout.LabelField(label, value.ToString());
                        break;
                }
            }
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

                using (new EditorGUI.DisabledScope(!IsButtonEnabled(button.Mode)))
                {
                    if (GUILayout.Button(label) && ConfirmButton(button, label))
                        InvokeOnTargets(method);
                }
            }
        }

        private static bool IsButtonEnabled(EButtonMode mode)
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

        private static bool ConfirmButton(ButtonAttribute button, string label)
        {
            if (string.IsNullOrEmpty(button.Confirm))
                return true;

            return EditorUtility.DisplayDialog(label, button.Confirm, "Confirm", "Cancel");
        }

        private void InvokeOnTargets(MethodInfo method)
        {
            foreach (Object item in targets)
                method.Invoke(item, null);
        }

        private T GetMemberAttribute<T>(string fieldName) where T : Attribute
        {
            FieldInfo field = FindField(target.GetType(), fieldName);
            return field?.GetCustomAttribute<T>();
        }
    }
}
