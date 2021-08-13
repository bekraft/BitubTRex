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

namespace Bitub.Ifc.Transform.Requests
{
    using PropertyReference = Tuple<XbimInstanceHandle, System.Reflection.PropertyInfo>;

    /// <summary>
    /// Strategy of product replacement.
    /// </summary>
    [Flags]
    public enum ProductRepresentationRefactorStrategy
    {
        /// <summary>
        /// Default splitting multiple representation items by cloneing embedding product.
        /// Create a sequence of new products with new GUIDs of the same type replacing the single multibody product.
        /// </summary>
        ReplaceMultipleRepresentations = 0,
        /// <summary>
        /// Additionally refactor hierarchy using <see cref="IIfcElementAssembly"/> entities.
        /// </summary>
        RefactorWithEntityElementAssembly = 2,
    }

    /// <summary>
    /// Transformation work package of product replacement request.
    /// </summary>
    public sealed class ProductRepresentationRefactorTransformPackage : TransformPackage
    {
        public ProductRepresentationRefactorStrategy Strategy { get; private set; }

        public ISet<string> ContextIdentifier { get; private set; }

        internal IfcBuilder Builder { get; private set; }

        // Product dropouts in source model
        internal ISet<XbimInstanceHandle> ProductDropouts { get; } = new HashSet<XbimInstanceHandle>();
        // New assembly addins
        internal IDictionary<XbimInstanceHandle, XbimInstanceHandle> ProductAssemblyAddins { get; } = new Dictionary<XbimInstanceHandle, XbimInstanceHandle>();
        
        // Reference to products in source model
        internal IDictionary<PropertyReference, XbimInstanceHandle[]> ProductReferences { get; } = new Dictionary<PropertyReference, XbimInstanceHandle[]>();
        // Product representations of source onto target
        internal IDictionary<XbimInstanceHandle, XbimInstanceHandle[]> ProductRepresentationDisassembly { get; } = new Dictionary<XbimInstanceHandle, XbimInstanceHandle[]>();
        // Product disassemly from single source to disassembled target
        internal IDictionary<XbimInstanceHandle, XbimInstanceHandle[]> ProductDisassembly { get; } = new Dictionary<XbimInstanceHandle, XbimInstanceHandle[]>();

        internal ProductRepresentationRefactorTransformPackage(IModel aSource, IModel aTarget, CancelableProgressing progressMonitor,
            ProductRepresentationRefactorStrategy strategy, string[] contextIdentifiers) 
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

        internal bool IsMultibodyRepresentation(IIfcRepresentation r)
        {
            var isInContext = ContextIdentifier.Contains(r.ContextOfItems.ContextIdentifier.ToString().ToLower());
            if (Strategy.HasFlag(ProductRepresentationRefactorStrategy.ReplaceMultipleRepresentations))
                return isInContext && r.Items.Count > 1;

            throw new NotImplementedException($"{Strategy}");
        }

        internal bool IsMultibodyRepresentation(IIfcProduct p)
        {
            return ProductDropouts.Contains(new XbimInstanceHandle(p)) || IsMultibodyRepresentation(p?.Representation);
        }

        internal bool IsMultibodyRepresentation(IIfcProductRepresentation pr)
        {
            return pr?.Representations.Any(r => IsMultibodyRepresentation(r)) ?? false;
        }
    }

    /// <summary>
    /// IFC representation replacement and flattener. Will take an IFC model and flattens all representations having more than one
    /// representation of the same context by default.
    /// <para>
    /// Limitations: No disassembly of spatial or logical aggregation hosts will be performed.
    /// </para>
    /// </summary>
    public class ProductRepresentationRefactorTransform : ModelTransformTemplate<ProductRepresentationRefactorTransformPackage>
    {
        public override string Name => "Product Representation Refactoring Transform";

        public override ILogger Log { get; protected set; }

        public ProductRepresentationRefactorTransform(ILoggerFactory loggerFactory)
        {
            Log = loggerFactory.CreateLogger<ProductRepresentationRefactorTransform>();
        }

        public ProductRepresentationRefactorStrategy Strategy { get; set; } = ProductRepresentationRefactorStrategy.ReplaceMultipleRepresentations;

        public string[] ContextIdentifiers { get; set; } = new[] { "Body" };

        protected override ProductRepresentationRefactorTransformPackage CreateTransformPackage(IModel aSource, IModel aTarget, 
            CancelableProgressing progressMonitor)
        {
            return new ProductRepresentationRefactorTransformPackage(aSource, aTarget, progressMonitor, Strategy, ContextIdentifiers);
        }

        #region Transformation handling

        // Will drop product, representation and shape representation when having multibody characteristics
        protected override TransformActionType PassInstance(IPersistEntity instance, 
            ProductRepresentationRefactorTransformPackage package)
        {
            if (instance is IIfcProduct product && package.IsMultibodyRepresentation(product))
            {
                package.ProductDropouts.Add(new XbimInstanceHandle(product));
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

                if (package.Strategy.HasFlag(ProductRepresentationRefactorStrategy.RefactorWithEntityElementAssembly))
                {
                    var newAssembly = InsertIfcElementAssembly(product, disassembledProducts, package);
                    var assemblyHandle = new XbimInstanceHandle(newAssembly);
                    package.ProductAssemblyAddins.Add(new XbimInstanceHandle(product), assemblyHandle);
                    package.Log?.Add(new TransformLogEntry(assemblyHandle, TransformAction.Added));
                }

                package.Log?.AddRange(disassembledProducts.Select(e => new TransformLogEntry(new XbimInstanceHandle(e), TransformAction.Added)));
                // Drop current product
                return TransformActionType.Drop;
            }
            else if (instance is IIfcRepresentation r && package.IsMultibodyRepresentation(r))
            {
                // Drop only, reproduction is done through product handling
                return TransformActionType.Drop;
            }
            else if (instance is IIfcProductRepresentation pr && package.IsMultibodyRepresentation(pr))
            {
                // Drop only, reproduction is done through product handling            
                return TransformActionType.Drop;
            }

            return TransformActionType.Copy;
        }

        protected override object PropertyTransform(ExpressMetaProperty property, 
            object hostObject, ProductRepresentationRefactorTransformPackage package)
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
                {   // or enumerable of product (relations), cast to null if empty
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

                // Cut product representation
                if (value is IIfcProductRepresentation productRepresentation && package.IsMultibodyRepresentation(productRepresentation))
                {
                    return null;
                }

                // Cut representation
                if (value is IIfcRepresentation representation && package.IsMultibodyRepresentation(representation))
                {
                    return null;
                }
            }
            return base.PropertyTransform(property, hostObject, package);
        }

        protected override TransformResult.Code DoPostTransform(ProductRepresentationRefactorTransformPackage package)
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
                        var targetReferenceHandle = package.ProductAssemblyAddins[sourceReferenceHandle];
                        if (targetReferenceHandle.IsEmpty)
                        {
                            // Fallback to first instance of disassembly
                            targetReferenceHandle = package.ProductDisassembly[sourceReferenceHandle].FirstOrDefault();
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
                    else if (typeof(IItemSet).IsAssignableFrom(hostPropertyInfo.PropertyType))
                    {
                        var targetType = targetHostHandle.GetEntity().GetType();

                        var propertyInfo = targetType.GetInterfaces()
                            .SelectMany(t => t.GetProperties())
                            .Where(p => p.Name == hostPropertyInfo.Name)
                            .Where(p => p.PropertyType.GetGenericArguments().All(t => t.IsAssignableFrom(typeof(IIfcProduct))))
                            .FirstOrDefault();

                        //var testProp = targetType.GetLowerConstraintRelationType<IIfcProduct>(hostPropertyInfo.Name);

                        var items = propertyInfo.GetValue(targetHostHandle.GetEntity());

                        package.Builder.Transactively(m =>
                        {
                            items.GetType().GetMethod("AddRange").Invoke(
                                items,
                                new object[]
                                {
                                    referenceInfo.Value.SelectMany(h =>
                                    {
                                        // If assembly is existing use that assembly as proxy, too
                                        if (package.ProductAssemblyAddins.ContainsKey(h))
                                            return new[] { package.ProductAssemblyAddins[h].GetEntity() as IIfcProduct };
                                        else if (package.ProductDisassembly.ContainsKey(h))
                                            return package.ProductDisassembly[h].Select(p => p.GetEntity()).Cast<IIfcProduct>();
                                        else
                                            return Enumerable.Empty<IIfcProduct>();
                                    }).ToList()
                                }
                            );
                        });
                    }
                });
            }

            return base.DoPostTransform(package);
        }

        /*
        private static bool IsCovariant(Type expectedUpper, Type lower)
        {
            if (lower.IsGenericType)
            {
                var upArgs = expectedUpper.GetGenericArguments();
                return expectedUpper.IsAssignableFrom(lower.Ba) 
                    && (upArgs.Length == 0 || lower.GetGenericArguments().Select((t, i) => IsCovariant(upArgs[i], t)).All(r => r));
            }
            return expectedUpper.IsAssignableFrom(lower);
        }
        /*
        private static System.Reflection.PropertyInfo FindInterfaceEnumerableProperty<TParam>(Type t, string propertyName)
        {
            System.Reflection.PropertyInfo propInfo;
            var interfaces = new Queue(t.GetInterfaces());
            do
            {
                propInfo = t.GetProperty(propertyName);
            } while (null != methodInfo);
            return methodInfo;
        }
        */
        #endregion

        #region Modification helpers

        // Insert a single element assembly which has the same GUID as p
        private IIfcElementAssembly InsertIfcElementAssembly(IIfcProduct p, IEnumerable<IIfcProduct> products, 
            ProductRepresentationRefactorTransformPackage package)
        {
            IIfcElementAssembly assembly = null;
            var newPlacement = Copy(p.ObjectPlacement, package, false);

            package.Builder.Transactively(m =>
            {
                assembly = package.Builder.ifcProductScope.NewOf<IIfcElementAssembly>(e =>
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
            IEnumerable<IIfcProductRepresentation> productRepresentations, ProductRepresentationRefactorTransformPackage package)
        {
            var newPlacement = Copy(p.ObjectPlacement, package, false);
            return productRepresentations.Select(newRepresentation =>
            {
                IIfcProduct newProduct = null;
                package.Builder.Transactively(m =>
                {
                    newProduct = package.Builder.ifcProductScope.New<IIfcProduct>(p.GetType(), e =>
                    {
                        e.Name = p.Name;
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
            ProductRepresentationRefactorTransformPackage package)
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
        private IEnumerable<IIfcRepresentation> FlattenRepresentationItems(IIfcRepresentation p, ProductRepresentationRefactorTransformPackage package)
        {
            return p.Items.Select(item =>
            {
                var contextOfItems = Copy(p.ContextOfItems, package, false);
                var newItem = Copy(item, package, false);

                var newRepresentation = package.Builder.New<IIfcRepresentation>(p.GetType(), (e) =>
                {
                    e.ContextOfItems = contextOfItems;
                    e.RepresentationIdentifier = p.RepresentationIdentifier;
                    e.RepresentationType = p.RepresentationType;
                    e.Items.Add(newItem);
                });
                return newRepresentation;
            });
        }

        #endregion
    }
}
