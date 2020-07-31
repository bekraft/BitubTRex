using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

using Xbim.Ifc;

using Bitub.Ifc.Validation;

namespace Bitub.Ifc.Tests
{
    [TestClass]
    public class IfcValidationTests : BaseTest<IfcValidationTests>
    {
        [TestInitialize]
        public void StartUp()
        {
            StartUpLogging();
        }

        [DeploymentItem(@"Resources\Ifc2x3-Slab-BooleanResult.ifc")]
        [TestMethod]
        public void SchemaValidateTest()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\Ifc2x3-Slab-BooleanResult.ifc"))
            {
                var validationStamp = IfcSchemaValidationStamp.OfModel(source,
                    Xbim.Common.Enumerations.ValidationFlags.Properties | Xbim.Common.Enumerations.ValidationFlags.Inverses);

                var lookUp = validationStamp.InstanceResults;
                
                Assert.AreEqual(1, lookUp.Count);

                Assert.IsTrue(validationStamp.IsConstraintToSchema);
                Assert.IsFalse(validationStamp.IsCompliantToSchema);

                var results = lookUp[new Xbim.Common.XbimInstanceHandle(source.Instances[176464])];
                Assert.AreEqual(1, results.Count());

                Assert.IsFalse(validationStamp.Diff(validationStamp).Any());
                Assert.IsTrue(validationStamp.Equals(validationStamp));
            }
        }

        [DeploymentItem(@"Resources\Ifc2x3-Slab-BooleanResult.ifc")]
        [TestMethod]
        public void SchemaConstraintTest()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\Ifc2x3-Slab-BooleanResult.ifc"))
            {
                var validationStamp = IfcSchemaValidationStamp.OfModel(source,
                    Xbim.Common.Enumerations.ValidationFlags.TypeWhereClauses | Xbim.Common.Enumerations.ValidationFlags.EntityWhereClauses);

                var lookUp = validationStamp.InstanceResults;

                Assert.AreEqual(1, lookUp.Count);

                Assert.IsFalse(validationStamp.IsConstraintToSchema);
                Assert.IsTrue(validationStamp.IsCompliantToSchema);

                var results = lookUp[new Xbim.Common.XbimInstanceHandle(source.Model.Instances[25])];
                Assert.AreEqual(1, results.Count());

                Assert.IsFalse(validationStamp.Diff(validationStamp).Any());
                Assert.IsTrue(validationStamp.Equals(validationStamp));
            }
        }

        [DeploymentItem(@"Resources\Ifc4-Rotated-1st-floor.ifc")]
        [TestMethod]
        public void GeometryConstraintTest()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\Ifc4-Rotated-1st-floor.ifc"))
            {

            }
        }
    }
}
