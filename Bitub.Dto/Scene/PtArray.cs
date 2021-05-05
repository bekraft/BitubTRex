using System;
using System.Collections.Generic;
using System.Linq;

using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene
{
    public partial class PtArray
    {
        /// <summary>
        /// Returns a sequence of <see cref="XYZ"/> points.
        /// </summary>
        /// <returns>Points</returns>
        public IEnumerable<XYZ> ToXYZ()
        {
            for (int k = 0; k < Xyz.Count; k += 3)
            {
                yield return new XYZ
                {
                    X = Xyz[k],
                    Y = Xyz[k + 1],
                    Z = Xyz[k + 2]
                };
            }
        }

        /// <summary>
        /// Picks a point by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public XYZ ToXYZ(int index)
        {
            return new XYZ
            {
                X = Xyz[index * 3],
                Y = Xyz[index * 3 + 1],
                Z = Xyz[index * 3 + 2]
            };
        }

    }
}
