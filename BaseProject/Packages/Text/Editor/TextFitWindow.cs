using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Base.EditorUIPackage.Editor;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Renders every localized text in every language in its real box and lists what does not fit, one card per
    /// text with a line per language and buttons to fix the box, open it or open the text. Further pages list the
    /// texts with little room left, the boxes no source checks and the largest size each text style could take.
    /// </summary>
    /// <remarks>
    /// While open, the check runs on its own: when the window opens, after a script reload and shortly after a
    /// change to a text, style, prefab or scene, so the list is never older than the project. The automatic check
    /// can be turned off for a slow machine; F5 still runs it.
    /// </remarks>
    internal sealed class TextFitWindow : EditorWindow
    {
        private const string AllLabel = "All";
        private const string AllTooltip = "Shows every language.";
        private const string AutoCheckKey = "Base.TextFit.AutoCheck";
        private const string AutoLabel = "Recheck on save";
        private const string AutoTooltip = "On: the list checks itself again a moment after you save a text, style, "
            + "prefab or scene, so it is never out of date. Off: it only checks when you press F5. Only for you.";
        private const double AutoCheckDelay = 1d;
        private const float ButtonHeight = 28f;
        private const string CheckingText = "Rendering every localized text in every language...";
        private const string CheckLabel = "Check Texts  F5";
        private const string CleanFormat = "Every text fits in all {0} languages";
        private const string Description = "Renders every localized text in every language in its real box.";
        private const string FailedFormat = "{0} {1} not fit, in {2} of {3} languages";
        private const string WorstCaseKey = "Base.TextFit.WorstCase";
        private const string WorstCaseLabel = "Worst case";
        private const string WorstCaseTooltip = "On: checks with the widest readable font, wider spacing and the "
            + "largest text size, the hardest case a player can pick. Text in a part that zooms may "
            + "shrink as far as the zoom grows it back. Off: the designed look. Only for you.";
        private const string FixHint = "Fix the box with Auto Size or Show Box or shorten the text with Edit Text.";
        private const string FooterFormat = "Checked at {0:HH:mm} in {1:0.0} s  ·  {2:N0} texts  ·  {3} languages";
        private const string LocaleChipFormat = "{0}  {1}";
        private const string LocaleChipTooltipFormat = "{0}: {1} texts do not fit. Click to show only this language.";
        private const string MenuPath = "Tools/Base Packages/Assets/Localization/Text Fit";
        private const float MinHeight = 480f;
        private const float MinWidth = 460f;
        private const string NoMatchTitle = "Nothing matches the filter";
        private const string NotRunHint = "The check starts on its own once the editor is idle and out of play "
            + "mode. Press F5 to start it now.";
        private const string NotRunTitle = "Not checked yet";
        private const string OffFooter = "  ·  recheck on save off";
        private const string PluralVerb = "texts do";
        private const string PseudoFooter = " + pseudo";
        private const string SearchPrompt = "Filter by text, key or box";
        private const string SingularVerb = "text does";
        private const string SizesEmptyHint = "Press Suggest Sizes (F6) to try sizes for every text style.";
        private const string SizesEmptyTitle = "No sizes tried yet";
        private const string StatusFormat = "Ready to ship.  {0} tight texts  ·  {1} unchecked boxes";
        private const string SuggestingFormat = "Trying sizes for {0} in {1}...";
        private const string SuggestLabel = "Suggest Sizes  F6";
        private const string TabFormat = "{0}  {1}";
        private const float TabHeight = 24f;
        private const string TightClean = "Every text has room to grow";
        private const string TightHint = "These fit today, but not in the pseudo-language, which makes every text as "
            + "long as our translations get at most. The next language may not fit. A warning, not a must-fix.";
        private const string UncheckedClean = "Every box with words in it is checked";
        private const string WindowTitle = "Text Fit";

        private static readonly string[] TabNames = { "Problems", "Tight", "Unchecked", "Sizes" };

        private static readonly string[] TabTooltips =
        {
            "Texts that do not fit in a real language.",
            "Texts that fit, but have little room left for a longer translation.",
            "Boxes with words in them that the check never renders.",
            "The size of every text style per language and the largest that fits. F6 fills it."
        };

        private readonly TextFitStyles _styles = new();
        private readonly Dictionary<string, List<SizeSuggestion>> _suggestions = new();

        [SerializeField] private string localeFilter;
        [SerializeField] private string search;
        [SerializeField] private bool isShowingIgnored;
        [SerializeField] private ETextFitTab tab;

        private DateTime _checkedAt;
        private bool _isStale;
        private List<TextFitIssue> _issues;
        private TextFitIssueView _issueView;
        private int _languageCount;
        private List<string> _locales = new();
        private List<TextFitGroup> _problems = new();
        private List<TextFitGroup> _pseudo = new();
        private int _sampleCount;
        private int _tightCount;
        private Vector2 _scroll;
        private double _seconds;
        private TextFitSizesView _sizesView;
        private double _staleSince;
        private int _realLanguageCount;
        private List<UncheckedText> _unchecked = new();
        private TextFitUncheckedView _uncheckedView;

        // Saved as empty rather than null, since Unity serializes a null string as an empty one.
        private string PickedLocale => string.IsNullOrEmpty(localeFilter)
            ? null
            : localeFilter;

        private static bool IsAutoCheck
        {
            get => EditorPrefs.GetBool(AutoCheckKey, true);
            set => EditorPrefs.SetBool(AutoCheckKey, value);
        }

        private static bool IsWorstCase
        {
            get => EditorPrefs.GetBool(WorstCaseKey, false);
            set => EditorPrefs.SetBool(WorstCaseKey, value);
        }

#region Unity Callbacks
        // A script reload clears the results, so opening and reloading both start a new check.
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
            wantsMouseMove = true;
            _issueView = new TextFitIssueView(_styles);
            _uncheckedView = new TextFitUncheckedView(_styles);
            _sizesView = new TextFitSizesView(_styles);

            TextFitWatcher.Changed += MarkStale;
            EditorApplication.update += CheckWhenIdle;
            MarkStale();
        }

        private void OnGUI()
        {
            _styles.EnsureBuilt();
            HandleEvents();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorTableStyles.OuterMargin);

                using (new EditorGUILayout.VerticalScope())
                    DrawPage();

                GUILayout.Space(EditorTableStyles.OuterMargin);
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= CheckWhenIdle;
            TextFitWatcher.Changed -= MarkStale;
            _styles.Dispose();
        }
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            TextFitWindow window = GetWindow<TextFitWindow>(WindowTitle);
            window.minSize = new Vector2(MinWidth, MinHeight);
        }

        // Through a SerializedObject, so the change can be undone and the asset is saved like an inspector edit.
        private static void SetSize(TextStyle style, float size)
        {
            SerializedObject serialized = new(style);
            serialized.FindProperty(TextStyle.FontSizeField).floatValue = size;
            serialized.ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(style);
        }

        private void MarkStale()
        {
            _isStale = true;
            _staleSince = EditorApplication.timeSinceStartup;
        }

        // Waits until no change came in for a moment, so saving several assets at once runs one check. Never in play
        // mode or during a compile or import, where the scenes and types it reads are in flux. The first check runs
        // even with the automatic check off, so an open window always has a result.
        private void CheckWhenIdle()
        {
            if (!_isStale
                || (!IsAutoCheck && _issues != null)
                || EditorApplication.isPlayingOrWillChangePlaymode
                || EditorApplication.isCompiling
                || EditorApplication.isUpdating
                || EditorApplication.timeSinceStartup - _staleSince < AutoCheckDelay)
                return;

            Run(isLogged: false, includesAllStyles: false);
        }

        // F5 reruns the check, like a refresh, so the designer can force one without the mouse. F6 sits next to it
        // and adds the slower size suggestions for every style. The pill buttons light up under the mouse, so a
        // move repaints.
        private void HandleEvents()
        {
            Event current = Event.current;

            if (current.type == EventType.MouseMove)
                Repaint();

            if (current.type != EventType.KeyDown)
                return;

            if (current.keyCode == KeyCode.F5)
            {
                current.Use();
                Run(isLogged: true, includesAllStyles: false);
            }
            else if (current.keyCode == KeyCode.F6)
            {
                current.Use();
                Run(isLogged: true, includesAllStyles: true);
            }
        }

        // One validator for the check and the sizes, so every scene and prefab is loaded once. The automatic runs
        // stay out of the console, the window shows their result.
        private void Run(bool isLogged, bool includesAllStyles)
        {
            _isStale = false;
            Stopwatch watch = Stopwatch.StartNew();
            bool isWorstCase = IsWorstCase
                && TextFitWorstCase.TryBegin();

            try
            {
                EditorUtility.DisplayProgressBar(WindowTitle, CheckingText, 0f);

                using TextFitValidator validator = new();
                _issues = validator.Validate();
                _unchecked = validator.Unchecked;
                _sampleCount = validator.SampleCount;

                Suggest(validator, includesAllStyles
                    ? validator.StyleKeys()
                    : KeysToSuggest());
            }
            finally
            {
                if (isWorstCase)
                    TextFitWorstCase.End();

                EditorUtility.ClearProgressBar();
            }

            _seconds = watch.Elapsed.TotalSeconds;
            _checkedAt = DateTime.Now;
            _languageCount = TextFitLocales.All().Count;
            _realLanguageCount = LocalizationEditorSettings.GetLocales().Count;
            _locales = _issues.Where(issue => !issue.IsPseudo).Select(issue => issue.Locale).Distinct().ToList();
            _problems = TextFitGroup.Of(_issues.Where(issue => !issue.IsPseudo));
            _pseudo = TextFitGroup.ByBox(_issues.Where(issue => issue.IsPseudo));
            _tightCount = _pseudo.Sum(group => group.Issues.Count);

            if (!_locales.Contains(localeFilter))
                localeFilter = string.Empty;

            if (includesAllStyles)
                tab = ETextFitTab.Sizes;

            if (!isLogged)
                return;

            TextFitValidator.Report(_issues);

            if (includesAllStyles)
                CustomLogger.Log(string.Join("\n", _suggestions.Values.SelectMany(rows => rows)), this);
        }

        // The styles already shown are measured again, because a changed text can change their sizes too and every
        // style with a text that does not fit joins them.
        private List<string> KeysToSuggest() => _issues
            .Where(issue => !issue.IsPseudo
                && issue.Sample.Box.HasStyle)
            .Select(issue => issue.Sample.Box.StyleKey)
            .Concat(_suggestions.Keys)
            .Distinct()
            .OrderBy(key => key)
            .ToList();

        // The pseudo-language has no styles of its own, it borrows English, so it gets no row to apply sizes on.
        private void Suggest(TextFitValidator validator, List<string> keys)
        {
            _suggestions.Clear();

            List<Locale> locales = TextFitLocales.All()
                .Where(locale => TextFitLocales.CodeOf(locale) != TextFitLocales.PseudoCode)
                .ToList();

            for (int i = 0; i < keys.Count; i++)
            {
                List<SizeSuggestion> rows = new();

                foreach (Locale locale in locales)
                {
                    EditorUtility.DisplayProgressBar(WindowTitle,
                        string.Format(SuggestingFormat, keys[i], locale.LocaleName), (float)i / keys.Count);

                    TextStyle style = TextStyleTable.For(locale, keys[i]);

                    if (style == null)
                        continue;

                    float? fits = validator.SuggestSize(locale, keys[i]);
                    bool isCapped = fits.HasValue
                        && Mathf.Approximately(fits.Value, TextFitValidator.CeilingOf(style));

                    rows.Add(new SizeSuggestion(TextFitLocales.CodeOf(locale), style, fits, isCapped));
                }

                // A style missing in every language has nothing to size and is reported as a problem already.
                if (rows.Count > 0)
                    _suggestions[keys[i]] = rows;
            }
        }

        private void DrawPage()
        {
            EditorWindowChrome.DrawHeader(_styles, WindowTitle, Description);
            DrawActions();

            if (_issues == null)
            {
                EditorWindowChrome.DrawEmptyState(_styles, null, NotRunTitle, NotRunHint);
                return;
            }

            DrawSummary();
            DrawTabs();

            _scroll = EditorGUILayout.BeginScrollView(_scroll, false, false, GUIStyle.none,
                GUI.skin.verticalScrollbar, GUIStyle.none);
            DrawTab();
            EditorGUILayout.EndScrollView();

            string footer = string.Format(FooterFormat, _checkedAt, _seconds, _sampleCount, _realLanguageCount);

            if (_languageCount > _realLanguageCount)
                footer += PseudoFooter;

            EditorWindowChrome.DrawFooter(_styles, IsAutoCheck
                ? footer
                : footer + OffFooter);
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (EditorWindowChrome.PrimaryButton(_styles, CheckLabel, GUILayout.Height(ButtonHeight)))
                    Run(isLogged: true, includesAllStyles: false);

                GUILayout.Space(EditorMetrics.TightGap);

                if (EditorWindowChrome.SecondaryButton(_styles, SuggestLabel, GUILayout.Height(ButtonHeight)))
                    Run(isLogged: true, includesAllStyles: true);

                GUILayout.Space(EditorMetrics.ItemGap);

                using (new EditorGUILayout.VerticalScope(GUILayout.Height(ButtonHeight), GUILayout.ExpandWidth(false)))
                {
                    GUILayout.FlexibleSpace();
                    IsAutoCheck = GUILayout.Toggle(IsAutoCheck, new GUIContent(AutoLabel, AutoTooltip),
                        GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                }

                GUILayout.Space(EditorMetrics.ItemGap);

                using (new EditorGUILayout.VerticalScope(GUILayout.Height(ButtonHeight), GUILayout.ExpandWidth(false)))
                {
                    GUILayout.FlexibleSpace();
                    bool isWorstCase = GUILayout.Toggle(IsWorstCase, new GUIContent(WorstCaseLabel, WorstCaseTooltip),
                        GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();

                    // Another case is another result, so the list checks again.
                    if (isWorstCase != IsWorstCase)
                    {
                        IsWorstCase = isWorstCase;
                        MarkStale();
                    }
                }
            }

            EditorGUILayout.Space(EditorMetrics.SectionGap);
        }

        // The real languages are what ships, so their state leads and every language gets a chip that filters the
        // list down to it.
        private void DrawSummary()
        {
            EditorWindowChrome.BeginCard(_styles);

            int failing = _problems.Count;

            using (new EditorGUILayout.HorizontalScope(GUILayout.Height(EditorMetrics.RowHeight)))
            {
                TextFitWidgets.Icon(failing == 0
                    ? EditorIcons.Success
                    : EditorIcons.Error);
                GUILayout.Space(EditorMetrics.TightGap);

                string verb = failing == 1
                    ? SingularVerb
                    : PluralVerb;
                string headline = failing == 0
                    ? string.Format(CleanFormat, _realLanguageCount)
                    : string.Format(FailedFormat, failing, verb, _locales.Count, _realLanguageCount);

                TextFitWidgets.Label(headline, _styles.SummaryTitle);
            }

            // A problem needs a way out, a clean result only what is left to look at.
            string status = failing == 0
                ? string.Format(StatusFormat, _tightCount, OpenUncheckedCount())
                : FixHint;

            TextFitWidgets.Label(status, _styles.Detail);

            if (_locales.Count > 0)
            {
                EditorGUILayout.Space(EditorMetrics.ItemGap);
                DrawLocaleChips();
            }

            EditorWindowChrome.EndCard();
            EditorGUILayout.Space(EditorMetrics.ItemGap);
        }

        // Wraps onto a new line when the window is too narrow for every chip.
        private void DrawLocaleChips()
        {
            float available = position.width - EditorTableStyles.OuterMargin * 4f;
            float used = 0f;

            EditorGUILayout.BeginHorizontal();

            if (TextFitWidgets.Chip(_styles, AllLabel, AllTooltip, EditorTableStyles.NeutralBadgeColor,
                    PickedLocale == null))
                localeFilter = string.Empty;

            used += TextFitWidgets.ChipWidth(_styles, AllLabel) + EditorMetrics.TightGap;

            foreach (string locale in _locales)
            {
                int count = _problems.Count(group => group.Issues.Any(issue => issue.Locale == locale));
                string label = string.Format(LocaleChipFormat, locale, count);
                float width = TextFitWidgets.ChipWidth(_styles, label) + EditorMetrics.TightGap;

                if (used + width > available)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(EditorMetrics.TightGap);
                    EditorGUILayout.BeginHorizontal();
                    used = 0f;
                }

                GUILayout.Space(EditorMetrics.TightGap);
                used += width;

                bool isSelected = localeFilter == locale;
                string tooltip = string.Format(LocaleChipTooltipFormat, TextFitLocales.NameOf(locale), count);

                if (TextFitWidgets.Chip(_styles, label, tooltip, EditorTableStyles.DangerBadgeColor, isSelected))
                {
                    localeFilter = isSelected
                        ? string.Empty
                        : locale;
                    tab = ETextFitTab.Problems;
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabs()
        {
            int[] counts = { _problems.Count, _tightCount, OpenUncheckedCount(), _suggestions.Count };

            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < TabNames.Length; i++)
                {
                    // An empty count reads as a result, while the sizes are only tried on request.
                    bool isUntried = counts[i] == 0
                        && (ETextFitTab)i == ETextFitTab.Sizes;
                    string label = isUntried
                        ? TabNames[i]
                        : string.Format(TabFormat, TabNames[i], counts[i]);
                    bool isCurrent = (int)tab == i;
                    bool isClicked = isCurrent
                        ? EditorWindowChrome.PrimaryButton(_styles, label, GUILayout.Height(TabHeight))
                        : EditorWindowChrome.SecondaryButton(_styles, label, GUILayout.Height(TabHeight));

                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent(string.Empty, TabTooltips[i]));

                    if (isClicked)
                    {
                        tab = (ETextFitTab)i;
                        _scroll = Vector2.zero;
                    }

                    GUILayout.Space(EditorMetrics.TightGap);
                }
            }

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            Rect field = GUILayoutUtility.GetRect(0f, TabHeight, GUILayout.ExpandWidth(true));
            search = EditorSearchField.Draw(_styles, field, search, SearchPrompt);
            EditorGUILayout.Space(EditorMetrics.ItemGap);
        }

        private void DrawTab()
        {
            switch (tab)
            {
                case ETextFitTab.Problems:
                    DrawIssues(_problems, PickedLocale, EditorTableStyles.DangerBadgeColor, null,
                        string.Format(CleanFormat, _realLanguageCount));
                    break;
                case ETextFitTab.Tight:
                    DrawIssues(_pseudo, null, null, TightHint, TightClean);
                    break;
                case ETextFitTab.Unchecked:
                    DrawUnchecked();
                    break;
                case ETextFitTab.Sizes:
                    DrawSizes();
                    break;
            }
        }

        private void DrawIssues(List<TextFitGroup> groups, string locale, Color? fill, string hint,
            string cleanTitle)
        {
            if (groups.Count == 0)
            {
                DrawEmpty(EditorIcons.Success, cleanTitle, null);
                return;
            }

            if (hint != null)
            {
                GUILayout.Label(hint, _styles.Description);
                EditorGUILayout.Space(EditorMetrics.ItemGap);
            }

            if (_issueView.Draw(groups, locale, search, fill) == 0)
                DrawEmpty(null, NoMatchTitle, null);
        }

        private void DrawUnchecked()
        {
            if (_unchecked.Count == 0)
            {
                DrawEmpty(EditorIcons.Success, UncheckedClean, null);
                return;
            }

            _uncheckedView.Draw(_unchecked, search, ref isShowingIgnored);
        }

        private void DrawSizes()
        {
            if (_suggestions.Count == 0)
            {
                DrawEmpty(null, SizesEmptyTitle, SizesEmptyHint);
                return;
            }

            (List<SizeSuggestion> Rows, float Size)? picked = _sizesView.Draw(_suggestions, PickedLocale,
                search);

            if (picked.HasValue)
                Apply(picked.Value.Rows, picked.Value.Size);
        }

        // The empty state centers itself in the space it is given, which a scroll view has none of.
        private void DrawEmpty(Texture icon, string title, string hint)
        {
            EditorGUILayout.Space(EditorMetrics.SectionGap);
            EditorWindowChrome.DrawEmptyState(_styles, icon, title, hint);
        }

        private int OpenUncheckedCount()
        {
            TextFitIgnoreList ignored = TextFitIgnoreList.instance;

            return _unchecked.Count(text => !ignored.IsIgnored(text.Location));
        }

        // Applied during a click, so the rest of this GUI pass is skipped and the next one draws the new sizes.
        private void Apply(IEnumerable<SizeSuggestion> suggestions, float size)
        {
            foreach (SizeSuggestion suggestion in suggestions)
                SetSize(suggestion.Style, size);

            Run(isLogged: true, includesAllStyles: false);
            GUIUtility.ExitGUI();
        }
    }
}