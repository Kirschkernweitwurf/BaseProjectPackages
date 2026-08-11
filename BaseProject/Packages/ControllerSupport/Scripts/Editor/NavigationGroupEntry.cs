using Base.ControllerSupport.Controller.Navigation;
using UnityEditor;
using Menu = Base.CorePackage.MenuManaging.Menu;

namespace Base.ControllerSupport.Editor
{
    /// <summary>
    /// One scanned <see cref="NavigableGroup"/> together with the <see cref="Menu"/> it sits on, its
    /// element count and the menu rule violations that follow from both. A group on a menu must leave
    /// Auto Activate off, since the menu is the one activating, and should carry the menu's priority.
    /// Violations are only ever fixed through <see cref="Fix"/>, never silently while scanning.
    /// </summary>
    internal sealed class NavigationGroupEntry
    {
        private const string NoMenuText = "None";
        private const string SingleElementText = "1 Element";

        /// <summary>The scanned group.</summary>
        public NavigableGroup Group { get; }

        /// <summary>The menu on the same GameObject, or null when the group manages its own activation.</summary>
        public Menu Menu { get; }

        /// <summary>True while the group still exists. Scanned rows can be destroyed between repaints.</summary>
        public bool IsAlive => Group != null;

        /// <summary>True when a menu drives activation but the group also activates itself.</summary>
        public bool HasAutoActivateConflict => Menu != null
            && Group.AutoActivate;

        /// <summary>True when the group's focus priority differs from its menu's priority.</summary>
        public bool HasPriorityMismatch => Menu != null
            && Group.Priority != Menu.Priority;

        /// <summary>True when the group breaks at least one menu rule.</summary>
        public bool HasIssues => HasAutoActivateConflict
            || HasPriorityMismatch;

        /// <summary>True when the group holds no navigable elements at all.</summary>
        public bool IsEmpty => ElementCount == 0;

        /// <summary>Badge text for the menu column.</summary>
        public string MenuText => Menu != null
            ? Menu.GetType().Name
            : NoMenuText;

        /// <summary>Badge text for the element count column.</summary>
        public string ElementsText => ElementCount == 1
            ? SingleElementText
            : $"{ElementCount} Elements";

        /// <summary>Badge text for the priority column.</summary>
        public string PriorityText => Group.Priority.ToString();

        /// <summary>Badge text for the scene column.</summary>
        public string SceneText => Group.gameObject.scene.name;

        /// <summary>Tooltip explaining the priority badge.</summary>
        public string PriorityTooltip => HasPriorityMismatch
            ? $"Priority differs from the menu ({Menu.Priority})."
            : "Focus priority.";

        /// <summary>Number of navigable elements below the group.</summary>
        private int ElementCount { get; }

        /// <summary>Collects the menu and element count belonging to a group.</summary>
        public NavigationGroupEntry(NavigableGroup group)
        {
            Group = group;
            Menu = group.GetComponent<Menu>();
            ElementCount = group.GetComponentsInChildren<NavigableElement>(true).Length;
        }

        /// <summary>Tooltip explaining the menu badge and any rule the group breaks.</summary>
        public string BuildMenuTooltip()
        {
            if (Menu == null)
                return "This group sits on no menu and manages its own activation.";

            if (!HasIssues)
                return "This group sits on a menu, which drives its activation.";

            string tooltip = string.Empty;

            if (HasAutoActivateConflict)
                tooltip = "Auto Activate is enabled, but the menu is the one activating this group.";

            if (!HasPriorityMismatch)
                return tooltip;

            if (HasAutoActivateConflict)
                tooltip += "\n";

            return tooltip + $"Priority differs from the menu ({Menu.Priority}).";
        }

        /// <summary>Selects the group in the hierarchy and pings it.</summary>
        public void GoTo()
        {
            Selection.activeGameObject = Group.gameObject;
            EditorGUIUtility.PingObject(Group.gameObject);
        }

        /// <summary>Aligns the group with its menu's rules. Only ever called from an explicit click.</summary>
        public void Fix()
        {
            SerializedObject serializedGroup = new(Group);

            if (HasAutoActivateConflict)
                serializedGroup.FindProperty(NavigableGroup.AutoActivateFieldName).boolValue = false;

            if (HasPriorityMismatch)
                serializedGroup.FindProperty(NavigableGroup.PriorityFieldName).intValue = (int)Menu.Priority;

            serializedGroup.ApplyModifiedProperties();
        }
    }
}