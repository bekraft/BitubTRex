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
    public sealed class ProductPropertyTransform : TransformPackage
    {
        private ILookup<string, FeatureEntityMapping> perSetRules;
        private FeatureStageCache featureStageCache;

        internal ProductPropertyTransform(FeatureEntityMapping[] mappingRules)
        {
            featureStageCache = new FeatureStageCache();
        }

        private void Init(FeatureEntityMapping[] rules)
        {            
        }
    }

    public class ProductPropertyTransformRequest : IfcTransformRequestTemplate<ProductPropertyTransform>
    {
        public FeatureEntityMapping[] PropertyMappingRules { get; set; }

        public override string Name => "IFC Product Property Mapping";

        public override ILogger Log { get; protected set; }

        protected override ProductPropertyTransform CreateTransformPackage(IModel aSource, IModel aTarget,
            CancelableProgressing cancelableProgressing)
        {
            throw new NotImplementedException();
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, 
            ProductPropertyTransform package, CancelableProgressing cancelableProgressing)
        {
            throw new NotImplementedException();
        }
    }
}
