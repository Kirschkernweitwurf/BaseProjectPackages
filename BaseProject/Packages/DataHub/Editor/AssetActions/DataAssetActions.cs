using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Base.DataHubPackage.Editor.Extensions;
using Base.ToolsPackage.Editor.NamingConventions;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.AssetActions
{
    /// <summary>
    /// New, Duplicate, Rename and Delete for the Data Hub. Every name runs through the naming
    /// conventions, every new asset joins its collection, and a delete takes the asset out of every
    /// list first, so none of these leaves work behind for someone to notice at runtime.
    /// </summary>
    internal static class DataAssetActions
    {
        private const string AssetExtension = ".asset";

        /// <summary>The folder every project asset lives under, and the folder of last resort.</summary>
        internal const string AssetsRoot = "Assets";

        private const string CancelLabel = "Cancel";
        private const string DeleteLabel = "Delete";
        private const string DeleteManyFormat = "Delete these {0} assets?";
        private const string DeleteOneText = "Delete this asset?";
        private const string DeleteTitle = "Delete Assets";
        private const string DeleteWarning = "The files go to the trash. Scenes and prefabs are not changed.";
        private const char FolderSeparator = '/';
        private const string ListIndent = "  ";
        private const int MaxListedNames = 10;
        private const string MoreFormat = "...and {0} more";
        private const string RemovedFromText = "They are also taken out of:";
        private const char WindowsSeparator = '\\';

        /// <summary>
        /// Creates a new asset of <paramref name="type"/> where its create rule says, or in
        /// <paramref name="fallbackFolder"/> when it has none.
        /// </summary>
        /// <param name="type">The concrete ScriptableObject type to create.</param>
        /// <param name="rule">The create rule of the type, may be null.</param>
        /// <param name="fallbackFolder">The folder used without a rule.</param>
        /// <param name="collections">The collection lookup the new asset is added through.</param>
        /// <returns>The new asset, or null when it could not be created.</returns>
        internal static Object Create(Type type, IDataCreateRule rule, string fallbackFolder,
            DataCollections collections)
        {
            if (!typeof(ScriptableObject).IsAssignableFrom(type)
                || type.IsAbstract)
            {
                CustomLogger.LogError($"The Data Hub can only create concrete {nameof(ScriptableObject)} types, "
                    + $"not {type.Name}.", null);

                return null;
            }

            string folder = string.IsNullOrEmpty(rule?.Folder)
                ? fallbackFolder
                : rule.Folder.TrimEnd(FolderSeparator);

            string startName = string.IsNullOrEmpty(rule?.DefaultName)
                ? type.Name
                : rule.DefaultName;

            if (!EnsureFolder(folder))
            {
                CustomLogger.LogError($"Could not create the folder '{folder}' for a new {type.Name}.", null);
                return null;
            }

            string path = UniquePath(folder, startName, type);
            ScriptableObject asset = ScriptableObject.CreateInstance(type);

            // The same reset the inspector runs, so defaults a type fills in Reset are there from the start.
            Unsupported.SmartReset(asset);

            AssetDatabase.CreateAsset(asset, path);
            collections.Add(rule, asset);

            return asset;
        }

        /// <summary>Copies <paramref name="source"/> next to itself and adds the copy to its collection.</summary>
        /// <param name="source">The asset to copy.</param>
        /// <param name="rule">The create rule of its type, may be null.</param>
        /// <param name="collections">The collection lookup the copy is added through.</param>
        /// <returns>The copy, or null when copying failed.</returns>
        internal static Object Duplicate(Object source, IDataCreateRule rule, DataCollections collections)
        {
            string sourcePath = AssetDatabase.GetAssetPath(source);
            string folder = FolderOf(sourcePath);
            string path = UniquePath(folder, source.name, source.GetType());

            if (!AssetDatabase.CopyAsset(sourcePath, path))
            {
                CustomLogger.LogError($"Could not duplicate {source.name}.", source);
                return null;
            }

            Object copy = AssetDatabase.LoadMainAssetAtPath(path);
            collections.Add(rule, copy);

            return copy;
        }

        /// <summary>
        /// The name <paramref name="asset"/> would end up with when renamed to <paramref name="requested"/>.
        /// </summary>
        /// <param name="asset">The asset to rename.</param>
        /// <param name="requested">The name the user typed.</param>
        /// <returns>The name after the naming conventions.</returns>
        internal static string PreviewName(Object asset, string requested)
        {
            if (string.IsNullOrWhiteSpace(requested))
                return asset.name;

            return AssetNamingConventions.SuggestName(AssetDatabase.GetAssetPath(asset), asset.GetType(),
                requested.Trim());
        }

        /// <summary>Renames <paramref name="asset"/>, keeping the naming conventions.</summary>
        /// <param name="asset">The asset to rename.</param>
        /// <param name="requested">The name the user typed.</param>
        internal static void Rename(Object asset, string requested)
        {
            string name = PreviewName(asset, requested);

            if (name == asset.name)
                return;

            string error = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(asset), name);

            if (!string.IsNullOrEmpty(error))
                CustomLogger.LogError(error, asset);
        }

        /// <summary>
        /// Asks, removes every reference to <paramref name="assets"/> from the project's data assets,
        /// then moves them to the trash. Asset deletion cannot be undone, the trash can.
        /// </summary>
        /// <param name="assets">The assets to delete.</param>
        /// <param name="index">The assets of the project.</param>
        /// <returns>True when the user confirmed.</returns>
        internal static bool Delete(IReadOnlyList<Object> assets, DataAssetIndex index)
        {
            if (assets.Count == 0)
                return false;

            List<DataReference> references = DataReferenceCleaner.Find(assets, index);

            if (!EditorUtility.DisplayDialog(DeleteTitle, DeleteMessage(assets, references), DeleteLabel,
                    CancelLabel))
                return false;

            DataReferenceCleaner.Remove(references);
            AssetDatabase.SaveAssets();

            foreach (Object asset in assets)
            {
                if (!AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(asset)))
                    CustomLogger.LogError($"Could not move {asset.name} to the trash.", asset);
            }

            return true;
        }

        /// <summary>The folder of an asset path, with forward slashes.</summary>
        /// <param name="assetPath">The project path of an asset.</param>
        /// <returns>The folder, or <see cref="AssetsRoot"/> for a path without one.</returns>
        internal static string FolderOf(string assetPath)
        {
            string folder = Path.GetDirectoryName(assetPath);

            return string.IsNullOrEmpty(folder)
                ? AssetsRoot
                : folder.Replace(WindowsSeparator, FolderSeparator);
        }

        private static string UniquePath(string folder, string startName, Type type)
        {
            string planned = folder + FolderSeparator + startName + AssetExtension;
            string name = AssetNamingConventions.SuggestNewName(planned, type);

            // The conventions already avoid a collision. This is the last guard for a rule set that
            // cannot find a free name, because creating onto an existing path would replace an asset.
            return AssetDatabase.GenerateUniqueAssetPath(folder + FolderSeparator + name + AssetExtension);
        }

        private static bool EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
                return false;

            if (AssetDatabase.IsValidFolder(folder))
                return true;

            int separator = folder.LastIndexOf(FolderSeparator);

            if (separator <= 0)
                return false;

            string parent = folder.Substring(0, separator);

            if (!EnsureFolder(parent))
                return false;

            return !string.IsNullOrEmpty(AssetDatabase.CreateFolder(parent, folder.Substring(separator + 1)));
        }

        private static string DeleteMessage(IReadOnlyList<Object> assets, List<DataReference> references)
        {
            StringBuilder builder = new();

            builder.AppendLine(assets.Count == 1
                ? DeleteOneText
                : string.Format(DeleteManyFormat, assets.Count));

            AppendNames(builder, assets);

            HashSet<Object> owners = new();

            foreach (DataReference reference in references)
                owners.Add(reference.Owner);

            if (owners.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine(RemovedFromText);
                AppendNames(builder, new List<Object>(owners));
            }

            builder.AppendLine();
            builder.Append(DeleteWarning);

            return builder.ToString();
        }

        private static void AppendNames(StringBuilder builder, IReadOnlyList<Object> objects)
        {
            int listed = Math.Min(objects.Count, MaxListedNames);

            for (int i = 0; i < listed; i++)
                builder.AppendLine(ListIndent + objects[i].name);

            if (objects.Count > listed)
                builder.AppendLine(ListIndent + string.Format(MoreFormat, objects.Count - listed));
        }
    }
}