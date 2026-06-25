using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.ControllerSupport.InputPrompts.Glyphs
{
    /// <summary>
    /// Maps a single input action to its prompt glyph for one device. Holds both a sprite (for raw
    /// <see cref="UnityEngine.UI.Image"/> display) and a TextMeshPro sprite name (for inline tags).
    /// </summary>
    [Serializable]
    public sealed class InputGlyphEntry
    {
        [field: Tooltip("The action this glyph represents.")]
        [field: SerializeField] public InputActionReference Action { get; private set; }

        [field: Tooltip("Sprite shown for this action on the matching device.")]
        [field: SerializeField] public Sprite Sprite { get; private set; }

        [field: Tooltip("TextMeshPro sprite name used for inline <sprite> tags. Optional.")]
        [field: SerializeField] public string TmpSpriteName { get; private set; }
    }
}