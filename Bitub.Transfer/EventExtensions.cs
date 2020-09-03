using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitub.Transfer
{
    public static class EventExtensions
    {
        /// <summary>
        /// Shortcut for <c>foreach(..)</c>.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="enumerable">An enumerable of <typeparamref name="T"/>T</param>
        /// <param name="forEach">The action to take</param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> forEach)
        {
            foreach (T t in enumerable.ToArray()) forEach?.Invoke(t);
        }

        /// <summary>
        /// Raises the actions in async way.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="enumerable">An enumerable of <typeparamref name="T"/>T</param>
        /// <param name="forEach">The action to take</param>
        public static void ForEachAsync<T>(this IEnumerable<T> enumerable, Action<T> forEach)
        {
            foreach (T t in enumerable.ToArray()) forEach?.RaiseAsync(t);
        }

        /// <summary>
        /// Raises an event handler asynchronously.
        /// </summary>
        /// <typeparam name="TArg1">Type of first argument</typeparam>
        /// <param name="handlers">The event</param>
        /// <param name="arg1">The event argument</param>
        /// <returns>An awaitable task completing when all handlers completed</returns>
        public static Task RaiseAsync<TArg1>(this Action<TArg1> handlers, TArg1 arg1)
        {
            if (handlers != null)
            {
                return Task.WhenAll(handlers.GetInvocationList().OfType<Action<TArg1>>().Select(f =>
                {
                    return Task.Factory.FromAsync(
                        (asyncCallback, @object) => f.BeginInvoke(arg1, asyncCallback, @object), f.EndInvoke, null);
                }));
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Raises an event handler asynchronously.
        /// </summary>
        /// <typeparam name="TArg1">Type of first argument</typeparam>
        /// <param name="handlers">The event</param>
        /// <param name="arg1">The event argument</param>
        /// <returns>An awaitable task completing when all handlers completed</returns>
        public static Task RaiseAsync<TArg>(this EventHandler<TArg> handlers, object sender, TArg arg)
        {
            if (handlers != null)
            {
                return Task.WhenAll(handlers.GetInvocationList().OfType<EventHandler<TArg>>().Select(f =>
                {
                    return Task.Factory.FromAsync(
                        (asyncCallback, @object) => f.BeginInvoke(sender, arg, asyncCallback, @object), f.EndInvoke, null);
                }));
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Raises an event handler asynchronously.
        /// </summary>
        /// <typeparam name="TArg1">Type of first argument</typeparam>
        /// <typeparam name="TArg2">Type of the second argument</typeparam>
        /// <param name="handlers">The event</param>
        /// <param name="arg1">The event argument</param>
        /// <param name="arg2">The second event argument</param>
        /// <returns>An awaitable task completing when all handlers completed</returns>
        public static Task RaiseAsync<TArg1, TArg2>(this Action<TArg1, TArg2> handlers, TArg1 arg1, TArg2 arg2)
        {
            if (handlers != null)
            {
                return Task.WhenAll(handlers.GetInvocationList().OfType<Action<TArg1, TArg2>>().Select(f =>
                {
                    return Task.Factory.FromAsync(
                        (asyncCallback, @object) => f.BeginInvoke(arg1, arg2, asyncCallback, @object), f.EndInvoke, null);
                }));
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Raises an event handler asynchronously.
        /// </summary>
        /// <typeparam name="TArg1">Type of first argument</typeparam>
        /// <typeparam name="TArg2">Type of the second argument</typeparam>
        /// <typeparam name="TArg3">Type of the third argument</typeparam>
        /// <param name="handlers">The event</param>
        /// <param name="arg1">The event argument</param>
        /// <param name="arg2">The second event argument</param>
        /// <param name="arg3">The third event argument</param>
        /// <returns>An awaitable task completing when all handlers completed</returns>
        public static Task RaiseAsync<TArg1, TArg2, TArg3>(this Action<TArg1, TArg2, TArg3> handlers, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            if (handlers != null)
            {
                return Task.WhenAll(handlers.GetInvocationList().OfType<Action<TArg1, TArg2, TArg3>>().Select(f =>
                {
                    return Task.Factory.FromAsync(
                        (asyncCallback, @object) => f.BeginInvoke(arg1, arg2, arg3, asyncCallback, @object), f.EndInvoke, null);
                }));
            }

            return Task.CompletedTask;
        }

        public static Task RaiseAsync<TArg1, TArg2>(this Action<TArg1, TArg2, object[]> handlers, TArg1 arg1, TArg2 arg2, params object[] args3)
        {
            return RaiseAsync<TArg1, TArg2, object[]>(handlers, arg1, arg2, args3);
        }
    }
}
