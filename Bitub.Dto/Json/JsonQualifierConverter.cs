using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Bitub.Dto.Json
{
    public class JsonQualifierConverter : JsonConverter<Qualifier>
    {
        private readonly JsonNamingPolicy namingPolicy;

        public JsonQualifierConverter() : this(JsonNamingPolicy.CamelCase)
        { }

        public JsonQualifierConverter(JsonNamingPolicy propertyNamingPolicy)
        {
            namingPolicy = propertyNamingPolicy;
        }

        private Name ReadNamed(ref Utf8JsonReader reader)
        {
            List<string> fragments = new List<string>();
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray:
                        break;
                    case JsonTokenType.String:
                        fragments.Add(reader.GetString());
                        break;
                    case JsonTokenType.EndArray:
                        return fragments.ToArray().ToName();
                    default:
                        throw new NotSupportedException($"Token type in named qualifer not supported: {reader.TokenType}");
                }
            } while (reader.Read());
            return new string[0].ToName();
        }

        private GlobalUniqueId ReadGlobalUniqueId(ref Utf8JsonReader reader)
        {
            var id = new GlobalUniqueId();
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.PropertyName:
                        var propertyName = reader.GetString();
                        if (propertyName == namingPolicy.ConvertName(nameof(GlobalUniqueId.Base64)))
                            id.Base64 = "";
                        else if (propertyName == namingPolicy.ConvertName(nameof(GlobalUniqueId.Guid)))
                            id.Guid = new Guid();
                        else
                            throw new JsonException($"Property name '{propertyName}' not supported");

                        break;
                    case JsonTokenType.String:
                        switch (id.NumericalOrStringCase)
                        {
                            case GlobalUniqueId.NumericalOrStringOneofCase.Base64:
                                id.Base64 = reader.GetString();
                                break;
                            case GlobalUniqueId.NumericalOrStringOneofCase.Guid:
                                id.Guid.Raw = reader.GetBytesFromBase64().ToByteString();
                                break;
                        }
                        break;
                    default:
                        return id;
                        
                }
            } while (reader.Read());
            return id;
        }

        public override Qualifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var q = new Qualifier();
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        return new Qualifier { Anonymous = ReadGlobalUniqueId(ref reader) };                        
                    case JsonTokenType.StartArray:
                        return new Qualifier { Named = ReadNamed(ref reader) };
                }
            } while (reader.Read());
            return q;
        }

        public override void Write(Utf8JsonWriter writer, Qualifier value, JsonSerializerOptions options)
        {
            switch (value.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Anonymous:
                    writer.WriteStartObject();                    
                    switch (value.Anonymous.NumericalOrStringCase)
                    {
                        case GlobalUniqueId.NumericalOrStringOneofCase.Base64:
                            writer.WritePropertyName(namingPolicy.ConvertName(nameof(GlobalUniqueId.Base64)));
                            writer.WriteStringValue(value.Anonymous.Base64);
                            break;
                        case GlobalUniqueId.NumericalOrStringOneofCase.Guid:
                            writer.WritePropertyName(namingPolicy.ConvertName(nameof(GlobalUniqueId.Guid)));
                            writer.WriteBase64StringValue(value.Anonymous.Guid.Raw.ToByteArray());
                            break;
                    }
                    writer.WriteEndObject();
                    
                    break;
                case Qualifier.GuidOrNameOneofCase.Named:
                    writer.WriteStartArray();
                    value.Named.Frags.ForEach(f => writer.WriteStringValue(f));
                    writer.WriteEndArray();

                    break;
            }
        }
    }
}
