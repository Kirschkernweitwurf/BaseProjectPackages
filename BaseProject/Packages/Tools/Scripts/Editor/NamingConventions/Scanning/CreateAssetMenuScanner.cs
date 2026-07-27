using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Scanning
{
    /// <summary>
    /// Reads the default file names of the asset creation entries. A type created as
    /// "ANRS_AssetNamingRuleSet" already declares its prefix there, so the convention detection
    /// does not have to guess it from the assets that happen to exist.
    /// </summary>
    public static class CreateAssetMenuScanner
    {
        private const char PrefixSeparator = '_';

        /// <summary>Prefix per scriptable object type, taken from its creation entry.</summary>
        public static Dictionary<Type, string> CollectPrefixes()
        {
            Dictionary<Type, string> prefixes = new();

            CollectDynamic(prefixes);
            CollectStatic(prefixes);

            return prefixes;
        }

        private static void CollectDynamic(Dictionary<Type, string> prefixes)
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<DynamicCreateAssetMenuAttribute>())
            {
                DynamicCreateAssetMenuAttribute attribute =
                    type.GetCustomAttribute<DynamicCreateAssetMenuAttribute>();

                if (attribute == null)
                    continue;

                Add(prefixes, type, attribute.FileName);
            }
        }

        private static void CollectStatic(Dictionary<Type, string> prefixes)
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<CreateAssetMenuAttribute>())
            {
                CreateAssetMenuAttribute attribute = type.GetCustomAttribute<CreateAssetMenuAttribute>();

                if (attribute == null)
                    continue;

                Add(prefixes, type, attribute.fileName);
            }
        }

        private static void Add(Dictionary<Type, string> prefixes, Type type, string fileName)
        {
            if (type.IsAbstract)
                return;

            string prefix = PrefixOf(fileName);

            if (prefix.Length == 0)
                return;

            prefixes[type] = prefix;
        }

        /// <summary>Leading token of a default file name, including its underscore.</summary>
        private static string PrefixOf(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            int separator = fileName.IndexOf(PrefixSeparator);

            // A file name without an underscore carries no prefix, and one that starts with the
            // separator is a naming accident rather than a convention.
            return separator <= 0
                ? string.Empty
                : fileName[..(separator + 1)];
        }
    }
}
