using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolPackage.Editor.MenuManagerWindows;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Scanning
{
    /// <summary>
    /// Reads the default file names of the asset creation entries. A type created as
    /// "ANRS_AssetNamingRuleSet" already declares its prefix there, so the convention detection
    /// does not have to guess it from the assets that happen to exist. The menu manager registry
    /// wins over the attribute in code, because that is where the file name is actually edited.
    /// </summary>
    public static class CreateAssetMenuScanner
    {
        private const string IdPrefix = "CA:";
        private const char PrefixSeparator = '_';

        /// <summary>Prefix per scriptable object type, taken from its creation entry.</summary>
        public static Dictionary<Type, string> CollectPrefixes()
        {
            Dictionary<string, Type> types = new();
            Dictionary<Type, string> prefixes = new();

            CollectDynamic(types, prefixes);
            CollectStatic(types, prefixes);
            ApplyRegistry(types, prefixes);

            return prefixes;
        }

        private static void CollectDynamic(Dictionary<string, Type> types, Dictionary<Type, string> prefixes)
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<DynamicCreateAssetMenuAttribute>())
            {
                DynamicCreateAssetMenuAttribute attribute =
                    type.GetCustomAttribute<DynamicCreateAssetMenuAttribute>();

                if (attribute == null)
                    continue;

                Add(types, prefixes, type, attribute.FileName);
            }
        }

        private static void CollectStatic(Dictionary<string, Type> types, Dictionary<Type, string> prefixes)
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<CreateAssetMenuAttribute>())
            {
                CreateAssetMenuAttribute attribute = type.GetCustomAttribute<CreateAssetMenuAttribute>();

                if (attribute == null)
                    continue;

                Add(types, prefixes, type, attribute.fileName);
            }
        }

        /// <summary>Overrides the prefixes with the file names stored in the menu manager.</summary>
        private static void ApplyRegistry(Dictionary<string, Type> types, Dictionary<Type, string> prefixes)
        {
            foreach (List<MenuNode> root in MenuComposite.RootsFor(EMenuEntryKind.CreateAsset))
                ApplyNodes(root, types, prefixes);
        }

        private static void ApplyNodes(List<MenuNode> nodes, Dictionary<string, Type> types,
            Dictionary<Type, string> prefixes)
        {
            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                {
                    ApplyNodes(group.Children, types, prefixes);
                    continue;
                }

                if (node is not MenuEntryNode entryNode
                    || entryNode.Entry == null)
                    continue;

                ApplyEntry(entryNode.Entry, types, prefixes);
            }
        }

        private static void ApplyEntry(MenuEntry entry, Dictionary<string, Type> types,
            Dictionary<Type, string> prefixes)
        {
            if (entry.Kind != EMenuEntryKind.CreateAsset)
                return;

            string prefix = PrefixOf(entry.CreateFileName);

            if (prefix.Length == 0)
                return;

            if (!types.TryGetValue(TypeNameOf(entry.Id), out Type type))
                return;

            prefixes[type] = prefix;
        }

        /// <summary>Type behind a create asset entry id, which is the full name after "CA:".</summary>
        private static string TypeNameOf(string entryId)
        {
            if (string.IsNullOrEmpty(entryId))
                return string.Empty;

            return entryId.StartsWith(IdPrefix, StringComparison.Ordinal)
                ? entryId[IdPrefix.Length..]
                : entryId;
        }

        private static void Add(Dictionary<string, Type> types, Dictionary<Type, string> prefixes, Type type,
            string fileName)
        {
            if (type.IsAbstract
                || type.FullName == null)
                return;

            types[type.FullName] = type;

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
