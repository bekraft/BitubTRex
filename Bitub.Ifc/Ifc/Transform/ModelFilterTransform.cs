using Microsoft.Extensions.Logging;
using System;

using Xbim.Common;
using Xbim.Common.Metadata;

using Bitub.Dto;

namespace Bitub.Ifc.Transform
{
    /// <summary>
    /// Model filter strategy used by <see cref="ModelFilterTransform"/>.
    /// </summary>
    [Flags]
    public enum ModelFilterStrategy
    {
        /// <summary>
        /// Pure copy without any relations. If not specified with relations, all matching elements will be aggregated into project scope.
        /// </summary>
        PureCopy = 0x00,
        
        /// <summary>
        /// Transfer IFC decomposition relations with direct or indirect scope to matching elements.
        /// </summary>
        WithIfcRelDecomposes = 0x01,

        /// <summary>
        /// Transfer IFC spatial relations with direct or indirect scope to matching elements.
        /// </summary>
        WithIfcRelContainedInSpatialStructure = 0x02,

        /// <summary>
        /// Transfer IFC property relations with direct or indirect scope to matching elements.
        /// </summary>
        WithIfcRelDefinesByProperties = 0x04,

        /// <summary>
        /// Transfer IFC type relations with direct or indirect scope to matching elements.
        /// </summary>
        WithIfcRelDefinesByType = 0x08,

        /// <summary>
        /// Transfer IFC product representation relations with direct or indirect scope to matching elements.
        /// </summary>
        WithIfcRepresentation = 0x10,

        /// <summary>
        /// Combined flag of all relationship flags.
        /// </summary>
        WithAllIfcRelations = 0x0f,
    }

    public class ModelFilterTransformPackage : TransformPackage
    {
        public int[] ExclusiveEntityLabels { get; private set; }
        public ModelFilterStrategy RelationalStrategy { get; private set; }

        public ModelFilterTransformPackage(IModel source, IModel target, CancelableProgressing progressMonitor,
            int[] labelFilter, ModelFilterStrategy rules = 0) : base(source, target, progressMonitor)
        {
            ExclusiveEntityLabels = labelFilter ?? new int[] { };
            Array.Sort(ExclusiveEntityLabels);
            RelationalStrategy = rules;            
        }

        internal bool IsAccepted(IPersistEntity entity)
        {
            return 0 > Array.BinarySearch(ExclusiveEntityLabels, entity.EntityLabel);
        }
    }

    /// <summary>
    /// Filtering request which will restrict the model output to the given explicitely and exclusively given entity labels and/or express types.
    /// Additionally, the relational strategy will embed decomposition, spatial and semantical references.
    /// </summary>
    public class ModelFilterTransform : ModelTransformTemplate<ModelFilterTransformPackage>
    {
        public override string Name => "Model filtering";

        public override ILogger Log { get; protected set; }

        public int[] ExclusiveEntityLabels { get; set; } = new int[] { };

        public ModelFilterStrategy RelationalStrategy { get; set; } = ModelFilterStrategy.PureCopy;

        protected override ModelFilterTransformPackage CreateTransformPackage(IModel aSource, IModel aTarget, 
            CancelableProgressing progressMonitor)
        {
            return new ModelFilterTransformPackage(aSource, aTarget, progressMonitor, ExclusiveEntityLabels, RelationalStrategy);
        }

        protected override object PropertyTransform(ExpressMetaProperty property, 
            object hostObject, ModelFilterTransformPackage package)
        {
            return base.PropertyTransform(property, hostObject, package);
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, 
            ModelFilterTransformPackage package)
        {
            if (!package.IsAccepted(instance))
                return TransformActionType.Drop;
            else
                return TransformActionType.Copy;
        }
    }
}
