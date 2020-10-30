using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xbim.Common;

using Xbim.Ifc4.Interfaces;

using Microsoft.Extensions.Logging;
using Bitub.Dto;

namespace Bitub.Ifc.Transform.Requests
{
    /// <summary>
    /// Metadata transformation type.
    /// </summary>
    [Flags]
    public enum IfcMetadataTransformStrategy : int
    {
        /// <summary>
        /// Tags the project instance with new owner history.
        /// </summary>
        TagProject = 0,
        /// <summary>
        /// Tags the complete model with new owner history.
        /// </summary>
        TagCompleteModel = 1,
        /// <summary>
        /// Refactors the model and cleans all non-uptodate owner history instances (orphans)
        /// </summary>
        CleanUpModel = 2,
        /// <summary>
        /// Tags the entire model and cleans up owner history.
        /// </summary>
        TagAndCleanModel = 3
    }

    /// <summary>
    /// Metadata transformation package.
    /// </summary>
    public class IfcMetadataTransformPackage : TransformPackage
    {
        public readonly IfcMetadataTransformStrategy AuthoringTransformType;
        public readonly IfcAuthoringMetadata AuthoringMetadata;

        private IIfcOwnerHistory _ownerHistoryInstance;        
        
        internal protected IfcMetadataTransformPackage(IModel aSource, IModel aTarget, 
            IfcAuthoringMetadata metadata, IfcMetadataTransformStrategy transformType) : base(aSource, aTarget)
        {
            AuthoringMetadata = metadata;
            AuthoringTransformType = transformType;
        }
    }

    /// <summary>
    /// Transformation request which will change the owner history & journal.
    /// </summary>
    public class IfcProjectMetaDataChangeRequest : IfcTransformRequestTemplate<IfcMetadataTransformPackage>
    {
        /// <summary>
        /// The logger.
        /// </summary>
        public override ILogger Log { get; protected set; }

        public override string Name => "Project Meta Data Change";

        protected override IfcMetadataTransformPackage CreateTransformPackage(IModel aSource, IModel aTarget)
        {
            throw new NotImplementedException();
        }

        protected override IPersistEntity DelegateCopy(IPersistEntity instance, IfcMetadataTransformPackage package, CancelableProgressing cp)
        {
            return base.DelegateCopy(instance, package, cp);
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, IfcMetadataTransformPackage package)
        {
            throw new NotImplementedException();
        }
    }
}
