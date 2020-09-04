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
        public void ImportBcfZip()
        {
        }

        [TestMethod]
        public void ImportBcf()
        {
        }

    }
}
