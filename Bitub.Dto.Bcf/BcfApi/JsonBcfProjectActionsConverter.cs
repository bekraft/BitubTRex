using System;
using System.Collections.Generic;
using System.Text;

using Bitub.Dto.Json;

namespace Bitub.Dto.BcfApi
{
    public class JsonBcfProjectActionsConverter : JsonEnumFlagAsArrayConverter<BcfProjectActions>
    {
        public JsonBcfProjectActionsConverter() 
            : base(new JsonLowerFirstNamingPolicy(), BcfProjectActions.None | BcfProjectActions.Read)
        {
        }
    }
}
