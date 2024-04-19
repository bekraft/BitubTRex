using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitub.Dto.Math
{
    public enum Valency
    {
        /// <summary>
        /// Coplanar configuration.
        /// </summary>
        COPLANAR,
        /// <summary>
        /// Negative configuration.
        /// </summary>
        NEGATIVE,
        /// <summary>
        /// Positive configuration.
        /// </summary>
        POSITIVE
    }

    public interface ISimplexPrecisionPredicateBuilder
    {
        /// <summary>
        /// Build a new precision predicate. A threshold of 0.0 enables auto configuration by system thresholds.
        /// </summary>
        /// <param name="threshold">Threshold of minimal distinguishable floating point value.</param>
        /// <returns>An instance</returns>
        ISimplexPrecisionPredicate Build();
    }

    public interface ISimplexPrecisionPredicate
    {
        Valency Orient3(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d);
        Valency Orient2(ref Vec2 a, ref Vec2 b, ref Vec2 c);
        Valency InSphere(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d, ref Vec3 e);
        Valency InCircle(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d);

        double NormativePrecision { get; }
    }
}
