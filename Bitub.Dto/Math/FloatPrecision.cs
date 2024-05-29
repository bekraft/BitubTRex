using System;

using System.Runtime.InteropServices;

namespace Bitub.Dto.Math
{
    internal static class FloatPrecisionPrefs
    {
#if Is_WINDOWS
        internal const string BITUB_PRECISION_LIB = "bitub.precision.dll";
#endif
    }

    internal sealed class FloatPrecision
    {
        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int exactsign(float value);
        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float exactinit();

        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float orient3d(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d);
        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float orient3dfast(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d);
        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float orient3dexact(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d);

        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float orient2d(ref Vec2 a, ref Vec2 b, ref Vec2 c);
        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float orient2dfast(ref Vec2 a, ref Vec2 b, ref Vec2 c);
        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float orient2dexact(ref Vec2 a, ref Vec2 b, ref Vec2 c);

        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float insphere(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d, ref Vec3 e);
        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float inspherefast(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d, ref Vec3 e);
        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float insphereexact(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d, ref Vec3 e);

        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float incircle(ref Vec2 a, ref Vec2 b, ref Vec2 c, ref Vec2 d);
        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float incirclefast(ref Vec2 a, ref Vec2 b, ref Vec2 c, ref Vec2 d);
        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float incircleexact(ref Vec2 a, ref Vec2 b, ref Vec2 c, ref Vec2 d);
    }

    /// <summary>
    /// Exact precision predicates.
    /// </summary>
    public sealed class FloatExactPrecisionPredicate : ISimplexPrecisionPredicate
    {
        public double NormativePrecision { get; private set; }

        private Valency ConvertToValency(float value)
        {
            return (Valency)FloatPrecision.exactsign(value);
        }

        public Valency InCircle(ref Vec2 a, ref Vec2 b, ref Vec2 c, ref Vec2 d)
        {
            return ConvertToValency(FloatPrecision.incircleexact(ref a, ref b, ref c, ref d));
        }

        public Valency InSphere(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d, ref Vec3 e)
        {
            return ConvertToValency(FloatPrecision.insphereexact(ref a, ref b, ref c, ref d, ref e));
        }

        public Valency Orient2(ref Vec2 a, ref Vec2 b, ref Vec2 c)
        {
            return ConvertToValency(FloatPrecision.orient2dexact(ref a, ref b, ref c));
        }

        public Valency Orient3(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d)
        {
            return ConvertToValency(FloatPrecision.orient3dexact(ref a, ref b, ref c, ref d));
        }

        public sealed class Builder : ISimplexPrecisionPredicateBuilder
        {
            public Builder() { }

            public static FloatExactPrecisionPredicate Build()
            {
                return new FloatExactPrecisionPredicate { NormativePrecision = FloatPrecision.exactinit() };
            }

            ISimplexPrecisionPredicate ISimplexPrecisionPredicateBuilder.Build()
            {
                return Build();
            }
        }


    }
}
