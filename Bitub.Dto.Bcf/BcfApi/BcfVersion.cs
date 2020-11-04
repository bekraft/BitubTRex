using System;
using System.Text.Json.Serialization;

namespace Bitub.Dto.BcfApi
{
    public class BcfVersion
    {
        public const string V1x0 = "1.0";
        public const string V2x1 = "2.1";

        public static readonly BcfVersion[] Versions = new[]
        {
            new BcfVersion{ Version = V1x0,  ApiReference = new Uri("https://github.com/BuildingSMART/BCF-API") },
            new BcfVersion{ Version = V2x1,  ApiReference = new Uri("https://github.com/BuildingSMART/BCF-API") },
        };

        [JsonPropertyName("version_id")]
        public string Version { get; set; }
        [JsonPropertyName("detailed_version")]
        public Uri ApiReference { get; set; }
    }
}
