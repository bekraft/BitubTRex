using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bitub.Dto.Spatial;
using NUnit.Framework;

namespace Bitub.Dto.Tests.Spatial
{
    public class KdNodeTests : TestBase<KdNodeTests>
    {
        [Test]
        public void Absorb()
        {
            var kdtree = new KdRange(1e-6f, 1e-2f);
            // Initial point
            var p1 = new XYZ(1, 0, 0);
            var n1 = kdtree.Append(p1);
            Assert.IsFalse(n1.IsCluster);
            Assert.AreEqual(0, n1.ClusterCount);
            
            // 2nd outside cluster eps
            var p2 = new XYZ(1.01, 0, 0);
            var n2 = kdtree.Append(p2);
            Assert.AreEqual(n2, n1.Right);
            
            // 3rd inside cluster eps of 2nd
            var p3 = new XYZ(1.015, 0, 0);
            var n3 = kdtree.Append(p3);
            Assert.IsTrue(n3.IsCluster);
            Assert.AreEqual(2, n3.ClusterCount);
            Assert.AreEqual(new [] { p3, p2 },n3.ClusterPoints.ToArray());
            
            // 4th inside cluster eps of 2nd and 1st, transitive of 3rd
            var p4 = new XYZ(1.005, 0, 0);
            var n4 = kdtree.Append(p4);
            Assert.IsTrue(n4.IsCluster);
            Assert.AreEqual(4, n4.ClusterCount);
            Assert.AreEqual(2, n4.CoreWeight);
            Assert.AreEqual(new [] { p4, p1, p3, p2 },n4.ClusterPoints.ToArray());
            Assert.AreEqual(new XYZ(1.00874996f, 0, 0), n4.Center);
        }

        [Test]
        public void Propagate()
        {
            var kdtree = new KdRange(1e-6f, 1e-2f);
            
            // Initial point
            var p1 = new XYZ(1, 0, 0);
            var n1 = kdtree.Append(p1);
            Assert.AreEqual(n1, kdtree.Root);
            Assert.AreEqual(0, n1.Dim);
         
            // 2nd 
            var p2 = new XYZ(1.01, 0, 0);
            var n2 = n1.Propagate(p2, 1e-2f);
            Assert.AreEqual(n2, n1.Right);
            Assert.AreEqual((double)0.01f, (double)n1.RMin, 1e-5);
            Assert.AreEqual(1, n2.Dim);
            
            // 3rd 
            var p3 = new XYZ(1.015, 0, 0);
            var n3 = n1.Propagate(p3, 1e-2f);
            Assert.AreEqual(n3, n2.Left);
            Assert.AreEqual(0, n2.LMin);
            Assert.AreEqual(2, n3.Dim);
            
            // 4th
            var p4 = new XYZ(1.005, 0, 0);
            var n4 = n1.Propagate(p4, 1e-2f);
            Assert.AreEqual(n1, n4);
        }
    }
}