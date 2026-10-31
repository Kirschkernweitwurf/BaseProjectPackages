using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Extensions
{
    /// <summary>
    /// A project-specific check the Data Hub runs on every asset of <see cref="TargetType"/>. A row
    /// with a problem gets a red badge, so a broken asset is seen before it reaches play mode.
    /// </summary>
    /// <remarks>
    /// The validation attributes like <c>[Required]</c> are checked by the hub already. A validator is
    /// for rules no attribute can express.
    /// </remarks>
    public interface IDataValidator
    {
        /// <summary>The type the check applies to, including derived types.</summary>
        Type TargetType { get; }

        /// <summary>Appends one short line per problem found on <paramref name="asset"/>.</summary>
        /// <param name="asset">The asset to check, always of <see cref="TargetType"/>.</param>
        /// <param name="problems">The list the problems are appended to.</param>
        void Validate(Object asset, List<string> problems);

        /// <summary>
        /// Appends one short line per thing worth a second look on <paramref name="asset"/>, like a missing
        /// translation. A warning gets an amber badge and never counts as broken. Adds nothing by default.
        /// </summary>
        /// <param name="asset">The asset to check, always of <see cref="TargetType"/>.</param>
        /// <param name="warnings">The list the warnings are appended to.</param>
        void CollectWarnings(Object asset, List<string> warnings)
        {
        }
    }
}