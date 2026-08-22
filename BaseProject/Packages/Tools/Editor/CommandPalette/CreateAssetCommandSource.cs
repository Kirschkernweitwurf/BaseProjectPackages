using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolPackage.Editor.MenuManagerWindows;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Collects every type marked with <see cref="CreateAssetMenuAttribute"/>. The command builds
    /// the asset through the project window directly instead of walking the menu, so it works no
    /// matter which window currently has focus.
    /// </summary>
    internal sealed class CreateAssetCommandSource : ICommandSource
    {
        /// <inheritdoc/>
        public void Collect(List<CommandEntry> entries)
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<CreateAssetMenuAttribute>())
            {
                if (type.IsAbstract || !typeof(ScriptableObject).IsAssignableFrom(type))
                    continue;

                CreateAssetMenuAttribute attribute = type.GetCustomAttribute<CreateAssetMenuAttribute>(false);

                if (attribute == null)
                    continue;

                entries.Add(Build(type, attribute));
            }
        }

        private static CommandEntry Build(Type type, CreateAssetMenuAttribute attribute)
        {
            // Unity falls back to the type name when no menu name is supplied.
            string relative = string.IsNullOrWhiteSpace(attribute.menuName)
                ? type.Name
                : attribute.menuName;

            string path = $"{MenuPath.AssetRoot}/{relative}";
            string fileName = attribute.fileName;

            return new CommandEntry(MenuEntryId.ForCreateAsset(type), path, type, ECommandKind.CreateAsset,
                AssemblyOriginLookup.Classify(type), () => MenuAssetCreator.Create(type, fileName));
        }
    }
}