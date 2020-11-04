using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bitub.Dto.Json
{
    public class JsonEnumFlagAsArrayConverter<T> : JsonConverter<T> where T : Enum
    {
        readonly JsonNamingPolicy namingPolicy;
        readonly Dictionary<string, string> translation;
        readonly bool saveAsNumber;
        readonly T defaultFlags;

        public JsonEnumFlagAsArrayConverter(JsonNamingPolicy namingPolicy, T defaultFlags, bool saveAsNumber = false)
        {            
            this.namingPolicy = namingPolicy;
            this.translation = Enum.GetNames(typeof(T))
                .ToDictionary(n => namingPolicy.ConvertName(n), StringComparer.InvariantCulture);
            this.saveAsNumber = saveAsNumber;
            this.defaultFlags = defaultFlags;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int flag = DecomposeFlags(defaultFlags).Cast<int>().Aggregate((a, b) => a|b);
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.Comment:
                    case JsonTokenType.StartArray:
                        break;
                    case JsonTokenType.String:
                        flag |= (int)Enum.Parse(typeof(T), translation[reader.GetString()]);
                        break;
                    case JsonTokenType.Number:
                        flag |= reader.GetInt32();
                        break;
                    default:
                        throw new NotSupportedException($"Not supported: {reader.TokenType}");
                }
            } while(reader.Read() && JsonTokenType.EndArray != reader.TokenType);
            return (T)Enum.ToObject(typeof(T), flag);
        }

        private IEnumerable<T> DecomposeFlags(T aggregation, T ignoreFlags = default(T))
        {
            foreach (var flag in Enum.GetValues(typeof(T)).Cast<T>())
            {
                if (aggregation.HasFlag(flag) && !ignoreFlags.HasFlag(flag))
                    yield return flag;
            }                
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            var decomposedFlags = DecomposeFlags(value, defaultFlags).ToArray();
            if (saveAsNumber)
            {
                foreach (var flag in decomposedFlags.Cast<int>())
                    writer.WriteNumberValue(flag);
            }
            else
            {
                foreach (var flag in decomposedFlags)
                    writer.WriteStringValue(namingPolicy.ConvertName(Enum.GetName(typeof(T), flag)));
            }
            writer.WriteEndArray();
        }
    }
}
