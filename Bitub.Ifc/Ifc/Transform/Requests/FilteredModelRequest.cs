using Microsoft.Extensions.Logging;
using System;

using Xbim.Common;
using Xbim.Common.Metadata;

using Bitub.Dto;

namespace Bitub.Ifc.Transform.Requests
{
    [Flags]
    public enum FilteringRelationsStrategy
    {
        CopyInstanceOnly = 0x00,
        WithDecomposition = 0x01,
        WithContainment = 0x02,
        WithPropertySet = 0x04,

        WithRepresentation = 0x10,

        WithAllRelations = 0x0f
    }

    public class FilteredModelPackage : TransformPackage
    {
        public int[] EntityLabels { get; private set; }
        public ExpressType[] ExpressTypes { get; private set; }
        public FilteringRelationsStrategy RelationalStrategy { get; private set; }

        public FilteredModelPackage(IModel source, IModel target, 
            int[] labelFilter, ExpressType[] typeFilter, FilteringRelationsStrategy rules = 0) : base(source, target)
        {
            EntityLabels = labelFilter ?? new int[] { };
            Array.Sort(EntityLabels);
            ExpressTypes = typeFilter ?? new ExpressType[] { };
            Array.Sort(ExpressTypes, (a, b) => Math.Sign(a.TypeId - b.TypeId));
            RelationalStrategy = rules;        }
    }

    /// <summary>
    /// Filtering request which will restrict the model output to the given explicitely and exclusively given entity labels and/or express types.
    /// Additionally, the relational strategy will embed decomposition, spatial and semantical references.
    /// </summary>
    public class FilteredModelRequest : IfcTransformRequestTemplate<FilteredModelPackage>
    {
        public override string Name => "Model filtering";

        public override ILogger Log { get; protected set; }

        public int[] ExclusiveEntityLabels { get; set; } = new int[] { };

        public ExpressType[] ExclusiveExpressTypes { get; set; } = new ExpressType[] { };

        public FilteringRelationsStrategy RelationalStrategy { get; set; } = FilteringRelationsStrategy.CopyInstanceOnly;

        protected override FilteredModelPackage CreateTransformPackage(IModel aSource, IModel aTarget, 
            CancelableProgressing cancelableProgressing)
        {
            return new FilteredModelPackage(aSource, aTarget, ExclusiveEntityLabels, ExclusiveExpressTypes, RelationalStrategy);
        }

        protected override object PropertyTransform(ExpressMetaProperty property, 
            object hostObject, FilteredModelPackage package, CancelableProgressing cancelableProgressing)
        {
            return base.PropertyTransform(property, hostObject, package, cancelableProgressing);
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, 
            FilteredModelPackage package, CancelableProgressing cancelableProgressing)
        {
            throw new NotImplementedException();
        }
    }
}
