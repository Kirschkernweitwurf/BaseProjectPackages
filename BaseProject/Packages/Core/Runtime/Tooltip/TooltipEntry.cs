using UnityEngine;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// One panel of a tooltip: an optional header with icon and title, and the text below it. A tooltip shows
    /// several of them side by side, for example an object's description plus one panel per keyword it uses.
    /// The text may use TextMeshPro rich text, including <c>&lt;sprite name="..."&gt;</c> icons.
    /// </summary>
    public readonly struct TooltipEntry
    {
        /// <summary>Bold header of the panel. Empty hides the title.</summary>
        public string Title { get; }

        /// <summary>Main text of the panel. Empty hides the text.</summary>
        public string Body { get; }

        /// <summary>Icon in front of the title. Null hides the icon.</summary>
        public Sprite Icon { get; }

        /// <summary>Tint of the icon, so one white icon can serve several categories.</summary>
        public Color IconColor { get; }

        /// <summary>Creates a panel with just a text.</summary>
        /// <param name="body">Text to show.</param>
        public TooltipEntry(string body) : this(null, body)
        {
        }

        /// <summary>Creates a panel with a header and a text.</summary>
        /// <param name="title">Header text. Null or empty for none.</param>
        /// <param name="body">Text to show. Null or empty for none.</param>
        /// <param name="icon">Icon in front of the title. Null for none.</param>
        /// <param name="iconColor">Tint of the icon. White when omitted.</param>
        public TooltipEntry(string title, string body, Sprite icon = null, Color? iconColor = null)
        {
            Title = title;
            Body = body;
            Icon = icon;
            IconColor = iconColor ?? Color.white;
        }
    }
}