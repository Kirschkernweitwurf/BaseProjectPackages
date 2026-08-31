using System.Collections.Generic;
using Base.AttributePackage;
using Base.SettingsPackage.Core;
using Base.UtilityPackage;
using Base.UtilityPackage.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.SettingsPackage.UI
{
    /// <summary>
    /// Base for settings that cycle through a fixed list of labeled options using left/right buttons and an
    /// optional row of selection indicators. Subclasses map between the option index and the stored value.
    /// </summary>
    /// <typeparam name="TValue">The value type held by the setting.</typeparam>
    /// <typeparam name="TSetting">The concrete <see cref="Setting{T}"/> type.</typeparam>
    public abstract class MultipleChoiceElement<TValue, TSetting> : TypedSettingElement<TValue, TSetting>
        where TSetting : Setting<TValue>
    {
        [Title("Multiple Choice")]
        [SerializeField] [Required] private Button leftButton;
        [SerializeField] [Required] private Button rightButton;
        [SerializeField] [Required] private TMP_Text valueText;
        [SerializeField] [Required] private SelectionIndicatorButton selectionIndicatorPrefab;
        [SerializeField] [Required] private Transform selectionIndicatorParent;
        [SerializeField] private List<string> options = new();

        /// <summary>Every selectable option label, in display order.</summary>
        protected IReadOnlyList<string> Options => options;

        /// <summary>Index of the currently selected option within <see cref="Options"/>.</summary>
        protected int CurrentIndex { get; private set; }

        private readonly List<SelectionIndicatorButton> _indicators = new();

        private HashSetObjectPool<SelectionIndicatorButton> _indicatorPool;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            _indicatorPool = new HashSetObjectPool<SelectionIndicatorButton>(selectionIndicatorPrefab,
                selectionIndicatorParent, CleanupIndicator);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            leftButton.onClick.AddListener(SelectPrevious);
            rightButton.onClick.AddListener(SelectNext);

            CoroutineRunner.Instance.RunNextFrame(RefreshIndicators);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            leftButton.onClick.RemoveListener(SelectPrevious);
            rightButton.onClick.RemoveListener(SelectNext);
        }
#endregion

        /// <inheritdoc/>
        protected sealed override void OnBound()
        {
            options = ResolveOptions();
            CurrentIndex = IndexOf(Setting.Value);

            RefreshValueText();
            BuildIndicators();
        }

        /// <inheritdoc/>
        protected sealed override void OnSettingChanged(TValue value)
        {
            CurrentIndex = IndexOf(value);

            RefreshValueText();
            RefreshIndicators();
        }

        /// <summary>Supplies the selectable options. Defaults to the serialized list.</summary>
        protected virtual List<string> ResolveOptions() => options;

        /// <summary>Maps a stored value onto the index of the option that represents it.</summary>
        protected abstract int IndexOf(TValue value);

        /// <summary>Maps an option index onto the value to store in the setting.</summary>
        protected abstract TValue ValueAt(int index);

        /// <summary>Returns the index of the option with the given label, or the first option when absent.</summary>
        protected int IndexOfOption(string label)
        {
            int index = options.IndexOf(label);
            return index < 0
                ? 0
                : index;
        }

        private static void CleanupIndicator(SelectionIndicatorButton indicator) => indicator.Cleanup();

        private void SelectPrevious() => Select(CurrentIndex - 1);

        private void SelectNext() => Select(CurrentIndex + 1);

        private void Select(int index)
        {
            // The buttons are live before the element binds, so an unbound element ignores them.
            if (Setting == null
                || options.Count == 0)
                return;

            CurrentIndex = (index % options.Count + options.Count) % options.Count;
            Setting.Value = ValueAt(CurrentIndex);

            RefreshValueText();
            RefreshIndicators();
        }

        private void BuildIndicators()
        {
            _indicatorPool.ReleaseAll();
            _indicators.Clear();

            for (int i = 0; i < options.Count; i++)
            {
                int index = i;
                SelectionIndicatorButton indicator = _indicatorPool.Get();
                indicator.Initialize(index == CurrentIndex, onClick: () => Select(index));
                _indicators.Add(indicator);
            }
        }

        private void RefreshIndicators()
        {
            for (int i = 0; i < _indicators.Count; i++)
                _indicators[i].SetSelected(i == CurrentIndex);
        }

        private void RefreshValueText()
        {
            if (CurrentIndex >= 0
                && CurrentIndex < options.Count)
                valueText.text = options[CurrentIndex];
        }
    }
}