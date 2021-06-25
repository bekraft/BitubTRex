using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bitub.Dto.Json
{
    public class JsonDelegateConverter<TSource, TDelegate> : JsonConverter<TSource>
    {
        private readonly Func<TSource, TDelegate> transform;
        private readonly Func<TDelegate, TSource> inverse;

        public JsonDelegateConverter(Func<TSource, TDelegate> transform, Func<TDelegate, TSource> inverse)
        {
            this.transform = transform;
        }

        public override TSource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, TSource value, JsonSerializerOptions options)
        {
            var output = transform(value);
            if (null == output)
            {
                writer.WriteNullValue();
            } 
            else
            {
                JsonSerializer.Serialize(writer, output, options);
            } 
        }
    }
}
