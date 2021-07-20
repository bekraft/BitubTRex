using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Bitub.Dto.Cpi.Tests
{
    public abstract class TestBase<T>
    {
        protected readonly ILogger<T> logger;

        private static ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        protected TestBase()
        {
            logger = loggerFactory.CreateLogger<T>();
        }

        protected Stream GetEmbeddedFileStream(string resourceName)
        {
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            return Assembly.GetExecutingAssembly().GetManifestResourceStream($"{name}.Resources.{resourceName}");
        }

        protected string GetUtf8TextFrom(string resourceName)
        {
            using (var fs = GetEmbeddedFileStream(resourceName))
            {
                return GetUtf8TextFrom(fs);
            }
        }

        protected string GetUtf8TextFrom(Stream binStream)
        {
            using (var sr = new StreamReader(binStream, System.Text.Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        protected string ResolveFilename(string localPath)
        {
            return Path.Combine(ExecutingFullpath, localPath);
        }

        protected string ExecutingFullpath
        {
            get
            {
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(assemblyLocation);
            }
        }
    }
}
