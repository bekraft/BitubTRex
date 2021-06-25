using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bitub.Dto.Json
{
    public class JsonProxyConverter<TConcrete, TProxy> : JsonConverter<TConcrete> where TConcrete : TProxy
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(TProxy) == typeToConvert;
        }

        public override TConcrete Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<TConcrete>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, TConcrete value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<TProxy>(writer, value, options);
        }
    }
}
