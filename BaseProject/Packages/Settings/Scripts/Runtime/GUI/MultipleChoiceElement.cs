using System.Collections.Generic;
using Base.CorePackage.ObjectPooling;
using Base.UtilityPackage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.SettingsPackage.GUI
{
    /// <summary>
    /// Base for settings that cycle through a fixed list of labeled options using left/right buttons and an
    /// optional row of selection indicators. Subclasses bind the current index to a typed setting.
    /// </summary>
    public abstract class MultipleChoiceElement : SettingElement
    {
        [Header("Multiple Choice")]

        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private SelectionIndicatorButton selectionIndicatorPrefab;
        [SerializeField] private Transform selectionIndicatorParent;

        /// <summary>Every selectable option, in display order.</summary>
        [SerializeField] protected List<string> options = new();

        /// <summary>Index of the currently selected option within <see cref="options"/>.</summary>
        protected int CurrentIndex { get; set; }

        private readonly List<SelectionIndicatorButton> _indicators = new();

        private HashSetObjectPool<SelectionIndicatorButton> _indicatorPool;

#region Unity Callbacks
        private void Awake() => _indicatorPool = new HashSetObjectPool<SelectionIndicatorButton>(
            selectionIndicatorPrefab,
            selectionIndicatorParent, CleanupIndicator);

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

        /// <summary>Pushes the current index into the bound setting.</summary>
        protected abstract void ApplySelection();

        /// <summary>Rebuilds the selection indicator row to match the current options and index.</summary>
        protected void BuildIndicators()
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

        /// <summary>Updates which indicator appears selected.</summary>
        protected void RefreshIndicators()
        {
            for (int i = 0; i < _indicators.Count; i++)
                _indicators[i].SetSelected(i == CurrentIndex);
        }

        /// <summary>Writes the current option label into the value text.</summary>
        protected void RefreshValueText()
        {
            if (CurrentIndex >= 0 && CurrentIndex < options.Count)
                valueText.text = options[CurrentIndex];
        }

        private static void CleanupIndicator(SelectionIndicatorButton indicator) => indicator.Cleanup();

        private void Select(int index)
        {
            if (options.Count == 0)
                return;

            CurrentIndex = (index % options.Count + options.Count) % options.Count;
            ApplySelection();
            RefreshValueText();
            RefreshIndicators();
        }

        private void SelectPrevious() => Select(CurrentIndex - 1);

        private void SelectNext() => Select(CurrentIndex + 1);
    }
}