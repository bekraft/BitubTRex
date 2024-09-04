using System;

namespace Bitub.Dto.Scene
{
    /// <summary>
    /// A simplified topological triangle representation based on indices.
    /// </summary>
    public struct Simplex2
    {
        public uint A, B, C;

        public bool IsValid => A != B && B != C;

        /// <summary>
        /// Shifting the indices by some positive value.
        /// </summary>
        /// <param name="shift">Unsigned shift</param>
        /// <returns>A new shifted Tridex</returns>
        public Simplex2 Shift(uint shift)
        {
            return new Simplex2 { A = A + shift, B = B + shift, C = C + shift };
        }

        public override bool Equals(object obj)
        {
            if (obj is Simplex2 tridex)
                return A == tridex.A && B == tridex.B && C == tridex.C;

            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 866298673;
            hashCode = hashCode * -1521134295 + (int)A;
            hashCode = hashCode * -1521134295 + (int)B;
            hashCode = hashCode * -1521134295 + (int)C;
            return hashCode;
        }
    }
}
