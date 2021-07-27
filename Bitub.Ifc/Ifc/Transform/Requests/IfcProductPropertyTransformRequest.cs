using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using Bitub.Dto;
using Bitub.Dto.Concept;
using Bitub.Ifc.Concept;
using Bitub.Ifc.TRex;

using Xbim.Common;
using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc.Transform.Requests
{
    public class IfcProductPropertyTransform : TransformPackage
    {
        private ILookup<string, FeatureEntityMapping> perSetRules;
        private FeatureStageCache featureStageCache;

        internal IfcProductPropertyTransform(FeatureEntityMapping[] mappingRules)
        {
            featureStageCache = new FeatureStageCache();
        }

        private void Init(FeatureEntityMapping[] rules)
        {            
        }
    }

    public class IfcProductPropertyTransformRequest : IfcTransformRequestTemplate<IfcProductPropertyTransform>
    {
        public FeatureEntityMapping[] PropertyMappingRules { get; set; }

        public override string Name => "IFC Product Property Mapping";

        public override ILogger Log { get; protected set; }

        protected override IfcProductPropertyTransform CreateTransformPackage(IModel aSource, IModel aTarget)
        {
            throw new NotImplementedException();
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, IfcProductPropertyTransform package)
        {
            throw new NotImplementedException();
        }
    }
}
