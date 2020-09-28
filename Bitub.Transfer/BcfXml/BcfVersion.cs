
using System.Xml;
using System.Xml.Serialization;

namespace Bitub.Transfer.BcfXml
{
    [XmlRoot(ElementName = "Version")]
    public class BcfVersion
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns { get; set; }
        
        [XmlAttribute(AttributeName = "VersionId")]
        public string VersionId { get; set; } = "2.1";

        [XmlElement(ElementName = "DetailedVersion")]
        public string DetailedVersion { get; set; } = "2.1";

        [XmlAnyAttribute]
        public XmlAttribute[] Unknown { get; set; }
    }
}