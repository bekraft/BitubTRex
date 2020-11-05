using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

using Xbim.Common.Step21;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

using Xbim.IO;

using System.Xml.Linq;

namespace Bitub.Ifc.Tests
{
    [TestClass]
    public class RegisteredTypeFactoryTests : BaseTests<RegisteredTypeFactoryTests>
    {
        [TestMethod]
        public void ToXNameTests()
        {
            using (var store = IfcStore.Create(XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel))
            using (var tx = store.BeginTransaction())
            {
                var wall = store.Instances.New<Xbim.Ifc4.SharedBldgElements.IfcWall>();
                XName assertedName = "{IFC4}IFCWALL";
                Assert.AreEqual(assertedName, wall.ToXName());
                Assert.IsTrue(typeof(IIfcWall).IsAssignableFrom(wall.GetType()));
                tx.Commit();
            }
        }

        [TestMethod]
        public void ToIfcWallScopeTests()
        {
            var rtf = new RegisteredTypeFactory(typeof(Xbim.Ifc4.EntityFactoryIfc4).Assembly, typeof(Xbim.Ifc2x3.EntityFactoryIfc2x3).Assembly);
            var wallScope = rtf.GetScopeOf<IIfcWall>();

            IsSameArrayElements(new string[] { "Xbim.Ifc4", "Xbim.Ifc2x3" }, rtf.TypeSpaces.ToArray());
            Assert.AreEqual(2, rtf.TypeSpaces.Count());
            Assert.AreEqual(5, wallScope.Implementations.Count());

            var wallScopeIfc4 = rtf.GetScopeOf<IIfcWall>("Xbim.Ifc4");
            Assert.AreEqual(3, wallScopeIfc4.Implementations.Count());
        }
    }
}
