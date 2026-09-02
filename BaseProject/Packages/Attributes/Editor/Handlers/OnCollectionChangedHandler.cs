using System.Collections.Generic;
using System.Reflection;
using Base.AttributesPackage.Editor.Core.Interfaces;
using UnityEditor;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>
    /// Calls the before and after methods of <see cref="OnCollectionChangedAttribute"/> around a change
    /// to the element count.
    /// </summary>
    /// <remarks>
    /// The before method runs while the old contents are still there, which is the whole point of the
    /// attribute: a collection that owns something has to release what is leaving before the list
    /// forgets it existed. It therefore runs in the before-field phase, ahead of the drawing that will
    /// change the size, and the after method runs once the new size is known.
    /// </remarks>
    internal sealed class OnCollectionChangedHandler : IBeforeFieldHandler, IAfterFieldHandler
    {
        private const int AfterFieldOrder = -280;
        private const int BeforeFieldOrder = 999;
        private const string KeySeparator = ":";

        int IBeforeFieldHandler.Order => BeforeFieldOrder;

        int IAfterFieldHandler.Order => AfterFieldOrder;

        // Handlers are shared across inspectors, so the recorded size is keyed by target and path rather
        // than held in a field. Entries are removed as soon as they are consumed.
        private static readonly Dictionary<string, int> Recorded = new();

        /// <inheritdoc/>
        public void AfterField(in MemberContext context)
        {
            OnCollectionChangedAttribute attribute = context.GetAttribute<OnCollectionChangedAttribute>();
            if (attribute == null)
                return;

            string key = KeyFor(context);
            if (!Recorded.TryGetValue(key, out int before))
                return;

            Recorded.Remove(key);

            int after = context.Property.arraySize;
            if (after == before)
                return;

            // The before method is called here rather than in the before-field phase, because that phase
            // runs on every repaint and the change is only known once the field has been drawn. Undoing
            // the edit for the duration of the call is what lets it see the old contents.
            context.Property.arraySize = before;
            context.Editor.serializedObject.ApplyModifiedProperties();
            Invoke(context, attribute.Before, before);

            context.Property.arraySize = after;
            context.Editor.serializedObject.ApplyModifiedProperties();
            Invoke(context, attribute.After, after);

            context.Editor.Repaint();
        }

        /// <inheritdoc/>
        public void BeforeField(in MemberContext context)
        {
            if (context.GetAttribute<OnCollectionChangedAttribute>() == null)
                return;

            if (!IsCollection(context.Property))
                return;

            Recorded[KeyFor(context)] = context.Property.arraySize;
        }

        private static bool IsCollection(SerializedProperty property)
            => property.isArray && property.propertyType != SerializedPropertyType.String;

        private static string KeyFor(in MemberContext context)
            => context.Target.GetInstanceID() + KeySeparator + context.Property.propertyPath;

        private static void Invoke(in MemberContext context, string methodName, int size)
        {
            if (string.IsNullOrEmpty(methodName) || context.DeclaringObject == null)
                return;

            MethodInfo method = ReflectionCache.GetMethod(context.DeclaringType, methodName);
            if (method == null)
                return;

            ParameterInfo[] parameters = method.GetParameters();
            object[] arguments;

            if (parameters.Length == 0)
                arguments = null;
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
                arguments = new object[]
                {
                    size
                };
            else
                return;

            method.Invoke(context.DeclaringObject, arguments);
        }
    }
}