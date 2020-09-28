using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitub.Dto.Scene
{
    /// <summary>
    /// A topological triangle representation based on indices.
    /// </summary>
    public struct Tridex
    {
        public uint A, B, C;

        public bool IsValid => A != B && B != C;

        /// <summary>
        /// Shifting the indices by some positive value.
        /// </summary>
        /// <param name="shift">Unsigned shift</param>
        /// <returns></returns>
        public Tridex Shift(uint shift)
        {
            return new Tridex { A = A + shift, B = B + shift, C = C + shift };
        }
    }

    public static class FacetExtensions
    {
        public static Tridex ToTridex(this Facet t)
        {
            if (!t.IsTriangle())
                throw new NotSupportedException($"Supporting only triangle represenations");

            return new Tridex { A = t.A, B = t.B, C = t.C };
        }

        /// <summary>
        /// A new tridex with reordered vertices keeping the global orientation.
        /// </summary>
        /// <param name="t">The facet</param>
        /// <param name="pivot">The pivot index (A index)</param>
        /// <returns>A reordered topological triangle</returns>
        public static Tridex ToTridex(this Facet t, uint? pivot)
        {
            if (!pivot.HasValue)
                return t.ToTridex();

            if (!t.IsTriangle())
                throw new NotSupportedException($"Supporting only triangle represenations");

            // By default assume A is the connector
            uint v1 = t.B;
            uint v2 = t.C;

            if (pivot == t.B)
            {   // Connected at B
                v1 = t.C;
                v2 = t.A;
            }
            else if (pivot == t.C)
            {   // Connected at C
                v1 = t.A;
                v2 = t.B;
            }
            else if (pivot != t.A)
                throw new ArgumentException($"Given pivot index {pivot} isn't held by given facet {t}");

            return new Tridex { A = pivot.Value, B = v1, C = v2 };
        }

    }
}
