using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Detects element count changes on <see cref="OnArraySizeChangedAttribute"/> fields and invokes the
    /// named callback. The size before the field is drawn is recorded and compared afterwards, so edits
    /// that only change element values do not fire. The new size is applied before the callback runs.
    /// </summary>
    public sealed class OnArraySizeChangedHandler : IBeforeFieldHandler, IAfterFieldHandler
    {
        private const string KeySeparator = ":";

        // Handlers are shared across inspectors, so the recorded size is keyed by target and path
        // instead of held in an instance field. Entries are removed as soon as they are consumed.
        private static readonly Dictionary<string, int> Recorded = new();

        int IBeforeFieldHandler.Order => 1000;

        int IAfterFieldHandler.Order => -110;

        public void BeforeField(in MemberContext context)
        {
            if (context.GetAttribute<OnArraySizeChangedAttribute>() == null)
                return;

            if (!context.Property.isArray || context.Property.propertyType == SerializedPropertyType.String)
                return;

            Recorded[KeyFor(context)] = context.Property.arraySize;
        }

        public void AfterField(in MemberContext context)
        {
            OnArraySizeChangedAttribute attribute = context.GetAttribute<OnArraySizeChangedAttribute>();
            if (attribute == null)
                return;

            string key = KeyFor(context);
            if (!Recorded.TryGetValue(key, out int before))
                return;

            Recorded.Remove(key);

            int after = context.Property.arraySize;
            if (after == before)
                return;

            context.Editor.serializedObject.ApplyModifiedProperties();
            Invoke(context, attribute.Method, after);
            context.Editor.Repaint();
        }

        private static string KeyFor(in MemberContext context)
            => context.Target.GetInstanceID() + KeySeparator + context.Property.propertyPath;

        private static void Invoke(in MemberContext context, string methodName, int size)
        {
            MethodInfo method = ReflectionCache.GetMethod(context.DeclaringType, methodName);
            if (method == null || context.DeclaringObject == null)
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
