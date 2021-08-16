using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

using Bitub.Ifc;

using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.Ifc4.Interfaces;


namespace Bitub.Ifc.Tests
{
    [TestClass]
    public class IfcModelBuildingTests : TestBase<IfcModelBuildingTests>
    {
        [TestMethod]
        public void BuildingIFC2x3()
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
        public void BuildingIFC4()
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

        [TestMethod]
        public void BuildingIFC4_Relations()
        {
            using (var store = IfcStore.Create(XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel))
            {
                var builder = new Ifc4Builder(store, LoggerFactory);
                Assert.IsNotNull(builder.NewSite("Some building"));
                Assert.IsNotNull(builder.NewBuilding("Some building"));
                Assert.IsNotNull(builder.NewStorey("Some building"));

                var globalPlacement = builder.NewLocalPlacement(new Xbim.Common.Geometry.XbimVector3D());
                var fixture1 = builder.NewProduct<IIfcWallStandardCase>(globalPlacement);
                Assert.IsNotNull(fixture1);

                Assert.IsTrue(fixture1.ContainedInStructure.Count() == 1);

                builder.Transactively(m =>
                {
                    var fixture2 = builder.ifcEntityScope.New<IIfcWallStandardCase>(fixture1.GetType(), e =>
                    {
                        e.ObjectPlacement = globalPlacement;
                    });

                    Assert.IsNull(fixture2.IsContainedIn);
                    Assert.IsTrue(fixture2.ContainedInStructure.Count() == 0);

                    fixture2 = fixture2.CreateSameRelationshipsLike(fixture1);
                    Assert.IsNotNull(fixture2);

                    Assert.IsTrue(fixture2.ContainedInStructure.Count() == 1);
                    Assert.AreEqual(fixture1.ContainedInStructure.First().RelatingStructure, fixture2.ContainedInStructure.First().RelatingStructure);
                });
            }
        }
    }
}
