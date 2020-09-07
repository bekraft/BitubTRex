using System;
using System.Xml.Serialization;

namespace Bitub.Transfer.Bcf
{
    public class BcfComment
    {
        [XmlAttribute(AttributeName = "Guid")]
        public System.Guid ID { get; set; } = System.Guid.NewGuid();

        [XmlElement(ElementName = "Date")]
        public DateTime Date { get; set; }
        [XmlElement(ElementName = "Author")]
        public string Author { get; set; }
        [XmlElement(ElementName = "Comment")]
        public string Comment { get; set; }

        [XmlElement(ElementName = "Viewpoint")]
        public BcfViewpoint[] ViewpointReferences { get; set; }
        [XmlElement(ElementName = "ModifiedDate")]
        public DateTime ModifiedDate { get; set; }
        [XmlElement(ElementName = "ModifiedAuthor")]
        public string ModifiedAuthor { get; set; }
    }
}
