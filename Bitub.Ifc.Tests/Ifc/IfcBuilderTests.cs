using Microsoft.VisualStudio.TestTools.UnitTesting;

using Bitub.Ifc;

using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.Ifc4.Interfaces;
using System.Linq;

namespace Bitub.Ifc.Tests
{
    [TestClass]
    public class IfcBuilderTests : BaseTests<IfcBuilderTests>
    {
        [TestMethod]
        public void Ifc2x3BuilderTests()
        {
            using (var store = IfcStore.Create(XbimSchemaVersion.Ifc2X3, XbimStoreType.InMemoryModel))
            {
                var builder = new Ifc2x3Builder(store, LoggerFactory);
                Assert.IsNotNull(builder.NewSite("Some building"));
                Assert.IsNotNull(builder.NewBuilding("Some building"));
                Assert.IsNotNull(builder.NewStorey("Some building"));

                var globalPlacement = builder.NewLocalPlacement(new Xbim.Common.Geometry.XbimVector3D());
                Assert.IsNotNull(builder.NewProduct<IIfcWallStandardCase>(globalPlacement));

                Assert.AreEqual(4, store.Instances.OfType<IIfcProduct>().Count());
                Assert.AreEqual(1, store.Instances.OfType<IIfcLocalPlacement>().Count());
                Assert.AreEqual(1, store.Instances.OfType<IIfcRelContainedInSpatialStructure>().Count());
                Assert.AreEqual(3, store.Instances.OfType<IIfcRelAggregates>().Count());
            }
        }

        [TestMethod]
        public void Ifc4BuilderTests()
        {
            using (var store = IfcStore.Create(XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel))
            {
                var builder = new Ifc4Builder(store, LoggerFactory);
                Assert.IsNotNull(builder.NewSite("Some building"));
                Assert.IsNotNull(builder.NewBuilding("Some building"));
                Assert.IsNotNull(builder.NewStorey("Some building"));

                var globalPlacement = builder.NewLocalPlacement(new Xbim.Common.Geometry.XbimVector3D());
                Assert.IsNotNull(builder.NewProduct<IIfcWallStandardCase>(globalPlacement));

                Assert.AreEqual(4, store.Instances.OfType<IIfcProduct>().Count());
                Assert.AreEqual(1, store.Instances.OfType<IIfcLocalPlacement>().Count());
                Assert.AreEqual(1, store.Instances.OfType<IIfcRelContainedInSpatialStructure>().Count());
                Assert.AreEqual(3, store.Instances.OfType<IIfcRelAggregates>().Count());
            }
        }

    }
}
