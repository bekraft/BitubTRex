﻿using Microsoft.Extensions.Logging;

namespace Bitub.Dto.Bcf.Tests
{
    public abstract class BaseTest<T>
    {
        protected ILoggerFactory TestLoggerFactory { get; private set; }
        protected ILogger TestLogger { get; private set; }

        protected void StartUpLogging()
        {
            TestLoggerFactory = new LoggerFactory().AddConsole();
            TestLogger = TestLoggerFactory.CreateLogger<T>();
            TestLogger.LogInformation($"Starting up {GetType()}...");
        }

    }
}
