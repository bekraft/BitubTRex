using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

using Microsoft.VisualStudio.TestTools.UnitTesting;


using Bitub.Ifc.Transform.Requests;
using Bitub.Ifc.Validation;

namespace Bitub.Ifc.Tests.Transform
{
    [TestClass]
    public class RepresentationReplaceRequestTests : TestBase<RepresentationReplaceRequestTests>
    {

        private static bool IsMultiRepresentation(IIfcProduct product, params string[] contexts)
        {
            return product.Representation.Representations
                .Where(r => contexts.Contains(r.ContextOfItems.ContextIdentifier.ToString()))
                .Any(r => r.Items.Count() > 1);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-MultipleBodiesPerProduct.ifc")]
        public async Task RefactorOnly()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\Ifc4-MultipleBodiesPerProduct.ifc"))
            {
                var stampBefore = SchemaValidator.OfModel(source);
                Assert.IsTrue(stampBefore.IsCompliantToSchema);

                var transform = new ProductRepresentationRefactorTransform(LoggerFactory)
                {
                    ContextIdentifiers = new[] { "Body" },
                    Strategy = ProductRepresentationRefactorStrategy.ReplaceMultipleRepresentations,
                    TargetStoreType = Xbim.IO.XbimStoreType.InMemoryModel,
                    EditorCredentials = EditorCredentials
                };

                Assert.AreEqual(1, source.Instances.OfType<IIfcBuildingElementProxy>().Count(p => IsMultiRepresentation(p, "Body")));
                Assert.AreEqual(4, source.Instances.OfType<IIfcBuildingElementProxy>().Count());

                var result = await transform.Run(source, NewProgressMonitor(true));

                Assert.AreEqual(0, result.Target.Instances.OfType<IIfcBuildingElementProxy>().Count(p => IsMultiRepresentation(p, "Body")));
                Assert.AreEqual(17, result.Target.Instances.OfType<IIfcBuildingElementProxy>().Count());

                var stampAfter = SchemaValidator.OfModel(result.Target);
                Assert.IsTrue(stampAfter.IsCompliantToSchema);

                result.Target.SaveAsIfc(new FileStream("Ifc4-MultipleBodiesPerProduct-1.ifc", FileMode.Create));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-MultipleBodiesPerProduct.ifc")]
        public async Task RefactorWithIfcAssembly()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\Ifc4-MultipleBodiesPerProduct.ifc"))
            {
                var stampBefore = SchemaValidator.OfModel(source);
                Assert.IsTrue(stampBefore.IsCompliantToSchema);

                var transform = new ProductRepresentationRefactorTransform(LoggerFactory)
                {
                    ContextIdentifiers = new[] { "Body" },
                    Strategy = ProductRepresentationRefactorStrategy.RefactorWithEntityElementAssembly | ProductRepresentationRefactorStrategy.ReplaceMultipleRepresentations,
                    TargetStoreType = Xbim.IO.XbimStoreType.InMemoryModel,
                    EditorCredentials = EditorCredentials
                };

                Assert.AreEqual(1, source.Instances.OfType<IIfcBuildingElementProxy>().Count(p => IsMultiRepresentation(p, "Body")));
                Assert.AreEqual(4, source.Instances.OfType<IIfcBuildingElementProxy>().Count());

                var result = await transform.Run(source, NewProgressMonitor(true));

                Assert.AreEqual(0, result.Target.Instances.OfType<IIfcBuildingElementProxy>().Count(p => IsMultiRepresentation(p, "Body")));
                Assert.AreEqual(17, result.Target.Instances.OfType<IIfcBuildingElementProxy>().Count());
                Assert.AreEqual(1, result.Target.Instances.OfType<IIfcElementAssembly>().Count());
                Assert.AreEqual(14, result.Target.Instances.OfType<IIfcElementAssembly>().First().IsDecomposedBy.SelectMany(r => r.RelatedObjects).Count());

                var stampAfter = SchemaValidator.OfModel(result.Target);
                Assert.IsTrue(stampAfter.IsCompliantToSchema);

                result.Target.SaveAsIfc(new FileStream("Ifc4-MultipleBodiesPerProduct-2.ifc", FileMode.Create));
            }
        }
    }
}