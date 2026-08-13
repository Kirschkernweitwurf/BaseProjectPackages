using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A box of text next to a field.</summary>
    [AttributeSample(typeof(InfoBoxAttribute), EAttributeCategory.Layout,
        Description = "Puts a box of text beside the field, for saying something the field name cannot. Use it for the "
            + "note you would otherwise leave in a comment nobody editing the component ever sees.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "A second argument picks the styling: Info, Warning, Error or None.",
            "A third argument moves the box below the field instead of above it.",
            "A fourth argument makes it compact, which fits it on a single line.",
            "An EColor overload colors the box instead of using one of the standard types.",
            "A text starting with a dollar names a member to read, so the box can comment on the values around it."
        })]
    internal sealed class InfoBoxSample : ScriptableObject
    {
        [InfoBox("The default: an info box above its field.")]
        public string standard = "Below a default box";

        [InfoBox("Warning styling, for a setup that works but probably should not.", EInfoBoxType.Warning)]
        public string warning = "Below a warning box";

        [InfoBox("Error styling, for a setup that is genuinely broken.", EInfoBoxType.Error)]
        public string error = "Below an error box";

        [InfoBox("Drawn below its field instead of above it.", EInfoBoxType.Info, EInfoBoxPosition.Below)]
        public string below = "Above its own box";

        [InfoBox("Compact, so it takes a single line.", EInfoBoxType.Info, EInfoBoxPosition.Above, true)]
        public string compact = "Below a compact box";

        [InfoBox("Compact and a warning, for a caveat that does not need a paragraph.",
            EInfoBoxType.Warning, EInfoBoxPosition.Above, true)]
        public string compactWarning = "Below a compact warning";

        [InfoBox("Compact, below and an error, which is the shape a failed check wants: one line, under "
            + "the field it is about.", EInfoBoxType.Error, EInfoBoxPosition.Below, true)]
        public string compactBelow = "Above a compact error";
    }
}