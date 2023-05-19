using System.Text.Json;

namespace Bitub.Dto.BcfApi
{
    public static class BcfApiExtensions
    {
        public static JsonSerializerOptions AddBcfApiConverters(this JsonSerializerOptions options)
        {
            options.Converters.Add(new JsonBcfProjectActionsConverter());
            options.Converters.Add(new JsonBcfTopicActionsConverter());
            options.Converters.Add(new JsonBcfCommentActionsConverter());
            return options;
        }
    }
}
