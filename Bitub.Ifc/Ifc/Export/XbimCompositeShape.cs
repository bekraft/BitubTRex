using System.Collections.Generic;
using System.Linq;
using Bitub.Dto.Scene;
using Xbim.Common.Geometry;

namespace Bitub.Ifc.Export
{
    // Aggregates component and remaining shape labels.
    public sealed class XbimCompositeShape
    {
        // Sorted list
        private List<int> instanceLabels = new List<int>();
        private List<Shape> shapeList = new List<Shape>();

        public XbimCompositeShape(IEnumerable<XbimShapeInstance> productShapeInstances)
        {
            instanceLabels = productShapeInstances.Select(i => i.InstanceLabel).OrderBy(i => i).ToList();
        }

        public bool MarkDone(XbimShapeInstance productShapeInstance)
        {
            var idx = instanceLabels.BinarySearch(productShapeInstance.InstanceLabel);
            if (0 > idx)
                return false;
            else
                instanceLabels.RemoveAt(idx);

            return true;
        }

        public bool Add(XbimShapeInstance productShapeInstance, Shape productShape)
        {
            var isHeldAndDone = MarkDone(productShapeInstance);
            shapeList.Add(productShape);
            return isHeldAndDone;
        }

        internal IEnumerable<Shape> Shapes
        {
            get => shapeList.ToArray();
        }

        internal bool IsComplete
        {
            get => instanceLabels.Count == 0;
        }
    }
}
