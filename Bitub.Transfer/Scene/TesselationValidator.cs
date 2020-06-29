using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TopoEdge = System.ValueTuple<int, int>;

namespace Bitub.Transfer.Scene
{
    [Flags]
    public enum TesselationIssueType
    {
        None = 0,
        TopologyOpen = 1,
        TopologyClustered = 2,
        GeometryOpen = 4,
        GeometryClustered = 8,
        NonManifold = 32
    }

    public class TesselationValidationResult
    {
        public readonly Component Component;
        public readonly Representation Representation;
        public readonly FaceBody Body;
        public readonly TesselationIssueType IssueType;
        public readonly double? TesselatedVolume;
    }    

    public class TesselationValidator
    {
        /*
        public IEnumerable<TesselationValidationResult> Validate(SceneModel scene)
        {
            foreach(var c in scene.Components)
            {

            }
        }

        protected TesselationIssueType CheckGeometry(IEnumerable<Facet> facets, out double approximateVolume)
        {

        }

        protected TesselationIssueType CheckTopology(IEnumerable<Facet> facets)
        {

        }
        */
    }
}
