using System;
using System.Net.Sockets;

namespace Bitub.Dto.Classify
{
    public enum FilterOrderType
    {
        /// <summary>
        /// Applies an optimistic policy: If no include filter is defined, drop only if the exclusion filter matches. Finally if an existing
        /// inclusion filter does not let the specimen pass, it will be excluded.
        /// </summary>
        IncludeBeforeExclude, 
        /// <summary>
        /// Applies an restrictive policy: If no exclude filter is defined, include only if the inclusion filter matches.
        /// </summary>
        ExcludeBeforeInclude
    }

    /// <summary>
    /// Classifying filter rule given both, inclusion and exclusion filter in combination.
    /// </summary>
    public class ClassifyingFilterRule
    {
        protected ClassifyingFilter includeFilter;
        protected ClassifyingFilter excludeFilter;

        public ClassifyingFilterRule(FilterOrderType filterOrderType)
        {
            OrderType = filterOrderType;
        }

        public FilterOrderType OrderType { get; protected set; }

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

        public bool IsAcceptedBy(Classifier specimen)
        {
            switch (OrderType)
            {
                case FilterOrderType.IncludeBeforeExclude:
                    // If given && if not to be included => deny, otherwise check if to drop by exclusion filter.
                    return (Include?.IsPassedBy(specimen) ?? true) && !(Exclude?.IsPassedBy(specimen) ?? false);
                case FilterOrderType.ExcludeBeforeInclude:
                    // If given && if to be excluded => deny, otherwise check
                    return !(Exclude?.IsPassedBy(specimen) ?? false) && (Include?.IsPassedBy(specimen) ?? true);
            }
            throw new NotImplementedException($"Missing {OrderType}");
        }

        public bool IsAcceptedBy(Qualifier singleSpecimen)
        {
            throw new NotImplementedException();
        }
    }
}
