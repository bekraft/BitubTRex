using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Linq;

using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Ifc;

namespace Bitub.Ifc.Tests
{
    [TestClass]
    public class XbimRegressionTests : TestBase<XbimRegressionTests>
    {
        [DeploymentItem(@"Resources\Ifc4-Storey-With-4Walls.ifc")]
        [TestMethod]
        public void IfcStoreInsertCopyCloneTests()
        {
            using (var store = IfcStore.Open(@"Resources\Ifc4-Storey-With-4Walls.ifc"))
            {
                var testStore = IfcStore.Create(store.SchemaVersion, Xbim.IO.XbimStoreType.InMemoryModel);
                var map = new XbimInstanceHandleMap(store, testStore);

                using (var tx = testStore.BeginTransaction("Test"))
                {
                    foreach (var e in store.Instances)
                    {
                        Assert.IsNotNull(testStore.InsertCopy(e, map, (m, p) => m.PropertyInfo.GetValue(p), true, true));
                    }
                    tx.Commit();
                }
                Assert.IsTrue(store.Instances.All(i => map.Keys.Count(k => k.EntityLabel == i.EntityLabel) == 1));
            }
        }

        [TestMethod]
        public void QuaternionAndMatrixOrientationTests()
        {
            var t = new XbimMatrix3D(new XbimVector3D(1, 1, 0));
            t.RotateAroundZAxis(Math.PI / 2);
            // Left chaining matrix operation => transposed in Xbim
            Assert.AreEqual(new XbimVector3D(0, 1, 0), t.Right);
            Assert.AreEqual(new XbimVector3D(-1, 0, 0), t.Up);
            Assert.AreEqual(new XbimVector3D(0, 0, 1), t.Backward);

            var q = t.GetRotationQuaternion();
            Assert.AreEqual(Math.Sqrt(2) / 2, q.W, 1e-5);
            Assert.AreEqual(Math.Sqrt(2) / 2, q.Z, 1e-5);
            Assert.AreEqual(0, q.X, 1e-8);
            Assert.AreEqual(0, q.Y, 1e-8);
        }

        [TestMethod]
        public void QuaternionVersusMatrixMovingTests()
        {
            XbimVector3D t = new XbimVector3D(2, 0, 0);
            XbimMatrix3D T = new XbimMatrix3D(t);
            T.RotateAroundZAxis(Math.PI / 2);
            XbimQuaternion q;
            XbimQuaternion.RotationMatrix(ref T, out q);

            // Do the quaternion approach
            XbimVector3D v = new XbimVector3D(1, 1, 0);
            XbimVector3D v1;
            XbimQuaternion.Transform(ref v, ref q, out v1);
            // Rotate and translate
            var p1Result = v1 + t;

            // Do the same with matrix approach
            XbimPoint3D p = new XbimPoint3D(1, 1, 0);
            var p2Result = T.Transform(p);

            Assert.AreEqual(p1Result, XbimPoint3D.Subtract(p2Result, XbimPoint3D.Zero));

            // Test quaternion and delta translation (relocating)
            XbimVector3D mt = new XbimVector3D(2, 2, 0);
            var dt = t - mt;
            var pMoved = v1 + dt;
            var p3Result = pMoved + mt;
            
            Assert.AreEqual(p1Result, p3Result);
        }

    }
}
