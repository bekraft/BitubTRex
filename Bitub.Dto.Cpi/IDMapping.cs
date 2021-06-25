using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi
{
    public class IdMappingContainer
    {
        [XmlElement("ID")]
        public List<IdMapping> Mapping { get; set; } = new List<IdMapping>();
    }

    public class IdMapping
    {
        [XmlAttribute("k")]
        public int CpiId { get; set; }
        [XmlAttribute("v")]
        public string SourceId { get; set; }
    }
}
