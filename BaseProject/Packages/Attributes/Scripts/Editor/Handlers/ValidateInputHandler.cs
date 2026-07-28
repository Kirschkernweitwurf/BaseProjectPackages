using System;
using System.Reflection;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>Runs the custom method of a <see cref="ValidateInputAttribute"/> and reports failures.</summary>
    public sealed class ValidateInputHandler : IAfterFieldHandler
    {
        private const string FailedPrefix = "Validation failed: ";
        private const string MissingPrefix = "Validation method not found: ";
        private const string ThrewPrefix = "Validation method threw an exception: ";

        public int Order => 20;

        public void AfterField(in MemberContext context)
        {
            ValidateInputAttribute attribute = context.GetAttribute<ValidateInputAttribute>();
            if (attribute == null)
                return;

            MethodInfo method = ReflectionCache.GetMethod(context.DeclaringType, attribute.MethodName);
            if (method == null)
                EditorGUILayout.HelpBox(MissingPrefix + attribute.MethodName, MessageType.Warning);
            else if (!Invoke(method, context))
                EditorGUILayout.HelpBox(attribute.Message ?? FailedPrefix + attribute.MethodName, MessageType.Error);
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
                    context.Field?.GetValue(context.DeclaringObject)
                };
            else
                return true;

            try
            {
                object result = method.Invoke(context.DeclaringObject, arguments);
                return result is not bool valid || valid;
            }
            catch (Exception exception)
            {
                // A throwing validator is a bug in the validator, not an invalid value. Report it and
                // let the field pass, so the inspector stays usable.
                CustomLogger.LogError(ThrewPrefix + method.Name + "\n" + exception, context.Target);
                return true;
            }
        }
    }
}