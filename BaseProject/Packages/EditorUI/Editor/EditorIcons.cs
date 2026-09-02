using UnityEditor;
using UnityEngine;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// The built-in editor icons the Base windows report findings with, resolved once and cached.
    /// </summary>
    /// <remarks>
    /// <see cref="EditorGUIUtility.IconContent(string)"/> is only valid inside a GUI callback, so
    /// every icon here is resolved on first access rather than in a static constructor.
    /// </remarks>
    public static class EditorIcons
    {
        private const string ErrorName = "console.erroricon.sml";
        private const string GameObjectName = "GameObject Icon";
        private const string ScriptName = "cs Script Icon";
        private const string SuccessName = "TestPassed";
        private const string WarningName = "console.warnicon.sml";

        /// <summary>Red icon for a finding that breaks something.</summary>
        public static Texture Error => Get(ref _error, ErrorName);

        /// <summary>Default icon for a game object in a group header.</summary>
        public static Texture GameObject => Get(ref _gameObject, GameObjectName);

        /// <summary>Script icon for a type in a group header.</summary>
        public static Texture Script => Get(ref _script, ScriptName);

        /// <summary>Green check shown when a scan found nothing to report.</summary>
        public static Texture Success => Get(ref _success, SuccessName);

        /// <summary>Yellow icon for a finding that only changes behavior.</summary>
        public static Texture Warning => Get(ref _warning, WarningName);

        private static Texture _error;
        private static Texture _gameObject;
        private static Texture _script;
        private static Texture _success;
        private static Texture _warning;

        /// <summary>
        /// Resolves any built-in editor icon by name, for the ones this class does not name itself.
        /// </summary>
        /// <param name="iconName">The built-in icon name.</param>
        /// <returns>The icon texture, or null outside a GUI callback.</returns>
        public static Texture Named(string iconName) => EditorGUIUtility.IconContent(iconName).image;

        /// <summary>
        /// Draws a built-in icon centered in an area, at its own size or a whole multiple of it.
        /// </summary>
        /// <remarks>
        /// The built-in icons are small bitmaps rather than vectors. Handing one to
        /// <see cref="GUILayout.Label(GUIContent, GUILayoutOption[])"/> draws it into the content
        /// rect of a style, which is the given size less that style's padding, so a sixteen pixel
        /// icon asked for at sixteen pixels comes out at thirteen or fourteen. A bitmap resampled to
        /// a fraction of its size lands between pixels on every edge, and that is what reads as
        /// ragged rather than merely soft. Asking for one at a size it does not have, such as the
        /// forty four pixels of an empty state, does the same thing in the other direction.
        /// <para>
        /// So the size is snapped to a whole multiple and the position to a whole pixel. The retina
        /// variant of a built-in icon is twice the size and meant to be drawn at the same number of
        /// points, which is why the measurement goes through
        /// <see cref="EditorGUIUtility.pixelsPerPoint"/> before the multiple is worked out.
        /// </para>
        /// </remarks>
        /// <param name="area">The area to center the icon in.</param>
        /// <param name="icon">The icon to draw. Nothing is drawn when this is null.</param>
        public static void Draw(Rect area, Texture icon)
        {
            if (Event.current.type != EventType.Repaint || icon == null)
                return;

            GUI.DrawTexture(Snap(area, icon), icon, ScaleMode.ScaleToFit);
        }

        private static Rect Snap(Rect area, Texture icon)
        {
            float points = Mathf.Max(1f, EditorGUIUtility.pixelsPerPoint);
            float width = icon.width / points;
            float height = icon.height / points;

            if (width <= 0f || height <= 0f)
                return area;

            float scale = Mathf.Max(1f, Mathf.Floor(Mathf.Min(area.width / width, area.height / height)));

            width *= scale;
            height *= scale;

            return new Rect(Mathf.Round(area.x + (area.width - width) * 0.5f),
                Mathf.Round(area.y + (area.height - height) * 0.5f), width, height);
        }

        private static Texture Get(ref Texture cached, string iconName)
        {
            if (cached == null)
                cached = Named(iconName);

            return cached;
        }
    }
}