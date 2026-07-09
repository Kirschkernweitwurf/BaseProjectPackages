using System.Reflection;
using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>Runs the custom method of a <see cref="ValidateInputAttribute"/> and reports failures.</summary>
    public sealed class ValidateInputHandler : IAfterFieldHandler
    {
        public int Order => 20;

        public void AfterField(in MemberContext context)
        {
            ValidateInputAttribute attribute = context.GetAttribute<ValidateInputAttribute>();
            if (attribute == null)
                return;

            MethodInfo method = ReflectionCache.GetMethod(context.Target.GetType(), attribute.MethodName);
            if (method == null)
                EditorGUILayout.HelpBox("Validation method not found: " + attribute.MethodName, MessageType.Warning);
            else if (!Invoke(method, context))
                EditorGUILayout.HelpBox(attribute.Message ?? "Validation failed: " + attribute.MethodName,
                    MessageType.Error);
        }

        private static bool Invoke(MethodInfo method, in MemberContext context)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] arguments;

            if (parameters.Length == 0)
                arguments = null;
            else if (parameters.Length == 1)
                arguments = new[]
                {
                    context.Field?.GetValue(context.Target)
                };
            else
                return true;

            try
            {
                object result = method.Invoke(context.Target, arguments);
                return !(result is bool valid) || valid;
            }
            catch
            {
                return true;
            }
        }
    }
}