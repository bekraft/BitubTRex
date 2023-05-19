using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi
{
    public enum ContentType
    {
        [XmlEnum("objectSection")]
        objectSection,
        [XmlEnum("extendedObjectSection")]
        extendedObjectSection,
        [XmlEnum("objectDataSection")]
        objectDataSection
    }
}
