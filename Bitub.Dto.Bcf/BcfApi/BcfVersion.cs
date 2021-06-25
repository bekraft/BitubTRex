using System;
using System.Collections.Generic;

using System.Text.Json.Serialization;

namespace Bitub.Dto.BcfApi
{
    public sealed class BcfVersion
    {
        public readonly static BcfVersion Bcf2x1 = new BcfVersion
        { 
            Version = "2.1", 
            ApiReference = new Uri("https://github.com/BuildingSMART/BCF-API") 
        };

        [JsonPropertyName("version_id")]
        public string Version { get; set; }
        [JsonPropertyName("detailed_version")]
        public Uri ApiReference { get; set; }
    }

    public sealed class BcfInfo
    {
        [JsonPropertyName("versions")]
        public List<BcfVersion> Versions { get; set; } = new List<BcfVersion>();
    }
}
