using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.ExecutionOrderOverview
{
    /// <summary>
    /// Computes the column rectangles for a single row so the header and the rows stay
    /// aligned. Order is pinned left, the source badge is pinned right, and the name and
    /// namespace share the remaining width.
    /// </summary>
    public readonly struct ExecutionOrderColumnLayout
    {
        /// <summary>Effective execution order.</summary>
        public Rect Order { get; }

        /// <summary>Clickable script name.</summary>
        public Rect Name { get; }

        /// <summary>Type namespace.</summary>
        public Rect Namespace { get; }

        /// <summary>Source badge (pkg / lib).</summary>
        public Rect Badge { get; }

        /// <summary>Builds the column rectangles inside the given row.</summary>
        public ExecutionOrderColumnLayout(Rect row)
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
            Name = new Rect(fieldsLeft, y, nameWidth, height);

            float namespaceLeft = Name.xMax + padding;
            Namespace = new Rect(namespaceLeft, y, Mathf.Max(0f, fieldsRight - namespaceLeft), height);
        }
    }
}