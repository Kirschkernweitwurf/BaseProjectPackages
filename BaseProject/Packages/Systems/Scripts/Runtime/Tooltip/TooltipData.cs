using System;
using UnityEngine;

namespace Systems.Tooltip
{
    /// <summary>
    /// Data structure representing the information needed to display a tooltip.
    /// Contains the message to display and a function to get the screen position for the tooltip.
    /// </summary>
    public readonly struct TooltipData
    {
        public readonly string Message;
        public readonly Func<Vector2> GetScreenPosition;

        public TooltipData(string message, Func<Vector2> getScreenPosition)
        {
            Message = message;
            GetScreenPosition = getScreenPosition;
        }
    }
}