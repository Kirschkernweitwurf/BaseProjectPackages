using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// Paints a UI Toolkit tree from the active theme by class name, for the colors a style sheet
    /// cannot take from it.
    /// </summary>
    /// <remarks>
    /// A sheet routes its colors through USS custom properties, and nothing in C# can write one, so a
    /// themed color has to arrive as an inline style instead. This is the registration list for that:
    /// a window says which of its classes stand for which palette color once, and every repaint after
    /// that follows from the same list.
    /// <para>
    /// Colors are registered as functions rather than values, because the point is to read them again
    /// each time the theme moves. A window keeps whatever its sheet says for anything it does not
    /// register, which is the right home for a color only that window has a meaning for.
    /// </para>
    /// </remarks>
    public sealed class EditorUssPainter
    {
        private readonly List<Entry> _entries = new();

        /// <summary>
        /// Paints the background of every element carrying the class.
        /// </summary>
        /// <param name="className">The USS class to look for.</param>
        /// <param name="color">Reads the color, called again on every repaint.</param>
        /// <returns>The same painter, so registrations can be chained.</returns>
        public EditorUssPainter Background(string className, Func<Color> color)
            => Add(className, color, EUssPaintTarget.Background);

        /// <summary>
        /// Paints the border of every element carrying the class, on all four sides.
        /// </summary>
        /// <remarks>
        /// Only the color is set. The widths stay with the sheet, because whether an edge is drawn at
        /// all is part of the shape rather than part of the theme.
        /// </remarks>
        /// <param name="className">The USS class to look for.</param>
        /// <param name="color">Reads the color, called again on every repaint.</param>
        /// <returns>The same painter, so registrations can be chained.</returns>
        public EditorUssPainter Border(string className, Func<Color> color)
            => Add(className, color, EUssPaintTarget.Border);

        /// <summary>
        /// Paints the text of every element carrying the class.
        /// </summary>
        /// <param name="className">The USS class to look for.</param>
        /// <param name="color">Reads the color, called again on every repaint.</param>
        /// <returns>The same painter, so registrations can be chained.</returns>
        public EditorUssPainter Text(string className, Func<Color> color)
            => Add(className, color, EUssPaintTarget.Text);

        /// <summary>
        /// Applies every registration to the tree.
        /// </summary>
        /// <remarks>
        /// Safe to call as often as needed, and it walks the tree as it is each time, so elements the
        /// window added since the last call are picked up.
        /// </remarks>
        /// <param name="root">The element to paint, along with everything under it.</param>
        public void Paint(VisualElement root)
        {
            if (root == null)
                return;

            foreach (Entry entry in _entries)
                PaintEntry(root, entry);
        }

        private static void PaintEntry(VisualElement root, Entry entry)
        {
            Color color = entry.Color.Invoke();

            foreach (VisualElement element in root.Query(className: entry.ClassName).Build())
                Assign(element, entry.Target, color);
        }

        private static void Assign(VisualElement element, EUssPaintTarget target, Color color)
        {
            switch (target)
            {
                case EUssPaintTarget.Background:
                    element.style.backgroundColor = color;
                    break;

                case EUssPaintTarget.Border:
                    element.style.borderTopColor = color;
                    element.style.borderBottomColor = color;
                    element.style.borderLeftColor = color;
                    element.style.borderRightColor = color;
                    break;

                case EUssPaintTarget.Text:
                    element.style.color = color;
                    break;
            }
        }

        private EditorUssPainter Add(string className, Func<Color> color, EUssPaintTarget target)
        {
            if (string.IsNullOrEmpty(className) || color == null)
                return this;

            _entries.Add(new Entry(className, color, target));

            return this;
        }

        private readonly struct Entry
        {
            internal string ClassName { get; }

            internal Func<Color> Color { get; }

            internal EUssPaintTarget Target { get; }

            internal Entry(string className, Func<Color> color, EUssPaintTarget target)
            {
                ClassName = className;
                Color = color;
                Target = target;
            }
        }
    }
}