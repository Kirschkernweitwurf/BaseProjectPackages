#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Builds full menu paths from group names and the per kind root prefix.</summary>
    public static class MenuPath
    {
        /// <summary>Fixed root that asset creation entries are always placed under.</summary>
        public const string AssetRoot = "Assets/Create";

        /// <summary>Returns the automatic root prefix for a kind, or an empty string.</summary>
        public static string Prefix(EMenuEntryKind kind) => kind == EMenuEntryKind.CreateAsset
            ? AssetRoot
            : string.Empty;

        /// <summary>Joins segments into a menu path, skipping empty parts and trimming stray slashes.</summary>
        public static string Combine(IEnumerable<string> segments)
        {
            StringBuilder builder = new();

            foreach (string segment in segments)
            {
                if (string.IsNullOrWhiteSpace(segment))
                    continue;

                string trimmed = segment.Trim().Trim('/');

                if (trimmed.Length == 0)
                    continue;

                if (builder.Length > 0)
                    builder.Append('/');

                builder.Append(trimmed);
            }

            return builder.ToString();
        }
    }
}
#endif