using System;

namespace Bitub.Transfer
{
    /// <summary>
    /// State of the progressing token.
    /// </summary>
    [Flags]
    public enum ProgressTokenState
    {
        IsRunning = 0x01,
        IsCancelable = 0x02,
        IsCancelling = 0x04,        
        IsInterupted = 0x08,
        IsCanceled = 0x20,
        IsBroken = 0x40,
        IsTerminated = 0x10
    }

    /// <summary>
    /// A progressing process emitting progress announcements.
    /// </summary>
    public interface IProgressing
    {
        event EventHandler<ProgressStateToken> OnProgressChange;
        event EventHandler<ProgressStateToken> OnProgressEnd;
        event EventHandler<ProgressStateToken> OnCancellingProgress;
    }

    /// <summary>
    /// A cancelable progress announcer.
    /// </summary>
    public interface ICancelable
    {
        void Cancel();
    }

    /// <summary>
    /// A cancelable progressing process emitting cancelable progress states.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICancelableProgressing : ICancelable, IProgressing
    {
    }
}
