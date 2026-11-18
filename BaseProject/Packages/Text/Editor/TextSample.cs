using System;
using UnityEngine.Localization;
using Object = UnityEngine.Object;

namespace Base.TextPackage.Editor
{
    /// <summary>One text in one language, the box it is shown in and where the text can be edited.</summary>
    public sealed class TextSample
    {
        /// <summary>Creates the sample.</summary>
        /// <param name="area">The part of the game it belongs to, like "Cards" or "UI", for grouping.</param>
        /// <param name="box">The box the text is shown in.</param>
        /// <param name="source">What the text is, for the report, like a card or a table key.</param>
        /// <param name="arguments">The Smart String values it was rendered with or empty.</param>
        /// <param name="text">The text as the language shows it.</param>
        /// <param name="context">The asset the text comes from, like a card or null.</param>
        /// <param name="entry">The string table entry to edit the text in or null when it is built in code.</param>
        /// <param name="arrange">Puts the box's surroundings in the state the game shows this text in or null.</param>
        public TextSample(string area, TextBox box, string source, string arguments, string text, Object context,
            LocalizedString entry = null, Action arrange = null)
        {
            Area = area;
            Box = box;
            Source = source;
            Arguments = arguments;
            Text = text;
            Context = context;
            Entry = entry;
            Arrange = arrange;
        }

        /// <summary>The part of the game it belongs to, like "Cards" or "UI".</summary>
        public string Area { get; }

        /// <summary>The Smart String values it was rendered with or empty.</summary>
        public string Arguments { get; }

        /// <summary>Puts the box's surroundings in the state the game shows this text in or null.</summary>
        public Action Arrange { get; }

        /// <summary>The box the text is shown in.</summary>
        public TextBox Box { get; }

        /// <summary>The asset the text comes from, like a card or null.</summary>
        public Object Context { get; }

        /// <summary>The string table entry to edit the text in or null when it is built in code.</summary>
        public LocalizedString Entry { get; }

        /// <summary>What the text is, for the report.</summary>
        public string Source { get; }

        /// <summary>The text as the language shows it.</summary>
        public string Text { get; }
    }
}