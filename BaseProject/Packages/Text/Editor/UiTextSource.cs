using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage.Logging;
using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Pseudo;
using UnityEngine.Localization.Tables;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Every text box that shows one localized string: boxes fed by a <see cref="LocalizeStringEvent"/>, like menu
    /// buttons and labels and boxes that code fills from a string marked with <see cref="ShownInAttribute"/>.
    /// Found on their own in every prefab and scene, so a new localized label is checked without any setup.
    /// </summary>
    internal sealed class UiTextSource : ITextSampleSource
    {
        private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public
            | BindingFlags.NonPublic;
        private const string NoTextFieldFormat = "[{0}] on {1}.{2} names '{3}', which is no text field of {1}.";

        private static readonly string ShownInName = nameof(ShownInAttribute).Replace(nameof(Attribute),
            string.Empty);

        private readonly List<(TextBox Box, LocalizedString Text)> _boxes = new();

        /// <inheritdoc/>
        public string Area => "UI";

        /// <inheritdoc/>
        public void Load(TextFitStage stage)
        {
            foreach (LocalizeStringEvent localizer in stage.FindAll<LocalizeStringEvent>())
            {
                TMP_Text text = TargetOf(localizer);

                if (text != null
                    && !localizer.StringReference.IsEmpty)
                    _boxes.Add((stage.Box(text), localizer.StringReference));
            }

            foreach (FieldInfo field in TypeCache.GetFieldsWithAttribute<ShownInAttribute>())
                AddShownIn(stage, field);
        }

        /// <inheritdoc/>
        public IEnumerable<TextSample> Collect(Locale locale)
        {
            Locale source = TextFitLocales.SourceOf(locale);
            PseudoLocale pseudo = locale as PseudoLocale;
            List<TextSample> samples = new();

            foreach ((TextBox box, LocalizedString text) in _boxes)
            {
                StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(
                    text.TableReference);
                StringTableEntry entry = TextFitLocales.FindShownEntry(collection, text.TableEntryReference, source);

                // Not even English has it, so the game shows nothing there: the empty sample fails the check.
                if (entry == null)
                {
                    samples.Add(new TextSample(Area, box, KeyOf(collection, text), string.Empty, string.Empty, null,
                        text));
                    continue;
                }

                if (!entry.IsSmart)
                {
                    samples.Add(new TextSample(Area, box, entry.Key, string.Empty,
                        entry.GetLocalizedString(null, null, pseudo), null, text));
                    continue;
                }

                foreach ((Dictionary<string, object> values, string formatted) in SmartTestArguments.For(entry.Value,
                             set => entry.GetLocalizedString(null, new object[] { set }, pseudo)))
                {
                    samples.Add(new TextSample(Area, box, entry.Key, SmartTestArguments.Describe(values), formatted,
                        null, text));
                }
            }

            return samples;
        }

        // The key of a text, for the report. Empty when its table is gone.
        private static string KeyOf(StringTableCollection collection, LocalizedString text) => collection == null
            ? string.Empty
            : text.TableEntryReference.ResolveKeyName(collection.SharedData);

        // The event writes into whatever text its update event targets. Without a target set up in the inspector it
        // usually sits on a button and feeds the label below it.
        private static TMP_Text TargetOf(LocalizeStringEvent localizer)
        {
            UnityEvent<string> update = localizer.OnUpdateString;

            for (int i = 0; i < update.GetPersistentEventCount(); i++)
            {
                if (update.GetPersistentTarget(i) is TMP_Text target)
                    return target;
            }

            return localizer.GetComponentInChildren<TMP_Text>(true);
        }

        // A button whose code swaps the string names its Localize String Event, which writes into the label.
        private static TMP_Text TextOf(object box)
        {
            if (box is LocalizeStringEvent localizer
                && localizer != null)
                return TargetOf(localizer);

            return box as TMP_Text;
        }

        // A field holds one string or a list of the strings its box shows one at a time, like carousel options.
        private static bool IsLocalizedText(Type type) => typeof(LocalizedString).IsAssignableFrom(type)
            || typeof(IEnumerable<LocalizedString>).IsAssignableFrom(type);

        private static IEnumerable<LocalizedString> StringsOf(object value) => value switch
        {
            LocalizedString single => new[] { single },
            IEnumerable<LocalizedString> many => many,
            _ => Array.Empty<LocalizedString>()
        };

        private void AddShownIn(TextFitStage stage, FieldInfo field)
        {
            Type owner = field.DeclaringType;

            if (owner == null
                || !typeof(Component).IsAssignableFrom(owner)
                || !IsLocalizedText(field.FieldType))
                return;

            string textField = field.GetCustomAttribute<ShownInAttribute>().TextField;
            FieldInfo boxField = owner.GetField(textField, InstanceFields);

            if (boxField == null
                || (!typeof(TMP_Text).IsAssignableFrom(boxField.FieldType)
                    && !typeof(LocalizeStringEvent).IsAssignableFrom(boxField.FieldType)))
            {
                CustomLogger.LogError(string.Format(NoTextFieldFormat, ShownInName, owner.Name, field.Name,
                    textField), null);
                return;
            }

            foreach (Component component in stage.FindAll(owner))
            {
                TMP_Text text = TextOf(boxField.GetValue(component));

                if (text == null)
                    continue;

                foreach (LocalizedString value in StringsOf(field.GetValue(component)))
                {
                    if (!value.IsEmpty)
                        _boxes.Add((stage.Box(text), value));
                }
            }
        }
    }
}