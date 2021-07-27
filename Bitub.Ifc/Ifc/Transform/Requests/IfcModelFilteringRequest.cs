using Microsoft.Extensions.Logging;
using System;

using Xbim.Common;
using Xbim.Common.Metadata;

using Bitub.Dto;

namespace Bitub.Ifc.Transform.Requests
{
    [Flags]
    public enum FilteringRules
    {
        WithInverseAggregation = 0x01,
        WithInverseSpatialReferences = 0x02,
        WithInverseProductRepresentation = 0x04,

        WithAllInverseRelations = 0x0f
    }

    public class IfcModelFilteringPackage : TransformPackage
    {
        public readonly int[] entityLabels;
        public readonly ExpressType[] expressTypes;
        public readonly FilteringRules filteringFlag;

        public IfcModelFilteringPackage(IModel source, IModel target, 
            int[] labelFilter, ExpressType[] typeFilter, FilteringRules rules = 0) : base(source, target)
        {
            entityLabels = labelFilter ?? new int[] { };
            Array.Sort(entityLabels);
            expressTypes = typeFilter ?? new ExpressType[] { };
            Array.Sort(expressTypes, (a, b) => Math.Sign(a.TypeId - b.TypeId));
            filteringFlag = rules;
        }
    }

    public class IfcModelFilteringRequest : IfcTransformRequestTemplate<IfcModelFilteringPackage>
    {
        public override string Name => "Model filtering";

        public override ILogger Log { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        protected override IfcModelFilteringPackage CreateTransformPackage(IModel aSource, IModel aTarget)
        {
            throw new NotImplementedException();
        }

        protected override IPersistEntity DelegateCopy(IPersistEntity instance, IfcModelFilteringPackage package, CancelableProgressing cp)
        {
            return base.DelegateCopy(instance, package, cp);
        }

        protected override object PropertyTransform(ExpressMetaProperty property, object hostObject, IfcModelFilteringPackage package, CancelableProgressing cp)
        {
            return base.PropertyTransform(property, hostObject, package, cp);
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, IfcModelFilteringPackage package)
        {
            throw new NotImplementedException();
        }
    }
}
