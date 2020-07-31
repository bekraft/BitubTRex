using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitub.Transfer.Tests
{
    [TestClass]
    public class CommonTests : BaseTest<CommonTests>
    {
        [TestInitialize]
        public void StartUp()
        {
            base.StartUpLogging();
        }

        [TestMethod]
        public void QualifierLogicTests()
        {
            var nq1 = new string[] { "A", "Test1" }.ToQualifier();
            var nq2 = new string[] { "A" }.ToQualifier();
            var nq3 = new string[] { "A", "Test1" }.ToQualifier();
            var nq4 = new string[] { "A", "Test2" }.ToQualifier();

            var aq1 = System.Guid.NewGuid().ToQualifier();

            Assert.IsTrue(nq2.IsSuperQualifierOf(nq1));
            Assert.IsFalse(nq1.IsSuperQualifierOf(nq2));

            Assert.AreEqual(nq1, nq3);

            var q1 = nq1.ToSubQualifierOf(nq2);
            Assert.IsTrue(nq1.IsCompliantTo(q1));
            Assert.AreEqual(1, q1.Named.Frags.Count);
            Assert.AreEqual("Test1", q1.Named.Frags[0]);

            var q2 = nq1.ToCommonRoot(nq4);
            Assert.IsTrue(nq1.IsCompliantTo(q2));
            Assert.AreEqual(1, q2.Named.Frags.Count);
            Assert.AreEqual("A", q2.Named.Frags[0]);
        }

        [TestMethod]
        public void ClassifierLogicTests()
        {
            var nc1 = new string[] { "A", "Test1" }.ToQualifier().ToClassifier();
            var nq2 = new string[] { "A" }.ToQualifier();
            var nq3 = new string[] { "Test1" }.ToQualifier();

            var q1 = nc1.ToFilteredSubQualifiers(nq2).ToArray();
            Assert.AreEqual(1, q1.Length);
            Assert.AreEqual(nq3, q1[0]);
        }
    }
}
