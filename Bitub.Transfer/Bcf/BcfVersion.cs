
using System.Xml.Serialization;

namespace Bitub.Transfer.Bcf
{
    [XmlRoot("Version")]
    public class BcfVersion
    {
        [XmlAttribute(AttributeName = "VersionId")]
        public string VersionId { get; set; } = "2.1";

        [XmlElement(ElementName = "DetailedVersion")]
        public string DetailedVersion { get; set; } = "2.1";
    }
}
