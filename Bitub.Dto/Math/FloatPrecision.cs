using System;

using System.Runtime.InteropServices;

namespace Bitub.Dto.Math
{
    internal static class FloatPrecisionPrefs
    {
        internal const string BITUB_PRECISION_LIB = "bitub.precision.dll";
    }

    internal class FloatPrecision
    {
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
        internal static extern float incircle(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d);
        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float incirclefast(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d);
        [DllImport(FloatPrecisionPrefs.BITUB_PRECISION_LIB, CallingConvention = CallingConvention.Cdecl)]
        internal static extern float incircleexact(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d);
    }

    public sealed class FloatPrecisionPredicate : ISimplexPrecisionPredicate
    {
        public double NormativePrecision { get; private set; }

        public Valency InCircle(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d)
        {
            throw new NotImplementedException();
        }

        public Valency InSphere(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d, ref Vec3 e)
        {
            throw new NotImplementedException();
        }

        public Valency Orient2(ref Vec2 a, ref Vec2 b, ref Vec2 c)
        {
            throw new NotImplementedException();
        }

        public Valency Orient3(ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 d)
        {
            throw new NotImplementedException();
        }

        public sealed class Builder : ISimplexPrecisionPredicateBuilder
        {
            public Builder() { }

            public static FloatPrecisionPredicate Build()
            {
                return new FloatPrecisionPredicate { NormativePrecision = FloatPrecision.exactinit() };
            }

            ISimplexPrecisionPredicate ISimplexPrecisionPredicateBuilder.Build()
            {
                return Build();
            }
        }


    }
}
