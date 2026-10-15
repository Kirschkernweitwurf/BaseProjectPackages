using System;
using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.AssetActions
{
    /// <summary>
    /// The box Replace Everywhere opens on the row: the asset to put in instead, and every list and field that
    /// holds the old one with what will happen to it, so nothing changes unseen.
    /// </summary>
    internal sealed class DataReplacePopup : PopupWindowContent
    {
        private const string ApplyFormat = "{0} in {1}";
        private const string CancelLabel = "Cancel";
        private const string CantHoldFormat = "can't hold {0}, stays as it is";
        private const string CollectionKeepsFormat = "already has {0}, so it is only taken out";
        private const string CountFormat = "{0}  ×{1}";
        private const float FieldLabelWidth = 40f;
        private const float Height = 300f;
        private const string LineFormat = "{0}  ·  {1}";
        private const float LineHeight = 18f;
        private const string ManyPlacesFormat = "{0} places";
        private const string NothingFoundText = "Nothing uses it.";
        private const string OnePlaceText = "1 place";
        private const float Padding = 10f;
        private const string RemovedText = "removed";
        private const string RemoveHint = "Leave it empty to take it out everywhere.";
        private const string RemoveLabel = "Remove";
        private const string ReplacedText = "replaced";
        private const string ReplaceLabel = "Replace";
        private const string SameAssetText = "Pick a different asset.";
        private const string ScenesNote = "Scenes and prefabs are not changed.";
        private const string TitleFormat = "Replace {0} everywhere";
        private const float Width = 380f;
        private const string WithLabel = "With";

        private readonly Object _asset;
        private readonly List<DataReplaceLine> _lines;
        private readonly Action _onApplied;
        private readonly Type _pickType;
        private readonly DataHubStyles _styles;

        private Object _replacement;
        private Vector2 _scroll;

        /// <summary>Creates the box for one asset.</summary>
        /// <param name="asset">The asset to replace.</param>
        /// <param name="pickType">The type the replacement is picked from, the type of its table.</param>
        /// <param name="lines">Every list and field holding the asset, from <see cref="DataReplace.Plan"/>.</param>
        /// <param name="styles">The built styles of the hub that opened it.</param>
        /// <param name="onApplied">Called after the replacement was applied.</param>
        internal DataReplacePopup(Object asset, Type pickType, List<DataReplaceLine> lines, DataHubStyles styles,
            Action onApplied)
        {
            _asset = asset;
            _pickType = pickType;
            _lines = lines;
            _styles = styles;
            _onApplied = onApplied;

            DataReplace.Prepare(_lines, null);
        }

        private int ApplicableCount
        {
            get
            {
                int count = 0;

                foreach (DataReplaceLine line in _lines)
                {
                    if (line.Fits(_replacement))
                        count += line.Count;
                }

                return count;
            }
        }

        private bool CanApply => ApplicableCount > 0
            && _replacement != _asset;

        /// <inheritdoc/>
        public override Vector2 GetWindowSize() => new(Width, Height);

        /// <inheritdoc/>
        public override void OnGUI(Rect rect)
        {
            // The asset can be deleted from outside while the box is open.
            if (_asset == null)
            {
                editorWindow.Close();
                return;
            }

            if (HandleKeys())
                return;

            GUILayout.BeginArea(new Rect(rect.x + Padding, rect.y + Padding, rect.width - Padding * 2f,
                rect.height - Padding * 2f));

            GUILayout.Label(string.Format(TitleFormat, _asset.name), _styles.SectionHeader);
            GUILayout.Space(EditorMetrics.TightGap);

            DrawPicker();
            GUILayout.Space(EditorMetrics.ItemGap);
            DrawLines();
            GUILayout.FlexibleSpace();
            GUILayout.Label(ScenesNote, _styles.Summary);
            DrawButtons();

            GUILayout.EndArea();
        }

        private static string Places(int count) => count == 1
            ? OnePlaceText
            : string.Format(ManyPlacesFormat, count);

        private void DrawPicker()
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal();
            GUILayout.Label(WithLabel, GUILayout.Width(FieldLabelWidth));
            _replacement = EditorGUILayout.ObjectField(_replacement, _pickType, false);
            GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
                DataReplace.Prepare(_lines, _replacement);

            string hint = _replacement == _asset
                ? SameAssetText
                : RemoveHint;

            GUILayout.Label(hint, _styles.Summary);
        }

        private void DrawLines()
        {
            if (_lines.Count == 0)
            {
                GUILayout.Label(NothingFoundText, _styles.Detail);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (DataReplaceLine line in _lines)
            {
                Rect row = GUILayoutUtility.GetRect(0f, LineHeight, GUILayout.ExpandWidth(true));
                Rect name = new(row.x, row.y, row.width * 0.6f, row.height);
                Rect outcome = new(name.xMax, row.y, row.width - name.width, row.height);

                string place = string.Format(LineFormat, line.Owner == null
                    ? string.Empty
                    : line.Owner.name, line.Label);

                GUI.Label(name, string.Format(CountFormat, place, line.Count), _styles.Name);
                GUI.Label(outcome, OutcomeOf(line), line.Fits(_replacement)
                    ? _styles.Name
                    : _styles.Detail);
            }

            EditorGUILayout.EndScrollView();
        }

        private string OutcomeOf(DataReplaceLine line)
        {
            if (!line.Fits(_replacement))
                return string.Format(CantHoldFormat, _replacement.name);

            if (_replacement == null)
                return RemovedText;

            return line.IsRemoval(_replacement)
                ? string.Format(CollectionKeepsFormat, _replacement.name)
                : ReplacedText;
        }

        private void DrawButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(CancelLabel, _styles.HeaderButton))
                editorWindow.Close();

            string verb = _replacement == null
                ? RemoveLabel
                : ReplaceLabel;

            using (new EditorGUI.DisabledScope(!CanApply))
            {
                if (GUILayout.Button(string.Format(ApplyFormat, verb, Places(ApplicableCount)),
                        _styles.HeaderPrimaryButton))
                    Apply();
            }

            GUILayout.EndHorizontal();
        }

        // Enter applies and Escape cancels, the same as the other boxes of the hub.
        private bool HandleKeys()
        {
            Event current = Event.current;

            if (current.type != EventType.KeyDown)
                return false;

            switch (current.keyCode)
            {
                case KeyCode.Return when CanApply:
                case KeyCode.KeypadEnter when CanApply:
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
            DataReplace.Apply(_lines, _asset, _replacement);
            _onApplied?.Invoke();
            editorWindow.Close();
        }
    }
}