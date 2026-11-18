using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.TextPackage
{
    /// <summary>
    /// Keeps the row the player is on at its spot in a scroll view when the reading settings change. A new text size,
    /// font or spacing changes the height of every row above it, while the scroll view keeps its offset, so the row
    /// slid away, often out of view, right while the player changed the setting in it. The row is the selected one,
    /// which a hover selects too or the one selected last, since a mouse press drops the selection or else the top
    /// row in view.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public sealed class KeepRowInPlaceOnTextChange : MonoBehaviour
    {
        private const int CornerCount = 4;
        private const int TopLeftCorner = 1;

        [Required] [GetComponent]
        [Tooltip("The scroll view whose row stays in place. Auto-assigned from the same GameObject.")]
        [SerializeField] private ScrollRect scrollRect;

        private readonly Vector3[] _corners = new Vector3[CornerCount];

        private RectTransform _lastSelectedRow;
        private RectTransform _row;
        private float _rowTop;

#region Unity Callbacks
        private void OnEnable() => ReadableText.Changed += OnReadableTextChanged;

        // Late, after the texts took their new style this frame, so the layout measures the new sizes.
        private void LateUpdate()
        {
            RememberSelectedRow();

            if (_row == null)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            float shift = GetTopInView(_row) - _rowTop;
            _row = null;

            scrollRect.velocity = Vector2.zero;
            scrollRect.content.anchoredPosition -= new Vector2(0f, shift);
        }

        private void OnDisable()
        {
            ReadableText.Changed -= OnReadableTextChanged;
            _row = null;
            _lastSelectedRow = null;
        }
#endregion

        // Read before the new layout exists: the rows still sit where the player saw them.
        private void OnReadableTextChanged()
        {
            _row = FindRow();

            if (_row != null)
                _rowTop = GetTopInView(_row);
        }

        private RectTransform FindRow()
        {
            RectTransform selected = GetSelectedRow();

            if (selected != null)
                return selected;

            if (_lastSelectedRow != null
                && _lastSelectedRow.gameObject.activeInHierarchy)
                return _lastSelectedRow;

            RectTransform content = scrollRect.content;
            float viewTop = GetViewport().rect.yMax;

            foreach (Transform child in content)
            {
                if (child.gameObject.activeInHierarchy
                    && GetTopInView((RectTransform)child) <= viewTop)
                    return (RectTransform)child;
            }

            return null;
        }

        private void RememberSelectedRow()
        {
            RectTransform selected = GetSelectedRow();

            if (selected != null)
                _lastSelectedRow = selected;
        }

        private RectTransform GetSelectedRow()
        {
            GameObject selected = EventSystem.current == null
                ? null
                : EventSystem.current.currentSelectedGameObject;

            // IsChildOf counts the content as its own child, but the content is no row.
            return selected != null
                && selected.transform != scrollRect.content
                && selected.transform.IsChildOf(scrollRect.content)
                ? RowOf(selected.transform)
                : null;
        }

        // The child of the content that holds the selected part, like the row around a carousel's arrow.
        private RectTransform RowOf(Transform part)
        {
            Transform row = part;

            while (row.parent != scrollRect.content)
                row = row.parent;

            return (RectTransform)row;
        }

        private float GetTopInView(RectTransform row)
        {
            row.GetWorldCorners(_corners);
            return GetViewport().InverseTransformPoint(_corners[TopLeftCorner]).y;
        }

        private RectTransform GetViewport() => scrollRect.viewport == null
            ? (RectTransform)scrollRect.transform
            : scrollRect.viewport;
    }
}