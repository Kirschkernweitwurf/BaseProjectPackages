#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CreateAssetMenuOverview
{
    /// <summary>
    /// Computes the column rectangles for a single row so the header and rows stay aligned.
    /// Order is pinned left, the source badge is pinned right, and the menu name, type and
    /// file name share the remaining width.
    /// </summary>
    public readonly struct CreateAssetColumnLayout
    {
        /// <summary>Menu order.</summary>
        public Rect Order { get; }

        /// <summary>Clickable menu name.</summary>
        public Rect MenuName { get; }

        /// <summary>ScriptableObject type name.</summary>
        public Rect Type { get; }

        /// <summary>Default file name.</summary>
        public Rect FileName { get; }

        /// <summary>Source badge (pkg / lib).</summary>
        public Rect Badge { get; }

        /// <summary>Builds the column rectangles inside the given row.</summary>
        public CreateAssetColumnLayout(Rect row)
        {
            const float padding = 6f;
            const float orderWidth = 50f;
            const float badgeWidth = 38f;

            float height = EditorGUIUtility.singleLineHeight;
            float y = row.y + (row.height - height) * 0.5f;
            float left = row.x + padding;
            float right = row.xMax - padding;

            Order = new Rect(left, y, orderWidth, height);
            Badge = new Rect(right - badgeWidth, y, badgeWidth, height);

            float fieldsLeft = Order.xMax + padding;
            float fieldsRight = Badge.x - padding;
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
#endif