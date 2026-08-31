using Base.EditorUiPackage;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// The control surface for the package. During play mode it lists what is marked, in edit mode it lists
    /// what was captured and lets the user apply or discard each entry. The captured list is cleared
    /// automatically when the next play session starts.
    /// </summary>
    public class PlayModeSaverWindow : EditorWindow
    {
        private const float ActionButtonWidth = 60f;
        private const string ApplyAllLabel = "Apply All";
        private const string ApplyLabel = "Apply";
        private const string CapturedHeaderFormat = "Captured ({0})";
        private const string ClearHistoryLabel = "Clear History";
        private const string Description = "Keeps the changes you make to a component while the game runs. "
            + "Mark a component in play mode, then apply what it captured back to the scene or the prefab "
            + "once you stop.";
        private const string DiscardAllLabel = "Discard All";
        private const string DiscardLabel = "Discard";
        private const string EditModeHint = "Enter play mode, then right click a component header and "
            + "choose Save Play Mode Changes.";
        private const string HistoryHeaderFormat = "History ({0})";
        private const string MarkedHeaderFormat = "Marked ({0})";
        private const string NothingCapturedText = "Nothing captured.";
        private const string NothingMarkedText = "Nothing marked.";
        private const string NothingYetText = "Nothing yet.";
        private const string PickPrefabWarning = "Pick the destination prefab.";
        private const string RemoveLabel = "x";
        private const string SceneWarningFormat = "Open '{0}' to apply this.";
        private const float ActionLabelWidth = 66f;
        private const float DetailWidth = 100f;
        private const float MinimumWindowHeight = 360f;
        private const float MinimumWindowWidth = 520f;
        private const float PrefabFieldWidth = 150f;
        private const float RemoveButtonWidth = 22f;
        private const float TargetFieldWidth = 110f;
        private const float TimestampWidth = 58f;
        private const string WindowMenuPath = "Tools/Base Packages/Unity Editor/Play Mode Saver";
        private const string WindowTitle = "Play Mode Saver";

        [SerializeField]
        private Vector2 scrollPosition;

        private readonly EditorWindowStyles _styles = new();

        // What each action means is the same everywhere in the Base windows, so the four colors come
        // from the palette rather than being picked again here.
        private static Color AppliedColor => EditorPalette.Success;

        private static Color CapturedColor => EditorPalette.Accent;

        private static Color DiscardedColor => EditorPalette.DimText;

        private static Color FailedColor => EditorPalette.Danger;

#region Unity Callbacks
        private void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        private void OnGUI()
        {
            _styles.EnsureBuilt();

            PlayModeStateStore store = PlayModeStateStore.instance;

            EditorWindowChrome.DrawHeader(_styles, WindowTitle, Description);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawMarks();
            EditorGUILayout.Space(EditorMetrics.SectionGap);
            DrawPayloads(store);
            EditorGUILayout.Space(EditorMetrics.SectionGap);
            DrawHistory(store);

            EditorGUILayout.EndScrollView();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            _styles.Dispose();
        }
#endregion

        [DynamicMenuItem(WindowMenuPath)]
        private static void Open()
        {
            PlayModeSaverWindow window = GetWindow<PlayModeSaverWindow>(WindowTitle);
            window.minSize = new Vector2(MinimumWindowWidth, MinimumWindowHeight);
            window.Show();
        }

        private static Color GetActionColor(EPlayModeHistoryAction action) => action switch
        {
            EPlayModeHistoryAction.Applied => AppliedColor,
            EPlayModeHistoryAction.Discarded => DiscardedColor,
            EPlayModeHistoryAction.Failed => FailedColor,
            _ => CapturedColor
        };

        private void OnPlayModeStateChanged(PlayModeStateChange change) => Repaint();

        private void DrawMarks()
        {
            EditorWindowChrome.DrawSectionHeader(_styles,
                string.Format(MarkedHeaderFormat, PlayModeMarks.Components.Count));

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(EditModeHint, MessageType.Info);
                return;
            }

            if (PlayModeMarks.Components.Count == 0)
            {
                GUILayout.Label(NothingMarkedText, _styles.EmptyHint);
                return;
            }

            for (int index = PlayModeMarks.Components.Count - 1; index >= 0; index--)
                DrawMarkRow(index);
        }

        private void DrawMarkRow(int index)
        {
            Component component = PlayModeMarks.Components[index];
            if (component == null)
                return;

            Rect row = EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorTableStyles.RowHeight));

            EditorRows.DrawRowBackground(row, index);

            GUILayout.Label(PlayModeCapturer.BuildDisplayName(component), _styles.Name);
            GUILayout.FlexibleSpace();

            if (EditorWindowChrome.SecondaryButton(_styles, RemoveLabel,
                    GUILayout.Width(RemoveButtonWidth)))
                PlayModeMarks.RemoveAt(index);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPayloads(PlayModeStateStore store)
        {
            EditorWindowChrome.DrawSectionHeader(_styles,
                string.Format(CapturedHeaderFormat, store.Payloads.Count));

            if (store.Payloads.Count == 0)
            {
                GUILayout.Label(NothingCapturedText, _styles.EmptyHint);
                return;
            }

            for (int index = store.Payloads.Count - 1; index >= 0; index--)
                DrawPayloadRow(store, index);

            EditorGUILayout.Space(EditorMetrics.ItemGap);
            DrawBulkActions(store);
        }

        private void DrawPayloadRow(PlayModeStateStore store, int index)
        {
            PlayModeSavePayload payload = store.Payloads[index];

            EditorWindowChrome.BeginCard(_styles);
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(payload.displayName, _styles.Name);
            DrawApplyTarget(store, payload, index);
            DrawPrefabField(store, payload, index);

            using (new EditorGUI.DisabledScope(!PlayModeApplier.CanApply(payload)))
            {
                if (EditorWindowChrome.PrimaryButton(_styles, ApplyLabel,
                        GUILayout.Width(ActionButtonWidth)))
                    ApplySingle(store, index);
            }

            GUILayout.Space(EditorMetrics.TightGap);

            if (EditorWindowChrome.SecondaryButton(_styles, DiscardLabel,
                    GUILayout.Width(ActionButtonWidth)))
            {
                PlayModeHistory.Record(EPlayModeHistoryAction.Discarded, payload.displayName,
                    payload.applyTarget.ToString());

                store.RemovePayload(index);
                store.Persist();
            }

            EditorGUILayout.EndHorizontal();

            DrawPayloadWarning(payload);

            EditorWindowChrome.EndCard();
        }

        private void DrawApplyTarget(PlayModeStateStore store, PlayModeSavePayload payload, int index)
        {
            EPlayModeApplyTarget applyTarget = (EPlayModeApplyTarget)EditorGUILayout.EnumPopup(
                payload.applyTarget, GUILayout.Width(TargetFieldWidth));

            if (applyTarget == payload.applyTarget)
                return;

            store.SetPayloadApplyTarget(index, applyTarget);
            store.Persist();
        }

        private void DrawPrefabField(PlayModeStateStore store, PlayModeSavePayload payload, int index)
        {
            if (payload.applyTarget != EPlayModeApplyTarget.PrefabAsset)
                return;

            string assetPath = AssetDatabase.GUIDToAssetPath(payload.sourcePrefabGuid);
            GameObject current = string.IsNullOrEmpty(assetPath)
                ? null
                : AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            GameObject picked = (GameObject)EditorGUILayout.ObjectField(current, typeof(GameObject), false,
                GUILayout.Width(PrefabFieldWidth));

            if (picked == current)
                return;

            string pickedPath = picked != null
                ? AssetDatabase.GetAssetPath(picked)
                : string.Empty;

            store.SetPayloadPrefab(index, AssetDatabase.AssetPathToGUID(pickedPath));
            store.Persist();
        }

        private void DrawPayloadWarning(PlayModeSavePayload payload)
        {
            if (payload.applyTarget == EPlayModeApplyTarget.PrefabAsset)
            {
                if (string.IsNullOrEmpty(payload.sourcePrefabGuid))
                    EditorGUILayout.HelpBox(PickPrefabWarning, MessageType.Warning);

                return;
            }

            if (!PlayModeApplier.CanApply(payload))
                EditorGUILayout.HelpBox(string.Format(SceneWarningFormat, payload.scenePath),
                    MessageType.Warning);
        }

        private void DrawBulkActions(PlayModeStateStore store)
        {
            EditorGUILayout.BeginHorizontal();

            if (EditorWindowChrome.PrimaryButton(_styles, ApplyAllLabel))
                ApplyAll(store);

            GUILayout.Space(EditorMetrics.TightGap);

            if (EditorWindowChrome.SecondaryButton(_styles, DiscardAllLabel))
                DiscardAll(store);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawHistory(PlayModeStateStore store)
        {
            EditorWindowChrome.DrawSectionHeader(_styles,
                string.Format(HistoryHeaderFormat, store.History.Count));

            if (store.History.Count == 0)
            {
                GUILayout.Label(NothingYetText, _styles.EmptyHint);
                return;
            }

            for (int index = store.History.Count - 1; index >= 0; index--)
                DrawHistoryRow(store.History[index], index);

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            if (EditorWindowChrome.SecondaryButton(_styles, ClearHistoryLabel))
            {
                store.ClearHistory();
                store.Persist();
            }
        }

        private void DrawHistoryRow(PlayModeHistoryEntry entry, int index)
        {
            Rect row = EditorGUILayout.BeginHorizontal();

            EditorRows.DrawRowBackground(row, index);

            GUILayout.Label(entry.timestamp, _styles.Detail, GUILayout.Width(TimestampWidth));

            Color previousColor = GUI.contentColor;

            GUI.contentColor = GetActionColor(entry.action);
            GUILayout.Label(entry.action.ToString(), _styles.Badge, GUILayout.Width(ActionLabelWidth));
            GUI.contentColor = previousColor;

            GUILayout.Label(entry.displayName, _styles.Detail);
            GUILayout.Label(entry.detail, _styles.Detail, GUILayout.Width(DetailWidth));

            EditorGUILayout.EndHorizontal();
        }

        private void ApplySingle(PlayModeStateStore store, int index)
        {
            if (!TryApplyPayload(store, index))
                return;

            store.Persist();
            AssetDatabase.SaveAssets();
        }

        private void ApplyAll(PlayModeStateStore store)
        {
            for (int index = store.Payloads.Count - 1; index >= 0; index--)
            {
                if (!PlayModeApplier.CanApply(store.Payloads[index]))
                    continue;

                TryApplyPayload(store, index);
            }

            store.Persist();
            AssetDatabase.SaveAssets();
        }

        private void DiscardAll(PlayModeStateStore store)
        {
            foreach (PlayModeSavePayload payload in store.Payloads)
            {
                PlayModeHistory.Record(EPlayModeHistoryAction.Discarded, payload.displayName,
                    payload.applyTarget.ToString());
            }

            store.ClearPayloads();
            store.Persist();
        }

        private bool TryApplyPayload(PlayModeStateStore store, int index)
        {
            PlayModeSavePayload payload = store.Payloads[index];
            string displayName = payload.displayName;
            string detail = payload.applyTarget.ToString();

            if (!PlayModeApplier.TryApply(payload))
            {
                PlayModeHistory.Record(EPlayModeHistoryAction.Failed, displayName, detail);
                return false;
            }

            store.RemovePayload(index);
            PlayModeHistory.Record(EPlayModeHistoryAction.Applied, displayName, detail);
            return true;
        }
    }
}