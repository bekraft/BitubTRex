using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitub.Dto;
using Bitub.Dto.Concept;

namespace Bitub.Ifc.TRex
{
    public class FeatureStageCache
    {
        #region Internals
        private Dictionary<Qualifier, SortedList<int, FeatureConcept>> cache;
        private int stage;
        #endregion

        public FeatureStageCache(QualifierCaseEqualityComparer comparer) 
        {
            cache = new Dictionary<Qualifier, SortedList<int, FeatureConcept>>(comparer);
        }

        public FeatureStageCache() : this(new QualifierCaseEqualityComparer(StringComparison.OrdinalIgnoreCase))
        { }

        public int Stage 
        {
            get => stage;
            set => stage = value;
        }

        public IEnumerable<FeatureConcept> GetAllByDepth(Qualifier qualifier, FeatureStageStrategy strategy, FeatureStageRange stageRange)
        {
            var features = cache[qualifier]?.Where(f => f.Key <= stage);
            if (null == features)
                yield break;

            switch (strategy)
            {
                case FeatureStageStrategy.FirstOf:
                    var first = features.FirstOrDefault(f => f.Key >= stageRange.lower);
                    if (null != first.Value)
                        yield return first.Value;
                    break;
                case FeatureStageStrategy.LastOf:
                    var last = features.LastOrDefault(f => f.Key <= stageRange.upper);
                    if (null != last.Value)
                        yield return last.Value;
                    break;
                case FeatureStageStrategy.AllOf:
                    foreach (var f in features.Where(f => stageRange.IsInRange(f.Key)))
                        yield return f.Value;
                    break;
                default:
                    throw new NotImplementedException($"Missing '{strategy}'");
            }
        }

        public FeatureStage AddFeatureStage(FeatureStage featureStage)
        {
            return AddFeatureStage(featureStage.stage, featureStage.feature);
        }

        public FeatureStage AddFeatureStage(int stage, FeatureConcept feature)
        {
            SortedList<int, FeatureConcept> features;
            if (!cache.TryGetValue(feature.Canonical, out features))
                cache.Add(feature.Canonical, features = new SortedList<int, FeatureConcept>());

            var formerFeature = features[stage];
            features[stage] = feature;
            return null != formerFeature ? new FeatureStage(stage, formerFeature) : null;
        }

        public void DropAllAboveStage(int stage)
        {
            foreach (var fKeyValue in cache)
            {
                foreach (var stageKey in fKeyValue.Value.Keys.Where(k => k > stage).ToArray())
                    fKeyValue.Value.Remove(stageKey);
            }    
        }
    }
}
