using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xbim.Common;

using Bitub.Transfer;
using Bitub.Ifc.Transform;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc2x3.Kernel;
using System.Configuration;

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
        public override string Name => "Project Meta Data Change";

        public override bool IsInplaceTransform => throw new NotImplementedException();

        protected override bool IsNoopTransform => throw new NotImplementedException();

        protected override IfcMetadataTransformPackage CreateTransformPackage(IModel aSource, IModel aTarget)
        {
            throw new NotImplementedException();
        }

        protected override IPersistEntity DelegateCopy(IPersistEntity instance, IfcMetadataTransformPackage package)
        {
            return base.DelegateCopy(instance, package);
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, IfcMetadataTransformPackage package)
        {
            throw new NotImplementedException();
        }
    }
}
