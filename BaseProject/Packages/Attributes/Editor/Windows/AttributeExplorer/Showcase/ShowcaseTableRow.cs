using System;
using Base.AttributesPackage.Editor.Windows.AttributeExplorer.Troubleshoot.Samples;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Windows.AttributeExplorer.Showcase
{
    /// <summary>
    /// Row type for the showcase list and table sections. The fields are public so the label member can
    /// be named with nameof rather than a string literal, which is the whole point of that setting.
    /// </summary>
    [Serializable]
    internal sealed class ShowcaseTableRow
    {
        /// <summary>Row name, used as the list label and as the widest column.</summary>
        [TableColumn(2f)] public string id = "Row";

        /// <summary>Numeric column with a custom header.</summary>
        [TableColumn(Header = "Qty")] public int amount = 1;

        /// <summary>Enum column at the default weight.</summary>
        [TableColumn] public ESampleMode state = ESampleMode.Normal;

        /// <summary>Object column, wider than the default.</summary>
        [TableColumn(1.5f)] public GameObject prefab;

        /// <summary>Field kept out of the table but still edited by the list drawers.</summary>
        [TableColumn(Hidden = true)] public string internalNote = "Hidden column";
    }
}