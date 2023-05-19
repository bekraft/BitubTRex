using System.Text.Json;

namespace Bitub.Dto.Json
{
    public sealed class JsonLowerNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name?.ToLowerInvariant();
        }
    }
}
