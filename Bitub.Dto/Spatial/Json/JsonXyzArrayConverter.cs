using System;

using System.Text.Json.Serialization;

using System.Text.Json;

namespace Bitub.Dto.Spatial.Json
{
    public class JsonXyzArrayConverter : JsonConverter<XYZ>
    {
        protected XYZ Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            int index = 0;
            XYZ xyz = new XYZ();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndArray:
                        return xyz;
                    case JsonTokenType.StartArray:
                        break;
                    case JsonTokenType.Number:
                        xyz.SetCoordinate(index++, (float)reader.GetDouble());
                        break;
                    default:
                        throw new NotSupportedException($"Token '{reader.TokenType}' not supported by ReadXYZ");
                }
            }
            throw new NotSupportedException("Unexpected reader state. Reached end before end of array");
        }

        public override XYZ Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeof(XYZ) == typeToConvert)
                return Read(ref reader, options);
            throw new NotImplementedException($"Type not implemented: {typeToConvert.FullName}");
        }

        public override void Write(Utf8JsonWriter writer, XYZ value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteNumberValue(value.Z);
            writer.WriteEndArray();
        }
    }
}
