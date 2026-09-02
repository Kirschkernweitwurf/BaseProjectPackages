using System;
using Base.UtilityPackage;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ServicesPackage.Editor
{
    /// <summary>
    /// One row of the <see cref="ServiceLocatorWindow"/>: a snapshot of a single entry in the
    /// <see cref="ServiceLocator"/> taken the last time the window read it.
    /// </summary>
    /// <remarks>
    /// Everything the row draws is resolved once here rather than per repaint, because a destroyed
    /// instance cannot be asked for its game object or its scene at all and the check that
    /// establishes that has to run before either of them.
    /// </remarks>
    internal sealed class ServiceRegistrationEntry
    {
        private const string MissingValue = "-";
        private const string PlainObjectLocation = "C# object";
        private const string SceneFormat = "{0} ({1})";

        /// <summary>The type the service is filed under, which is what callers ask for.</summary>
        internal Type RegisteredType { get; }

        /// <summary>Short name of <see cref="RegisteredType"/>.</summary>
        internal string TypeName { get; }

        /// <summary>Namespace of <see cref="RegisteredType"/>, or a dash when it has none.</summary>
        internal string NamespaceName { get; }

        /// <summary>Short name of the type the registered instance actually is.</summary>
        internal string InstanceTypeName { get; }

        /// <summary>The condition of this entry.</summary>
        internal EServiceState State { get; }

        /// <summary>The instance as a Unity object, or null for a plain C# service.</summary>
        internal Object Context { get; }

        /// <summary>
        /// True when there is a live Unity object behind this entry for the row to ping.
        /// </summary>
        /// <remarks>
        /// Resolved here rather than per repaint on purpose. Whether the row draws a button decides
        /// how many controls the row has, and IMGUI matches those against the last layout pass, so a
        /// check that could change between the two would eventually throw.
        /// </remarks>
        internal bool CanPing { get; }

        /// <summary>The game object and scene the instance lives in, or a dash when it has neither.</summary>
        internal string Location { get; }

        /// <summary>True for anything other than a healthy entry, which is what the filter keeps.</summary>
        internal bool IsProblem => State != EServiceState.Alive;

        /// <summary>Creates the snapshot of a single registration.</summary>
        /// <param name="registeredType">The type the service is filed under.</param>
        /// <param name="instance">The registered instance, which may already be destroyed.</param>
        internal ServiceRegistrationEntry(Type registeredType, IGameService instance)
        {
            RegisteredType = registeredType;
            TypeName = registeredType.Name;

            NamespaceName = string.IsNullOrEmpty(registeredType.Namespace)
                ? MissingValue
                : registeredType.Namespace;

            // The managed wrapper of a destroyed object still answers GetType, so the instance type
            // can be reported even for an entry that is no longer usable.
            InstanceTypeName = ReferenceEquals(instance, null)
                ? MissingValue
                : instance.GetType().Name;

            State = ResolveState(registeredType, instance);
            Context = instance as Object;
            CanPing = Context != null;
            Location = ResolveLocation(instance, State);
        }

        /// <summary>
        /// Reports whether this row survives the given search term.
        /// </summary>
        /// <param name="search">The term typed into the toolbar. An empty term matches everything.</param>
        /// <returns><c>true</c> when the term appears in any of the columns the row draws.</returns>
        internal bool Matches(string search)
        {
            if (string.IsNullOrEmpty(search))
                return true;

            return Contains(TypeName, search)
                || Contains(InstanceTypeName, search)
                || Contains(NamespaceName, search)
                || Contains(Location, search);
        }

        private static bool Contains(string value, string search)
            => value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

        // Register accepts any IGameService under any type, so an entry can be filed under a type its
        // instance does not implement. TryGet reports that as its own error, so the window does too.
        private static EServiceState ResolveState(Type registeredType, IGameService instance)
        {
            if (!UnityObjectUtility.IsAlive(instance))
                return EServiceState.Destroyed;

            return registeredType.IsInstanceOfType(instance)
                ? EServiceState.Alive
                : EServiceState.Mismatch;
        }

        private static string ResolveLocation(IGameService instance, EServiceState state)
        {
            if (state == EServiceState.Destroyed)
                return MissingValue;

            if (instance is Component component)
                return DescribeHost(component.gameObject);

            // A service does not have to be a component. A ScriptableObject one has an asset name
            // worth showing; a plain class has nothing but its type, which the Instance column
            // already carries, so naming it again here would say nothing.
            if (instance is Object unityObject)
                return unityObject.name;

            return PlainObjectLocation;
        }

        private static string DescribeHost(GameObject host)
        {
            string scene = host.scene.name;

            return string.IsNullOrEmpty(scene)
                ? host.name
                : string.Format(SceneFormat, host.name, scene);
        }
    }
}