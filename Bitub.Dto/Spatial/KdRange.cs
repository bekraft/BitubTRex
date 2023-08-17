using System.Collections.Generic;
using System.Linq;

namespace Bitub.Dto.Spatial
{
    public class KdRange : IRangeQuery<KdNode>
    {
        public KdRange() : this(1e-6f, 1e-4f)
        {}
        
        public KdRange(float epsSame, float epsCluster)
        {
            EpsSame = epsSame;
            EpsCluster = epsCluster;
        }
        
        public KdNode Root { get; protected set; }

        /// <summary>
        /// Minimal distance between any two points to be distinct.
        /// </summary>
        public float EpsSame { get; private set; } 

        /// <summary>
        /// Minimal distance between any two points to be clustered.
        /// </summary>
        public float EpsCluster { get; private set; }

        /// <summary>
        /// The current Abox of this point cloud.
        /// </summary>
        public ABox ABox { get; private set; } = Spatial.ABox.Empty;

        /// <summary>
        /// Appends a new point ot Kd range tree.
        /// </summary>
        /// <param name="xyz">The point.</param>
        /// <returns>The new or present node.</returns>
        public KdNode Append(XYZ xyz)
        {
            if (null == Root)
            {
                Root = new KdNode(xyz);
                this.ABox = new ABox { Min = new XYZ(xyz), Max = new XYZ(xyz) };
                return Root;
            }
            else
            {
                // Add point
                var newNode = Root.Propagate(xyz, EpsSame);
                this.ABox = this.ABox.UnionWith(xyz);

                // Absorb cluster if required
                foreach (var node in 
                         new KdNodeEnumerable(Root, new ABox(xyz, EpsCluster))
                             .Where(n => n.Point.Distance(xyz) < EpsCluster))
                {
                    node.Absorb(newNode, EpsCluster);
                }

                return newNode;
            }
        }

        public IEnumerable<XYZ> Points => new KdNodeEnumerable(Root).Select(n => n.Point);

        public IEnumerable<XYZ> PointsWithin(ABox aBox) => new KdNodeEnumerable(Root, aBox).Select(n => n.Point);

        public IEnumerable<XYZ> NearestNeighbors(XYZ xyz, float range)
        {
            return PointsWithin(new ABox(xyz, range)).Where(p => p.Distance(xyz) < range);
        }
    }
}