using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// Works out who is behind a subscribed delegate: the object it runs on, the type whose code
    /// subscribed it, and a name for the method that a reader can recognize.
    /// </summary>
    /// <remarks>
    /// A lambda is not compiled into the type it was written in. It becomes a method on a generated
    /// closure class nested in that type, and the object it captured "this" into is an instance of
    /// that closure rather than the subscriber. Reported raw, every lambda subscription would name
    /// the same meaningless generated type, which is exactly the case where knowing the subscriber
    /// matters most: a lambda is the subscription most likely to outlive the object it captured.
    /// </remarks>
    internal static class SubscriberResolver
    {
        private const BindingFlags CaptureFlags = BindingFlags.Instance
            | BindingFlags.NonPublic
            | BindingFlags.Public;

        private const char GeneratedNameEnd = '>';
        private const char GeneratedNameStart = '<';
        private const string LambdaFormat = "{0}() lambda";
        private const string LocalFunctionFormat = "{0}() local function";
        private const char LocalFunctionMarker = '|';

        // The name the compiler gives the field it captures "this" into. It is not a legal C#
        // identifier, so nothing can reach it through nameof and it has to be spelled out.
        private const string ThisCaptureField = "<>4__this";

        /// <summary>
        /// The object the handler runs on, unwrapped out of a lambda's closure where there is one.
        /// </summary>
        /// <param name="handler">The subscribed delegate.</param>
        /// <returns>The subscriber instance, or <c>null</c> for a static method.</returns>
        internal static object ResolveOwner(Delegate handler)
        {
            object target = handler.Target;

            if (target == null)
                return null;

            if (target is Object)
                return target;

            FieldInfo captured = target.GetType().GetField(ThisCaptureField, CaptureFlags);

            if (captured == null)
                return target;

            object owner = captured.GetValue(target);

            // A destroyed Unity object is not a null reference, so it survives this check and gets
            // reported as the leak it is instead of falling back to the closure.
            if (owner == null)
                return target;

            return owner;
        }

        /// <summary>
        /// The type whose code subscribed the handler, with generated closure classes walked out of.
        /// </summary>
        /// <param name="handler">The subscribed delegate.</param>
        /// <returns>The declaring type, or <c>null</c> when the method has none.</returns>
        internal static Type ResolveDeclaringType(Delegate handler)
        {
            Type type = handler.Method.DeclaringType;

            while (type != null
                && IsGenerated(type)
                && type.DeclaringType != null)
                type = type.DeclaringType;

            return type;
        }

        /// <summary>
        /// A readable name for the subscribed method.
        /// </summary>
        /// <param name="handler">The subscribed delegate.</param>
        /// <returns>The method name, or the method a lambda or local function was written inside.</returns>
        internal static string DescribeMethod(Delegate handler)
        {
            string name = handler.Method.Name;

            if (name.Length == 0
                || name[0] != GeneratedNameStart)
                return name;

            int end = name.IndexOf(GeneratedNameEnd);

            if (end <= 1)
                return name;

            // A lambda or local function is named "<Awake>b__12_0". The part inside the brackets is
            // the method it was written in, which is the only part of that name worth showing.
            string origin = name[1..end];

            return name.IndexOf(LocalFunctionMarker) >= 0
                ? string.Format(LocalFunctionFormat, origin)
                : string.Format(LambdaFormat, origin);
        }

        private static bool IsGenerated(Type type) => type.IsDefined(typeof(CompilerGeneratedAttribute), false);
    }
}