using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.ExecutionOrderOverview
{
    /// <summary>
    /// Computes the column rectangles for a single row so the header and the rows stay
    /// aligned. The layout is derived purely from the supplied row rectangle.
    /// </summary>
    public readonly struct ExecutionOrderColumnLayout
    {
        /// <summary>Effective execution order.</summary>
        public Rect Order { get; }

        /// <summary>Clickable script name.</summary>
        public Rect Name { get; }

        /// <summary>Type namespace.</summary>
        public Rect Namespace { get; }

        /// <summary>Optional package marker.</summary>
        public Rect Package { get; }

        /// <summary>Builds the column rectangles inside the given row.</summary>
        public ExecutionOrderColumnLayout(Rect row)
        {
            const float padding = 4f;
            float height = EditorGUIUtility.singleLineHeight;
            float y = row.y + (row.height - height) * 0.5f;
            float x = row.x + padding;

            Order = new Rect(x, y, 55f, height);
            x += 60f;

            float nameWidth = Mathf.Max(150f, row.width * 0.30f);
            Name = new Rect(x, y, nameWidth, height);
            x += nameWidth + padding;

            float namespaceWidth = Mathf.Max(90f, row.width * 0.22f);
            Namespace = new Rect(x, y, namespaceWidth, height);
            x += namespaceWidth + padding;

            Package = new Rect(x, y, 40f, height);
        }
    }
}