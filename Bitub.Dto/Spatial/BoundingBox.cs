namespace Bitub.Dto.Spatial
{
    public partial class BoundingBox
    {
        public BoundingBox UnionWith(BoundingBox other)
        {
            if (OBoxOrABoxCase == BoundingBox.OBoxOrABoxOneofCase.None)
                return other;
            if (other.OBoxOrABoxCase == BoundingBox.OBoxOrABoxOneofCase.None)
                return this;

            var aAbox = (OBoxOrABoxCase == BoundingBox.OBoxOrABoxOneofCase.ABox) ? ABox : OBox.ToABox();
            var bAbox = (other.OBoxOrABoxCase == BoundingBox.OBoxOrABoxOneofCase.ABox) ? other.ABox : other.OBox.ToABox();
            return new BoundingBox { ABox = aAbox.UnionWith(bAbox) };
        }
   
    }
}