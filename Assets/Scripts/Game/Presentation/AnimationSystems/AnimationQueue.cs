using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Presentation.AnimationSystems
{
    public class AnimationQueue
    {
        private readonly Queue<Func<Awaitable>> _queue = new();
        private bool _isPlaying = false;

        public async Task Enqueue(Func<Awaitable> animation)
        {
            _queue.Enqueue(animation);
            if (!_isPlaying)
            {
                await Play();
            }
        }

        public void Enqueue<T>(Func<T, Awaitable> animation, T input)
        {
            Func<Awaitable> applied = animation.Apply(input);
            Enqueue(applied);
        }
        
        public void Enqueue<T1, T2>(Func<T1, T2, Awaitable> animation, T1 input1, T2 input2)
        {
            Func<Awaitable> applied = animation.Apply(input1, input2);
            Enqueue(applied);
        }
        
        private async Awaitable Play()
        {
            _isPlaying = true;
            while (_queue.Count > 0)
            {
                Func<Awaitable> animation = _queue.Dequeue();
                await animation();
            }

            _isPlaying = false;
        }

        public void PlayQueue(Queue<Func<Awaitable>> queue)
        {
            AnimationQueue animationQueue = new();
            while (_queue.Count > 0)
            {
                Func<Awaitable> animation = _queue.Dequeue();
                animationQueue.Enqueue(animation);
            }
        }
    }
}