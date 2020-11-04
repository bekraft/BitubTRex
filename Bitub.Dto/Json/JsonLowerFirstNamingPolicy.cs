﻿using System.Text.Json;

namespace Bitub.Dto.Json
{
    public class JsonLowerFirstNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return $"{char.ToLower(name[0])}{name.Substring(1)}";
        }
    }
}
