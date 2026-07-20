using System.Reflection;
using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Detects inspector edits on <see cref="OnValueChangedAttribute"/> fields and invokes the named
    /// callback. Opens a change check right before the field is drawn and closes it right after, so the
    /// check captures only that field. The edited value is applied to the target before the callback.
    /// </summary>
    public sealed class OnValueChangedHandler : IBeforeFieldHandler, IAfterFieldHandler
    {
        int IBeforeFieldHandler.Order => 1000;
        int IAfterFieldHandler.Order => -100;

        public void BeforeField(in MemberContext context)
        {
            if (context.GetAttribute<OnValueChangedAttribute>() == null)
                return;

            EditorGUI.BeginChangeCheck();
        }

        public void AfterField(in MemberContext context)
        {
            OnValueChangedAttribute attribute = context.GetAttribute<OnValueChangedAttribute>();
            if (attribute == null)
                return;

            if (!EditorGUI.EndChangeCheck())
                return;

            context.Editor.serializedObject.ApplyModifiedProperties();
            Invoke(context, attribute.Method);
            context.Editor.Repaint();
        }

        private static void Invoke(in MemberContext context, string methodName)
        {
            MethodInfo method = ReflectionCache.GetMethod(context.DeclaringType, methodName);
            if (method == null || context.DeclaringObject == null)
                return;

            ParameterInfo[] parameters = method.GetParameters();
            object[] arguments;

            if (parameters.Length == 0)
                arguments = null;
            else if (parameters.Length == 1)
                arguments = new[] { context.Field?.GetValue(context.DeclaringObject) };
            else
                return;

            method.Invoke(context.DeclaringObject, arguments);
        }
    }
}