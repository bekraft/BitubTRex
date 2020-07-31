using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bitub.Transfer
{
    public static class EventExtensions
    {
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
