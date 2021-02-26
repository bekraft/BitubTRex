using Google.Protobuf;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Bitub.Dto.Json
{
    public class JsonProtoDelegateConverter<T> : JsonConverter<T> where T : IMessage, new()
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var ms = new MemoryStream())
            {
                do
                {
                    var readData = reader.ValueSequence.ToArray();
                    ms.Write(readData, 0, readData.Length);

                } while ((JsonTokenType.EndObject != reader.TokenType) && reader.Read());
                return JsonParser.Default.Parse<T>(Encoding.UTF8.GetString(ms.ToArray()));
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
