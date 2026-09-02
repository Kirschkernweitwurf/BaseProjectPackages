using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// The list every Base window shows a collection in. It is Unity's <see cref="ReorderableList"/>
    /// drawn by Unity, inside <see cref="EditorListTintScope"/>, so the header bar, the box, the drag
    /// handles, the selection and the tab holding add and remove are the ones from the inspector,
    /// only in the theme's tone.
    /// </summary>
    /// <remarks>
    /// An earlier version turned <c>showDefaultBackground</c> off and repainted the chrome by hand.
    /// That was the wrong trade: it bought recolorable surfaces and gave up the one pixel row inset,
    /// the header height floor, the footer Unity draws and, worst of all, every array nested inside a
    /// row, which Unity keeps drawing in the built-in grey no matter what the list around it does.
    /// Tinting reaches all of it and reimplements none of it.
    /// <para>
    /// What is left here is the small amount a Base window wants on top: a title in place of the
    /// property name, a line for the empty state, and add and remove handed to the caller. The
    /// foldout and the typed array size beside it are Unity's, rebuilt because a header callback
    /// replaces them wholesale, and folding shut is honored by drawing the strip and nothing else.
    /// </para>
    /// <para>
    /// The target is a list indistinguishable from the one the inspector puts in front of an array,
    /// so anything that wrapper switches on is switched on here too. Delete, the row context menu,
    /// keyboard navigation and the scheduled remove all come free with the footer being Unity's, but
    /// the selection mode does not, and is set below.
    /// </para>
    /// </remarks>
    public sealed class EditorList
    {
        /// <summary>
        /// The two pixels Unity adds to every row that has any height. Its own element drawer takes
        /// them back off before drawing; a custom one is left to do it, so it is done here.
        /// </summary>
        private const float ElementPadding = 2f;

        /// <summary>Width the foldout arrow takes before the title starts.</summary>
        private const float FoldoutWidth = 13f;

        /// <summary>How far Unity pushes the header content down inside the header strip.</summary>
        private const float HeaderContentLift = 1f;

        /// <summary>How much shorter the header content is than the header strip.</summary>
        private const float HeaderContentShrink = 2f;

        /// <summary>Width of the array size field at the right of the header.</summary>
        private const float SizeFieldWidth = 48f;

        /// <summary>
        /// The name in the header bar. Leave empty for a list with no header, which suits a list that
        /// already sits under a section header of its own.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Draws the content of one row into the rect the list hands over, which already excludes the
        /// drag handle, the padding at both ends and the two pixels Unity pads every row by.
        /// </summary>
        public Action<Rect, int, bool> DrawElement { get; set; }

        /// <summary>
        /// The height of one row, for a list whose rows are not all the same height. Rows are the
        /// height Unity gives its own when this is not set.
        /// </summary>
        public Func<int, float> ElementHeight { get; set; }

        /// <summary>
        /// Runs when the plus is pressed. Unity appends the element itself when this is not set,
        /// which copies the last one the way it does everywhere else.
        /// </summary>
        public Action OnAdd { get; set; }

        /// <summary>
        /// Runs when the minus is pressed, with the selected index. Unity deletes the element itself
        /// when this is not set.
        /// </summary>
        public Action<int> OnRemove { get; set; }

        /// <summary>The line shown in place of the rows while the collection is empty.</summary>
        public string EmptyLabel { get; set; } = string.Empty;

        /// <summary>Whether a row can be dragged to a new position.</summary>
        public bool Draggable
        {
            get => _list.draggable;
            set => _list.draggable = value;
        }

        /// <summary>The selected row.</summary>
        public int SelectedIndex => _list.index;

        /// <summary>Number of rows the list is showing.</summary>
        public int Count => _list.count;

        private readonly ReorderableList _list;
        private readonly float _defaultHeaderHeight;

        private EditorWindowStyles _styles;

        /// <summary>
        /// Creates a list over one array property. Build a new one when the serialized object behind
        /// it is replaced, because the property it holds does not survive that.
        /// </summary>
        /// <param name="elements">The array property the rows come from.</param>
        public EditorList(SerializedProperty elements)
        {
            // Off by default on a list built by hand, on for every array the inspector draws, because
            // the wrapper Unity puts in front of one turns it on. Leaving it off is the whole reason a
            // list built here felt unlike the nested one inside a row: no shift range, no control add
            // to the selection, and a minus that could only ever take one element at a time.
            _list = new ReorderableList(elements.serializedObject, elements, true, true, true, true)
            {
                multiSelect = true
            };

            // Whatever this Unity sizes its own header at, so a list with no title can put it back.
            _defaultHeaderHeight = _list.headerHeight;

            _list.drawHeaderCallback = DrawHeaderContent;
            _list.drawElementCallback = (rect, index, isActive, isFocused) => DrawRow(rect, index, isActive);
            _list.drawNoneElementCallback = DrawEmpty;
            _list.elementHeightCallback = HeightOf;
        }

        /// <summary>Draws the list.</summary>
        /// <param name="styles">The built chrome styles.</param>
        public void DrawLayout(EditorWindowStyles styles)
        {
            if (styles == null)
                return;

            _styles = styles;

            _list.headerHeight = HasHeader()
                ? _defaultHeaderHeight
                : 0f;

            // Assigned per pass rather than in the constructor, because a null callback is how Unity
            // is told to keep its own add and remove, and both are set after the list is built.
            _list.onAddCallback = OnAdd == null
                ? null
                : AddCallback;

            _list.onRemoveCallback = OnRemove == null
                ? null
                : RemoveCallback;

            // Folded shut, the header strip is all there is, exactly as an array in the inspector
            // behaves. Unity has no switch for that, it simply stops drawing the rest, so the strip is
            // asked for on its own and its background is still Unity's own.
            if (HasHeader() && !_list.serializedProperty.isExpanded)
            {
                DrawCollapsedHeader();
                return;
            }

            using (new EditorListTintScope())
                _list.DoLayoutList();
        }

        /// <summary>Selects a row, which is what the minus acts on.</summary>
        /// <param name="index">The row to select.</param>
        public void Select(int index) => _list.Select(index);

        /// <summary>
        /// The rectangle Unity hands a header callback: the strip less the padding at both ends, a
        /// pixel down and two pixels shorter. Mirrored so the folded header lines up with the open one.
        /// </summary>
        private static Rect HeaderContentRect(Rect header)
        {
            header.xMin += ReorderableList.Defaults.padding;
            header.xMax -= ReorderableList.Defaults.padding;
            header.height -= HeaderContentShrink;
            header.y += HeaderContentLift;

            return header;
        }

        private bool HasHeader() => !string.IsNullOrEmpty(Title);

        private void DrawCollapsedHeader()
        {
            Rect header = GUILayoutUtility.GetRect(0f, _list.headerHeight, GUILayout.ExpandWidth(true));

            using (new EditorListTintScope())
            {
                ReorderableList.defaultBehaviours.DrawHeaderBackground(header);

                DrawHeaderContent(HeaderContentRect(header));
            }
        }

        private void DrawHeaderContent(Rect rect)
        {
            if (_styles == null || !HasHeader())
                return;

            SerializedProperty elements = _list.serializedProperty;

            Rect size = new(rect.xMax - SizeFieldWidth, rect.y, SizeFieldWidth, rect.height);
            Rect label = new(rect.x, rect.y, Mathf.Max(0f, size.x - rect.x - EditorMetrics.TightGap),
                rect.height);

            // The arrow is drawn with no content of its own and given the whole width up to the size
            // field, so the title beside it is clickable too, and the title is then labelled over it
            // in the theme's own color rather than the one the foldout style carries.
            bool wasExpanded = elements.isExpanded;
            bool isExpanded = EditorGUI.Foldout(label, wasExpanded, GUIContent.none, true);

            if (isExpanded != wasExpanded)
                elements.isExpanded = isExpanded;

            GUI.Label(new Rect(label.x + FoldoutWidth, label.y, Mathf.Max(0f, label.width - FoldoutWidth),
                label.height), Title, _styles.Header);

            // Delayed, so typing a three does not resize the array twice on the way to thirty.
            int count = Mathf.Max(0, EditorGUI.DelayedIntField(size, elements.arraySize));

            if (count != elements.arraySize)
                elements.arraySize = count;
        }

        private void DrawRow(Rect rect, int index, bool isActive)
        {
            if (DrawElement == null)
                return;

            float padding = rect.height > 0f
                ? ElementPadding
                : 0f;

            DrawElement(new Rect(rect.x, rect.y + padding * 0.5f, rect.width, rect.height - padding), index,
                isActive);
        }

        private void DrawEmpty(Rect rect)
        {
            if (_styles == null)
                return;

            GUI.Label(rect, EmptyLabel, _styles.EmptyHint);
        }

        private float HeightOf(int index) => ElementHeight == null
            ? _list.elementHeight
            : ElementHeight(index);

        private void AddCallback(ReorderableList list) => OnAdd();

        private void RemoveCallback(ReorderableList list) => OnRemove(list.index);
    }
}