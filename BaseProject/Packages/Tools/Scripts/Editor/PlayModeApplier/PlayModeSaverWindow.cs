using Base.ToolPackage.MenuManagerWindow;
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

        private static readonly Color AppliedColor = new(0.4f, 0.85f, 0.45f);
        private static readonly Color CapturedColor = new(0.55f, 0.75f, 1f);
        private static readonly Color DiscardedColor = new(0.65f, 0.65f, 0.65f);
        private static readonly Color FailedColor = new(1f, 0.45f, 0.4f);
        private static readonly Color RowColor = new(0f, 0f, 0f, 0.1f);

        [SerializeField]
        private Vector2 scrollPosition;

#region Unity Callbacks
        private void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        private void OnGUI()
        {
            PlayModeStateStore store = PlayModeStateStore.instance;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawMarks();
            EditorGUILayout.Space();
            DrawPayloads(store);
            EditorGUILayout.Space();
            DrawHistory(store);
            EditorGUILayout.EndScrollView();
        }

        private void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
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
            EditorGUILayout.LabelField($"Marked ({PlayModeMarks.Components.Count})", EditorStyles.boldLabel);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter play mode, then right click a component header and choose Save Play Mode Changes.",
                    MessageType.Info);

                return;
            }

            if (PlayModeMarks.Components.Count == 0)
            {
                EditorGUILayout.LabelField("Nothing marked.", EditorStyles.centeredGreyMiniLabel);
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

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(PlayModeCapturer.BuildDisplayName(component));

            if (GUILayout.Button("x", GUILayout.Width(RemoveButtonWidth)))
                PlayModeMarks.RemoveAt(index);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPayloads(PlayModeStateStore store)
        {
            EditorGUILayout.LabelField($"Captured ({store.Payloads.Count})", EditorStyles.boldLabel);

            if (store.Payloads.Count == 0)
            {
                EditorGUILayout.LabelField("Nothing captured.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            for (int index = store.Payloads.Count - 1; index >= 0; index--)
                DrawPayloadRow(store, index);

            EditorGUILayout.Space();
            DrawBulkActions(store);
        }

        private void DrawPayloadRow(PlayModeStateStore store, int index)
        {
            PlayModeSavePayload payload = store.Payloads[index];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(payload.displayName);
            DrawApplyTarget(store, payload, index);
            DrawPrefabField(store, payload, index);

            using (new EditorGUI.DisabledScope(!PlayModeApplier.CanApply(payload)))
            {
                if (GUILayout.Button("Apply", GUILayout.Width(ActionButtonWidth)))
                    ApplySingle(store, index);
            }

            if (GUILayout.Button("Discard", GUILayout.Width(ActionButtonWidth)))
            {
                PlayModeHistory.Record(EPlayModeHistoryAction.Discarded, payload.displayName,
                    payload.applyTarget.ToString());

                store.RemovePayload(index);
                store.Persist();
            }

            EditorGUILayout.EndHorizontal();
            DrawPayloadWarning(payload);
            EditorGUILayout.EndVertical();
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
                    EditorGUILayout.HelpBox("Pick the destination prefab.", MessageType.Warning);

                return;
            }

            if (!PlayModeApplier.CanApply(payload))
                EditorGUILayout.HelpBox($"Open '{payload.scenePath}' to apply this.", MessageType.Warning);
        }

        private void DrawBulkActions(PlayModeStateStore store)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply All"))
                ApplyAll(store);

            if (GUILayout.Button("Discard All"))
                DiscardAll(store);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawHistory(PlayModeStateStore store)
        {
            EditorGUILayout.LabelField($"History ({store.History.Count})", EditorStyles.boldLabel);

            if (store.History.Count == 0)
            {
                EditorGUILayout.LabelField("Nothing yet.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            for (int index = store.History.Count - 1; index >= 0; index--)
                DrawHistoryRow(store.History[index], index);

            EditorGUILayout.Space();

            if (GUILayout.Button("Clear History"))
            {
                store.ClearHistory();
                store.Persist();
            }
        }

        private void DrawHistoryRow(PlayModeHistoryEntry entry, int index)
        {
            Rect row = EditorGUILayout.BeginHorizontal();

            if (Event.current.type == EventType.Repaint
                && index % 2 == 0)
                EditorGUI.DrawRect(row, RowColor);

            EditorGUILayout.LabelField(entry.timestamp, EditorStyles.miniLabel, GUILayout.Width(TimestampWidth));

            Color previousColor = GUI.contentColor;
            GUI.contentColor = GetActionColor(entry.action);
            EditorGUILayout.LabelField(entry.action.ToString(), EditorStyles.miniBoldLabel,
                GUILayout.Width(ActionLabelWidth));

            GUI.contentColor = previousColor;
            EditorGUILayout.LabelField(entry.displayName, EditorStyles.miniLabel);
            EditorGUILayout.LabelField(entry.detail, EditorStyles.miniLabel, GUILayout.Width(DetailWidth));
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