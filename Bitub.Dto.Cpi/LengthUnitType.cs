using System.Xml.Serialization;

namespace Bitub.Dto.Cpi
{
    public enum LengthUnitType
    {
        [XmlEnum("m")]
        Meter,
        [XmlEnum("mm")]
        Millimeter
    }
}
