using System;

namespace Bitub.Transfer
{
    /// <summary>
    /// Cancelable progressing emitter. Will raise event asyncronously to receivers.
    /// <para>Provides two separate ways of propagating events. It uses a single <see cref="IProgress{T}"/> callback and 
    /// addtionally three additional event sources defined by <see cref="ICancelableProgressing{T}"/></para>
    /// </summary>
    public sealed class CancelableProgressing : ICancelableProgressing, IDisposable
    {
        #region Internals
        private IProgress<ProgressStateToken> _progressObserver;
        private readonly ProgressStateToken _state;

        private EventHandler<ProgressStateToken> _progressEventDelegate;
        private EventHandler<ProgressStateToken> _onProgressEndDelegates;
        private EventHandler<ProgressStateToken> _cancellingEventDelegate;
        #endregion

        /// <summary>
        /// The sender of emitted events.
        /// </summary>
        public readonly object Sender;

        /// <summary>
        /// The associated progress state token.
        /// </summary>
        public ProgressStateToken State { get => _state; }

        /// <summary>
        /// The wrapped progress reporter.
        /// </summary>
        public IProgress<ProgressStateToken> ProgressObserver
        {
            get {
                lock (_state)
                    return _progressObserver;
            }
            set {
                lock (_state)
                    _progressObserver = value;
            }
        }

        /// <summary>
        /// Send whenever a progress has been changed.
        /// </summary>
        public event EventHandler<ProgressStateToken> OnProgressChange
        {
            add {
                lock (_state)
                    _progressEventDelegate += value;
            }
            remove {
                lock (_state)
                    _progressEventDelegate -= value;
            }
        }

        /// <summary>
        /// Send whenever a progress has been finished.
        /// </summary>
        public event EventHandler<ProgressStateToken> OnProgressEnd
        {
            add {
                lock (_state)
                    _onProgressEndDelegates += value;
            }
            remove {
                lock (_state)
                    _onProgressEndDelegates -= value;
            }
        }

        /// <summary>
        /// Send whenever a progress has been canceled.
        /// </summary>
        public event EventHandler<ProgressStateToken> OnCancellingProgress
        {
            add {
                lock (_state)
                    _cancellingEventDelegate += value;
            }
            remove {
                lock (_state)
                    _cancellingEventDelegate -= value;
            }
        }

        /// <summary>
        /// A new cancelable progress event emitter.
        /// </summary>
        /// <param name="isCancelable">True, if cancelable progress</param>
        public CancelableProgressing(bool isCancelable) : this(null, isCancelable)
        {
        }

        /// <summary>
        /// A new cancelable progress initially attached to a progress callback.
        /// </summary>
        /// <param name="sender">The sender of emitted events</param>
        /// <param name="isCancelable">True, if cancelable progress</param>
        public CancelableProgressing(object sender, bool isCancelable)
        {
            Sender = sender ?? this;
            _state = new ProgressStateToken(isCancelable, 1);
        }

        public void NotifyOnProgressChange()
        {
            lock (_state) // sync on handler changes
            {
                if (null != ProgressObserver)
                {
                    Action<ProgressStateToken> observer = (state) => ProgressObserver.Report(state);
                    observer.RaiseAsync(_state);
                }
                _progressEventDelegate.RaiseAsync(this, _state);
            }
        }

        public void NotifyOnProgressChange(object stateObject)
        {
            _state.UpdateDone(_state.Done, stateObject);
            NotifyOnProgressChange();
        }

        public void NotifyOnProgressChange(long incrementDone, object stateObject)
        {
            _state.IncreaseDone(incrementDone, stateObject);
            NotifyOnProgressChange();
        }

        public void NotifyOnProgressEnd(object stateObject = null)
        {
            _state.UpdateDoneComplete(stateObject);
            NotifyOnProgressChange();
            
            lock (_state)
                _onProgressEndDelegates.RaiseAsync(this, _state);
        }

        public void Cancel()
        {
            _state.MarkCancelling();
            NotifyOnProgressChange();

            lock (_state)
                _cancellingEventDelegate.RaiseAsync(this, _state);
        }

        public long NotifyProgressEstimateChange(long deltaEstimate)
        {
            var increasedEstimate = _state.IncreaseEstimate(deltaEstimate);
            NotifyOnProgressChange();

            return increasedEstimate;
        }

        public long NotifyProgressEstimateUpdate(long newEstimate)
        {
            var total = _state.UpdateEstimate(newEstimate);
            NotifyOnProgressChange();

            return newEstimate;
        }

        public void Dispose()
        {
            _progressEventDelegate = null;
            _cancellingEventDelegate = null;
            _onProgressEndDelegates = null;
            _progressObserver = null;
        }
    }
}
