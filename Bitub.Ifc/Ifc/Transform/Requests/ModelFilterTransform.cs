using Microsoft.Extensions.Logging;
using System;

using Xbim.Common;
using Xbim.Common.Metadata;

using Bitub.Dto;

namespace Bitub.Ifc.Transform.Requests
{
    [Flags]
    public enum ModelFilterStrategy
    {
        CopyInstanceOnly = 0x00,
        WithDecomposition = 0x01,
        WithContainment = 0x02,
        WithPropertySet = 0x04,

        WithRepresentation = 0x10,

        WithAllRelations = 0x0f
    }

    public class ModelFilterTransformPackage : TransformPackage
    {
        public int[] EntityLabels { get; private set; }
        public ExpressType[] ExpressTypes { get; private set; }
        public ModelFilterStrategy RelationalStrategy { get; private set; }

        public ModelFilterTransformPackage(IModel source, IModel target, CancelableProgressing progressMonitor,
            int[] labelFilter, ExpressType[] typeFilter, ModelFilterStrategy rules = 0) : base(source, target, progressMonitor)
        {
            EntityLabels = labelFilter ?? new int[] { };
            Array.Sort(EntityLabels);
            ExpressTypes = typeFilter ?? new ExpressType[] { };
            Array.Sort(ExpressTypes, (a, b) => Math.Sign(a.TypeId - b.TypeId));
            RelationalStrategy = rules;            
        }
    }

    /// <summary>
    /// Filtering request which will restrict the model output to the given explicitely and exclusively given entity labels and/or express types.
    /// Additionally, the relational strategy will embed decomposition, spatial and semantical references.
    /// </summary>
    public class ModelFilterTransform : ModelTransformTemplate<ModelFilterTransformPackage>
    {
        public override string Name => "Model filtering";

        public override ILogger Log { get; protected set; }

        public int[] ExclusiveEntityLabels { get; set; } = new int[] { };

        public ExpressType[] ExclusiveExpressTypes { get; set; } = new ExpressType[] { };

        public ModelFilterStrategy RelationalStrategy { get; set; } = ModelFilterStrategy.CopyInstanceOnly;

        protected override ModelFilterTransformPackage CreateTransformPackage(IModel aSource, IModel aTarget, 
            CancelableProgressing progressMonitor)
        {
            return new ModelFilterTransformPackage(aSource, aTarget, progressMonitor, ExclusiveEntityLabels, ExclusiveExpressTypes, RelationalStrategy);
        }

        protected override object PropertyTransform(ExpressMetaProperty property, 
            object hostObject, ModelFilterTransformPackage package)
        {
            return base.PropertyTransform(property, hostObject, package);
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, 
            ModelFilterTransformPackage package)
        {
            throw new NotImplementedException();
        }
    }
}
