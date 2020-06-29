using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xbim.Common.Metadata;
using Xbim.Ifc;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.SharedBldgElements;

using Bitub.Ifc.Transform;
using System.Xml.Linq;

namespace Bitub.Ifc.Tests
{
    [TestClass]
    public class IfcModelInjectorWorkerTests : BaseTest<IfcModelInjectorWorkerTests>
    {
        private readonly IEnumerable<XName> CopyType = new XName[] { typeof(IfcSlab).XLabel() };

        [TestInitialize]
        public void StartUp()
        {
            StartUpLogging();
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Storey-With-4Walls.ifc")]
        [DeploymentItem(@"Resources\Ifc4-Storey-With-Slab.ifc")]
        public void IfcModelInjectionTests()
        {
            using (var targetStore = IfcStore.Open(@"Resources\Ifc4-Storey-With-4Walls.ifc"))
            using (var sourceStore = IfcStore.Open(@"Resources\Ifc4-Storey-With-Slab.ifc"))
            {
                var container = targetStore.Instances.Where<IfcBuildingStorey>(p => p.GlobalId == "3sQDoA6fn17QZIkXQPg8aQ").First();
                Assert.IsNotNull(container);

                var worker = new IfcModelInjectorWorker(targetStore, CopyType, new IfcStore[] { sourceStore }, container);

                Assert.AreEqual(container, worker.ContainerCandidate.FirstOrDefault());
                Assert.AreEqual(IfcModelInjectorMode.SingletonContainer, worker.InjectorMode);
                worker.RunModelMerge( (i,s) => TestLogger.LogInformation($"... at ${i} (${s ?? ""})"));

                var inserted = targetStore.Instances.OfType<IfcSlab>().FirstOrDefault();
                Assert.IsNotNull(inserted);
            }
        }
    }
}
