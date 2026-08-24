using System;
using Base.UtilityPackage;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// One subscriber row: a snapshot of a single delegate in the invocation list the bus would run
    /// for an event, taken the last time the window read the bus.
    /// </summary>
    internal sealed class HandlerEntry
    {
        private const string DestroyedTarget = "destroyed object";
        private const string MissingValue = "-";
        private const string StaticTarget = "static method";

        /// <summary>Short name of the type whose code subscribed this handler.</summary>
        internal string SubscriberName { get; }

        /// <summary>The subscribed method, with lambdas named after the method they sit in.</summary>
        internal string MethodName { get; }

        /// <summary>The object the handler runs on, described in the terms its state calls for.</summary>
        internal string TargetName { get; }

        /// <summary>The condition of this subscription.</summary>
        internal EHandlerState State { get; }

        /// <summary>The subscriber as a Unity object, or null when it is not one.</summary>
        internal Object Context { get; }

        /// <summary>
        /// True when there is a live Unity object behind this handler for the row to ping.
        /// </summary>
        /// <remarks>
        /// Resolved here rather than per repaint on purpose. Whether the row draws a button decides
        /// how many controls the row has, and IMGUI matches those against the last layout pass, so a
        /// check that could change between the two would eventually throw.
        /// </remarks>
        internal bool CanPing { get; }

        /// <summary>
        /// True when the handler still runs on an object Unity already destroyed, which means the
        /// subscription outlived its subscriber and nothing will ever remove it.
        /// </summary>
        internal bool IsLeak => State == EHandlerState.Destroyed;

        /// <summary>Creates the snapshot of a single subscription.</summary>
        /// <param name="handler">One delegate out of an event's invocation list.</param>
        internal HandlerEntry(Delegate handler)
        {
            Type declaring = SubscriberResolver.ResolveDeclaringType(handler);
            object owner = SubscriberResolver.ResolveOwner(handler);

            SubscriberName = declaring == null
                ? MissingValue
                : declaring.Name;

            MethodName = SubscriberResolver.DescribeMethod(handler);
            State = ResolveState(owner);
            TargetName = ResolveTarget(owner, State);
            Context = owner as Object;
            CanPing = Context != null;
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

            return Contains(SubscriberName, search)
                || Contains(MethodName, search)
                || Contains(TargetName, search);
        }

        private static bool Contains(string value, string search)
            => value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

        // Ordered so the Unity check runs before anything asks the object about itself: a destroyed
        // component answers GetType but throws on gameObject, and behind an object reference the
        // plain null check would report it as alive.
        private static EHandlerState ResolveState(object owner)
        {
            if (owner == null)
                return EHandlerState.Static;

            if (owner is not Object)
                return EHandlerState.Plain;

            return UnityObjectUtility.IsAlive(owner)
                ? EHandlerState.Live
                : EHandlerState.Destroyed;
        }

        // A plain C# subscriber has nothing but its type to go by, and that is already what the
        // Subscriber column says, so it is the one case that falls back to the type name.
        private static string ResolveTarget(object owner, EHandlerState state) => state switch
        {
            EHandlerState.Destroyed => DestroyedTarget,
            EHandlerState.Live => SceneLabel.Describe((Object)owner),
            EHandlerState.Static => StaticTarget,
            _ => owner.GetType().Name
        };
    }
}