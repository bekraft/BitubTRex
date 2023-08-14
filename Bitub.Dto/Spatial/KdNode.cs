using System;

namespace Bitub.Dto.Spatial
{
    public class KdNode : DbScanClusterNode<KdNode>
    {
        protected internal KdNode(XYZ xyz) : base(xyz)
        {
        }

        public KdNode Left { get; private set; } = null;
        public KdNode Right { get; private set; } = null;

        public float LMin { get; private set; } = float.MaxValue;
        public float RMin { get; private set; } = float.MaxValue;
        public int Dim { get; private set; } = 0;

        /// <summary>
        /// Will propagate a point along kd-tree substructure in order to find a position.
        /// </summary>
        /// <param name="xyz">The point</param>
        /// <param name="epsSame">The threshold where any two points considered to be same</param>
        /// <returns>Either returns an existing node or a new node.</returns>
        public KdNode Propagate(XYZ xyz, float epsSame)
        {
            KdNode node = null;
            var parent = this;
            while (node == null)
            {
                var leftPositive = parent.Point.GetCoordinate(parent.Dim) - xyz.GetCoordinate(parent.Dim);
                KdNode newParent = null;

                if (Math.Abs(leftPositive) < epsSame)
                {
                    if (parent.Point.Distance(xyz) < epsSame)
                    {
                        node = parent;
                    }
                }
                if (0 <= Math.Sign(leftPositive))
                {
                    // If lefthand but outside epsSame
                    parent.LMin = Math.Min(parent.LMin, leftPositive);
                    if (null == (newParent = parent.Left))
                    {
                        node = CreateChildDimNode(xyz);
                        parent.Left = node;
                    }
                } 
                else
                {
                    // If righthand but outside of epsSame
                    parent.RMin = Math.Min(parent.RMin, -leftPositive);
                    if (null == (newParent = parent.Right))
                    {
                        node = CreateChildDimNode(xyz);
                        parent.Right = node;
                    }
                }

                parent = newParent;
            }

            return node;
        }

        private KdNode CreateChildDimNode(XYZ xyz)
        {
            return new KdNode(xyz)
            {
                Dim = (this.Dim + 1) % 3
            };
        }
    }
}