using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bitub.Dto.Json
{
    public class JsonDelegateConverter<TConcrete, TInterface> : JsonConverter<TConcrete> where TConcrete : TInterface
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(TInterface) == typeToConvert;
        }

        public override TConcrete Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<TConcrete>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, TConcrete value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<TInterface>(writer, value, options);
        }
    }
}
