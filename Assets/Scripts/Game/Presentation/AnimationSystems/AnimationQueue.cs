using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Game.Presentation.AnimationSystems
{
    public class AnimationQueue : IDisposable
    {
        private readonly Queue<Func<CancellationToken, Awaitable>> _queue = new();
        private CancellationTokenSource _cts = new();
        private bool _isPlaying;

        public void Enqueue(Func<CancellationToken, Awaitable> animation)
        {
            _queue.Enqueue(animation);
            if (!_isPlaying)
            {
                Play().Forget();
            }
        }

        private async Awaitable Play()
        {
            _isPlaying = true;
            CancellationTokenSource cts = _cts;
            CancellationToken token = cts.Token;
            try
            {
                while (_queue.Count > 0 && !token.IsCancellationRequested)
                {
                    Func<CancellationToken, Awaitable> animation = _queue.Dequeue();
                    await animation(token);
                }
            }
            catch (OperationCanceledException)
            {
                // Queue was cleared while an animation was playing.
            }
            finally
            {
                // A newer run may already be active after Clear.
                if (ReferenceEquals(cts, _cts))
                {
                    _isPlaying = false;
                }
            }
        }

        public void Clear()
        {
            _queue.Clear();
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            _isPlaying = false;
        }

        public void Dispose()
        {
            _queue.Clear();
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
