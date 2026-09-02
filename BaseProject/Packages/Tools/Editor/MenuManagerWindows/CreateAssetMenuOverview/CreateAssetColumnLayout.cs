using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.MenuManagerWindows.CreateAssetMenuOverview
{
    /// <summary>
    /// Computes the column rectangles for a single row so the header and rows stay aligned.
    /// Order and the kind chip are pinned left, the state marker, the source badge and the
    /// manage link are pinned right, and the menu name, type and file name share the rest.
    /// </summary>
    internal readonly struct CreateAssetColumnLayout
    {
        /// <summary>Menu order.</summary>
        internal Rect Order { get; }

        /// <summary>Chip that tells a dynamic entry from a static one.</summary>
        internal Rect Kind { get; }

        /// <summary>Clickable menu name.</summary>
        internal Rect MenuName { get; }

        /// <summary>ScriptableObject type name.</summary>
        internal Rect Type { get; }

        /// <summary>Default file name.</summary>
        internal Rect FileName { get; }

        /// <summary>Compact state marker for disabled and missing entries.</summary>
        internal Rect State { get; }

        /// <summary>Source badge (pkg / lib).</summary>
        internal Rect Badge { get; }

        /// <summary>Link that opens a dynamic entry in the create asset manager.</summary>
        internal Rect Manage { get; }

        /// <summary>Builds the column rectangles inside the given row.</summary>
        public CreateAssetColumnLayout(Rect row)
        {
            const float orderWidth = 46f;
            const float kindWidth = 62f;
            const float stateWidth = 30f;
            const float badgeWidth = 32f;
            const float manageWidth = 66f;

            float padding = MenuOverviewGui.Padding;
            float height = EditorGUIUtility.singleLineHeight;
            float y = row.y + (row.height - height) * 0.5f;
            float left = row.x + MenuOverviewGui.StripeWidth + padding;
            float right = row.xMax - padding;

            Order = new Rect(left, y, orderWidth, height);
            Kind = new Rect(Order.xMax + padding, y, kindWidth, height);

            Manage = new Rect(right - manageWidth, y, manageWidth, height);
            Badge = new Rect(Manage.x - padding - badgeWidth, y, badgeWidth, height);
            State = new Rect(Badge.x - stateWidth, y, stateWidth, height);

            float fieldsLeft = Kind.xMax + padding;
            float fieldsRight = State.x - padding;
            float fieldsWidth = Mathf.Max(0f, fieldsRight - fieldsLeft);

            float nameWidth = fieldsWidth * 0.45f;
            MenuName = new Rect(fieldsLeft, y, nameWidth, height);

            float typeLeft = MenuName.xMax + padding;
            float typeWidth = fieldsWidth * 0.3f;
            Type = new Rect(typeLeft, y, typeWidth, height);

            float fileLeft = Type.xMax + padding;
            FileName = new Rect(fileLeft, y, Mathf.Max(0f, fieldsRight - fileLeft), height);
        }
    }
}