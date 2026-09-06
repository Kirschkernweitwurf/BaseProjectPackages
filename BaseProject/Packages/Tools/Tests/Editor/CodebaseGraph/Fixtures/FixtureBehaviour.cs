using System;
using System.Collections;
using UnityEngine;

namespace Base.ToolsPackage.Editor.Tests.CodebaseGraph.Fixtures
{
    /// <summary>
    /// The shapes that a scanner reading compiled metadata gets wrong unless it goes out of its way.
    /// Every member here is reachable, several of them only through machinery the compiler generated or
    /// through a string the engine resolves at runtime.
    /// </summary>
    public sealed class FixtureBehaviour : MonoBehaviour, IFixtureContract
    {
        private const string InvokedName = "InvokedByName";

        /// <summary>A field like event, both subscribed to and raised from this same type.</summary>
        public event Action Changed;

        /// <summary>Written by Unity through the generated backing field, never assigned in code.</summary>
        [field: SerializeField] public GameObject Prefab { get; private set; }

        [SerializeField] private int _neverRead;

        /// <summary>Runs a lambda from inside a getter, which is where the owner name is an accessor.</summary>
        public int LambdaInAccessor
        {
            get
            {
                Func<int> step = () =>
                {
                    CalledFromAccessorLambda();
                    return 1;
                };

                return step();
            }
        }

        private readonly FixtureDeadCode _dead = new();
        [SerializeField] private FixtureVector _vector;

#region Unity Callbacks
        private void Awake()
        {
            Changed += OnChanged;
            Changed?.Invoke();

            Invoke(InvokedName, 1f);

            _dead.Touch();
            StartCoroutine(Countdown());

            RunThroughLocalFunction();
            RunThroughLambda();

            ReadIndexer();
            CompareVectors();

            Debug.Log(LambdaInAccessor);

            IFixtureContract contract = this;
            contract.Implicit();
            contract.Explicit();

            Debug.Log(FixtureConstants.SharedLabel);
            Debug.Log(FixtureNestingHost.Metrics.Padding);
        }
#endregion

        /// <summary>Implemented implicitly, called through the interface rather than by name.</summary>
        public void Implicit() { }

        /// <summary>Explicit implementation, whose metadata name carries the interface in front of it.</summary>
        void IFixtureContract.Explicit() { }

        /// <summary>Called by the engine from the string handed to Invoke, and by nothing else.</summary>
        private void InvokedByName() { }

        /// <summary>Subscribed to the field like event, and called by nothing else.</summary>
        private void OnChanged() { }

        /// <summary>An iterator, whose body the compiler moves into a hidden state machine.</summary>
        private IEnumerator Countdown()
        {
            yield return null;
        }

        /// <summary>Called only from inside a local function, which is itself hidden machinery.</summary>
        private void CalledFromLocalFunction() { }

        /// <summary>Called only from inside a lambda, which the compiler moves into a hidden class.</summary>
        private void CalledFromLambda() { }

        /// <summary>
        /// Called only from inside a lambda that lives in a property getter. The machinery names its
        /// owner as get_LambdaInAccessor, and only the property is ever registered, so this is the
        /// shape where the owner lookup can fail and take every call in the lambda with it.
        /// </summary>
        private void CalledFromAccessorLambda() { }

        private void RunThroughLocalFunction()
        {
            Step();
            return;

            void Step()
            {
                CalledFromLocalFunction();
            }
        }

        private void RunThroughLambda()
        {
            Action step = () => CalledFromLambda();
            step();
        }

        private void ReadIndexer() => Debug.Log(_vector[0]);

        private void CompareVectors()
        {
            FixtureVector other = new(1, 2);

            bool isSame = _vector == other;
            bool isDifferent = _vector != other;

            Debug.Log($"{isSame} {isDifferent}");
        }
    }
}