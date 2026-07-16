using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Lets the user flag components during play mode and snapshots them the moment play mode starts to exit,
    /// while the play mode scene is still alive. Nothing is written here, capture only fills the pending list.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeCapturer
    {
        private const string ComponentForgetPath = "CONTEXT/Component/Forget Play Mode Changes";
        private const string ComponentMarkPath = "CONTEXT/Component/Save Play Mode Changes";
        private const int GameObjectMarkPriority = 20;
        private const string GameObjectMarkPath = "GameObject/Save Play Mode Changes";

        static PlayModeCapturer()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /// <summary>Flags a component for saving. Marking the same component twice is a no-op.</summary>
        public static void Mark(Object target)
        {
            if (target == null)
                return;

            Component component = target as Component;
            if (component == null)
            {
                Debug.LogError(
                    $"Play Mode Saver tracks components, not '{target.name}'. Mark a component on it instead.",
                    target);
                return;
            }

            PlayModeMarks.Add(component);
        }

        /// <summary>Removes the flag from a component.</summary>
        public static void Forget(Object target) => PlayModeMarks.Remove(target as Component);

        /// <summary>Builds a readable label such as "P_Garden_Bush01(Clone) (Transform)".</summary>
        public static string BuildDisplayName(Object target)
        {
            Component component = target as Component;
            if (component != null)
                return $"{component.gameObject.name} ({component.GetType().Name})";

            return target.name;
        }

        [MenuItem(ComponentMarkPath, false)]
        private static void MarkComponent(MenuCommand command) => Mark(command.context);

        [MenuItem(ComponentMarkPath, true)]
        private static bool IsMarkComponentValid(MenuCommand command)
        {
            if (!EditorApplication.isPlaying)
                return false;

            return command.context != null
                && !PlayModeMarks.HasComponent(command.context as Component);
        }

        [MenuItem(ComponentForgetPath, false)]
        private static void ForgetComponent(MenuCommand command) => Forget(command.context);

        [MenuItem(ComponentForgetPath, true)]
        private static bool IsForgetComponentValid(MenuCommand command)
        {
            if (!EditorApplication.isPlaying)
                return false;

            return command.context != null
                && PlayModeMarks.HasComponent(command.context as Component);
        }

        [MenuItem(GameObjectMarkPath, false, GameObjectMarkPriority)]
        private static void MarkSelectedGameObjects()
        {
            foreach (GameObject selected in Selection.gameObjects)
            {
                foreach (Component component in selected.GetComponents<Component>())
                {
                    if (component == null)
                        continue;

                    Mark(component);
                }
            }
        }

        [MenuItem(GameObjectMarkPath, true)]
        private static bool IsMarkSelectedGameObjectsValid()
        {
            if (!EditorApplication.isPlaying)
                return false;

            return Selection.gameObjects.Length > 0;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                DiscardPending();
                return;
            }

            if (change == PlayModeStateChange.ExitingPlayMode)
                Capture();
        }

        private static void DiscardPending()
        {
            PlayModeStateStore store = PlayModeStateStore.instance;
            if (store.Payloads.Count == 0
                && store.History.Count == 0)
                return;

            store.ClearPayloads();
            store.ClearHistory();
            store.Persist();
        }

        private static void Capture()
        {
            PlayModeMarks.Prune();
            List<PlayModeSavePayload> payloads = new();

            foreach (Component component in PlayModeMarks.Components)
            {
                payloads.Add(BuildPayload(component));
            }

            PlayModeStateStore store = PlayModeStateStore.instance;
            store.SetPayloads(payloads);
            store.Persist();
            PlayModeMarks.Clear();

            foreach (PlayModeSavePayload payload in payloads)
            {
                PlayModeHistory.Record(EPlayModeHistoryAction.Captured, payload.displayName,
                    payload.applyTarget.ToString());
            }
        }

        private static PlayModeSavePayload BuildPayload(Component component)
        {
            Transform cloneRoot = PlayModePrefabResolver.FindCloneRoot(component.transform);
            Transform prefabRoot = cloneRoot ?? component.transform.root;

            return new PlayModeSavePayload
            {
                displayName = BuildDisplayName(component),
                applyTarget = cloneRoot != null
                    ? EPlayModeApplyTarget.PrefabAsset
                    : EPlayModeApplyTarget.SceneInstance,
                json = PlayModeSerializationUtility.CaptureJson(component),
                objectReferences = PlayModeSerializationUtility.CaptureObjectReferences(component, cloneRoot),
                componentTypeName = component.GetType().FullName,
                componentIndex = PlayModeObjectLocator.FindComponentIndex(component),
                scenePath = component.gameObject.scene.path,
                sceneNamePath = PlayModeObjectLocator.BuildSceneNamePath(component.transform),
                sceneIndexPath = PlayModeObjectLocator.BuildSceneIndexPath(component.transform),
                sourcePrefabGuid = PlayModePrefabResolver.FindPrefabGuid(prefabRoot),
                prefabNamePath = PlayModeObjectLocator.BuildRelativeNamePath(prefabRoot, component.transform),
                prefabIndexPath = PlayModeObjectLocator.BuildRelativeIndexPath(prefabRoot, component.transform)
            };
        }
    }
}
