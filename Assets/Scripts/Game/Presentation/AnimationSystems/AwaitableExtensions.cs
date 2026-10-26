using System;
using System.Threading;
using LitMotion;
using UnityEngine;

namespace Game.Presentation.AnimationSystems
{
    public static class AwaitableExtensions
    {
        public static async void Forget(this Awaitable awaitable)
        {
            try
            {
                await awaitable;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public static MotionHandle RegisterTo(this MotionHandle handle, CancellationToken token)
        {
            token.Register(() => handle.TryCancel());
            return handle;
        }

        public static async Awaitable<TResult> Then<TSource, TResult>(
            this Awaitable<TSource> task,
            Func<TSource, Awaitable<TResult>> continuation)
        {
            TSource result = await task;
            return await continuation(result);
        }
    }
}