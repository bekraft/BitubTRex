using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Serilog.Core;
using Serilog.Events;

using Bitub.Dto;

namespace Bitub.Ifc.Logging
{
    public class LogEventQueue : ILogEventSink
    {
        #region Internals

        private EventHandler<LogEventArgs> _logHandler;
        private EventHandler<LogEventArgs> _onDrop;

        private int __thresholdCapacity = 10;
        private object _monitor = new object();
        private ConcurrentQueue<LogEventArgs> _logQueue;

        #endregion

        public readonly int MaximumCapacity;

        public event EventHandler<LogEventArgs> OnLogged
        {
            add {
                lock (_monitor)
                    _logHandler += value;
            }
            remove {
                lock (_monitor)
                    _logHandler -= value;
            }
        }

        public event EventHandler<LogEventArgs> OnDropped
        {
            add {
                lock (_monitor)
                    _onDrop += value;
            }
            remove {
                lock (_monitor)
                    _onDrop -= value;
            }
        }

        public IEnumerable<LogEventArgs> LogQueue { get => _logQueue.ToArray(); }
       
        /// <summary>
        /// A new queue with maximum capacity.
        /// </summary>
        /// <param name="maximumCount">Maximum capacity</param>
        public LogEventQueue(int maximumCapacity = int.MaxValue)
        {
            MaximumCapacity = maximumCapacity;
            _logQueue = new ConcurrentQueue<LogEventArgs>();
        }

        public void Emit(LogEvent logEvent)
        {
            if (_logQueue.Count > MaximumCapacity)
            {
                do
                {                    
                    LogEventArgs args;
                    if (_logQueue.TryDequeue(out args))
                    {
                        Task.Factory.FromAsync(
                            (asyncCallback, @object) =>
                                _onDrop.BeginInvoke(this, args, asyncCallback, @object), _onDrop.EndInvoke, null);
                    }
                } while (_logQueue.Count > (MaximumCapacity - __thresholdCapacity));
            }

            //_logQueue.Enqueue(new LogEventArgs(logEvent.Level, logEvent.S))
        }
    }
}
