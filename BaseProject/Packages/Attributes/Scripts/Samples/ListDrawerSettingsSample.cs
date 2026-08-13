using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A list with its drawer configured.</summary>
    [AttributeSample(typeof(ListDrawerSettingsAttribute), EAttributeCategory.Collections,
        Description = "Configures how a list is drawn: what its rows are named, whether it can be searched, and "
            + "whether removing a row asks first.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "LabelMember names a field on the element to use as the row label.",
            "Searchable adds a filter box, which switches dragging off while it is filtering.",
            "ConfirmDelete asks before removing a row, and names the row it is about to delete.",
            "ShowAlternatingBackground turns the row tinting off."
        })]
    internal sealed class ListDrawerSettingsSample : ScriptableObject
    {
        [Tooltip("A plain list with no attribute, drawn by Unity, for comparison. Its rows are numbered.")]
        public List<Row> plain = new()
        {
            new Row { id = "Ash", amount = 1 },
            new Row { id = "Birch", amount = 2 }
        };

        [ListDrawerSettings(LabelMember = nameof(Row.id))]
        [Tooltip("The same rows, named after their id instead of by their position.")]
        public List<Row> labeled = new()
        {
            new Row { id = "Ash", amount = 1 },
            new Row { id = "Birch", amount = 2 }
        };

        [ListDrawerSettings(Searchable = true, LabelMember = nameof(Row.id))]
        [Tooltip("A search box that hides the rows whose label does not match.")]
        public List<Row> searchable = new()
        {
            new Row { id = "Ash", amount = 1 },
            new Row { id = "Birch", amount = 2 },
            new Row { id = "Cedar", amount = 3 }
        };

        [ListDrawerSettings(ConfirmDelete = true, LabelMember = nameof(Row.id))]
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