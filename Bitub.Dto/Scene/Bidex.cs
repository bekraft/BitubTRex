namespace Bitub.Dto.Scene
{
    public struct Bidex
    {
        public readonly uint O, T;
        
        public Bidex(uint o, uint t)
        {
            O = o;
            T = t;
        }

        public Bidex Twin => new Bidex(T, O);

        public override bool Equals(object obj)
        {
            return obj is Bidex bidex &&
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
