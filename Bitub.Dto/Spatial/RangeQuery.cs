using System.Collections.Generic;

namespace Bitub.Dto.Spatial
{
    /// <summary>
    /// A spatial node contract referring to a single spatial point.
    /// </summary>
    public interface ISpatialNode
    {
        public XYZ Point { get; }
    }
    
    /// <summary>
    /// Range query contract able to query points within a given range.
    /// </summary>
    public interface IRangeQuery<out T> where T : IRangeCluster
    {
        public ABox ABox { get; }
        public IEnumerable<XYZ> Points { get; }
        public IEnumerable<XYZ> PointsWithin(ABox aBox);
        public IEnumerable<XYZ> NearestNeighbors(XYZ xyz, float range);
        public T Append(XYZ xyz);
    }

    /// <summary>
    /// A range cluster contract aggregating several points to a cluster.
    /// </summary>
    public interface IRangeCluster
    {
        public XYZ Center { get; }
        public IEnumerable<XYZ> ClusterPoints { get; }
        public int ClusterWeight { get; }
        public ABox ABox { get; }
    }
}