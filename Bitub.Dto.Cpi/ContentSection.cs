using System;
using System.Linq;
using System.Collections.Generic;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi
{
    [XmlType("content")]
    public sealed class ContentSection : Section
    {
        public ContentSection() : base("1.2")
        { }

        [XmlElement("section")]
        public List<ContentReference> Section { get; set; } = new List<ContentReference>();

        public bool IsValid
        {
            get => Section?.Select(s => s.SectionType).Distinct().Count() == Section?.Count;
        }
    }

    public sealed class ContentReference
    {
        [XmlAttribute("name")]
        public ContentType SectionType { get; set; } = ContentType.objectSection;
    }
}
