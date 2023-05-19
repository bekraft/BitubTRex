using System;
using System.Collections.Generic;
using System.Text;

using Bitub.Dto.Json;

namespace Bitub.Dto.BcfApi
{
    public class JsonBcfCommentActionsConverter: JsonEnumFlagAsArrayConverter<BcfCommentActions>
    {
        public JsonBcfCommentActionsConverter()
            : base(new JsonLowerFirstNamingPolicy(), BcfCommentActions.None | BcfCommentActions.Read)
        {
        }
    }
}
