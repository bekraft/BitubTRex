using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Bitub.Dto
{
    public static class CommonExtensions
    {
        /// <summary>
        /// Converts a <c>GlobalUniqueId</c> into a serializable string representation in base64,
        /// </summary>
        /// <param name="id">The ID</param>
        /// <returns>A base64 representation</returns>
        public static string ToBase64String(this GlobalUniqueId id)
        {
            switch (id.NumericalOrStringCase)
            {
                case GlobalUniqueId.NumericalOrStringOneofCase.Base64:
                    return id.Base64;
                case GlobalUniqueId.NumericalOrStringOneofCase.Guid:
                    return id.Guid.Raw.ToBase64();
                default:
                    throw new NotImplementedException($"Missing implementation for '{id.NumericalOrStringCase}'");
            }
        }

        public static bool IsEqualTo(this Guid id, System.Guid guid)
        {
            return Enumerable.SequenceEqual(id.Raw.ToArray(), guid.ToByteArray());
        }

        public static bool IsEqualTo(this System.Guid guid, Guid id)
        {
            return Enumerable.SequenceEqual(id.Raw.ToArray(), guid.ToByteArray());
        }

        public static GlobalUniqueId ToGlobalUniqueId(this System.Guid guid)
        {
            return new GlobalUniqueId { Guid = new Guid { Raw = guid.ToByteArray().ToByteString() } };
        }

        public static Qualifier ToQualifier(this System.Guid guid)
        {
            return new GlobalUniqueId{ Guid = new Guid { Raw = guid.ToByteArray().ToByteString() }}.ToQualifier();
        }

        public static Qualifier ToQualifier(this GlobalUniqueId guid)
        {
            return new Qualifier
            {
                Anonymous = guid
            };
        }

        public static Qualifier ToQualifier(this Name name)
        {
            return new Qualifier
            {
                Named = name
            };
        }

        public static Name ToName(this string frag, params string[] parentFrags)
        {
            var name = new Name();
            foreach (var parent in parentFrags)
                if (null != parent)
                    name.Frags.Add(parent);

            name.Frags.Add(frag);
            return name;
        }

        public static Qualifier ToQualifier(this string frag, params string[] parentFrags)
        {
            return new Qualifier
            {
                Named = frag.ToName(parentFrags)
            };
        }

        public static Name ToName(this IEnumerable<string> frags)
        {
            var name = new Name();
            foreach (var s in frags)
                if (null != s)
                    name.Frags.Add(s);

            return name;
        }

        public static string ToLabel(this Name name, 
            string separator = ".", int fromStart = 0, int fromEnd = 0)
        {
            return string.Join(separator, name.Frags
                        .Skip(fromStart)
                        .Take(Math.Max(0, name.Frags.Count - fromEnd - fromStart)));
        }

        public static Name ToName(this XName t)
        {
            var named = new Name();
            named.Frags.Add(t.NamespaceName);
            named.Frags.Add(t.LocalName);
            return named;
        }

        public static Name ToName(this Type t)
        {
            var named = new Name();
            named.Frags.Add(t.Name);
            return named;
        }

        public static Name ToName(this string[] frags)
        {
            var named = new Name();
            named.Frags.AddRange(frags);
            return named;
        }

        public static Qualifier ToQualifier(this XName t)
        {
            return new Qualifier
            {
                Named = t.ToName()
            };
        }

        public static Qualifier ToQualifier(this Type t)
        {
            return new Qualifier
            {
                Named = t.ToName()
            };
        }

        public static Qualifier ToQualifier(this string[] frags)
        {
            return new Qualifier
            {
                Named = frags.ToName()
            };
        }

        public static Classifier ToClassifier(this Qualifier name)
        {
            var c = new Classifier();
            c.Path.Add(name);
            return c;
        }

        public static Classifier Enqueue(this Classifier classifier, Qualifier tail)
        {
            classifier?.Path.Add(tail);
            return classifier;
        }

        public static Classifier ToClassifier(this IEnumerable<Qualifier> qualifiers)
        {
            var c = new Classifier();
            c.Path.AddRange(qualifiers);
            return c;
        }
    }
}
