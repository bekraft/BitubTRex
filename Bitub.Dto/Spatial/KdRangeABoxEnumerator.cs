using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bitub.Dto.Spatial
{
    public class KdRangeABoxEnumerator : IEnumerator<KdNode>
    {
        private readonly ABox _aBox;
        private KdNode _root;
        private KdNode _current;
        private LinkedList<KdNode> _fifo;

        public KdRangeABoxEnumerator(KdNode node, ABox aBox)
        {
            _aBox = aBox;
            _root = node;
            Reset();
        }

        public ABox ABox => _aBox;

        public bool MoveNext()
        {
            KdNode hit = null;
            // Check queue
            while (null == hit && _fifo.Count > 0)
            {
                var next = _fifo.First();
                _fifo.RemoveFirst();

                // Calculate the distances to the AABB borders
                var dim = next.Dim;
                var leftDist = next.Point.GetCoordinate(dim) - _aBox.Min.GetCoordinate(dim);
                var rightDist = _aBox.Max.GetCoordinate(dim) - next.Point.GetCoordinate(dim);

                var rightNode = next.Right;
                // Add right node to query list, if right minimum inside bounding box
                if (null != rightNode && next.RMin < rightDist) 
                {
                    _fifo.AddFirst(rightNode);
                }

                var leftNode = next.Left;
                // Add left node to query list, if left minimum inside bounding box
                if (null != leftNode && next.LMin < leftDist) 
                {
                    _fifo.AddFirst(leftNode);
                }

                // Otherwise, assuming parent was inside -> test only when new dimension is inside too
                if ((0 < leftDist) && (0 < rightDist)) 
                {
                    if (_aBox.Covers(next.Point)) 
                    {
                        hit = next;
                    }
                }
            }

            _current = hit;
            return hit != null;
        }

        public void Reset()
        {
            _current = null;
            _fifo = new LinkedList<KdNode>();
            if (null != _root)
                _fifo.AddFirst(_root);
        }

        public KdNode Current => _current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _fifo.Clear();
            _root = null;
        }
    }
}