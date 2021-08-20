using System;
using System.Collections.Generic;
using System.Linq;

using Xbim.Common;
using Xbim.Common.Metadata;

using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.UtilityResource;

using Bitub.Dto;
using Bitub.Ifc;

using Microsoft.Extensions.Logging;
using System.Collections;

namespace Bitub.Ifc.Transform
{
    using PropertyReference = Tuple<XbimInstanceHandle, System.Reflection.PropertyInfo>;

    /// <summary>
    /// Strategy of product decomposition.
    /// </summary>
    [Flags]
    public enum ProductRefactorStrategy
    {
        /// <summary>
        /// Default splitting multiple representation items by cloneing embedding product.
        /// Create a sequence of new products with new GUIDs of the same type replacing the single multibody product.
        /// </summary>
        DecomposeMultiItemRepresentations = 0,
        /// <summary>
        /// Additionally refactor hierarchy using <see cref="IIfcElementAssembly"/> entities.
        /// </summary>
        DecomposeWithEntityElementAssembly = 2,
        /// <summary>
        /// Refactor mapped representations. If not specified, only directly referenced representations will be refactored.
        /// </summary>
        DecomposeMappedRepresentations = 4
    }

    /// <summary>
    /// Transformation work package of product decomposition transform. Holds runtime transform data.
    /// </summary>
    public sealed class ProductRefactorTransformPackage : TransformPackage
    {
        public ProductRefactorStrategy Strategy { get; private set; }

        internal Func<string, int, string> NameRefactorFunction { get; set; }

        internal ISet<string> ContextIdentifier { get; private set; }

        internal IfcBuilder Builder { get; private set; }

        // Product dropouts in source model
        internal ISet<XbimInstanceHandle> EntityDropoutSet { get; } = new HashSet<XbimInstanceHandle>();

        // New assembly addins
        internal IDictionary<XbimInstanceHandle, XbimInstanceHandle> ProductAssemblyAddins { get; } = new Dictionary<XbimInstanceHandle, XbimInstanceHandle>();
        
        // Reference to products in source model
        internal IDictionary<PropertyReference, XbimInstanceHandle[]> ProductReferences { get; } = new Dictionary<PropertyReference, XbimInstanceHandle[]>();
        // Product representations of source onto target
        internal IDictionary<XbimInstanceHandle, XbimInstanceHandle[]> ProductRepresentationDisassembly { get; } = new Dictionary<XbimInstanceHandle, XbimInstanceHandle[]>();
        // Product disassemly from single source to disassembled target
        internal IDictionary<XbimInstanceHandle, XbimInstanceHandle[]> ProductDisassembly { get; } = new Dictionary<XbimInstanceHandle, XbimInstanceHandle[]>();
        // Representation map disassembly mapping
        internal IDictionary<XbimInstanceHandle, XbimInstanceHandle[]> RepresentationMapDisassambly { get; } = new Dictionary<XbimInstanceHandle, XbimInstanceHandle[]>();

        internal ProductRefactorTransformPackage(IModel aSource, IModel aTarget, CancelableProgressing progressMonitor,
            ProductRefactorStrategy strategy, string[] contextIdentifiers) 
            : base(aSource, aTarget, progressMonitor)
        {
            Strategy = strategy;
            ContextIdentifier = new HashSet<string>(contextIdentifiers.Select(s => s.ToLower()));
            Builder = IfcBuilder.WithModel(aTarget);            
        }

        public bool TryGetReplacedRepresentation(IIfcProductRepresentation sourceRepresentation, 
            out IIfcProductRepresentation[] productRepresentations, Func<IEnumerable<IIfcProductRepresentation>> generator = null)
        {
            XbimInstanceHandle[] handles;
            var sourceHandle = new XbimInstanceHandle(sourceRepresentation);
            if (ProductRepresentationDisassembly.TryGetValue(sourceHandle, out handles))
            {
                productRepresentations = handles.Select(h => h.GetEntity() as IIfcProductRepresentation).ToArray();
                return true;
            }
            else
            {
                productRepresentations = generator?.Invoke()?.ToArray();
                if (null != productRepresentations)
                    ProductRepresentationDisassembly.Add(sourceHandle, productRepresentations.Select(pr => new XbimInstanceHandle(pr)).ToArray());
            }
            
            return false;
        }

        // Is multi-body if there are more than 1 (nested) item(s) included
        internal bool IsMultibodyRepresentation(IIfcRepresentation r)
        {
            var isInContext = ContextIdentifier.Contains(r.ContextOfItems.ContextIdentifier.ToString().ToLower());
            return isInContext && r.Items.Select(i => CountOfNestedItems(i)).Sum() > 1;
        }

        // Special mapped representation handling
        internal bool IsMultibodyRepresentation(IIfcRepresentationMap map)
        {   // Solely depends on mapped representation
            return RepresentationMapDisassambly.ContainsKey(new XbimInstanceHandle(map)) || IsMultibodyRepresentation(map.MappedRepresentation);
        }

        internal bool IsMultibodyRepresentationItem(IIfcRepresentationItem item)
        {
            return CountOfNestedItems(item) > 1;
        }

        internal int CountOfNestedItems(IIfcRepresentationItem item)
        {
            if (Strategy.HasFlag(ProductRefactorStrategy.DecomposeMappedRepresentations) && 
                item is IIfcMappedItem mappedItem)
            {
                return mappedItem
                    .MappingSource
                    .MappedRepresentation
                    .Items
                    .Select(i => CountOfNestedItems(i))
                    .Sum();
            }
            else
            {
                return 1;
            }
        }

        internal bool IsMultibodyRepresentation(IIfcProduct p)
        {
            return EntityDropoutSet.Contains(new XbimInstanceHandle(p)) || IsMultibodyRepresentation(p?.Representation);
        }

        internal bool IsMultibodyRepresentation(IIfcProductRepresentation pr)
        {
            return pr?.Representations.Any(r => IsMultibodyRepresentation(r)) ?? false;
        }        
    }

    /// <summary>
    /// IFC representation decomposing transform. Will take an IFC model and flattens all representations having more than one
    /// representation of the same context. Each representation item will be assigned to a new representation and a new product clone of
    /// the former hosting product.
    /// </summary>
    public class ProductRepresentationRefactorTransform : ModelTransformTemplate<ProductRefactorTransformPackage>
    {
        public override string Name => "Product Representation Decomposiong Transform";

        public override ILogger Log { get; protected set; }

        public ProductRepresentationRefactorTransform(ILoggerFactory loggerFactory, params TransformActionResult[] logFilter) : base(logFilter)
        {
            Log = loggerFactory.CreateLogger<ProductRepresentationRefactorTransform>();
        }

        /// <summary>
        /// Strategy to refactor by.
        /// </summary>
        public ProductRefactorStrategy Strategy { get; set; } = ProductRefactorStrategy.DecomposeMultiItemRepresentations;

        /// <summary>
        /// Context identifiers to refactor. By default set to "Body".
        /// </summary>
        public string[] ContextIdentifiers { get; set; } = new[] { "Body" };

        /// <summary>
        /// The product name refactoring function. Takes the original product name and a progressive counter index as input.
        /// </summary>
        public Func<string, int, string> NameRefactorFunction { get; set; } = (label, idx) => string.IsNullOrWhiteSpace(label) ? $"{idx}" : $"{label}-{idx}";

        protected override ProductRefactorTransformPackage CreateTransformPackage(IModel aSource, IModel aTarget,
            CancelableProgressing progressMonitor)
        {
            var package = new ProductRefactorTransformPackage(aSource, aTarget,
                progressMonitor, Strategy, ContextIdentifiers)
            {
                NameRefactorFunction = NameRefactorFunction
            };
            LogFilter.ForEach(f => package.LogFilter.Add(f));

            return package;
        }

        #region Transformation handling

        // Will drop product, representation and shape representation when having multibody characteristics
        protected override TransformActionType PassInstance(IPersistEntity instance, 
            ProductRefactorTransformPackage package)
        {
            if (instance is IIfcProduct product && package.IsMultibodyRepresentation(product))
            {
                package.EntityDropoutSet.Add(new XbimInstanceHandle(product));
                IIfcProductRepresentation[] disassembledRepresentations;
                if (package.TryGetReplacedRepresentation(product.Representation, 
                    out disassembledRepresentations, () => FlattenProductRepresentation(product, package)))
                {
                    Log?.LogInformation("Reusing already decomposed representation #{0} into {1} representations.", 
                        product.Representation.EntityLabel, disassembledRepresentations.Length);                    
                }
                else
                {
                    Log?.LogInformation("Creating decomposed representations for #{0} into {1} representations.",
                        product.Representation.EntityLabel, disassembledRepresentations.Length);
                }

                // Register product disassembly
                var disassembledProducts = FlattenProduct(product, disassembledRepresentations, package).ToArray();
                package.ProductDisassembly.Add(new XbimInstanceHandle(product), disassembledProducts.Select(e => new XbimInstanceHandle(e)).ToArray());

                if (package.Strategy.HasFlag(ProductRefactorStrategy.DecomposeWithEntityElementAssembly))
                {
                    var newAssembly = InsertIfcElementAssembly(product, disassembledProducts, package);
                    var assemblyHandle = new XbimInstanceHandle(newAssembly);
                    package.ProductAssemblyAddins.Add(new XbimInstanceHandle(product), assemblyHandle);
                    package.LogAction(assemblyHandle, TransformActionResult.Added);
                }

                foreach (var e in disassembledProducts)
                    package.LogAction(new XbimInstanceHandle(e), TransformActionResult.Added);

                // Drop current product
                return TransformActionType.Drop;
            }
            else if (instance is IIfcRepresentation r && package.IsMultibodyRepresentation(r))
            {
                var handle = new XbimInstanceHandle(r);
                if (package.EntityDropoutSet.Contains(handle))
                {
                    // Drop if already marked, reproduction is done through product handling
                    return TransformActionType.Drop;
                }
                else
                {   
                    // Delay until known source reference
                    package.EntityDropoutSet.Add(handle);
                    return TransformActionType.Delegate;
                }
            }
            else if (instance is IIfcProductRepresentation pr && package.IsMultibodyRepresentation(pr))
            {
                // Drop only, reproduction is done through product handling            
                return TransformActionType.Drop;
            }
            else if (instance is IIfcRepresentationItem item && package.IsMultibodyRepresentationItem(item))
            {
                // Should only apply to mapped items with nested representations
                return TransformActionType.Drop;
            }
            else if (instance is IIfcRepresentationMap map && package.IsMultibodyRepresentation(map))
            {
                var handle = new XbimInstanceHandle(map);
                if (package.EntityDropoutSet.Contains(handle))
                {
                    // Drop if already marked, reproduction is done through product handling
                    return TransformActionType.Drop;
                }
                else
                {
                    // Delay until known source reference
                    package.EntityDropoutSet.Add(handle);
                    return TransformActionType.Delegate;
                }
            }

            return TransformActionType.Copy;
        }

        protected override IPersistEntity DelegateCopy(IPersistEntity instance, ProductRefactorTransformPackage package)
        {
            if (instance is IIfcRepresentation)
                return null;
            if (instance is IIfcRepresentationMap)
                return null;

            return base.DelegateCopy(instance, package);
        }

        protected override object PropertyTransform(ExpressMetaProperty property, 
            object hostObject, ProductRefactorTransformPackage package)
        {
            // Check any reference to products if it has been dropped or has to be dropped
            if (hostObject is IPersistEntity entity)
            {
                var value = property?.PropertyInfo.GetValue(hostObject);
                if (null == value)
                    return null;

                // If single product value
                if (value is IIfcProduct product && package.IsMultibodyRepresentation(product))
                {
                    package.ProductReferences.Add(
                        new PropertyReference(new XbimInstanceHandle(hostObject as IPersistEntity), property.PropertyInfo), 
                        new[] { new XbimInstanceHandle(product) });
                    return null;
                }
                else if (value is IEnumerable instances)
                {
                    if (property.PropertyInfo.HasLowerConstraintRelationTypeEquivalent<IIfcProduct>())
                    {
                        // or enumerable of product (relations), cast to null if empty
                        var products = instances
                            .OfType<IIfcProduct>()
                            .Where(p => package.IsMultibodyRepresentation(p))
                            .ToHashSet();

                        if (products.Count > 0)
                            package.ProductReferences.Add(
                                new PropertyReference(new XbimInstanceHandle(hostObject as IPersistEntity), property.PropertyInfo),
                                products.Select(p => new XbimInstanceHandle(p)).ToArray());

                        return EmptyToNull(instances.OfType<IPersist>().Where(e => !products.Contains(e)));
                    }
                    else if (property.PropertyInfo.HasLowerConstraintRelationTypeEquivalent<IIfcRepresentationMap>()
                        && package.Strategy.HasFlag(ProductRefactorStrategy.DecomposeMappedRepresentations))
                    {
                        var maps = instances
                            .OfType<IIfcRepresentationMap>()
                            .Where(r => package.IsMultibodyRepresentation(r))
                            .ToHashSet();

                        if (maps.Count > 0)
                            package.ProductReferences.Add(
                                new PropertyReference(new XbimInstanceHandle(hostObject as IPersistEntity), property.PropertyInfo),
                                maps.Select(p => new XbimInstanceHandle(p)).ToArray());

                        return EmptyToNull(instances.OfType<IPersist>().Where(e => !maps.Contains(e)));
                    }
                }

                // Cut product representation
                if (value is IIfcProductRepresentation productRepresentation && package.IsMultibodyRepresentation(productRepresentation))
                {
                    // Will be refactored by product, drop here
                    return null;
                }

                // Cut representation
                if (value is IIfcRepresentation representation && package.IsMultibodyRepresentation(representation))
                {
                    if (property.PropertyInfo.Name == nameof(IIfcRepresentationMap.MappedRepresentation))
                        // Test, if reference passes dropout filter
                        return PassReferenceDropoutFilter(representation, package);
                    else
                        // Will be refactored by product, drop here
                        return null;
                }

                if (value is IIfcRepresentationMap map && package.IsMultibodyRepresentation(map.MappedRepresentation))
                {
                    return PassReferenceDropoutFilter(map, package);
                }
            }
            // Fallback
            return base.PropertyTransform(property, hostObject, package);
        }

        private object PassReferenceDropoutFilter(IPersistEntity persistEntity, ProductRefactorTransformPackage package)
        {
            var handle = new XbimInstanceHandle(persistEntity);
            if (package.Strategy.HasFlag(ProductRefactorStrategy.DecomposeMappedRepresentations))
            {
                // If already hit by pass instance filter, mark as removed
                if (package.EntityDropoutSet.Contains(handle))
                    package.LogAction(handle, TransformActionResult.NotTransferred);
                else
                    // Anyway, mark for removal if it by pass instance filter
                    package.EntityDropoutSet.Add(handle);

                return null;
            }
            else
            {
                // Remove from removal candidates and return value as needed reference
                package.EntityDropoutSet.Remove(handle);
                return persistEntity;
            }
        }

        protected override TransformResult.Code DoPostTransform(ProductRefactorTransformPackage package)
        {
            // Finally regenerate cut references to products
            foreach (var referenceInfo in package.ProductReferences)
            {
                if (package.IsCanceledOrBroken)
                {
                    return TransformResult.Code.Canceled;
                }

                var (hostHandle, hostPropertyInfo) = referenceInfo.Key;
                var targetHostHandle = package.Map[hostHandle];
                package.Builder.Transactively(m =>
                {
                    if (typeof(IIfcProduct).IsAssignableFrom(hostPropertyInfo.PropertyType) && referenceInfo.Value.Length == 1)
                    {
                        var sourceReferenceHandle = referenceInfo.Value[0];
                        // Find assembly instance first, if present (indicated by strategy)
                        XbimInstanceHandle targetReferenceHandle;
                        if (!package.ProductAssemblyAddins.TryGetValue(sourceReferenceHandle, out targetReferenceHandle))
                        {
                            // Fallback to first instance of disassembly
                            targetReferenceHandle = package.ProductDisassembly[sourceReferenceHandle].FirstOrDefault();
                            Log?.LogWarning("Detected singleton reference to omitted product #{0}{1} by #{2}{3}.{4}. Replaced by #{5}{6}.",
                                sourceReferenceHandle.EntityLabel, sourceReferenceHandle.EntityExpressType.Name,
                                hostHandle.EntityLabel, hostHandle.EntityExpressType.Name, hostPropertyInfo.Name,
                                targetReferenceHandle.EntityLabel, targetReferenceHandle.EntityExpressType.Name);
                        }

                        if (targetReferenceHandle.IsEmpty)
                        {
                            Log?.LogWarning("Reference from #{0}.{1} to #{2} ({3}) not found in target.",
                                hostHandle.EntityLabel, hostPropertyInfo.Name, sourceReferenceHandle.EntityLabel, sourceReferenceHandle.EntityExpressType.Name);
                        }
                        else
                        {
                            Log?.LogWarning("Having unary reference to disassembled product instance. Using #{0} as proxy relation target of #{1}.{2}.",
                                targetReferenceHandle.EntityLabel, targetHostHandle.EntityLabel, hostPropertyInfo.Name);

                            package.Builder.Transactively(m =>
                            {
                                hostPropertyInfo.SetValue(targetHostHandle.GetEntity(), targetReferenceHandle.GetEntity());
                            });
                        }
                    }
                    else if (hostPropertyInfo.HasLowerConstraintRelationTypeEquivalent<IIfcProduct>())
                    {
                        if (!targetHostHandle.GetEntity().AddRelationsByLowerConstraint(
                            hostPropertyInfo.Name,
                            referenceInfo.Value.SelectMany(h =>
                            {
                                // Containment relations
                                bool isContainmentRelation = typeof(IIfcRelDecomposes).IsAssignableFrom(targetHostHandle.EntityExpressType.Type)
                                    || typeof(IIfcRelContainedInSpatialStructure).IsAssignableFrom(targetHostHandle.EntityExpressType.Type);

                                bool hasAssemblyParent = package.ProductAssemblyAddins.ContainsKey(h);

                                // If assembly is existing use that assembly as proxy, too
                                IEnumerable<IIfcProduct> candidates = Enumerable.Empty<IIfcProduct>();
                                if (hasAssemblyParent)
                                    candidates = candidates.Concat(new[] { package.ProductAssemblyAddins[h].GetEntity() as IIfcProduct });
                                if (package.ProductDisassembly.ContainsKey(h) && (!isContainmentRelation || !hasAssemblyParent))
                                    // Don't propagate contaiment concepts to children (if they are children)
                                    candidates = candidates.Concat(package.ProductDisassembly[h].Select(p => p.GetEntity()).Cast<IIfcProduct>());

                                return candidates;
                            })))
                        {
                            Log?.LogWarning("Could not add instances to #{0}{1}.{2} relation.",
                                targetHostHandle.EntityLabel, targetHostHandle.EntityExpressType.Name, hostPropertyInfo.Name);
                        }
                    }
                    else if (hostPropertyInfo.HasLowerConstraintRelationTypeEquivalent<IIfcRepresentationMap>())
                    {
                        if (!targetHostHandle.GetEntity().AddRelationsByLowerConstraint(
                            hostPropertyInfo.Name,
                            referenceInfo.Value
                                .SelectMany(h => package.RepresentationMapDisassambly[h]
                                    .Select(p => p.GetEntity())
                                    .Cast<IIfcRepresentationMap>())))
                        {
                            Log?.LogWarning("Could not add instances to #{0}{1}.{2} relation.",
                                targetHostHandle.EntityLabel, targetHostHandle.EntityExpressType.Name, hostPropertyInfo.Name);
                        }
                    }
                });
            }

            return base.DoPostTransform(package);
        }

        #endregion

        #region Modification helpers

        // Insert a single element assembly which has the same GUID as p
        private IIfcElementAssembly InsertIfcElementAssembly(IIfcProduct p, IEnumerable<IIfcProduct> products, 
            ProductRefactorTransformPackage package)
        {
            IIfcElementAssembly assembly = null;
            var newPlacement = Copy(p.ObjectPlacement, package, false);

            package.Builder.Transactively(m =>
            {
                assembly = package.Builder.ifcEntityScope.NewOf<IIfcElementAssembly>(e =>
                {
                    e.Name = p.Name;
                    e.GlobalId = p.GlobalId;
                    e.ObjectPlacement = newPlacement;
                    e.ObjectType = p.ObjectType;
                    e.OwnerHistory = package.Builder.OwnerHistoryTag;
                });
                // Use decomposition relationship
                m.NewDecomposes(assembly).RelatedObjects.AddRange(products);
            });
            return assembly;
        }

        // Create a sequence of new products with new GUIDs of the same type replacing the single multibody product
        private IEnumerable<IIfcProduct> FlattenProduct(IIfcProduct p, 
            IEnumerable<IIfcProductRepresentation> productRepresentations, ProductRefactorTransformPackage package)
        {
            var newPlacement = Copy(p.ObjectPlacement, package, false);
            return productRepresentations.Select((newRepresentation, idx) =>
            {
                IIfcProduct newProduct = null;
                package.Builder.Transactively(m =>
                {
                    newProduct = package.Builder.ifcEntityScope.New<IIfcProduct>(p.GetType(), e =>
                    {
                        e.Name = package.NameRefactorFunction?.Invoke(p.Name, idx) ?? p.Name;
                        e.GlobalId = IfcGloballyUniqueId.ConvertToBase64(System.Guid.NewGuid());
                        e.Representation = newRepresentation;
                        e.OwnerHistory = package.Builder.OwnerHistoryTag;
                        e.ObjectPlacement = newPlacement;
                        e.ObjectType = p.ObjectType;                        
                    });
                });

                return newProduct;
            });
        }

        // Flatten products per items, single product per single representation and item
        private IEnumerable<IIfcProductRepresentation> FlattenProductRepresentation(IIfcProduct p, 
            ProductRefactorTransformPackage package)
        {
            return p.Representation.Representations
                .SelectMany(r => FlattenRepresentationItems(r, package))
                .Select(r =>
                {
                    IIfcProductRepresentation newRepresentation = null;
                    package.Builder.Transactively(m =>
                    {
                        newRepresentation = package.Builder.ifcEntityScope.New<IIfcProductRepresentation>(p.Representation.GetType(), e =>
                        {
                            e.Name = p.Representation.Name;
                            e.Representations.Add(r);
                        });
                    });

                    return newRepresentation;
                });
        }

        // Flatten representation items, single item per representation
        private IEnumerable<IIfcRepresentation> FlattenRepresentationItems(IIfcRepresentation p, 
            ProductRefactorTransformPackage package)
        {
            return p.Items.SelectMany(item => FlattenRepresentationItem(p, item, package));
        }

        private IEnumerable<IIfcRepresentation> FlattenRepresentationItem(IIfcRepresentation p, IIfcRepresentationItem i,
            ProductRefactorTransformPackage package)
        {
            if (i is IIfcMappedItem mappedItem)
            {
                // Special handling of mapped items, unwrap and create a new representation for each mapped item
                foreach(var newRepresentation in FlattenMappedRepresentationItem(mappedItem, package)
                    .Select(newItem => CopyCloneRepresentation(p, newItem, package)))
                {
                    yield return newRepresentation;
                }
            }
            else
            {
                yield return CopyCloneRepresentation(p, Copy(i, package, false), package);
            }
        }

        // Clones attributes but sets the item's reference to a new item.
        private IIfcRepresentation CopyCloneRepresentation(IIfcRepresentation p, IIfcRepresentationItem newItem,
            ProductRefactorTransformPackage package)
        {
            var contextOfItems = Copy(p.ContextOfItems, package, false);

            return package.Builder.New<IIfcRepresentation>(p.GetType(), (e) =>
            {
                e.ContextOfItems = contextOfItems;
                e.RepresentationIdentifier = p.RepresentationIdentifier;
                e.RepresentationType = p.RepresentationType;
                e.Items.Add(newItem);
            });
        }

        // Special handling if representation item wraps a mapoed representation
        private IEnumerable<IIfcMappedItem> FlattenMappedRepresentationItem(IIfcMappedItem item, 
            ProductRefactorTransformPackage package)
        {
            var sourceRepresentation = item.MappingSource.MappedRepresentation;
            return FlattenRepresentationItems(sourceRepresentation, package).Select(newRepresentation =>
            {
                var target = Copy(item.MappingTarget, package, false);
                var origin = Copy(item.MappingSource.MappingOrigin, package, false);

                var representationMap = package.Builder.ifcEntityScope.NewOf<IIfcRepresentationMap>(e =>
                {
                    e.MappedRepresentation = newRepresentation;
                    e.MappingOrigin = origin;
                });

                XbimInstanceHandle[] handles;
                var keyHandle = new XbimInstanceHandle(item.MappingSource);
                if (!package.RepresentationMapDisassambly.TryGetValue(keyHandle, out handles))
                {
                    package.RepresentationMapDisassambly.Add(keyHandle, new[] { new XbimInstanceHandle(representationMap) });
                }
                else
                {
                    package.RepresentationMapDisassambly[keyHandle] = handles.Concat(new[] { new XbimInstanceHandle(representationMap) }).ToArray();
                }

                return package.Builder.ifcEntityScope.NewOf<IIfcMappedItem>(e =>
                {
                    e.MappingSource = representationMap;
                    e.MappingTarget = target;
                });
            });
        }

        #endregion
    }
}
