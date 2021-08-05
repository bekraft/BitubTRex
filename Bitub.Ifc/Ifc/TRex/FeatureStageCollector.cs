using System;
using System.Linq;
using System.Collections.Generic;

using Xbim.Common;
using Xbim.Ifc4.Interfaces;

using Bitub.Dto.Concept;
using Bitub.Dto;

using Bitub.Ifc.Concept;

namespace Bitub.Ifc.TRex
{
    public class FeatureStageCollector 
    {
        #region Internals
        private CanonicalFilterRule entityFilterRule;
        private StringComparison defaultNameComparison;
        private FilterMatchingType defaultFilterMatchingType;

        private FeatureStageCache stageCache;
        #endregion

        public FeatureStageCollector(StringComparison nameComparison, FilterMatchingType defaultMatchingType = FilterMatchingType.Exists)
        {
            entityFilterRule = new CanonicalFilterRule();            
            defaultNameComparison = nameComparison;
            defaultFilterMatchingType = defaultMatchingType;
            stageCache = new FeatureStageCache(new QualifierCaseEqualityComparer(defaultNameComparison));

            PropertyFilter = new CanonicalFilter(defaultMatchingType, defaultNameComparison);
        }

        public CanonicalFilterRule EntityTypeFilterRule { get => entityFilterRule; }

        public CanonicalFilter PropertyFilter { get; set; }

        public void IncludeIfcTypes(IEnumerable<Classifier> entityClassifiers)
        {
            if (null == entityFilterRule.Include)
                entityFilterRule.Include = new CanonicalFilter(defaultFilterMatchingType, defaultNameComparison);
            entityFilterRule.Include.Filter.AddRange(entityClassifiers);
        }

        public void ExcludeIfcTypes(IEnumerable<Classifier> entityClassifiers)
        {
            if (null == entityFilterRule.Exclude)
                entityFilterRule.Exclude = new CanonicalFilter(defaultFilterMatchingType, defaultNameComparison);
            entityFilterRule.Exclude.Filter.AddRange(entityClassifiers);
        }

        public FeatureStageCollector IncludeIfcElement(params IfcAssemblyScope[] ifcAssemblies)
        {
            IncludeIfcTypes(ifcAssemblies.Select(a => a.schemaQualifier.Append("IfcElement").ToClassifier()));
            return this;
        }

        public FeatureStageCollector IncludeIfcProduct(params IfcAssemblyScope[] ifcAssemblies)
        {
            IncludeIfcTypes(ifcAssemblies.Select(a => a.schemaQualifier.Append("IfcProduct").ToClassifier()));
            return this;
        }

        public FeatureStageCollector IncludeAllIfcProductsOf<T>(params IfcAssemblyScope[] ifcAssemblies) where T : IIfcProduct
        {
            IncludeIfcTypes(ifcAssemblies.SelectMany(a => a.GetScopeOf<T>().TypeQualifiers).Select(q => q.ToClassifier()));
            return this;
        }

        public FeatureStageCollector ExcludeAllIfcProductsOf<T>(params IfcAssemblyScope[] ifcAssemblies) where T : IIfcProduct
        {
            ExcludeIfcTypes(ifcAssemblies.SelectMany(a => a.GetScopeOf<T>().TypeQualifiers).Select(q => q.ToClassifier()));
            return this;
        }

        public bool CheckConsistency(Action<CanonicalFilter, Classifier, Classifier> fixitAction = null)
        {
            throw new NotImplementedException();
        }

        public bool HandleVisit(IPersistEntity instance, int instanceAtStage)
        {
            if (instance is IIfcObject ifcObject)
            {
                var ifcQn = ifcObject.ToQualifiedName();
                if (EntityTypeFilterRule.IsAcceptedBy(ifcQn.ToClassifier()))
                {
                    var features = ifcObject.ToFeatures<IIfcSimpleProperty>(PropertyFilter);
                    features.ForEach(f => stageCache.AddFeatureStage(new FeatureStage(instanceAtStage, f)));
                    return true;
                }                
            }
            return false;
        }
    }

}
