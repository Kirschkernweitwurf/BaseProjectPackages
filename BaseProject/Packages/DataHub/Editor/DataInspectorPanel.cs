using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// The right panel of the Data Hub: the normal inspector of the selected rows, so fields a table
    /// cell cannot edit, like the effect list with its reference picker, work exactly as they do in
    /// the Inspector window.
    /// </summary>
    internal sealed class DataInspectorPanel
    {
        private const string ManyTitleFormat = "{0} assets";
        private const string MixedTypesMessage = "The selected assets have different types. Select assets of one "
            + "type to edit them together.";
        private const string NothingSelectedMessage = "Select a row to see its inspector.";
        private const float WideModeWidth = 330f;

        private UnityEditor.Editor _editor;
        private Vector2 _scroll;

        /// <summary>Draws the inspector of <paramref name="targets"/>.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="targets">The selected assets. The same array as long as the selection is unchanged.</param>
        /// <param name="styles">The built window styles.</param>
        /// <returns>True when a value was changed through the inspector.</returns>
        internal bool Draw(Rect area, Object[] targets, DataHubStyles styles)
        {
            GUILayout.BeginArea(area);

            bool isChanged = DrawContent(targets, styles, area.width);

            GUILayout.EndArea();

            return isChanged;
        }

        /// <summary>Releases the inspector. Call when the window closes.</summary>
        internal void Dispose()
        {
            if (_editor != null)
                Object.DestroyImmediate(_editor);

            _editor = null;
        }

        private static bool HasMixedTypes(Object[] targets)
        {
            foreach (Object target in targets)
            {
                if (target.GetType() != targets[0].GetType())
                    return true;
            }

            return false;
        }

        private bool DrawContent(Object[] targets, DataHubStyles styles, float width)
        {
            // A deleted asset stays selected until the next rebuild, a moment later.
            if (Array.Exists(targets, target => target == null))
                return false;

            if (targets.Length == 0)
            {
                EditorGUILayout.HelpBox(NothingSelectedMessage, MessageType.Info);
                return false;
            }

            if (HasMixedTypes(targets))
            {
                EditorGUILayout.HelpBox(MixedTypesMessage, MessageType.Info);
                return false;
            }

            string title = targets.Length == 1
                ? targets[0].name
                : string.Format(ManyTitleFormat, targets.Length);

            GUILayout.Label(title, styles.SectionHeader);

            UnityEditor.Editor.CreateCachedEditor(targets, null, ref _editor);

            bool wasWide = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = width > WideModeWidth;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUI.BeginChangeCheck();

            _editor.OnInspectorGUI();

            bool isChanged = EditorGUI.EndChangeCheck();

            EditorGUILayout.EndScrollView();
            EditorGUIUtility.wideMode = wasWide;

            return isChanged;
        }
    }
}