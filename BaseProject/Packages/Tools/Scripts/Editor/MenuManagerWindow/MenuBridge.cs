#if UNITY_EDITOR
using System;
using System.Reflection;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Reflection bridge to Unity's internal dynamic menu API.</summary>
    public static class MenuBridge
    {
        private const BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>True when the internal API was found and menu registration can run.</summary>
        public static bool IsAvailable => AddMethod != null && RemoveMethod != null;

        private static readonly MethodInfo AddMethod;
        private static readonly MethodInfo RemoveMethod;

        static MenuBridge()
        {
            Type menu = typeof(Menu);

            AddMethod = menu.GetMethod("AddMenuItem",
                Flags,
                null,
                new[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(bool),
                    typeof(int),
                    typeof(Action),
                    typeof(Func<bool>)
                },
                null);

            RemoveMethod = menu.GetMethod("RemoveMenuItem",
                Flags,
                null,
                new[]
                {
                    typeof(string)
                },
                null);

            if (AddMethod == null || RemoveMethod == null)
                CustomLogger.LogWarning(
                    "Menu Manager: could not bind Unity's dynamic menu API. Registration is disabled.", null);
        }

        /// <summary>Registers a menu item at the given path and priority.</summary>
        public static void Add(string path, int priority, Action execute, Func<bool> validate)
        {
            if (AddMethod == null)
                return;

            AddMethod.Invoke(null, new object[]
            {
                path,
                string.Empty,
                false,
                priority,
                execute,
                validate
            });
        }

        /// <summary>Removes a previously registered menu item.</summary>
        public static void Remove(string path)
        {
            if (RemoveMethod == null)
                return;

            RemoveMethod.Invoke(null, new object[]
            {
                path
            });
        }
    }
}
#endif