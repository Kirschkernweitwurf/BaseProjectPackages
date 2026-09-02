using Base.TweeningPackage.Core;

namespace Base.TweeningPackage.Tests
{
    /// <summary>
    /// A tween that does nothing on its own and finishes when a test says so. It lets the sequence and
    /// runner tests drive the exact order of events they are about, without a clock or a real value to
    /// interpolate.
    /// </summary>
    internal sealed class TweenProbe : TweenBase
    {
        /// <inheritdoc/>
        public override bool IsRunning => _isRunning;

        /// <inheritdoc/>
        public override bool IsCompleted => _isCompleted;

        /// <summary>How often the tween was started.</summary>
        internal int StartCount { get; private set; }

        /// <summary>How often the tween was stopped.</summary>
        internal int StopCount { get; private set; }

        /// <summary>Whether the last stop asked the tween to snap to its end value.</summary>
        internal bool WasStoppedWithComplete { get; private set; }

        private bool _isRunning;
        private bool _isCompleted;

        /// <inheritdoc/>
        public override void Start()
        {
            StartCount++;
            _isRunning = true;
            _isCompleted = false;
        }

        /// <inheritdoc/>
        public override void Stop(bool complete = false)
        {
            StopCount++;
            WasStoppedWithComplete = complete;
            _isRunning = false;
            _isCompleted = true;

            if (complete)
                InvokeComplete();

            InvokeKill();
        }

        /// <inheritdoc/>
        public override void Tick(float deltaTime) { }

        /// <summary>Finishes the tween the way a real one does when it reaches its end value.</summary>
        internal void Finish()
        {
            _isRunning = false;
            _isCompleted = true;

            InvokeComplete();
            InvokeKill();
        }
    }
}