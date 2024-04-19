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
            Assert.That(n1.IsCluster, Is.False);
            Assert.That(0, Is.EqualTo(n1.ClusterCount));
            
            // 2nd outside cluster eps
            var p2 = new XYZ(1.01, 0, 0);
            var n2 = kdtree.Append(p2);
            Assert.That(n2, Is.EqualTo(n1.Right));
            
            // 3rd inside cluster eps of 2nd
            var p3 = new XYZ(1.015, 0, 0);
            var n3 = kdtree.Append(p3);
            Assert.That(n3.IsCluster, Is.True);
            Assert.That(2, Is.EqualTo(n3.ClusterCount));
            Assert.That(new[] { p3, p2 }, Is.EquivalentTo(n3.ClusterPoints.ToArray()));
            
            // 4th inside cluster eps of 2nd and 1st, transitive of 3rd
            var p4 = new XYZ(1.005, 0, 0);
            var n4 = kdtree.Append(p4);
            Assert.That(n4.IsCluster, Is.True);
            Assert.That(4, Is.EqualTo(n4.ClusterCount));
            Assert.That(2, Is.EqualTo(n4.CoreWeight));
            Assert.That(new[] { p4, p1, p3, p2 }, Is.EquivalentTo(n4.ClusterPoints.ToArray()));
            Assert.That(new XYZ(1.00874996f, 0, 0), Is.EqualTo(n4.Center));
        }

        [Test]
        public void Propagate()
        {
            var kdtree = new KdRange(1e-6f, 1e-2f);
            
            // Initial point
            var p1 = new XYZ(1, 0, 0);
            var n1 = kdtree.Append(p1);
            Assert.That(n1, Is.EqualTo(kdtree.Root));
            Assert.That(0, Is.EqualTo(n1.Dim));
         
            // 2nd 
            var p2 = new XYZ(1.01, 0, 0);
            var n2 = n1.Propagate(p2, 1e-2f);
            Assert.That(n2, Is.EqualTo(n1.Right));
            Assert.That(0.01f, Is.EqualTo(n1.RMin).Within(1e-5f));
            Assert.That(1, Is.EqualTo(n2.Dim));
            
            // 3rd 
            var p3 = new XYZ(1.015, 0, 0);
            var n3 = n1.Propagate(p3, 1e-2f);
            Assert.That(n3, Is.EqualTo(n2.Left));
            Assert.That(0, Is.EqualTo(n2.LMin));
            Assert.That(2, Is.EqualTo(n3.Dim));
            
            // 4th
            var p4 = new XYZ(1.005, 0, 0);
            var n4 = n1.Propagate(p4, 1e-2f);
            Assert.That(n1, Is.EqualTo(n4));
        }
    }
}