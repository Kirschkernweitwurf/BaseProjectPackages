using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ControllerSupportPackage.Controller.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UINavigation = UnityEngine.UI.Navigation;

namespace Base.ControllerSupportPackage.Tests
{
    /// <summary>
    /// Covers the explicit navigation written across a set of elements from where they sit on screen.
    /// A wrong neighbour is a menu that traps the stick, so what matters is that each direction picks
    /// the nearest element that actually lines up, and that nothing off-axis gets wired at all.
    /// </summary>
    public sealed class NavigationBuilderTests
    {

        private readonly List<GameObject> _created = new();

        /// <summary>Takes down everything a test built.</summary>
        [TearDown]
        public void Release()
        {
            foreach (GameObject created in _created)
            {
                if (created != null)
                    Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        /// <summary>Wiring switches the elements to explicit navigation.</summary>
        [Test]
        public void WiringSwitchesToExplicitNavigation()
        {
            NavigableElement only = Element(Vector3.zero);

            NavigationBuilder.Wire(new[] { only }, wrap: false);

            Assert.That(only.Selectable.navigation.mode, Is.EqualTo(UINavigation.Mode.Explicit));
        }

        /// <summary>A column wires up and down between its neighbours.</summary>
        [Test]
        public void AColumnWiresUpAndDown()
        {
            NavigableElement bottom = Element(new Vector3(0f, 0f, 0f));
            NavigableElement middle = Element(new Vector3(0f, 1f, 0f));
            NavigableElement top = Element(new Vector3(0f, 2f, 0f));

            NavigationBuilder.Wire(new[] { bottom, middle, top }, wrap: false);

            Assert.That(middle.Selectable.navigation.selectOnUp, Is.SameAs(top.Selectable));
            Assert.That(middle.Selectable.navigation.selectOnDown, Is.SameAs(bottom.Selectable));
        }

        /// <summary>A column picks the nearest neighbour rather than the furthest one.</summary>
        [Test]
        public void AColumnPicksTheNearestNeighbour()
        {
            NavigableElement bottom = Element(new Vector3(0f, 0f, 0f));
            NavigableElement middle = Element(new Vector3(0f, 1f, 0f));
            NavigableElement top = Element(new Vector3(0f, 2f, 0f));

            NavigationBuilder.Wire(new[] { bottom, middle, top }, wrap: false);

            Assert.That(bottom.Selectable.navigation.selectOnUp, Is.SameAs(middle.Selectable));
        }

        /// <summary>A column leaves left and right unwired, since nothing sits beside it.</summary>
        [Test]
        public void AColumnLeavesTheSidesUnwired()
        {
            NavigableElement bottom = Element(new Vector3(0f, 0f, 0f));
            NavigableElement top = Element(new Vector3(0f, 1f, 0f));

            NavigationBuilder.Wire(new[] { bottom, top }, wrap: false);

            Assert.That(bottom.Selectable.navigation.selectOnLeft, Is.Null);
            Assert.That(bottom.Selectable.navigation.selectOnRight, Is.Null);
        }

        /// <summary>Without wrapping the ends of a column lead nowhere.</summary>
        [Test]
        public void WithoutWrappingTheEndsLeadNowhere()
        {
            NavigableElement bottom = Element(new Vector3(0f, 0f, 0f));
            NavigableElement top = Element(new Vector3(0f, 1f, 0f));

            NavigationBuilder.Wire(new[] { bottom, top }, wrap: false);

            Assert.That(top.Selectable.navigation.selectOnUp, Is.Null);
            Assert.That(bottom.Selectable.navigation.selectOnDown, Is.Null);
        }

        /// <summary>With wrapping the ends of a column lead to the opposite side.</summary>
        [Test]
        public void WithWrappingTheEndsLeadToTheOppositeSide()
        {
            NavigableElement bottom = Element(new Vector3(0f, 0f, 0f));
            NavigableElement middle = Element(new Vector3(0f, 1f, 0f));
            NavigableElement top = Element(new Vector3(0f, 2f, 0f));

            NavigationBuilder.Wire(new[] { bottom, middle, top }, wrap: true);

            Assert.That(top.Selectable.navigation.selectOnUp, Is.SameAs(bottom.Selectable));
            Assert.That(bottom.Selectable.navigation.selectOnDown, Is.SameAs(top.Selectable));
        }

        /// <summary>Wrapping does not disturb the neighbours in the middle.</summary>
        [Test]
        public void WrappingLeavesTheMiddleAlone()
        {
            NavigableElement bottom = Element(new Vector3(0f, 0f, 0f));
            NavigableElement middle = Element(new Vector3(0f, 1f, 0f));
            NavigableElement top = Element(new Vector3(0f, 2f, 0f));

            NavigationBuilder.Wire(new[] { bottom, middle, top }, wrap: true);

            Assert.That(middle.Selectable.navigation.selectOnUp, Is.SameAs(top.Selectable));
            Assert.That(middle.Selectable.navigation.selectOnDown, Is.SameAs(bottom.Selectable));
        }

        /// <summary>A row wires left and right between its neighbours.</summary>
        [Test]
        public void ARowWiresLeftAndRight()
        {
            NavigableElement left = Element(new Vector3(0f, 0f, 0f));
            NavigableElement middle = Element(new Vector3(1f, 0f, 0f));
            NavigableElement right = Element(new Vector3(2f, 0f, 0f));

            NavigationBuilder.Wire(new[] { left, middle, right }, wrap: false);

            Assert.That(middle.Selectable.navigation.selectOnRight, Is.SameAs(right.Selectable));
            Assert.That(middle.Selectable.navigation.selectOnLeft, Is.SameAs(left.Selectable));
        }

        /// <summary>
        /// An element that sits off to the side is only reachable in the direction it lines up with, so
        /// the stick does not jump across the screen on a press it never meant.
        /// </summary>
        [Test]
        public void AnOffAxisElementIsOnlyWiredWhereItLinesUp()
        {
            NavigableElement origin = Element(new Vector3(0f, 0f, 0f));
            NavigableElement offAxis = Element(new Vector3(10f, 3f, 0f));

            NavigationBuilder.Wire(new[] { origin, offAxis }, wrap: false);

            Assert.That(origin.Selectable.navigation.selectOnRight, Is.SameAs(offAxis.Selectable));
            Assert.That(origin.Selectable.navigation.selectOnUp, Is.Null);
        }

        /// <summary>An element that cannot take focus is stepped over rather than wired to.</summary>
        [Test]
        public void AnElementThatCannotTakeFocusIsSteppedOver()
        {
            NavigableElement bottom = Element(new Vector3(0f, 0f, 0f));
            NavigableElement middle = Element(new Vector3(0f, 1f, 0f));
            NavigableElement top = Element(new Vector3(0f, 2f, 0f));

            middle.gameObject.SetActive(false);

            NavigationBuilder.Wire(new[] { bottom, middle, top }, wrap: false);

            Assert.That(bottom.Selectable.navigation.selectOnUp, Is.SameAs(top.Selectable));
        }

        /// <summary>An element with nothing to make navigable is skipped.</summary>
        [Test]
        public void AnElementWithoutASelectableIsSkipped()
        {
            NavigableElement bottom = Element(new Vector3(0f, 0f, 0f));
            NavigableElement top = Element(new Vector3(0f, 1f, 0f));
            NavigableElement unwired = ElementWithoutSelectable(new Vector3(0f, 0.5f, 0f));

            NavigationBuilder.Wire(new[] { bottom, unwired, top }, wrap: false);

            Assert.That(bottom.Selectable.navigation.selectOnUp, Is.SameAs(top.Selectable));
        }

        /// <summary>A single element has no neighbours to lead to.</summary>
        [Test]
        public void ASingleElementLeadsNowhere()
        {
            NavigableElement only = Element(Vector3.zero);

            NavigationBuilder.Wire(new[] { only }, wrap: true);

            Assert.That(only.Selectable.navigation.selectOnUp, Is.Null);
            Assert.That(only.Selectable.navigation.selectOnDown, Is.Null);
            Assert.That(only.Selectable.navigation.selectOnLeft, Is.Null);
            Assert.That(only.Selectable.navigation.selectOnRight, Is.Null);
        }

        /// <summary>Nothing to wire is not an error.</summary>
        [Test]
        public void NothingToWireIsFine()
            => Assert.DoesNotThrow(() => NavigationBuilder.Wire(Array.Empty<NavigableElement>(), wrap: true));

        /// <summary>A missing list is reported rather than walked into.</summary>
        [Test]
        public void AMissingListIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex("without a list"));

            NavigationBuilder.Wire(null, wrap: false);
        }

        // The selectable goes on first, so the required component is already there and the element
        // does not bring a second one of its own along.
        private NavigableElement Element(Vector3 position)
        {
            GameObject created = Create(position);
            Selectable selectable = created.AddComponent<Selectable>();
            NavigableElement element = created.AddComponent<NavigableElement>();

            // The field is filled by an attribute processor that only runs in a real editor pass, so
            // the test wires it through the serialized name the tooling already uses.
            SerializedObject serialized = new(element);

            serialized.FindProperty(NavigableElement.SelectableFieldName).objectReferenceValue = selectable;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return element;
        }

        // The required component still arrives, but the serialized field stays empty, which is the
        // state an element is in before anything filled it in.
        private NavigableElement ElementWithoutSelectable(Vector3 position)
            => Create(position).AddComponent<NavigableElement>();

        private GameObject Create(Vector3 position)
        {
            GameObject created = new(nameof(NavigableElement));

            created.transform.position = position;
            _created.Add(created);

            return created;
        }
    }
}