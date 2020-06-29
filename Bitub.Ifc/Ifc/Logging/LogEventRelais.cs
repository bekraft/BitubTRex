using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Bitub.Ifc.Logging
{
    public class LogEventRelais
    {
        public ILogger Logger { get; private set; }
        public ISet<object> ExcludeFilter { get; private set; }

        public LogEventRelais(ILoggerFactory loggerFactory, string categoryName)
        {
            Logger = loggerFactory.CreateLogger(categoryName);
            ExcludeFilter = new HashSet<object>();
        }

        public void OnLog(LogLevel logLevel, object sender, string message, params object[] args)
        {
            bool isExcluded;
            lock (ExcludeFilter)
                isExcluded = ExcludeFilter.Contains(sender);

            if (!isExcluded)
                Logger.Log(logLevel, $"({sender?.GetType()}): {message}", args);
        }
    }
}
