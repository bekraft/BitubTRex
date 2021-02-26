using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitub.Dto.Concept;
using Bitub.Ifc.Concept;
using Bitub.Ifc.TRex;

using Xbim.Common;

namespace Bitub.Ifc.Transform.Requests
{
    public class IfcProductPropertyTransform : TransformPackage
    {
        // General filter
        public readonly CanonicalFilterRule entityTypeFilter;
        // Product enhancement filter

        // Product property mapping filter
    }

    public class IfcProductPropertyTransformRequest : IfcTransformRequestTemplate<IfcProductPropertyTransform>
    {
        public CanonicalFilterRule EntityTypeFilter { get; set; }
        public FeatureMapping[] FeatureMapping { get; set; }

        public override string Name => throw new NotImplementedException();

        public override ILogger Log { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

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
