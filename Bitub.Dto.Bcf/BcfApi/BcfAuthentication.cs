using System;
using System.Collections.Generic;
using System.Linq;

using System.Text.Json.Serialization;

namespace Bitub.Dto.Bcf.BcfApi
{
    [Flags]
    public enum BcfAuthFlows
    {
        AuthorizationCodeGrant = 0,
        ImplicitGrant = 1,
        ResourceOwnerPasswordCredentialsGrant = 2
    }

    public sealed class BcfAuthentication
    {
        [JsonPropertyName("oauth2_auth_url")]
        public string OAuth2Url { get; set; }
        [JsonPropertyName("oauth2_token_url")]
        public string OAuth2TokenUrl { get; set; }
        [JsonPropertyName("http_basic_supported")]
        public bool IsHttpBasicAuthSupported { get; set; }
        [JsonPropertyName("supported_oauth2_flows")]
        public BcfAuthFlows OAuth2SupportedFlows { get; set; }
    }
}
