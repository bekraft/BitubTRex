using System;
using System.Threading;

namespace Bitub.Transfer
{
    /// <summary>
    /// A shared implementation of cancelable progress state.
    /// </summary>
    public class CancelableProgressStateToken : ICancelableProgressState
    {
        #region Internals
        private object _monitor = new object();

        private int __state;
        private long __done;
        private long __total;
        private object __stateObject;
        #endregion

        public CancelableProgressStateToken(bool isCancelable, long total, string s = null)
        {
            __total = total;            
            __done = 0;
            __stateObject = s;

            __state = (int)(isCancelable ? ProgressTokenState.IsCancelableRunning : ProgressTokenState.IsRunning);
        }

        public CancelableProgressStateToken(bool isCancelable, long done, long total, string s = null) : this(isCancelable, total, s)
        {
            __done = done;
        }

        /// <summary>
        /// The percentage of progress between 0 and 100.
        /// </summary>
        public int Percentage
        {
            get => Math.Max(0, Math.Min(100, (int)Math.Ceiling(100.0 * ((double)Done / Total))));
        }

        /// <summary>
        /// The current state object.
        /// </summary>
        public object StateObject
        {
            get {
                lock (_monitor)
                {
                    return __stateObject;
                }
            }
        }

        /// <summary>
        /// Total effort to be done.
        /// </summary>
        public long Total
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
            get => (ProgressTokenState)Interlocked.CompareExchange(ref __state, 0, 0);
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
        /// Force cancelation by user.
        /// </summary>
        public void MarkCanceled()
        {
            if ((State & ProgressTokenState.IsCancelableRunning) > 0)
                Interlocked.Add(ref __state, (int)ProgressTokenState.IsCanceled);
            else
                throw new NotSupportedException($"No cancelable progress support enabled");
        }

        /// <summary>
        /// Mark as interrupted
        /// </summary>
        public void MarkInterupted()
        {
            Interlocked.Add(ref __state, (int)ProgressTokenState.IsInterupted);
        }

        /// <summary>
        /// Extends the total effort by given delta.
        /// </summary>
        /// <param name="addToTotalEffort">To be added to current total</param>
        /// <returns>The new total effort</returns>
        public long IncreaseTotalEffort(long addToTotalEffort)
        {
            return Interlocked.Add(ref __total, addToTotalEffort);
        }

        /// <summary>
        /// Sets the progress to total effort.
        /// </summary>
        /// <returns></returns>
        public long SetProgressComplete()
        {            
            return Interlocked.Exchange(ref __done, Interlocked.Read(ref __total));
        }

        /// <summary>
        /// Updates the current state by given done efforts and current state description.
        /// </summary>
        /// <param name="done">An absolute done effort</param>
        /// <param name="s">The description of state</param>
        /// <returns>This instance</returns>
        public CancelableProgressStateToken Update(long done, string s = null)
        {
            if (IsCanceled)
                throw new NotSupportedException("Progress has been already cancelled");

            Interlocked.Exchange(ref __done, done);
            if (null != s)
            {
                lock (_monitor)
                {
                    __stateObject = s;
                }
            }

            return this;
        }

        /// <summary>
        /// Increment current done effort by given delta increment and current state description.
        /// </summary>
        /// <param name="inc">The increment</param>
        /// <param name="s">The state description</param>
        /// <returns>This instance</returns>
        public CancelableProgressStateToken Increment(long inc = 1, string s = null)
        {
            if (IsCanceled)
                throw new NotSupportedException("Progress has been already cancelled");

            Interlocked.Add(ref __done, inc);
            if (null != s)
            {
                lock (_monitor)
                {
                    __stateObject = s;
                }
            }

            return this;
        }
    }
}
