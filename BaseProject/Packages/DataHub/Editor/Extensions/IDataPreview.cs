using System;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Extensions
{
    /// <summary>
    /// A card the project adds to the Preview tab of the Data Hub, like the finished card face of a card asset
    /// or how it did in the last playtest. It redraws while the designer edits the row, so a change shows the
    /// way the player will see it. Types without one get Unity's own asset preview.
    /// </summary>
    /// <remarks>
    /// A preview that holds something to release, like a preview scene, implements <see cref="IDisposable"/>. The
    /// hub disposes it when its window closes.
    /// </remarks>
    public interface IDataPreview
    {
        /// <summary>The type the preview shows, including derived types.</summary>
        Type TargetType { get; }

        /// <summary>The title of the card the preview is drawn in.</summary>
        string Title { get; }

        /// <summary>Position among the previews of the same asset. Lower comes first.</summary>
        int Order { get; }

        /// <summary>Draws the preview with GUILayout. The hub draws the title and the card around it.</summary>
        /// <param name="asset">The selected asset, always of <see cref="TargetType"/>.</param>
        void Draw(Object asset);
    }
}