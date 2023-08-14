namespace Bitub.Dto.Scene
{
    public struct Simplex1
    {
        public readonly uint O, T;
        
        public Simplex1(uint o, uint t)
        {
            O = o;
            T = t;
        }

        public Simplex1 Twin => new Simplex1(T, O);

        public override bool Equals(object obj)
        {
            return obj is Simplex1 bidex &&
                   O == bidex.O &&
                   T == bidex.T;
        }

        public override int GetHashCode()
        {
            var hashCode = 866298673;
            hashCode = hashCode * -1521134295 + (int)O;
            hashCode = hashCode * -1521134295 + (int)T;
            return hashCode;
        }
    }
}
