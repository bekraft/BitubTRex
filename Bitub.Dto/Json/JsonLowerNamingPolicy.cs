using System.Text.Json;

namespace Bitub.Dto.Json
{
    /// <summary>
    /// Simple lower naming policy.
    /// </summary>
    public sealed class JsonLowerNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name?.ToLowerInvariant();
        }
    }
}
