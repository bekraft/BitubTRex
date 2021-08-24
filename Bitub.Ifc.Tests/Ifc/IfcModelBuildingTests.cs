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
            var builder = IfcBuilder.WithNewProject("Test", EditorCredentials, XbimSchemaVersion.Ifc2X3, LoggerFactory);
            Assert.IsNotNull(builder.NewSite("Some building"));
            Assert.IsNotNull(builder.NewBuilding("Some building"));
            Assert.IsNotNull(builder.NewStorey("Some building"));

            var globalPlacement = builder.NewLocalPlacement(new Xbim.Common.Geometry.XbimVector3D());
            Assert.IsNotNull(builder.NewProduct<IIfcWallStandardCase>(globalPlacement));

            Assert.AreEqual(4, builder.model.Instances.OfType<IIfcProduct>().Count());
            Assert.AreEqual(1, builder.model.Instances.OfType<IIfcLocalPlacement>().Count());
            Assert.AreEqual(1, builder.model.Instances.OfType<IIfcRelContainedInSpatialStructure>().Count());
            Assert.AreEqual(3, builder.model.Instances.OfType<IIfcRelAggregates>().Count());
        }

        [TestMethod]
        public void BuildingIFC4()
        {
            var builder = IfcBuilder.WithNewProject("Test", EditorCredentials, XbimSchemaVersion.Ifc4, LoggerFactory);
            Assert.IsNotNull(builder.NewSite("Some building"));
            Assert.IsNotNull(builder.NewBuilding("Some building"));
            Assert.IsNotNull(builder.NewStorey("Some building"));

            var globalPlacement = builder.NewLocalPlacement(new Xbim.Common.Geometry.XbimVector3D());
            Assert.IsNotNull(builder.NewProduct<IIfcWallStandardCase>(globalPlacement));

            Assert.AreEqual(4, builder.model.Instances.OfType<IIfcProduct>().Count());
            Assert.AreEqual(1, builder.model.Instances.OfType<IIfcLocalPlacement>().Count());
            Assert.AreEqual(1, builder.model.Instances.OfType<IIfcRelContainedInSpatialStructure>().Count());
            Assert.AreEqual(3, builder.model.Instances.OfType<IIfcRelAggregates>().Count());
        }

        [TestMethod]
        public void BuildingIFC4_Relations()
        {
            var builder = IfcBuilder.WithNewProject("Test", EditorCredentials, XbimSchemaVersion.Ifc2X3, LoggerFactory);
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
