using Bitub.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Ifc4.Interfaces;

using Microsoft.Extensions.Logging;

namespace Bitub.Ifc.Transform.Requests
{
    [Flags]
    public enum IfcRepresentationSplitStrategy
    {
        /// <summary>
        /// Default splitting multiple representation items by cloneing embedding product.
        /// </summary>
        SplitMultiBodyRepresentation = 0,
        /// <summary>
        /// Additionally split geometrical components.
        /// </summary>
        SplitGeometricalBodyComponents = 1,        
        /// <summary>
        /// Additionally refactor hierarchy using <see cref="IIfcElementAssembly"/> entities.
        /// </summary>
        EntityElementAssemblyRefactor = 2
    }

    public class IfcRepresentationSplitResultPackage : TransformPackage
    {
        public IfcRepresentationSplitStrategy Strategy { get; private set; }

        public ISet<string> ContextIdentifier { get; private set; }

        protected internal IfcBuilder Builder { get; private set; }

        protected internal IfcRepresentationSplitResultPackage(IModel aSource, IModel aTarget, 
            IfcRepresentationSplitStrategy strategy, string[] contextIdentifiers) 
            : base(aSource, aTarget)
        {
            Strategy = strategy;
            ContextIdentifier = new HashSet<string>(contextIdentifiers);
        }

        protected internal bool IsMultibodyRepresentation(IIfcRepresentation r, IfcRepresentationSplitStrategy strategy)
        {
            var isInContext = ContextIdentifier.Contains(r.ContextOfItems.ContextIdentifier.ToString().ToLower());
            if (strategy.HasFlag(IfcRepresentationSplitStrategy.SplitMultiBodyRepresentation))
                return isInContext && r.Items.Count > 1;

            throw new NotImplementedException($"{strategy}");
        }
    }

    public class IfcRepresentationSplitRequest : IfcTransformRequestTemplate<IfcRepresentationSplitResultPackage>
    {
        public override string Name => "IFC Representation Entity Split";

        public override ILogger Log { get; protected set; }

        public IfcRepresentationSplitRequest(ILoggerFactory loggerFactory)
        {
            Log = loggerFactory.CreateLogger<IfcRepresentationSplitRequest>();
        }

        public IfcRepresentationSplitStrategy Strategy { get; set; } = IfcRepresentationSplitStrategy.SplitMultiBodyRepresentation;

        public string[] ContextIdentifiers { get; set; } = new[] { "Body" };

        protected override IfcRepresentationSplitResultPackage CreateTransformPackage(IModel aSource, IModel aTarget)
        {
            return new IfcRepresentationSplitResultPackage(aSource, aTarget, Strategy, ContextIdentifiers);
        }        

        protected override TransformActionType PassInstance(IPersistEntity instance, IfcRepresentationSplitResultPackage package)
        {
            if (instance is IIfcProduct p)
            {
                var items = p.Representation
                    .Representations
                    .Where(r => package.ContextIdentifier.Contains(r.ContextOfItems.ContextIdentifier.ToString().ToLower()) && r.Items.Count > 1)
                    .ToArray();

                if (items.Length > 1)
                    return TransformActionType.Delegate;
            }

            return TransformActionType.Copy;
        }

        protected override IPersistEntity DelegateCopy(IPersistEntity instance, 
            IfcRepresentationSplitResultPackage package, CancelableProgressing cp)
        {
            return base.DelegateCopy(instance, package, cp);
        }

        protected override object PropertyTransform(ExpressMetaProperty property, 
            object hostObject, IfcRepresentationSplitResultPackage package, CancelableProgressing cp)
        {
            // Exclude representations
            return base.PropertyTransform(property, hostObject, package, cp);
        }
    }
}
