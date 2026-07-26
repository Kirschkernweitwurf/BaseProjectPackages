using Object = UnityEngine.Object;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Helper methods for working with <see cref="Object"/> references through non-Unity static types.
    /// </summary>
    public static class UnityObjectUtility
    {
        /// <summary>
        /// Checks whether a reference still points at a usable object.
        /// </summary>
        /// <remarks>
        /// <see cref="Object"/> overloads its equality operators, but the compiler only picks that overload when
        /// the static type of the operand is <see cref="Object"/> or a subclass of it. Behind an interface,
        /// <c>object</c>, or a generic type parameter, the plain reference comparison runs instead and a destroyed
        /// object is reported as alive. Route those checks through this method to get Unity's behavior back.
        /// </remarks>
        /// <param name="instance">The reference to check. Can be a plain C# object or a Unity object.</param>
        /// <returns>
        /// <c>true</c> if the reference is set and the object was not destroyed; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAlive(object instance)
        {
            if (instance is Object unityObject)
                return unityObject != null;

            return instance != null;
        }
    }
}
