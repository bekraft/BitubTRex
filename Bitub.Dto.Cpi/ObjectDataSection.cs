using System;
using System.Collections.Generic;
using System.Linq;

using System.Xml.Serialization;

using Bitub.Dto.Cpi.Geometry;

namespace Bitub.Dto.Cpi
{
    [XmlType("objectDataSection")]
    public class ObjectDataSection : Section
    {
        public ObjectDataSection() : base("1.4")
        { }

        [XmlElement("data3D")]
        public List<Data3D> Body { get; set; } = new List<Data3D>();
    }
}
