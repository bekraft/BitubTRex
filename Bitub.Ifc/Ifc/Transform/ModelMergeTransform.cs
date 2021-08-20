using System;

using System.Collections.Generic;

using Xbim.Common;
using Xbim.Common.Geometry;

using Xbim.Ifc4.Interfaces;

using Xbim.ModelGeometry.Scene;

using Xbim.Geometry.Engine.Interop;
using Microsoft.Extensions.Logging;
using Bitub.Dto;

namespace Bitub.Ifc.Transform
{
    public sealed class ModelMergeTransformPackage : TransformPackage
    {
        private IDictionary<IModel, XbimPlacementTree> placements = new Dictionary<IModel, XbimPlacementTree>();
        private IDictionary<XbimInstanceHandle, XbimMatrix3D> tInverted = new Dictionary<XbimInstanceHandle, XbimMatrix3D>();
        private XbimGeometryEngine engine;

        internal XbimGeometryEngine Engine
        {
            get { return engine ?? (engine = new XbimGeometryEngine()); }
        }

        internal XbimMatrix3D PlacementOf(IIfcProduct p)
        {
            XbimPlacementTree tree;
            if (!placements.TryGetValue(p.Model, out tree))
            {
                tree = new XbimPlacementTree(p.Model, false);
                placements.Add(p.Model, tree);
            }
            return XbimPlacementTree.GetTransform(p, tree, Engine);
        }

        internal XbimMatrix3D NewPlacementRelative(IIfcProduct container, IIfcProduct newRelativeProduct)
        {
            XbimMatrix3D t;
            var handle = new XbimInstanceHandle(container);
            if (tInverted.TryGetValue(handle, out t))
            {
                // Compute inv of container local placement
                t = PlacementOf(container);
                t.Invert();
                tInverted.Add(handle, t);
            }

            // New placement based on given global transformation
            return t * PlacementOf(newRelativeProduct);            
        }
    }

    public class ModelMergeTransform : ModelTransformTemplate<ModelMergeTransformPackage>
    {
        public override string Name => throw new NotImplementedException();

        public override ILogger Log { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        protected override ModelMergeTransformPackage CreateTransformPackage(IModel aSource, IModel aTarget, CancelableProgressing progressMonitor)
        {
            throw new NotImplementedException();
        }

        protected override TransformActionType PassInstance(IPersistEntity instance, ModelMergeTransformPackage package)
        {
            throw new NotImplementedException();
        }
    }
}
