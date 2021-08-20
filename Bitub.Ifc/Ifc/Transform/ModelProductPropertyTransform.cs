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

namespace Bitub.Ifc.Transform
{
    public sealed class ModelProductPropertyTransformPackage : TransformPackage
    {
        private ILookup<string, FeatureEntityMapping> perSetRules;
        private FeatureStageCache featureStageCache;

        internal ModelProductPropertyTransformPackage(IModel s, IModel t,
            CancelableProgressing progressMonitor, FeatureEntityMapping[] mappingRules)
            : base(s, t, progressMonitor)
        {
            featureStageCache = new FeatureStageCache();
        }
    }

    public class ModelProductPropertyTransform : ModelTransformTemplate<ModelProductPropertyTransformPackage>
    {
        public FeatureEntityMapping[] PropertyMappingRules { get; set; }

        public override string Name => "IFC Product Property Mapping";

        public override ILogger Log { get; protected set; }

        protected override ModelProductPropertyTransformPackage CreateTransformPackage(IModel aSource, IModel aTarget,
            CancelableProgressing cancelableProgressing)
        {
            throw new NotImplementedException();
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, 
            ModelProductPropertyTransformPackage package)
        {
            throw new NotImplementedException();
        }
    }
}
