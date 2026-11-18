using Base.AttributesPackage;
using Base.UtilityPackage.Menus;
using TMPro;
using UnityEngine;

namespace Base.TextPackage
{
    /// <summary>
    /// How one kind of text box looks in one language: font, material, size and the limits the text has to stay
    /// within. Font and size live together so a language switch changes both at once. The size is a design value:
    /// locked by default, so a translation that does not fit gets rewritten. A style with an auto size minimum lets
    /// a long text shrink down to it instead, for UI that was laid out that way.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Text/New Text Style", "TS_TextStyle")]
    public sealed class TextStyle : ScriptableObject
    {
        /// <summary>
        /// The serialized name of the size, for editor tools that change it through a SerializedObject so undo and
        /// saving work like an edit in the inspector.
        /// </summary>
        public const string FontSizeField = nameof(fontSize);

        // World-space labels measure their text in meters, so sizes go far below 1.
        private const float SmallestFontSize = 0.01f;

        [Tooltip("The font this language reads in.")]
        [Required] [SerializeField] private TMP_FontAsset font;

        [Tooltip("The material of the font. Empty uses the font's own default material.")]
        [SerializeField] private Material material;

        [Min(SmallestFontSize)]
        [Tooltip("The font size. Every box with this style uses it or shrinks from it when auto size is on. "
            + "World-space labels use meters, so their sizes are far below 1.")]
        [SerializeField] private float fontSize = 6f;

        [Min(0f)]
        [Tooltip("Lets a long text shrink down to this size instead of overflowing. 0 keeps the size locked.")]
        [SerializeField] private float autoSizeMin;

        [Tooltip("Extra space between lines, in TextMeshPro units.")]
        [SerializeField] private float lineSpacing;

        [Min(1)]
        [Tooltip("The most lines a text with this style may take.")]
        [SerializeField] private int maxLines = 1;

        [Min(SmallestFontSize)]
        [Tooltip("The smallest size still readable. The text fit check fails a style below it and a frame never "
            + "shrinks below it.")]
        [SerializeField] private float minReadableSize = 5f;

        [Tooltip("Cut a text that is still too long with '…' as the very last resort, like a name. Off lets it "
            + "overflow, so the text fit check reports it instead.")]
        [SerializeField] private bool canCutWithEllipsis;

        [Tooltip("How this kind of text makes room when the player's font, spacing or text size make it bigger. Panel: "
            + "the box grows with its text (rows, tooltips, dialogs). Frame: the box keeps its size (cards, buttons, "
            + "badges, table labels), so the text shrinks back into it, down to Min Readable Size.")]
        [SerializeField] private ETextRole role;

        /// <summary>The font this language reads in.</summary>
        public TMP_FontAsset Font => font;

        /// <summary>The material to draw with: the set one or the font's own default.</summary>
        public Material Material => material == null
            ? font.material
            : material;

        /// <summary>The font size and the largest size of a style that shrinks.</summary>
        public float FontSize => fontSize;

        /// <summary>The size a long text may shrink down to or 0 when the size is locked.</summary>
        public float AutoSizeMin => autoSizeMin;

        /// <summary>True when a long text may shrink down to <see cref="AutoSizeMin"/>.</summary>
        public bool IsAutoSized => autoSizeMin > 0f;

        /// <summary>The smallest size a text with this style is ever shown at.</summary>
        public float SmallestSize => IsAutoSized
            ? autoSizeMin
            : fontSize;

        /// <summary>Extra space between lines.</summary>
        public float LineSpacing => lineSpacing;

        /// <summary>The most lines a text with this style may take.</summary>
        public int MaxLines => maxLines;

        /// <summary>The smallest size still readable.</summary>
        public float MinReadableSize => minReadableSize;

        /// <summary>True when a text that is still too long gets cut with '…' instead of overflowing.</summary>
        public bool CanCutWithEllipsis => canCutWithEllipsis;

        /// <summary>True for a box that keeps its size, so text the player made bigger shrinks back into it.</summary>
        public bool IsFrame => role == ETextRole.Frame;

        /// <summary>
        /// Lets <paramref name="text"/> shrink from <paramref name="max"/> down to <paramref name="min"/>. A minimum
        /// above the maximum is lowered to it, since TextMeshPro never settles on a size between the two otherwise
        /// and keeps the frame busy for good.
        /// </summary>
        public static void SetAutoSizeRange(TMP_Text text, float min, float max)
        {
            text.fontSizeMax = max;
            text.fontSizeMin = Mathf.Min(min, max);
        }
    }
}