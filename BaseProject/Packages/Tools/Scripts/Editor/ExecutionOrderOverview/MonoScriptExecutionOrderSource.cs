using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.ExecutionOrderOverview
{
    /// <summary>
    /// Default source that reads execution orders from all runtime MonoScripts, covering
    /// both project scripts and package scripts in a single fast call.
    /// </summary>
    public sealed class MonoScriptExecutionOrderSource : IExecutionOrderSource
    {
        private const string PackagePrefix = "Packages/";

        /// <inheritdoc />
        public IReadOnlyList<ExecutionOrderEntry> Collect()
        {
            List<ExecutionOrderEntry> entries = new();
            MonoScript[] scripts = MonoImporter.GetAllRuntimeMonoScripts();

            foreach (MonoScript script in scripts)
            {
                if (script == null)
                    continue;

                Type type = script.GetClass();
                if (type == null)
                    continue;

                int projectOrder = MonoImporter.GetExecutionOrder(script);
                DefaultExecutionOrder attribute = (DefaultExecutionOrder)Attribute
                    .GetCustomAttribute(type, typeof(DefaultExecutionOrder), false);
                bool hasAttribute = attribute != null;

                if (!hasAttribute && projectOrder == 0)
                    continue;

                int attributeOrder = hasAttribute ? attribute.order : 0;
                string assetPath = AssetDatabase.GetAssetPath(script);
                bool isPackage = assetPath.StartsWith(PackagePrefix, StringComparison.Ordinal);

                entries.Add(new ExecutionOrderEntry(script, type, assetPath, isPackage, hasAttribute,
                    attributeOrder, projectOrder));
            }

            return entries;
        }
    }
}