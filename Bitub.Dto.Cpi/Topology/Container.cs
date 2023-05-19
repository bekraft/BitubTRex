using System;
using System.Collections.Generic;
using System.Linq;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi.Topology
{
    [XmlType("container")]
    public sealed class Container : CpiObject
    {
        [XmlAttribute("composite")]
        public string CompositeFlag { get; set; }

        [XmlIgnore]
        public bool IsComposite
        {
            get => CompositeFlag == "true";
            set => CompositeFlag = value.ToString().ToLower();
        }

        [XmlElement("container", Type = typeof(Container)), XmlElement("object3D", Type = typeof(Object3D))]
        public List<CpiObject> Child { get; set; } = new List<CpiObject>();
    }
}
