using System.Collections.Generic;
using UnityEngine;

namespace Base.SettingsPackage.Display
{
    /// <summary>
    /// Enumerates the display resolutions reported by Unity as stable "{width}x{height}" labels,
    /// and maps between those labels and the active resolution.
    /// </summary>
    public static class ResolutionProvider
    {
        private const char Separator = 'x';

        /// <summary>Returns the distinct available resolutions as labels, ordered from highest to lowest.</summary>
        public static IReadOnlyList<string> GetAvailableResolutions()
        {
            Resolution[] available = Screen.resolutions;
            List<string> resolutions = new(available.Length);
            HashSet<string> seen = new(available.Length);

            for (int i = available.Length - 1; i >= 0; i--)
            {
                string label = Format(available[i].width, available[i].height);
                if (seen.Add(label))
                    resolutions.Add(label);
            }

            return resolutions;
        }

        /// <summary>Returns the label for the resolution currently presented by the screen.</summary>
        public static string GetCurrentResolutionLabel() => Screen.fullScreenMode == FullScreenMode.Windowed
            ? Format(Screen.width, Screen.height)
            : Format(Screen.currentResolution.width, Screen.currentResolution.height);

        /// <summary>Parses a resolution label into its width and height. Returns false on a malformed label.</summary>
        public static bool TryParse(string label, out int width, out int height)
        {
            width = 0;
            height = 0;

            if (string.IsNullOrWhiteSpace(label))
                return false;

            string[] parts = label.Split(Separator);
            return parts.Length == 2
                && int.TryParse(parts[0].Trim(), out width)
                && int.TryParse(parts[1].Trim(), out height);
        }

        /// <summary>Formats a width and height into the canonical resolution label.</summary>
        public static string Format(int width, int height) => $"{width}{Separator}{height}";
    }
}
