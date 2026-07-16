using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.ToolPackage.Editor.ComponentClipboard
{
    /// <summary>
    /// Undo aware operations used by the <see cref="ComponentClipboardWindow"/>.
    /// All methods are safe to call with empty or null input.
    /// </summary>
    public static class ComponentOperations
    {
        private const int MinimumDuplicateCount = 2;
        private const string MoveUndoName = "Reorder Components";
        private const string PasteUndoName = "Paste Components";
        private const int SingleEntry = 1;

        /// <summary>Returns true when the component can be captured into the clipboard.</summary>
        /// <param name="component">Component to test. May be null.</param>
        public static bool CanCopy(Component component) => component != null && !(component is Transform);

        /// <summary>
        /// Works out what a paste would do, one step per entry and in entry order. A single entry of
        /// a type overwrites every overwrite target of that type. Several entries of the same type
        /// are paired with the targets one by one, extra entries add new components. Entries without
        /// a matching target add a new component. Nothing is changed here, so this is safe to call
        /// from OnGUI.
        /// </summary>
        /// <param name="target">GameObject receiving the components.</param>
        /// <param name="entries">Snapshots to paste, in order.</param>
        /// <param name="overwriteTargets">
        /// Components that may be overwritten, usually the selection. Candidates not sitting on
        /// <paramref name="target"/> are ignored. Pass null or an empty list to always add new.
        /// </param>
        /// <returns>One step per entry, empty when there is nothing to paste.</returns>
        public static List<ComponentPasteStep> BuildPastePlan(GameObject target,
            IReadOnlyList<ComponentClipboardEntry> entries, IReadOnlyList<Component> overwriteTargets)
        {
            List<ComponentPasteStep> plan = new();

            if (target == null || entries == null)
                return plan;

            Dictionary<Type, List<Component>> candidates = BuildCandidatesByType(target, overwriteTargets);
            Dictionary<Type, int> entryCounts = CountEntriesByType(entries);
            Dictionary<Type, int> consumed = new();

            foreach (ComponentClipboardEntry entry in entries)
            {
                Type type = ResolvePastableType(entry);
                plan.Add(new ComponentPasteStep(entry, type, TakeTargets(candidates, entryCounts, consumed, type)));
            }

            return plan;
        }

        /// <summary>Runs a paste against the given overwrite targets.</summary>
        /// <param name="target">GameObject receiving the components.</param>
        /// <param name="entries">Snapshots to paste, applied in order.</param>
        /// <param name="overwriteTargets">Components that may be overwritten, usually the selection.</param>
        public static void Paste(GameObject target, IReadOnlyList<ComponentClipboardEntry> entries,
            IReadOnlyList<Component> overwriteTargets)
            => Execute(target, BuildPastePlan(target, entries, overwriteTargets), true);

        /// <summary>
        /// Pastes without a selection, matching each entry against the components already present on
        /// the target by type. Used when pasting onto GameObjects other than the inspected one, where
        /// no per component selection exists.
        /// </summary>
        /// <param name="target">GameObject receiving the components.</param>
        /// <param name="entries">Snapshots to paste, applied in order.</param>
        public static void PasteMatchingByType(GameObject target, IReadOnlyList<ComponentClipboardEntry> entries)
        {
            if (target == null)
                return;

            Paste(target, entries, target.GetComponents<Component>());
        }

        /// <summary>Always adds new components, even when the target already has that type.</summary>
        /// <param name="target">GameObject receiving the components.</param>
        /// <param name="entries">Snapshots to paste, applied in order.</param>
        public static void PasteAsNew(GameObject target, IReadOnlyList<ComponentClipboardEntry> entries)
            => Paste(target, entries, null);

        /// <summary>
        /// Overwrites the given targets only. Entries without a matching target are skipped instead
        /// of being added as new components.
        /// </summary>
        /// <param name="target">GameObject whose components are overwritten.</param>
        /// <param name="entries">Snapshots to apply.</param>
        /// <param name="overwriteTargets">Components that may be overwritten, usually the selection.</param>
        public static void PasteValues(GameObject target, IReadOnlyList<ComponentClipboardEntry> entries,
            IReadOnlyList<Component> overwriteTargets)
            => Execute(target, BuildPastePlan(target, entries, overwriteTargets), false);

        /// <summary>Destroys the given components, skipping any that are required by others.</summary>
        /// <param name="components">Components to destroy.</param>
        public static void Delete(IReadOnlyList<Component> components)
        {
            if (components == null)
                return;

            for (int index = components.Count - 1; index >= 0; index--)
            {
                Component component = components[index];

                if (!CanDelete(component))
                {
                    Debug.LogWarning($"Component Clipboard: '{component?.GetType().Name}' cannot be removed.");
                    continue;
                }

                Undo.DestroyObjectImmediate(component);
            }
        }

        /// <summary>Moves the given components one slot up in the inspector order.</summary>
        /// <param name="components">Components to move, expected in inspector order.</param>
        public static void MoveUp(IReadOnlyList<Component> components)
        {
            if (components == null)
                return;

            foreach (Component component in components)
                Move(component, true);
        }

        /// <summary>Moves the given components one slot down in the inspector order.</summary>
        /// <param name="components">Components to move, expected in inspector order.</param>
        public static void MoveDown(IReadOnlyList<Component> components)
        {
            if (components == null)
                return;

            for (int index = components.Count - 1; index >= 0; index--)
                Move(components[index], false);
        }

        /// <summary>
        /// Returns true when the component can be destroyed. Transforms and components that another
        /// component requires via <see cref="RequireComponent"/> cannot be removed.
        /// </summary>
        /// <param name="component">Component to test. May be null.</param>
        private static bool CanDelete(Component component)
        {
            if (component == null)
                return false;

            if (component is Transform)
                return false;

            Type type = component.GetType();

            if (component.gameObject.GetComponents(type).Length >= MinimumDuplicateCount)
                return true;

            Component[] siblings = component.gameObject.GetComponents<Component>();

            foreach (Component sibling in siblings)
            {
                if (sibling == null || sibling == component)
                    continue;

                if (IsRequiredBy(type, sibling.GetType()))
                    return false;
            }

            return true;
        }

        private static void Execute(GameObject target, List<ComponentPasteStep> plan, bool canAddNew)
        {
            if (target == null)
                return;

            foreach (ComponentPasteStep step in plan)
            {
                if (!step.IsValid)
                {
                    Debug.LogError($"Component Clipboard: type '{step.Entry.TypeName}' could not be resolved.");
                    continue;
                }

                if (step.IsOverwrite)
                {
                    Overwrite(step);
                    continue;
                }

                if (!canAddNew)
                    continue;

                Component created = Undo.AddComponent(target, step.Type);

                if (created == null)
                    continue;

                Apply(step.Entry, created);
            }
        }

        private static void Overwrite(ComponentPasteStep step)
        {
            foreach (Component existing in step.Targets)
            {
                if (existing == null)
                    continue;

                Undo.RecordObject(existing, PasteUndoName);
                Apply(step.Entry, existing);
            }
        }

        private static Dictionary<Type, List<Component>> BuildCandidatesByType(GameObject target,
            IReadOnlyList<Component> candidates)
        {
            Dictionary<Type, List<Component>> byType = new();

            if (candidates == null)
                return byType;

            foreach (Component candidate in candidates)
            {
                if (candidate == null || candidate is Transform || candidate.gameObject != target)
                    continue;

                Type type = candidate.GetType();

                if (!byType.TryGetValue(type, out List<Component> bucket))
                {
                    bucket = new List<Component>();
                    byType[type] = bucket;
                }

                bucket.Add(candidate);
            }

            return byType;
        }

        private static Dictionary<Type, int> CountEntriesByType(IReadOnlyList<ComponentClipboardEntry> entries)
        {
            Dictionary<Type, int> counts = new();

            foreach (ComponentClipboardEntry entry in entries)
            {
                Type type = ResolvePastableType(entry);

                if (type == null)
                    continue;

                counts.TryGetValue(type, out int count);
                counts[type] = count + 1;
            }

            return counts;
        }

        private static List<Component> TakeTargets(Dictionary<Type, List<Component>> candidates,
            Dictionary<Type, int> entryCounts, Dictionary<Type, int> consumed, Type type)
        {
            List<Component> targets = new();

            if (type == null || !candidates.TryGetValue(type, out List<Component> available))
                return targets;

            entryCounts.TryGetValue(type, out int entryCount);

            if (entryCount <= SingleEntry)
            {
                targets.AddRange(available);
                return targets;
            }

            consumed.TryGetValue(type, out int index);

            if (index >= available.Count)
                return targets;

            targets.Add(available[index]);
            consumed[type] = index + 1;
            return targets;
        }

        private static Type ResolvePastableType(ComponentClipboardEntry entry)
        {
            Type type = entry.ResolveType();

            if (type == null || typeof(Transform).IsAssignableFrom(type))
                return null;

            return type;
        }

        private static void Apply(ComponentClipboardEntry entry, Component component)
        {
            EditorJsonUtility.FromJsonOverwrite(entry.Json, component);
            entry.ApplyReferences(component);
            EditorUtility.SetDirty(component);
        }

        private static void Move(Component component, bool isUpwards)
        {
            if (component == null || component is Transform)
                return;

            Undo.RegisterCompleteObjectUndo(component.gameObject, MoveUndoName);

            if (isUpwards)
                ComponentUtility.MoveComponentUp(component);
            else
                ComponentUtility.MoveComponentDown(component);
        }

        private static bool IsRequiredBy(Type requiredType, Type ownerType)
        {
            object[] attributes = ownerType.GetCustomAttributes(typeof(RequireComponent), true);

            foreach (object attribute in attributes)
            {
                RequireComponent requirement = attribute as RequireComponent;

                if (requirement == null)
                    continue;

                if (IsMatch(requirement.m_Type0, requiredType)
                    || IsMatch(requirement.m_Type1, requiredType)
                    || IsMatch(requirement.m_Type2, requiredType))
                    return true;
            }

            return false;
        }

        private static bool IsMatch(Type requirement, Type candidate)
            => requirement != null && requirement.IsAssignableFrom(candidate);
    }
}