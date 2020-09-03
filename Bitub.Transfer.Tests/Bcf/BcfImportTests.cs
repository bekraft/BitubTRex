using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitub.Transfer.Tests;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Bitub.Transfer.Bcf.Tests
{
    [TestClass]
    public class BcfImportTests : BaseTest<BcfImportTests>
    {
        [TestInitialize]
        public void StartUp()
        {
            base.StartUpLogging();
        }

        [TestMethod]
        public void Test()
        {
            var regex = new Regex("(?<name>[^:]*(?=:))+|(?<name>[^:]*(?!:)$)", RegexOptions.ExplicitCapture);
            foreach (Match m in regex.Matches("Furniture_Desk:1525x762mm:287689"))
            {
                var captures = m.Groups["name"].Captures;
                foreach (Capture c in captures)
                {
                    Logger.LogInformation($"{m.Index}:{c.Index} = {c.Value}");
                }
            }
        }
    }
}
