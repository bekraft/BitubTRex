using System;
using System.Text.Json.Serialization;

using Bitub.Dto.Json;

namespace Bitub.Dto.BcfApi
{
    public class BcfProject
    {
        [JsonPropertyName("project_id")]
        public string Id { get; set; } = System.Guid.NewGuid().ToString();
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("authorization"), JsonConverter(typeof(JsonDelegateConverter<BcfAuthorization, IBcfProjectAuthorization>))]
        public IBcfProjectAuthorization ProjectAuthorization { get; set; }
    }
}
