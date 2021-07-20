using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitub.Dto.Cpi.Topology;
using Bitub.Dto.Cpi.Data;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi
{
    [XmlType("objectSection")]
    public class ObjectSection : Section
    {
        public ObjectSection() : base("1.4")
        { }

        [XmlAttribute("unit")]
        public LengthUnitType LengthUnit { get; set; } = LengthUnitType.Meter;

        [XmlElement("rootContainer")]
        public RootContainer RootContainer { get; set; }

        [XmlElement("materialSection")]
        public MaterialSection MaterialSection { get; set; }

        [XmlElement("propertySection")]
        public PropertySection PropertySection { get; set; }
    }
}
