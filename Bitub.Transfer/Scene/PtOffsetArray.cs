using Bitub.Transfer.Spatial;

namespace Bitub.Transfer.Scene
{
    public sealed class PtOffsetArray
    {
        public readonly uint Offset;
        public readonly PtArray Points;

        public PtOffsetArray(PtArray ptArray, uint offset = 0)
        {
            Offset = offset;
            Points = ptArray;
        }

        public XYZ Center
        {
            get {
                var c = new XYZ();
                for(int i=0; i<Points.Xyz.Count; i+=3)
                {
                    c.X += Points.Xyz[i];
                    c.Y += Points.Xyz[i + 1];
                    c.Z += Points.Xyz[i + 2];
                }
                c.X /= Points.Xyz.Count;
                c.Y /= Points.Xyz.Count;
                c.Z /= Points.Xyz.Count;
                return c;
            }
        }
    }
}
