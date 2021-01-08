using System;
using System.Linq;

namespace Bitub.Dto
{
    public static class QualifierExtensions
    {
        public static Qualifier Append(this Qualifier qualifier, string fragment)
        {
            switch (qualifier.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Named:
                    var q = new Qualifier(qualifier);
                    q.Named.Frags.Add(fragment);
                    return q;
                case Qualifier.GuidOrNameOneofCase.None:
                    var name = new Name();
                    name.Frags.Add(fragment);
                    return new Qualifier { Named = name };
                default:
                    throw new NotSupportedException($"Cannot append '{fragment}' to qualifier of type '{qualifier.GuidOrNameCase}'");
            }
        }

        public static Qualifier Prepend(this Qualifier qualifier, string fragment)
        {
            switch (qualifier.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Named:
                    var q = new Qualifier(qualifier);
                    q.Named.Frags.Insert(0, fragment);
                    return q;
                case Qualifier.GuidOrNameOneofCase.None:
                    var name = new Name();
                    name.Frags.Add(fragment);
                    return new Qualifier { Named = name };
                default:
                    throw new NotSupportedException($"Cannot prepend '{fragment}' to qualifier of type '{qualifier.GuidOrNameCase}'");
            }
        }

        public static string GetFragment(this Name name, int index = 0)
        {
            if (index == 0)
                return name.Frags.FirstOrDefault();
            else
                return name.Frags.Skip(index).FirstOrDefault();
        }

        public static Name SetFragment(this Name name, int index, string fragment, string defaultParent = "")
        {
            for (int i=0; i<=index; i++)
            {
                if (name.Frags.Count == i)
                    name.Frags.Add(defaultParent);
            }
            name.Frags[index] = fragment;
            return name;
        }

        public static bool IsEmpty(this Qualifier qualifier)
        {
            if (null == qualifier)
                return true;

            switch(qualifier.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Anonymous:
                    return false;
                case Qualifier.GuidOrNameOneofCase.Named:
                    return qualifier.Named.Frags.Count == 0;
                case Qualifier.GuidOrNameOneofCase.None:
                    return true;
                default:
                    throw new NotSupportedException($"State of '{qualifier}' not supported.");
            }
        }

        public static string GetLastFragment(this Qualifier qualifier)
        {
            switch (qualifier.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Named:
                    return qualifier.Named.Frags.Count > 0 ? qualifier.Named.Frags[qualifier.Named.Frags.Count - 1] : null;
                default:
                    return null;
            }
        }

        public static Qualifier ToHead(this Qualifier qualifier)
        {
            switch (qualifier.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Named:
                    return qualifier.Named.Frags.Take(1).ToArray().ToQualifier();
                default:
                    return qualifier;
            }
        }

        public static Qualifier ToTail(this Qualifier qualifier)
        {
            switch (qualifier.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Named:
                    return qualifier.Named.Frags.Skip(1).ToArray().ToQualifier();
                default:
                    return qualifier;
            }
        }

        public static (Qualifier, Qualifier) ToHeadAndTail(this Qualifier qualifier)
        {
            switch (qualifier.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Named:
                    return (qualifier.Named.Frags.Take(1).ToArray().ToQualifier(), qualifier.Named.Frags.Skip(1).ToArray().ToQualifier());
                default:
                    return (qualifier, new Qualifier());
            }
        }

        public static bool IsEqualTo(this Qualifier qualifier, Qualifier other, StringComparison stringComparison)
        {
            return QualifierCaseEqualityComparer.Equals(qualifier, other, stringComparison);
        }

        public static Qualifier ToTrimmedLength(this Qualifier qualifier, int countFrags)
        {
            return ToTrimmed(qualifier, 0, countFrags);
        }

        public static Qualifier ToTrimmed(this Qualifier qualifier, int startFrag, int countFrags = int.MaxValue)
        {
            if (null == qualifier)
                return null;

            startFrag = Math.Max(0, startFrag);
            switch(qualifier.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Named:
                    if (startFrag > qualifier.Named.Frags.Count)
                        return new Qualifier();

                    var newFrags = new string[Math.Min(qualifier.Named.Frags.Count - startFrag, countFrags)];
                    Array.Copy(qualifier.Named.Frags.ToArray(), startFrag, newFrags, 0, newFrags.Length);
                    return newFrags.ToQualifier();
                default:
                    throw new NotSupportedException($"Cannot trim '{qualifier}' of type '{qualifier.GuidOrNameCase}'");
            }
        }

        /// <summary>
        /// If both qualifiers have the same type.
        /// </summary>
        /// <param name="qualifier">A qualifier</param>
        /// <param name="other">Other qualifier</param>
        /// <returns>True, if both have the same type</returns>
        public static bool IsCompliantTo(this Qualifier qualifier, Qualifier other)
        {
            if (null == qualifier || null == other)
                return false;

            return qualifier.GuidOrNameCase == other.GuidOrNameCase;
        }

        public static Qualifier Append(this Qualifier aQualifier, Qualifier bQualifier)
        {
            if (aQualifier.IsCompliantTo(bQualifier))
            {
                switch(aQualifier.GuidOrNameCase)
                {
                    case Qualifier.GuidOrNameOneofCase.Named:
                        return aQualifier.Named.Frags.Concat(bQualifier.Named.Frags).ToArray().ToQualifier();
                    default:
                        throw new NotSupportedException($"Not support for '{Qualifier.GuidOrNameOneofCase.Anonymous}'");
                }
            }
            throw new NotSupportedException("Incompliant qualifier types");
        }

        public static Qualifier ToCommonRoot(this Qualifier qualifier, Qualifier other, StringComparison comparison = StringComparison.Ordinal)
        {
            var fragmentLength = FindCommonFragmentLength(qualifier, other, comparison);
            switch(fragmentLength)
            {
                case int.MinValue:
                    return new Qualifier();
                default:
                    return qualifier.ToTrimmed(0, fragmentLength);
            }
        }

        /// <summary>
        /// Extracts a sub qualifier relative to given super qualifier given the current qualifier.
        /// </summary>
        /// <param name="qualifier">The given qualifier</param>
        /// <param name="supQualifier">The super qualifier</param>
        /// <param name="comparison">Comparision method</param>
        /// <returns>A sub qualifier relative to super qualifier</returns>
        public static Qualifier ToSubQualifierOf(this Qualifier qualifier, Qualifier supQualifier, StringComparison comparison = StringComparison.Ordinal)
        {
            var fragmentLength = FindCommonFragmentLength(supQualifier, qualifier, comparison);
            switch (fragmentLength)
            {
                case int.MaxValue:
                    return new Qualifier();
                default:
                    if (fragmentLength == supQualifier.Named.Frags.Count)
                        // Only if super qualifier is completely covered by sub qualifier
                        return qualifier.ToTrimmed(supQualifier.Named.Frags.Count);
                    else
                        return new Qualifier();
            }
        }

        private static int FindCommonFragmentLength(Qualifier q1, Qualifier q2, StringComparison comparison = StringComparison.Ordinal)
        {
            if (null == q1 || null == q2)
                return int.MinValue;
            if (!q1.IsCompliantTo(q2))
                return int.MinValue;
            if (q1.GuidOrNameCase != Qualifier.GuidOrNameOneofCase.Named)
                return int.MinValue;

            int count = Math.Min(q1.Named.Frags.Count, q2.Named.Frags.Count);
            int index;
            for (index = 0; index < count; index++)
            {
                if (0 != string.Compare(q1.Named.Frags[index], q2.Named.Frags[index], comparison))
                    return index;
            }

            return index;
        }

        public static bool IsSuperQualifierOf(this Qualifier supQualifier, Qualifier subQualifier, StringComparison comparison = StringComparison.Ordinal)
        {
            var fragmentLength = FindCommonFragmentLength(supQualifier, subQualifier, comparison);
            switch (fragmentLength)
            {
                case int.MinValue:
                    return false;
                default:
                    // Only if super qualifier is completely covered by sub qualifier
                    return fragmentLength == supQualifier.Named.Frags.Count;
            }
        }

        public static string ToLabel(this Qualifier c, string separator = ".", int fromStart = 0, int fromEnd = 0)
        {
            switch (c.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Anonymous:
                    switch(c.Anonymous.GuidOrStringCase)
                    {
                        case GlobalUniqueId.GuidOrStringOneofCase.Guid:
                            return new System.Guid(c.Anonymous.Guid.Raw.ToByteArray()).ToString();
                        case GlobalUniqueId.GuidOrStringOneofCase.Base64:
                            return c.Anonymous.ToBase64String();
                    }
                    return null;
                case Qualifier.GuidOrNameOneofCase.Named:
                    return c.Named.ToLabel(separator, fromStart, fromEnd);
                default:
                    return null;
            }
        }
    }
}
