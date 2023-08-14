using System.Collections.Generic;
using System.Linq;

namespace Bitub.Dto.Spatial
{
    public class DbScanClusterNode<T> : IRangeCluster, ISpatialNode where T : DbScanClusterNode<T>
    {
        // referring the next
        private DbScanClusterNode<T> _coreNext = null;
        
        protected DbScanClusterNode(XYZ xyz)
        {
            Point = xyz;
        }

        private void ComputeWeight(DbScanClusterNode<T> newNode, 
            List<DbScanClusterNode<T>> clusterNode, float epsCluster)
        {
            var coreCount = 0;
            foreach (var node in clusterNode)
            {
                if (newNode != node && epsCluster > newNode.Point.Distance(node.Point))
                    coreCount++;
            }

            newNode.CoreWeight = coreCount;
        }

        protected internal bool Absorb(DbScanClusterNode<T> newNode, float epsCluster)
        {
            var cluster = ClusterRing.ToList();
            if (cluster.Contains(newNode)) 
                return false;

            if (newNode.IsCluster)
            {
                // Bridge from foreign to own
                newNode.Predecessor._coreNext = _coreNext;
                // Bridge from own to foreign
                _coreNext = newNode;
                // Compute weights
                cluster.ForEach(n => ComputeWeight(n, cluster, epsCluster));
            }
            else
            {
                newNode._coreNext = this._coreNext ?? this;
                this._coreNext = newNode;
            }
            
            ComputeWeight(newNode, cluster, epsCluster);

            return true;
        }
        
        protected internal IEnumerable<DbScanClusterNode<T>> ClusterRing
        {
            get
            {
                yield return this;
                for (var nextOnRing = _coreNext;
                     nextOnRing != this && null != nextOnRing; 
                     nextOnRing = nextOnRing._coreNext)
                {
                    yield return nextOnRing;
                }
            }
        }
        
        /// <summary>
        /// Ring's predecessor.
        /// </summary>
        protected internal DbScanClusterNode<T> Successor => _coreNext;
        
        /// <summary>
        /// Ring's successor
        /// </summary>
        protected internal DbScanClusterNode<T> Predecessor => ClusterRing.First(n => n._coreNext == this);

        /// <summary>
        /// Will expel this cluster node from its cluster.
        /// </summary>
        /// <returns>True, if it has been associated to a cluster before.</returns>
        protected internal bool Expel()
        {
            bool isClusterMember = null != _coreNext;
            if (isClusterMember)
            {
                var predecessor = Predecessor;
                predecessor._coreNext = _coreNext;
                _coreNext = null;
                CoreWeight = 0;
            }

            return isClusterMember;
        }
        
        /// <summary>
        /// Returns the weighted center of this cluster.
        /// </summary>
        public XYZ Center
        {
            get
            {
                var clusterInvWeight = 1.0 / ClusterWeight;
                return ClusterRing
                    .Select(n => n.Point * (clusterInvWeight * n.CoreWeight))
                    .Aggregate((a, b) => a + b);
            }
        }
        
        /// <summary>
        /// Returns the referenced point of this cluster node.
        /// </summary>
        public XYZ Point { get; private set; }
        
        /// <summary>
        /// Returns core weight of this cluster point (count of point directly reachable).
        /// </summary>
        public int CoreWeight { get; private set; }
        
        /// <summary>
        /// Returns all cluster points as <see cref="XYZ"/> instance.
        /// </summary>
        public IEnumerable<XYZ> ClusterPoints => ClusterRing.Select(n => n.Point);

        /// <summary>
        /// Returns the bounding box of all cluster points.
        /// </summary>
        public ABox ABox => ClusterRing.Select(n => n.Point).Aggregate(ABox.Empty, (abox, p) => abox.UnionWith(p));

        /// <summary>
        /// Get the overall weight as sum of core weights.
        /// </summary>
        public int ClusterWeight => ClusterRing.Sum(n => n.CoreWeight);

        /// <summary>
        /// Returns the count of aggregated cluster points.
        /// </summary>
        public int ClusterCount => ClusterRing.Count();
        
        /// <summary>
        /// True, if this cluster point is a cluster (with more than itself).
        /// </summary>
        public bool IsCluster => null != _coreNext;
    }
}