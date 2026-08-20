using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.Shared;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.ExecutionOrderOverview
{
    /// <summary>
    /// Default source that reads execution orders from all runtime MonoScripts, covering
    /// project scripts, package scripts and Unity's built-in scripts in a single call.
    /// </summary>
    internal sealed class MonoScriptExecutionOrderSource : IExecutionOrderSource
    {
        /// <inheritdoc/>
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

                int attributeOrder = hasAttribute
                    ? attribute.order
                    : 0;

                string assetPath = AssetDatabase.GetAssetPath(script);
                EAssetOrigin origin = AssetOriginResolver.Classify(assetPath);

                entries.Add(new ExecutionOrderEntry(script, type, assetPath, origin, attributeOrder, projectOrder));
            }

            return entries;
        }
    }
}