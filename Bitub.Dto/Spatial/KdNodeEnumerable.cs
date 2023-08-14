using System;
using System.Collections;
using System.Collections.Generic;

namespace Bitub.Dto.Spatial
{
    public class KdNodeEnumerable : IEnumerable<KdNode>
    {
        private readonly Func<IEnumerator<KdNode>> _nodeSupplier;

        public KdNodeEnumerable(KdNode node)
        {
            _nodeSupplier = () => new KdRangeEnumerator(node);
        }
        
        public KdNodeEnumerable(KdNode node, ABox aBox)
        {
            _nodeSupplier = () => new KdRangeABoxEnumerator(node, aBox);
        }

        public IEnumerator<KdNode> GetEnumerator() => _nodeSupplier();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}