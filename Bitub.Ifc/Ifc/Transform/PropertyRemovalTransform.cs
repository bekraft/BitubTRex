using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;

using Microsoft.Extensions.Logging;

using Bitub.Dto;

namespace Bitub.Ifc.Transform
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
    public sealed class PropertyRemovalTransformPackage : TransformPackage
    {
        public PropertyRemovalStrategy RemovalStrategy { get; private set; }
        public Qualifier[] RemovePropertyByQualifier { get; private set; }

        internal PropertyRemovalTransformPackage(IModel aSource, IModel aTarget, 
            CancelableProgressing progressMonitor, Qualifier[] propertyQualifiers) 
            : base(aSource, aTarget, progressMonitor)
        {
            RemovePropertyByQualifier = propertyQualifiers.ToArray();
        }
    }


    /// <summary>
    /// Removes specifc properties by full qualified names.
    /// </summary>
    public class PropertyRemovalTransform : ModelTransformTemplate<PropertyRemovalTransformPackage>
    {
        /// <summary>
        /// The logger.
        /// </summary>
        public override ILogger Log { get; protected set; }

        public override string Name { get => "Property Removal"; }

        protected override PropertyRemovalTransformPackage CreateTransformPackage(IModel aSource, IModel aTarget, CancelableProgressing progressMonitor)
        {
            throw new NotImplementedException();
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, PropertyRemovalTransformPackage package)
        {
            throw new NotImplementedException();
        }
    }
}
