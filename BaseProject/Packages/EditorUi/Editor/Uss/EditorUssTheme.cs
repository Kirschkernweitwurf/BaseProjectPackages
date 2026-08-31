using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// Gives a UI Toolkit window the shared Base look and keeps it in step with the active theme.
    /// </summary>
    /// <remarks>
    /// UI Toolkit has no way to write a USS custom property from C#, so the sheet cannot simply read
    /// the theme. The split instead is that the sheet owns shape, spacing and typography, and this
    /// class writes the colors on top as inline styles, which override a sheet. One sheet therefore
    /// serves both editor skins and every theme, and a color changed in the settings page lands in a
    /// UI Toolkit window the same way it lands in an IMGUI one.
    /// <para>
    /// <see cref="Track"/> keeps watching after the first paint. It schedules itself on the element,
    /// so it stops on its own when the window closes rather than outliving it the way a static event
    /// would while Domain Reload is off.
    /// </para>
    /// </remarks>
    public static class EditorUssTheme
    {
        /// <summary>The GUID of the shared sheet, from its meta file.</summary>
        public const string SheetGuid = "0b7c4f6a2d8e4a1cb95f3e07d41a6c58";

        private const long PollMilliseconds = 250;
        private const string SheetFilter = "BaseEditorUi t:StyleSheet";

        /// <summary>
        /// Marks the root as a Base window, attaches the shared sheet, paints it from the active
        /// theme and keeps repainting it whenever the theme moves.
        /// </summary>
        /// <remarks>
        /// Call once, after the window has built its tree. A window that builds more elements later
        /// does not have to call anything again, because the repaint walks the tree as it is each
        /// time it runs.
        /// </remarks>
        /// <param name="root">The window's root visual element.</param>
        /// <param name="onThemeChanged">
        /// Called after each repaint that a theme change caused, for a window that also has colors it
        /// sets by hand. A GraphView node paints its own containers, and those are only written while
        /// the node is being built, so such a window rebuilds from here instead.
        /// </param>
        /// <returns>False when the shared sheet could not be found, so the caller can report it.</returns>
        public static bool Apply(VisualElement root, Action onThemeChanged = null)
        {
            if (root == null)
                return false;

            if (!root.ClassListContains(EditorUiClass.Root))
                root.AddToClassList(EditorUiClass.Root);

            bool attached = Attach(root);

            Paint(root);
            Track(root, onThemeChanged);

            return attached;
        }

        // The GUID is the reliable way in, but a meta file Unity decides to regenerate takes the
        // sheet out of reach of it, and an unstyled UI Toolkit window reads as broken rather than
        // plain. The name search is the fallback for exactly that case.
        private static bool Attach(VisualElement root)
        {
            if (EditorStyleSheets.Apply(root, SheetGuid))
                return true;

            foreach (string guid in AssetDatabase.FindAssets(SheetFilter))
            {
                if (EditorStyleSheets.Apply(root, guid))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// The same as the other overload, with a painter that carries the colors this window's own
        /// sheet cannot take from the theme. It is applied straight away and again on every change.
        /// </summary>
        /// <param name="root">The window's root visual element.</param>
        /// <param name="painter">The window's own class to color registrations.</param>
        /// <returns>False when the shared sheet could not be found, so the caller can report it.</returns>
        public static bool Apply(VisualElement root, EditorUssPainter painter)
        {
            if (painter == null)
                return Apply(root);

            bool attached = Apply(root, () => painter.Paint(root));

            painter.Paint(root);

            return attached;
        }

        /// <summary>
        /// Writes the active theme's colors onto every element carrying a shared class.
        /// </summary>
        /// <remarks>
        /// Safe to call as often as needed. It only assigns, so an element the window has since
        /// added is picked up on the next pass.
        /// </remarks>
        /// <param name="root">The element to paint, along with everything under it.</param>
        public static void Paint(VisualElement root)
        {
            if (root == null)
                return;

            PaintBackground(root, EditorUiClass.Card, EditorPalette.Card);
            PaintBackground(root, EditorUiClass.Toolbar, EditorTableStyles.HeaderColor);
            PaintBackground(root, EditorUiClass.Separator, EditorPalette.Separator);
            PaintBackground(root, EditorUiClass.RowAlternate, EditorPalette.Stripe);
            PaintBackground(root, EditorUiClass.RowSelected, EditorPalette.SelectionFill);

            PaintText(root, EditorUiClass.Title, EditorPalette.Text);
            PaintText(root, EditorUiClass.Subtitle, EditorPalette.DimText);
            PaintText(root, EditorUiClass.SectionHeader, EditorPalette.Text);
            PaintText(root, EditorUiClass.Dim, EditorPalette.DimText);
            PaintText(root, EditorUiClass.Accent, EditorPalette.Accent);
            PaintText(root, EditorUiClass.Success, EditorPalette.Success);
            PaintText(root, EditorUiClass.Warning, EditorPalette.Warning);
            PaintText(root, EditorUiClass.Danger, EditorPalette.Danger);
            PaintText(root, EditorUiClass.EmptyTitle, EditorPalette.DimText);
            PaintText(root, EditorUiClass.EmptyHint, EditorPalette.DimText);

            PaintRadius(root, EditorUiClass.Card, EditorMetrics.CardCornerRadius);
            PaintRadius(root, EditorUiClass.Button, EditorMetrics.CardCornerRadius);
            PaintRadius(root, EditorUiClass.Badge, EditorMetrics.PillCornerRadius);
            PaintRadius(root, EditorUiClass.Chip, EditorMetrics.PillCornerRadius);

            PaintButtons(root);
        }

        /// <summary>
        /// Repaints the tree whenever the active theme changes, for as long as the element lives.
        /// </summary>
        /// <param name="root">The element to keep in step with the theme.</param>
        /// <param name="onThemeChanged">Called after the repaint, for colors the caller sets itself.</param>
        public static void Track(VisualElement root, Action onThemeChanged = null)
        {
            if (root == null)
                return;

            int revision = EditorThemeProvider.Revision;

            // Scheduled on the element, so it stops when the window closes. A static event would
            // outlive it while Domain Reload is off and keep calling into a window that is gone.
            root.schedule.Execute(() =>
            {
                if (revision == EditorThemeProvider.Revision)
                    return;

                revision = EditorThemeProvider.Revision;

                Paint(root);

                onThemeChanged?.Invoke();
            }).Every(PollMilliseconds);
        }

        private static void PaintButtons(VisualElement root)
        {
            foreach (VisualElement element in root.Query(className: EditorUiClass.ButtonPrimary).Build())
            {
                element.style.backgroundColor = EditorPalette.Accent;
                element.style.color = EditorPalette.AccentText;
            }

            foreach (VisualElement element in root.Query(className: EditorUiClass.ButtonSecondary).Build())
            {
                element.style.backgroundColor = EditorPalette.Secondary;
                element.style.color = EditorPalette.SecondaryText;
            }
        }

        private static void PaintBackground(VisualElement root, string className, Color color)
        {
            foreach (VisualElement element in root.Query(className: className).Build())
                element.style.backgroundColor = color;
        }

        private static void PaintText(VisualElement root, string className, Color color)
        {
            foreach (VisualElement element in root.Query(className: className).Build())
                element.style.color = color;
        }

        private static void PaintRadius(VisualElement root, string className, int radius)
        {
            foreach (VisualElement element in root.Query(className: className).Build())
            {
                element.style.borderTopLeftRadius = radius;
                element.style.borderTopRightRadius = radius;
                element.style.borderBottomLeftRadius = radius;
                element.style.borderBottomRightRadius = radius;
            }
        }
    }
}