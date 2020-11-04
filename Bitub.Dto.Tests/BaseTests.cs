using Microsoft.Extensions.Logging;

namespace Bitub.Dto.Tests
{
    public abstract class BaseTests<T>
    {
        protected ILogger<T> logger;

        private static ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        protected void InternallySetup()
        {
            logger = loggerFactory.CreateLogger<T>();
        }
    }
}
