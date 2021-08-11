using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;

using Microsoft.Extensions.Logging;

using Bitub.Dto;

namespace Bitub.Ifc.Transform.Requests
{
    public enum PropertyRemovalStrategy
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
    public sealed class PropertyRemovalPackage : TransformPackage
    {
        public PropertyRemovalStrategy RemovalStrategy { get; private set; }
        public Qualifier[] RemovePropertyByQualifier { get; private set; }

        internal PropertyRemovalPackage(IModel aSource, IModel aTarget, Qualifier[] propertyQualifiers) 
            : base(aSource, aTarget)
        {
            RemovePropertyByQualifier = propertyQualifiers.ToArray();
        }
    }


    /// <summary>
    /// Removes specifc properties by full qualified names.
    /// </summary>
    public class PropertyRemovalRequest : IfcTransformRequestTemplate<PropertyRemovalPackage>
    {
        /// <summary>
        /// The logger.
        /// </summary>
        public override ILogger Log { get; protected set; }

        public override string Name { get => "Property Removal"; }

        protected override PropertyRemovalPackage CreateTransformPackage(IModel aSource, IModel aTarget, CancelableProgressing cancelableProgressing)
        {
            throw new NotImplementedException();
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, PropertyRemovalPackage package, CancelableProgressing cancelableProgressing)
        {
            throw new NotImplementedException();
        }
    }
}
