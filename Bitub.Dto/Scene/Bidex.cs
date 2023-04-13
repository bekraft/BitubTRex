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
            return obj is Bidex duodex &&
                   O == duodex.O &&
                   T == duodex.T;
        }

        public override int GetHashCode()
        {
            int hashCode = 866298673;
            hashCode = hashCode * -1521134295 + O.GetHashCode();
            hashCode = hashCode * -1521134295 + T.GetHashCode();
            return hashCode;
        }
    }
}
