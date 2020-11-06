using System.Linq;

namespace Bitub.Dto.Classify
{
    /// <summary>
    /// Classifying filter rule given both, inclusion and exclusion filter in combination.
    /// </summary>
    public class ClassifyingFilterRule
    {
        #region Internals
        protected ClassifyingFilter includeFilter;
        protected ClassifyingFilter excludeFilter;
        protected bool acceptEquivIncludeExclude;
        #endregion

        /// <summary>
        /// The inclusion filter. If given, include all matches in result.
        /// </summary>
        public ClassifyingFilter Include 
        {
            get => includeFilter;
            protected set => includeFilter = value; 
        }

        /// <summary>
        /// The exclusion filter. If given, exclude all matches from result.
        /// </summary>
        public ClassifyingFilter Exclude 
        {
            get => excludeFilter;
            protected set => excludeFilter = value; 
        }

        public bool IsAcceptingEquivIncludeExclude
        {
            get => acceptEquivIncludeExclude;
            protected set => acceptEquivIncludeExclude = value;
        }

        /// <summary>
        /// Tests a given classifier specimen if it will accepted by the filter
        /// </summary>
        /// <param name="specimen">The classifier</param>
        /// <returns>True or false</returns>
        public bool IsAcceptedBy(Classifier specimen)
        {
            Classifier[] includes = null;
            bool? isIncludeMatch = Include?.IsPassedBy(specimen, out includes);
            Classifier[] excludes = null;
            bool? isExcludeMatch = Exclude?.IsPassedBy(specimen, out excludes);

            if ((isIncludeMatch ?? false) && (isExcludeMatch ?? false))
            {
                // Test grades if both have matches
                var includesSpecialised = excludes?.SelectMany(e => includes?.Where(i => e.IsSuperClassifierOf(i, !acceptEquivIncludeExclude)));
                return includesSpecialised.Any();
            }
            // Use include first and if not defined, exclude as second
            return (isIncludeMatch ?? true) && !(isExcludeMatch ?? false);
        }

        /// <summary>
        /// Tests a given classifier specimen if it will accepted by the filter
        /// </summary>
        /// <param name="specimen">The classifier</param>
        /// <returns>True or false</returns>
        public bool IsAcceptedBy(Qualifier singleSpecimen)
        {
            Classifier[] includes = null;
            bool? isIncludeMatch = Include?.IsPassedBy(singleSpecimen, out includes);
            Classifier[] excludes = null;
            bool? isExcludeMatch = Exclude?.IsPassedBy(singleSpecimen, out excludes);

            if ((isIncludeMatch ?? false) && (isExcludeMatch ?? false))
            {
                // Test grades if both have matches
                var includesSpecialised = excludes?.SelectMany(e => includes?.Where(i => e.IsSuperClassifierOf(i, !acceptEquivIncludeExclude)));
                return includesSpecialised.Any();
            }
            // Use include first and if not defined, exclude as second
            return (isIncludeMatch ?? true) && !(isExcludeMatch ?? false);
        }
    }
}
