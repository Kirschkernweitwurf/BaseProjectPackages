using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.AssetZoo.Generation
{
    /// <summary>
    /// The naming prefixes one generation run works with, both the ones the user typed and the ones
    /// the scan found on its own. A prefix is stripped from the asset name instead of becoming a group.
    /// </summary>
    internal sealed class PrefixSet
    {
        private readonly Dictionary<string, int> _orders;
        private readonly List<string> _detected;
        private readonly List<string> _suspects;

        /// <summary>
        /// Prefixes the scan recognized without being told about them.
        /// </summary>
        public IReadOnlyList<string> Detected => _detected;

        /// <summary>
        /// Tokens that look like a prefix but show up too rarely to be treated as one, so they became
        /// group names. Reported back so the user can add them to the known prefixes if that is wrong.
        /// </summary>
        public IReadOnlyList<string> Suspects => _suspects;

        /// <summary>
        /// Sort order for assets that carry no prefix at all. Sits behind every known prefix.
        /// </summary>
        public int NoPrefixOrder => _orders.Count;

        /// <summary>Creates a set from the prefixes resolved for one scan.</summary>
        /// <param name="orders">Prefix to sort order, in the order the prefixes should appear.</param>
        /// <param name="detected">The prefixes that were found automatically.</param>
        /// <param name="suspects">Tokens that were treated as group names but may be prefixes.</param>
        public PrefixSet(Dictionary<string, int> orders, List<string> detected, List<string> suspects)
        {
            _orders = orders;
            _detected = detected;
            _suspects = suspects;
        }

        /// <summary>Checks whether the token is a prefix and returns the sort order it stands for.</summary>
        /// <param name="token">The first part of an asset name.</param>
        /// <param name="order">Sort order of the prefix, zero when the token is not a prefix.</param>
        public bool TryGetOrder(string token, out int order) => _orders.TryGetValue(token, out order);
    }
}