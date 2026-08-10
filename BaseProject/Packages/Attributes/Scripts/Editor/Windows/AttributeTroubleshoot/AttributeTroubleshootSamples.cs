using System;
using System.Collections.Generic;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot
{
    /// <summary>
    /// Fabricated findings covering every check and both severities. A healthy project produces an empty
    /// window, which is the right result and a useless preview, so this builds what a broken project
    /// would look like. The groups carry no real type, so nothing here can be confused with a scan.
    /// </summary>
    public static class AttributeTroubleshootSamples
    {
        /// <summary>Builds the sample groups.</summary>
        /// <param name="errors">Number of sample findings that stop an attribute from working.</param>
        /// <param name="warnings">Number of sample findings that only change behavior.</param>
        /// <returns>The fabricated groups.</returns>
        public static List<AttributeIssueGroup> Build(out int errors, out int warnings)
        {
            List<AttributeIssueGroup> groups = new()
            {
                new AttributeIssueGroup("PlayerHealth", new List<AttributeIssue>
                {
                    Error("regenBar", typeof(ShowIfAttribute),
                        "'_isRegenerating' does not exist on PlayerHealth. The condition always evaluates to true."),
                    Error("maxHealth", typeof(OnValueChangedAttribute),
                        "'OnMaxHealthChanged' does not exist on PlayerHealth. The callback never fires.")
                }),
                new AttributeIssueGroup("WeaponConfig", new List<AttributeIssue>
                {
                    Error("fireRate", typeof(RequiredAttribute),
                        "Single is not supported. Use [Required] with an object reference."),
                    Error("ammoType", typeof(DropdownAttribute),
                        "'AmmoTypes' is not an enumerable of options. The plain field is drawn instead."),
                    Error("damageRange", typeof(MinMaxSliderAttribute),
                        "Vector3 is not supported. Use [MinMaxSlider] with a Vector2.")
                }),
                new AttributeIssueGroup("EnemySpawner", new List<AttributeIssue>
                {
                    Error("spawnRoot", typeof(GetComponentAttribute),
                        "GameObject is not a component type, so the lookup never returns anything. "
                        + "Use Transform instead."),
                    Error("idleParam", typeof(AnimatorParamAttribute),
                        "'animatorRef' is not a Animator field. The plain field is drawn instead."),
                    Error("SpawnWave", typeof(HeaderButtonAttribute),
                        "The method takes parameters, so no button is drawn.")
                }),
                new AttributeIssueGroup("AbilityDefinition", new List<AttributeIssue>
                {
                    Warning("ability", typeof(ReferencePickerAttribute),
                        "No instantiable type implements IAbility, so the picker stays empty. "
                        + "Candidates need a public parameterless constructor."),
                    Warning("cooldown", typeof(HideIfAttribute),
                        "No condition members given, so the attribute never applies.")
                })
            };

            errors = 0;
            warnings = 0;

            foreach (AttributeIssueGroup group in groups)
            {
                errors += group.ErrorCount;
                warnings += group.Issues.Count - group.ErrorCount;
            }

            return groups;
        }

        private static AttributeIssue Error(string member, Type attributeType, string message)
            => Build(member, attributeType, message, EAttributeIssueSeverity.Error);

        private static AttributeIssue Warning(string member, Type attributeType, string message)
            => Build(member, attributeType, message, EAttributeIssueSeverity.Warning);

        private static AttributeIssue Build(string member, Type attributeType, string message,
            EAttributeIssueSeverity severity)
            => new(member, AttributeNames.Display(attributeType), message, severity);
    }
}
