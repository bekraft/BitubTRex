using Bitub.Dto;

namespace Bitub.Ifc.TRex
{
    public sealed class FeatureMapping
    {
        public FeatureMapping()
        { }

        public Qualifier FeatureSource { get; set; }

        public Qualifier FeatureTarget { get; set; }

        public FeatureStageStrategy StageStrategy { get; set; } = FeatureStageStrategy.LastOf;

        public FeatureStageRange StageRange { get; set; } = FeatureStageRange.GlobalRange;
    }
}
