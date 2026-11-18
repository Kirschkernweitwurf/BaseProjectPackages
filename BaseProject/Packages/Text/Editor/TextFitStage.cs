using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Preview scenes the prefabs and scenes of the check are loaded into, so every text is measured in exactly the
    /// box and with the margins the game uses, without touching the scenes the user has open. Sources mark the
    /// boxes they check through <see cref="Box"/>, so every other box with words in it can be listed as unchecked.
    /// A dynamic font takes every new glyph it renders into its asset, so texts are measured with an in-memory twin
    /// of each dynamic font, built from the same font file. No font asset of the project changes.
    /// </summary>
    public sealed class TextFitStage : IDisposable
    {
        private const string BrokenWordFormat = "'{0}' is wider than the box and breaks inside the word.";
        private const string CopyHolderName = "Scene Copy";
        private const string CutFormat = "Cut with '…' at size {0:0.##}.";
        private const string EmptyBoxFormat = "The box is {0:0.##} × {1:0.##}, its layout leaves no room for text. "
            + "Give it a size, like a Layout Element height.";
        private const string LinesFormat = "Needs {0} lines, {1} allowed.";
        // World space texts measure in meters, so only a box with no room at all is empty.
        private const float MinBoxSide = 0.001f;
        private const string MissingGlyphsFormat = "The font has no glyph for: {0}";
        private const char PathSeparator = '/';
        private const float Percent = 100f;
        private const string TooTallFormat = "Too tall by {2:0}% at size {3:0.##}: needs {0:0.#}, the box has {1:0.#}.";
        private const string TooWideFormat = "Too wide by {2:0}% at size {3:0.##}: needs {0:0.#}, the box has {1:0.#}.";
        private const string UiRootName = "UI Root";

        // Screen space canvases get their size from the screen, which a preview scene has none of. The game is laid
        // out for this resolution, so boxes are measured at it.
        private static readonly Vector2 ReferenceResolution = new(1920f, 1080f);

        private readonly Dictionary<TMP_Text, TextBox> _boxes = new();
        private readonly HashSet<(string Parent, string Name, string Text)> _claimedLooks = new();
        private readonly HashSet<Object> _claimedSources = new();
        private readonly Scene _prefabScene;
        private readonly Dictionary<GameObject, Object> _roots = new();
        private readonly List<Scene> _scenes = new();
        private readonly Dictionary<TMP_FontAsset, TMP_FontAsset> _twins = new();
        private readonly RectTransform _uiRoot;

        /// <summary>Creates an empty stage.</summary>
        internal TextFitStage()
        {
            _prefabScene = EditorSceneManager.NewPreviewScene();
            _scenes.Add(_prefabScene);

            _uiRoot = (RectTransform)Create(UiRootName, typeof(RectTransform), typeof(Canvas)).transform;
            _uiRoot.sizeDelta = ReferenceResolution;
        }

        /// <summary>
        /// Puts an instance of <paramref name="prefab"/> on the stage, laid out like in the game. A UI prefab without
        /// a canvas of its own goes under a screen-sized one, like it would in a scene.
        /// </summary>
        public GameObject Instantiate(GameObject prefab)
        {
            bool isLooseUi = prefab.TryGetComponent(out RectTransform _)
                && !prefab.TryGetComponent(out Canvas _);
            GameObject instance = isLooseUi
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefab, _uiRoot)
                : (GameObject)PrefabUtility.InstantiatePrefab(prefab, _prefabScene);

            _roots[instance] = prefab;
            Layout(isLooseUi
                ? _uiRoot.gameObject
                : instance);

            return instance;
        }

        /// <summary>
        /// Puts an instance of <paramref name="prefab"/> under <paramref name="parent"/>, which is on the stage
        /// already, like an entry the game adds to a list. The list's layout sizes it, as it would in the game.
        /// </summary>
        public GameObject Instantiate(GameObject prefab, Transform parent)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);

            _roots[instance] = prefab;
            Layout(TopRootOf(parent).gameObject);

            return instance;
        }

        /// <summary>
        /// Puts the objects of the scene at <paramref name="path"/> on the stage, laid out like in the game and
        /// returns them. A scene the user has open is copied instead of opened a second time, so nothing the check
        /// does can reach the open one.
        /// </summary>
        public GameObject[] Open(string path)
        {
            Scene open = SceneManager.GetSceneByPath(path);
            GameObject[] roots = open.isLoaded
                ? Copy(open.GetRootGameObjects())
                : OpenPreview(path);
            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

            foreach (GameObject root in roots)
            {
                _roots[root] = asset;
                Layout(root);
            }

            return roots;
        }

        /// <summary>
        /// Every <typeparamref name="T"/> on the stage, also on inactive objects. A list, so a source can put more
        /// on the stage while it goes through them.
        /// </summary>
        public List<T> FindAll<T>() where T : Component => FindAll(typeof(T)).Cast<T>().ToList();

        /// <summary>Every component of <paramref name="type"/> on the stage, also on inactive objects.</summary>
        public List<Component> FindAll(Type type) => _roots.Keys
            .Where(root => !HasRootAbove(root.transform))
            .SelectMany(root => root.GetComponentsInChildren(type, true))
            .ToList();

        /// <summary>Marks <paramref name="text"/> as checked and returns its box.</summary>
        /// <param name="text">The text, on the stage.</param>
        /// <param name="prepare">
        /// Runs after every new text, before the layout, for a box that code sizes to its text. Null for none.
        /// </param>
        public TextBox Box(TMP_Text text, Action<TMP_Text> prepare = null)
        {
            if (_boxes.TryGetValue(text, out TextBox box))
                return box;

            box = new TextBox(text, LocationOf(text.transform), prepare);
            _boxes[text] = box;
            _claimedSources.Add(SourceOf(text));
            _claimedLooks.Add(LookOf(text));

            return box;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Scenes first: a box whose twin is gone would render its last text again with the default font and fill
            // the real fallbacks.
            foreach (Scene scene in _scenes)
                EditorSceneManager.ClosePreviewScene(scene);

            foreach (TMP_FontAsset twin in _twins.Values)
                Destroy(twin);
        }

        /// <summary>
        /// Renders <paramref name="sample"/> and returns why it does not fit or null when it does. With a
        /// <paramref name="style"/> the box takes it at <paramref name="fontSize"/>; without one it keeps its own font
        /// and size. <paramref name="checksWords"/> adds the check for words that break inside, which only makes
        /// sense for languages that put spaces between words.
        /// </summary>
        internal string Check(TextSample sample, TextStyle style, float fontSize, bool checksWords)
        {
            List<GameObject> woken = WakeLayout(sample.Box.Text.rectTransform);

            try
            {
                return RenderWithTwins(sample, style, fontSize, checksWords);
            }
            finally
            {
                foreach (GameObject sleeper in woken)
                    sleeper.SetActive(false);
            }
        }

        /// <summary>
        /// Every box with words in it that no source marked through <see cref="Box"/>, once each. Call it before
        /// the check renders, while the boxes still hold the text of their prefab or scene.
        /// </summary>
        internal List<UncheckedText> Unclaimed()
        {
            List<UncheckedText> unclaimed = new();
            HashSet<Object> seenSources = new();
            HashSet<(string, string, string)> seenLooks = new();

            foreach (TMP_Text text in FindAll<TMP_Text>())
            {
                Object source = SourceOf(text);
                (string, string, string) look = LookOf(text);

                if (_boxes.ContainsKey(text)
                    || _claimedSources.Contains(source)
                    || _claimedLooks.Contains(look)
                    || string.IsNullOrEmpty(text.text)
                    || !text.text.Any(char.IsLetter)
                    || !seenSources.Add(source)
                    || !seenLooks.Add(look))
                    continue;

                unclaimed.Add(new UncheckedText(LocationOf(text.transform), text.text));
            }

            return unclaimed;
        }

        private static void Destroy(TMP_FontAsset twin)
        {
            foreach (Texture2D atlas in twin.atlasTextures)
                Object.DestroyImmediate(atlas);

            Object.DestroyImmediate(twin.material);
            Object.DestroyImmediate(twin);
        }

        // Layout groups and fitters only run with a canvas update, which a preview scene never gets on its own. A
        // rebuild only walks down while it meets layout components, so every outermost one is rebuilt itself, outer
        // ones first, since they size the ones inside.
        private static void Layout(GameObject root)
        {
            foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                if (IsScreenRoot(canvas))
                    ((RectTransform)canvas.transform).sizeDelta = ReferenceResolution;
            }

            foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (IsLayoutRoot(rect))
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }

        private static bool IsLayoutRoot(RectTransform rect) => rect.TryGetComponent(out ILayoutController _)
            && !(rect.parent is RectTransform parent
                && parent.TryGetComponent(out ILayoutController _));

        // Read from the render at the size the text settled on, so a one-line text that runs too wide counts too.
        // The axis that runs further past its box, by share, says what to fix.
        private static string Overflow(TMP_Text text, Vector2 box)
        {
            Vector2 spill = TextOverflow.Measure(text);
            float tolerance = TextOverflow.ToleranceOf(text);
            bool isWide = spill.x > tolerance;
            bool isTall = spill.y > tolerance;

            if (!isWide
                && !isTall)
                return null;

            float wide = spill.x / box.x;
            float tall = spill.y / box.y;

            return isWide && (!isTall || wide >= tall)
                ? string.Format(TooWideFormat, box.x + spill.x, box.x, wide * Percent, text.fontSize)
                : string.Format(TooTallFormat, box.y + spill.y, box.y, tall * Percent, text.fontSize);
        }

        // The inner size, without margins.
        private static Vector2 InnerSize(TMP_Text text)
        {
            Rect rect = text.rectTransform.rect;
            Vector4 margin = text.margin;

            return new Vector2(rect.width - margin.x - margin.z, rect.height - margin.y - margin.w);
        }

        // A box inside layout groups or size fitters takes its size from the text, so its layout runs again from the
        // outermost of them after every new text.
        private static RectTransform LayoutRoot(RectTransform box)
        {
            RectTransform root = box;

            for (Transform parent = box.parent; parent is RectTransform rect; parent = parent.parent)
            {
                if (!rect.TryGetComponent(out ILayoutController _))
                    break;

                root = rect;
            }

            return root;
        }

        // A hidden page was never laid out, so it still has its saved size, often none. Its size comes from the
        // layout group around it and a plain holder in a group, like the one around a scroll view, from the group
        // above that, so the page is laid out once from the outermost group that sizes it.
        private static void LayOutWoken(RectTransform page)
        {
            RectTransform root = page;

            for (Transform parent = page.parent; parent is RectTransform rect; parent = parent.parent)
            {
                if (!rect.TryGetComponent(out ILayoutController _)
                    && !IsSizedByGroup(rect))
                    break;

                root = rect;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(root);

            // A rebuild stops at a holder without a group of its own, so the page's own groups run after it has
            // its size.
            LayoutRebuilder.ForceRebuildLayoutImmediate(page);
        }

        private static bool IsSizedByGroup(RectTransform rect) => rect.parent != null
            && rect.parent.TryGetComponent(out LayoutGroup _)
            && !(rect.TryGetComponent(out ILayoutIgnorer ignorer) && ignorer.ignoreLayout);

        // Layout skips inactive objects, so a box under a hidden parent, like the prompt of a hidden page, would keep
        // the size it was saved with. Only its own inactive parents wake and only while it renders: waking whole
        // scenes made the check take minutes, since every Localize String Event loads its table. A box hidden itself,
        // like a card's description, is shown by code that also arranges its neighbors, so it keeps its saved size.
        private static List<GameObject> WakeLayout(RectTransform box)
        {
            List<GameObject> woken = new();

            if (!box.gameObject.activeSelf
                || box.gameObject.activeInHierarchy
                || !LayoutRoot(box).TryGetComponent(out ILayoutController _))
                return woken;

            for (Transform current = box.parent; current != null; current = current.parent)
            {
                if (!current.gameObject.activeSelf)
                    woken.Add(current.gameObject);
            }

            foreach (GameObject sleeper in woken)
                sleeper.SetActive(true);

            if (woken.Count > 0)
                LayOutWoken((RectTransform)woken[^1].transform);

            return woken;
        }

        private static bool IsScreenRoot(Canvas canvas)
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
                return false;

            Transform parent = canvas.transform.parent;

            return parent == null
                || parent.GetComponentInParent<Canvas>(true) == null;
        }

        // Read from the render itself, so an auto-sized text that shrinks to keep its words whole passes. TMP keeps a
        // hyphen or a soft hyphen inside a word, but a line may end there.
        private static string BrokenWord(TMP_TextInfo info) => BrokenWords.TryFind(info, out TMP_WordInfo word)
            ? word.GetWord()
            : null;

        // The same box loaded twice, through a prefab and through a scene that holds it, shares its original.
        private static Object SourceOf(TMP_Text text)
        {
            TMP_Text original = PrefabUtility.GetCorrespondingObjectFromOriginalSource(text);

            return original == null
                ? text
                : original;
        }

        // A copy of an open scene has lost its link to its prefabs, so its boxes are matched by how they look.
        private static (string Parent, string Name, string Text) LookOf(TMP_Text text)
        {
            Transform parent = text.transform.parent;
            string parentName = parent == null
                ? string.Empty
                : parent.name;

            return (parentName, text.name, text.text);
        }

        private static string PathOf(Transform transform, Transform above)
        {
            List<string> names = new();

            for (Transform current = transform; current != null && current != above; current = current.parent)
                names.Add(current.name);

            names.Reverse();

            return string.Join(PathSeparator, names);
        }

        private string RenderWithTwins(TextSample sample, TextStyle style, float fontSize, bool checksWords)
        {
            List<TMP_FontAsset> fallbacks = TMP_Settings.fallbackFontAssets;

            if (fallbacks == null)
                return Render(sample, style, fontSize, checksWords);

            // The project-wide fallbacks are swapped for their twins only while one text renders, so not even an
            // error leaves them changed.
            List<TMP_FontAsset> saved = new(fallbacks);
            fallbacks.Clear();
            fallbacks.AddRange(saved.Select(TwinOf));

            try
            {
                return Render(sample, style, fontSize, checksWords);
            }
            finally
            {
                fallbacks.Clear();
                fallbacks.AddRange(saved);
            }
        }

        // The box keeps its style's size, but measures with the twin of its font.
        private string Render(TextSample sample, TextStyle style, float fontSize, bool checksWords)
        {
            TMP_Text text = sample.Box.Text;
            bool isEnlarged = false;

            if (style != null)
            {
                StyleNeighbors(text.rectTransform);
                sample.Box.Styled.Apply(style);
                isEnlarged = ReadableText.IsEnlarged
                    || text.font != style.Font;

                // A readable font the player can pick scales the style's size; the size being tried scales with it.
                float scale = text.fontSize / style.FontSize;
                text.fontSize = fontSize * scale;

                // A box that shrinks starts from the size being tried and keeps the floor its style gave it, so a size
                // suggestion means the same and a frame keeps its readable minimum.
                if (text.enableAutoSizing)
                    TextStyle.SetAutoSizeRange(text, text.fontSizeMin, fontSize * scale);
            }

            text.font = TwinOf(text.font);
            string missing = MissingGlyphs(text.font, sample.Text);

            if (missing.Length > 0)
                return string.Format(MissingGlyphsFormat, missing);

            // The game hyphenates such a box from its Awake, which never runs on the stage.
            text.textPreprocessor = sample.Box.IsHyphenated
                ? TextHyphenator.Shared
                : null;

            sample.Arrange?.Invoke();
            text.text = sample.Text;
            sample.Box.Prepare?.Invoke(text);
            LayoutRebuilder.ForceRebuildLayoutImmediate(LayoutRoot(text.rectTransform));
            text.ForceMeshUpdate(true, true);

            Vector2 box = InnerSize(text);

            if (box.x < MinBoxSide
                || box.y < MinBoxSide)
                return string.Format(EmptyBoxFormat, box.x, box.y);

            string overflow = Overflow(text, box);

            if (overflow != null)
                return overflow;

            // A cut text hides words from the player. Only a box made to show a short look may cut.
            if (text.isTextTruncated
                && sample.Box.HasStyle
                && !sample.Box.CanCutLongText)
                return string.Format(CutFormat, text.fontSize);

            // The line limit is a rule for writing and translating texts, so only the designed look holds a text to
            // it. A font, spacing or size the player picked may add lines: a panel grows with them, a frame shrinks its
            // text into its box and a one-line frame never wraps at all.
            if (style != null
                && !isEnlarged
                && text.textInfo.lineCount > style.MaxLines)
                return string.Format(LinesFormat, text.textInfo.lineCount, style.MaxLines);

            if (!checksWords)
                return null;

            string word = BrokenWord(text.textInfo);

            return word == null
                ? null
                : string.Format(BrokenWordFormat, word);
        }

        // In the game every box styles itself, but on the stage only the measured one gets a style. Its neighbors get
        // theirs once, so a layout that follows another text, like a tooltip panel widening with its body, sizes as in
        // the game.
        private static void StyleNeighbors(RectTransform box)
        {
            Locale locale = TextLocale.Current;

            if (locale == null)
                return;

            foreach (LocalizedTextBox neighbor in LayoutRoot(box).GetComponentsInChildren<LocalizedTextBox>(true))
            {
                if (neighbor.AppliedStyle != null)
                    continue;

                TextStyle style = TextStyleTable.For(locale, neighbor);

                if (style != null)
                    neighbor.Apply(style);
            }
        }

        // A box from a prefab points at the prefab it was placed from, where its size is set. One that only exists in
        // a scene points at the scene.
        private TextBoxLocation LocationOf(Transform box)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(box.gameObject);

            if (source != null)
            {
                Object prefab = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(source));
                return new TextBoxLocation(prefab, PathOf(source.transform, null));
            }

            Transform root = box;

            while (root.parent != null
                && !_roots.ContainsKey(root.gameObject))
                root = root.parent;

            Object asset = _roots.TryGetValue(root.gameObject, out Object loaded)
                ? loaded
                : null;

            return new TextBoxLocation(asset, PathOf(box, root.parent));
        }

        private bool HasRootAbove(Transform transform)
        {
            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
            {
                if (_roots.ContainsKey(parent.gameObject))
                    return true;
            }

            return false;
        }

        private Transform TopRootOf(Transform transform)
        {
            Transform top = transform;

            for (Transform current = transform; current != null; current = current.parent)
            {
                if (_roots.ContainsKey(current.gameObject))
                    top = current;
            }

            return top;
        }

        // Made like Unity's own preview objects: a plain new object lands in the open scene first and marks it as
        // changed, one that is never saved does not.
        private GameObject Create(string objectName, params Type[] components)
        {
            GameObject created = EditorUtility.CreateGameObjectWithHideFlags(objectName, HideFlags.HideAndDontSave,
                components);
            SceneManager.MoveGameObjectToScene(created, _prefabScene);

            return created;
        }

        // Copied straight under a holder on the stage: a plain copy would land in the open scene first and mark it
        // as changed.
        private GameObject[] Copy(GameObject[] originals)
        {
            GameObject holder = Create(CopyHolderName);
            GameObject[] copies = new GameObject[originals.Length];

            for (int i = 0; i < originals.Length; i++)
            {
                copies[i] = Object.Instantiate(originals[i], holder.transform);
                copies[i].name = originals[i].name;
            }

            return copies;
        }

        private GameObject[] OpenPreview(string path)
        {
            Scene scene = EditorSceneManager.OpenPreviewScene(path);
            _scenes.Add(scene);

            return scene.GetRootGameObjects();
        }

        // Fallback fonts count, because the game draws a character from them when the main font lacks it. A static
        // font cannot take new characters and logs a warning each time it is asked to, so it is only asked what it
        // has.
        private static string MissingGlyphs(TMP_FontAsset font, string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            if (font.atlasPopulationMode == AtlasPopulationMode.Dynamic
                && font.TryAddCharacters(content, out string _))
                return string.Empty;

            string missing = string.Empty;

            foreach (char character in content.Distinct())
            {
                if (!char.IsWhiteSpace(character)
                    && !font.HasCharacter(character, true, true))
                    missing += character;
            }

            return missing;
        }

        // Built from the same font file at the same size, with the same line metrics, so it measures like the
        // original. Static fonts never change and are used as they are. The twin is listed before its fallbacks are,
        // so a font that falls back to itself ends the chain.
        private TMP_FontAsset TwinOf(TMP_FontAsset font)
        {
            if (font == null
                || font.atlasPopulationMode == AtlasPopulationMode.Static
                || font.sourceFontFile == null
                || _twins.ContainsValue(font))
                return font;

            if (_twins.TryGetValue(font, out TMP_FontAsset twin))
                return twin;

            twin = TMP_FontAsset.CreateFontAsset(font.sourceFontFile, (int)font.faceInfo.pointSize, font.atlasPadding,
                font.atlasRenderMode, font.atlasWidth, font.atlasHeight, AtlasPopulationMode.Dynamic,
                font.isMultiAtlasTexturesEnabled);
            twin.hideFlags = HideFlags.HideAndDontSave;
            twin.name = font.name;
            twin.faceInfo = font.faceInfo;
            _twins[font] = twin;

            twin.fallbackFontAssetTable = font.fallbackFontAssetTable.Select(TwinOf).ToList();
            CopyStyles(font, twin);

            return twin;
        }

        // Bold, italic and tabs read these and <b> draws with the font's real bold face through its weight table.
        // Without them the twin fakes bold by stretching the letters, which measures wider than the game draws.
        private void CopyStyles(TMP_FontAsset font, TMP_FontAsset twin)
        {
            twin.normalStyle = font.normalStyle;
            twin.normalSpacingOffset = font.normalSpacingOffset;
            twin.boldStyle = font.boldStyle;
            twin.boldSpacing = font.boldSpacing;
            twin.italicStyle = font.italicStyle;
            twin.tabSize = font.tabSize;

            TMP_FontWeightPair[] weights = font.fontWeightTable;
            TMP_FontWeightPair[] twinWeights = twin.fontWeightTable;
            int count = Mathf.Min(weights.Length, twinWeights.Length);

            for (int i = 0; i < count; i++)
            {
                twinWeights[i].regularTypeface = TwinOf(weights[i].regularTypeface);
                twinWeights[i].italicTypeface = TwinOf(weights[i].italicTypeface);
            }
        }
    }
}