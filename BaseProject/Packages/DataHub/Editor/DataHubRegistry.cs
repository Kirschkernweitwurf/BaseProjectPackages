using System;
using System.Collections.Generic;
using Base.DataHubPackage.Editor.Extensions;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// Every table, column, check, create rule, summary, preview and page the project declares, found through
    /// <see cref="TypeCache"/> and created once per window. This is what makes a new feature a single
    /// new class: nothing has to be registered by hand.
    /// </summary>
    internal sealed class DataHubRegistry
    {
        private readonly List<IDataColumn> _columns = new();
        private readonly List<IDataCreateRule> _createRules = new();
        private readonly List<IDataFieldText> _fieldTexts = new();
        private readonly List<IDataHubPage> _pages = new();
        private readonly List<IDataPreview> _previews = new();
        private readonly List<IDataSummary> _summaries = new();
        private readonly List<IDataTable> _tables = new();
        private readonly List<IDataUsageCheck> _usageChecks = new();
        private readonly List<IDataValidator> _validators = new();

        /// <summary>Every table entry of the tree.</summary>
        internal IReadOnlyList<IDataTable> Tables => _tables;

        /// <summary>Every custom page of the tree.</summary>
        internal IReadOnlyList<IDataHubPage> Pages => _pages;

        /// <summary>Every project check.</summary>
        internal IReadOnlyList<IDataValidator> Validators => _validators;

        /// <summary>Finds and creates every extension in the loaded assemblies.</summary>
        internal DataHubRegistry()
        {
            Collect(_columns);
            Collect(_createRules);
            Collect(_fieldTexts);
            Collect(_pages);
            Collect(_previews);
            Collect(_summaries);
            Collect(_tables);
            Collect(_usageChecks);
            Collect(_validators);

            _columns.Sort((first, second) => first.Order.CompareTo(second.Order));
            _summaries.Sort((first, second) => first.Order.CompareTo(second.Order));
            _previews.Sort((first, second) => first.Order.CompareTo(second.Order));
        }

        /// <summary>
        /// Disposes every extension that holds something to release, like the preview scene of a card preview.
        /// Call when the window closes.
        /// </summary>
        internal void Dispose()
        {
            DisposeAll(_previews);
            DisposeAll(_summaries);
            DisposeAll(_pages);
        }

        /// <summary>Appends the computed columns that can read assets of <paramref name="tableType"/>.</summary>
        /// <param name="tableType">The type the table lists.</param>
        /// <param name="columns">The list the matching columns are appended to, in their order.</param>
        internal void CollectColumns(Type tableType, List<IDataColumn> columns)
        {
            foreach (IDataColumn column in _columns)
            {
                if (IsTarget(column.TargetType, tableType))
                    columns.Add(column);
            }
        }

        /// <summary>The extension that shows fields of <paramref name="fieldType"/> as text, or null.</summary>
        /// <param name="fieldType">The declared type of a serialized field.</param>
        /// <returns>The first matching extension, or null.</returns>
        internal IDataFieldText FieldTextFor(Type fieldType)
        {
            foreach (IDataFieldText fieldText in _fieldTexts)
            {
                if (IsTarget(fieldText.FieldType, fieldType))
                    return fieldText;
            }

            return null;
        }

        /// <summary>Appends the summaries that describe assets of <paramref name="assetType"/>.</summary>
        /// <param name="assetType">The concrete type of the asset.</param>
        /// <param name="summaries">The list the matching summaries are appended to, in their order.</param>
        internal void CollectSummaries(Type assetType, List<IDataSummary> summaries)
        {
            foreach (IDataSummary summary in _summaries)
            {
                if (IsTarget(summary.TargetType, assetType))
                    summaries.Add(summary);
            }
        }

        /// <summary>Appends the preview cards that show assets of <paramref name="assetType"/>.</summary>
        /// <param name="assetType">The concrete type of the asset.</param>
        /// <param name="previews">The list the matching previews are appended to, in their order.</param>
        internal void CollectPreviews(Type assetType, List<IDataPreview> previews)
        {
            foreach (IDataPreview preview in _previews)
            {
                if (IsTarget(preview.TargetType, assetType))
                    previews.Add(preview);
            }
        }

        /// <summary>
        /// The create rule for <paramref name="assetType"/>, or null when there is none. A rule for a
        /// derived type beats one for its base type, so a project can set a default and override it.
        /// </summary>
        /// <param name="assetType">The concrete type of the asset.</param>
        /// <returns>The most specific rule, or null.</returns>
        internal IDataCreateRule FindCreateRule(Type assetType)
        {
            IDataCreateRule best = null;

            foreach (IDataCreateRule rule in _createRules)
            {
                if (!IsTarget(rule.TargetType, assetType))
                    continue;

                if (best == null
                    || best.TargetType.IsAssignableFrom(rule.TargetType))
                    best = rule;
            }

            return best;
        }

        /// <summary>
        /// The table that lists <paramref name="assetType"/> most closely, or null when none does. The Dialogue
        /// table beats All Cards for a dialogue card, so revealing an asset lands where its own columns are.
        /// </summary>
        /// <param name="assetType">The concrete type of the asset.</param>
        /// <returns>The most specific table, or null.</returns>
        internal IDataTable FindTable(Type assetType)
        {
            IDataTable best = null;

            foreach (IDataTable table in _tables)
            {
                if (!IsTarget(table.DataType, assetType))
                    continue;

                if (best == null
                    || best.DataType.IsAssignableFrom(table.DataType))
                    best = table;
            }

            return best;
        }

        /// <summary>
        /// The create rule whose collection is the list <paramref name="listPath"/> on assets of
        /// <paramref name="ownerType"/>, or null when that list is no collection.
        /// </summary>
        /// <param name="ownerType">The type of the asset holding the list.</param>
        /// <param name="listPath">The serialized path of the list.</param>
        /// <returns>The rule, or null.</returns>
        internal IDataCreateRule FindCollectionRule(Type ownerType, string listPath)
        {
            foreach (IDataCreateRule rule in _createRules)
            {
                if (IsTarget(rule.CollectionType, ownerType)
                    && rule.CollectionListProperty == listPath)
                    return rule;
            }

            return null;
        }

        /// <summary>True when the project wants a warning on unused assets of <paramref name="assetType"/>.</summary>
        /// <param name="assetType">The concrete type of the asset.</param>
        /// <returns>True when its usage is checked.</returns>
        internal bool IsUsageChecked(Type assetType)
        {
            foreach (IDataUsageCheck check in _usageChecks)
            {
                if (IsTarget(check.TargetType, assetType))
                    return true;
            }

            return false;
        }

        /// <summary>True when assets of <paramref name="assetType"/> are the collection of a create rule.</summary>
        /// <param name="assetType">The type of an asset.</param>
        /// <returns>True for a collection type.</returns>
        internal bool IsCollectionType(Type assetType)
        {
            foreach (IDataCreateRule rule in _createRules)
            {
                if (IsTarget(rule.CollectionType, assetType))
                    return true;
            }

            return false;
        }

        private static bool IsTarget(Type targetType, Type candidate) => targetType != null
            && candidate != null
            && targetType.IsAssignableFrom(candidate);

        // Project code, so one that throws must not keep the others from releasing theirs.
        private static void DisposeAll<T>(List<T> extensions)
        {
            foreach (T extension in extensions)
            {
                if (!(extension is IDisposable disposable))
                    continue;

                try
                {
                    disposable.Dispose();
                }
                catch (Exception exception)
                {
                    CustomLogger.LogError($"Could not release {extension.GetType().Name}: {exception.Message}", null);
                }
            }
        }

        private static void Collect<T>(List<T> target) where T : class
        {
            foreach (Type type in TypeCache.GetTypesDerivedFrom<T>())
            {
                if (type.IsAbstract
                    || type.IsInterface
                    || type.ContainsGenericParameters)
                    continue;

                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    CustomLogger.LogError($"{type.Name} needs a parameterless constructor to show up in the "
                        + "Data Hub.", null);

                    continue;
                }

                // One broken extension must not take the whole window down with it.
                try
                {
                    target.Add((T)Activator.CreateInstance(type));
                }
                catch (Exception exception)
                {
                    CustomLogger.LogError($"Could not create {type.Name} for the Data Hub: "
                        + (exception.InnerException?.Message ?? exception.Message), null);
                }
            }
        }
    }
}