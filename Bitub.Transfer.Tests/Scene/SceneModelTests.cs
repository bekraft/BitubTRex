using Microsoft.VisualStudio.TestTools.UnitTesting;

using Bitub.Transfer.Scene;
using Bitub.Transfer.Spatial;
using Bitub.Transfer.Tests;

namespace Bitub.Transfer.Scene.Tests
{
    [TestClass]
    public class SceneModelTests : BaseTest<SceneModelTests>
    {
        const string tExample1 = "{ \"q\": { \"x\": 0.25, \"y\": 0, \"z\": 0, \"w\": 1 }, \"t\": { \"x\": 38.41, \"y\": 0.7, \"z\": 62.75 } }";

        [TestInitialize]
        public void StartUp()
        {
            base.StartUpLogging();
        }

        [TestMethod]
        public void TransformIdentityEqualityTests()
        {
            var t1 = new Transform
            {
                Q = new Quaternion { X = 0, Y = 0, Z = 0, W = 1 },
                T = new XYZ { X = 0, Y = 0, Z = 0 }
            };
            var t2 = new Transform
            {
                Q = new Quaternion { X = 0, Y = 0, Z = 0, W = 1 },
                T = new XYZ { X = 0, Y = 0, Z = 0 }
            };

            Assert.AreEqual(t1, t2);

            t1 = Transform.Parser.ParseJson(tExample1);
            t2 = Transform.Parser.ParseJson(tExample1);

            Assert.AreEqual(t1, t2);
        }

        [TestMethod]
        public void TransformParsedEqualityTests()
        {
            var t1 = Transform.Parser.ParseJson(tExample1);
            var t2 = Transform.Parser.ParseJson(tExample1);

            Assert.AreEqual(t1, t2);
        }

    }
}
