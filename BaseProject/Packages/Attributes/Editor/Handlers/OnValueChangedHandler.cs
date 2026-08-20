using System.Reflection;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.Drawers;
using UnityEditor;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Detects inspector edits on <see cref="OnValueChangedAttribute"/> fields and invokes the named
    /// callback. Opens a change check right before the field is drawn and closes it right after, so the
    /// check captures only that field. The edited value is applied to the target before the callback.
    /// </summary>
    /// <remarks>
    /// The after-field order has to be the lowest of any handler, because the handlers that draw a
    /// control over the field's own row run in that same phase. Closing the check after them would count
    /// a click on a foldout arrow or a prefix toggle as an edit of the field they sit in front of.
    /// </remarks>
    internal sealed class OnValueChangedHandler : IBeforeFieldHandler, IAfterFieldHandler
    {
        private const int AfterFieldOrder = -300;
        private const int BeforeFieldOrder = 1000;

        int IBeforeFieldHandler.Order => BeforeFieldOrder;

        int IAfterFieldHandler.Order => AfterFieldOrder;

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

        public void BeforeField(in MemberContext context)
        {
            if (context.GetAttribute<OnValueChangedAttribute>() == null)
                return;

            EditorGUI.BeginChangeCheck();
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
                arguments = new[]
                {
                    context.Field?.GetValue(context.DeclaringObject)
                };
            else
                return;

            method.Invoke(context.DeclaringObject, arguments);
        }
    }
}