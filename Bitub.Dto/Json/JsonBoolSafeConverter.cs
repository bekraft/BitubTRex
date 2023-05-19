using System;

using System.Text.Json.Serialization;
using System.Text.Json;

namespace Bitub.Dto.Json
{
    public enum JsonBoolSerializationType
    {
        JsonConformant, AsString, AsFlag, AsStringFlag
    }

    public sealed class JsonBoolSafeReadWriteFlagConverter : JsonBoolSafeConverter
    {
        public JsonBoolSafeReadWriteFlagConverter() : base(JsonBoolSerializationType.AsFlag)
        { }
    }

    public abstract class JsonBoolSafeConverter : JsonConverter<bool>
    {
        public JsonBoolSerializationType WriteOutType { get; set; } = JsonBoolSerializationType.JsonConformant;

        protected JsonBoolSafeConverter(JsonBoolSerializationType serializeAsType)
        {
            WriteOutType = serializeAsType;
        }

        public override bool HandleNull { get => false; }

        protected bool ParseIntFlag(int flag)
        {
            switch (flag)
            {
                case 0:
                    return false;
                case 1:
                    return true;
                default:
                    throw new JsonException($"Misformated boolean value expression \"{flag}\".");
            }
        }

        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.String:
                    var value = reader.GetString();
                    bool result;
                    if (!bool.TryParse(value.ToLower(), out result))
                    {
                        int flag;
                        if (int.TryParse(reader.GetString(), out flag))
                        {
                            result = ParseIntFlag(flag);
                        }
                        else
                        {
                            throw new JsonException($"Expecting boolean. Found value \"{value}\" cannot be parsed to bool.");
                        }
                    }
                    return result;
                case JsonTokenType.Number:
                    return ParseIntFlag(reader.GetInt32());
                default:
                    throw new JsonException($"Unexpected token \"{reader.TokenType}\" for bool property.");
            }
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            switch (WriteOutType)
            {
                case JsonBoolSerializationType.JsonConformant:
                    writer.WriteBooleanValue(value);
                    break;
                case JsonBoolSerializationType.AsString:
                    writer.WriteStringValue(value.ToString().ToLower());
                    break;
                case JsonBoolSerializationType.AsFlag:
                    writer.WriteNumberValue(value ? 1 : 0);
                    break;
                case JsonBoolSerializationType.AsStringFlag:
                    writer.WriteStringValue($"{(value ? 1 : 0)}");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
