using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Troubleshoot
{
    /// <summary>
    /// Cached styles, colors and icons for the findings list. Pure presentation.
    /// </summary>
    /// <remarks>
    /// Rebuilt when the editor skin changes, since every color here is picked for one skin and the light
    /// and dark versions are not interchangeable.
    /// </remarks>
    internal sealed class AttributeTroubleshootStyles
    {
        private const string ErrorIcon = "console.erroricon.sml";
        private const int SummaryFontSize = 12;
        private const string ScriptIcon = "cs Script Icon";
        private const string SuccessIcon = "TestPassed";
        private const int TitleFontSize = 15;
        private const string WarningIcon = "console.warnicon.sml";

        private static readonly Color DarkHeader = new(1f, 1f, 1f, 0.06f);
        private static readonly Color ErrorColor = new(0.86f, 0.33f, 0.33f);
        private static readonly Color LightHeader = new(0f, 0f, 0f, 0.05f);
        private static readonly Color SubtitleColor = new(0.5f, 0.5f, 0.5f);
        private static readonly Color SuccessColor = new(0.36f, 0.76f, 0.46f);
        private static readonly Color WarningColor = new(0.90f, 0.68f, 0.24f);

        private static Texture _errorTexture;
        private static Texture _scriptTexture;
        private static Texture _successTexture;
        private static Texture _warningTexture;

        /// <summary>Subtle background behind a group header.</summary>
        internal static Color Header => EditorGUIUtility.isProSkin
            ? DarkHeader
            : LightHeader;

        /// <summary>The bar marking a finding that stops an attribute from working.</summary>
        internal static Color Error => ErrorColor;

        /// <summary>The bar marking a finding that only changes behavior.</summary>
        internal static Color Warning => WarningColor;

        /// <summary>Red icon shown next to an error finding.</summary>
        internal static Texture ErrorTexture => Resolve(ref _errorTexture, ErrorIcon);

        /// <summary>Yellow icon shown next to a warning finding.</summary>
        internal static Texture WarningTexture => Resolve(ref _warningTexture, WarningIcon);

        /// <summary>Script icon shown in a group header.</summary>
        internal static Texture ScriptTexture => Resolve(ref _scriptTexture, ScriptIcon);

        /// <summary>Green icon shown in the empty state.</summary>
        internal static Texture SuccessTexture => Resolve(ref _successTexture, SuccessIcon);

        /// <summary>Tint of a row the pointer is over.</summary>
        internal Color Hover { get; private set; }

        /// <summary>Tint of every other finding, so a long group reads as rows.</summary>
        internal Color Stripe { get; private set; }

        /// <summary>The count shown at the right of a group header.</summary>
        internal GUIStyle Count { get; private set; }

        /// <summary>Label for the member and attribute of a single finding.</summary>
        internal GUIStyle Member { get; private set; }

        /// <summary>Muted label for the explanation of a single finding.</summary>
        internal GUIStyle Message { get; private set; }

        /// <summary>Bold label for the type name in a group header.</summary>
        internal GUIStyle Name { get; private set; }

        /// <summary>Bold label used in the summary row under the action bar.</summary>
        internal GUIStyle Summary { get; private set; }

        /// <summary>Muted subtitle shown under the success title.</summary>
        internal GUIStyle SuccessSubtitle { get; private set; }

        /// <summary>Large green title shown when nothing is wrong.</summary>
        internal GUIStyle SuccessTitle { get; private set; }

        private bool _built;
        private bool _builtForProSkin;

        /// <summary>Builds the styles once, and again after the editor skin changed.</summary>
        internal void EnsureBuilt()
        {
            if (_built && _builtForProSkin == EditorGUIUtility.isProSkin)
                return;

            Build();

            _built = true;
            _builtForProSkin = EditorGUIUtility.isProSkin;
        }

        // Icons are only available inside a GUI callback, so they are resolved on first use.
        private static Texture Resolve(ref Texture cached, string iconName)
        {
            if (cached == null)
                cached = EditorGUIUtility.IconContent(iconName).image;

            return cached;
        }

        private static Color Tint(float strength) => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, strength)
            : new Color(0f, 0f, 0f, strength);

        private void Build()
        {
            Name = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };

            Count = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                normal =
                {
                    textColor = SubtitleColor
                }
            };

            Member = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft
            };

            Message = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                normal =
                {
                    textColor = SubtitleColor
                },
                hover =
                {
                    textColor = SubtitleColor
                }
            };

            Summary = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = SummaryFontSize
            };

            SuccessTitle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = TitleFontSize,
                normal =
                {
                    textColor = SuccessColor
                },
                hover =
                {
                    textColor = SuccessColor
                }
            };

            SuccessSubtitle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal =
                {
                    textColor = SubtitleColor
                },
                hover =
                {
                    textColor = SubtitleColor
                }
            };

            Hover = Tint(0.06f);
            Stripe = Tint(0.025f);
        }
    }
}