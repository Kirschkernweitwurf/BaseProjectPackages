using System.Collections.Generic;
using Systems.Tweening.Core.Data;
using Utility.Logging;

namespace Systems.Tweening.Core
{
    /// <summary>
    /// A sequence of tweens that can run sequentially or in parallel.
    /// </summary>
    public sealed class TweenSequence : TweenBase
    {
        public override bool IsRunning => _isRunning;

        public override bool IsCompleted => _isCompleted;

        private readonly List<TweenBase> _tweens = new();
        private readonly ESequenceMode _mode;

        private int _currentIndex;
        private int _completedCount;
        private bool _isRunning;
        private bool _isCompleted;

        public TweenSequence(ESequenceMode mode) => _mode = mode;

        /// <summary>
        /// Adds a tween to the sequence.
        /// </summary>
        /// <param name="tween">The tween to add.</param>
        public void Add(TweenBase tween)
        {
            if (tween == null)
            {
                CustomLogger.LogWarning("Attempted to add a null tween to the sequence. Ignoring.", null);
                return;
            }

            _tweens.Add(tween);
        }

        public override void Start()
        {
            if (_tweens.Count == 0)
            {
                Complete();
                return;
            }

            _isRunning = true;
            _isCompleted = false;
            _completedCount = 0;

            switch (_mode)
            {
                case ESequenceMode.Parallel:
                {
                    foreach (TweenBase tween in _tweens)
                    {
                        tween.OnComplete += OnTweenComplete;
                        tween.Start();
                    }

                    break;
                }
                case ESequenceMode.Sequential:
                {
                    _currentIndex = 0;
                    PlayNext();
                    break;
                }
                default:
                {
                    CustomLogger.LogWarning("Unhandled sequence mode. Completing sequence immediately.", null);
                    Complete();
                    break;
                }
            }
        }

        public override void Stop()
        {
            foreach (TweenBase tween in _tweens)
                tween.Stop();

            _isRunning = false;
            _isCompleted = true;
        }

        public override void Tick(float deltaTime) { } // Individual tweens are ticked by TweenRunner

        private void PlayNext()
        {
            if (_currentIndex >= _tweens.Count)
            {
                _isRunning = false;
                _isCompleted = true;
                Complete();
                return;
            }

            TweenBase tween = _tweens[_currentIndex];
            tween.OnComplete += OnTweenComplete;
            tween.Start();
        }

        private void OnTweenComplete(TweenBase completedTween)
        {
            switch (_mode)
            {
                case ESequenceMode.Parallel:
                {
                    _completedCount++;
                    if (_completedCount < _tweens.Count)
                        return;

                    _isRunning = false;
                    _isCompleted = true;
                    Complete();
                    break;
                }
                case ESequenceMode.Sequential:
                {
                    _currentIndex++;
                    PlayNext();
                    break;
                }
                default:
                {
                    CustomLogger.LogWarning("Unhandled sequence mode. Completing sequence immediately.", null);
                    _isRunning = false;
                    _isCompleted = true;
                    Complete();
                    break;
                }
            }

            completedTween.OnComplete -= OnTweenComplete;
        }
    }
}