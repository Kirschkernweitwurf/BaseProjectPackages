using System.Collections.Generic;
using UnityEngine.Localization;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// One part of the game whose texts the text fit check renders, like the cards or the menus. Every class that
    /// implements this and has a parameterless constructor is found and used on its own, in any editor assembly.
    /// </summary>
    public interface ITextSampleSource
    {
        /// <summary>The name of the part, like "Cards", for grouping in the report.</summary>
        string Area { get; }

        /// <summary>Loads the boxes of this part into <paramref name="stage"/>, once per check.</summary>
        void Load(TextFitStage stage);

        /// <summary>Every text this part shows in <paramref name="locale"/>, each with its box.</summary>
        IEnumerable<TextSample> Collect(Locale locale);
    }
}