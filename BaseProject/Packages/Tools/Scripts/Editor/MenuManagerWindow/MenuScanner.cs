#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolPackage.MenuManagerWindow;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Scans the project for dynamic menu attributes and builds resolved entries.</summary>
    public static class MenuScanner
    {
        private const BindingFlags MethodFlags =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>Returns all resolved entries keyed by their stable id.</summary>
        public static Dictionary<string, ResolvedMenu> Scan()
        {
            Dictionary<string, ResolvedMenu> result = new();

            ScanMenuItems(result);
            ScanCreateAssets(result);

            return result;
        }

        private static void ScanMenuItems(Dictionary<string, ResolvedMenu> result)
        {
            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<DynamicMenuItemAttribute>())
            {
                if (!method.IsStatic || method.GetParameters().Length != 0)
                {
                    CustomLogger.LogWarning(
                        $"Menu Manager: {Describe(method)} must be a static method with no parameters.", null);

                    continue;
                }

                Type owner = method.DeclaringType;

                if (owner == null)
                    continue;

                DynamicMenuItemAttribute attribute = method.GetCustomAttribute<DynamicMenuItemAttribute>();
                string id = "MI:" + owner.FullName + "." + method.Name;
                string defaultPath = string.IsNullOrWhiteSpace(attribute.DefaultPath)
                    ? "Tools/" + method.Name
                    : attribute.DefaultPath;

                Action execute = CreateAction(method);
                Func<bool> validate = CreateValidate(owner, attribute.ValidateMethod);

                result[id] = ResolvedMenu.MenuItem(defaultPath, execute, validate);
            }
        }

        private static void ScanCreateAssets(Dictionary<string, ResolvedMenu> result)
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<DynamicCreateAssetMenuAttribute>())
            {
                if (type.IsAbstract || !typeof(ScriptableObject).IsAssignableFrom(type))
                {
                    CustomLogger.LogWarning(
                        $"Menu Manager: {type.Name} must be a concrete ScriptableObject to use DynamicCreateAssetMenu.",
                        null);

                    continue;
                }

                DynamicCreateAssetMenuAttribute attribute = type.GetCustomAttribute<DynamicCreateAssetMenuAttribute>();
                string relative = string.IsNullOrWhiteSpace(attribute.DefaultPath)
                    ? type.Name
                    : attribute.DefaultPath;

                string fileName = string.IsNullOrWhiteSpace(attribute.FileName)
                    ? type.Name
                    : attribute.FileName;

                string id = "CA:" + type.FullName;
                string defaultPath = "Assets/Create/" + relative;

                result[id] = ResolvedMenu.CreateAsset(defaultPath, type, fileName);
            }
        }

        private static Action CreateAction(MethodInfo method)
        {
            try
            {
                return (Action)Delegate.CreateDelegate(typeof(Action), method);
            }
            catch (ArgumentException)
            {
                return () => method.Invoke(null, null);
            }
        }

        private static Func<bool> CreateValidate(Type owner, string methodName)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                return null;

            MethodInfo method = owner.GetMethod(methodName, MethodFlags, null, Type.EmptyTypes, null);

            if (method == null || method.ReturnType != typeof(bool))
            {
                CustomLogger.LogWarning(
                    $"Menu Manager: validate method '{methodName}' on {owner.Name} must be a static bool method with no parameters.",
                    null);

                return null;
            }

            return (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), method);
        }

        private static string Describe(MethodInfo method) => (method.DeclaringType != null
                ? method.DeclaringType.Name + "."
                : string.Empty)
            + method.Name;
    }
}
#endif