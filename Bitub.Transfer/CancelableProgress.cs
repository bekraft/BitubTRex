using System;

namespace Bitub.Transfer
{
    /// <summary>
    /// A cancelable progress handler wrapping <see cref="System.IProgress{T}"/>.
    /// </summary>
    public sealed class CancelableProgress : ICancelableProgressing
    {
        /// <summary>
        /// The associated progress state token.
        /// </summary>
        public readonly CancelableProgressStateToken State;

        /// <summary>
        /// The wrapped progress reporter.
        /// </summary>
        public readonly IProgress<ICancelableProgressState> Receiver;

        /// <summary>
        /// Send whenever a progress has been changed.
        /// </summary>
        public event OnProgressChangeDelegate OnProgressChange;

        /// <summary>
        /// Send whenever a progress has been finished.
        /// </summary>
        public event OnProgressFinishedDelegate OnProgressFinished;

        /// <summary>
        /// Send whenever a progress has been canceled.
        /// </summary>
        public event OnProgressCanceledDelegate OnCanceledProgress;

        /// <summary>
        /// A new cancelable progress.
        /// </summary>
        /// <param name="receiver">A report receiver</param>
        /// <param name="totalEffort">The total effert to be done</param>
        public CancelableProgress(IProgress<ICancelableProgressState> receiver, long totalEffort = 100)
        {
            Receiver = receiver;
            State = new CancelableProgressStateToken(true, totalEffort);
        }

        public void NotifyProgress(string message)
        {
            State.Update(State.Done, message);
            Receiver?.Report(State);
            OnProgressChange?.Invoke(State);
        }

        public void NotifyFinish(string message = null)
        {
            OnProgressChange?.Invoke(State.Update(State.Total, message));
            OnProgressFinished?.Invoke(State);
        }

        public void NotifyProgress(long incrementDone = 1)
        {
            State.Increment(incrementDone);
            Receiver?.Report(State);
        }

        public void Cancel()
        {
            State.MarkCanceled();
            OnCanceledProgress?.Invoke(State);
        }
    }
}
