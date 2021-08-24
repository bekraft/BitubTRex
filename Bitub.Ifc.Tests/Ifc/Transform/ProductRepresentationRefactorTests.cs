using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Bitub.Ifc.Transform;
using Bitub.Ifc.Validation;

namespace Bitub.Ifc.Tests.Transform
{
    [TestClass]
    public class ProductRepresentationRefactorTests : TestBase<ProductRepresentationRefactorTests>
    {
        private TransformActionResult[] defaultLogResultTypes = new[]
        {
            TransformActionResult.Skipped,
            TransformActionResult.Added
        };

        private static bool IsMultiRepresentation(IIfcProduct product, bool includingMappedItems, params string[] contexts)
        {
            return product.Representation.Representations
                .Where(r => contexts.Contains(r.ContextOfItems.ContextIdentifier.ToString()))
                .Any(r => r.Items.Select(i => CountOfNestedItems(i, includingMappedItems)).Sum() > 1);
        }

        private static int CountOfNestedItems(IIfcRepresentationItem item, bool includingMappedItems)
        {
            if (includingMappedItems && item is IIfcMappedItem mappedItem)
            {
                return mappedItem
                    .MappingSource
                    .MappedRepresentation
                    .Items
                    .Select(i => CountOfNestedItems(i, includingMappedItems))
                    .Sum();
            }
            else
            {
                return 1;
            }
        }


        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-MultipleBodiesPerProduct.ifc")]
        public async Task RefactorBody()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\Ifc4-MultipleBodiesPerProduct.ifc"))
            {
                var stampBefore = SchemaValidator.OfModel(source);
                Assert.IsTrue(stampBefore.IsCompliantToSchema);

                var transform = new ProductRepresentationRefactorTransform(LoggerFactory, defaultLogResultTypes)
                {
                    ContextIdentifiers = new[] { "Body" },
                    Strategy = ProductRefactorStrategy.DecomposeMultiItemRepresentations,
                    TargetStoreType = Xbim.IO.XbimStoreType.InMemoryModel,
                    EditorCredentials = EditorCredentials
                };

                Assert.AreEqual(1, source.Instances.OfType<IIfcBuildingElementProxy>().Count(p => IsMultiRepresentation(p, false,"Body")));
                Assert.AreEqual(4, source.Instances.OfType<IIfcBuildingElementProxy>().Count());

                var result = await transform.Run(source, NewProgressMonitor(true));

                Assert.AreEqual(0, result.Target.Instances.OfType<IIfcBuildingElementProxy>().Count(p => IsMultiRepresentation(p, false, "Body")));
                Assert.AreEqual(17, result.Target.Instances.OfType<IIfcBuildingElementProxy>().Count());

                var stampAfter = SchemaValidator.OfModel(result.Target);
                Assert.IsTrue(stampAfter.IsCompliantToSchema);

                result.Target.SaveAsIfc(new FileStream("Ifc4-MultipleBodiesPerProduct-1.ifc", FileMode.Create));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\mapped-shape-with-transformation.ifc")]
        public async Task RefactorMappedBodyWithIfcAssembly()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\mapped-shape-with-transformation.ifc"))
            {
                var stampBefore = SchemaValidator.OfModel(source);
                Assert.IsTrue(stampBefore.IsCompliantToSchema);

                var transform = new ProductRepresentationRefactorTransform(LoggerFactory)
                {
                    ContextIdentifiers = new[] { "Body" },
                    Strategy = ProductRefactorStrategy.DecomposeMultiItemRepresentations 
                        | ProductRefactorStrategy.DecomposeMappedRepresentations 
                        | ProductRefactorStrategy.DecomposeWithEntityElementAssembly,
                    TargetStoreType = Xbim.IO.XbimStoreType.InMemoryModel,
                    EditorCredentials = EditorCredentials
                };

                Assert.AreEqual(1, source.Instances.OfType<IIfcBuildingElementProxy>().Count(p => IsMultiRepresentation(p, true, "Body")));
                Assert.AreEqual(1, source.Instances.OfType<IIfcBuildingElementProxy>().Count());

                var result = await transform.Run(source, NewProgressMonitor(true));

                Assert.AreEqual(0, result.Target.Instances.OfType<IIfcBuildingElementProxy>().Count(p => IsMultiRepresentation(p, true, "Body")));
                Assert.AreEqual(2, result.Target.Instances.OfType<IIfcBuildingElementProxy>().Count());

                var stampAfter = SchemaValidator.OfModel(result.Target);
                Assert.IsTrue(stampAfter.IsCompliantToSchema);

                result.Target.SaveAsIfc(new FileStream("mapped-shape-with-transformation-1.ifc", FileMode.Create));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\mapped-shape-with-transformation.ifc")]
        public async Task RefactorKeepMappedBody()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\mapped-shape-with-transformation.ifc"))
            {
                var stampBefore = SchemaValidator.OfModel(source);
                Assert.IsTrue(stampBefore.IsCompliantToSchema);

                var transform = new ProductRepresentationRefactorTransform(LoggerFactory)
                {
                    ContextIdentifiers = new[] { "Body" },
                    Strategy = ProductRefactorStrategy.DecomposeMultiItemRepresentations,
                    TargetStoreType = Xbim.IO.XbimStoreType.InMemoryModel,
                    EditorCredentials = EditorCredentials
                };

                Assert.AreEqual(1, source.Instances.OfType<IIfcBuildingElementProxy>().Count(p => IsMultiRepresentation(p, true, "Body")));
                Assert.AreEqual(1, source.Instances.OfType<IIfcBuildingElementProxy>().Count());

                var result = await transform.Run(source, NewProgressMonitor(true));

                Assert.AreEqual(1, result.Target.Instances.OfType<IIfcBuildingElementProxy>().Count(p => IsMultiRepresentation(p, true, "Body")));
                Assert.AreEqual(1, result.Target.Instances.OfType<IIfcBuildingElementProxy>().Count());

                var stampAfter = SchemaValidator.OfModel(result.Target);
                Assert.IsTrue(stampAfter.IsCompliantToSchema);

                result.Target.SaveAsIfc(new FileStream("mapped-shape-with-transformation-2.ifc", FileMode.Create));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-MultipleBodiesPerProduct.ifc")]
        public async Task RefactorBodyWithIfcAssembly()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\Ifc4-MultipleBodiesPerProduct.ifc"))
            {
                var stampBefore = SchemaValidator.OfModel(source);
                Assert.IsTrue(stampBefore.IsCompliantToSchema);

                var transform = new ProductRepresentationRefactorTransform(LoggerFactory)
                {
                    ContextIdentifiers = new[] { "Body" },
                    Strategy = ProductRefactorStrategy.DecomposeWithEntityElementAssembly | ProductRefactorStrategy.DecomposeMultiItemRepresentations,
                    TargetStoreType = Xbim.IO.XbimStoreType.InMemoryModel,
                    EditorCredentials = EditorCredentials
                };

                Assert.AreEqual(1, source.Instances.OfType<IIfcBuildingElementProxy>().Count(p => IsMultiRepresentation(p, false, "Body")));
                Assert.AreEqual(4, source.Instances.OfType<IIfcBuildingElementProxy>().Count());

                var result = await transform.Run(source, NewProgressMonitor(true));

                Assert.AreEqual(0, result.Target.Instances.OfType<IIfcBuildingElementProxy>().Count(p => IsMultiRepresentation(p, false, "Body")));
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