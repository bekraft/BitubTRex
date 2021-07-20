using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi
{
    public sealed class IdMappingContainer
    {
        [XmlElement("ID")]
        public List<IdMapping> Mapping { get; set; } = new List<IdMapping>();

        public bool IsValid
        {
            get => (Mapping?.Select(m => m.CpiId).Distinct().Count() == Mapping?.Count) 
                && (Mapping?.Select(m => m.SourceId).Distinct().Count() == Mapping?.Count);
        }
    }

    public sealed class IdMapping
    {
        [XmlAttribute("k")]
        public int CpiId { get; set; }
        [XmlAttribute("v")]
        public string SourceId { get; set; }

        public override bool Equals(object obj)
        {
            return obj is IdMapping mapping &&
                   CpiId == mapping.CpiId &&
                   SourceId == mapping.SourceId;
        }

        public override int GetHashCode()
        {
            int hashCode = 1282183885;
            hashCode = hashCode * -1521134295 + CpiId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SourceId);
            return hashCode;
        }
    }
}
