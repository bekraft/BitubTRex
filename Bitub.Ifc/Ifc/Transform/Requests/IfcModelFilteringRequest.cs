using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;
using Xbim.Common.Metadata;

namespace Bitub.Ifc.Transform.Requests
{
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
            expressTypes = typeFilter ?? new ExpressType[] { };
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

        protected override IPersistEntity DelegateCopy(IPersistEntity instance, IfcModelFilteringPackage package)
        {
            return base.DelegateCopy(instance, package);
        }

        protected override object PropertyTransform(ExpressMetaProperty property, object hostObject, IfcModelFilteringPackage package)
        {
            return base.PropertyTransform(property, hostObject, package);
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, IfcModelFilteringPackage package)
        {
            throw new NotImplementedException();
        }
    }
}
