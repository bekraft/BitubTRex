using System;
using System.Collections.Generic;
using System.Linq;

using Xbim.IO;

using Xbim.Ifc4.Interfaces;
using Xbim.Ifc;

using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Common.Geometry;

using Microsoft.Extensions.Logging;

using Bitub.Dto;

namespace Bitub.Ifc
{
    /// <summary>
    /// Generic Ifc builder bound to an IFC schema version and assembly.
    /// </summary>
    public abstract class IfcBuilder
    {
        public readonly IModel model;
        public readonly IfcAssemblyScope ifcAssembly;

        #region Internals        
        protected Stack<IIfcObjectDefinition> InstanceScopeStack { get; private set; } = new Stack<IIfcObjectDefinition>();

        protected readonly ILogger log;
        protected readonly Qualifier schema;
        #endregion

        public readonly IfcEntityScope<IPersistEntity> ifcEntityScope;
        public readonly IfcEntityScope<IIfcProduct> ifcProductScope;
        public readonly IfcEntityScope<IIfcProperty> ifcPropertyScope;
        public readonly IfcEntityScope<IIfcValue> ifcValueScope;

        /// <summary>
        /// New builder attached to IFC entity assembly given as scope.
        /// </summary>
        /// <param name="m">A store.</param>
        /// <param name="loggerFactory">The logger factory</param>
        protected IfcBuilder(IModel m, ILoggerFactory loggerFactory = null)
        {
            // Principle properties
            log = loggerFactory?.CreateLogger(GetType());
            model = m;
            schema = m.SchemaVersion.ToString().ToQualifier();
            ifcAssembly = IfcAssemblyScope.SchemaAssemblyScope[m.SchemaVersion];

            // Type scopes
            ifcEntityScope = new IfcEntityScope<IPersistEntity>(this);
            ifcProductScope = new IfcEntityScope<IIfcProduct>(this);
            ifcPropertyScope = new IfcEntityScope<IIfcProperty>(this);
            ifcValueScope = new IfcEntityScope<IIfcValue>(this);

            // Initialization
            var project = model.Instances.OfType<IIfcProject>().FirstOrDefault();
            if (null == project)
            {
                Transactively(s =>
                {
                    project = InitProject();
                    OwnerHistoryTag = NewOwnerHistoryEntry("Initial contribution");
                });
            }
            else
            {
                Transactively(s => OwnerHistoryTag = NewOwnerHistoryEntry("Adding new data"));
            }
            InstanceScopeStack.Push(project);
        }

        protected void NewContainer(IIfcObjectDefinition container)
        {
            var scope = CurrentScope;
            InstanceScopeStack.Push(container);
            model.NewDecomposes(scope).RelatedObjects.Add(container);
        }

        public Qualifier Schema { get => new Qualifier(schema); }

        /// <summary>
        /// Current owner history entry.
        /// </summary>
        public IIfcOwnerHistory OwnerHistoryTag { get; protected set; }

        /// <summary>
        /// New builder wrapping a new in-memory IFC model.
        /// </summary>
        /// <param name="c">The editor's credentials</param>
        /// <param name="version">The schema version</param>
        /// <param name="loggerFactory">A logger factory</param>
        /// <returns>A builder instance</returns>
        public static IfcBuilder WithCredentials(XbimEditorCredentials c, XbimSchemaVersion version = XbimSchemaVersion.Ifc4, ILoggerFactory loggerFactory = null)
        {
            var newStore = IfcStore.Create(version, XbimStoreType.InMemoryModel);
            return WithModel(newStore, loggerFactory);
        }    

        /// <summary>
        /// Wraps an existing store.
        /// </summary>
        /// <param name="store">The store</param>
        /// <param name="loggerFactory">The logger factory</param>
        /// <returns>A new builder instance</returns>
        public static IfcBuilder WithModel(IModel model, ILoggerFactory loggerFactory = null)
        {
            switch (model.SchemaVersion)
            {
                case XbimSchemaVersion.Ifc2X3:
                    return new Ifc2x3Builder(model, loggerFactory);
                case XbimSchemaVersion.Ifc4:
                case XbimSchemaVersion.Ifc4x1:
                    return new Ifc4Builder(model, loggerFactory);
            }
            throw new NotImplementedException($"Missing implementation for ${model.SchemaVersion}");
        }

        abstract protected IIfcProject InitProject();

        abstract protected IIfcOwnerHistory NewOwnerHistoryEntry(string comment);

        abstract public IIfcPersonAndOrganization OwningUser { get; set; }

        abstract public IIfcApplication OwningApplication { get; set; }


        /// <summary>
        /// Current top scope of model hierarchy.
        /// </summary>
        public IIfcObjectDefinition CurrentScope
        {
            get => InstanceScopeStack.Peek();
        }

        /// <summary>
        /// Current top placement in model hierarchy.
        /// </summary>
        public IIfcObjectPlacement CurrentPlacement => InstanceScopeStack
            .OfType<IIfcProduct>()
            .FirstOrDefault(p => p.ObjectPlacement != null)?
            .ObjectPlacement;

        /// <summary>
        /// Returns a collection of concrete product types (which might be an IfcElement)
        /// </summary>
        public IEnumerable<Type> InstanstiableProducts
        {
            get => ifcProductScope.Implementing<IIfcProduct>();
        }

        /// <summary>
        /// Returns a subset of IfcProduct which is conforming to IfcElement
        /// </summary>
        public IEnumerable<Type> InstanstiableElements
        {
            get => ifcProductScope.Implementing<IIfcElement>();
        }

        public string TransactionContext
        {
            get {
                return $"Modification ${DateTime.Now}";
            }
        }

        /// <summary>
        /// Wraps an IfcStore modification into a transaction context.
        /// </summary>
        /// <param name="action">The modification return true, if transaction shall be applied</param>
        public void Wrap(Func<IModel,bool> action)
        {
            using (var txn = model.BeginTransaction(TransactionContext))
            {
                try
                {
                    if (action(model))
                    {
                        txn.Commit();
                    }
                    else
                    {
                        txn.RollBack();
                        log?.LogWarning($"Detected cancellation of commit '{txn.Name}'");
                    }
                }
                catch(Exception e)
                {
                    txn.RollBack();
                    log?.LogError(e, "Exception caught. Rollback done.");
                }
            }
        }

        /// <summary>
        /// Wraps an IfcStore modification into a transaction context.
        /// </summary>
        /// <param name="action">The modification</param>
        public void Transactively(Action<IModel> action)
        {
            if (null != model.CurrentTransaction)
            {
                action?.Invoke(model);
            }
            else
            {
                using (var txn = model.BeginTransaction(TransactionContext))
                {
                    try
                    {
                        action?.Invoke(model);
                        txn.Commit();
                    }
                    catch (Exception e)
                    {
                        txn.RollBack();
                        log?.LogError(e, "Exception caught. Rollback done.");
                    }
                }
            }
        }

        /// <summary>
        /// Adds a placement to current top scope.
        /// </summary>
        /// <param name="modifier">Action to modify placement by given relative coordinates</param>
        /// <returns>A local placement reference</returns>
        public IIfcLocalPlacement NewLocalPlacement(XbimVector3D refPosition, bool scaleUp = false)
        {
            IIfcLocalPlacement placement = null;
            Transactively(s =>
            {
                var product = CurrentScope as IIfcProduct;
                var relPlacement = CurrentPlacement;
                if (null != product)
                {
                    if (null == product.ObjectPlacement)
                    {
                        placement = s.NewLocalPlacement(refPosition, scaleUp);
                        if (relPlacement != product.ObjectPlacement)
                            // Don't reference former placement while replacing own placement
                            placement.PlacementRelTo = relPlacement;

                        product.ObjectPlacement = placement;
                    }
                    else
                    {
                        log.LogWarning($"#{product.EntityLabel} has already a placement #{product.ObjectPlacement.EntityLabel}");
                    }
                }
                else
                {
                    throw new OperationCanceledException("No IfcProduct as head of current hierarchy");
                }
            });
            return placement;
        }

        public E New<E>(Type t, Action<E> modifier = null) where E : IPersistEntity
        {
            E entity = default(E);
            Transactively(s =>
            {
                entity = (E)ifcEntityScope.New<E>(t);
                modifier?.Invoke(entity);
            });
            return entity;
        }

        public IIfcSite NewSite(string siteName = null)
        {
            IIfcSite site = null;
            Transactively(s =>
            {
                site = ifcProductScope.NewOf<IIfcSite>();
                site.OwnerHistory = OwnerHistoryTag;
                site.Name = siteName;

                NewContainer(site);
            });

            return site;
        }

        public IIfcBuilding NewBuilding(string buildingName = null)
        {
            IIfcBuilding building = null;
            Transactively(s =>
            {
                building = ifcProductScope.NewOf<IIfcBuilding>();
                building.OwnerHistory = OwnerHistoryTag;
                building.Name = buildingName;

                NewContainer(building);
            });

            return building;
        }

        public IIfcBuildingStorey NewStorey(string name = null, double elevation = 0)
        {
            IIfcBuildingStorey storey = null;
            Transactively(s =>
            {
                storey = ifcProductScope.NewOf<IIfcBuildingStorey>();
                storey.Name = name;
                storey.OwnerHistory = OwnerHistoryTag;
                storey.Elevation = elevation;

                NewContainer(storey);
            });

            return storey;
        }

        private void InitProduct(IIfcProduct product)
        {
            var cScope = CurrentScope;
            if (cScope is IIfcSpatialStructureElement e)
            {
                // If spatial container at head, create containment
                model.NewContains(e).RelatedElements.Add(product);
            }
            else
            {
                // Otherwise create an aggregation relation
                model.NewDecomposes(cScope).RelatedObjects.Add(product);
            }
        }

        /// <summary>
        /// New product instance given by type parameter.
        /// </summary>
        /// <typeparam name="P">The product type</typeparam>
        /// <param name="placement">A placement</param>
        /// <param name="name">An optional name</param>
        /// <returns>New stored product</returns>
        public P NewProduct<P>(IIfcLocalPlacement placement = null, string name = null) where P : IIfcProduct
        {
            P product = default(P);
            Transactively(s =>
            {
                product = ifcProductScope.NewOf<P>();
                product.Name = name;
                product.ObjectPlacement = placement;
                InitProduct(product);
            });
            return product;
        }

        /// <summary>
        /// New product instance given by type parameter.
        /// </summary>
        /// <param name="productName">A type label of the product instance</param>
        /// <param name="placement">A placement</param>
        /// <param name="name">An optional name</param>
        /// <returns>New stored product</returns>
        public IIfcProduct NewProduct(Qualifier productName, IIfcLocalPlacement placement = null, string name = null)
        {
            IIfcProduct product = null;
            if (!schema.IsSuperQualifierOf(productName))
                throw new ArgumentException($"Wrong schema version of pName. Store is a {model.SchemaVersion}");

            Transactively(s =>
            {
                product = ifcProductScope.New(productName);
                product.Name = name;
                product.ObjectPlacement = placement;
                InitProduct(product);
            });
            return product;
        }

        /// <summary>
        /// Wrap subsequent product creation by given product into a assembly group.
        /// </summary>
        /// <param name="p">The new group product</param>
        public void NewScope(IIfcProduct p)
        {
            if (InstanceScopeStack.Any(e => e == p))
                throw new ArgumentException($"#{p.EntityLabel} already scoped.");

            NewContainer(p);
        }

        /// <summary>
        /// Drop current product scope.
        /// </summary>
        /// <returns>Dropped container or null, if there's no.</returns>
        public IIfcObjectDefinition DropCurrentScope()
        {
            if (InstanceScopeStack.Count > 1)
                return InstanceScopeStack.Pop();
            else
                return null;
        }

        public P NewProperty<P>(string propertyName, string description = null) where P : IIfcProperty
        {
            P property = default(P);
            Transactively(s =>
            {
                property = ifcPropertyScope.NewOf<P>();
                property.Name = propertyName;
                property.Description = description;
            });
            return property;
        }

        public T NewValueType<T>(object value) where T : IIfcValue
        {
            return ifcValueScope.NewOf<T>(value);
        }

        public IIfcRelDefinesByProperties NewPropertySet(string propertySetName, string description = null, IIfcProduct initialProduct = null)
        {
            IIfcRelDefinesByProperties pSetRel = null;
            Transactively(s =>
            {
                var set = s.NewIfcPropertySet(propertySetName, description);
                pSetRel = s.NewIfcRelDefinesByProperties(set);
                if(null != initialProduct)
                    pSetRel.RelatedObjects.Add(initialProduct);
            });
            return pSetRel;
        }
    }
}
