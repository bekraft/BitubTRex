using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitub.Ifc.Transform;
using Xbim.Common;

using Microsoft.Extensions.Logging;

using IfcProductPredicate = System.Func<Xbim.Ifc4.Interfaces.IIfcProduct, bool>;

namespace Bitub.Ifc.Transform.Requests
{    
    /// <summary>
    /// The prodzct transformation type.
    /// </summary>
    [Flags]
    public enum IfcProductTransformType : int
    {
        NoRemoval = 0,
        RemoveWithChildren = 1,
        ReparentChildren = 2,
        RemoveRepresentation = 4,
        RemovePropertis = 8
    }

    /// <summary>
    /// The product transformation package.
    /// </summary>
    public class IfcProductFilterPackage : TransformPackage
    {
        public readonly IEnumerable<IfcProductPredicate> InclusionPredicates;
        public readonly IEnumerable<IfcProductPredicate> ExclusionPredicates;
        public readonly bool IsOverrideWithInclude;
        public readonly IfcProductTransformType ProductTransformType;

        internal protected IfcProductFilterPackage(IModel aSource, IModel aTarget,
            bool isOverrideWithInclude, IEnumerable<IfcProductPredicate> include, IEnumerable<IfcProductPredicate> exclude): base(aSource, aTarget)
        {
            IsOverrideWithInclude = isOverrideWithInclude;
            InclusionPredicates = include;
            ExclusionPredicates = exclude;
        }
    }

    /// <summary>
    /// IFC product filtering & transformation request.
    /// </summary>
    public class IfcProductFilterRequest : IfcTransformRequestTemplate<IfcProductFilterPackage>
    {
        /// <summary>
        /// The logger.
        /// </summary>
        public override ILogger Log { get; protected set; }

        public override string Name => "IFC Product Filter";

        public ICollection<IfcProductPredicate> IncludeProducts { get; } = new List<IfcProductPredicate>();
        public ICollection<IfcProductPredicate> ExcludeProducts { get; } = new List<IfcProductPredicate>();

        public bool IsIncludeOverride { get; set; } = true;

        public override bool IsInplaceTransform => throw new NotImplementedException();

        protected override bool IsNoopTransform => throw new NotImplementedException();

        public IfcProductFilterRequest()
        {
        }

        protected override IfcProductFilterPackage CreateTransformPackage(IModel aSource, IModel aTarget)
        {
            return new IfcProductFilterPackage(aSource, aTarget, IsIncludeOverride, IncludeProducts, ExcludeProducts);
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, IfcProductFilterPackage package)
        {
            throw new NotImplementedException();
        }
    }
}
