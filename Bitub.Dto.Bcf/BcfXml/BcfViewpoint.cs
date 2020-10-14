using System.Xml.Serialization;

namespace Bitub.Dto.BcfXml
{
    public class BcfViewpoint
    {
        [XmlAttribute(AttributeName = "Guid")]
        public System.Guid ID { get; set; } = System.Guid.NewGuid();

        [XmlElement(ElementName = "Viewpoint")]
        public string Reference { get; set; }
        [XmlElement(ElementName = "Snapshot")]
        public string Snapshot { get; set; }

        [XmlElement(ElementName = "Index", IsNullable = true)]
        public int? Index { get; set; } 
    }
}
