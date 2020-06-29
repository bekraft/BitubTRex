using System;

namespace Bitub.Transfer
{
    /// <summary>
    /// Progress delegate using a delegation function and reporting the result
    /// to the underlaying receiver.
    /// </summary>
    /// <typeparam name="E">The delegated progress state type</typeparam>
    /// <typeparam name="T">The accumulated progress state type</typeparam>
    public class ProgressTransform<E, T> : IProgress<E> where E : ICancelableProgressState
    {
        public ProgressTransform(IProgressing announcer, Func<E, T> delegation)
        {
            announcer.OnProgressChange += OnProgressChanged;
            Delegation = delegation;
        }

        private void OnProgressChanged(ICancelableProgressState state)
        {
            if (state.GetType().IsAssignableFrom(typeof(E)))
                Report((E)state);
            else
                throw new ArgumentException($"Expecting state of {typeof(E)}");
        }

        public Func<E, T> Delegation { get; set; }

        public IProgress<T> Delegated { get; set; }

        /// <summary>
        /// Outgoing progress change event.
        /// </summary>
        public event OnProgressChangeDelegate OnProgressChange;

        public void Report(E value)
        {
            var transformed = Delegation.Invoke(value);
            Delegated?.Report(transformed);
        }
    }
}
