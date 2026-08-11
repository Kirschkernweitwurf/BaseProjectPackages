using UnityEditor;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Showcase
{
    /// <summary>
    /// Draws the showcase asset through the real inspector pipeline, so the attributes are seen doing
    /// what they actually do rather than being described. Pure presentation.
    /// </summary>
    public static class AttributeShowcaseView
    {
        private const string Explanation =
            "This is a throwaway asset drawn through the normal inspector. Edit anything you like, nothing "
            + "is saved. Header buttons are the one exception: they live in the component header, which an "
            + "embedded inspector does not draw.";

        /// <summary>Draws the explanation and the showcase inspector.</summary>
        /// <param name="showcase">The in-memory showcase asset.</param>
        public static void Draw(AttributeShowcase showcase)
        {
            EditorGUILayout.HelpBox(Explanation, MessageType.Info);

            UnityEditor.Editor editor = EmbeddedEditorCache.Get(showcase);
            if (editor == null)
                return;

            editor.OnInspectorGUI();
        }
    }
}