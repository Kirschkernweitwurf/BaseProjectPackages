using System;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Extensions
{
    /// <summary>
    /// Where the Data Hub puts a new asset of <see cref="TargetType"/>, what it starts out called and
    /// which collection list it joins, so a new asset is in the right place from the first second.
    /// </summary>
    /// <remarks>
    /// The final name also runs through the project's naming conventions, so a prefix is defined once
    /// in the naming rules and never repeated here. The most specific rule for a type wins.
    /// </remarks>
    public interface IDataCreateRule
    {
        /// <summary>The type the rule applies to, including derived types.</summary>
        Type TargetType { get; }

        /// <summary>Project folder for new assets, for example "Assets/Data/Cards/Gift".</summary>
        string Folder { get; }

        /// <summary>The name a new asset starts from, before the naming conventions are applied.</summary>
        string DefaultName { get; }

        /// <summary>
        /// Type of the asset holding the list new assets are added to, or null when there is none.
        /// The project must contain exactly one asset of this type.
        /// </summary>
        Type CollectionType { get; }

        /// <summary>Serialized name of the list field on the collection, for example "cards".</summary>
        string CollectionListProperty { get; }

        /// <summary>
        /// True when <paramref name="asset"/> has to be in the collection, which the check column reports as a
        /// problem otherwise. Override it for assets that legitimately live elsewhere, like a card that belongs
        /// to one deck and must stay out of a random pool. True for every asset by default.
        /// </summary>
        /// <param name="asset">An asset of <see cref="TargetType"/>.</param>
        /// <returns>True to require the asset in the collection.</returns>
        bool NeedsCollection(Object asset) => true;
    }
}