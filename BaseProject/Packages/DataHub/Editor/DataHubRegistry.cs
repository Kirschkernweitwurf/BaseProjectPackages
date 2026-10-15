using System;
using System.Collections.Generic;
using Base.DataHubPackage.Editor.Extensions;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// Every table, column, check, create rule and page the project declares, found through
    /// <see cref="TypeCache"/> and created once per window. This is what makes a new feature a single
    /// new class: nothing has to be registered by hand.
    /// </summary>
    internal sealed class DataHubRegistry
    {
        private readonly List<IDataColumn> _columns = new();
        private readonly List<IDataCreateRule> _createRules = new();
        private readonly List<IDataHubPage> _pages = new();
        private readonly List<IDataTable> _tables = new();
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
            Collect(_pages);
            Collect(_tables);
            Collect(_validators);

            _columns.Sort((first, second) => first.Order.CompareTo(second.Order));
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

        private static bool IsTarget(Type targetType, Type candidate) => targetType != null
            && candidate != null
            && targetType.IsAssignableFrom(candidate);

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