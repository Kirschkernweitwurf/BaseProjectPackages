using System;
using System.Collections.Generic;
using System.Globalization;
using Base.DataHubPackage.Editor.Table;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.BulkEdit
{
    /// <summary>
    /// The small box a bulk edit opens at the cell or column title it came from: add, multiply or set, the
    /// number, and the first rows before and after, so the designer sees the result before applying it.
    /// </summary>
    internal sealed class DataBulkEditPopup : PopupWindowContent
    {
        private const string AddLabel = "Add";
        private const string ApplyFormat = "Apply to {0}";
        private const float ButtonWidth = 96f;
        private const string CancelLabel = "Cancel";
        private const string ClampedFormat = "Kept within the limit of {0}.";
        private const string FieldControlName = "DataHubBulkValue";
        private const string FloatFormat = "0.###";
        private const float LineHeight = 18f;
        private const string ManyRowsFormat = "{0} rows";
        private const int MaxPreviewLines = 6;
        private const string MaxText = "{0} or less";
        private const string MinMaxFormat = "{0} to {1}";
        private const string MinText = "{0} or more";
        private const string ModeAddTooltip = "Adds the number to every row. Use a negative number to subtract.";
        private const string ModeMultiplyTooltip = "Multiplies every row by the number. 1.5 means 50% more.";
        private const string ModeSetTooltip = "Sets every row to the number.";
        private const string MoreFormat = "and {0} more";
        private const float MultiplyDefault = 1.5f;
        private const string MultiplyLabel = "Multiply";
        private const string NoChangeText = "Nothing would change.";
        private const string OneRowText = "1 row";
        private const float Padding = 10f;
        private const string ResultFormat = "{0}  →  {1}";
        private const string RoundedText = "Rounded to whole numbers.";
        private const string SetLabel = "Set";
        private const string TitleFormat = "Adjust {0}";
        private const float ValueWidth = 90f;
        private const float Width = 340f;

        private static readonly string[] ValueLabels = { "Add", "Multiply by", "Set to" };

        private readonly List<double> _before = new();
        private readonly List<DataBulkChange> _changes = new();
        private readonly DataColumn _column;
        private readonly Action<int> _onApplied;
        private readonly DataHubStyles _styles;
        private readonly Object[] _targets;

        private bool _isClamped;
        private bool _isFocused;
        private bool _isRounded;
        private EDataBulkMode _mode;
        private double _value = 1d;

        private string RowsText => _targets.Length == 1
            ? OneRowText
            : string.Format(ManyRowsFormat, _targets.Length);

        /// <summary>Creates the box for one column and the rows it changes.</summary>
        /// <param name="column">The number column.</param>
        /// <param name="targets">The assets of the rows.</param>
        /// <param name="styles">The built styles of the hub that opened it.</param>
        /// <param name="onApplied">Called with the number of changed assets after applying.</param>
        internal DataBulkEditPopup(DataColumn column, Object[] targets, DataHubStyles styles, Action<int> onApplied)
        {
            _column = column;
            _targets = targets;
            _styles = styles;
            _onApplied = onApplied;

            DataBulkEdit.Read(targets, column, _before);
            UpdatePreview();
        }

        /// <inheritdoc/>
        public override Vector2 GetWindowSize()
        {
            int lines = Mathf.Min(_targets.Length, MaxPreviewLines) + 1;
            float height = Padding * 2f + LineHeight * 2f + DataHubStyles.ButtonHeight * 3f + LineHeight * lines
                + LineHeight + EditorMetrics.SectionGap * 2f;

            return new Vector2(Width, height);
        }

        /// <inheritdoc/>
        public override void OnGUI(Rect rect)
        {
            if (HandleKeys())
                return;

            GUILayout.BeginArea(new Rect(rect.x + Padding, rect.y + Padding, rect.width - Padding * 2f,
                rect.height - Padding * 2f));

            DrawHeader();
            DrawInputs();
            DrawPreview();
            GUILayout.FlexibleSpace();
            DrawButtons();

            GUILayout.EndArea();

            if (_isFocused)
                return;

            _isFocused = true;
            EditorGUI.FocusTextInControl(FieldControlName);
        }

        private static string Format(double value) => value.ToString(FloatFormat, CultureInfo.InvariantCulture);

        private static GUIContent[] ModeContents() => new[]
        {
            new GUIContent(AddLabel, ModeAddTooltip),
            new GUIContent(MultiplyLabel, ModeMultiplyTooltip),
            new GUIContent(SetLabel, ModeSetTooltip)
        };

        private void DrawHeader()
        {
            GUILayout.Label(string.Format(TitleFormat, _column.FullTitle), _styles.SectionHeader);
            GUILayout.Label(RowsText, _styles.Summary);
            GUILayout.Space(EditorMetrics.TightGap);
        }

        private void DrawInputs()
        {
            Rect modes = GUILayoutUtility.GetRect(0f, DataHubStyles.ButtonHeight, GUILayout.ExpandWidth(true));
            EDataBulkMode mode = (EDataBulkMode)DataHubGui.Segmented(modes, (int)_mode, ModeContents(), _styles);

            if (mode != _mode)
                ChangeMode(mode);

            GUILayout.Space(EditorMetrics.TightGap);

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal();
            GUILayout.Label(ValueLabels[(int)_mode], GUILayout.ExpandWidth(false));
            GUI.SetNextControlName(FieldControlName);
            _value = EditorGUILayout.DoubleField(_value, GUILayout.Width(ValueWidth));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
                UpdatePreview();

            GUILayout.Space(EditorMetrics.SectionGap);
        }

        // Set starts at the first row's value, so typing a small change to it is quick, and multiply at a
        // typical 1.5 instead of a 1 that would change nothing.
        private void ChangeMode(EDataBulkMode mode)
        {
            _mode = mode;

            _value = mode switch
            {
                EDataBulkMode.Set when _before.Count > 0 => _before[0],
                EDataBulkMode.Multiply => MultiplyDefault,
                _ => 1d
            };

            GUIUtility.keyboardControl = 0;
            _isFocused = false;

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            _changes.Clear();
            _isClamped = false;
            _isRounded = false;

            for (int i = 0; i < _targets.Length; i++)
            {
                double after = DataBulkEdit.Compute(_column, _mode, _before[i], _value, out bool isClamped,
                    out bool isRounded);

                _isClamped |= isClamped;
                _isRounded |= isRounded;
                _changes.Add(new DataBulkChange(_targets[i].name, _before[i], after));
            }
        }

        private void DrawPreview()
        {
            int shown = Mathf.Min(_changes.Count, MaxPreviewLines);

            for (int i = 0; i < shown; i++)
            {
                DataBulkChange change = _changes[i];
                Rect line = GUILayoutUtility.GetRect(0f, LineHeight, GUILayout.ExpandWidth(true));

                Rect name = new(line.x, line.y, line.width * 0.55f, line.height);
                Rect result = new(name.xMax, line.y, line.width - name.width, line.height);

                GUI.Label(name, change.Name, _styles.Name);
                GUI.Label(result, string.Format(ResultFormat, Format(change.Before), Format(change.After)),
                    change.IsChanged
                        ? _styles.Name
                        : _styles.Detail);
            }

            if (_changes.Count > shown)
                GUILayout.Label(string.Format(MoreFormat, _changes.Count - shown), _styles.Detail);

            GUILayout.Label(Notes(), _styles.Summary);
        }

        // Rounding and limits are said out loud, so a number that comes out different from what was typed
        // never surprises anyone.
        private string Notes()
        {
            if (!_changes.Exists(change => change.IsChanged))
                return NoChangeText;

            string notes = _isRounded
                ? RoundedText
                : string.Empty;

            if (!_isClamped)
                return notes;

            return (notes + " " + string.Format(ClampedFormat, LimitText())).Trim();
        }

        private string LimitText()
        {
            if (_column.MinValue.HasValue
                && _column.MaxValue.HasValue)
                return string.Format(MinMaxFormat, Format(_column.MinValue.Value), Format(_column.MaxValue.Value));

            return _column.MinValue.HasValue
                ? string.Format(MinText, Format(_column.MinValue.Value))
                : string.Format(MaxText, Format(_column.MaxValue ?? 0f));
        }

        private void DrawButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(CancelLabel, _styles.HeaderButton, GUILayout.Width(ButtonWidth)))
                editorWindow.Close();

            using (new EditorGUI.DisabledScope(!_changes.Exists(change => change.IsChanged)))
            {
                if (GUILayout.Button(string.Format(ApplyFormat, RowsText), _styles.HeaderPrimaryButton))
                    Apply();
            }

            GUILayout.EndHorizontal();
        }

        // Enter applies and Escape cancels, the same as the rename box.
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
            int changed = DataBulkEdit.Apply(_targets, _column, _mode, _value);
            _onApplied?.Invoke(changed);
            editorWindow.Close();
        }
    }
}