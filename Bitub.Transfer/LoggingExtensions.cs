using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Bitub.Transfer
{
    public delegate void OnLogDelegate(LogLevel logLevel, object sender, string message, params object[] args);

    /// <summary>
    /// Logging event.
    /// </summary>
    public class LogEventArgs : System.EventArgs
    {
        public LogLevel Level { get; private set; }
        public object Sender { get; private set; }
        public object[] Arguments { get; private set; }
        public string MessageTemplate { get; private set; }

        public LogEventArgs(LogLevel logLevel, object sender, string message, params object[] args)
        {
            Level = logLevel;
            Sender = sender;
            MessageTemplate = message;
            Arguments = args;
        }

        public override bool Equals(object obj)
        {
            return obj is LogEventArgs args &&
                   Level == args.Level &&
                   EqualityComparer<object>.Default.Equals(Sender, args.Sender) &&
                   EqualityComparer<object[]>.Default.Equals(Arguments, args.Arguments) &&
                   MessageTemplate == args.MessageTemplate;
        }

        public override int GetHashCode()
        {
            var hashCode = 395351326;
            hashCode = hashCode * -1521134295 + Level.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Sender);
            hashCode = hashCode * -1521134295 + EqualityComparer<object[]>.Default.GetHashCode(Arguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MessageTemplate);
            return hashCode;
        }
    }

    /// <summary>
    /// Logging source implementing an event emitter.
    /// </summary>
    public interface ILogSource
    {        
        event OnLogDelegate OnLog;        
    }    
}
