using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A list with its drawer configured.</summary>
    [AttributeSample(typeof(ListDrawerSettingsAttribute), EAttributeCategory.Collections,
        Description = "Configures how a list is drawn: whether it can be searched, whether removing a row "
            + "asks first, and whether the rows are tinted. The list itself stays Unity's own, so it "
            + "reorders, selects and resizes exactly like a list without the attribute.",
        Requirements = "Nothing.",
        Info = "Rows are named after the element's first string field, with no setting to say so. Unity's "
            + "own list does the same, so naming the member was configuration to reach the default.",
        Variations = new[]
        {
            "Searchable adds a filter box, which switches dragging off while it is filtering.",
            "ConfirmDelete asks before removing a row, and names the row it is about to delete.",
            "ShowAlternatingBackground turns the row tinting off."
        })]
    internal sealed class ListDrawerSettingsSample : ScriptableObject
    {
        [Tooltip("A plain list with no attribute, drawn by Unity, for comparison.")]
        public List<Row> plain = new()
        {
            new Row { id = "Ash", amount = 1 },
            new Row { id = "Birch", amount = 2 }
        };

        [ListDrawerSettings(Searchable = true)]
        [Tooltip("A search box that hides the rows whose label does not match.")]
        public List<Row> searchable = new()
        {
            new Row { id = "Ash", amount = 1 },
            new Row { id = "Birch", amount = 2 },
            new Row { id = "Cedar", amount = 3 }
        };

        [ListDrawerSettings(ConfirmDelete = true)]
        [Tooltip("Removing a row asks first, naming the row it is about to delete.")]
        public List<Row> confirmedDelete = new()
        {
            new Row { id = "Delete me", amount = 3 }
        };

        /// <summary>One element, shaped so the settings above have something to name.</summary>
        [Serializable]
        public sealed class Row
        {
            /// <summary>Row name, used as the list label.</summary>
            public string id = "Row";

            /// <summary>A second value, so a row is worth a foldout.</summary>
            public int amount = 1;
        }
    }
}