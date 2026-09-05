using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ControllerSupportPackage.Controller.Navigation;
using Base.ControllerSupportPackage.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Base.ControllerSupportPackage.Tests
{
    /// <summary>
    /// Covers the first step of a navigation rebuild. A selectable without an element is skipped by
    /// the wiring, so it silently stops being reachable with a gamepad while still looking and
    /// clicking exactly as it should with a mouse.
    /// </summary>
    public sealed class NavigationValidatorTests
    {
        private const string MissingRootMessage = "without a root";

        private readonly List<GameObject> _hosts = new();

        /// <summary>Hands back everything the test put in the scene.</summary>
        [TearDown]
        public void Cleanup()
        {
            foreach (GameObject host in _hosts)
            {
                if (host != null)
                    Object.DestroyImmediate(host);
            }

            _hosts.Clear();
        }

        /// <summary>A selectable with nothing on it is the case the whole step exists for.</summary>
        [Test]
        public void ASelectableWithoutAnElementIsGivenOne()
        {
            Button button = CreateButton();

            Assert.That(NavigationValidator.AddMissingElements(button.transform), Is.EqualTo(1));
            Assert.That(button.GetComponent<NavigableElement>(), Is.Not.Null);
        }

        /// <summary>
        /// The element is wired to the selectable it was added for rather than left for somebody to
        /// assign, since an unwired element navigates no better than a missing one.
        /// </summary>
        [Test]
        public void TheAddedElementIsWiredToItsSelectable()
        {
            Button button = CreateButton();
            NavigationValidator.AddMissingElements(button.transform);

            Assert.That(button.GetComponent<NavigableElement>().Selectable, Is.SameAs(button));
        }

        /// <summary>
        /// Running the step twice adds nothing the second time, which is what lets it run at the head
        /// of every rebuild without piling components onto the same object.
        /// </summary>
        [Test]
        public void RunningTwiceAddsNothingTheSecondTime()
        {
            Button button = CreateButton();
            NavigationValidator.AddMissingElements(button.transform);

            Assert.That(NavigationValidator.AddMissingElements(button.transform), Is.EqualTo(0));
            Assert.That(button.GetComponents<NavigableElement>(), Has.Length.EqualTo(1));
        }

        /// <summary>Every selectable below the root is reached, not only the one carrying it.</summary>
        [Test]
        public void SelectablesFurtherDownAreReachedToo()
        {
            GameObject root = CreateHost(nameof(SelectablesFurtherDownAreReachedToo));
            Button child = CreateButton();
            Button grandChild = CreateButton();

            child.transform.SetParent(root.transform);
            grandChild.transform.SetParent(child.transform);

            Assert.That(NavigationValidator.AddMissingElements(root.transform), Is.EqualTo(2));
        }

        /// <summary>
        /// A selectable that starts switched off is wired too. It is going to be turned on at some
        /// point, and nothing rebuilds navigation at that moment.
        /// </summary>
        [Test]
        public void ASelectableThatIsSwitchedOffIsWiredAsWell()
        {
            GameObject root = CreateHost(nameof(ASelectableThatIsSwitchedOffIsWiredAsWell));
            Button hidden = CreateButton();

            hidden.transform.SetParent(root.transform);
            hidden.gameObject.SetActive(false);

            Assert.That(NavigationValidator.AddMissingElements(root.transform), Is.EqualTo(1));
        }

        /// <summary>An object with nothing selectable under it needs nothing done to it.</summary>
        [Test]
        public void AnObjectWithNothingSelectableIsLeftAlone()
        {
            GameObject root = CreateHost(nameof(AnObjectWithNothingSelectableIsLeftAlone));

            Assert.That(NavigationValidator.AddMissingElements(root.transform), Is.EqualTo(0));
        }

        /// <summary>Nothing to check is said out loud rather than counted as a clean pass.</summary>
        [Test]
        public void ValidatingNothingIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex(MissingRootMessage));

            Assert.That(NavigationValidator.AddMissingElements(null), Is.EqualTo(0));
        }

        /// <summary>Creates a button and remembers its object so the teardown can clean it up.</summary>
        private Button CreateButton() => CreateHost(nameof(Button)).AddComponent<Button>();

        /// <summary>Creates an object and remembers it so the teardown can clean it up.</summary>
        private GameObject CreateHost(string name)
        {
            GameObject host = new(name);
            _hosts.Add(host);

            return host;
        }
    }
}