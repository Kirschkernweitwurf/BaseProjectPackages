using System;
using Base.DataHubPackage.Editor.Extensions;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// What the column builder asks about each field while it walks a type: whether the table hides it, and
    /// which extension, if any, shows it as text.
    /// </summary>
    internal readonly struct DataFieldRules
    {
        private readonly Func<Type, IDataFieldText> _fieldTextFor;
        private readonly Func<string, bool> _isHidden;

        /// <summary>Bundles the two lookups.</summary>
        /// <param name="fieldTextFor">The extension that shows a field type as text, or null for none.</param>
        /// <param name="isHidden">True for a field path the table leaves out.</param>
        internal DataFieldRules(Func<Type, IDataFieldText> fieldTextFor, Func<string, bool> isHidden)
        {
            _fieldTextFor = fieldTextFor;
            _isHidden = isHidden;
        }

        /// <summary>The extension that shows <paramref name="fieldType"/> as text, or null.</summary>
        /// <param name="fieldType">The declared type of a field.</param>
        /// <returns>The extension, or null.</returns>
        internal IDataFieldText FieldTextFor(Type fieldType) => _fieldTextFor(fieldType);

        /// <summary>True when the table leaves the field at <paramref name="path"/> out.</summary>
        /// <param name="path">The property path of the field.</param>
        /// <returns>True to skip the field.</returns>
        internal bool IsHidden(string path) => _isHidden(path);
    }
}