using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.OverviewGui.PrefabOverviewWindow
{
    /// <summary>
    /// Ready made labels for one row of the prefab overview. Built once per scan so that drawing does not
    /// format strings or look up icons on every repaint.
    /// </summary>
    internal sealed class PrefabRowContent
    {
        private const string FallbackIconName = "Prefab Icon";
        private const string IssueBadgeText = "!";
        private const string VariantBadgeTooltip = "Variants derived from this prefab";

        /// <summary>Icon of the prefab asset.</summary>
        internal GUIContent Icon { get; }

        /// <summary>Name of the prefab, with the asset details as tooltip.</summary>
        internal GUIContent Label { get; }

        /// <summary>Number of variants below the prefab, or null when it has none.</summary>
        internal GUIContent VariantBadge { get; }

        /// <summary>Number of overrides against the base, or null when there is nothing to show.</summary>
        internal GUIContent OverrideBadge { get; }

        /// <summary>Marker for the issues of the prefab, or null when it has none.</summary>
        internal GUIContent IssueBadge { get; }

        /// <summary>Color role of the override badge. Only meaningful together with that badge.</summary>
        internal EOverviewAccent OverrideAccent { get; }

        /// <summary>Builds the labels of one row.</summary>
        /// <param name="entry">Entry the row stands for.</param>
        /// <param name="overridesAnalyzed">True when the scan counted overrides.</param>
        public PrefabRowContent(PrefabEntry entry, bool overridesAnalyzed)
        {
            Icon = BuildIcon(entry);
            Label = new GUIContent(entry.Name, BuildTooltip(entry));

            if (entry.TotalVariants > 0)
                VariantBadge = new GUIContent(entry.TotalVariants.ToString(), VariantBadgeTooltip);

            if (entry.Kind == EPrefabKind.Variant
                && overridesAnalyzed)
            {
                OverrideBadge = new GUIContent(entry.Overrides.Total.ToString(), BuildOverrideTooltip(entry));
                OverrideAccent = ResolveOverrideAccent(entry);
            }

            if (entry.Issues != EPrefabIssue.None)
                IssueBadge = new GUIContent(IssueBadgeText, DescribeIssues(entry.Issues));
        }

        private static GUIContent BuildIcon(PrefabEntry entry)
        {
            Texture icon = AssetDatabase.GetCachedIcon(entry.AssetPath);

            if (icon == null)
                return EditorGUIUtility.IconContent(FallbackIconName);

            return new GUIContent(icon);
        }

        private static string BuildTooltip(PrefabEntry entry)
        {
            string text = $"{entry.AssetPath}\n{entry.Kind}, {entry.GameObjectCount} GameObjects, "
                + $"{entry.ComponentCount} components";

            if (entry.BaseEntry != null)
                text += $"\nBase: {entry.BaseEntry.Name}";

            if (entry.TotalVariants > 0)
                text += $"\nVariants below: {entry.TotalVariants}";

            return text;
        }

        private static string BuildOverrideTooltip(PrefabEntry entry)
        {
            PrefabOverrideCounts counts = entry.Overrides;

            return $"{counts.Total} overrides against the base\n"
                + $"{counts.ModifiedProperties} modified properties\n"
                + $"{counts.AddedComponents} added components\n"
                + $"{counts.RemovedComponents} removed components\n"
                + $"{counts.AddedGameObjects} added GameObjects";
        }

        private static EOverviewAccent ResolveOverrideAccent(PrefabEntry entry)
            => (entry.Issues & (EPrefabIssue.HeavyOverrides | EPrefabIssue.RedundantVariant)) != 0
                ? EOverviewAccent.Warning
                : EOverviewAccent.Neutral;

        private static string DescribeIssues(EPrefabIssue issues)
        {
            List<string> parts = new();

            if ((issues & EPrefabIssue.RedundantVariant) != 0)
                parts.Add("Variant without any override");

            if ((issues & EPrefabIssue.HeavyOverrides) != 0)
                parts.Add("Variant overrides most of its base");

            if ((issues & EPrefabIssue.DeepChain) != 0)
                parts.Add("Variant sits deep in a variant chain");

            if ((issues & EPrefabIssue.MissingBase) != 0)
                parts.Add("Base prefab could not be resolved");

            return string.Join("\n", parts);
        }
    }
}