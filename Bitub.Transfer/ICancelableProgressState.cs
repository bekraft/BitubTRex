using System;

namespace Bitub.Transfer
{
    [Flags]
    public enum ProgressTokenState
    {
        IsRunning = 0,
        IsCancelableRunning = 1,
        IsCanceled = 2,
        IsInterupted = 4,
        IsBroken = 6,
    }

    /// <summary>
    /// A cancelable progress state
    /// </summary>
    public interface ICancelableProgressState : IProgressState
    {        
        void MarkCanceled();
        bool IsCanceled { get; }
    }

    /// <summary>
    /// A progressing state
    /// </summary>
    public interface IProgressState
    {
        ProgressTokenState State { get; }
        void MarkInterupted();
        bool IsBroken { get; }
        int Percentage { get; }
        object StateObject { get; }
    }

    /// <summary>
    /// A progressing process
    /// </summary>
    public interface IProgressing<T> where T : IProgressState
    {
        event EventHandler<T> OnProgressChange;
        event EventHandler<T> OnProgressFinished;
    }

    /// <summary>
    /// A cancelable progress announcer.
    /// </summary>
    public interface ICancelable<T> where T : IProgressState
    {
        event EventHandler<T> OnCanceledProgress;
        void Cancel();
    }

    public interface ICancelableProgressing<T> : ICancelable<T>, IProgressing<T> where T : IProgressState
    {
    }
}
