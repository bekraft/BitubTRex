using System;
using System.Collections.Generic;
using System.Linq;

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

        #if (NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER)

        public override int GetHashCode()
        {
            return HashCode.Combine(CpiId, SourceId);
        }

        #else

        public override int GetHashCode()
        {
            return new { CpiId, SourceId }.GetHashCode();
        }

        #endif
    }
}
