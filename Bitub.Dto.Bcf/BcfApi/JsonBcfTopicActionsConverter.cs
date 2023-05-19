using System;
using System.Collections.Generic;
using System.Text;

using Bitub.Dto.Json;

namespace Bitub.Dto.BcfApi
{
    public class JsonBcfTopicActionsConverter : JsonEnumFlagAsArrayConverter<BcfTopicActions>
    {
        public JsonBcfTopicActionsConverter()
            : base(new JsonLowerFirstNamingPolicy(), BcfTopicActions.None | BcfTopicActions.Read)
        {
        }
    }
}
