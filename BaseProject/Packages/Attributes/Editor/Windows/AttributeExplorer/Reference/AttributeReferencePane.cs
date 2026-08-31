using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Reference
{
    /// <summary>
    /// The reference tab: a searchable list of attributes on the left, and on the right either one
    /// attribute drawn live with the source behind it, or the page for a whole category.
    /// </summary>
    /// <remarks>
    /// The source is the point. A gallery shows what an attribute looks like; putting the lines that
    /// produced it directly beneath is what turns looking into knowing what to type.
    /// <para>
    /// Serializable and owned by the window, so the selection and the scroll positions survive the domain
    /// reload that recompiling a sample triggers. The sample object itself does not survive it and is
    /// rebuilt from the selected name.
    /// </para>
    /// </remarks>
    [Serializable]
    internal sealed class AttributeReferencePane
    {
        private const float ActionGap = 6f;
        private const float ActionHeight = 22f;
        private const string BulletGlyph = "\u2022";
        private const float BulletWidth = 14f;
        private const float CardGap = 4f;
        private const float CardPadding = 10f;
        private const float CardSpacing = 6f;
        private const string CopiedAttributeNotice = "Attribute copied";
        private const string CopiedNotice = "Snippet copied";
        private const string CopyAttributeLabel = "Copy attribute";
        private const float CopyAttributeWidth = 118f;
        private const string CopyLabel = "Copy snippet";
        private const float CopySnippetWidth = 108f;
        private const string CountFormat = "{0} attributes in this category";
        private const string CreateSceneLabel = "Create in scene";
        private const string CreateSceneTooltip = "Creates a temporary object carrying this sample and "
            + "selects it, so the Scene view and the component header can show what an embedded "
            + "inspector cannot. It is never saved with the scene.";
        private const float CreateSceneWidth = 118f;
        private const float DividerWidth = 1f;
        private const float FocusOutline = 1f;
        private const float HeadingHeight = 26f;
        private const string InfoHeading = "Good to know";
        private const float MinimumWidth = 120f;
        private const float NotificationFade = 0.8f;
        private const float OpenFileWidth = 82f;
        private const string OpenLabel = "Open file";
        private const string PreviewHeading = "Live";
        private const string RequirementsHeading = "Requirements";
        private const float ScrollStep = 40f;
        private const string SourceHeading = "Source";
        private const string VariationsHeading = "Variations";

        private static readonly GUIContent CopiedAttributeContent = new(CopiedAttributeNotice);
        private static readonly GUIContent CopiedContent = new(CopiedNotice);
        private static readonly GUIContent CreateSceneContent = new(CreateSceneLabel, CreateSceneTooltip);

        [SerializeField] private string search = string.Empty;
        [SerializeField] private Vector2 listScroll;
        [SerializeField] private Vector2 contentScroll;
        [SerializeField] private string selectedTitle;
        [SerializeField] private string selectedCategory;
        [SerializeField] private bool contentFocused;
        [SerializeField] private int focusedCard = -1;

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

        private readonly AttributeReferenceList _list = new();

        private AttributeSampleEntry _selected;
        private Object _instance;
        private GameObject _host;
        private UnityEditor.Editor _editor;
        private MonoScript _script;
        private string _snippet = string.Empty;
        private EditorWindow _owner;
        private float _contentWidth = MinimumWidth;

        /// <summary>Whether the given attribute is the one being shown.</summary>
        /// <param name="entry">The entry to test.</param>
        /// <returns>True when it is selected.</returns>
        internal bool IsSelected(in AttributeSampleEntry entry) => entry.Title == selectedTitle;

        /// <summary>Whether the page of the named category is the one being shown.</summary>
        /// <param name="category">The category to test.</param>
        /// <returns>True while that category is selected.</returns>
        internal bool IsCategorySelected(string category) => selectedTitle == null && selectedCategory == category;

        /// <summary>Shows the given attribute, replacing whatever was shown before.</summary>
        /// <param name="entry">The entry to show.</param>
        internal void Select(in AttributeSampleEntry entry)
        {
            bool sameSample = _instance != null && _instance.GetType() == entry.SampleType;

            if (!sameSample)
                Release();

            selectedTitle = entry.Title;
            selectedCategory = entry.CategoryName;
            _selected = entry;
            contentScroll = Vector2.zero;
            contentFocused = false;
            focusedCard = -1;

            Unfocus();

            if (!sameSample)
            {
                // Never saved, so nothing here can be committed by accident.
                _instance = AttributeSampleHost.CreatePreview(entry.SampleType, entry.Title, out _host);

                _editor = UnityEditor.Editor.CreateEditor(_instance);
                _script = Script(_instance);
            }

            // A page is opened to be read, not continued, so the sample presents as authored rather
            // than as the last reader happened to leave it.
            SamplePreviewDefaults.Reapply(entry.SampleType);

            _snippet = _script == null
                ? string.Empty
                : AttributeSampleSource.Extract(_script.text, entry.SampleType.Name);
        }

        /// <summary>Shows the page for a whole category instead of a single attribute.</summary>
        /// <param name="category">The name of the category to show.</param>
        internal void SelectCategory(string category)
        {
            selectedTitle = null;
            selectedCategory = category;
            contentScroll = Vector2.zero;
            contentFocused = false;
            focusedCard = -1;

            Unfocus();
            Release();
        }

        /// <summary>Draws the whole tab into the given area.</summary>
        /// <param name="area">The area the tab owns, in window coordinates.</param>
        /// <param name="styles">The window styles.</param>
        /// <param name="owner">The window the pane belongs to, for repaints and notifications.</param>
        internal void Draw(Rect area, AttributeExplorerStyles styles, EditorWindow owner)
        {
            _owner = owner;

            HandleArrowKeys();

            // Laid out as three rectangles rather than as nested layout groups. A vertical group given a
            // fixed width inside a horizontal one reserves its width from the parent before the parent
            // knows how wide it is, which is where the dead strip on the left came from.
            float listWidth = Mathf.Min(AttributeExplorerStyles.ListWidth, area.width * 0.5f);
            float contentX = listWidth + DividerWidth + AttributeExplorerStyles.ColumnGap;

            Rect list = new(area.x, area.y, listWidth, area.height);
            Rect divider = new(area.x + listWidth, area.y, DividerWidth, area.height);
            Rect content = new(area.x + contentX, area.y, Mathf.Max(area.width - contentX, MinimumWidth),
                area.height);

            _contentWidth = Mathf.Max(content.width
                - AttributeExplorerStyles.Padding * 2f
                - AttributeExplorerStyles.ScrollBarWidth, MinimumWidth);

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(divider, styles.Divider);

            GUILayout.BeginArea(list);
            _list.Draw(this, styles);
            GUILayout.EndArea();

            if (contentFocused && Event.current.type == EventType.Repaint)
                DrawOutline(content, styles.Selection);

            GUILayout.BeginArea(content);
            DrawContent(styles);
            GUILayout.EndArea();
        }

        /// <summary>Destroys the in-memory sample. Call when the owning window closes.</summary>
        internal void Release()
        {
            if (_editor != null)
                Object.DestroyImmediate(_editor);

            AttributeSampleHost.DestroyPreview(_host);

            // A component sample is destroyed together with the object it lives on, so only an asset
            // sample is left to clean up on its own.
            if (_host == null && _instance != null)
                Object.DestroyImmediate(_instance);

            _editor = null;
            _instance = null;
            _host = null;
            _script = null;
            _snippet = string.Empty;
        }

        // A field left mid-edit otherwise keeps the keyboard across the page change, so the next arrow
        // key edits a value on a page nobody is looking at any more.
        private static void Unfocus()
        {
            GUIUtility.keyboardControl = 0;
            EditorGUIUtility.editingTextField = false;
            GUI.FocusControl(null);
        }

        private static void DrawOutline(Rect rect, Color color)
        {
            Rect inset = new(rect.x, rect.y, rect.width - FocusOutline, rect.height - FocusOutline);

            DrawBorder(inset, color);
        }

        private static void DrawBorder(Rect rect, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1f, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), color);
        }

        private static MonoScript Script(Object instance) => instance switch
        {
            MonoBehaviour behaviour => MonoScript.FromMonoBehaviour(behaviour),
            ScriptableObject asset => MonoScript.FromScriptableObject(asset),
            _ => null
        };

        /// <summary>Repaints the window this pane belongs to.</summary>
        private void Repaint()
        {
            if (_owner != null)
                _owner.Repaint();
        }

        // Moves through the list as it is currently shown, so a filtered or collapsed list steps over
        // exactly the rows the reader can see.
        private void HandleArrowKeys()
        {
            Event current = Event.current;

            if (current.type != EventType.KeyDown)
                return;

            if (current.keyCode == KeyCode.F && (current.control || current.command))
            {
                _list.FocusSearch();

                current.Use();
                Repaint();

                return;
            }

            if (EditorGUIUtility.editingTextField && current.keyCode != KeyCode.DownArrow)
                return;

            int step = current.keyCode switch
            {
                KeyCode.DownArrow => 1,
                KeyCode.UpArrow => -1,
                _ => 0
            };

            // Once the keyboard is on the right pane the arrows scroll it, which is the only thing that
            // pane can usefully do with them, and left brings the keyboard back to the list.
            if (contentFocused)
            {
                if (!HandleContentKeys(current, step))
                    return;

                current.Use();
                Repaint();

                return;
            }

            if (current.keyCode == KeyCode.RightArrow && MoveRight())
            {
                current.Use();
                Repaint();

                return;
            }

            // Left closes the header the keyboard is on, which is how every other tree in the editor
            // behaves and saves reaching for the mouse to fold a category away.
            if (current.keyCode == KeyCode.LeftArrow && selectedTitle == null && selectedCategory != null)
            {
                AttributeReferenceList.SetExpanded(selectedCategory, false);

                current.Use();
                Repaint();

                return;
            }

            if (step == 0)
                return;

            IReadOnlyList<AttributeSampleRow> visible = _list.Rows;

            if (visible.Count == 0)
                return;

            int index = CurrentRow(visible);

            // Up from the top row lands in the search box, which is the only thing above the list and
            // the thing most likely to be wanted after walking to the top of it.
            if (index == 0 && step < 0)
            {
                _list.FocusSearch();

                current.Use();
                Repaint();

                return;
            }

            if (index < 0)
            {
                // Nothing on screen is selected, which is where a fresh search leaves things. Landing on
                // a header there would show a category page rather than the match that was typed for, so
                // the first attribute is taken instead.
                if (!MoveToFirstEntry(visible))
                    return;
            }
            else
            {
                int next = Mathf.Clamp(index + step, 0, visible.Count - 1);

                if (next == index)
                    return;

                Move(visible[next]);
            }

            current.Use();
            Repaint();
        }

        // Right opens a closed header, and on anything already open it moves into the pane beside the
        // list. A category page has cards to land on, so the keyboard lands on the first of them; an
        // attribute page is an embedded inspector, whose controls have no handle the keyboard can be
        // put on from out here, so there the pane itself takes focus and the arrows scroll it.
        private bool MoveRight()
        {
            bool onHeader = selectedTitle == null && selectedCategory != null;

            if (onHeader && !AttributeReferenceList.IsExpanded(selectedCategory))
            {
                AttributeReferenceList.SetExpanded(selectedCategory, true);
                return true;
            }

            if (selectedTitle == null && selectedCategory == null)
                return false;

            contentFocused = true;
            focusedCard = onHeader
                ? 0
                : -1;

            Unfocus();

            return true;
        }

        // Returns true when the key was consumed.
        private bool HandleContentKeys(Event current, int step)
        {
            if (current.keyCode == KeyCode.LeftArrow)
            {
                contentFocused = false;
                focusedCard = -1;

                return true;
            }

            bool onCards = selectedTitle == null && selectedCategory != null;

            if (!onCards)
            {
                if (step == 0)
                    return false;

                contentScroll.y = Mathf.Max(contentScroll.y + step * ScrollStep, 0f);

                return true;
            }

            int count = CategoryCount();

            if (count == 0)
                return false;

            if (current.keyCode == KeyCode.Return
                || current.keyCode == KeyCode.KeypadEnter
                || current.keyCode == KeyCode.RightArrow)
            {
                OpenFocusedCard();
                return true;
            }

            if (step == 0)
                return false;

            focusedCard = Mathf.Clamp(focusedCard + step, 0, count - 1);

            return true;
        }

        private int CategoryCount()
        {
            int count = 0;

            foreach (AttributeSampleEntry entry in AttributeSampleRegistry.All())
            {
                if (entry.CategoryName == selectedCategory)
                    count++;
            }

            return count;
        }

        private void OpenFocusedCard()
        {
            int index = 0;

            foreach (AttributeSampleEntry entry in AttributeSampleRegistry.All())
            {
                if (entry.CategoryName != selectedCategory)
                    continue;

                if (index == focusedCard)
                {
                    Select(entry);
                    return;
                }

                index++;
            }
        }

        private bool MoveToFirstEntry(IReadOnlyList<AttributeSampleRow> visible)
        {
            foreach (AttributeSampleRow row in visible)
            {
                if (row.IsHeader)
                    continue;

                Select(row.Entry);

                return true;
            }

            return false;
        }

        private int CurrentRow(IReadOnlyList<AttributeSampleRow> visible)
        {
            for (int i = 0; i < visible.Count; i++)
            {
                if (visible[i].IsHeader)
                {
                    if (selectedTitle == null && visible[i].Category == selectedCategory)
                        return i;

                    continue;
                }

                if (visible[i].Entry.Title == selectedTitle)
                    return i;
            }

            return -1;
        }

        // Landing on a header shows that category rather than an attribute, so walking the list with the
        // arrows reads as moving through a table of contents instead of skipping over the headings.
        private void Move(in AttributeSampleRow row)
        {
            if (row.IsHeader)
                SelectCategory(row.Category);
            else
                Select(row.Entry);
        }

        private void DrawContent(AttributeExplorerStyles styles)
        {
            if (_instance == null && !string.IsNullOrEmpty(selectedTitle))
                Restore();

            if (_editor == null && string.IsNullOrEmpty(selectedCategory))
            {
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField(AttributeExplorerStyles.EmptyMessage,
                    EditorStyles.centeredGreyMiniLabel);

                GUILayout.FlexibleSpace();

                return;
            }

            contentScroll = EditorGUILayout.BeginScrollView(contentScroll);
            EditorGUILayout.BeginVertical(styles.ContentPane);

            if (_editor == null)
                DrawCategoryPage(styles);
            else
                DrawAttributePage(styles);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        // The page a header opens: what the category is for, and everything in it with its one-liner, so
        // a reader who does not yet know the name of the attribute they want can still find it.
        private void DrawCategoryPage(AttributeExplorerStyles styles)
        {
            AttributeSampleEntry[] entries = AttributeSampleRegistry.All();
            int count = 0;

            foreach (AttributeSampleEntry entry in entries)
            {
                if (entry.CategoryName == selectedCategory)
                    count++;
            }

            EditorGUILayout.LabelField(AttributeExplorerStyles.CategoryEyebrow, styles.Eyebrow);
            EditorGUILayout.LabelField(selectedCategory, styles.PageHeading,
                GUILayout.Height(AttributeExplorerStyles.PageHeadingHeight));

            foreach (AttributeSampleEntry entry in entries)
            {
                if (entry.CategoryName != selectedCategory)
                    continue;

                Wrapped(AttributeCategoryInfo.Describe(entry.Category), styles.Description);
                break;
            }

            GUILayout.Space(AttributeExplorerStyles.SectionGap);
            EditorGUILayout.LabelField(string.Format(CountFormat, count), styles.Section);
            GUILayout.Space(AttributeExplorerStyles.TightGap);

            int row = 0;

            foreach (AttributeSampleEntry entry in entries)
            {
                if (entry.CategoryName != selectedCategory)
                    continue;

                DrawCategoryRow(entry, styles, row);
                row++;
            }
        }

        // A card per attribute: a background, a border and room inside it, so a row reads as an element
        // that can be clicked. Zebra striping was doing the separating instead, which only tells rows
        // apart and never says any of them is a thing you can press.
        private void DrawCategoryRow(in AttributeSampleEntry entry, AttributeExplorerStyles styles, int row)
        {
            string title = $"[{entry.Title}]";
            float width = _contentWidth - CardPadding * 2f;

            float height = styles.CardTitle.CalcHeight(ScratchContent.For(title), width);

            if (!string.IsNullOrEmpty(entry.Description))
                height += CardGap + styles.Bullet.CalcHeight(ScratchContent.For(entry.Description), width);

            Rect card = GUILayoutUtility.GetRect(_contentWidth, height + CardPadding * 2f);
            bool hovered = card.Contains(Event.current.mousePosition);

            if (Event.current.type == EventType.Repaint)
            {
                bool focused = contentFocused && row == focusedCard;

                Color fill = hovered
                    ? styles.CardHover
                    : focused
                        ? styles.CardFocused
                        : row % 2 == 0
                            ? styles.CardFill
                            : styles.CardFillAlternate;

                EditorGUI.DrawRect(card, fill);

                if (focused)
                    EditorGUI.DrawRect(new Rect(card.x, card.y, AttributeExplorerStyles.SelectionBarWidth,
                        card.height), styles.Selection);

                DrawBorder(card, styles.CardBorder);

                if (hovered)
                    EditorGUI.DrawRect(new Rect(card.x, card.y, AttributeExplorerStyles.SelectionBarWidth,
                        card.height), styles.Selection);
            }

            EditorGUIUtility.AddCursorRect(card, MouseCursor.Link);

            if (GUI.Button(card, GUIContent.none, GUIStyle.none))
                Select(entry);

            Rect name = new(card.x + CardPadding, card.y + CardPadding, width,
                styles.CardTitle.CalcHeight(ScratchContent.For(title), width));

            GUI.Label(name, title, styles.CardTitle);

            if (!string.IsNullOrEmpty(entry.Description))
                GUI.Label(new Rect(name.x, name.yMax + CardGap, width,
                    card.yMax - name.yMax - CardGap - CardPadding), entry.Description, styles.Bullet);

            GUILayout.Space(CardSpacing);
        }

        private void DrawAttributePage(AttributeExplorerStyles styles)
        {
            DrawHeading(styles);
            DrawRequirements(styles);
            DrawPreview(styles);
            DrawVariations(styles);
            DrawInfo(styles);
            DrawSource(styles);
        }

        private void DrawHeading(AttributeExplorerStyles styles)
        {
            EditorGUILayout.LabelField(_selected.CategoryName, styles.Eyebrow);

            EditorGUILayout.LabelField($"[{_selected.Title}]", styles.Heading,
                GUILayout.Height(HeadingHeight));

            if (!string.IsNullOrEmpty(_selected.Description))
                Wrapped(_selected.Description, styles.Description);

            GUILayout.Space(AttributeExplorerStyles.SectionGap);
        }

        // What the reader has to do before the preview does anything. Most attributes need nothing, but
        // the ones reading an Animator or a Material do, and a preview that looks broken until an
        // unstated field is filled in is the fastest way to make a reference look wrong.
        private void DrawRequirements(AttributeExplorerStyles styles)
        {
            if (string.IsNullOrEmpty(_selected.Requirements))
                return;

            EditorGUILayout.LabelField(RequirementsHeading, styles.Section);
            Wrapped(_selected.Requirements, styles.Body);

            GUILayout.Space(AttributeExplorerStyles.SectionGap);
        }

        // The whole sample, because a sample demonstrates exactly one attribute and everything in it is
        // therefore part of the answer: the bool a condition watches, the property a dropdown reads.
        private void DrawPreview(AttributeExplorerStyles styles)
        {
            EditorGUILayout.LabelField(PreviewHeading, styles.Section);

            EditorGUILayout.BeginVertical(styles.Card);
            _editor.OnInspectorGUI();
            EditorGUILayout.EndVertical();

            GUILayout.Space(AttributeExplorerStyles.SectionGap);
        }

        // The other ways the attribute can be written. The sample can only show one of them, and the rest
        // would otherwise only be findable by opening the attribute and reading its constructors.
        private void DrawVariations(AttributeExplorerStyles styles)
        {
            if (_selected.Variations.Length == 0)
                return;

            EditorGUILayout.LabelField(VariationsHeading, styles.Section);

            float width = _contentWidth - BulletWidth;

            foreach (string variation in _selected.Variations)
            {
                float height = styles.Bullet.CalcHeight(ScratchContent.For(variation), width);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(BulletGlyph, styles.Bullet, GUILayout.Width(BulletWidth),
                    GUILayout.Height(height));

                EditorGUILayout.LabelField(variation, styles.Bullet, GUILayout.Height(height));

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(AttributeExplorerStyles.SectionGap);
        }

        // Kept apart from the variations, which are ways of writing the attribute. A fact about how it
        // behaves is not one of those, and listing it as though it were made the list read wrong.
        private void DrawInfo(AttributeExplorerStyles styles)
        {
            if (string.IsNullOrEmpty(_selected.Info))
                return;

            EditorGUILayout.LabelField(InfoHeading, styles.Section);
            Wrapped(_selected.Info, styles.Body);

            GUILayout.Space(AttributeExplorerStyles.SectionGap);
        }

        private void DrawSource(AttributeExplorerStyles styles)
        {
            EditorGUILayout.LabelField(SourceHeading, styles.Section);

            // A row of their own, with room between them. Crammed against the heading the three read as
            // one wide control rather than as three things you can choose between.
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(CopyAttributeLabel, GUILayout.Height(ActionHeight),
                    GUILayout.Width(CopyAttributeWidth)))
            {
                EditorGUIUtility.systemCopyBuffer = $"[{_selected.Title}]";
                Notify(CopiedAttributeContent);
            }

            GUILayout.Space(ActionGap);

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_snippet)))
            {
                if (GUILayout.Button(CopyLabel, GUILayout.Height(ActionHeight),
                        GUILayout.Width(CopySnippetWidth)))
                {
                    EditorGUIUtility.systemCopyBuffer = _snippet;
                    Notify(CopiedContent);
                }
            }

            GUILayout.Space(ActionGap);

            using (new EditorGUI.DisabledScope(_script == null))
            {
                if (GUILayout.Button(OpenLabel, GUILayout.Height(ActionHeight),
                        GUILayout.Width(OpenFileWidth)))
                    AssetDatabase.OpenAsset(_script);
            }

            // Only a component sample offers this. A scene handle draws for the selected object and a
            // header control is drawn by the real Inspector, so neither can be shown in here at all.
            if (AttributeSampleHost.IsComponent(_selected.SampleType))
            {
                GUILayout.Space(ActionGap);

                if (GUILayout.Button(CreateSceneContent, GUILayout.Height(ActionHeight),
                        GUILayout.Width(CreateSceneWidth)))
                    AttributeSampleHost.CreateInScene(_selected.SampleType, _selected.Title);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(ActionGap);

            // Selectable rather than editable: a text area is the only control that lets a reader select
            // a line, and the disabled scope stops it suggesting the file can be changed from here.
            //
            // Given an explicit rect rather than left to the layout. A text area that does not wrap is as
            // wide as its longest line, and the window then refuses to be made narrower than that.
            float height = styles.Source.CalcHeight(ScratchContent.For(_snippet), _contentWidth);
            Rect area = GUILayoutUtility.GetRect(_contentWidth, height, GUILayout.ExpandWidth(false));

            using (new EditorGUI.DisabledScope(true))
                EditorGUI.TextArea(area, _snippet, styles.Source);
        }

        private void Wrapped(string text, GUIStyle style)
        {
            if (string.IsNullOrEmpty(text))
                return;

            EditorGUILayout.LabelField(text, style,
                GUILayout.Height(style.CalcHeight(ScratchContent.For(text), _contentWidth)));
        }

        private void Notify(GUIContent content)
        {
            if (_owner != null)
                _owner.ShowNotification(content, NotificationFade);
        }

        // A domain reload destroys the in-memory sample but the window survives with its selection, so
        // the sample is rebuilt from the name rather than leaving the pane blank.
        private void Restore()
        {
            if (AttributeSampleRegistry.TryFind(selectedTitle, out AttributeSampleEntry entry))
            {
                Select(entry);
                return;
            }

            selectedTitle = null;
        }
    }
}