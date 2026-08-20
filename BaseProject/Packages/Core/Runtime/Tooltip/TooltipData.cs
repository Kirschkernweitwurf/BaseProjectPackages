using System;
using UnityEngine;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Immutable payload of a single tooltip request: the message and where to draw it.
    /// </summary>
    public readonly struct TooltipData
    {
        /// <summary>Text shown in the tooltip.</summary>
        public string Message { get; }

        /// <summary>Screen position the tooltip follows. Queried every frame while visible.</summary>
        public Func<Vector2> GetScreenPosition { get; }

        /// <summary>Creates a tooltip payload.</summary>
        /// <param name="message">Text to show.</param>
        /// <param name="getScreenPosition">Supplies the screen position to follow.</param>
        public TooltipData(string message, Func<Vector2> getScreenPosition)
        {
            Message = message;
            GetScreenPosition = getScreenPosition;
        }
    }
}