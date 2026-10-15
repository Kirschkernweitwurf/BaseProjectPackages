using System;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.AssetActions
{
    /// <summary>One list or field that holds the asset being replaced, with what replacing will do to it.</summary>
    internal sealed class DataReplaceLine
    {
        /// <summary>Creates the line.</summary>
        /// <param name="owner">The asset holding the list or field.</param>
        /// <param name="path">The serialized path of the list or field.</param>
        /// <param name="label">The readable name of the list or field.</param>
        /// <param name="slotType">The type a slot of it accepts.</param>
        /// <param name="isList">True for a list, false for a single field.</param>
        /// <param name="isCollection">True for a collection list, which holds every asset once.</param>
        internal DataReplaceLine(Object owner, string path, string label, Type slotType, bool isList,
            bool isCollection)
        {
            Owner = owner;
            Path = path;
            Label = label;
            SlotType = slotType;
            IsList = isList;
            IsCollection = isCollection;
        }

        /// <summary>The asset holding the list or field.</summary>
        internal Object Owner { get; }

        /// <summary>The serialized path of the list or field.</summary>
        internal string Path { get; }

        /// <summary>The readable name of the list or field.</summary>
        internal string Label { get; }

        /// <summary>The type a slot of it accepts, or null when it could not be worked out.</summary>
        internal Type SlotType { get; }

        /// <summary>True for a list, false for a single field.</summary>
        internal bool IsList { get; }

        /// <summary>True for a collection list, which holds every asset once.</summary>
        internal bool IsCollection { get; }

        /// <summary>How often the list holds the old asset, 1 for a field.</summary>
        internal int Count { get; set; }

        /// <summary>True when the list already holds the new asset.</summary>
        internal bool HasReplacement { get; set; }

        /// <summary>True when the new asset fits the slots, or when the old one is only removed.</summary>
        /// <param name="replacement">The new asset, or null to remove the old one.</param>
        /// <returns>True when the line can be changed.</returns>
        internal bool Fits(Object replacement) => replacement == null
            || SlotType == null
            || SlotType.IsInstanceOfType(replacement);

        /// <summary>
        /// True when the old asset is taken out instead of swapped: when nothing replaces it, or when a
        /// collection already holds the new one and a second copy would break it.
        /// </summary>
        /// <param name="replacement">The new asset, or null to remove the old one.</param>
        /// <returns>True when the old asset is only removed here.</returns>
        internal bool IsRemoval(Object replacement)
        {
            if (replacement == null)
                return true;

            return IsCollection
                && HasReplacement;
        }
    }
}