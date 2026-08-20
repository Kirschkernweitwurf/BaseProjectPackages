using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// One reusable <see cref="GUIContent"/> for transient measuring, so drawers do not allocate a new
    /// instance every repaint. The returned content is only valid until the next call and must never
    /// be stored.
    /// </summary>
    internal static class ScratchContent
    {
        private static readonly GUIContent Content = new();

        /// <summary>Returns the shared content filled with the given text.</summary>
        public static GUIContent For(string text)
        {
            Content.text = text;
            return Content;
        }
    }
}