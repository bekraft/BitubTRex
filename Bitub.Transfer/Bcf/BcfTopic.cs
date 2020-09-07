using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Bitub.Transfer.Bcf
{
    public class BcfTopic
    {
        [XmlAttribute(AttributeName = "Guid")]
        public System.Guid ID { get; set; } = System.Guid.NewGuid();
        [XmlElement(ElementName = "ReferenceLink")]
        public string ReferenceLink { get; set; }
        [XmlElement(ElementName = "Title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "Priority")]
        public string Priority { get; set; }

        [XmlElement(ElementName = "Index", IsNullable = true)]
        public int? Index { get; set; }
        [XmlElement(ElementName = "Labels")]
        public string[] Labels { get; set; } = new string[0];
        [XmlElement(ElementName = "CreationDate")]
        public string CreationDate { get; set; }
        [XmlElement(ElementName = "CreationAuthor")]
        public string CreationAuthor { get; set; }
        [XmlElement(ElementName = "DueDate")]
        public DateTime DueDate { get; set; }
        [XmlElement(ElementName = "AssignedTo")]
        public string AssignedTo { get; set; }
        [XmlElement(ElementName = "Stage")]
        public string Stage { get; set; }
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "BimSnippet")]
        public BimSnippet BimSnippet { get; set; }
        [XmlElement(ElementName = "DocumentReference")]
        public DocumentReference[] DocumentReferences { get; set; }

        [XmlElement(ElementName = "RelatedTopic")]
        public BcfTopic[] RelatedTopic { get; set; } = new BcfTopic[0];
    }

    public class DocumentReference
    {
        [XmlAttribute(AttributeName = "Guid")]        
        public System.Guid ID { get; set; } = System.Guid.NewGuid();

        [XmlAttribute(AttributeName = "isExternal")]
        public bool IsExternal { get; set; }

        [XmlElement(ElementName = "ReferencedDocument")]
        public string ReferencedDocument { get; set; }
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
    }

    public class BimSnippet
    {

    }
}
