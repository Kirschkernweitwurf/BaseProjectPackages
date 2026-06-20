#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuItemOverview
{
    /// <summary>
    /// Computes the column rectangles for a single row so the header and rows stay aligned.
    /// Priority is pinned left, the validation marker and source badge are pinned right, and
    /// the menu path and member share the remaining width.
    /// </summary>
    public readonly struct MenuItemColumnLayout
    {
        /// <summary>Menu priority.</summary>
        public Rect Priority { get; }

        /// <summary>Clickable menu path.</summary>
        public Rect Path { get; }

        /// <summary>Declaring "Type.Method".</summary>
        public Rect Member { get; }

        /// <summary>Compact validation marker.</summary>
        public Rect Validation { get; }

        /// <summary>Source badge (pkg / lib).</summary>
        public Rect Badge { get; }

        /// <summary>Builds the column rectangles inside the given row.</summary>
        public MenuItemColumnLayout(Rect row)
        {
            const float padding = 6f;
            const float priorityWidth = 50f;
            const float validationWidth = 18f;
            const float badgeWidth = 38f;

            float height = EditorGUIUtility.singleLineHeight;
            float y = row.y + (row.height - height) * 0.5f;
            float left = row.x + padding;
            float right = row.xMax - padding;

            Priority = new Rect(left, y, priorityWidth, height);
            Badge = new Rect(right - badgeWidth, y, badgeWidth, height);
            Validation = new Rect(Badge.x - validationWidth, y, validationWidth, height);

            float fieldsLeft = Priority.xMax + padding;
            float fieldsRight = Validation.x - padding;
            float fieldsWidth = Mathf.Max(0f, fieldsRight - fieldsLeft);

            float pathWidth = fieldsWidth * 0.6f;
            Path = new Rect(fieldsLeft, y, pathWidth, height);

            float memberLeft = Path.xMax + padding;
            Member = new Rect(memberLeft, y, Mathf.Max(0f, fieldsRight - memberLeft), height);
        }
    }
}
#endif