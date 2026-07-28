using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows.MenuItemOverview
{
    /// <summary>
    /// Computes the column rectangles for a single row so the header and rows stay aligned.
    /// Priority and the kind chip are pinned left, the state marker, the source badge and the
    /// manage link are pinned right, and the menu path and member share the remaining width.
    /// </summary>
    public readonly struct MenuItemColumnLayout
    {
        /// <summary>Menu priority.</summary>
        public Rect Priority { get; }

        /// <summary>Chip that tells a dynamic entry from a static one.</summary>
        public Rect Kind { get; }

        /// <summary>Clickable menu path.</summary>
        public Rect Path { get; }

        /// <summary>Declaring "Type.Method".</summary>
        public Rect Member { get; }

        /// <summary>Compact state marker for validation, disabled and missing entries.</summary>
        public Rect State { get; }

        /// <summary>Source badge (pkg / lib).</summary>
        public Rect Badge { get; }

        /// <summary>Link that opens a dynamic entry in the menu item manager.</summary>
        public Rect Manage { get; }

        /// <summary>Builds the column rectangles inside the given row.</summary>
        public MenuItemColumnLayout(Rect row)
        {
            const float priorityWidth = 46f;
            const float kindWidth = 62f;
            const float stateWidth = 30f;
            const float badgeWidth = 32f;
            const float manageWidth = 66f;

            float padding = MenuOverviewGui.Padding;
            float height = EditorGUIUtility.singleLineHeight;
            float y = row.y + (row.height - height) * 0.5f;
            float left = row.x + MenuOverviewGui.StripeWidth + padding;
            float right = row.xMax - padding;

            Priority = new Rect(left, y, priorityWidth, height);
            Kind = new Rect(Priority.xMax + padding, y, kindWidth, height);

            Manage = new Rect(right - manageWidth, y, manageWidth, height);
            Badge = new Rect(Manage.x - padding - badgeWidth, y, badgeWidth, height);
            State = new Rect(Badge.x - stateWidth, y, stateWidth, height);

            float fieldsLeft = Kind.xMax + padding;
            float fieldsRight = State.x - padding;
            float fieldsWidth = Mathf.Max(0f, fieldsRight - fieldsLeft);

            float pathWidth = fieldsWidth * 0.58f;
            Path = new Rect(fieldsLeft, y, pathWidth, height);

            float memberLeft = Path.xMax + padding;
            Member = new Rect(memberLeft, y, Mathf.Max(0f, fieldsRight - memberLeft), height);
        }
    }
}