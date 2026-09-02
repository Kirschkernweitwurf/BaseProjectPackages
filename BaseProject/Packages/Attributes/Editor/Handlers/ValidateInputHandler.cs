using System;
using System.Reflection;
using Base.AttributesPackage.Editor.Core;
using Base.AttributesPackage.Editor.Core.Interfaces;
using Base.AttributesPackage.Editor.Drawers;
using Base.UtilityPackage.Logging;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>
    /// Runs the custom method of a <see cref="ValidateInputAttribute"/> and reports what it returns.
    /// </summary>
    /// <remarks>
    /// The method may return a bool or a <see cref="ValidationResult"/>. A bool can only say pass or
    /// fail, always with the one message baked into the attribute; a result lets the validator say which
    /// of several things went wrong and whether it is worth an error or only a warning.
    /// </remarks>
    internal sealed class ValidateInputHandler : IAfterFieldHandler
    {
        private const string FailedPrefix = "Validation failed: ";
        private const int HandlerOrder = 20;
        private const string MissingPrefix = "Validation method not found: ";
        private const string ThrewPrefix = "Validation method threw an exception: ";

        /// <inheritdoc/>
        public int Order => HandlerOrder;

        /// <inheritdoc/>
        public void AfterField(in MemberContext context)
        {
            ValidateInputAttribute attribute = context.GetAttribute<ValidateInputAttribute>();
            if (attribute == null)
                return;

            MethodInfo method = ReflectionCache.GetMethod(context.DeclaringType, attribute.MethodName);
            if (method == null)
            {
                CompactHelpBox.Warning(MissingPrefix + attribute.MethodName);
                return;
            }

            ValidationResult result = Invoke(method, context);
            if (result.IsValid)
                return;

            // The validator's own message wins, since it knows which of several checks failed. The
            // attribute's message is the fallback for a validator that only returns a bool.
            string message = result.Message
                ?? ValueResolver.Text(context, attribute.Message)
                ?? FailedPrefix + attribute.MethodName;

            FixableHelpBox.Draw(context, message, ToInfoBoxType(result.Severity), attribute.FixAction,
                attribute.FixActionName ?? ValidateInputAttribute.DefaultFixLabel);
        }

        private static EInfoBoxType ToInfoBoxType(EValidationSeverity severity)
            => severity == EValidationSeverity.Warning
                ? EInfoBoxType.Warning
                : EInfoBoxType.Error;

        private static ValidationResult Invoke(MethodInfo method, in MemberContext context)
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
                return ValidationResult.Valid;

            try
            {
                return Interpret(method.Invoke(context.DeclaringObject, arguments));
            }
            catch (Exception exception)
            {
                // A throwing validator is a bug in the validator, not an invalid value. Report it and
                // let the field pass, so the inspector stays usable.
                CustomLogger.LogError(ThrewPrefix + method.Name + "\n" + exception, context.Target);
                return ValidationResult.Valid;
            }
        }

        private static ValidationResult Interpret(object returned)
        {
            switch (returned)
            {
                case ValidationResult result:
                    return result;
                case bool valid:
                    return valid
                        ? ValidationResult.Valid
                        : ValidationResult.Error(null);
                default:
                    // A validator returning neither is a signature mistake. Passing is the safe reading,
                    // and the troubleshoot window reports the signature separately.
                    return ValidationResult.Valid;
            }
        }
    }
}