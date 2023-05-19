namespace Bitub.Dto.Spatial
{
    public partial class BoundingBox
    {
        public static BoundingBox Open => new BoundingBox { ABox = ABox.Open };

        public static BoundingBox Empty => new BoundingBox { ABox = ABox.Empty };
    
        public BoundingBox UnionWith(BoundingBox other)
        {
            if (OBoxOrABoxCase == OBoxOrABoxOneofCase.None)
                return other;
            if (other.OBoxOrABoxCase == OBoxOrABoxOneofCase.None)
                return this;

            var aAbox = (OBoxOrABoxCase == OBoxOrABoxOneofCase.ABox) ? ABox : OBox.ToABox();
            var bAbox = (other.OBoxOrABoxCase == OBoxOrABoxOneofCase.ABox) ? other.ABox : other.OBox.ToABox();
            return new BoundingBox { ABox = aAbox.UnionWith(bAbox) };
        } 
    }
}