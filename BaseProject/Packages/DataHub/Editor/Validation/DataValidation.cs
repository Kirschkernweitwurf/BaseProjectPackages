using System;
using Base.AttributesPackage;
using Base.DataHubPackage.Editor.AssetActions;
using Base.DataHubPackage.Editor.Extensions;
using Base.DataHubPackage.Editor.Table;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Validation
{
    /// <summary>
    /// Fills the problem list of a row from three sources: the validation attributes on its fields,
    /// the project's <see cref="IDataValidator"/>s, and whether it sits in the collection its create
    /// rule names. The warning list says whether anything uses it, for the types the project asks about.
    /// </summary>
    internal sealed class DataValidation
    {
        private const string NotInCollectionFormat = "Not in {0}.";
        private const string UnusedText = "Not used anywhere yet.";
        private const string ValidatorFailedFormat = "{0} failed: {1}";

        private readonly DataCollections _collections;
        private readonly DataHubRegistry _registry;
        private readonly DataUsageIndex _usage;

        /// <summary>Creates the validation over the registry and the collections.</summary>
        /// <param name="registry">The project checks and create rules.</param>
        /// <param name="collections">The collection lookup.</param>
        /// <param name="usage">Which assets other data assets use.</param>
        internal DataValidation(DataHubRegistry registry, DataCollections collections, DataUsageIndex usage)
        {
            _registry = registry;
            _collections = collections;
            _usage = usage;
        }

        /// <summary>Checks the asset of a row again and replaces its problem list.</summary>
        /// <param name="row">The row to check.</param>
        internal void Validate(DataRow row)
        {
            row.Problems.Clear();
            row.Warnings.Clear();

            Object asset = row.Asset;

            // Deleted from outside between two rebuilds. The next rebuild drops the row.
            if (asset == null)
                return;

            ReferenceValidation.Collect(asset, row.Problems);

            foreach (IDataValidator validator in _registry.Validators)
            {
                if (validator.TargetType != null
                    && validator.TargetType.IsInstanceOfType(asset))
                    RunValidator(validator, row);
            }

            IDataCreateRule rule = _registry.FindCreateRule(asset.GetType());

            if (rule != null
                && rule.NeedsCollection(asset)
                && _collections.IsMissingFrom(rule, asset, out Object collection))
                row.Problems.Add(string.Format(NotInCollectionFormat, collection.name));

            if (_registry.IsUsageChecked(asset.GetType())
                && !_usage.IsUsed(asset))
                row.Warnings.Add(UnusedText);
        }

        // A validator is project code. One that throws shows up as a problem on the row instead of
        // breaking the table for every asset after it.
        private static void RunValidator(IDataValidator validator, DataRow row)
        {
            try
            {
                validator.Validate(row.Asset, row.Problems);
                validator.CollectWarnings(row.Asset, row.Warnings);
            }
            catch (Exception exception)
            {
                row.Problems.Add(string.Format(ValidatorFailedFormat, validator.GetType().Name, exception.Message));
            }
        }
    }
}