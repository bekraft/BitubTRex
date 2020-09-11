using System.Xml;
using System.Xml.Serialization;

namespace Bitub.Transfer.Bcf
{
    [XmlRoot(ElementName = "ProjectExtension")]
    public class BcfProjectExtension
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns { get; set; }

        [XmlElement(ElementName = "Project")]
        public BcfProject Project { get; set; }

        [XmlElement(ElementName = "ExtensionSchema")]
        public string ExtensionSchema { get; set; }

        [XmlAnyAttribute]
        public XmlAttribute[] Unknown { get; set; }
    }

    public class BcfProject
    {
        [XmlAttribute(AttributeName = "ProjectId")]
        public System.Guid ID { get; set; }

        [XmlElement(ElementName = "Project")]
        public string Name { get; set; }
    }
}
