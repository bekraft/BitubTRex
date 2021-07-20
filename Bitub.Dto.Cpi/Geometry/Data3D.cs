using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi.Geometry
{
    [XmlType("data3D")]
    public sealed class Data3D : ReferencesCpiObject
    {
        [XmlElement("p")]
        public List<Point> Point { get; set; } = new List<Point>();
        [XmlElement("solid")]
        public List<Solid> Solid { get; set; } = new List<Solid>();
    }
}
