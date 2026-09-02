using System;
using UnityEngine;

namespace Base.ToolsPackage.Editor.TodoOverview.Model
{
    /// <summary>
    /// One keyword the scan looks for, together with the color it is drawn in. Serializable so the set
    /// of keywords can be edited per project instead of being baked into the tool.
    /// </summary>
    [Serializable]
    internal sealed class TodoTag
    {
        /// <summary>Serialized name of the color field, for a drawer that edits it by hand.</summary>
        internal const string ColorPropertyName = nameof(color);

        /// <summary>Serialized name of the enabled field, for a drawer that edits it by hand.</summary>
        internal const string EnabledPropertyName = nameof(enabled);

        /// <summary>Serialized name of the keyword field, for a drawer that edits it by hand.</summary>
        internal const string KeywordPropertyName = nameof(keyword);

        [SerializeField] private string keyword;
        [SerializeField] private Color color;
        [SerializeField] private bool enabled;

        /// <summary>The word that marks an item, for example TODO. Matched as a whole word.</summary>
        internal string Keyword => keyword;

        /// <summary>The color of this keyword's pill, its band and its section header.</summary>
        internal Color Color => color;

        /// <summary>Whether the scan looks for this keyword at all.</summary>
        internal bool Enabled => enabled;

        /// <summary>Creates a keyword definition.</summary>
        /// <param name="keyword">The word that marks an item.</param>
        /// <param name="color">The color the keyword is drawn in.</param>
        /// <param name="enabled">Whether the scan looks for it.</param>
        internal TodoTag(string keyword, Color color, bool enabled)
        {
            this.keyword = keyword;
            this.color = color;
            this.enabled = enabled;
        }
    }
}