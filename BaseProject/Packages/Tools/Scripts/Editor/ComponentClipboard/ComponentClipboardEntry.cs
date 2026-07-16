using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.ComponentClipboard
{
    /// <summary>
    /// Serialized snapshot of a single component. Stores the assembly qualified type name, the
    /// component values as JSON and all object references as global object ids, so the entry
    /// survives domain reloads.
    /// </summary>
    [Serializable]
    public class ComponentClipboardEntry
    {
        private const string ScriptPropertyPath = "m_Script";

        [SerializeField] private string typeName;
        [SerializeField] private string json;
        [SerializeField] private string displayName;
        [SerializeField] private List<ComponentReferenceEntry> references = new();

        /// <summary>Assembly qualified name of the captured component type.</summary>
        public string TypeName => typeName;

        /// <summary>Serialized values of the captured component.</summary>
        public string Json => json;

        /// <summary>Human-readable name used for list entries and menu labels.</summary>
        public string DisplayName => displayName;

        /// <summary>Creates an entry by capturing the current values of the given component.</summary>
        /// <param name="component">Component to capture. Must not be null.</param>
        public ComponentClipboardEntry(Component component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            Type type = component.GetType();
            typeName = type.AssemblyQualifiedName;
            json = EditorJsonUtility.ToJson(component);
            displayName = ObjectNames.NicifyVariableName(type.Name);
            CaptureReferences(component);
        }

        /// <summary>Resolves the stored type name back into a runtime type.</summary>
        /// <returns>The component type, or null when the type no longer exists.</returns>
        public Type ResolveType() => Type.GetType(typeName);

        /// <summary>Returns true when the stored type can still be resolved.</summary>
        public bool IsValid() => ResolveType() != null;

        /// <summary>
        /// Restores every captured object reference on the target. References that cannot be
        /// resolved are cleared, so no stale instance id survives.
        /// </summary>
        /// <param name="target">Component that already received the JSON values.</param>
        public void ApplyReferences(Component target)
        {
            if (target == null)
                return;

            SerializedObject serializedObject = new(target);

            foreach (ComponentReferenceEntry reference in references)
            {
                SerializedProperty property = serializedObject.FindProperty(reference.PropertyPath);

                if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                Object resolved = reference.Resolve();

                if (resolved == null && reference.HasTarget())
                    Debug.LogWarning($"Component Clipboard: reference '{reference.PropertyPath}' on "
                        + $"{displayName} could not be resolved and was cleared.");

                property.objectReferenceValue = resolved;
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Dispose();
        }

        private void CaptureReferences(Component component)
        {
            references.Clear();

            SerializedObject serializedObject = new(component);
            SerializedProperty property = serializedObject.GetIterator();

            while (property.NextVisible(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                if (property.propertyPath == ScriptPropertyPath)
                    continue;

                references.Add(new ComponentReferenceEntry(property.propertyPath, GetGlobalId(property)));
            }

            serializedObject.Dispose();
        }

        private string GetGlobalId(SerializedProperty property)
        {
            Object value = property.objectReferenceValue;

            if (value == null)
                return string.Empty;

            return GlobalObjectId.GetGlobalObjectIdSlow(value).ToString();
        }
    }
}