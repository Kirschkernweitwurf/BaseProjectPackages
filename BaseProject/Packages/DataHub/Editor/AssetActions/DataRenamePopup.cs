using System;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.AssetActions
{
    /// <summary>
    /// The small box Rename opens on the row's name: a name field and the name the asset will really get
    /// once the naming conventions are applied, so a prefix added automatically is never a surprise.
    /// </summary>
    internal sealed class DataRenamePopup : PopupWindowContent
    {
        private const string ApplyLabel = "Rename";
        private const float ButtonWidth = 76f;
        private const string CancelLabel = "Cancel";
        private const string FieldControlName = "DataHubRenameField";
        private const float Height = 112f;
        private const float Padding = 10f;
        private const string PreviewChangedFormat = "Saved as  {0}";
        private const string PreviewSameText = "Saved as typed";
        private const string PreviewTooltip = "The naming conventions can add a prefix or change the casing, so this "
            + "is the name the file really gets.";
        private const string Title = "Rename";
        private const float Width = 340f;

        private readonly Object _asset;
        private readonly Action<string> _onApply;
        private readonly DataHubStyles _styles;

        private bool _isFocused;
        private string _name;
        private string _preview;

        /// <summary>Creates the popup for one asset.</summary>
        /// <param name="asset">The asset to rename.</param>
        /// <param name="styles">The built window styles of the hub that opened it.</param>
        /// <param name="onApply">Called with the typed name when the user confirms.</param>
        internal DataRenamePopup(Object asset, DataHubStyles styles, Action<string> onApply)
        {
            _asset = asset;
            _styles = styles;
            _onApply = onApply;
            _name = asset.name;
            _preview = asset.name;
        }

        /// <inheritdoc/>
        public override Vector2 GetWindowSize() => new(Width, Height);

        /// <inheritdoc/>
        public override void OnGUI(Rect rect)
        {
            // The asset can be deleted from outside while the popup is open.
            if (_asset == null)
            {
                editorWindow.Close();
                return;
            }

            if (HandleKeys())
                return;

            GUILayout.BeginArea(new Rect(rect.x + Padding, rect.y + Padding, rect.width - Padding * 2f,
                rect.height - Padding * 2f));

            DrawContent();

            GUILayout.EndArea();

            if (_isFocused)
                return;

            _isFocused = true;
            EditorGUI.FocusTextInControl(FieldControlName);
        }

        private void DrawContent()
        {
            GUILayout.Label(Title, _styles.SectionHeader);
            GUILayout.Space(EditorMetrics.TightGap);

            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName(FieldControlName);
            _name = EditorGUILayout.TextField(_name);

            if (EditorGUI.EndChangeCheck())
                _preview = DataAssetActions.PreviewName(_asset, _name);

            // Only worth a mention when the conventions change what was typed.
            string preview = _preview == _name
                ? PreviewSameText
                : string.Format(PreviewChangedFormat, _preview);

            GUILayout.Label(new GUIContent(preview, PreviewTooltip), _styles.Summary);
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(CancelLabel, _styles.HeaderButton, GUILayout.Width(ButtonWidth)))
                editorWindow.Close();

            if (GUILayout.Button(ApplyLabel, _styles.HeaderPrimaryButton, GUILayout.Width(ButtonWidth)))
                Apply();

            GUILayout.EndHorizontal();
        }

        // Enter confirms and Escape cancels, the same as renaming in the Project window.
        private bool HandleKeys()
        {
            Event current = Event.current;

            if (current.type != EventType.KeyDown)
                return false;

            switch (current.keyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    current.Use();
                    Apply();

                    return true;

                case KeyCode.Escape:
                    current.Use();
                    editorWindow.Close();

                    return true;

                default:
                    return false;
            }
        }

        private void Apply()
        {
            _onApply?.Invoke(_name);
            editorWindow.Close();
        }
    }
}