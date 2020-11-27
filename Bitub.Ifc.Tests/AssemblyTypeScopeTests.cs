using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

using Xbim.IO;

using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Common.Step21;

using Bitub.Dto;
using Bitub.Ifc;
using Bitub.Ifc.Concept;

namespace Bitub.Ifc.Tests
{
    [TestClass]
    public class AssemblyTypeScopeTests : BaseTests<AssemblyTypeScopeTests>
    {
        [TestMethod]
        public void Ifc4IfcWallTest()
        {
            using (var store = IfcStore.Create(XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel))
            using (var tx = store.BeginTransaction())
            {
                var wall = store.Instances.New<Xbim.Ifc4.SharedBldgElements.IfcWall>();
                var assertedName = new[] { "Ifc4", "IfcWall" }.ToQualifier();
                Assert.AreEqual(assertedName, wall.ToQualifiedName());
                tx.Commit();
            }
        }

        [TestMethod]
        public void Ifc2x3AssemblyTest()
        {
            var ifc2x3 = IfcAssemblyScope.Ifc2x3;
            var wallScope = ifc2x3.GetScopeOf<IIfcWall>();
            var wallExpressTypes = ifc2x3.metadata.ExpressTypesImplementing(typeof(IIfcWall));

            Assert.AreEqual(wallScope.Types.Count(), wallExpressTypes.Count());
            var fromScope = wallScope.TypeQualifiers.ToArray();
            var fromMetadata = wallExpressTypes.Select(e => XbimSchemaVersion.Ifc2X3.ToQualifiedName(e)).ToArray();

            Assert.IsTrue(Enumerable.SequenceEqual(fromScope, fromMetadata));
        }
    }
}
