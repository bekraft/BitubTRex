using System;
using System.Collections.Generic;
using System.Linq;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi.Topology
{
    public class Container : CpiObject
    {
        [XmlElement("container", Type = typeof(Container)), XmlElement("object3D", Type = typeof(Object3D))]
        public List<CpiObject> Child { get; set; } = new List<CpiObject>();
    }
}
