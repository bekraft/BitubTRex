using System;

namespace Bitub.Dto.Scene
{
    /// <summary>
    /// A simplified topological triangle representation based on indices.
    /// </summary>
    public struct Tridex
    {
        public uint A, B, C;

        public bool IsValid { get => A != B && B != C; }

        /// <summary>
        /// Shifting the indices by some positive value.
        /// </summary>
        /// <param name="shift">Unsigned shift</param>
        /// <returns>A new shifted Tridex</returns>
        public Tridex Shift(uint shift)
        {
            return new Tridex { A = A + shift, B = B + shift, C = C + shift };
        }
    }
}
