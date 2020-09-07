using System;

using System.Xml.Serialization;

namespace Bitub.Transfer.Bcf
{
    [XmlRoot("Markup")]
    public class BcfMarkup
    {
        [XmlElement(ElementName = "Header")]
        public BcfHeader Header { get; set; }

        [XmlElement(ElementName = "Title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "Priority")]
        public string Priority { get; set; }

        [XmlElement(ElementName = "Topic")]
        public BcfTopic Topic { get; set; }

        [XmlElement(ElementName = "Comment")]
        public BcfComment[] Comments { get; set; } = new BcfComment[0];

        [XmlElement(ElementName = "Viewpoints")]
        public BcfViewpoint[] Viewpoints { get; set; } = new BcfViewpoint[0];
    }

    public class BcfHeader
    {      
        [XmlElement(ElementName = "File")]
        public BcfHeaderFile[] Files { get; set; }
    }

    public class BcfHeaderFile : BcfFileAttributes
    {
        [XmlElement(ElementName = "Filename")]
        public string FileName { get; set; }
        [XmlElement(ElementName = "Date")]
        public DateTime Date { get; set; }
        [XmlElement(ElementName = "Reference")]
        public string Reference { get; set; }
    }

    public abstract class BcfFileAttributes
    {
        [XmlAttribute(AttributeName = "IfcProject")]
        public IfcGuidQualifier IfcProject { get; set; }
        [XmlAttribute(AttributeName = "IfcSpatialStructureElement")]
        public IfcGuidQualifier IfcSpatialStructureElement { get; set; }
        [XmlAttribute(AttributeName = "isExternal")]
        public bool IsExternal { get; set; }
    }
}
