using System;
using System.Collections.Generic;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi.Topology
{
    public abstract class CpiObject
    {
        [XmlAttribute("ID")]
        public int CpiId { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
