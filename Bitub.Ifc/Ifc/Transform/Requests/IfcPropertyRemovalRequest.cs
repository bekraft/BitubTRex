using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;

using Microsoft.Extensions.Logging;

using Bitub.Transfer;

namespace Bitub.Ifc.Transform.Requests
{
    public enum IfcPropertyRemovalStrategy
    {
        /// <summary>
        /// Leaves remaining empty psets untouched.
        /// </summary>
        LeaveEmptyPset,
        /// <summary>
        /// Cleans up for empty sets.
        /// </summary>
        CleanEmptyPset
    }

    /// <summary>
    /// Property removal packages.
    /// </summary>
    public class IfcPropertyRemovalPackage : TransformPackage
    {
        public readonly IfcPropertyRemovalStrategy RemovalStrategy;
        public readonly Qualifier[] RemovePropertyByQualifier;

        internal IfcPropertyRemovalPackage(IModel aSource, IModel aTarget, Qualifier[] propertyQualifiers) 
            : base(aSource, aTarget)
        {
            RemovePropertyByQualifier = propertyQualifiers.ToArray();
        }
    }


    /// <summary>
    /// Removes specifc properties by full qualified names.
    /// </summary>
    public class IfcPropertyRemovalRequest : IfcTransformRequestTemplate<IfcPropertyRemovalPackage>
    {
        /// <summary>
        /// The logger.
        /// </summary>
        public override ILogger Log { get; protected set; }

        public override string Name { get => "Property Removal"; }

        public override bool IsInplaceTransform => throw new NotImplementedException();

        protected override bool IsNoopTransform => throw new NotImplementedException();

        protected override IfcPropertyRemovalPackage CreateTransformPackage(IModel aSource, IModel aTarget)
        {
            throw new NotImplementedException();
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, IfcPropertyRemovalPackage package)
        {
            throw new NotImplementedException();
        }
    }
}
