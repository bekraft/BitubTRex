using System;

namespace Bitub.Dto.Scene
{
    /// <summary>
    /// A simplified topological triangle representation based on indices.
    /// </summary>
    public struct Tridex
    {
        public uint A, B, C;

        public bool IsValid => A != B && B != C;

        /// <summary>
        /// Shifting the indices by some positive value.
        /// </summary>
        /// <param name="shift">Unsigned shift</param>
        /// <returns>A new shifted Tridex</returns>
        public Tridex Shift(uint shift)
        {
            return new Tridex { A = A + shift, B = B + shift, C = C + shift };
        }

        public override bool Equals(object obj)
        {
            if (obj is Tridex tridex)
                return A == tridex.A && B == tridex.B && C == tridex.C;

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, C);
        }
    }
}
