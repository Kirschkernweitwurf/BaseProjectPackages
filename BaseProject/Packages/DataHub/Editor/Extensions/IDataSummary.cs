using System;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Extensions
{
    /// <summary>
    /// A live summary of an asset the project adds to the top of the Contents tab, like the mood balance of a
    /// deck. It redraws while the designer changes the lists below it, so every plus or minus shows its
    /// effect right away.
    /// </summary>
    public interface IDataSummary
    {
        /// <summary>The type the summary describes, including derived types.</summary>
        Type TargetType { get; }

        /// <summary>The title of the card the summary is drawn in.</summary>
        string Title { get; }

        /// <summary>Position among the summaries of the same asset. Lower comes first.</summary>
        int Order { get; }

        /// <summary>Draws the summary with GUILayout. The hub draws the title and the card around it.</summary>
        /// <param name="asset">The selected asset, always of <see cref="TargetType"/>.</param>
        void Draw(Object asset);
    }
}