using System;
using System.Linq;
using System.Reflection;
using Base.AttributesPackage.Editor.Core;
using Base.AttributesPackage.Editor.Drawers;
using NUnit.Framework;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Which header controls a type declares, and which of them are passed over.
    /// <para>
    /// Every rule here fails silently. A method carrying the attribute but the wrong signature is
    /// skipped rather than reported, so a button that never appears in the header looks exactly like
    /// one that was never written. These pin what appears and what does not.
    /// </para>
    /// </summary>
    public sealed class HeaderItemCollectorTests
    {
        private const string ArgumentMethod = "TakesAnArgument";
        private const string DrawMethod = "DrawInto";
        private const string FallbackLabel = "Do The Thing";
        private const string FallbackMethod = "DoTheThing";
        private const string LabelMethod = "ReadState";
        private const string NamedMethod = "Named";
        private const string ReadableProperty = "ReadableLabel";
        private const string SameNameMethod = "SameName";
        private const string VoidLabelMethod = "ReturnsNothing";
        private const string WriteOnlyProperty = "WriteOnlyLabel";
        private const string WrongDrawMethod = "DrawWrongArgument";

        /// <summary>A type with none of the attributes declares no controls.</summary>
        [Test]
        public void ATypeWithoutTheAttributesDeclaresNothing()
            => Assert.That(HeaderItemCollector.Collect(typeof(HeaderItemCollectorTests)), Is.Empty);

        /// <summary>
        /// A button with no label of its own reads as the method name, spaced out the way Unity spaces
        /// a field name, because the method name is the only thing there is to go on.
        /// </summary>
        [Test]
        public void AButtonWithoutALabelFallsBackToTheMethodName()
            => Assert.That(Find(FallbackMethod).Label, Is.EqualTo(FallbackLabel));

        /// <summary>A button with a label of its own shows that instead.</summary>
        [Test]
        public void AButtonWithALabelShowsIt()
            => Assert.That(Find(NamedMethod).Label, Is.EqualTo(HeaderItemProbe.GivenLabel));

        /// <summary>
        /// A button the header cannot call is passed over. Nothing supplies an argument, so a method
        /// that wants one would throw the moment it was pressed.
        /// </summary>
        [Test]
        public void AButtonTakingAnArgumentIsSkipped()
            => Assert.That(Names(), Does.Not.Contain(ArgumentMethod));

        /// <summary>A label method that returns something becomes a label.</summary>
        [Test]
        public void ALabelMethodReturningSomethingIsCollected()
            => Assert.That(Find(LabelMethod).Kind, Is.EqualTo(EHeaderItemKind.Label));

        /// <summary>A label method returning nothing is passed over, since it has nothing to show.</summary>
        [Test]
        public void ALabelMethodReturningNothingIsSkipped()
            => Assert.That(Names(), Does.Not.Contain(VoidLabelMethod));

        /// <summary>A draw method taking the rect it is handed becomes a draw control.</summary>
        [Test]
        public void ADrawMethodTakingARectIsCollected()
            => Assert.That(Find(DrawMethod).Kind, Is.EqualTo(EHeaderItemKind.Draw));

        /// <summary>A draw method taking anything else is passed over.</summary>
        [Test]
        public void ADrawMethodTakingSomethingElseIsSkipped()
            => Assert.That(Names(), Does.Not.Contain(WrongDrawMethod));

        /// <summary>A readable property carrying the label attribute becomes a label.</summary>
        [Test]
        public void AReadablePropertyIsCollected()
            => Assert.That(Find(ReadableProperty).Kind, Is.EqualTo(EHeaderItemKind.Label));

        /// <summary>A property nothing can read is passed over, since there is nothing to display.</summary>
        [Test]
        public void AWriteOnlyPropertyIsSkipped()
            => Assert.That(Names(), Does.Not.Contain(WriteOnlyProperty));

        /// <summary>
        /// Methods come before properties, which is what puts the buttons and the readouts in a fixed
        /// order in the header rather than one that depends on how the type happened to be written.
        /// </summary>
        [Test]
        public void MethodsAreCollectedBeforeProperties()
        {
            HeaderItem[] items = HeaderItemCollector.Collect(typeof(HeaderItemProbe));
            int lastMethod = Array.FindLastIndex(items, item => item.Member is MethodInfo);
            int firstProperty = Array.FindIndex(items, item => item.Member is PropertyInfo);

            Assert.That(lastMethod, Is.LessThan(firstProperty));
        }

        /// <summary>The tooltip names the member it will call, so a label alone is never ambiguous.</summary>
        [Test]
        public void TheTooltipNamesTheMethodBehindTheLabel()
            => Assert.That(HeaderItemCollector.Describe(Find(NamedMethod)), Is.EqualTo($"{NamedMethod}()"));

        /// <summary>A label that already is the member name is left as it is rather than repeated.</summary>
        [Test]
        public void ATooltipDoesNotRepeatALabelThatIsAlreadyTheName()
            => Assert.That(HeaderItemCollector.Describe(Find(SameNameMethod)), Is.EqualTo(SameNameMethod));

        /// <summary>A button with no mode set is enabled either way.</summary>
        [Test]
        public void AnAlwaysButtonIsEnabled()
            => Assert.That(HeaderItemCollector.IsEnabled(EButtonMode.Always), Is.True);

        /// <summary>An edit mode button is enabled while the game is not running.</summary>
        [Test]
        public void AnEditModeButtonIsEnabledOutsidePlayMode()
            => Assert.That(HeaderItemCollector.IsEnabled(EButtonMode.EditMode), Is.True);

        /// <summary>A play mode button is disabled while the game is not running.</summary>
        [Test]
        public void APlayModeButtonIsDisabledOutsidePlayMode()
            => Assert.That(HeaderItemCollector.IsEnabled(EButtonMode.PlayMode), Is.False);

        /// <summary>The member names of every control the probe declares.</summary>
        /// <returns>One name per collected control.</returns>
        private static string[] Names() => HeaderItemCollector.Collect(typeof(HeaderItemProbe))
            .Select(item => item.Member.Name)
            .ToArray();

        /// <summary>Finds the control declared on the member with the given name.</summary>
        /// <param name="memberName">Name of the member the control was declared on.</param>
        /// <returns>The control.</returns>
        private static HeaderItem Find(string memberName)
            => HeaderItemCollector.Collect(typeof(HeaderItemProbe))
                .First(item => item.Member.Name == memberName);
    }
}