using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Bitub.Dto
{
    public static class CommonExtensions
    {
        public static readonly Regex guidRegExpression = new Regex(@"^[{(]?[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static RefId ToRefId(this int index)
        {
            return new RefId { Nid = index };
        }

        public static RefId ToRefId(this Qualifier qualifier)
        {
            return new RefId { Sid = qualifier };
        }

        #region GlobalUniqueId context

        /// <summary>
        /// Converts a <c>GlobalUniqueId</c> into a serializable string representation in base64,
        /// </summary>
        /// <param name="id">The ID</param>
        /// <returns>A base64 representation</returns>
        public static string ToBase64String(this GlobalUniqueId id)
        {
            switch (id.GuidOrStringCase)
            {
                case GlobalUniqueId.GuidOrStringOneofCase.Base64:
                    return id.Base64;
                case GlobalUniqueId.GuidOrStringOneofCase.Guid:
                    return id.Guid.Raw.ToBase64();
                case GlobalUniqueId.GuidOrStringOneofCase.None:
                    return null;
                default:
                    throw new NotImplementedException($"Missing implementation for '{id.GuidOrStringCase}'");
            }
        }

        public static GlobalUniqueId ToGlobalUniqueId(this string guid)
        {
            if (guid.IsGuidStringRepresentation())
                return System.Guid.Parse(guid).ToGlobalUniqueId();
            else
                return new GlobalUniqueId { Base64 = guid };
        }

        public static Qualifier ToQualifier(this GlobalUniqueId guid)
        {
            return new Qualifier
            {
                Anonymous = guid
            };
        }

        public static Qualifier ToQualifier(this byte[] byteArray)
        {
            return new Qualifier 
            {
                Anonymous = new GlobalUniqueId { Base64 = Convert.ToBase64String(byteArray) }
            };
        }

        #endregion

        #region System.Guid context

        public static bool IsEqualTo(this Guid id, System.Guid guid)
        {
            return Enumerable.SequenceEqual(id.Raw.ToArray(), guid.ToByteArray());
        }

        public static bool IsEqualTo(this System.Guid guid, Guid id)
        {
            return Enumerable.SequenceEqual(id.Raw.ToArray(), guid.ToByteArray());
        }

        public static Guid ToDtoGuid(this System.Guid guid)
        {
            return new Guid { Raw = guid.ToByteArray().ToByteString() };
        }

        public static System.Guid ToGuid(this Guid dtoGuid)
        {
            return new System.Guid(dtoGuid.Raw.ToByteArray());
        }

        public static GlobalUniqueId ToGlobalUniqueId(this System.Guid guid)
        {
            return new GlobalUniqueId { Guid = guid.ToDtoGuid() };
        }

        public static Qualifier ToQualifier(this System.Guid guid)
        {
            return new GlobalUniqueId{ Guid = new Guid { Raw = guid.ToByteArray().ToByteString() }}.ToQualifier();
        }

        public static bool IsGuidStringRepresentation(this string guid)
        {
            var match = guidRegExpression.Match(guid);
            return null != match && match.Length == guid.Length;
        }

        #endregion

        #region Name and string context

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

        public static string ToLabel(this Name name, string separator = ".", int fromStart = 0, int fromEnd = 0)
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

        public static Name ToName(this System.Type t)
        {
            return ToName(t, null);
        }

        public static Name ToName(this System.Type t, Regex replacePattern, string replaceBy = "")
        {
            var named = new Name();
            var qualifiedName = null != replacePattern ? replacePattern.Replace(t.FullName, replaceBy) : t.FullName;
            qualifiedName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ForEach(f => named.Frags.Add(f));
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

        public static Qualifier ToQualifier(this System.Type t)
        {
            return ToQualifier(t, null);
        }

        public static Qualifier ToQualifier(this System.Type t, Regex replacePattern, string replaceBy = "")
        {
            return new Qualifier
            {
                Named = t.ToName(replacePattern, replaceBy)
            };
        }

        public static Qualifier ToQualifier(this string[] frags)
        {
            return new Qualifier
            {
                Named = frags.ToName()
            };
        }

        #endregion

        #region Qualifier and Classifier context

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

        #endregion

        public static Logical ToLogical(this bool? logicalFlag)
        {
            if (logicalFlag.HasValue)
                return new Logical { Known = logicalFlag.Value };
            else
                return new Logical { };
        }

        public static bool? ToBoolean(this Logical logical)
        {
            switch (logical.FlagCase)
            {
                case Logical.FlagOneofCase.Known:
                    return logical.Known;
                default:
                    return null;
            }
        }
    }
}
