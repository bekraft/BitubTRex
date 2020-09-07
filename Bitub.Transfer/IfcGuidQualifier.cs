using System;
using System.Linq;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Bitub.Transfer
{
    /// <summary>
    /// IFC Globally Unique ID as GUID qualifier representation.
    /// </summary>
    public class IfcGuidQualifier : IXmlSerializable, IEquatable<IfcGuidQualifier>
    {
        // See https://technical.buildingsmart.org/resources/ifcimplementationguidance/ifc-guid/
        public const string Alphabet =
           //          1         2         3         4         5         6   
           //0123456789012345678901234567890123456789012345678901234567890123
           "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_$";

        public static XmlSchema XmlSchema { get; } = new XmlSchema();

        /// <summary>
        /// Wrapped qualifier.
        /// </summary>
        public Qualifier Guid { get; private set; } = new Qualifier();

        public IfcGuidQualifier()
        {
        }

        public IfcGuidQualifier(Qualifier ifcGuid)
        {
            Guid = ifcGuid;
        }

        public bool Equals(IfcGuidQualifier other)
        {
            return Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        public bool IsValid { get => Guid.GuidOrNameCase == Qualifier.GuidOrNameOneofCase.Anonymous; }

        /// <summary>
        /// True, if given string is a valide Base64 encoded IFC guid.
        /// </summary>
        /// <param name="ifcBase64">A string</param>
        /// <returns>True, if valid</returns>
        public static bool IsValidBase64(string ifcBase64)
        {
            if (ifcBase64.Length != 22)
                return false;
            if (ifcBase64.Any(c => !Alphabet.Contains(c)))
                return false;

            return true;
        }

        public static IfcGuidQualifier FromBase64(string ifcBase64)
        {
            if (!IsValidBase64(ifcBase64))
                throw new ArgumentException("Invalid qualifier format.");

            return new IfcGuidQualifier(new Qualifier { Anonymous = new GlobalUniqueId { Base64 = ifcBase64 } });
        }

        public override bool Equals(object obj)
        {
            if (obj is IfcGuidQualifier ifcGuid)
                return Equals(ifcGuid);
            else
                return false;
        }

        public XmlSchema GetSchema()
        {
            return XmlSchema;
        }

        public void ReadXml(XmlReader reader)
        {
            var ifcGuid = reader.Value;
            if (!IsValidBase64(ifcGuid))
                throw new ArgumentException("Invalid qualifier format.");

            Guid.Anonymous = new GlobalUniqueId { Base64 = ifcGuid };
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(Guid.Anonymous.Base64);
        }
    }
}
