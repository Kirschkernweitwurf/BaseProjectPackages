using Base.AttributePackage.Editor.Core;
using UnityEditor;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Showcase
{
    /// <summary>
    /// Draws the showcase asset through the real inspector pipeline, so the attributes are seen doing
    /// what they actually do rather than being described. Pure presentation.
    /// </summary>
    internal static class AttributeShowcaseView
    {
        private const string Explanation =
            "A throwaway asset drawn through the normal inspector, mirroring the attribute tester section "
            + "for section. Edit anything, nothing is saved. Two families are missing because this is an "
            + "asset rather than a component: the scene handles and the hierarchy auto-getters have no "
            + "GameObject to work on. The header controls are declared but not visible, since an embedded "
            + "inspector draws the body and not the title bar.";

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