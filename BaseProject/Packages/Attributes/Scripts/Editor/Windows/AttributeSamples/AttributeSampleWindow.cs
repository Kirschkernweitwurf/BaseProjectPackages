using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples
{
    /// <summary>
    /// Browses the attributes one at a time: a searchable list on the left, and on the right the
    /// attribute drawn live with the few lines that produced it underneath.
    /// </summary>
    /// <remarks>
    /// The source is the point. A gallery shows what an attribute looks like; putting the lines that
    /// produced it directly beneath is what turns looking into knowing what to type.
    /// <para>
    /// Each sample is created in memory and never saved, so editing one changes nothing.
    /// </para>
    /// </remarks>
    internal sealed class AttributeSampleWindow : EditorWindow
    {
        private const string CopiedAttributeNotice = "Attribute copied";
        private const string CopiedNotice = "Snippet copied";
        private const string CopyAttributeLabel = "Copy attribute";
        private const float CopyAttributeWidth = 110f;
        private const string CopyLabel = "Copy snippet";
        private const float DividerWidth = 1f;
        private const float HeadingHeight = 26f;
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Project Health/Attribute Samples";
        private const float MinimumWidth = 120f;
        private const string OpenLabel = "Open file";
        private const string PreviewHeading = "Live";
        private const string SourceHeading = "Source";
        private const string WindowTitle = "Attribute Samples";

        [SerializeField] private string search = string.Empty;
        [SerializeField] private Vector2 listScroll;
        [SerializeField] private Vector2 contentScroll;
        [SerializeField] private string selectedTitle;

        private AttributeSampleEntry _selected;
        private ScriptableObject _instance;
        private UnityEditor.Editor _editor;
        private MonoScript _script;
        private string _snippet;
        private AttributeSampleStyles _styles;

        /// <summary>The text the list filters by.</summary>
        internal string Search
        {
            get => search;
            set => search = value;
        }

        /// <summary>Scroll position of the list.</summary>
        internal Vector2 ListScroll
        {
            get => listScroll;
            set => listScroll = value;
        }

        /// <summary>Whether the given attribute is the one being shown.</summary>
        /// <param name="entry">The entry to test.</param>
        /// <returns>True when it is selected.</returns>
        internal bool IsSelected(in AttributeSampleEntry entry) => entry.Title == selectedTitle;

        /// <summary>Shows the given attribute, replacing whatever was shown before.</summary>
        /// <param name="entry">The entry to show.</param>
        internal void Select(in AttributeSampleEntry entry)
        {
            bool sameSample = _instance != null && _instance.GetType() == entry.SampleType;

            if (!sameSample)
                Release();

            selectedTitle = entry.Title;
            _selected = entry;
            contentScroll = Vector2.zero;

            if (!sameSample)
            {
                // Never saved and never in a scene, so nothing here can be committed by accident.
                _instance = CreateInstance(entry.SampleType);
                _instance.name = entry.Title;
                _instance.hideFlags = HideFlags.DontSave;

                _editor = UnityEditor.Editor.CreateEditor(_instance);
                _script = MonoScript.FromScriptableObject(_instance);
            }

            _snippet = _script == null
                ? string.Empty
                : AttributeSampleSource.Extract(_script.text, entry.MemberName);
        }

        [MenuItem(MenuPath)]
        private static void Open()
        {
            AttributeSampleWindow window = GetWindow<AttributeSampleWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.Show();
        }

        private void OnDisable() => Release();

        private void OnGUI()
        {
            _styles ??= new AttributeSampleStyles();

            HandleArrowKeys();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(AttributeSampleStyles.ListWidth));
            AttributeSampleList.Draw(this, _styles);
            EditorGUILayout.EndVertical();

            GUILayout.Space(AttributeSampleStyles.ColumnGap);

            // A hairline between the panes, so the list reads as a sidebar rather than as the first
            // column of the content.
            Rect divider = GUILayoutUtility.GetRect(DividerWidth, DividerWidth, GUILayout.ExpandHeight(true));

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(divider, _styles.Divider);

            GUILayout.Space(AttributeSampleStyles.ColumnGap);

            EditorGUILayout.BeginVertical();
            DrawContent();
            EditorGUILayout.EndVertical();

            GUILayout.Space(AttributeSampleStyles.ColumnGap);
            EditorGUILayout.EndHorizontal();
        }

        // Moves through the list as it is currently shown, so a filtered or collapsed list steps over
        // exactly the rows the reader can see.
        private void HandleArrowKeys()
        {
            Event current = Event.current;

            if (current.type != EventType.KeyDown)
                return;

            int step = current.keyCode switch
            {
                KeyCode.DownArrow => 1,
                KeyCode.UpArrow => -1,
                _ => 0
            };

            if (step == 0)
                return;

            List<AttributeSampleEntry> visible = AttributeSampleRegistry.Visible;
            if (visible.Count == 0)
                return;

            int index = visible.FindIndex(entry => entry.Title == selectedTitle);
            int next = Mathf.Clamp(index + step, 0, visible.Count - 1);

            if (index >= 0 && next == index)
                return;

            Select(visible[index < 0
                ? 0
                : next]);

            current.Use();
            Repaint();
        }

        private void Release()
        {
            if (_editor != null)
                DestroyImmediate(_editor);

            if (_instance != null)
                DestroyImmediate(_instance);

            _editor = null;
            _instance = null;
            _script = null;
            _snippet = null;
        }

        private void DrawContent()
        {
            if (_instance == null && !string.IsNullOrEmpty(selectedTitle))
                Restore();

            if (_editor == null)
            {
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField(AttributeSampleStyles.EmptyMessage,
                    EditorStyles.centeredGreyMiniLabel);

                GUILayout.FlexibleSpace();
                return;
            }

            contentScroll = EditorGUILayout.BeginScrollView(contentScroll);

            GUILayout.Space(AttributeSampleStyles.Padding);
            DrawHeading();
            DrawPreview();
            DrawSource();

            GUILayout.Space(AttributeSampleStyles.SectionGap);
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeading()
        {
            EditorGUILayout.LabelField(_selected.Category, _styles.Eyebrow);

            EditorGUILayout.LabelField($"[{_selected.Title}]", _styles.Heading,
                GUILayout.Height(HeadingHeight));

            if (!string.IsNullOrEmpty(_selected.Description))
            {
                EditorGUILayout.LabelField(_selected.Description, _styles.Description,
                    GUILayout.Height(_styles.Description.CalcHeight(
                        ScratchContent.For(_selected.Description), ContentWidth())));
            }

            GUILayout.Space(AttributeSampleStyles.SectionGap);
        }

        // The scroll view has no rect of its own during layout, so the width is taken from the window
        // less the list and the padding around it.
        private float ContentWidth() => Mathf.Max(position.width - AttributeSampleStyles.ListWidth
            - AttributeSampleStyles.ColumnGap * 3f - DividerWidth
            - AttributeSampleStyles.Padding * 2f, MinimumWidth);

        // The whole sample is drawn, not just the one field. A conditional field means nothing without
        // the toggle that drives it, and cutting the object down to one row would hide exactly that.
        private void DrawPreview()
        {
            EditorGUILayout.LabelField(PreviewHeading, _styles.Section);

            EditorGUILayout.BeginVertical(_styles.Card);
            _editor.OnInspectorGUI();
            EditorGUILayout.EndVertical();

            GUILayout.Space(AttributeSampleStyles.SectionGap);
        }

        private void DrawSource()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(SourceHeading, _styles.Section);
            GUILayout.FlexibleSpace();

            // The three actions sit together, because all three are things you do with what is on
            // screen and hunting for one of them beside the title was never obvious.
            if (GUILayout.Button(CopyAttributeLabel, EditorStyles.miniButtonLeft,
                    GUILayout.Width(CopyAttributeWidth)))
            {
                EditorGUIUtility.systemCopyBuffer = $"[{_selected.Title}]";
                ShowNotification(ScratchContent.For(CopiedAttributeNotice));
            }

            if (GUILayout.Button(CopyLabel, EditorStyles.miniButtonMid,
                    GUILayout.Width(AttributeSampleStyles.ButtonWidth)))
            {
                EditorGUIUtility.systemCopyBuffer = _snippet;
                ShowNotification(ScratchContent.For(CopiedNotice));
            }

            using (new EditorGUI.DisabledScope(_script == null))
            {
                if (GUILayout.Button(OpenLabel, EditorStyles.miniButtonRight,
                        GUILayout.Width(AttributeSampleStyles.ButtonWidth)))
                    AssetDatabase.OpenAsset(_script);
            }

            EditorGUILayout.EndHorizontal();

            // Selectable rather than editable: a text area is the only control that lets a reader select
            // a line, and the disabled scope stops it suggesting the file can be changed from here.
            //
            // Given an explicit rect rather than left to the layout. A text area that does not wrap is
            // as wide as its longest line, and the window then refuses to be made narrower than that.
            float width = ContentWidth();
            float height = _styles.Source.CalcHeight(ScratchContent.For(_snippet), width);

            Rect area = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false));

            using (new EditorGUI.DisabledScope(true))
                EditorGUI.TextArea(area, _snippet, _styles.Source);

            GUILayout.Space(AttributeSampleStyles.Padding);
        }

        // A domain reload destroys the in-memory sample but the window survives with its selection, so
        // the sample is rebuilt from the name rather than leaving the pane blank.
        private void Restore()
        {
            foreach (AttributeSampleEntry entry in AttributeSampleRegistry.All())
            {
                if (entry.Title != selectedTitle)
                    continue;

                Select(entry);
                return;
            }

            selectedTitle = null;
        }
    }
}