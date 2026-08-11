using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>
    /// Walks the inspected object's serialized properties during a scene view repaint and lets each
    /// handle drawer visualize the fields it owns. Writes go through the SerializedObject, so undo and
    /// prefab overrides behave exactly as they do in the inspector.
    /// </summary>
    /// <remarks>
    /// The SerializedObject is passed in rather than taken from the editor. An editor's own
    /// serializedObject belongs to the inspector, and the scene view can repaint while the inspector is
    /// mid-draw, which would leave two walks running over the same object. Unity warns about exactly
    /// this, so the caller owns a second one.
    /// </remarks>
    public static class HandleRenderer
    {
        /// <summary>
        /// Draws every handle of the given object. Call from an editor's OnSceneGUI, passing a
        /// SerializedObject that is not the inspector's own.
        /// </summary>
        /// <param name="serializedObject">The serialized view of the object being visualized.</param>
        public static void Draw(SerializedObject serializedObject)
        {
            Object target = serializedObject.targetObject;
            if (target == null)
                return;

            Type type = target.GetType();
            if (!HandleRegistry.HasAny(type))
                return;

            Transform transform = target is Component component
                ? component.transform
                : null;

            serializedObject.Update();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                Visit(iterator.Copy(), type, target, target, transform);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void Visit(SerializedProperty property, Type declaringType, object declaringObject,
            Object target, Transform transform)
        {
            FieldInfo field = ReflectionCache.GetField(declaringType, property.name);
            if (field == null)
                return;

            HandleBinding[] bindings = HandleRegistry.GetBindings(field);

            if (bindings.Length > 0)
            {
                HandleContext context = new(property, field, target, transform, declaringType, declaringObject);

                foreach (HandleBinding binding in bindings)
                    binding.Drawer.Draw(context, binding.Attribute);
            }

            Descend(property, field, target, transform);
        }

        // Nested serializable types are walked too, so a handle keeps working when the field is moved
        // into a settings struct instead of sitting directly on the component.
        private static void Descend(SerializedProperty property, FieldInfo field, Object target,
            Transform transform)
        {
            if (property.propertyType != SerializedPropertyType.Generic || property.isArray)
                return;

            Type nested = field.FieldType;
            if (nested == typeof(string) || FrameworkAssemblies.Contains(nested))
                return;

            if (!HandleRegistry.HasAny(nested))
                return;

            object instance = SerializedPropertyReflection.GetValue(property);
            if (instance == null)
                return;

            SerializedProperty iterator = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;
                Visit(iterator.Copy(), nested, instance, target, transform);
            }
        }
    }
}