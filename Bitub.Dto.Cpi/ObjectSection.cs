using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

using Bitub.Dto.Cpi.Topology;

namespace Bitub.Dto.Cpi
{
    public class ObjectSection : Section
    {
        public ObjectSection() : base("1.4")
        { }

        [XmlAttribute("unit")]
        public LengthUnitType LengthUnit { get; set; } = LengthUnitType.Meter;
    }
}
