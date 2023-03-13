namespace Bitub.Dto.Spatial
{
    public partial class OBox
    {
        public ABox ToABox() => new ABox(Base, Base.Add(Ex).Add(Ey).Add(Ez));        
    }
}