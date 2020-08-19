using Microsoft.Extensions.Logging;

namespace Bitub.Transfer.Tests
{
    public abstract class BaseTest<T>
    {
        protected ILoggerFactory Factory { get; private set; }
        protected ILogger Logger { get; private set; }

        protected void StartUpLogging()
        {
            Factory = new LoggerFactory().AddConsole();
            Logger = Factory.CreateLogger<T>();
            Logger.LogInformation($"Starting up ${GetType()}...");
        }

    }
}
