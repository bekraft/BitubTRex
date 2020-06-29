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
    /// Informs on progress changes.
    /// </summary>
    /// <param name="changedState">The current state</param>
    public delegate void OnProgressChangeDelegate(ICancelableProgressState changedState);

    /// <summary>
    /// Informs on progress cancelation.
    /// </summary>
    /// <param name="canceledState">The most recent state</param>
    public delegate void OnProgressCanceledDelegate(IProgressState canceledState);

    /// <summary>
    /// Informs on progress finishing.
    /// </summary>
    /// <param name="finalState">The last state before finishing</param>
    public delegate void OnProgressFinishedDelegate(IProgressState finalState);

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
    public interface IProgressing
    {
        event OnProgressChangeDelegate OnProgressChange;
        event OnProgressFinishedDelegate OnProgressFinished;
    }

    /// <summary>
    /// A cancelable progress announcer.
    /// </summary>
    public interface ICancelable
    {
        event OnProgressCanceledDelegate OnCanceledProgress;        
        void Cancel();
    }

    public interface ICancelableProgressing : ICancelable, IProgressing
    {
    }
}
