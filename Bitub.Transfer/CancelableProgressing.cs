using System;
using System.Threading;

using Bitub.Transfer;

namespace Bitub.Transfer
{
    public sealed class CancelableProgressing : ICancelableProgressing<ICancelableProgressState>
    {
        #region Internals
        private IProgress<ICancelableProgressState> _progressObserver;
        private readonly CancelableProgressStateToken _state;

        private EventHandler<ICancelableProgressState> _progressEventDelegate;
        private EventHandler<ICancelableProgressState> _finishedEventDelegate;
        private EventHandler<ICancelableProgressState> _canceledEventDelegate;
        #endregion

        /// <summary>
        /// The associated progress state token.
        /// </summary>
        public ICancelableProgressState State { get => _state; }

        /// <summary>
        /// The wrapped progress reporter.
        /// </summary>
        public IProgress<ICancelableProgressState> ProgressObserver
        {
            get {
                lock (State)
                    return _progressObserver;
            }
            set {
                lock (State)
                    _progressObserver = value;
            }
        }

        /// <summary>
        /// Send whenever a progress has been changed.
        /// </summary>
        public event EventHandler<ICancelableProgressState> OnProgressChange
        {
            add {
                lock (State)
                    _progressEventDelegate += value;
            }
            remove {
                lock (State)
                    _progressEventDelegate -= value;
            }
        }

        /// <summary>
        /// Send whenever a progress has been finished.
        /// </summary>
        public event EventHandler<ICancelableProgressState> OnProgressFinished
        {
            add {
                lock (State)
                    _finishedEventDelegate += value;
            }
            remove {
                lock (State)
                    _finishedEventDelegate -= value;
            }
        }

        /// <summary>
        /// Send whenever a progress has been canceled.
        /// </summary>
        public event EventHandler<ICancelableProgressState> OnCanceledProgress
        {
            add {
                lock (State)
                    _canceledEventDelegate += value;
            }
            remove {
                lock (State)
                    _canceledEventDelegate -= value;
            }
        }

        /// <summary>
        /// A new cancelable progress.
        /// </summary>
        /// <param name="receiver">A report receiver</param>
        /// <param name="totalEffort">The total effert to be done</param>
        /// <param name="isCancelable">True, if cancelable progress</param>
        public CancelableProgressing(IProgress<ICancelableProgressState> receiver, bool isCancelable, long totalEffort = 100 )
        {
            ProgressObserver = receiver;
            _state = new CancelableProgressStateToken(isCancelable, totalEffort);
        }

        public void NotifyProgress(string message)
        {
            _state.Update(_state.Done, message);
            EventHandler<ICancelableProgressState> delegates;
            lock (State)
            {
                ProgressObserver?.Report(State);
                delegates = _progressEventDelegate;
            }
            // Raise async event
            delegates.RaiseAsync(this, _state);
        }

        public void NotifyFinish(string message = null)
        {
            EventHandler<ICancelableProgressState> progressDelegates, finishedDelegates;
            lock (State)
            {
                progressDelegates = _progressEventDelegate;
                finishedDelegates = _finishedEventDelegate;
            }
            // Update and raise async event
            _state.Update(_state.Total, message);
            progressDelegates.RaiseAsync(this, _state);
            finishedDelegates.RaiseAsync(this, _state);
        }

        public void NotifyProgress(long incrementDone = 1)
        {
            _state.Increment(incrementDone);
            EventHandler<ICancelableProgressState> delegates;
            lock (State)
            {
                ProgressObserver?.Report(State);
                delegates = _progressEventDelegate;
            }
            // Raise async event
            delegates.RaiseAsync(this, _state);
        }

        public void Cancel()
        {
            _state.MarkCanceled();
            EventHandler<ICancelableProgressState> delegates;
            lock (State)
            {
                ProgressObserver?.Report(State);
                delegates = _canceledEventDelegate;
            }
            // Raise async event
            delegates.RaiseAsync(this, _state);
        }
    }
}
