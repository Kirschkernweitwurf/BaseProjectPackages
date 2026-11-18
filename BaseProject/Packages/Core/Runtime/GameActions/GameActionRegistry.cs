using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
#if UNITY_EDITOR
using UnityEditor;

#else
using System.Reflection;
#endif

namespace Base.CorePackage.GameActions
{
    /// <summary>
    /// Central, lazily built lookup of every concrete <see cref="GameAction"/> subclass in
    /// the project. In the editor it is fed by Unity's fast <c>TypeCache</c>; in a build it
    /// is collected once via reflection and cached for the rest of the session. The optional
    /// <see cref="GameActionAttribute"/> only overrides an action's id. Call
    /// <see cref="Invalidate"/> to force a rebuild.
    /// </summary>
    public static class GameActionRegistry
    {
        private static Dictionary<string, GameActionInfo> _byId; // reset-ignore
        private static Dictionary<Type, GameActionInfo> _byType; // reset-ignore
        private static List<GameActionInfo> _all; // reset-ignore

        /// <summary>
        /// All registered game actions. Built on first access.
        /// </summary>
        public static IReadOnlyList<GameActionInfo> All
        {
            get
            {
                EnsureBuilt();
                return _all;
            }
        }

        /// <summary>
        /// Looks up a game action by its stable id.
        /// </summary>
        /// <param name="id">The id from <see cref="GameActionAttribute"/> or the type name.</param>
        /// <param name="info">The matching entry when found.</param>
        /// <returns><c>true</c> when an entry with that id exists.</returns>
        public static bool TryGet(string id, out GameActionInfo info)
        {
            EnsureBuilt();

            if (string.IsNullOrEmpty(id))
            {
                info = default(GameActionInfo);
                return false;
            }

            return _byId.TryGetValue(id, out info);
        }

        /// <summary>
        /// Looks up a game action by its concrete type.
        /// </summary>
        /// <param name="type">The action type.</param>
        /// <param name="info">The matching entry when found.</param>
        /// <returns><c>true</c> when the type is registered.</returns>
        public static bool TryGet(Type type, out GameActionInfo info)
        {
            EnsureBuilt();

            if (type == null)
            {
                info = default(GameActionInfo);
                return false;
            }

            return _byType.TryGetValue(type, out info);
        }

        /// <summary>
        /// Drops the cached data so the next access rebuilds it. Called by the editor when
        /// assets change.
        /// </summary>
        public static void Invalidate()
        {
            _byId = null;
            _byType = null;
            _all = null;
        }

        private static void EnsureBuilt()
        {
            if (_all != null)
                return;

            Build();
        }

        private static void Build()
        {
            Dictionary<string, GameActionInfo> byId = new();
            Dictionary<Type, GameActionInfo> byType = new();
            List<GameActionInfo> all = new();

            foreach (Type type in CollectTypes())
            {
                if (type == null
                    || type.IsAbstract)
                    continue;

                if (!typeof(GameAction).IsAssignableFrom(type))
                    continue;

                GameActionAttribute attribute = (GameActionAttribute)Attribute
                    .GetCustomAttribute(type, typeof(GameActionAttribute), false);

                string id = attribute == null || string.IsNullOrEmpty(attribute.Id)
                    ? type.Name
                    : attribute.Id;

                if (byId.ContainsKey(id))
                {
                    CustomLogger.LogWarning($"Duplicate game action id '{id}' on"
                        + $" {type.FullName}. Ignoring the duplicate.", null);

                    continue;
                }

                GameActionInfo info = new(type, id);
                byId[id] = info;
                byType[type] = info;
                all.Add(info);
            }

            _byId = byId;
            _byType = byType;
            _all = all;
        }

#if UNITY_EDITOR
        private static IEnumerable<Type> CollectTypes() => TypeCache.GetTypesDerivedFrom<GameAction>();
#else
        private static IEnumerable<Type> CollectTypes()
        {
            List<Type> results = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }

                if (types == null)
                    continue;

                foreach (Type type in types)
                {
                    if (type == null
                        || type == typeof(GameAction))
                        continue;

                    if (typeof(GameAction).IsAssignableFrom(type))
                        results.Add(type);
                }
            }

            return results;
        }
#endif
    }
}