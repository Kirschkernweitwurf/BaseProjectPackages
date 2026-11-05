using System;
using UnityEngine;

namespace Base.ControllerSupportPackage.InputPrompts.Glyphs
{
    /// <summary>
    /// The glyph of one physical control, like the G key or the left trigger. An action the player has rebound shows
    /// these, since its own entry only knows the button it started on.
    /// </summary>
    [Serializable]
    public sealed class InputControlGlyph
    {
        [field: Tooltip("Path of the control, like <Keyboard>/g or <Gamepad>/leftTrigger.")]
        [field: SerializeField] public string Path { get; private set; }

        [field: Tooltip("Sprite shown for this control.")]
        [field: SerializeField] public Sprite Sprite { get; private set; }
    }
}