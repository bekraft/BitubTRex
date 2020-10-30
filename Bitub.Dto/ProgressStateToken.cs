using System;
using System.Threading;

namespace Bitub.Dto
{
    /// <summary>
    /// A shared implementation of cancelable progress state.
    /// </summary>
    public sealed class ProgressStateToken : EventArgs
    {
        #region Internals
        private object _monitor = new object();

        private ProgressTokenState __state = 0;
        private long __done;
        private long __total;
        private object __stateObject;
        #endregion

        /// <summary>
        /// A new progress state token.
        /// </summary>
        /// <param name="isCancelable">Whether it's can signal cancellation</param>
        /// <param name="estimateTotal">The estimate total</param>
        /// <param name="stateObject">The initial state object</param>
        public ProgressStateToken(bool isCancelable, long estimateTotal, object stateObject = null)
        {
            __total = estimateTotal;            
            __done = 0;
            __stateObject = stateObject;

            __state = isCancelable ? ProgressTokenState.IsCancelable : 0;
        }

        /// <summary>
        /// A new progress state token.
        /// </summary>
        /// <param name="isCancelable">Whether it's can signal cancellation</param>
        /// <param name="done">The initial done amount</param>
        /// <param name="estimateTotal">The estimate total</param>
        /// <param name="stateObject">The initial state object</param>
        public ProgressStateToken(bool isCancelable, long done, long estimateTotal, string stateObject = null) : this(isCancelable, estimateTotal, stateObject)
        {
            __done = done;
        }

        /// <summary>
        /// The percentage of progress between 0 and 100.
        /// </summary>
        public int Percentage
        {
            get => Math.Max(0, Math.Min(100, (int)Math.Ceiling(100.0 * ((double)Done / Math.Max(1, TotalEstimate)))));
        }

        /// <summary>
        /// The current state object.
        /// </summary>
        public object StateObject
        {
            get {
                lock (_monitor)
                    return __stateObject;
            }
        }

        /// <summary>
        /// Total effort to be done.
        /// </summary>
        public long TotalEstimate
        {
            get => Interlocked.Read(ref __total);            
        }

        /// <summary>
        /// Current effort already done.
        /// </summary>
        public long Done
        {
            get => Interlocked.Read(ref __done);            
        }

        /// <summary>
        /// Gets the current state of progress token.
        /// </summary>
        public ProgressTokenState State
        {
            get {
                lock (_monitor)
                    return __state;
            }
        }

        /// <summary>
        /// Wether the state can signal a cancellation.
        /// </summary>
        public bool IsCancelable
        {
            get => (State & ProgressTokenState.IsCancelable) > 0;
        }

        /// <summary>
        /// Wether the state can signal a cancellation.
        /// </summary>
        public bool IsAboutCancelling
        {
            get => (State & ProgressTokenState.IsCancelling) > 0;
        }

        /// <summary>
        /// Whether the current progress has been canceled
        /// </summary>
        public bool IsCanceled
        {
            get => (State & ProgressTokenState.IsCanceled) > 0;
        }

        /// <summary>
        /// Whether the current progress is broken either by cancelation or interruption.
        /// </summary>
        public bool IsBroken
        {
            get => (State & ProgressTokenState.IsBroken) > 0;
        }

        /// <summary>
        /// True, if the current progress has got interruptions (but may still be busy).
        /// </summary>
        public bool HasErrors
        {
            get => (State & ProgressTokenState.IsInterupted) > 0;
        }

        /// <summary>
        /// True, if the current progress isn't canceled, terminated or broken anyway.
        /// </summary>
        public bool IsAlive
        {
            get => (State & (ProgressTokenState.IsCanceled | ProgressTokenState.IsTerminated | ProgressTokenState.IsBroken)) == 0;
        }

        /// <summary>
        /// Marks the curremt
        /// </summary>
        public void MarkRunning()
        {
            lock (_monitor)
                __state |= ProgressTokenState.IsRunning;
        }

        /// <summary>
        /// Marks the current progress as broken.
        /// </summary>
        public void MarkBroken()
        {
            lock (_monitor)
                __state |= ProgressTokenState.IsBroken;
        }

        /// <summary>
        /// Marks the state to be about cancelling.
        /// </summary>
        public void MarkCancelling()
        {
            lock (_monitor)
            {
                if ((__state & ProgressTokenState.IsCancelable) > 0)
                    __state |= ProgressTokenState.IsCancelling;
                else
                    throw new NotSupportedException($"Progress isn't cancelable in state '{__state}'");
            }
        }

        /// <summary>
        /// Marks the current progress as canceled.
        /// </summary>
        public void MarkCanceled()
        {
            lock (_monitor)
            {
                if ((__state & ProgressTokenState.IsCancelable) > 0)
                    __state |= ProgressTokenState.IsCanceled;
                else
                    throw new NotSupportedException($"Progress isn't cancelable in state '{__state}'");
            }
        }

        /// <summary>
        /// Marks the current progress as interrupted by errors (but maybe still busy).
        /// </summary>
        public void MarkInterupted()
        {
            lock (_monitor)
                __state |= ProgressTokenState.IsInterupted;
        }

        /// <summary>
        /// Marks the curent progress as terminated.
        /// </summary>
        public void MarkTerminated()
        {
            lock (_monitor)
                __state |= ProgressTokenState.IsTerminated;
        }

        /// <summary>
        /// Extends the total effort by given delta.
        /// </summary>
        /// <param name="additionalEffort">To be added to current total</param>
        /// <returns>The new total effort</returns>
        public long IncreaseEstimate(long additionalEffort)
        {            
            return Interlocked.Add(ref __total, additionalEffort);
        }

        /// <summary>
        /// Updates the estimated total.
        /// </summary>
        /// <param name="newEstimatedTotal">New effort estimate as positive value</param>
        /// <returns>The recent estimate</returns>
        public long UpdateEstimate(long newEstimatedTotal)
        {
            return Interlocked.Exchange(ref __total, Math.Max(0, newEstimatedTotal));
        }

        /// <summary>
        /// Sets the progress to total effort.
        /// </summary>
        /// <returns>The total effort done before completed</returns>
        public long UpdateDoneComplete(object stateObject = null)
        {
            if (null != stateObject)
            {
                lock (_monitor)
                    __stateObject = stateObject;
            }
            return Interlocked.Exchange(ref __done, Interlocked.Read(ref __total));
        }

        /// <summary>
        /// Updates the current state by given done efforts and current state description.
        /// </summary>
        /// <param name="absoluteDone">An absolute done effort</param>
        /// <param name="stateObject">The state object</param>
        /// <returns>This instance</returns>
        public ProgressStateToken UpdateDone(long absoluteDone, object stateObject)
        {
            Interlocked.Exchange(ref __done, absoluteDone);
            lock (_monitor)
            {
                __state |= ProgressTokenState.IsRunning;
                __stateObject = stateObject;
            }

            return this;
        }

        /// <summary>
        /// Increment current done effort by given delta increment and current state description.
        /// </summary>
        /// <param name="increment">The increment</param>
        /// <param name="stateObject">The state object</param>
        /// <returns>This instance</returns>
        public ProgressStateToken IncreaseDone(long increment, object stateObject)
        {
            Interlocked.Add(ref __done, increment);
            lock (_monitor)
            {
                __state |= ProgressTokenState.IsRunning;
                __stateObject = stateObject;
            }

            return this;
        }
    }
}
