using System;
using System.Collections.Generic;
using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Discovers and instantiates all handlers via <see cref="TypeCache"/>. Adding a handler
    /// requires no registration: create a class implementing a handler interface and it is picked up.
    /// Handlers must be stateless, since one instance is shared across all inspectors.
    /// </summary>
    public static class HandlerRegistry
    {
        /// <summary>All before-field handlers, sorted by order.</summary>
        public static IBeforeFieldHandler[] BeforeField => _beforeField ??= CreateBeforeField();

        /// <summary>All visibility handlers.</summary>
        public static IVisibilityHandler[] Visibility => _visibility ??= Create<IVisibilityHandler>();

        /// <summary>All enable handlers.</summary>
        public static IEnableHandler[] Enable => _enable ??= Create<IEnableHandler>();

        /// <summary>All after-field handlers, sorted by order.</summary>
        public static IAfterFieldHandler[] AfterField => _afterField ??= CreateAfterField();

        private static IBeforeFieldHandler[] _beforeField;
        private static IVisibilityHandler[] _visibility;
        private static IEnableHandler[] _enable;
        private static IAfterFieldHandler[] _afterField;

        private static T[] Create<T>()
        {
            List<T> handlers = new();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<T>())
            {
                if (!type.IsAbstract && !type.IsInterface)
                    handlers.Add((T)Activator.CreateInstance(type));
            }

            return handlers.ToArray();
        }

        private static IBeforeFieldHandler[] CreateBeforeField()
        {
            IBeforeFieldHandler[] handlers = Create<IBeforeFieldHandler>();
            Array.Sort(handlers, comparison: (a, b) => a.Order.CompareTo(b.Order));
            return handlers;
        }

        private static IAfterFieldHandler[] CreateAfterField()
        {
            IAfterFieldHandler[] handlers = Create<IAfterFieldHandler>();
            Array.Sort(handlers, comparison: (a, b) => a.Order.CompareTo(b.Order));
            return handlers;
        }
    }
}