using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xbim.Common;

using Xbim.Ifc4.Interfaces;

using Microsoft.Extensions.Logging;
using Bitub.Dto;

namespace Bitub.Ifc.Transform
{
    /// <summary>
    /// Metadata transformation type.
    /// </summary>
    [Flags]
    public enum ModelMetadataRefactorStrategy : int
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
    public sealed class ModelMetadataRefactorTransformPackage : TransformPackage
    {
        public ModelMetadataRefactorStrategy AuthoringTransformType { get; private set; }
        public IfcAuthoringMetadata AuthoringMetadata { get; private set; }

        public IfcBuilder Builder { get; private set; }
        
        internal ModelMetadataRefactorTransformPackage(IModel aSource, IModel aTarget, CancelableProgressing progressMonitor,
            IfcAuthoringMetadata metadata, ModelMetadataRefactorStrategy transformType) : base(aSource, aTarget, progressMonitor)
        {
            AuthoringMetadata = metadata;
            AuthoringTransformType = transformType;
        }
    }

    public class ModelMetadataRefactorTransform : ModelTransformTemplate<ModelMetadataRefactorTransformPackage>
    {
        public override ILogger Log { get; protected set; }

        public override string Name => "Model Metadata Refactoring Transform";

        public IfcAuthoringMetadata AuthoringMetadata { get; set; } = new IfcAuthoringMetadata();

        protected override ModelMetadataRefactorTransformPackage CreateTransformPackage(IModel aSource, IModel aTarget,
            CancelableProgressing cancelableProgressing)
        {
            throw new NotImplementedException();
        }

        protected override IPersistEntity DelegateCopy(IPersistEntity instance, 
            ModelMetadataRefactorTransformPackage package)
        {
            return base.DelegateCopy(instance, package);
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, 
            ModelMetadataRefactorTransformPackage package)
        {
            throw new NotImplementedException();
        }
    }
}
