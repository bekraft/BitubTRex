using System;

namespace Bitub.Ifc.TRex
{
    public enum FeatureStageStrategy
    {
        FirstOf, LastOf, AllOf
    }

    public struct FeatureStageRange
    {
        public readonly static FeatureStageRange GlobalRange = new FeatureStageRange(0, int.MaxValue);
        
        public readonly int lower;
        public readonly int upper;

        public FeatureStageRange(int lo, int up)
        {
            lower = lo;
            upper = up;
        }

        public bool IsSingleton { get => lower == upper; }

        public bool IsEmpty { get => lower > upper; }

        public bool IsInRange(int stage) => lower <= stage && stage <= upper;

        public static FeatureStageRange NewOpenRange(int start)
        {
            return new FeatureStageRange(start, int.MaxValue);
        }

        public static FeatureStageRange NewRootBased(int end)
        {
            return new FeatureStageRange(0, end);
        }
    }
}
