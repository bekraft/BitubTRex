using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitub.Transfer
{
    public static class ClassifierExtensions
    {
        /// <summary>
        /// Counterpart of <see cref="IsNothing(Classifier)"/>
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <returns>True if the classifier holds any path of qualifiers</returns>
        public static bool IsSomething(this Classifier classifier)
        {
            return !classifier.IsNothing();
        }

        /// <summary>
        /// True, if the classifier is empty.
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <returns>True if empty</returns>
        public static bool IsNothing(this Classifier classifier)
        {
            return classifier.Path.Count == 0;
        }

        /// <summary>
        /// If a classifier contains only unique qualifiers.
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <returns>True, if each path qualifier is unique (no cycles)</returns>
        public static bool IsValid(this Classifier classifier)
        {
            var set = new HashSet<Qualifier>(classifier.Path);
            return set.Count == classifier.Path.Count;
        }

        /// <summary>
        /// True, if the given qualifier is held by this classifier.
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <param name="qualifier">The qualifier</param>
        /// <returns>True, if the qualifier is held by this classifier</returns>
        public static bool IsMatching(this Classifier classifier, Qualifier qualifier)
        {
            return classifier.Path.Any(q => q.Equals(qualifier));
        }

        /// <summary>
        /// True, if the given qualifier is a super qualifier of at least on path fragment.
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <param name="qualifier">The qualifier</param>
        /// <returns>True, if the given qualifier is (paritally) a super qualifier</returns>
        public static bool IsSuperMatching(this Classifier classifier, Qualifier qualifier, bool caseInvariant = false)
        {
            return classifier.Path.Any(q => qualifier.IsSuperQualifierOf(q, caseInvariant));
        }

        /// <summary>
        /// Fetches the superset classifier if there's any.
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <param name="qualifier">The qualifier</param>
        /// <returns>A new super classifier trace or a classifier with <see cref="IsNothing(Classifier)"/> returning true.</returns>
        public static Classifier SuperClassifierOf(this Classifier classifier, Qualifier qualifier)
        {
            var superClassifier = new Classifier();
            for (int i=0; i < classifier.Path.IndexOf(qualifier); i++)
            {
                superClassifier.Path.Add(classifier.Path[i]);
            }
            return superClassifier;
        }

        /// <summary>
        /// Fetches the subset classifier if there's any.
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <param name="qualifier">The qualifier</param>
        /// <returns>A new subset classifier trace or a classifier with <see cref="IsNothing(Classifier)"/> returning true.</returns>
        public static Classifier SubClassifierOf(this Classifier classifier, Qualifier qualifier)
        {
            var subClassifier = new Classifier();
            for (int i = classifier.Path.IndexOf(qualifier); i > 0 && i < classifier.Path.Count; i++)
            {
                subClassifier.Path.Add(classifier.Path[i]);
            }
            return subClassifier;
        }
    }
}
