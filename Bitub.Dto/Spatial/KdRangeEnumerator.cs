using System.Collections;
using System.Collections.Generic;

namespace Bitub.Dto.Spatial
{
    public class KdRangeEnumerator : IEnumerator<KdNode>
    {
        private Queue<KdNode> _queue;
        private KdNode _root;

        public KdRangeEnumerator(KdNode root)
        {
            _root = root;
            Reset();
        }

        public bool MoveNext()
        {
            if (_queue.Count > 0)
            {
                var node = _queue.Dequeue();
                if (null != node.Left)
                    _queue.Enqueue(node.Left);
                if (null != node.Right)
                    _queue.Enqueue(node.Right);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            _queue = new Queue<KdNode>();
            if (null != _root)
                _queue.Enqueue(_root);
        }

        public KdNode Current => _queue.Peek();

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _queue = null;
            _root = null;
        }
    }
}